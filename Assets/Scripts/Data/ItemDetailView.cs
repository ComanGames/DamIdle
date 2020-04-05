using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000034 RID: 52
public class ItemDetailView : MonoBehaviour
{
	// Token: 0x060000F6 RID: 246 RVA: 0x000068C0 File Offset: 0x00004AC0
	private void Awake()
	{
		this.rarityStars.Add(this.starRarityImage);
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOreintationChanged)).AddTo(base.gameObject);
		this.btn_Equip.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnEquippedClicked();
		}).AddTo(base.gameObject);
		this.btn_Unequip.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnUnEquippedClicked();
		}).AddTo(base.gameObject);
	}

	// Token: 0x060000F7 RID: 247 RVA: 0x00006955 File Offset: 0x00004B55
	private void OnDestroy()
	{
		this.disposables.Dispose();
	}

	// Token: 0x060000F8 RID: 248 RVA: 0x00006962 File Offset: 0x00004B62
	private void OnEquippedClicked()
	{
		if (this.OnEquipCallback != null)
		{
			this.OnEquipCallback();
		}
	}

	// Token: 0x060000F9 RID: 249 RVA: 0x00006977 File Offset: 0x00004B77
	private void OnUnEquippedClicked()
	{
		if (this.OnUnequipCallback != null)
		{
			this.OnUnequipCallback();
		}
	}

	// Token: 0x060000FA RID: 250 RVA: 0x0000698C File Offset: 0x00004B8C
	private void OnOreintationChanged(OrientationChangedEvent orientation)
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		rectTransform.sizeDelta = (orientation.IsPortrait ? new Vector2(rectTransform.sizeDelta.x, 610f) : new Vector2(rectTransform.sizeDelta.x, 545f));
	}

	// Token: 0x060000FB RID: 251 RVA: 0x000069E0 File Offset: 0x00004BE0
	public void Setup(Item item, int addedLevels, Action onEquipCallback, Action onUneqipCallback, bool showButtons = true)
	{
		this.Item = item;
		this.OnEquipCallback = onEquipCallback;
		this.OnUnequipCallback = onUneqipCallback;
		base.gameObject.SetActive(true);
		string text = item.ItemName;
		string filledItemDescription = item.GetFilledItemDescription(addedLevels, "");
		if (item.ItemId == "gold")
		{
			text = string.Format("{0} Gold", addedLevels);
		}
		this.itemDescriptionText.text = filledItemDescription;
		this.itemNameText.text = text;
		this.itemRarityText.text = item.GetRarityName();
		Color colourForRarity = GameController.Instance.GlobalPlayerData.inventory.GetColourForRarity(item.RarityRank);
		this.itemRarityText.color = colourForRarity;
		if (this.itemIconView == null)
		{
			this.itemIconView = Object.Instantiate<ItemIconView>(this.itemIconViewPrefab);
			this.itemIconView.transform.SetParent(this.itemIconViewParent, false);
		}
		this.SetRarityStars();
		this.SetEquippedState(showButtons && item.IsEquipped);
		this.itemIconView.Setup(item, null, null, false, false, false, false, false, false, false, false);
		this.btn_Equip.gameObject.SetActive(showButtons);
		if (item.ItemType == ItemType.Trophy)
		{
			this.btn_Equip.gameObject.SetActive(false);
			this.btn_Unequip.gameObject.SetActive(false);
		}
	}

	// Token: 0x060000FC RID: 252 RVA: 0x00006B38 File Offset: 0x00004D38
	public void SetEquippedState(bool isEquipped)
	{
		this.btn_Unequip.gameObject.SetActive(isEquipped);
		this.equipButtonText.text = (isEquipped ? "Swap" : "Equip");
	}

	// Token: 0x060000FD RID: 253 RVA: 0x00006B68 File Offset: 0x00004D68
	private void SetRarityStars()
	{
		int i;
		for (i = 0; i < this.Item.RarityRank; i++)
		{
			if (i < this.rarityStars.Count)
			{
				this.rarityStars[i].gameObject.SetActive(true);
				this.rarityStars[i].color = GameController.Instance.GlobalPlayerData.inventory.GetColourForRarity(this.Item.RarityRank);
			}
			else
			{
				Image item = Object.Instantiate<Image>(this.starRarityImage, this.starRarityImage.transform.parent, false);
				this.rarityStars.Add(item);
			}
		}
		while (i < this.rarityStars.Count)
		{
			this.rarityStars[i].gameObject.SetActive(false);
			i++;
		}
	}

	// Token: 0x0400011F RID: 287
	[SerializeField]
	private ItemIconView itemIconViewPrefab;

	// Token: 0x04000120 RID: 288
	[SerializeField]
	private Transform itemIconViewParent;

	// Token: 0x04000121 RID: 289
	[SerializeField]
	private Text itemNameText;

	// Token: 0x04000122 RID: 290
	[SerializeField]
	private Text itemRarityText;

	// Token: 0x04000123 RID: 291
	[SerializeField]
	private Image starRarityImage;

	// Token: 0x04000124 RID: 292
	[SerializeField]
	private Text itemDescriptionText;

	// Token: 0x04000125 RID: 293
	[SerializeField]
	private Text equipButtonText;

	// Token: 0x04000126 RID: 294
	[HideInInspector]
	public Item Item;

	// Token: 0x04000127 RID: 295
	[SerializeField]
	private Button btn_Equip;

	// Token: 0x04000128 RID: 296
	[SerializeField]
	private Button btn_Unequip;

	// Token: 0x04000129 RID: 297
	private ItemIconView itemIconView;

	// Token: 0x0400012A RID: 298
	private List<Image> rarityStars = new List<Image>();

	// Token: 0x0400012B RID: 299
	private Action OnEquipCallback;

	// Token: 0x0400012C RID: 300
	private Action OnUnequipCallback;

	// Token: 0x0400012D RID: 301
	private CompositeDisposable disposables = new CompositeDisposable();
}
