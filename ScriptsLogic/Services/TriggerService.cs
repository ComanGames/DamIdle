using System;
using System.Collections.Generic;
using System.Linq;
using AdCap;
using AdCap.Store;
using Platforms.Logger;
using UniRx;
using UnityEngine;

// Token: 0x02000205 RID: 517
public class TriggerService : ITriggerService, IDisposable
{
	// Token: 0x06000EF8 RID: 3832 RVA: 0x00043B2C File Offset: 0x00041D2C
	public void Init(IGameController gameController, IUserDataService UserDataService, DataService dataService, IStoreService storeService, IAngelInvestorService angelService, IDateTimeService dateTimeService, NavigationService navigationService, IEventMissionService eventMissionService, PlanetMilestoneService planetMilestoneService, FirstTimeBuyerService firstTimePurchaseService)
	{
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		this.logger.Trace("Initializing.......", Array.Empty<object>());
		this.gameController = gameController;
		this.userDataService = UserDataService;
		this.dataService = dataService;
		this.storeService = storeService;
		this.angelService = angelService;
		this.dateTimeService = dateTimeService;
		this.navigationService = navigationService;
		this.eventMissionService = eventMissionService;
		this.planetMilestoneService = planetMilestoneService;
		this.firstTimePurchaseService = firstTimePurchaseService;
		this.gameController.State.Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
		this.logger.Trace("Initialized", Array.Empty<object>());
		gameController.OnLoadNewPlanetPre += this.UpdateIsLoadingPlanetTrue;
		gameController.OnLoadNewPlanetPost += this.UpdateIsLoadingPlanetFalse;
	}

	// Token: 0x06000EF9 RID: 3833 RVA: 0x00043C08 File Offset: 0x00041E08
	private void UpdateIsLoadingPlanetTrue()
	{
		this.isLoadingPlanet.Value = true;
	}

	// Token: 0x06000EFA RID: 3834 RVA: 0x00043C16 File Offset: 0x00041E16
	private void UpdateIsLoadingPlanetFalse()
	{
		this.isLoadingPlanet.Value = false;
	}

	// Token: 0x06000EFB RID: 3835 RVA: 0x00043C24 File Offset: 0x00041E24
	private void OnStateChanged(GameState state)
	{
		this.stateDisposables.Clear();
		this.state.Value = state;
		this.ventureMap.Clear();
		this.saveDataMap.Clear();
		this.state.Value.VentureModels.ToList<VentureModel>().ForEach(delegate(VentureModel venture)
		{
			this.ventureMap[venture.Id] = venture;
		});
		if (state.planetPlayerData != null && null != state.PlanetData)
		{
			(from value in state.planetPlayerData.GetObservable("PlanetLoadCount", 0.0)
			select value == 1.0).Subscribe(delegate(bool x)
			{
				this.CreateOrRetrieveEventStartedObservable(state.PlanetData.PlanetName).Value = x;
			}).AddTo(this.stateDisposables);
		}
	}

	// Token: 0x06000EFC RID: 3836 RVA: 0x00043D1C File Offset: 0x00041F1C
	public void Dispose()
	{
		this.disposables.Dispose();
		this.gameController.OnLoadNewPlanetPre -= this.UpdateIsLoadingPlanetTrue;
		this.gameController.OnLoadNewPlanetPre -= this.UpdateIsLoadingPlanetFalse;
	}

	// Token: 0x06000EFD RID: 3837 RVA: 0x00043D58 File Offset: 0x00041F58
	public IObservable<bool> MonitorTriggers(List<TriggerData> triggerDatas, bool defaultValueWhenNoTriggersPresent)
	{
		if (triggerDatas.Count != 0)
		{
			TriggerData[] array = (from o in triggerDatas
			orderby o.TriggerGroup
			select o).ToArray<TriggerData>();
			List<IObservable<bool>> list = new List<IObservable<bool>>();
			int triggerGroup = array[0].TriggerGroup;
			List<IObservable<bool>> list2 = new List<IObservable<bool>>();
			for (int i = 0; i < triggerDatas.Count; i++)
			{
				TriggerData triggerData = triggerDatas[i];
				if (triggerData.TriggerGroup != triggerGroup)
				{
					list.Add(from a in list2.CombineLatest<bool>()
					select a.All(v => v));
					list2 = new List<IObservable<bool>>();
					triggerGroup = triggerData.TriggerGroup;
				}
				IObservable<bool> item = this.MonitorTriggerInternal(triggerDatas[i]);
				list2.Add(item);
			}
			list.Add(from a in list2.CombineLatest<bool>()
			select a.All(v => v));
			return from a in list.CombineLatest<bool>()
			select a.Any(v => v);
		}
		return Observable.Return(defaultValueWhenNoTriggersPresent);
	}

	// Token: 0x06000EFE RID: 3838 RVA: 0x00043E88 File Offset: 0x00042088
	public bool CheckTrigger(TriggerData triggerData)
	{
		bool result = false;
		this.MonitorTriggerInternal(triggerData).Subscribe(delegate(bool x)
		{
			result = x;
		}).Dispose();
		return result;
	}

	// Token: 0x06000EFF RID: 3839 RVA: 0x00043EC8 File Offset: 0x000420C8
	private IObservable<bool> MonitorTriggerInternal(TriggerData triggerData)
	{
		TriggerService.TriggerContainer result = new TriggerService.TriggerContainer();
		result.triggerData = triggerData;
		result.triggerService = this;
		result.threshold = 0.0;
		switch (result.triggerData.TriggerType)
		{
		case ETriggerType.None:
			return Observable.Return(true);
		case ETriggerType.EventStart:
			return this.CreateOrRetrieveEventStartedObservable(result.triggerData.Id);
		case ETriggerType.EventUnlocked:
			return (from x in MessageBroker.Default.Receive<EventUnlockedEvent>()
			where string.IsNullOrEmpty(result.triggerData.Value) || result.triggerData.Id == x.EventData.name
			select x into _
			select true).StartWith(false);
		case ETriggerType.ItemPurchased:
			return from _ in this.storeService.PurchaseData.ObserveCountChanged(true)
			select result.triggerService.CheckPurchasedItem(result.triggerData.Id);
		case ETriggerType.PlanetUnlocked:
			return (from x in MessageBroker.Default.Receive<PlanetPurchasedEvent>()
			select x.planetName == result.triggerData.Id).StartWith(false);
		case ETriggerType.VentureQuantity:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return this.MonitorVentureQuantity(result.triggerData, result.threshold);
			}
			break;
		case ETriggerType.SessionCount:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return from x in this.gameController.GlobalPlayerData.GetObservable(GameController.SESSION_COUNT_KEY, 0.0)
				select result.triggerService.Compare<double>(result.triggerData.Operator, x, result.threshold);
			}
			break;
		case ETriggerType.GoldOnHand:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return from x in this.gameController.GlobalPlayerData.GetObservable("Gold", 0.0)
				select result.triggerService.Compare<double>(result.triggerData.Operator, x, result.threshold);
			}
			break;
		case ETriggerType.MegaBucksOnHand:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return from x in this.gameController.GlobalPlayerData.GetObservable("MegaBucksBalance", 0.0)
				select result.triggerService.Compare<double>(result.triggerData.Operator, x, result.threshold);
			}
			break;
		case ETriggerType.CompletedRealMoneyPurchases:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return from x in this.storeService.TotalRMPurchaseCount
				select result.triggerService.Compare<double>(result.triggerData.Operator, (double)x, result.threshold);
			}
			break;
		case ETriggerType.InAbTestGroup:
			return Observable.Return(this.userDataService.IsTestGroupMember(result.triggerData.Id, result.triggerData.Value));
		case ETriggerType.AngelsClaimed:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return this.MonitorAngelClaimedQuantity(result.triggerData, result.threshold);
			}
			break;
		case ETriggerType.BundleViewed:
		{
			ItemPurchaseData itemPurchaseData = this.storeService.PurchaseData.FirstOrDefault(data => data.ItemId == result.triggerData.Id);
			if (itemPurchaseData != null)
			{
				return Observable.Return(true);
			}
			return (from x in this.storeService.PurchaseData.ObserveAdd()
			where x.Value.ItemId == result.triggerData.Id
			select Observable.Return(true)).Switch<bool>();
		}
		case ETriggerType.TimeUtcNow:
		{
			DateTime dt;
			if (DateTime.TryParse(result.triggerData.Value, out dt))
			{
				return from x in Observable.Interval(TimeSpan.FromSeconds(5.0))
				select result.triggerService.Compare<DateTime>(result.triggerData.Operator, result.triggerService.dateTimeService.UtcNow, dt);
			}
			break;
		}
		case ETriggerType.ValueToDateUSD:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return (from x in this.dataService.ValueToDateUSD
				select result.triggerService.Compare<double>(result.triggerData.Operator, x, result.threshold)).StartWith(false);
			}
			break;
		case ETriggerType.AngelResetCount:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return this.MonitorAngelResetQuantity(result.triggerData, result.threshold);
			}
			break;
		case ETriggerType.InSegment:
		{
			bool flag = this.dataService.PlayerSegments.Contains(result.triggerData.Id);
			bool flag2 = bool.Parse(result.triggerData.Value);
			string @operator = result.triggerData.Operator;
			if (@operator == "==")
			{
				return Observable.Return(flag == flag2);
			}
			if (!(@operator == "!="))
			{
				Debug.LogError("[TriggerService](InSegment) Operator " + result.triggerData.Operator + " is not valid for this trigger type");
				return Observable.Return(false);
			}
			return Observable.Return(flag != flag2);
		}
		case ETriggerType.InEvent:
		{
			bool inEvent;
			if (bool.TryParse(result.triggerData.Value, out inEvent))
			{
				return from x in this.gameController.State
				select result.triggerService.Compare<bool>(result.triggerData.Operator, inEvent, x.IsEventPlanet);
			}
			break;
		}
		case ETriggerType.NumEventsPlayed:
		{
			int numEvents;
			if (int.TryParse(result.triggerData.Value, out numEvents))
			{
				return from x in this.gameController.EventService.PastEvents.ObserveCountChanged(true)
				select result.triggerService.Compare<int>(result.triggerData.Operator, x, numEvents);
			}
			break;
		}
		case ETriggerType.TutorialStepComplete:
		{
			bool isCompleted;
			if (bool.TryParse(result.triggerData.Value, out isCompleted))
			{
				return (from x in this.gameController.TutorialService.CompletedTutorialSteps.ObserveCountChanged(true)
				select result.triggerService.gameController.TutorialService.CompletedTutorialSteps.Contains(result.triggerData.Id) == isCompleted).StartWith(false);
			}
			break;
		}
		case ETriggerType.UpgradePurchased:
			return this.MonitorUpgradePurchased(result.triggerData);
		case ETriggerType.CashOnHand:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return this.MonitorCashOnHand(result.triggerData, result.threshold);
			}
			break;
		case ETriggerType.AngelsPending:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return this.MonitorPendingAngels(result.triggerData, result.threshold);
			}
			break;
		case ETriggerType.ItemOwned:
		{
			Item itemById = this.gameController.GlobalPlayerData.inventory.GetItemById(result.triggerData.Id);
			if (itemById == null)
			{
				this.logger.Error("Trying to monitor an Item that doesn't exist");
			}
			else if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return from x in itemById.Owned
				select result.triggerService.Compare<double>(result.triggerData.Operator, (double)x, result.threshold);
			}
			break;
		}
		case ETriggerType.CurrentGameLocation:
			return this.MonitorGameLocation(result.triggerData);
		case ETriggerType.BuyMultiplierValue:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return from x in this.gameController.BuyMultiplierService.BuyCount
				select result.triggerService.Compare<double>(result.triggerData.Operator, (double)x, result.threshold);
			}
			break;
		case ETriggerType.VentureGilded:
			return this.MonitorVentureGilded(result.triggerData);
		case ETriggerType.ItemTypeQuantity:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return this.MonitorItemTypeOwned(result.triggerData, result.threshold);
			}
			break;
		case ETriggerType.GiftClaimed:
			if (this.gameController.GiftService.ClaimedGiftIds.Contains(result.triggerData.Id))
			{
				return Observable.Return(true);
			}
			return from x in this.gameController.GiftService.ClaimedGiftIds.ObserveAdd()
			select x.Value == result.triggerData.Id;
		case ETriggerType.VentureRunning:
		{
			bool value;
			if (bool.TryParse(result.triggerData.Value, out value))
			{
				return this.MonitorVentureRunning(result.triggerData, value);
			}
			break;
		}
		case ETriggerType.AngelsPendingPercentage:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return this.MonitorPendingAngelsPercentage(result.triggerData, result.threshold);
			}
			break;
		case ETriggerType.UnfoldingStepComplete:
		{
			bool flag3;
			if (bool.TryParse(result.triggerData.Value, out flag3))
			{
				if (this.gameController.UnfoldingService.CompletedUnfoldingStepIds.Contains(result.triggerData.Id) == flag3)
				{
					return Observable.Return(true);
				}
				return (from x in this.gameController.UnfoldingService.CompletedUnfoldingStepIds.ObserveAdd()
				select x.Value == result.triggerData.Id);
			}
			break;
		}
		case ETriggerType.EventScore:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return from x in this.eventMissionService.CurentScore
				select result.triggerService.Compare<double>(result.triggerData.Operator, (double)x, result.threshold);
			}
			break;
		case ETriggerType.PlanetMilestoneClaimedCount:
			if (double.TryParse(result.triggerData.Value, out result.threshold))
			{
				return from x in (from x in this.planetMilestoneService.GetUserMilestonesForCurrentPlanet()
				select x.State).CombineLatest<UserPlanetMilestone.PlanetMilestoneState>()
				select result.triggerService.Compare<double>(result.triggerData.Operator, (double)x.Count(y => y == UserPlanetMilestone.PlanetMilestoneState.CLAIMED), result.threshold);
			}
			break;
		case ETriggerType.PlanetProgressionType:
		{
			PlanetProgressionType progType;
			if (Enum.TryParse<PlanetProgressionType>(result.triggerData.Value, true, out progType))
			{
				return from state in this.state
				select result.triggerService.Compare<int>(result.triggerData.Operator, (int)progType, (int)state.progressionType);
			}
			break;
		}
		case ETriggerType.FirstTimeBuyerPackPurchased:
		{
			bool isPurchased;
			if (bool.TryParse(result.triggerData.Value, out isPurchased))
			{
				return (from x in this.firstTimePurchaseService.HasPurchasedFirstTimeBuyerItem
				select x == isPurchased).StartWith(this.firstTimePurchaseService.HasPurchasedFirstTimeBuyerItem.Value);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		return Observable.Return(false);
	}

    private class TriggerContainer
    {
        public TriggerData triggerData;
        public TriggerService triggerService;
        public double threshold;
    }

    // Token: 0x06000F00 RID: 3840 RVA: 0x000449C8 File Offset: 0x00042BC8
	private IObservable<bool> MonitorGameLocation(TriggerData triggerData)
	{
		return OrientationController.Instance.OrientationStream.CombineLatest(this.navigationService.CurrentLocation, this.gameController.State, delegate(OrientationChangedEvent orientation, string loc, GameState state)
		{
			string a = triggerData.Id;
			if (!orientation.IsPortrait && a == "Menu")
			{
				a = "Root";
			}
			string value = state.planetTheme.Value;
			return a == loc && (string.IsNullOrEmpty(triggerData.PlanetId) || triggerData.PlanetId == value);
		}).StartWith(false);
	}

	// Token: 0x06000F01 RID: 3841 RVA: 0x00044A1C File Offset: 0x00042C1C
	private IObservable<bool> MonitorPendingAngels(TriggerData triggerData, double threshold)
	{
		return (from s in this.state.ObserveEveryValueChanged(x => x.Value, FrameCountType.Update, false)
		where s != null
		select s).CombineLatest(this.angelService.RewardAngelsAtInterval, this.isLoadingPlanet, (s, pendingAngels, isLoading) => !isLoading && s.planetTheme.Value == triggerData.PlanetId && this.Compare<double>(triggerData.Operator, pendingAngels, threshold)).StartWith(false);
	}

	// Token: 0x06000F02 RID: 3842 RVA: 0x00044ABC File Offset: 0x00042CBC
	private IObservable<bool> MonitorPendingAngelsPercentage(TriggerData triggerData, double threshold)
	{
		return (from s in this.state.ObserveEveryValueChanged(x => x.Value, FrameCountType.Update, false)
		where s != null
		select s).CombineLatest(this.angelService.RewardAngelsAtInterval, this.angelService.AngelsOnHand, this.isLoadingPlanet, delegate(GameState s, double pending, double onHand, bool isLoading)
		{
			double num = onHand * threshold;
			bool flag = this.Compare<double>(triggerData.Operator, pending, num);
			return !isLoading && s.planetTheme.Value == triggerData.PlanetId && num > 0.0 && flag;
		}).StartWith(false);
	}

	// Token: 0x06000F03 RID: 3843 RVA: 0x00044B68 File Offset: 0x00042D68
	private IObservable<bool> MonitorItemTypeOwned(TriggerData triggerData, double threshold)
	{
		IObservable<bool> result;
		try
		{
			ItemType key = (ItemType)Enum.Parse(typeof(ItemType), triggerData.Id);
			result = from x in this.gameController.GlobalPlayerData.inventory.ItemTypeQuantityMap[key]
			select this.Compare<double>(triggerData.Operator, (double)x, threshold);
		}
		catch (Exception)
		{
			result = Observable.Return(false);
		}
		return result;
	}

	// Token: 0x06000F04 RID: 3844 RVA: 0x00044BFC File Offset: 0x00042DFC
	private IObservable<bool> MonitorUpgradePurchased(TriggerData triggerData)
	{
		return from _ in this.gameController.UpgradeService.PurchasedUpgradeCollection.ObserveCountChanged(true)
		select this.gameController.UpgradeService.PurchasedUpgradeCollection.Contains(triggerData.Id);
	}

	// Token: 0x06000F05 RID: 3845 RVA: 0x00044C44 File Offset: 0x00042E44
	private IObservable<bool> MonitorCashOnHand(TriggerData triggerData, double threshold)
	{
		if (this.state.Value != null && this.state.Value.planetTheme.Value == triggerData.PlanetId)
		{
			return (from t in (from s in this.state.ObserveEveryValueChanged(x => x.Value, FrameCountType.Update, false)
			where s != null
			select s).CombineLatest(this.state.Value.CashOnHand, (s, cash) => new Tuple<string, double>(s.planetTheme.Value, cash))
			where t.Item1 == triggerData.PlanetId
			select this.Compare<double>(triggerData.Operator, t.Item2, threshold)).StartWith(false);
		}
		if (!string.IsNullOrEmpty(triggerData.PlanetId))
		{
			if (!this.saveDataMap.ContainsKey(triggerData.PlanetId))
			{
				bool isLoading = true;
				GameStateSaveLoad.Load(triggerData.PlanetId, delegate(GameStateSaveData sData)
				{
					if (sData != null)
					{
						this.saveDataMap[triggerData.PlanetId] = sData;
					}
					isLoading = false;
				});
				if (isLoading)
				{
					return Observable.Return(false);
				}
			}
			if (this.saveDataMap.ContainsKey(triggerData.PlanetId) && this.saveDataMap[triggerData.PlanetId] != null)
			{
				GameStateSaveData gameStateSaveData = this.saveDataMap[triggerData.PlanetId];
				if (this.Compare<double>(triggerData.Operator, gameStateSaveData.cashOnHand, threshold))
				{
					return Observable.Return(true);
				}
			}
			return (from t in (from s in this.state.ObserveEveryValueChanged(x => x.Value, FrameCountType.Update, false)
			where s != null
			select s).CombineLatest(this.state.Value.CashOnHand, (s, cash) => new Tuple<string, double>(s.planetTheme.Value, cash))
			where t.Item1 == triggerData.PlanetId
			select this.Compare<double>(triggerData.Operator, t.Item2, threshold)).StartWith(false);
		}
		return Observable.Return(false);
	}

	// Token: 0x06000F06 RID: 3846 RVA: 0x00044EE8 File Offset: 0x000430E8
	private IObservable<bool> MonitorVentureQuantity(TriggerData triggerData, double threshold)
	{
		VentureModel value;
		if (this.ventureMap.TryGetValue(triggerData.Id, out value))
		{
			return (from v in this.ventureMap.ObserveAdd().StartWith(new DictionaryAddEvent<string, VentureModel>(triggerData.Id, value))
			where v.Key == triggerData.Id
			select v).Select(delegate(DictionaryAddEvent<string, VentureModel> v)
			{
				IObservable<double> totalOwned = v.Value.TotalOwned;
				Func<double, bool> selector = ( (x => this.Compare<double>(triggerData.Operator, x, threshold)));
				return totalOwned.Select(selector);
			}).Switch<bool>().StartWith(false);
		}
		if (!string.IsNullOrEmpty(triggerData.PlanetId))
		{
			if (!this.saveDataMap.ContainsKey(triggerData.PlanetId))
			{
				bool isLoading = true;
				GameStateSaveLoad.Load(triggerData.PlanetId, delegate(GameStateSaveData sData)
				{
					if (sData != null)
					{
						this.saveDataMap[triggerData.PlanetId] = sData;
					}
					isLoading = false;
				});
				if (isLoading)
				{
					return Observable.Return(false);
				}
			}
			if (this.saveDataMap.ContainsKey(triggerData.PlanetId) && this.saveDataMap[triggerData.PlanetId] != null)
			{
				VentureSaveData ventureSaveData = this.saveDataMap[triggerData.PlanetId].ventures.FirstOrDefault(x => x.id == triggerData.Id);
				if (ventureSaveData != null && this.Compare<double>(triggerData.Operator, ventureSaveData.numOwned, threshold))
				{
					return Observable.Return(true);
				}
			}
			return (from v in this.ventureMap.ObserveAdd()
			where v.Key == triggerData.Id
			select v).Select(delegate(DictionaryAddEvent<string, VentureModel> v)
			{
				IObservable<double> totalOwned = v.Value.TotalOwned;
				Func<double, bool> selector = ((x => this.Compare<double>(triggerData.Operator, x, threshold)));
				return totalOwned.Select(selector);
			}).Switch<bool>().StartWith(false);
		}
		return Observable.Return(false);
	}

	// Token: 0x06000F07 RID: 3847 RVA: 0x000450B4 File Offset: 0x000432B4
	private IObservable<bool> MonitorVentureGilded(TriggerData triggerData)
	{
		VentureModel value;
		if (this.ventureMap.TryGetValue(triggerData.Id, out value))
		{
			return (from v in this.ventureMap.ObserveAdd().StartWith(new DictionaryAddEvent<string, VentureModel>(triggerData.Id, value))
			where v.Key == triggerData.Id
			select 
				from b in v.Value.IsBoosted
				select b).Switch<bool>().StartWith(false);
		}
		if (!string.IsNullOrEmpty(triggerData.PlanetId))
		{
			if (!this.saveDataMap.ContainsKey(triggerData.PlanetId))
			{
				bool isLoading = true;
				GameStateSaveLoad.Load(triggerData.PlanetId, delegate(GameStateSaveData sData)
				{
					if (sData != null)
					{
						this.saveDataMap[triggerData.PlanetId] = sData;
					}
					isLoading = false;
				});
				if (isLoading)
				{
					return Observable.Return(false);
				}
			}
			if (this.saveDataMap.ContainsKey(triggerData.PlanetId) && this.saveDataMap[triggerData.PlanetId] != null)
			{
				VentureSaveData ventureSaveData = this.saveDataMap[triggerData.PlanetId].ventures.FirstOrDefault(x => x.id == triggerData.Id);
				if (ventureSaveData != null && ventureSaveData.isBoosted)
				{
					return Observable.Return(true);
				}
			}
			return (from v in this.ventureMap.ObserveAdd()
			where v.Key == triggerData.Id
			select 
				from b in v.Value.IsBoosted
				select b).Switch<bool>().StartWith(false);
		}
		return Observable.Return(false);
	}

	// Token: 0x06000F08 RID: 3848 RVA: 0x00045288 File Offset: 0x00043488
	private IObservable<bool> MonitorVentureRunning(TriggerData triggerData, bool value)
	{
		VentureModel value2;
		if (this.ventureMap.TryGetValue(triggerData.Id, out value2))
		{
			return (from v in this.ventureMap.ObserveAdd().StartWith(new DictionaryAddEvent<string, VentureModel>(triggerData.Id, value2))
			where v.Key == triggerData.Id
			select v).Select(delegate(DictionaryAddEvent<string, VentureModel> v)
			{
				IObservable<bool> isRunning = v.Value.IsRunning;
				Func<bool, bool> selector= b => b == value;
				return isRunning.Select(selector);
			}).Switch<bool>().StartWith(false);
		}
		return (from v in this.ventureMap.ObserveAdd()
		where v.Key == triggerData.Id
		select v).Select(delegate(DictionaryAddEvent<string, VentureModel> v)
		{
			IObservable<bool> isRunning = v.Value.IsRunning;
			Func<bool, bool> selector;
            selector = (b => b == value);
			return isRunning.Select(selector);
		}).Switch<bool>().StartWith(false);
	}

	// Token: 0x06000F09 RID: 3849 RVA: 0x0004534C File Offset: 0x0004354C
	private IObservable<bool> MonitorAngelClaimedQuantity(TriggerData triggerData, double threshold)
	{
		if (this.state.Value != null && this.state.Value.planetTheme.Value == triggerData.PlanetId)
		{
			return (from x in (from x in this.state.ObserveEveryValueChanged(x => x.Value, FrameCountType.Update, false)
			where x != null
			select x).CombineLatest(this.angelService.AngelsOnHand, (d, p) => new Tuple<GameState, double>(this.state.Value, this.angelService.AngelsOnHand.Value + this.angelService.AngelsSpent.Value)).Where((x, y) => x.Item1.planetTheme.Value == triggerData.PlanetId)
			select this.Compare<double>(triggerData.Operator, x.Item2, threshold)).StartWith(false);
		}
		if (!string.IsNullOrEmpty(triggerData.PlanetId))
		{
			if (!this.saveDataMap.ContainsKey(triggerData.PlanetId))
			{
				bool isLoading = true;
				GameStateSaveLoad.Load(triggerData.PlanetId, delegate(GameStateSaveData sData)
				{
					if (sData != null)
					{
						this.saveDataMap[triggerData.PlanetId] = sData;
					}
					isLoading = false;
				});
				if (isLoading)
				{
					return Observable.Return(false);
				}
			}
			if (this.saveDataMap.ContainsKey(triggerData.PlanetId) && this.saveDataMap[triggerData.PlanetId] != null)
			{
				GameStateSaveData gameStateSaveData = this.saveDataMap[triggerData.PlanetId];
				double left = gameStateSaveData.angelInvestors + gameStateSaveData.angelInvestorsSpent;
				if (this.Compare<double>(triggerData.Operator, left, threshold))
				{
					return Observable.Return(true);
				}
			}
			return (from x in (from x in this.state.ObserveEveryValueChanged(x => x.Value, FrameCountType.Update, false)
			where x != null
			select x).CombineLatest(this.angelService.AngelsOnHand, (d, p) => new Tuple<GameState, double>(this.state.Value, this.angelService.AngelsOnHand.Value + this.angelService.AngelsSpent.Value)).Where((x, y) => x.Item1.planetTheme.Value == triggerData.PlanetId)
			select this.Compare<double>(triggerData.Operator, x.Item2, threshold)).StartWith(false);
		}
		return Observable.Return(false);
	}

	// Token: 0x06000F0A RID: 3850 RVA: 0x000455C8 File Offset: 0x000437C8
	private IObservable<bool> MonitorAngelResetQuantity(TriggerData triggerData, double threshold)
	{
		if (this.state.Value != null && this.state.Value.planetTheme.Value == triggerData.PlanetId)
		{
			return (from x in (from x in this.state.ObserveEveryValueChanged(x => x.Value, FrameCountType.Update, false)
			where x != null
			select x).CombineLatest(this.angelService.AngelResetCount, (d, p) => new Tuple<GameState, double>(this.state.Value, (double)p)).Where((x, y) => x.Item1.planetTheme.Value == triggerData.PlanetId)
			select this.Compare<double>(triggerData.Operator, x.Item2, threshold)).StartWith(false);
		}
		if (!string.IsNullOrEmpty(triggerData.PlanetId))
		{
			if (!this.saveDataMap.ContainsKey(triggerData.PlanetId))
			{
				bool isLoading = true;
				GameStateSaveLoad.Load(triggerData.PlanetId, delegate(GameStateSaveData sData)
				{
					if (sData != null)
					{
						this.saveDataMap[triggerData.PlanetId] = sData;
					}
					isLoading = false;
				});
				if (isLoading)
				{
					return Observable.Return(false);
				}
			}
			if (this.saveDataMap.ContainsKey(triggerData.PlanetId) && this.saveDataMap[triggerData.PlanetId] != null)
			{
				double left = (double)this.saveDataMap[triggerData.PlanetId].angelResetCount;
				if (this.Compare<double>(triggerData.Operator, left, threshold))
				{
					return Observable.Return(true);
				}
			}
			return (from x in (from x in this.state.ObserveEveryValueChanged(x => x.Value, FrameCountType.Update, false)
			where x != null
			select x).CombineLatest(this.angelService.AngelResetCount, (d, p) => new Tuple<GameState, double>(this.state.Value, (double)p)).Where((x, y) => x.Item1.planetTheme.Value == triggerData.PlanetId)
			select this.Compare<double>(triggerData.Operator, x.Item2, threshold)).StartWith(false);
		}
		return Observable.Return(false);
	}

	// Token: 0x06000F0B RID: 3851 RVA: 0x0004583C File Offset: 0x00043A3C
	private bool CheckPurchasedItem(string id)
	{
		return this.storeService.PurchaseData.Any(x => x.ItemId == id && x.TotalPurchaseCount > 0);
	}

	// Token: 0x06000F0C RID: 3852 RVA: 0x00045874 File Offset: 0x00043A74
	private bool Compare<T>(string op, T left, T right) where T : IComparable<T>
	{
		if (left != null && right == null)
		{
			return true;
		}
		if (left == null)
		{
			return false;
		}
		if (op == "<")
		{
			return left.CompareTo(right) < 0;
		}
		if (op == ">")
		{
			return left.CompareTo(right) > 0;
		}
		if (op == "<=")
		{
			return left.CompareTo(right) <= 0;
		}
		if (op == ">=")
		{
			return left.CompareTo(right) >= 0;
		}
		if (op == "==")
		{
			return left.Equals(right);
		}
		if (!(op == "!="))
		{
			throw new ArgumentException("Invalid comparison operator: {0}", op);
		}
		return !left.Equals(right);
	}

	// Token: 0x06000F0D RID: 3853 RVA: 0x00045971 File Offset: 0x00043B71
	private BoolReactiveProperty CreateOrRetrieveEventStartedObservable(string planetName)
	{
		if (!this.planetIDToEventStartedMap.ContainsKey(planetName))
		{
			this.planetIDToEventStartedMap.Add(planetName, new BoolReactiveProperty());
		}
		return this.planetIDToEventStartedMap[planetName];
	}

	// Token: 0x04000CE5 RID: 3301
	public const string EVENT_START_STATISTIC_KEY = "PlanetLoadCount";

	// Token: 0x04000CE6 RID: 3302
	private IStoreService storeService;

	// Token: 0x04000CE7 RID: 3303
	private IUserDataService userDataService;

	// Token: 0x04000CE8 RID: 3304
	private DataService dataService;

	// Token: 0x04000CE9 RID: 3305
	private IAngelInvestorService angelService;

	// Token: 0x04000CEA RID: 3306
	private IDateTimeService dateTimeService;

	// Token: 0x04000CEB RID: 3307
	private NavigationService navigationService;

	// Token: 0x04000CEC RID: 3308
	private IEventMissionService eventMissionService;

	// Token: 0x04000CED RID: 3309
	private PlanetMilestoneService planetMilestoneService;

	// Token: 0x04000CEE RID: 3310
	private FirstTimeBuyerService firstTimePurchaseService;

	// Token: 0x04000CEF RID: 3311
	private readonly ReactiveProperty<GameState> state = new ReactiveProperty<GameState>();

	// Token: 0x04000CF0 RID: 3312
	private readonly ReactiveDictionary<string, VentureModel> ventureMap = new ReactiveDictionary<string, VentureModel>();

	// Token: 0x04000CF1 RID: 3313
	private Dictionary<string, BoolReactiveProperty> planetIDToEventStartedMap = new Dictionary<string, BoolReactiveProperty>();

	// Token: 0x04000CF2 RID: 3314
	private Dictionary<string, GameStateSaveData> saveDataMap = new Dictionary<string, GameStateSaveData>();

	// Token: 0x04000CF3 RID: 3315
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000CF4 RID: 3316
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x04000CF5 RID: 3317
	private IGameController gameController;

	// Token: 0x04000CF6 RID: 3318
	private Platforms.Logger.Logger logger;

	// Token: 0x04000CF7 RID: 3319
	private ReactiveProperty<bool> isLoadingPlanet = new ReactiveProperty<bool>(false);
}
