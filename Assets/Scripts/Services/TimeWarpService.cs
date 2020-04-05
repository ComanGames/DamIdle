using System;
using System.Collections.Generic;
using System.Linq;
using AdCap;
using AdCap.Store;
using HHTools.Navigation;
using UniRx;

// Token: 0x020000FE RID: 254
public class TimeWarpService : IDisposable
{
	// Token: 0x060006AD RID: 1709 RVA: 0x0002390C File Offset: 0x00021B0C
	public void Dispose()
	{
		this.unlockDisposable.Dispose();
		this.stateDisposable.Dispose();
		this.disposables.Dispose();
		this.storeItemDisposable.Dispose();
	}

	// Token: 0x060006AE RID: 1710 RVA: 0x0002393C File Offset: 0x00021B3C
	public void Init(UnfoldingService unfoldingService, IStoreService storeService)
	{
		this.stateDisposable = GameController.Instance.State.Subscribe(new Action<GameState>(this.OnStateChanged));
		this.unfoldingService = unfoldingService;
		this.storeService = storeService;
		this.RetrieveStoreItem(TimeWarpService.TIME_WARP_EXPRESS_ITEM_ID);
	}

	// Token: 0x060006AF RID: 1711 RVA: 0x000239A8 File Offset: 0x00021BA8
	private void RetrieveStoreItem(string tweStoreItemId)
	{
		AdCapStoreItem adCapStoreItem = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault((AdCapStoreItem x) => x.Id == tweStoreItemId);
		if (adCapStoreItem == null)
		{
			this.storeItemDisposable = this.storeService.CurrentCatalog.ObserveAdd().First((CollectionAddEvent<AdCapStoreItem> x) => x.Value.Id == tweStoreItemId).Subscribe(delegate(CollectionAddEvent<AdCapStoreItem> x)
			{
				this.TIME_WARP_EXPRESS_STORE_ITEM.Value = x.Value;
			});
			return;
		}
		this.TIME_WARP_EXPRESS_STORE_ITEM.Value = adCapStoreItem;
	}

	// Token: 0x060006B0 RID: 1712 RVA: 0x00023A32 File Offset: 0x00021C32
	public bool HasInventory()
	{
		return this.timeWarpExpressItem.Owned.Value > 0;
	}

	// Token: 0x060006B1 RID: 1713 RVA: 0x00023A48 File Offset: 0x00021C48
	public void UseTimeWarpExpress()
	{
		int hours;
		if (int.TryParse(this.timeWarpExpressItem.BonusCustomData, out hours))
		{
			if (GameController.Instance.game.VentureModels.Any((VentureModel v) => v.ProfitSurgeAmount((double)(hours * 3600), false) > 0.0))
			{
				if (this.inventory.ConsumeItem(TimeWarpService.TIME_WARP_EXPRESS_ITEM_ID, 1))
				{
					this.ApplyTimeWarp((double)(hours * 3600), true);
					return;
				}
			}
			else
			{
				GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("No Investments Managed!", "You do not have any managed investments and will not earn anything.", null, PopupModal.PopupOptions.OK, "OK", "No", true, null, "");
			}
		}
	}

	// Token: 0x060006B2 RID: 1714 RVA: 0x00023AF8 File Offset: 0x00021CF8
	public void ApplyTimeWarp(double seconds, bool isExpress = false)
	{
		double newSeconds = GameController.Instance.game.CalculateElapsed(seconds);
		double num = GameController.Instance.game.VentureModels.Sum((VentureModel v) => v.ProfitSurgeAmount(newSeconds, false));
		GameController.Instance.game.AddCash(num, true);
		MessageBroker.Default.Publish<TimeWarpEvent>(new TimeWarpEvent
		{
			AmountEarned = num,
			IsExpress = isExpress,
			Time = TimeSpan.FromSeconds(seconds)
		});
	}

	// Token: 0x060006B3 RID: 1715 RVA: 0x00023B84 File Offset: 0x00021D84
	private void OnStateChanged(GameState state)
	{
		if (state == null)
		{
			return;
		}
		if (this.unlockDisposable != null)
		{
			this.unlockDisposable.Dispose();
			this.unlockDisposable = null;
		}
		this.inventory = GameController.Instance.GlobalPlayerData.inventory;
		this.timeWarpExpressItem = this.inventory.GetItemById(TimeWarpService.TIME_WARP_EXPRESS_ITEM_ID);
		this.IsUnlocked.Value = GameController.Instance.GlobalPlayerData.GetBool("isTimewarpExpressUnlocked");
		if (!this.IsUnlocked.Value)
		{
			this.MonitorUnlockCondition();
		}
	}

	// Token: 0x060006B4 RID: 1716 RVA: 0x00023C0C File Offset: 0x00021E0C
	private void MonitorUnlockCondition()
	{
		if (FeatureConfig.IsFlagSet("ShowTimeWarpUnfolding"))
		{
			if (GameController.Instance.UnfoldingService.HasUnfoldingTrigger(TimeWarpService.UNFOLDING_ID))
			{
				if (!GameController.Instance.UnfoldingService.CompletedUnfoldingStepIds.Contains(TimeWarpService.UNFOLDING_ID.Id))
				{
					this.unlockDisposable = (from x in GameController.Instance.UnfoldingService.CompletedUnfoldingStepIds.ObserveAdd()
					where x.Value == TimeWarpService.UNFOLDING_ID.Id
					select x).Take(1).Subscribe(delegate(CollectionAddEvent<string> _)
					{
						this.UnlockTimeWarpExpress();
					});
					return;
				}
				this.UnlockTimeWarpExpress();
				return;
			}
			else
			{
				VentureModel ventureModel = GameController.Instance.game.VentureModels.FirstOrDefault((VentureModel model) => model.Id.ToLower() == "hockey");
				if (ventureModel != null)
				{
					this.unlockDisposable = (from count in ventureModel.TotalOwned
					where count >= 1.0
					select count).Take(1).Subscribe(delegate(double count)
					{
						this.UnlockTimeWarpExpress();
					});
					return;
				}
			}
		}
		else
		{
			TriggerData item = new TriggerData
			{
				TriggerType = ETriggerType.ItemOwned,
				Id = TimeWarpService.TIME_WARP_EXPRESS_ITEM_ID,
				Operator = ">=",
				Value = "1"
			};
			this.unlockDisposable = GameController.Instance.TriggerService.MonitorTriggers(new List<TriggerData>
			{
				item
			}, true).First((bool x) => x).Subscribe(delegate(bool _)
			{
				this.IsUnlocked.Value = true;
				GameController.Instance.GlobalPlayerData.SetBool("isTimewarpExpressUnlocked", true);
				this.unlockDisposable.Dispose();
			});
		}
	}

	// Token: 0x060006B5 RID: 1717 RVA: 0x00023DC4 File Offset: 0x00021FC4
	private void UnlockTimeWarpExpress()
	{
		this.unlockDisposable.Dispose();
		this.inventory.AddItem(TimeWarpService.TIME_WARP_EXPRESS_ITEM_ID, 3, true, true);
		FTUE_Manager.ShowFTUE("TimewarpExpress", null);
		this.IsUnlocked.Value = true;
		GameController.Instance.GlobalPlayerData.SetBool("isTimewarpExpressUnlocked", true);
	}

	// Token: 0x04000629 RID: 1577
	public readonly ReactiveProperty<bool> IsUnlocked = new BoolReactiveProperty();

	// Token: 0x0400062A RID: 1578
	public static readonly string TIME_WARP_EXPRESS_ITEM_ID = "time_warp_express";

	// Token: 0x0400062B RID: 1579
	public readonly ReactiveProperty<AdCapStoreItem> TIME_WARP_EXPRESS_STORE_ITEM = new ReactiveProperty<AdCapStoreItem>();

	// Token: 0x0400062C RID: 1580
	private const string UNLOCKED_KEY = "isTimewarpExpressUnlocked";

	// Token: 0x0400062D RID: 1581
	private static readonly UnfoldingId UNFOLDING_ID = new UnfoldingId
	{
		Id = "ShowTWE"
	};

	// Token: 0x0400062E RID: 1582
	private IInventoryService inventory;

	// Token: 0x0400062F RID: 1583
	private UnfoldingService unfoldingService;

	// Token: 0x04000630 RID: 1584
	private IStoreService storeService;

	// Token: 0x04000631 RID: 1585
	private IDisposable unlockDisposable = Disposable.Empty;

	// Token: 0x04000632 RID: 1586
	private IDisposable stateDisposable = Disposable.Empty;

	// Token: 0x04000633 RID: 1587
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000634 RID: 1588
	private IDisposable storeItemDisposable = Disposable.Empty;

	// Token: 0x04000635 RID: 1589
	private Item timeWarpExpressItem;
}
