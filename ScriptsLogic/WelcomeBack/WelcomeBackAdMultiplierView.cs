using System;
using System.Linq;
using AdCap.Store;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000263 RID: 611
public class WelcomeBackAdMultiplierView : AnimatedModal
{
	// Token: 0x06001117 RID: 4375 RVA: 0x0004E9D0 File Offset: 0x0004CBD0
	protected override void Awake()
	{
		this.regularPriceText.gameObject.SetActive(false);
		base.Awake();
		this.btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnClose)).AddTo(base.gameObject);
	}

	// Token: 0x06001118 RID: 4376 RVA: 0x0004EA1C File Offset: 0x0004CC1C
	public void Start()
	{
		this.offlineCashMultiplier = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault(x => x.Id == "offline_cash_multiplier");
		if (this.offlineCashMultiplier == null)
		{
			this.btn_Purchase.gameObject.SetActive(false);
			GameController.Instance.StoreService.CurrentCatalog.ObserveAdd().First(x => x.Value.Id == "offline_cash_multiplier").Subscribe(new Action<CollectionAddEvent<AdCapStoreItem>>(this.OnOfflineItemReceived)).AddTo(base.gameObject);
		}
		else
		{
			this.btn_Purchase.gameObject.SetActive(true);
			this.UpdatePurchaseDetails(this.offlineCashMultiplier);
		}
		(from x in MessageBroker.Default.Receive<StorePurchaseEvent>()
		where x.PurchaseState == EStorePurchaseState.Success
		select x).Subscribe(new Action<StorePurchaseEvent>(this.OnStoreItemPurchased)).AddTo(base.gameObject);
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
	}

	// Token: 0x06001119 RID: 4377 RVA: 0x0004EB64 File Offset: 0x0004CD64
	private void OnOfflineItemReceived(CollectionAddEvent<AdCapStoreItem> offlineCashMultiplier)
	{
		this.offlineCashMultiplier = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault(x => x.Id == "offline_cash_multiplier");
		if (this.offlineCashMultiplier != null)
		{
			this.UpdatePurchaseDetails(this.offlineCashMultiplier);
		}
	}

	// Token: 0x0600111A RID: 4378 RVA: 0x0004EBC0 File Offset: 0x0004CDC0
	private void OnStoreItemPurchased(StorePurchaseEvent e)
	{
		IInventoryService inventory = GameController.Instance.GlobalPlayerData.inventory;
		for (int i = 0; i < e.Item.Rewards.Count; i++)
		{
			if (e.Item.Rewards[i].RewardType == ERewardType.Item)
			{
				Item itemById = inventory.GetItemById(e.Item.Rewards[i].Id);
				if (itemById != null && itemById.Product == Product.OfflineCashMultiplier)
				{
					this.OnOfflineCashMultiplierPurchased();
				}
			}
		}
	}

	// Token: 0x0600111B RID: 4379 RVA: 0x0004EC44 File Offset: 0x0004CE44
	private void UpdatePurchaseDetails(AdCapStoreItem offlineCashMultiplier)
	{
		if (offlineCashMultiplier.Cost.IsSaleActive)
		{
			this.regularPriceText.gameObject.SetActive(true);
			this.regularPriceText.text = offlineCashMultiplier.Cost.Price.ToString();
		}
		this.UpdateText();
	}

	// Token: 0x0600111C RID: 4380 RVA: 0x0004EC90 File Offset: 0x0004CE90
	public void WireData(double offlineCashEarned)
	{
		this.adBonusTotal = offlineCashEarned * 2.0;
		this.multiplierBonusTotal = offlineCashEarned * 5.0;
		this.UpdateText();
	}

	// Token: 0x0600111D RID: 4381 RVA: 0x0004ECBC File Offset: 0x0004CEBC
	private void UpdateText()
	{
		if (this.offlineCashMultiplier == null)
		{
			return;
		}
		string text = this.offlineCashMultiplier.Cost.GetCurrentCostAsDouble().ToString();
		string text2 = string.Format("Would you like to increase your offline multiplier to <color=#8EB757FF>x{0}</color> for <color=#8EB757FF>{1} Gold?</color>", 5, text);
		this.ctaPrompt.text = text2;
		this.costText.text = text;
	}

	// Token: 0x0600111E RID: 4382 RVA: 0x0004ED15 File Offset: 0x0004CF15
	private void OnClose(Unit u)
	{
		this.AwardCash(this.adBonusTotal);
		this.CloseModal(u);
	}

	// Token: 0x0600111F RID: 4383 RVA: 0x0004ED2A File Offset: 0x0004CF2A
	private void OnOfflineCashMultiplierPurchased()
	{
		this.AwardCash(this.multiplierBonusTotal);
		this.CloseModal(Unit.Default);
	}

	// Token: 0x06001120 RID: 4384 RVA: 0x0004ED44 File Offset: 0x0004CF44
	private void AwardCash(double amount)
	{
		GameController.Instance.game.AddCash(amount, true);
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData(string.Format("You earned {0} dollars while you were offline!", NumberFormat.Convert(amount, 1E+15, true, 3)), null, delegate()
		{
			MessageBroker.Default.Publish<WelcomeBackSequenceCompleted>(default(WelcomeBackSequenceCompleted));
		}, PopupModal.PopupOptions.OK, "Great!", "No", true, null, "");
	}

	// Token: 0x06001121 RID: 4385 RVA: 0x0004EDCC File Offset: 0x0004CFCC
	public void PurchaseOfflineCashMultiplier()
	{
		if (this.offlineCashMultiplier != null)
		{
			if (GameController.Instance.StoreService.AttemptPurchase(this.offlineCashMultiplier.Id, Currency.Gold))
			{
				GameController.Instance.AnalyticService.SendNavActionAnalytics(this.offlineCashMultiplier.Id, GameController.Instance.game.planetName + "_WelcomeBackAdMultiplier", "buy");
				return;
			}
		}
		else
		{
			GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Offline Cash Multiplier Error", "Sorry an error occured while trying to purchase your offline cash multiplier. Please reload the game and try again.", null, "", PopupModal.PopupOptions.OK, "OK!", "No", true);
		}
	}

	// Token: 0x06001122 RID: 4386 RVA: 0x0004EE70 File Offset: 0x0004D070
	protected override void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		if (orientation.IsPortrait)
		{
			this.tform_TV.anchorMin = new Vector2(0.5f, 0.5f);
			this.tform_TV.anchorMax = new Vector2(0.5f, 0.5f);
			this.tform_TV.anchoredPosition = new Vector2(0f, 318f);
			this.tform_Contents.anchorMin = new Vector2(0.5f, 0.5f);
			this.tform_Contents.anchorMax = new Vector2(0.5f, 0.5f);
			this.tform_Contents.anchoredPosition = new Vector2(0f, -318f);
			return;
		}
		this.tform_TV.anchorMin = new Vector2(0.5f, 0.5f);
		this.tform_TV.anchorMax = new Vector2(0.5f, 0.5f);
		this.tform_TV.anchoredPosition = new Vector2(-318f, 0f);
		this.tform_Contents.anchorMin = new Vector2(0.5f, 0.5f);
		this.tform_Contents.anchorMax = new Vector2(0.5f, 0.5f);
		this.tform_Contents.anchoredPosition = new Vector2(318f, 0f);
	}

	// Token: 0x04000EC0 RID: 3776
	[SerializeField]
	private Text ctaPrompt;

	// Token: 0x04000EC1 RID: 3777
	[SerializeField]
	private Text costText;

	// Token: 0x04000EC2 RID: 3778
	[SerializeField]
	private Text regularPriceText;

	// Token: 0x04000EC3 RID: 3779
	[SerializeField]
	private RectTransform tform_TV;

	// Token: 0x04000EC4 RID: 3780
	[SerializeField]
	private RectTransform tform_Contents;

	// Token: 0x04000EC5 RID: 3781
	[SerializeField]
	private Button btn_Close;

	// Token: 0x04000EC6 RID: 3782
	[SerializeField]
	private Button btn_Purchase;

	// Token: 0x04000EC7 RID: 3783
	private const int MULTIPLIER_QTY = 5;

	// Token: 0x04000EC8 RID: 3784
	private const string OFFLINE_CASH_MULTIPLIER_ID = "offline_cash_multiplier";

	// Token: 0x04000EC9 RID: 3785
	private double multiplierBonusTotal;

	// Token: 0x04000ECA RID: 3786
	private double adBonusTotal;

	// Token: 0x04000ECB RID: 3787
	private AdCapStoreItem offlineCashMultiplier;
}
