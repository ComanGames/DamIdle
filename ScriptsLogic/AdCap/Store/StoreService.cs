using System;
using System.Collections.Generic;
using System.Linq;
using Platforms;
using Platforms.Logger;
using UniRx;
using UnityEngine;
using Utils;

namespace AdCap.Store
{
	// Token: 0x020006EF RID: 1775
	public class StoreService : IStoreService, IDisposable
	{
		// Token: 0x17000308 RID: 776
		// (get) Token: 0x060024BF RID: 9407 RVA: 0x0009EA44 File Offset: 0x0009CC44
		// (set) Token: 0x060024C0 RID: 9408 RVA: 0x0009EA4C File Offset: 0x0009CC4C
		public ReactiveCollection<AdCapStoreItem> CurrentCatalog { get; private set; }

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x060024C1 RID: 9409 RVA: 0x0009EA55 File Offset: 0x0009CC55
		// (set) Token: 0x060024C2 RID: 9410 RVA: 0x0009EA5D File Offset: 0x0009CC5D
		public ReactiveCollection<ItemPurchaseData> PurchaseData { get; private set; }

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x060024C3 RID: 9411 RVA: 0x0009EA66 File Offset: 0x0009CC66
		// (set) Token: 0x060024C4 RID: 9412 RVA: 0x0009EA6E File Offset: 0x0009CC6E
		public ReactiveProperty<int> TotalRMPurchaseCount { get; private set; }

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x060024C5 RID: 9413 RVA: 0x0009EA77 File Offset: 0x0009CC77
		// (set) Token: 0x060024C6 RID: 9414 RVA: 0x0009EA7F File Offset: 0x0009CC7F
		public ReactiveCollection<AdCapStoreItem> ActiveLimitedTimeItems { get; private set; }

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x060024C7 RID: 9415 RVA: 0x0009EA88 File Offset: 0x0009CC88
		// (set) Token: 0x060024C8 RID: 9416 RVA: 0x0009EA90 File Offset: 0x0009CC90
		public ReactiveCollection<AdCapStoreItem> NewTimedItems { get; private set; }

		// Token: 0x060024C9 RID: 9417 RVA: 0x0009EA9C File Offset: 0x0009CC9C
		public StoreService()
		{
			this.CurrentCatalog = new ReactiveCollection<AdCapStoreItem>();
			this.PurchaseData = new ReactiveCollection<ItemPurchaseData>();
			this.TotalRMPurchaseCount = new ReactiveProperty<int>();
			this.ActiveLimitedTimeItems = new ReactiveCollection<AdCapStoreItem>();
			this.NewTimedItems = new ReactiveCollection<AdCapStoreItem>();
			this.logger = Platforms.Logger.Logger.GetLogger(this);
		}

		// Token: 0x060024CA RID: 9418 RVA: 0x0009EB13 File Offset: 0x0009CD13
		public void Dispose()
		{
			this.disposables.Dispose();
		}

		// Token: 0x060024CB RID: 9419 RVA: 0x0009EB20 File Offset: 0x0009CD20
		public void Init(IGameController gameController, IUserDataService userDataService, IGrantRewardService grantRewardService, IDateTimeService dateTimeService, UpgradeService upgradeService, IAngelInvestorService angelInvestorService, TimerService timerService, ITriggerService triggerService, PlayerData globalPlayerData, IInventoryService inventoryService, IPlatformStore platformStore)
		{
			this.logger.Trace("Initializing....", Array.Empty<object>());
			this.gameController = gameController;
			this.UserDataService = userDataService;
			this.grantRewardService = grantRewardService;
			this.dateTimeService = dateTimeService;
			this.timerService = timerService;
			this.triggerService = triggerService;
			this.playerData = globalPlayerData;
			this.inventoryService = inventoryService;
			this.platformStore = platformStore;
			this.angelInvestorService = angelInvestorService;
			this.upgradeService = upgradeService;
			this.WireStoreData();
			(from x in this.gameController.State
			where x != null
			select x).Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
			this.ProcessCatalog(new List<CatalogStoreItem>());
			this.platformStore.Ready.Subscribe(new Action<List<CatalogStoreItem>>(this.ProcessCatalog)).AddTo(this.disposables);
			(from x in this.timerService.GetTimer(TimerService.TimerGroups.Global)
			select x.TotalSeconds).Subscribe(new Action<double>(this.Update)).AddTo(this.disposables);
			this.logger.Trace("Initialized", Array.Empty<object>());
		}

		// Token: 0x060024CC RID: 9420 RVA: 0x0009EC80 File Offset: 0x0009CE80
		public void LoadPurchaseData()
		{
			string text = this.playerData.Get(StoreService.ITEMS_PURCHASE_DATA_KEY, "");
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					JsonUtility.FromJson<ItemPurchaseSaveDataHolder>(text).List.ForEach(delegate(ItemPurchaseSaveData x)
					{
						this.PurchaseData.Add(new ItemPurchaseData(x));
					});
				}
				catch
				{
					Debug.LogError("[Store Service] Failed to deserialize PurchaseData");
				}
			}
		}

		// Token: 0x060024CD RID: 9421 RVA: 0x0009ECE8 File Offset: 0x0009CEE8
		public void SavePurchaseData()
		{
			ItemPurchaseSaveDataHolder itemPurchaseSaveDataHolder = default(ItemPurchaseSaveDataHolder);
			itemPurchaseSaveDataHolder.List = (from x in this.PurchaseData
			select x.ToSaveData()).ToList<ItemPurchaseSaveData>();
			string value = JsonUtility.ToJson(itemPurchaseSaveDataHolder);
			this.playerData.Set(StoreService.ITEMS_PURCHASE_DATA_KEY, value);
		}

		// Token: 0x060024CE RID: 9422 RVA: 0x0009ED54 File Offset: 0x0009CF54
		public bool AttemptPurchase(string id, Currency currency)
		{
			AdCapStoreItem item = this.StoreItemMap[id];
			return this.AttemptPurchase(item, currency);
		}

		// Token: 0x060024CF RID: 9423 RVA: 0x0009ED78 File Offset: 0x0009CF78
		public bool AttemptPurchase(AdCapStoreItem item, Currency currency)
		{
			MessageBroker.Default.Publish<StorePurchaseEvent>(new StorePurchaseEvent
			{
				Item = item,
				PurchaseCurrency = Currency.Cash,
				PurchaseState = EStorePurchaseState.Started
			});
			Item itemById = this.inventoryService.GetItemById(item.Id);
			if (itemById != null)
			{
				Product product = itemById.Product;
				if (product != Product.TimeWarp)
				{
					if (product == Product.AngelClaim)
					{
						double num = this.angelInvestorService.CalculateAngelInvestors(this.gameController.game.TotalCashEarned);
						double num2 = this.angelInvestorService.CalculateAngelInvestors(this.gameController.game.TotalPreviousCash.Value);
						if (num - num2 <= 0.0)
						{
							this.OnPurchaseFail(item, currency, "You do not have any Angel Investors waiting to join your team!");
							return false;
						}
					}
				}
				else if (this.upgradeService.Managers.Count(m => m.IsPurchased.Value) == 0)
				{
					this.OnPurchaseFail(item, currency, "You do not have any managed investments and will not earn anything from this purchase.");
					return false;
				}
			}
			return this.Purchase(item, currency);
		}

		// Token: 0x060024D0 RID: 9424 RVA: 0x0009EE7C File Offset: 0x0009D07C
		private bool Purchase(AdCapStoreItem storeItem, Currency currency)
		{
			double currentCostAsDouble = storeItem.Cost.GetCurrentCostAsDouble();
			string text = "NA";
			switch (currency)
			{
			case Currency.Gold:
				text = "Gold";
				break;
			case Currency.MegaBuck:
				text = "MegaBucksBalance";
				break;
			case Currency.Cash:
				this.platformStore.Purchase(storeItem.PlatformId, "RM", new Action<PurchaseEvent>(this.OnPurchaseSuccess), new Action<PurchaseEvent>(this.OnPurchaseFail));
				return true;
			case Currency.Inventory:
				if (this.inventoryService.ConsumeItem(storeItem.Id, 1))
				{
					this.OnPurchaseSuccess(storeItem, currency);
					return true;
				}
				MessageBroker.Default.Publish<InsufficientFundsEvent>(new InsufficientFundsEvent(currency, 1));
				this.OnPurchaseFail(storeItem, currency, "Insufficient Funds");
				return false;
			case Currency.InGameCash:
			{
				if (this.state.CashOnHand.Value >= currentCostAsDouble)
				{
					this.state.CashOnHand.Value -= currentCostAsDouble;
					this.OnPurchaseSuccess(storeItem, currency);
					return true;
				}
				double num = currentCostAsDouble - this.state.CashOnHand.Value;
				MessageBroker.Default.Publish<InsufficientFundsEvent>(new InsufficientFundsEvent(currency, (int)num));
				this.OnPurchaseFail(storeItem, currency, "Insufficient Funds");
				return false;
			}
			case Currency.MegaTicket:
				text = "MegaTickets";
				break;
			}
			if (text == "NA")
			{
				this.OnPurchaseFail(storeItem, currency, string.Format("Invalid Currency name for Currency type [{0}]", currency.ToString()));
				return false;
			}
			if (PlayerData.GetPlayerData("Global").Spend(text, currentCostAsDouble))
			{
				this.OnPurchaseSuccess(storeItem, currency);
				return true;
			}
			double num2 = storeItem.Cost.Price - (double)PlayerData.GetPlayerData("Global").GetInt(text, 0);
			MessageBroker.Default.Publish<InsufficientFundsEvent>(new InsufficientFundsEvent(currency, (int)num2));
			this.OnPurchaseFail(storeItem, currency, "Insufficient Funds");
			return false;
		}

		// Token: 0x060024D1 RID: 9425 RVA: 0x0009F048 File Offset: 0x0009D248
		private void OnPurchaseFail(PurchaseEvent evt)
		{
			KeyValuePair<string, AdCapStoreItem> keyValuePair = this.StoreItemMap.FirstOrDefault(x => x.Value.PlatformId == evt.Item.ItemId);
			if (keyValuePair.Value != null)
			{
				AdCapStoreItem value = keyValuePair.Value;
				this.OnPurchaseFail(value, this.ConvertCurrency(evt.CurrencyCode), evt.Error.ErrorMessage);
			}
		}

		// Token: 0x060024D2 RID: 9426 RVA: 0x0009F0B4 File Offset: 0x0009D2B4
		private void OnPurchaseFail(AdCapStoreItem storeItem, Currency currency, string error)
		{
			this.logger.Debug("Purchase Failed for item [{0}] with error:{1}", new object[]
			{
				(storeItem == null) ? "Unkown" : storeItem.Id,
				error
			});
			MessageBroker.Default.Publish<StorePurchaseEvent>(new StorePurchaseEvent
			{
				Item = storeItem,
				PurchaseCurrency = currency,
				PurchaseState = EStorePurchaseState.Fail,
				Error = error
			});
		}

		// Token: 0x060024D3 RID: 9427 RVA: 0x0009F124 File Offset: 0x0009D324
		private void OnPurchaseSuccess(PurchaseEvent evt)
		{
			KeyValuePair<string, AdCapStoreItem> keyValuePair = this.StoreItemMap.FirstOrDefault(x => x.Value.PlatformId == evt.Item.ItemId);
			if (keyValuePair.Value != null)
			{
				AdCapStoreItem value = keyValuePair.Value;
				Currency currency = this.ConvertCurrency(evt.CurrencyCode);
				this.OnPurchaseSuccess(value, currency);
			}
		}

		// Token: 0x060024D4 RID: 9428 RVA: 0x0009F184 File Offset: 0x0009D384
		private void OnPurchaseSuccess(AdCapStoreItem storeItem, Currency currency)
		{
			this.logger.Debug("OnPurchaseSuccess ItemId:{0}", new object[]
			{
				storeItem.Id
			});
			MessageBroker.Default.Publish<StorePurchaseEvent>(new StorePurchaseEvent
			{
				Item = storeItem,
				PurchaseCurrency = currency,
				PurchaseState = EStorePurchaseState.Success
			});
			if (storeItem.Rewards.Count > 0)
			{
				this.grantRewardService.GrantRewards(storeItem.Rewards, "bundle_" + storeItem.Id, this.gameController.game.planetName, true);
			}
			this.UpdatePurchaseData(storeItem);
		}

		// Token: 0x060024D5 RID: 9429 RVA: 0x0009F224 File Offset: 0x0009D424
		private void UpdatePurchaseData(AdCapStoreItem storeItem)
		{
			ItemPurchaseData itemPurchaseData = this.PurchaseData.FirstOrDefault(x => x.ItemId == storeItem.Id);
			if (itemPurchaseData != null)
			{
				ReactiveProperty<int> currentPurchaseCount = itemPurchaseData.CurrentPurchaseCount;
				int value = currentPurchaseCount.Value + 1;
				currentPurchaseCount.Value = value;
				itemPurchaseData.TotalPurchaseCount++;
				if (storeItem.Cost.Currency == Currency.Cash)
				{
					ReactiveProperty<int> totalRMPurchaseCount = this.TotalRMPurchaseCount;
					value = totalRMPurchaseCount.Value + 1;
					totalRMPurchaseCount.Value = value;
				}
				this.SavePurchaseData();
			}
			else
			{
				Debug.LogErrorFormat("Failed to find purchase data on product {0}", new object[]
				{
					storeItem.Id
				});
			}
			AdCapStoreItem adCapStoreItem = this.StoreItemMap[storeItem.Id];
			if (adCapStoreItem != null && adCapStoreItem.PurchaseCount > 0 && itemPurchaseData != null && (itemPurchaseData.CurrentPurchaseCount.Value >= adCapStoreItem.PurchaseCount || itemPurchaseData.IsLastChance))
			{
				this.NewTimedItems.Remove(adCapStoreItem);
				this.CurrentCatalog.Remove(adCapStoreItem);
				this.ActiveLimitedTimeItems.Remove(adCapStoreItem);
			}
		}

		// Token: 0x060024D6 RID: 9430 RVA: 0x0009F334 File Offset: 0x0009D534
		private Currency ConvertCurrency(string currency)
		{
			if (currency == "RM")
			{
				return Currency.Cash;
			}
			if (currency == "GB")
			{
				return Currency.Gold;
			}
			if (currency == "KR")
			{
				return Currency.Kreds;
			}
			if (!(currency == "MB"))
			{
				return Currency.NA;
			}
			return Currency.MegaBuck;
		}

		// Token: 0x060024D7 RID: 9431 RVA: 0x0009F380 File Offset: 0x0009D580
		private void WireStoreData()
		{
			this.logger.Trace("WireStoreData....", Array.Empty<object>());
			List<AdCapStoreItem> source = this.playerData.StoreItemsLive;
			source = (from x in source
			where x.Active
			select x).ToList<AdCapStoreItem>();
			source = (from x in source
			where string.IsNullOrEmpty(x.ABTestGroup) || this.UserDataService.IsTestGroupMember(x.ABTestGroup)
			select x).ToList<AdCapStoreItem>();
			source = (from x in source
			where x.Platforms.Contains(this.gameController.PlatformId)
			select x).ToList<AdCapStoreItem>();
			List<string> rmItems = (from x in this.playerData.StoreItemsLive
			where x.Cost.Currency == Currency.Cash
			select x.Id).ToList<string>();
			this.TotalRMPurchaseCount.Value = (from x in this.PurchaseData
			where rmItems.Contains(x.ItemId)
			select x).Sum(x => x.TotalPurchaseCount);
			this.StoreItemMap = source.ToDictionary(item => item.Id);
			this.LoadPurchaseData();
			this.CreatePurchaseDataForNewItems();
			this.ConvertLegacyPurchaseIdsToPurchaseData();
			this.ConvertOldIdsToNewIds();
			this.LoadActiveItems();
			this.CreatePurchaseDataForActiveItems();
			this.RefreshExpiredItemsPurchaseCount();
			foreach (KeyValuePair<string, AdCapStoreItem> keyValuePair in this.StoreItemMap)
			{
				this.WireStoreItem(keyValuePair.Value);
			}
			this.SavePurchaseData();
			this.logger.Trace("WireStoreData Complete", Array.Empty<object>());
		}

		// Token: 0x060024D8 RID: 9432 RVA: 0x0009F578 File Offset: 0x0009D778
		private void ProcessCatalog(List<CatalogStoreItem> catalog)
		{
			using (Dictionary<string, AdCapStoreItem>.Enumerator enumerator = this.StoreItemMap.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, AdCapStoreItem> storeItem = enumerator.Current;
					CatalogStoreItem catalogStoreItem = catalog.FirstOrDefault(x => x.ItemId == storeItem.Value.PlatformId);
					if (catalogStoreItem != null && catalogStoreItem.Metadata != null)
					{
						storeItem.Value.Cost.Price = (double)catalogStoreItem.Metadata.localizedPrice;
						storeItem.Value.Cost.LocalizedPriceString = catalogStoreItem.Metadata.localizedPriceString;
					}
					else if (storeItem.Value.Cost.Currency == Currency.Cash && (catalogStoreItem == null || catalogStoreItem.Metadata == null))
					{
						storeItem.Value.Cost.LocalizedPriceString = "Loading";
					}
				}
			}
		}

		// Token: 0x060024D9 RID: 9433 RVA: 0x0009F690 File Offset: 0x0009D890
		private void OnItemExpired(bool isExpired, AdCapStoreItem item)
		{
			if (isExpired)
			{
				this.CurrentCatalog.Remove(item);
				this.OnItemExpired(item);
			}
		}

		// Token: 0x060024DA RID: 9434 RVA: 0x0009F6AC File Offset: 0x0009D8AC
		private void WireStoreItem(AdCapStoreItem item)
		{
			if (this.activeItemMap.ContainsKey(item.Id))
			{
				this.WireExistingStoreItem(item);
				return;
			}
			ItemPurchaseData itemPurchaseData = this.PurchaseData.FirstOrDefault(x => x.ItemId == item.Id);
			if (itemPurchaseData == null || item.PurchaseCount == 0 || item.PurchaseCount > itemPurchaseData.CurrentPurchaseCount.Value)
			{
				this.MonitorNewStoreItem(item);
			}
		}

		// Token: 0x060024DB RID: 9435 RVA: 0x0009F738 File Offset: 0x0009D938
		private void WireExistingStoreItem(AdCapStoreItem storeItem)
		{
			ItemPurchaseData itemPurchaseData = this.PurchaseData.FirstOrDefault(x => x.ItemId == storeItem.Id);
			if (itemPurchaseData != null && !itemPurchaseData.IsLastChance && (storeItem.PurchaseCount == 0 || storeItem.PurchaseCount > itemPurchaseData.CurrentPurchaseCount.Value))
			{
				DateTime utcNow = this.dateTimeService.UtcNow;
				long ticks = this.activeItemMap[storeItem.Id];
				storeItem.ExpiryDate = new DateTime(ticks);
				if (storeItem.ExpiryDate > utcNow)
				{
					storeItem.TimeRemaining.Value = storeItem.ExpiryDate.Subtract(utcNow).TotalSeconds;
					this.ActiveLimitedTimeItems.Add(storeItem);
					storeItem.TimeRemaining.First(x => x <= 0.0).Subscribe(delegate(double _)
					{
						this.OnItemExpired(storeItem);
					}).AddTo(this.disposables);
				}
				else if (storeItem.ExpiryDate != DateTime.MinValue)
				{
					itemPurchaseData.IsLastChance = true;
				}
				this.triggerService.MonitorTriggers(storeItem.ExpiryTriggerDatas, false).Subscribe(delegate(bool expired)
				{
					this.OnItemExpired(expired, storeItem);
				});
				this.CurrentCatalog.Add(storeItem);
				this.SavePurchaseData();
			}
		}

		// Token: 0x060024DC RID: 9436 RVA: 0x0009F8DC File Offset: 0x0009DADC
		private void MonitorNewStoreItem(AdCapStoreItem storeItem)
		{
			DateTime utcNow = this.dateTimeService.UtcNow;
			if (storeItem.EndDateUtc != DateTime.MinValue && storeItem.EndDateUtc < utcNow)
			{
				return;
			}
			if (storeItem.StartDateUtc != DateTime.MinValue && storeItem.StartDateUtc > utcNow)
			{
				if ((from x in storeItem.TriggerDatas
				select x.TriggerGroup).Distinct<int>().Count<int>() > 0)
				{
					int num = 0;
					for (;;)
					{
						if (num >= (from x in storeItem.TriggerDatas
						select x.TriggerGroup).Distinct<int>().Count<int>())
						{
							break;
						}
						int triggerGroup = num;
						storeItem.TriggerDatas.Add(new TriggerData
						{
							Value = storeItem.StartDateUtc.ToString(),
							Operator = ">=",
							TriggerGroup = triggerGroup,
							TriggerType = ETriggerType.TimeUtcNow
						});
						num++;
					}
				}
				else
				{
					storeItem.TriggerDatas.Add(new TriggerData
					{
						Value = storeItem.StartDateUtc.ToString(),
						Operator = ">=",
						TriggerType = ETriggerType.TimeUtcNow
					});
				}
			}
			(from x in this.triggerService.MonitorTriggers(storeItem.TriggerDatas, true)
			where x
			select x).Take(1).Subscribe(delegate(bool available)
			{
				this.OnNewItemAvailable(storeItem);
			});
		}

		// Token: 0x060024DD RID: 9437 RVA: 0x0009FAC4 File Offset: 0x0009DCC4
		private void OnNewItemAvailable(AdCapStoreItem storeItem)
		{
			DateTime utcNow = this.dateTimeService.UtcNow;
			DateTime dateTime = DateTime.MinValue;
			if (storeItem.Duration > 0.0)
			{
				dateTime = utcNow.AddSeconds(storeItem.Duration);
			}
			if (storeItem.StartDateUtc != DateTime.MinValue && storeItem.Duration > 0.0)
			{
				dateTime = storeItem.StartDateUtc.AddSeconds(storeItem.Duration);
			}
			if (storeItem.EndDateUtc != DateTime.MinValue && storeItem.EndDateUtc > utcNow)
			{
				if (storeItem.Duration <= 0.0)
				{
					dateTime = storeItem.EndDateUtc;
					storeItem.Duration = dateTime.Subtract(utcNow).TotalSeconds;
				}
				if (dateTime > storeItem.EndDateUtc)
				{
					dateTime = storeItem.EndDateUtc;
				}
			}
			storeItem.ExpiryDate = dateTime;
			if (storeItem.ExpiryDate > utcNow)
			{
				storeItem.TimeRemaining.Value = dateTime.Subtract(utcNow).TotalSeconds;
				storeItem.ExpiryDate = dateTime;
				storeItem.TimeRemaining.First(x => x <= 0.0).Subscribe(delegate(double _)
				{
					this.OnItemExpired(storeItem);
				}).AddTo(this.disposables);
				this.NewTimedItems.Add(storeItem);
				this.ActiveLimitedTimeItems.Add(storeItem);
			}
			this.PurchaseData.FirstOrDefault(x => x.ItemId == storeItem.Id).ExpiryDate = storeItem.ExpiryDate;
			this.activeItemMap.Add(storeItem.Id, storeItem.ExpiryDate.Ticks);
			this.CurrentCatalog.Add(storeItem);
			this.triggerService.MonitorTriggers(storeItem.ExpiryTriggerDatas, false).Subscribe(delegate(bool expired)
			{
				this.OnItemExpired(expired, storeItem);
			});
			this.SaveActiveItems();
			this.SavePurchaseData();
		}

		// Token: 0x060024DE RID: 9438 RVA: 0x0009FD40 File Offset: 0x0009DF40
		private void OnItemExpired(AdCapStoreItem item)
		{
			ItemPurchaseData itemPurchaseData = this.PurchaseData.FirstOrDefault(x => x.ItemId == item.Id);
			if (itemPurchaseData != null)
			{
				itemPurchaseData.HasExpired.Value = true;
				if (item.Duration > 0.0 || item.EndDateUtc != DateTime.MinValue)
				{
					if (this.NewTimedItems.Contains(item))
					{
						this.NewTimedItems.Remove(item);
					}
					if (this.ActiveLimitedTimeItems.Contains(item))
					{
						this.ActiveLimitedTimeItems.Remove(item);
					}
					itemPurchaseData.IsLastChance = true;
				}
				this.activeItemMap.Remove(item.Id);
				this.SavePurchaseData();
				this.SaveActiveItems();
			}
		}

		// Token: 0x060024DF RID: 9439 RVA: 0x0009FE28 File Offset: 0x0009E028
		private void OnStateChanged(GameState state)
		{
			this.state = state;
			foreach (AdCapStoreItem adCapStoreItem in this.ActiveLimitedTimeItems)
			{
				if (this.activeItemMap.ContainsKey(adCapStoreItem.Id))
				{
					long ticks = this.activeItemMap[adCapStoreItem.Id];
					DateTime dateTime = new DateTime(ticks);
					adCapStoreItem.TimeRemaining.Value = dateTime.Subtract(this.dateTimeService.UtcNow).TotalSeconds;
				}
			}
		}

		// Token: 0x060024E0 RID: 9440 RVA: 0x0009FEC8 File Offset: 0x0009E0C8
		private void Update(double deltaTime)
		{
			for (int i = this.ActiveLimitedTimeItems.Count - 1; i >= 0; i--)
			{
				this.ActiveLimitedTimeItems[i].TimeRemaining.Value -= deltaTime;
			}
		}

		// Token: 0x060024E1 RID: 9441 RVA: 0x0009FF0C File Offset: 0x0009E10C
		private void CreatePurchaseDataForNewItems()
		{
			List<string> purchaseDataIds = (from x in this.PurchaseData
			select x.ItemId).ToList<string>();
			IEnumerable<KeyValuePair<string, AdCapStoreItem>> storeItemMap = this.StoreItemMap;
			Func<KeyValuePair<string, AdCapStoreItem>, bool> predicate = (x) => !purchaseDataIds.Contains(x.Value.Id);
			foreach (KeyValuePair<string, AdCapStoreItem> keyValuePair in storeItemMap.Where(predicate))
			{
				ItemPurchaseData item = new ItemPurchaseData(keyValuePair.Value.Id);
				this.PurchaseData.Add(item);
			}
		}

		// Token: 0x060024E2 RID: 9442 RVA: 0x0009FFD4 File Offset: 0x0009E1D4
		private void LoadActiveItems()
		{
			string text = this.playerData.Get(StoreService.ITEMS_ACTIVE_KEY, "");
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(new char[]
				{
					','
				});
				int i = 0;
				while (i < array.Length)
				{
					string[] array2 = array[i].Split(new char[]
					{
						':'
					});
					string text2 = array2[0];
					long value;
					if (long.TryParse(array2[1], out value))
					{
						try
						{
							this.activeItemMap.Add(text2, value);
							goto IL_93;
						}
						catch (ArgumentException)
						{
							Debug.LogError("[StoreService](LoadActiveItems) " + string.Format("An element with the same key [{0}] already exists in the dictionary", text2));
							goto IL_93;
						}
						goto IL_83;
					}
					goto IL_83;
					IL_93:
					i++;
					continue;
					IL_83:
					Debug.LogError("[StoreService](LoadActiveItems) Error restoring active item " + text2);
					goto IL_93;
				}
			}
		}

		// Token: 0x060024E3 RID: 9443 RVA: 0x000A0090 File Offset: 0x0009E290
		private void RefreshExpiredItemsPurchaseCount()
		{
			DateTime now = this.dateTimeService.UtcNow;
			IEnumerable<ItemPurchaseData> purchaseData = this.PurchaseData;
            Func<ItemPurchaseData, bool> predicate = (x) =>
            {
              return this.StoreItemMap.ContainsKey(x.ItemId) && x.ExpiryDate != DateTime.MinValue && x.ExpiryDate <= now;
            };
			foreach (ItemPurchaseData itemPurchaseData in purchaseData.Where(predicate))
			{
				if (this.StoreItemMap[itemPurchaseData.ItemId].EndDateUtc != DateTime.MinValue)
				{
					itemPurchaseData.CurrentPurchaseCount.Value = 0;
					itemPurchaseData.HasExpired.Value = false;
					itemPurchaseData.IsLastChance = false;
					this.activeItemMap.Remove(itemPurchaseData.ItemId);
				}
			}
		}

		// Token: 0x060024E4 RID: 9444 RVA: 0x000A016C File Offset: 0x0009E36C
		private void CreatePurchaseDataForActiveItems()
		{
			using (Dictionary<string, long>.Enumerator enumerator = this.activeItemMap.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, long> kvp = enumerator.Current;
					ItemPurchaseData itemPurchaseData = this.PurchaseData.FirstOrDefault(x => x.ItemId == kvp.Key);
					if (itemPurchaseData == null)
					{
						itemPurchaseData = new ItemPurchaseData(kvp.Key);
						this.PurchaseData.Add(itemPurchaseData);
					}
					itemPurchaseData.ExpiryDate = new DateTime(kvp.Value);
				}
			}
		}

		// Token: 0x060024E5 RID: 9445 RVA: 0x000A0214 File Offset: 0x0009E414
		private void SaveActiveItems()
		{
			string value = string.Join(",", (from x in this.activeItemMap
			select string.Format("{0}:{1}", x.Key, x.Value)).ToArray<string>());
			this.playerData.Set(StoreService.ITEMS_ACTIVE_KEY, value);
		}

		// Token: 0x060024E6 RID: 9446 RVA: 0x000A0270 File Offset: 0x0009E470
		private void ConvertLegacyPurchaseIdsToPurchaseData()
		{
			string text = this.playerData.Get(StoreService.ITEMS_PURCHASED_KEY, "");
			if (!string.IsNullOrEmpty(text))
			{
				StoreService.DataContainer localData = new StoreService.DataContainer();
				localData.seperatedIds = text.Split(new char[]
				{
					','
				});
				int j;
				int i;
				for (i = 0; i < localData.seperatedIds.Length; i = j)
				{
					ItemPurchaseData itemPurchaseData = this.PurchaseData.FirstOrDefault(x => x.ItemId == localData.seperatedIds[i]);
					if (itemPurchaseData == null)
					{
						itemPurchaseData = new ItemPurchaseData(localData.seperatedIds[i]);
						this.PurchaseData.Add(itemPurchaseData);
					}
					itemPurchaseData.CurrentPurchaseCount.Value = 1;
					itemPurchaseData.TotalPurchaseCount = 1;
					itemPurchaseData.IsLastChance = false;
					j = i + 1;
				}
			}
			string text2 = this.playerData.Get(StoreService.ITEMS_EXPIRED_KEY, "");
			if (!string.IsNullOrEmpty(text2))
			{
				StoreService.DataContainer data = new StoreService.DataContainer();
				data.seperatedIds = text2.Split(new char[]
				{
					','
				});
				int j= 0;
				for (int i = 0; i < data.seperatedIds.Length; i = j)
				{
					ItemPurchaseData itemPurchaseData2 = this.PurchaseData.FirstOrDefault(x => x.ItemId == data.seperatedIds[i]);
					if (itemPurchaseData2 == null)
					{
						itemPurchaseData2 = new ItemPurchaseData(data.seperatedIds[i]);
						this.PurchaseData.Add(itemPurchaseData2);
					}
					itemPurchaseData2.CurrentPurchaseCount.Value = 1;
					itemPurchaseData2.TotalPurchaseCount = 1;
					itemPurchaseData2.IsLastChance = false;
					j = i + 1;
				}
			}
			this.playerData.Set(StoreService.ITEMS_EXPIRED_KEY, "");
			this.playerData.Set(StoreService.ITEMS_PURCHASED_KEY, "");
		}

        private class DataContainer
        {
            public string[] seperatedIds;
        }

        // Token: 0x060024E7 RID: 9447 RVA: 0x000A0474 File Offset: 0x0009E674
		private void ConvertOldIdsToNewIds()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>
			{
				{
					"gold_55",
					"t05_hard"
				},
				{
					"gold_115",
					"t10_hard2"
				},
				{
					"gold_240",
					"t20_hard"
				},
				{
					"gold_625",
					"t50_hard"
				},
				{
					"gold_1300",
					"t60_hard"
				},
				{
					"monthlysubscription",
					"t10_goldsubscription"
				}
			};
			foreach (ItemPurchaseData itemPurchaseData in this.PurchaseData)
			{
				string itemId;
				if (dictionary.TryGetValue(itemPurchaseData.ItemId, out itemId))
				{
					itemPurchaseData.ItemId = itemId;
				}
			}
			this.logger.Debug(string.Join(",", (from x in this.PurchaseData
			select x.ItemId).ToArray<string>()));
		}

		// Token: 0x04002579 RID: 9593
		public static readonly string ITEMS_PURCHASED_KEY = "ItemsPurchased";

		// Token: 0x0400257A RID: 9594
		public static readonly string ITEMS_PURCHASE_DATA_KEY = "ItemsPurchaseData";

		// Token: 0x0400257B RID: 9595
		public static readonly string ITEMS_ACTIVE_KEY = "ActiveItems";

		// Token: 0x0400257C RID: 9596
		public static readonly string ITEMS_EXPIRED_KEY = "ExpiredItems";

		// Token: 0x04002582 RID: 9602
		public Dictionary<string, AdCapStoreItem> StoreItemMap = new Dictionary<string, AdCapStoreItem>();

		// Token: 0x04002583 RID: 9603
		private Dictionary<string, long> activeItemMap = new Dictionary<string, long>();

		// Token: 0x04002584 RID: 9604
		private ITriggerService triggerService;

		// Token: 0x04002585 RID: 9605
		private IDateTimeService dateTimeService;

		// Token: 0x04002586 RID: 9606
		private TimerService timerService;

		// Token: 0x04002587 RID: 9607
		private IUserDataService UserDataService;

		// Token: 0x04002588 RID: 9608
		private IGameController gameController;

		// Token: 0x04002589 RID: 9609
		private IGrantRewardService grantRewardService;

		// Token: 0x0400258A RID: 9610
		private IPlatformStore platformStore;

		// Token: 0x0400258B RID: 9611
		private UpgradeService upgradeService;

		// Token: 0x0400258C RID: 9612
		private IAngelInvestorService angelInvestorService;

		// Token: 0x0400258D RID: 9613
		private IInventoryService inventoryService;

		// Token: 0x0400258E RID: 9614
		private CompositeDisposable disposables = new CompositeDisposable();

		// Token: 0x0400258F RID: 9615
		private GameState state;

		// Token: 0x04002590 RID: 9616
		private PlayerData playerData;

		// Token: 0x04002591 RID: 9617
		private Platforms.Logger.Logger logger;
	}
}
