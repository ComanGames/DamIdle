using System;
using System.Linq;
using AdCap.Store;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000209 RID: 521
public class MegaTicketsPanel : PanelBaseClass
{
	// Token: 0x06000F2B RID: 3883 RVA: 0x00046294 File Offset: 0x00044494
	private void Start()
	{
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
		GameController.Instance.IsInitialized.First(x => x).Subscribe(delegate(bool state)
		{
			this.Init();
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000F2C RID: 3884 RVA: 0x00046313 File Offset: 0x00044513
	private void Init()
	{
		this.storeService = GameController.Instance.StoreService;
		this.Wirebuttons();
	}

	// Token: 0x06000F2D RID: 3885 RVA: 0x0004632C File Offset: 0x0004452C
	private void Wirebuttons()
	{
		this.WireButon(this.btn_OneMegaTicketPurchaseGold, this.txt_OneMegaTicketPurchaseGold_price, "mega_ticket_1_gold");
		this.WireButon(this.btn_OneMegaTicketPurchaseMegabucks, this.txt_OneMegaTicketPurchaseMegabucks_price, "mega_ticket_1");
		this.WireButon(this.btn_TenMegaTicketPurchaseGold, this.txt_TenMegaTicketPurchaseGold_price, "mega_ticket_10_gold");
		this.WireButon(this.btn_TenMegaTicketPurchaseMegabucks, this.txt_TenMegaTicketPurchaseMegabucks_price, "mega_ticket_10");
	}

	// Token: 0x06000F2E RID: 3886 RVA: 0x00046398 File Offset: 0x00044598
	private void WireButon(Button btn, Text price_label, string itemId)
	{
		AdCapStoreItem storeItem = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault(x => x.Id == itemId);
		if (storeItem != null)
		{
			this.WirePurchaseButton(btn, price_label, itemId, storeItem);
			return;
		}
		(from x in GameController.Instance.StoreService.CurrentCatalog.ObserveAdd().First(x => x.Value.Id == itemId)
		select x.Value).Subscribe(delegate(AdCapStoreItem x)
		{
			this.WirePurchaseButton(btn, price_label, itemId, storeItem);
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000F2F RID: 3887 RVA: 0x0004647C File Offset: 0x0004467C
	private void WirePurchaseButton(Button btn, Text price_label, string itemId, AdCapStoreItem item)
	{
		price_label.text = item.Cost.GetCurrentCost();
		btn.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnPurchaseItem(itemId, item.Cost.Currency);
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000F30 RID: 3888 RVA: 0x000464DE File Offset: 0x000446DE
	public override void OnShowPanel()
	{
		FTUE_Manager.ShowFTUE("MegaBoostsPanel", null);
		GameController.Instance.AnalyticService.SendNavActionAnalytics("MegaTicketsPanel", GameController.Instance.game.planetName + "_MegaTickets", "Bundle");
	}

	// Token: 0x06000F31 RID: 3889 RVA: 0x00046520 File Offset: 0x00044720
	private void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		if (orientation.IsPortrait)
		{
			this.tform_TopPanel.sizeDelta = new Vector2(this.tform_TopPanel.sizeDelta.x, 256f);
			this.tform_TopPanelTextContent.anchorMin = new Vector2(0.5f, 1f);
			this.tform_TopPanelTextContent.anchorMax = new Vector2(0.5f, 1f);
			this.tform_TopPanelTextContent.pivot = new Vector2(0.5f, 1f);
			this.tform_TopPanelTextContent.anchoredPosition = new Vector2(0f, 0f);
			this.tform_TopPanelGildButton.anchorMin = new Vector2(0.5f, 0f);
			this.tform_TopPanelGildButton.anchorMax = new Vector2(0.5f, 0f);
			this.tform_TopPanelGildButton.pivot = new Vector2(0.5f, 0f);
			this.tform_TopPanelGildButton.anchoredPosition = new Vector2(0f, 0f);
			this.tform_BottomPanel.anchoredPosition = new Vector2(this.tform_BottomPanel.anchoredPosition.x, -150f);
			this.tform_BottomPanel.sizeDelta = new Vector2(this.tform_BottomPanel.offsetMin.x, 600f);
			this.tform_OneMegaTicketItem.anchorMin = new Vector2(0.5f, 1f);
			this.tform_OneMegaTicketItem.anchorMax = new Vector2(0.5f, 1f);
			this.tform_OneMegaTicketItem.pivot = new Vector2(0.5f, 1f);
			this.tform_OneMegaTicketItem.anchoredPosition = new Vector2(0f, 0f);
			this.tform_TenMegaTicketItem.anchorMin = new Vector2(0.5f, 0f);
			this.tform_TenMegaTicketItem.anchorMax = new Vector2(0.5f, 0f);
			this.tform_TenMegaTicketItem.pivot = new Vector2(0.5f, 0f);
			this.tform_TenMegaTicketItem.anchoredPosition = new Vector2(0f, 0f);
			return;
		}
		this.tform_TopPanel.sizeDelta = new Vector2(this.tform_TopPanel.sizeDelta.x, 128f);
		this.tform_TopPanelTextContent.anchorMin = new Vector2(0f, 0.5f);
		this.tform_TopPanelTextContent.anchorMax = new Vector2(0f, 0.5f);
		this.tform_TopPanelTextContent.pivot = new Vector2(0f, 0.5f);
		this.tform_TopPanelTextContent.anchoredPosition = new Vector2(20f, 0f);
		this.tform_TopPanelGildButton.anchorMin = new Vector2(1f, 0.5f);
		this.tform_TopPanelGildButton.anchorMax = new Vector2(1f, 0.5f);
		this.tform_TopPanelGildButton.pivot = new Vector2(1f, 0.5f);
		this.tform_TopPanelGildButton.anchoredPosition = new Vector2(-20f, 0f);
		this.tform_BottomPanel.anchoredPosition = new Vector2(this.tform_BottomPanel.anchoredPosition.x, -100f);
		this.tform_BottomPanel.sizeDelta = new Vector2(this.tform_BottomPanel.offsetMin.x, 300f);
		this.tform_OneMegaTicketItem.anchorMin = new Vector2(0.5f, 0.5f);
		this.tform_OneMegaTicketItem.anchorMax = new Vector2(0.5f, 0.5f);
		this.tform_OneMegaTicketItem.pivot = new Vector2(0.5f, 0.5f);
		this.tform_OneMegaTicketItem.anchoredPosition = new Vector2(-260f, 0f);
		this.tform_TenMegaTicketItem.anchorMin = new Vector2(0.5f, 0.5f);
		this.tform_TenMegaTicketItem.anchorMax = new Vector2(0.5f, 0.5f);
		this.tform_TenMegaTicketItem.pivot = new Vector2(0.5f, 0.5f);
		this.tform_TenMegaTicketItem.anchoredPosition = new Vector2(260f, 0f);
	}

	// Token: 0x06000F32 RID: 3890 RVA: 0x00046958 File Offset: 0x00044B58
	private void OnPurchaseItem(string itemId, Currency currency)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics(itemId + "_" + currency, GameController.Instance.game.planetName + "_MegaTicketsPanel", "buy");
		this.storeService.AttemptPurchase(itemId, currency);
	}

	// Token: 0x04000D11 RID: 3345
	private const string ONE_MEGATICKET_GOLD = "mega_ticket_1_gold";

	// Token: 0x04000D12 RID: 3346
	private const string ONE_MEGATICKET_MEGABUCKS = "mega_ticket_1";

	// Token: 0x04000D13 RID: 3347
	private const string TEN_MEGATICKET_GOLD = "mega_ticket_10_gold";

	// Token: 0x04000D14 RID: 3348
	private const string TEN_MEGATICKET_MEGABUCKS = "mega_ticket_10";

	// Token: 0x04000D15 RID: 3349
	[SerializeField]
	private RectTransform tform_TopPanel;

	// Token: 0x04000D16 RID: 3350
	[SerializeField]
	private RectTransform tform_TopPanelTextContent;

	// Token: 0x04000D17 RID: 3351
	[SerializeField]
	private RectTransform tform_TopPanelGildButton;

	// Token: 0x04000D18 RID: 3352
	[SerializeField]
	private RectTransform tform_BottomPanel;

	// Token: 0x04000D19 RID: 3353
	[SerializeField]
	private RectTransform tform_OneMegaTicketItem;

	// Token: 0x04000D1A RID: 3354
	[SerializeField]
	private RectTransform tform_TenMegaTicketItem;

	// Token: 0x04000D1B RID: 3355
	[SerializeField]
	private Button btn_OneMegaTicketPurchaseGold;

	// Token: 0x04000D1C RID: 3356
	[SerializeField]
	private Text txt_OneMegaTicketPurchaseGold_price;

	// Token: 0x04000D1D RID: 3357
	[SerializeField]
	private Button btn_OneMegaTicketPurchaseMegabucks;

	// Token: 0x04000D1E RID: 3358
	[SerializeField]
	private Text txt_OneMegaTicketPurchaseMegabucks_price;

	// Token: 0x04000D1F RID: 3359
	[SerializeField]
	private Button btn_TenMegaTicketPurchaseGold;

	// Token: 0x04000D20 RID: 3360
	[SerializeField]
	private Text txt_TenMegaTicketPurchaseGold_price;

	// Token: 0x04000D21 RID: 3361
	[SerializeField]
	private Button btn_TenMegaTicketPurchaseMegabucks;

	// Token: 0x04000D22 RID: 3362
	[SerializeField]
	private Text txt_TenMegaTicketPurchaseMegabucks_price;

	// Token: 0x04000D23 RID: 3363
	private IStoreService storeService;
}
