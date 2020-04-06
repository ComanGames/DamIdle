using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000035 RID: 53
public class ItemIconView : BaseItemIconView
{
	// Token: 0x06000101 RID: 257 RVA: 0x00006C6A File Offset: 0x00004E6A
	private void Awake()
	{
		((RectTransform)base.transform).SetAndStretchToParentSize((RectTransform)base.transform.parent);
	}

	// Token: 0x06000102 RID: 258 RVA: 0x00006C8C File Offset: 0x00004E8C
	private void OnDestroy()
	{
		this.OnItemIconClicked = null;
		this.EquipCallback = null;
		this.disposables.Dispose();
	}

	// Token: 0x06000103 RID: 259 RVA: 0x00006CA8 File Offset: 0x00004EA8
	private void SetItemLevel(bool showProgress)
	{
		if (!this.Item.IsEquipable())
		{
			this.itemLevelText.text = this.Item.Owned.Value.ToString();
			this.itemLevelUpBar.SetActive(false);
			return;
		}
		if (showProgress)
		{
			this.itemLevelBar.fillAmount = (float)this.Item.PreviousOwned / (float)this.Item.MaxLevel;
			this.itemLevelText.text = this.Item.PreviousOwned + "/" + this.Item.MaxLevel;
		}
		else
		{
			this.itemLevelBar.fillAmount = (float)this.Item.Owned.Value / (float)this.Item.MaxLevel;
			this.itemLevelText.text = this.Item.Owned.Value + "/" + this.Item.MaxLevel;
		}
		this.itemLevelUpBar.SetActive(true);
	}

	// Token: 0x06000104 RID: 260 RVA: 0x00006DC0 File Offset: 0x00004FC0
	public void ShowProgress()
	{
		float duration = 1f;
		this.itemLevelBar.fillAmount = (float)this.Item.PreviousOwned / (float)this.Item.MaxLevel;
		this.progressNumberTweener = this.itemLevelBar.DOFillAmount((float)this.Item.Owned.Value / (float)this.Item.MaxLevel, duration);
		this.progressTextTweener = DOTween.To(() => this.Item.PreviousOwned, delegate(int x)
		{
			this.itemLevelText.text = string.Format("{0}/{1}", x, this.Item.MaxLevel);
		}, this.Item.Owned.Value, duration);
	}

	// Token: 0x06000105 RID: 261 RVA: 0x00006E5C File Offset: 0x0000505C
	public void Setup(Item item, Action<Item> callback, Action<Item> equipCallback, bool levelBar = false, bool equip = false, bool replace = false, bool isNewItem = false, bool showCheckmark = false, bool showLevelProgress = false, bool showName = true, bool showQuantity = true)
	{
		base.Setup(item, showName);
		if (this.progressTextTweener != null)
		{
			this.progressTextTweener.Kill(false);
		}
		if (this.progressNumberTweener != null)
		{
			this.progressNumberTweener.Kill(false);
		}
		this.disposables.Clear();
		this.itemLevelBar.fillAmount = 0f;
		this.OnItemIconClicked = callback;
		this.itemButton.image.raycastTarget = (callback != null);
		if (this.OnItemIconClicked != null)
		{
			this.itemButton.OnClickAsObservable().DebugLog("On Click!").Subscribe(delegate(Unit _)
			{
				this.OnItemIconClicked(item);
			}).AddTo(this.disposables);
		}
		if (equipCallback != null)
		{
			this.EquipCallback = equipCallback;
			this.btn_Equip.OnClickAsObservable().Subscribe(delegate(Unit _)
			{
				this.EquipCallback(item);
			}).AddTo(this.disposables);
			this.btn_Replace.OnClickAsObservable().Subscribe(delegate(Unit _)
			{
				this.EquipCallback(item);
			}).AddTo(this.disposables);
		}
		this.itemBackgroundRarityImage.sprite = item.GetBGIcon();
		this.btn_Equip.gameObject.SetActive(false);
		this.newTag.SetActive(isNewItem);
		this.equippedCheckmark.SetActive(false);
		this.btn_Replace.gameObject.SetActive(false);
		this.equipableObjects.ForEach(delegate(GameObject x)
		{
			x.SetActive(this.Item.IsEquipable());
		});
		this.unequipableObjects.ForEach(delegate(GameObject x)
		{
			x.SetActive(!this.Item.IsEquipable());
		});
		this.qtyBadge.SetActive(showQuantity);
		this.SetItemLevel(showLevelProgress);
		if (!showLevelProgress)
		{
			this.Item.Owned.Subscribe(new Action<int>(this.UpdateQuantity)).AddTo(base.gameObject);
		}
		if (null != this.txt_ItemQty)
		{
			this.txt_ItemQty.text = string.Format("x{0}", this.Item.Owned.Value);
		}
		this.SetExtraInfo(levelBar, equip, replace, isNewItem, showCheckmark);
	}

	// Token: 0x06000106 RID: 262 RVA: 0x00007088 File Offset: 0x00005288
	public void SetExtraInfo(bool levelBar = false, bool equip = false, bool replace = false, bool isNewItem = false, bool showCheckmark = false)
	{
		this.equippedCheckmark.SetActive(showCheckmark);
		this.newTag.SetActive(isNewItem);
		this.btn_Equip.gameObject.SetActive(equip && !this.Item.IsEquipped);
		this.btn_Replace.gameObject.SetActive(replace);
		this.itemLevelBaseObject.SetActive(levelBar);
	}

	// Token: 0x06000107 RID: 263 RVA: 0x000070F0 File Offset: 0x000052F0
	private void UpdateQuantity(int quantity)
	{
		this.SetItemLevel(false);
	}

	// Token: 0x0400012E RID: 302
	[SerializeField]
	private GameObject equippedCheckmark;

	// Token: 0x0400012F RID: 303
	[SerializeField]
	private GameObject newTag;

	// Token: 0x04000130 RID: 304
	[SerializeField]
	private Image itemLevelBar;

	// Token: 0x04000131 RID: 305
	[SerializeField]
	private Image itemBackgroundRarityImage;

	// Token: 0x04000132 RID: 306
	[SerializeField]
	private GameObject itemLevelUpBar;

	// Token: 0x04000133 RID: 307
	[SerializeField]
	private Text itemLevelText;

	// Token: 0x04000134 RID: 308
	[SerializeField]
	private Text txt_ItemQty;

	// Token: 0x04000135 RID: 309
	[SerializeField]
	private GameObject itemLevelBaseObject;

	// Token: 0x04000136 RID: 310
	[SerializeField]
	private Button itemButton;

	// Token: 0x04000137 RID: 311
	[SerializeField]
	private GameObject qtyBadge;

	// Token: 0x04000138 RID: 312
	[SerializeField]
	private List<GameObject> equipableObjects = new List<GameObject>();

	// Token: 0x04000139 RID: 313
	[SerializeField]
	private List<GameObject> unequipableObjects = new List<GameObject>();

	// Token: 0x0400013A RID: 314
	[SerializeField]
	private Button btn_Equip;

	// Token: 0x0400013B RID: 315
	[SerializeField]
	private Button btn_Replace;

	// Token: 0x0400013C RID: 316
	private Action<Item> OnItemIconClicked;

	// Token: 0x0400013D RID: 317
	private Action<Item> EquipCallback;

	// Token: 0x0400013E RID: 318
	private Tweener progressNumberTweener;

	// Token: 0x0400013F RID: 319
	private Tweener progressTextTweener;
}
