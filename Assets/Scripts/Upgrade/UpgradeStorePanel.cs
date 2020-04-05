using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using ListContentPools;
using UniRx;
using UnityEngine;

// Token: 0x0200011A RID: 282
public class UpgradeStorePanel : PanelBaseClass
{
	// Token: 0x06000784 RID: 1924 RVA: 0x000280D4 File Offset: 0x000262D4
	private void Start()
	{
		OrientationController.Instance.OrientationStream.DelayFrame(1, FrameCountType.Update).Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
	}

	// Token: 0x06000785 RID: 1925 RVA: 0x00028104 File Offset: 0x00026304
	public override void Init(ModalController modalController)
	{
		this.storeItemIdToAdCapStoreItem = new Dictionary<string, AdCapStoreItem>();
		IStoreService storeService = GameController.Instance.StoreService;
		for (int i = 0; i < storeService.CurrentCatalog.Count; i++)
		{
			this.OnItemAdded(storeService.CurrentCatalog[i]);
		}
		(from add in storeService.CurrentCatalog.ObserveAdd()
		select add.Value).Subscribe(new Action<AdCapStoreItem>(this.OnItemAdded)).AddTo(base.gameObject);
		(from remove in storeService.CurrentCatalog.ObserveRemove()
		select remove.Value).Subscribe(new Action<AdCapStoreItem>(this.OnItemRemoved)).AddTo(base.gameObject);
		this.availableStoreItems.ObserveCountChanged(false).ThrottleFrame(1, FrameCountType.Update).Subscribe(delegate(int _)
		{
			this.RebuildList();
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000786 RID: 1926 RVA: 0x00028218 File Offset: 0x00026418
	private void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		this.numElementsHorizontal = (orientation.IsPortrait ? 0 : 2);
		this.customWidth = -1f;
		if (orientation.AspectRatio == new Vector2(3f, 4f))
		{
			this.customWidth = ((RectTransform)base.transform).rect.width * 0.8f;
		}
		this.RebuildList();
	}

	// Token: 0x06000787 RID: 1927 RVA: 0x00028288 File Offset: 0x00026488
	private void OnItemAdded(AdCapStoreItem upgrade)
	{
		if (upgrade.StoreId == "Boost" || upgrade.StoreId == "Items")
		{
			this.storeItemIdToAdCapStoreItem.Add(upgrade.Id, upgrade);
			this.availableStoreItems.Add(upgrade);
		}
	}

	// Token: 0x06000788 RID: 1928 RVA: 0x000282D7 File Offset: 0x000264D7
	private void OnItemRemoved(AdCapStoreItem upgrade)
	{
		this.storeItemIdToAdCapStoreItem.Remove(upgrade.Id);
		this.availableStoreItems.Remove(upgrade);
	}

	// Token: 0x06000789 RID: 1929 RVA: 0x000282F8 File Offset: 0x000264F8
	private void OnRowAdded(CollectionAddEvent<ReactiveGridRow<AdCapStoreItem>> addEvent)
	{
		this.verticalContentPool.AddChild(addEvent.Value.Id, "UpgradeStoreGridRowView", this.rebuildOnRowAdded);
	}

	// Token: 0x0600078A RID: 1930 RVA: 0x0002831C File Offset: 0x0002651C
	private void OnRowRemoved(CollectionRemoveEvent<ReactiveGridRow<AdCapStoreItem>> removeEvent)
	{
		this.verticalContentPool.RemoveChild(removeEvent.Value.Id, true);
	}

	// Token: 0x0600078B RID: 1931 RVA: 0x00026FEF File Offset: 0x000251EF
	private void OnCreateRow(UpgradeStoreGridRowView upgradeRowView)
	{
		upgradeRowView.gameObject.SetActive(true);
	}

	// Token: 0x0600078C RID: 1932 RVA: 0x00028338 File Offset: 0x00026538
	private void OnInitializeRow(string rowId, UpgradeStoreGridRowView upgradeRowView)
	{
		ReactiveGridRow<AdCapStoreItem> rowById = this.itemsReactiveGrid.GetRowById(rowId);
		upgradeRowView.WireData(rowById, new Action<AdCapStoreItem>(this.OnItemClicked));
	}

	// Token: 0x0600078D RID: 1933 RVA: 0x00028365 File Offset: 0x00026565
	private void OnItemClicked(AdCapStoreItem item)
	{
		GameController.Instance.StoreService.AttemptPurchase(item, item.Cost.Currency);
		this.SendStoreAnalytics(item.Id, "buy");
	}

	// Token: 0x0600078E RID: 1934 RVA: 0x00028394 File Offset: 0x00026594
	public void SendStoreAnalytics(string bundleId, string action)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics(bundleId, GameController.Instance.game.planetName + "_UpgradeStore", action);
	}

	// Token: 0x0600078F RID: 1935 RVA: 0x000283C0 File Offset: 0x000265C0
	private void RebuildList()
	{
		this.verticalContentPool.PaddingTop = 100f;
		this.rebuildOnRowAdded = false;
		this.disposables.Clear();
		this.verticalContentPool.ClearAndReset();
		if (this.rowPrefab != null)
		{
			Object.Destroy(this.rowPrefab.gameObject);
		}
		if (this.itemsReactiveGrid != null)
		{
			this.itemsReactiveGrid.Clear();
		}
		GameObject gameObject = new GameObject();
		this.rowPrefab = gameObject.AddComponent<UpgradeStoreGridRowView>();
		this.rowPrefab.Init(this.itemPrefab, this.tform_Content, this.contentPoolSpacing, this.numElementsHorizontal, -1f, this.customWidth);
		this.rowPrefab.name = "UpgradeStoreGridRowView_PREFAB";
		this.rowPrefab.gameObject.SetActive(false);
		this.verticalContentPool.RegisterChildType<UpgradeStoreGridRowView>("UpgradeStoreGridRowView", this.rowPrefab, new ElementSizing(HeightSizingType.Static, 0f), new Action<string, UpgradeStoreGridRowView>(this.OnInitializeRow), new Action<UpgradeStoreGridRowView>(this.OnCreateRow), null);
		this.itemsReactiveGrid = new ReactiveGrid<AdCapStoreItem>(this.rowPrefab.NumElements, "UpgradeStoreGridRowView");
		this.itemsReactiveGrid.Rows.ObserveAdd().Subscribe(new Action<CollectionAddEvent<ReactiveGridRow<AdCapStoreItem>>>(this.OnRowAdded)).AddTo(this.disposables);
		this.itemsReactiveGrid.Rows.ObserveRemove().Subscribe(new Action<CollectionRemoveEvent<ReactiveGridRow<AdCapStoreItem>>>(this.OnRowRemoved)).AddTo(this.disposables);
		(from x in this.availableStoreItems
		orderby x.DisplayPriority
		select x).ToList<AdCapStoreItem>().ForEach(delegate(AdCapStoreItem x)
		{
			this.itemsReactiveGrid.Add(x);
		});
		this.verticalContentPool.RebuildLayoutAndDraw();
		this.rebuildOnRowAdded = true;
	}

	// Token: 0x040006FD RID: 1789
	private const string ROW_VIEW_ID_PREFIX = "UpgradeStoreGridRowView";

	// Token: 0x040006FE RID: 1790
	private const string ROW_VIEW_PREFAB_NAME = "UpgradeStoreGridRowView_PREFAB";

	// Token: 0x040006FF RID: 1791
	[SerializeField]
	private float contentPoolSpacing;

	// Token: 0x04000700 RID: 1792
	[SerializeField]
	private RectTransform tform_Content;

	// Token: 0x04000701 RID: 1793
	[SerializeField]
	private AdCapStoreItemView itemPrefab;

	// Token: 0x04000702 RID: 1794
	[SerializeField]
	private VerticalListContentPool verticalContentPool;

	// Token: 0x04000703 RID: 1795
	private Dictionary<string, AdCapStoreItem> storeItemIdToAdCapStoreItem;

	// Token: 0x04000704 RID: 1796
	private ReactiveCollection<AdCapStoreItem> availableStoreItems = new ReactiveCollection<AdCapStoreItem>();

	// Token: 0x04000705 RID: 1797
	private ReactiveGrid<AdCapStoreItem> itemsReactiveGrid;

	// Token: 0x04000706 RID: 1798
	private UpgradeStoreGridRowView rowPrefab;

	// Token: 0x04000707 RID: 1799
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000708 RID: 1800
	private bool rebuildOnRowAdded;

	// Token: 0x04000709 RID: 1801
	private float customWidth = -1f;

	// Token: 0x0400070A RID: 1802
	private int numElementsHorizontal;
}
