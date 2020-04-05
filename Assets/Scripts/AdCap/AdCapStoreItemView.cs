using System;
using AdCap.Store;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000F0 RID: 240
public class AdCapStoreItemView : StoreItemViewBase
{
	// Token: 0x06000643 RID: 1603 RVA: 0x00021C80 File Offset: 0x0001FE80
	public override void WireData(AdCapStoreItem storeItem, Action<AdCapStoreItem> onClickAction)
	{
		base.WireData(storeItem, onClickAction);
		this.btn_Purchase.gameObject.SetActive(true);
		if (storeItem.Rewards.Count == 1)
		{
			Item itemById = GameController.Instance.GlobalPlayerData.inventory.GetItemById(storeItem.Rewards[0].Id);
			if (itemById != null)
			{
				this.img_Icon.sprite = Resources.Load<Sprite>(itemById.GetPathToIcon());
				this.txt_Name.text = (string.IsNullOrEmpty(storeItem.DisplayName) ? itemById.ItemName : storeItem.DisplayName);
				this.txt_Description.text = (string.IsNullOrEmpty(storeItem.Description) ? itemById.GetFilledItemDescription(0, AdCapColours.ColourMap[ColourNames.TextColourGreen].HexColor) : storeItem.Description);
				this.txt_Cost.text = storeItem.Cost.GetCurrentCost();
				this.img_CurrencyIcon.sprite = this.currencyIcons[storeItem.Cost.Currency];
				this.btn_Use.gameObject.SetActive(false);
				if (itemById.ItemType == ItemType.None)
				{
					itemById.Owned.Subscribe(new Action<int>(this.UpdateQuantity)).AddTo(this.disposables);
				}
				this.btn_Use.OnClickAsObservable().Subscribe(delegate(Unit _)
				{
					GameController.Instance.StoreService.AttemptPurchase(storeItem, Currency.Inventory);
				}).AddTo(this.disposables);
			}
		}
	}

	// Token: 0x06000644 RID: 1604 RVA: 0x00021E30 File Offset: 0x00020030
	private void UpdateQuantity(int qty)
	{
		if (null != this.txt_Quantity)
		{
			this.txt_Quantity.text = qty.ToString();
		}
		this.btn_Purchase.gameObject.SetActive(qty == 0);
		this.btn_Use.gameObject.SetActive(qty > 0);
	}

	// Token: 0x040005C4 RID: 1476
	[SerializeField]
	private Image img_Icon;

	// Token: 0x040005C5 RID: 1477
	[SerializeField]
	private Text txt_Name;

	// Token: 0x040005C6 RID: 1478
	[SerializeField]
	private Text txt_Description;

	// Token: 0x040005C7 RID: 1479
	[SerializeField]
	private Text txt_Cost;

	// Token: 0x040005C8 RID: 1480
	[SerializeField]
	private Image img_CurrencyIcon;

	// Token: 0x040005C9 RID: 1481
	[SerializeField]
	private Button btn_Use;

	// Token: 0x040005CA RID: 1482
	[SerializeField]
	private Text txt_Quantity;
}
