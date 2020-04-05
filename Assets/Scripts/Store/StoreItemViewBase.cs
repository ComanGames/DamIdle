using System;
using AdCap.Store;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000257 RID: 599
public abstract class StoreItemViewBase : MonoBehaviour, IDisposable
{
	// Token: 0x17000163 RID: 355
	// (get) Token: 0x060010C3 RID: 4291 RVA: 0x0004D006 File Offset: 0x0004B206
	// (set) Token: 0x060010C4 RID: 4292 RVA: 0x0004D00E File Offset: 0x0004B20E
	public AdCapStoreItem Item { get; private set; }

	// Token: 0x060010C5 RID: 4293 RVA: 0x0004D018 File Offset: 0x0004B218
	public virtual void WireData(AdCapStoreItem item, Action<AdCapStoreItem> onClickAction)
	{
		this.disposables.Clear();
		this.btn_Purchase.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			onClickAction(item);
		}).AddTo(this.disposables);
		if (null != this.btn_Details)
		{
			this.btn_Details.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowItemDetails)).AddTo(this.disposables);
		}
		this.Item = item;
		if (null != this.txt_TimeRemaining)
		{
			this.txt_TimeRemaining.text = string.Empty;
			if (item.Duration > 0.0 || item.EndDateUtc != DateTime.MinValue)
			{
				item.TimeRemaining.DistinctUntilChanged<double>().Subscribe(delegate(double x)
				{
					if (this.txt_TimeRemaining != null)
					{
						this.txt_TimeRemaining.text = ((x > 0.0) ? x.ToCountdown("Only {0} Remaining!") : "Last Chance!");
					}
				}).AddTo(this.disposables);
			}
		}
		if (null != this.go_Banner && null != this.txt_BannerText)
		{
			this.txt_BannerText.text = item.BannerText;
			this.go_Banner.SetActive(!string.IsNullOrEmpty(item.BannerText));
		}
		if (null != this.txt_RealPrice)
		{
			this.txt_RealPrice.text = (item.Cost.IsSaleActive ? string.Format("Was {0}", item.Cost.LocalizedPriceString) : string.Empty);
		}
		if (item.Cost.LocalizedPriceString == "Loading")
		{
			this.btn_Purchase.interactable = false;
		}
	}

	// Token: 0x060010C6 RID: 4294 RVA: 0x0004D1F1 File Offset: 0x0004B3F1
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x060010C7 RID: 4295 RVA: 0x0004D1FE File Offset: 0x0004B3FE
	private void ShowItemDetails(Unit u)
	{
		GameController.Instance.NavigationService.CreateModal<ItemDetailModal>(NavModals.ITEM_DETAIL, false).ShowItem(this.Item);
	}

	// Token: 0x060010C8 RID: 4296 RVA: 0x0004D1F1 File Offset: 0x0004B3F1
	private void OnDestroy()
	{
		this.disposables.Dispose();
	}

	// Token: 0x060010C9 RID: 4297 RVA: 0x0004D220 File Offset: 0x0004B420
	protected void CreateBundleItemView(string itemName, Item item, bool isEquipable, Color c, int qty)
	{
		BundleItemView bundleItemView = Object.Instantiate<BundleItemView>(this.bundleItemViewPrefab, this.bundleItemPrefabParent, false);
		if (null != bundleItemView)
		{
			bundleItemView.WireData(itemName, item, isEquipable, c, qty);
		}
	}

	// Token: 0x04000E5C RID: 3676
	[SerializeField]
	protected CurrencySpriteDictionary currencyIcons;

	// Token: 0x04000E5D RID: 3677
	[SerializeField]
	protected Button btn_Purchase;

	// Token: 0x04000E5E RID: 3678
	[SerializeField]
	protected Button btn_Details;

	// Token: 0x04000E5F RID: 3679
	[SerializeField]
	protected Text txt_TimeRemaining;

	// Token: 0x04000E60 RID: 3680
	[SerializeField]
	protected BundleItemView bundleItemViewPrefab;

	// Token: 0x04000E61 RID: 3681
	[SerializeField]
	protected Transform bundleItemPrefabParent;

	// Token: 0x04000E62 RID: 3682
	[SerializeField]
	protected GameObject go_Banner;

	// Token: 0x04000E63 RID: 3683
	[SerializeField]
	protected Text txt_BannerText;

	// Token: 0x04000E64 RID: 3684
	[SerializeField]
	protected Text txt_RealPrice;

	// Token: 0x04000E65 RID: 3685
	protected CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000E66 RID: 3686
	private const string TIME_REMAINING = "Only {0} Remaining!";
}
