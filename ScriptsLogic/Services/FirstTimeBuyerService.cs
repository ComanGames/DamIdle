using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using UniRx;

// Token: 0x02000099 RID: 153
public class FirstTimeBuyerService : IDisposable
{
	// Token: 0x06000414 RID: 1044 RVA: 0x00015930 File Offset: 0x00013B30
	public void Init(List<FirstTimeBuyerGroup> firstTimeBuyerGroups, IGameController gameController, IUserDataService userDataService, IStoreService storeService)
	{
		this.firstTimeBuyerGroups = firstTimeBuyerGroups;
		this.gameController = gameController;
		this.userDataService = userDataService;
		this.storeService = storeService;
		this.RestorePurchaseState();
		for (int i = 0; i < firstTimeBuyerGroups.Count; i++)
		{
			FirstTimeBuyerGroup firstTimeBuyerGroup = firstTimeBuyerGroups[i];
			if (string.IsNullOrEmpty(firstTimeBuyerGroup.ABTestGroup) || userDataService.IsTestGroupMember(firstTimeBuyerGroup.ABTestGroup))
			{
				this.FirstTimeBuyerItemId.Value = firstTimeBuyerGroup.FirstTimeBuyerItemId;
				break;
			}
		}
		if (!this.HasPurchasedFirstTimeBuyerItem.Value)
		{
			this.ftbItemIds.Add("t08_firsttimebuyer");
			if (!string.IsNullOrEmpty(this.FirstTimeBuyerItemId.Value))
			{
				this.ftbItemIds.Add(this.FirstTimeBuyerItemId.Value);
			}
			this.EvaluateIfPurchasedFirstTimeBuyer();
		}
	}

	// Token: 0x06000415 RID: 1045 RVA: 0x000159F2 File Offset: 0x00013BF2
	public void Dispose()
	{
		this.storeDisposables.Dispose();
	}

	// Token: 0x06000416 RID: 1046 RVA: 0x00015A00 File Offset: 0x00013C00
	private void EvaluateIfPurchasedFirstTimeBuyer()
	{
		this.storeDisposables.Clear();
		bool flag = false;
		for (int i = 0; i < this.ftbItemIds.Count; i++)
		{
			string id = this.ftbItemIds[i];
			ItemPurchaseData itemPurchaseData = this.storeService.PurchaseData.FirstOrDefault(y => y.ItemId == id);
			if (itemPurchaseData != null)
			{
				if (itemPurchaseData.TotalPurchaseCount == 0)
				{
					(from x in itemPurchaseData.CurrentPurchaseCount
					where x > 0
					select x).Subscribe(delegate(int x)
					{
						this.SetHasPurchased();
					}).AddTo(this.storeDisposables);
				}
				else
				{
					this.SetHasPurchased();
				}
			}
			else
			{
				flag = true;
			}
		}
		if (flag && !this.HasPurchasedFirstTimeBuyerItem.Value)
		{
			(from x in this.storeService.PurchaseData.ObserveAdd()
			where this.ftbItemIds.Contains(x.Value.ItemId)
			select x).Subscribe(delegate(CollectionAddEvent<ItemPurchaseData> x)
			{
				this.EvaluateIfPurchasedFirstTimeBuyer();
			}).AddTo(this.storeDisposables);
		}
	}

	// Token: 0x06000417 RID: 1047 RVA: 0x00015B17 File Offset: 0x00013D17
	private void RestorePurchaseState()
	{
		this.HasPurchasedFirstTimeBuyerItem.Value = this.gameController.GlobalPlayerData.GetBool(FirstTimeBuyerService.HAS_PURCHAED_FTBB_BOOL_KEY);
	}

	// Token: 0x06000418 RID: 1048 RVA: 0x00015B39 File Offset: 0x00013D39
	private void SetHasPurchased()
	{
		this.HasPurchasedFirstTimeBuyerItem.Value = true;
		this.gameController.GlobalPlayerData.SetBool(FirstTimeBuyerService.HAS_PURCHAED_FTBB_BOOL_KEY, true);
	}

	// Token: 0x0400039C RID: 924
	public static readonly string HAS_PURCHAED_FTBB_BOOL_KEY = "HAS_PURCHAED_FTBB";

	// Token: 0x0400039D RID: 925
	public readonly ReactiveProperty<string> FirstTimeBuyerItemId = new ReactiveProperty<string>();

	// Token: 0x0400039E RID: 926
	public readonly ReactiveProperty<bool> HasPurchasedFirstTimeBuyerItem = new ReactiveProperty<bool>();

	// Token: 0x0400039F RID: 927
	private List<FirstTimeBuyerGroup> firstTimeBuyerGroups;

	// Token: 0x040003A0 RID: 928
	private IGameController gameController;

	// Token: 0x040003A1 RID: 929
	private IUserDataService userDataService;

	// Token: 0x040003A2 RID: 930
	private IStoreService storeService;

	// Token: 0x040003A3 RID: 931
	private List<string> ftbItemIds = new List<string>();

	// Token: 0x040003A4 RID: 932
	private CompositeDisposable storeDisposables = new CompositeDisposable();
}
