using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200020C RID: 524
public class PlanetMissionPanel : PanelBaseClass
{
	// Token: 0x06000F42 RID: 3906 RVA: 0x00046B84 File Offset: 0x00044D84
	private void Start()
	{
		this.btn_Launch.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnClickLaunch)).AddTo(base.gameObject);
		this.btn_Purchase.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnPurchase)).AddTo(base.gameObject);
	}

	// Token: 0x06000F43 RID: 3907 RVA: 0x00046BE4 File Offset: 0x00044DE4
	public void WireData(PlanetData planetData)
	{
		this.planetData = planetData;
		base.name = this.planetData.PlanetName;
		this.TintAssets(this.planetData.ColorTint);
		string text = this.planetData.PlanetName;
		if (text.ToLower() == "moon")
		{
			text = "The Moon";
		}
		this.txt_Banner.text = string.Format("{0}", text);
		this.img_Mission.sprite = this.planetData.MissionImage;
		string id = this.planetData.PlanetName.ToLower() + "_unlock";
		this.inventoryItem = GameController.Instance.GlobalPlayerData.inventory.GetItemById(id);
		if (this.inventoryItem != null)
		{
			(from x in this.inventoryItem.Owned
			select x > 0).Subscribe(new Action<bool>(this.UpdateButtons)).AddTo(base.gameObject);
		}
		this.storeItem = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault((AdCapStoreItem x) => x.Id == this.planetData.PurchaseId);
		if (this.storeItem != null)
		{
			this.SetPriceText();
			return;
		}
		this.btn_Purchase.gameObject.SetActive(false);
		if (null != this.purchase_tag)
		{
			this.purchase_tag.SetActive(false);
		}
		GameController.Instance.StoreService.CurrentCatalog.ObserveAdd().First((CollectionAddEvent<AdCapStoreItem> x) => x.Value.Id == this.planetData.PurchaseId).Subscribe(new Action<CollectionAddEvent<AdCapStoreItem>>(this.OnPlanetUnlockItemAdded)).AddTo(base.gameObject);
	}

	// Token: 0x06000F44 RID: 3908 RVA: 0x00046D96 File Offset: 0x00044F96
	private void OnPlanetUnlockItemAdded(CollectionAddEvent<AdCapStoreItem> angelclaimitemaddedevent)
	{
		this.storeItem = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault((AdCapStoreItem x) => x.Id == this.planetData.PurchaseId);
		this.SetPriceText();
	}

	// Token: 0x06000F45 RID: 3909 RVA: 0x00046DC4 File Offset: 0x00044FC4
	private void UpdateButtons(bool isUnlocked)
	{
		this.btn_Launch.gameObject.SetActive(isUnlocked);
		this.btn_Purchase.gameObject.SetActive(!isUnlocked && this.storeItem != null);
		if (null != this.purchase_tag)
		{
			this.purchase_tag.SetActive(!isUnlocked && this.storeItem != null);
		}
	}

	// Token: 0x06000F46 RID: 3910 RVA: 0x00046E2C File Offset: 0x0004502C
	private void TintAssets(Color color)
	{
		this.tintableImages.ForEach(delegate(Image x)
		{
			x.color = color;
		});
	}

	// Token: 0x06000F47 RID: 3911 RVA: 0x00046E60 File Offset: 0x00045060
	private void SetPriceText()
	{
		this.txt_Price.text = string.Format("{0}{1}", (this.storeItem.Cost.Currency == Currency.InGameCash) ? "$" : "", NumberFormat.Convert(this.storeItem.Cost.Price, 999999.0, false, 3));
		this.img_Currency.sprite = this.currencyIcons[this.storeItem.Cost.Currency];
		this.UpdateButtons(this.inventoryItem.Owned.Value > 0);
	}

	// Token: 0x06000F48 RID: 3912 RVA: 0x00046F00 File Offset: 0x00045100
	private void OnClickLaunch(Unit u)
	{
		GameController.Instance.LoadPlanetScene(base.name, false);
	}

	// Token: 0x06000F49 RID: 3913 RVA: 0x00046F13 File Offset: 0x00045113
	private void OnPurchase(Unit u)
	{
		GameController.Instance.StoreService.AttemptPurchase(this.storeItem, this.storeItem.Cost.Currency);
	}

	// Token: 0x04000D2B RID: 3371
	[SerializeField]
	private Text txt_Banner;

	// Token: 0x04000D2C RID: 3372
	[SerializeField]
	private CurrencySpriteDictionary currencyIcons;

	// Token: 0x04000D2D RID: 3373
	[SerializeField]
	private Image img_Mission;

	// Token: 0x04000D2E RID: 3374
	[SerializeField]
	private Button btn_Purchase;

	// Token: 0x04000D2F RID: 3375
	[SerializeField]
	private GameObject purchase_tag;

	// Token: 0x04000D30 RID: 3376
	[SerializeField]
	private Button btn_Launch;

	// Token: 0x04000D31 RID: 3377
	[SerializeField]
	private List<Image> tintableImages = new List<Image>();

	// Token: 0x04000D32 RID: 3378
	[SerializeField]
	private Text txt_Price;

	// Token: 0x04000D33 RID: 3379
	[SerializeField]
	private Image img_Currency;

	// Token: 0x04000D34 RID: 3380
	private PlanetData planetData;

	// Token: 0x04000D35 RID: 3381
	private AdCapStoreItem storeItem;

	// Token: 0x04000D36 RID: 3382
	private Item inventoryItem;
}
