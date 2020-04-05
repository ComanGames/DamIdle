using System;
using System.Collections.Generic;
using System.Linq;
using ListContentPools;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

// Token: 0x02000117 RID: 279
public class UpgradePanel : PanelBaseClass
{
	// Token: 0x06000755 RID: 1877 RVA: 0x00026D7F File Offset: 0x00024F7F
	private void Start()
	{
		OrientationController.Instance.OrientationStream.DelayFrame(1, FrameCountType.Update).Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
	}

	// Token: 0x06000756 RID: 1878 RVA: 0x00026DB0 File Offset: 0x00024FB0
	public void WireData(List<Upgrade> upgradeList, IconDataScriptableObject iconData, ReactiveProperty<bool> QuikBuyUnlocked = null)
	{
		if (this.dataWired)
		{
			return;
		}
		this.dataWired = true;
		this.iconData = iconData;
		this.upgradeIdToModel = new Dictionary<string, Upgrade>();
		for (int i = 0; i < upgradeList.Count; i++)
		{
			if (!this.upgradeIdToModel.ContainsKey(upgradeList[i].id))
			{
				this.upgradeIdToModel.Add(upgradeList[i].id, upgradeList[i]);
			}
		}
		foreach (KeyValuePair<string, Upgrade> keyValuePair in this.upgradeIdToModel)
		{
			this.WireSubscription(keyValuePair.Value);
		}
		this.availableUpgrades.ObserveCountChanged(true).ThrottleFrame(1, FrameCountType.Update).Subscribe(delegate(int _)
		{
			this.RebuildList();
		}).AddTo(base.gameObject);
		if (QuikBuyUnlocked != null)
		{
			QuikBuyUnlocked.Subscribe(new Action<bool>(this.SetQuikBuyUnlocked)).AddTo(base.gameObject);
		}
	}

	// Token: 0x06000757 RID: 1879 RVA: 0x00026EC4 File Offset: 0x000250C4
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

	// Token: 0x06000758 RID: 1880 RVA: 0x00026F34 File Offset: 0x00025134
	private void WireSubscription(Upgrade upgrade)
	{
		upgrade.IsPurchased.Subscribe(delegate(bool x)
		{
			if (x)
			{
				this.RemoveUpgrade(upgrade);
				return;
			}
			this.AddUpgrade(upgrade);
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000759 RID: 1881 RVA: 0x00026F7D File Offset: 0x0002517D
	private void SetQuikBuyUnlocked(bool quikBuyUnlocked)
	{
		this.quikBuyUnlocked = quikBuyUnlocked;
		if (this.quikBuyUnlocked)
		{
			this.RebuildList();
		}
	}

	// Token: 0x0600075A RID: 1882 RVA: 0x00026F94 File Offset: 0x00025194
	private void AddUpgrade(Upgrade upgrade)
	{
		this.availableUpgrades.Add(upgrade);
	}

	// Token: 0x0600075B RID: 1883 RVA: 0x00026FA2 File Offset: 0x000251A2
	private void RemoveUpgrade(Upgrade upgrade)
	{
		this.availableUpgrades.Remove(upgrade);
	}

	// Token: 0x0600075C RID: 1884 RVA: 0x00026FB1 File Offset: 0x000251B1
	private void OnRowAdded(CollectionAddEvent<ReactiveGridRow<Upgrade>> addEvent)
	{
		this.verticalListContentPool.AddChild(addEvent.Value.Id, "UpgradeGridRowView", this.rebuildOnRowAdded);
	}

	// Token: 0x0600075D RID: 1885 RVA: 0x00026FD5 File Offset: 0x000251D5
	private void OnRowRemoved(CollectionRemoveEvent<ReactiveGridRow<Upgrade>> removeEvent)
	{
		this.verticalListContentPool.RemoveChild(removeEvent.Value.Id, true);
	}

	// Token: 0x0600075E RID: 1886 RVA: 0x00026FEF File Offset: 0x000251EF
	private void OnCreateRow(UpgradeGridRowView upgradeRowView)
	{
		upgradeRowView.gameObject.SetActive(true);
	}

	// Token: 0x0600075F RID: 1887 RVA: 0x00027000 File Offset: 0x00025200
	private void OnInitializeRow(string rowId, UpgradeGridRowView upgradeRowView)
	{
		ReactiveGridRow<Upgrade> rowById = this.upgradesReactiveGrid.GetRowById(rowId);
		upgradeRowView.WireData(rowById, this.iconData.iconMap, delegate(Upgrade _)
		{
		});
	}

	// Token: 0x06000760 RID: 1888 RVA: 0x0002704C File Offset: 0x0002524C
	private void RebuildList()
	{
		Debug.Log("Rebuild List");
		if (this.quickBuyUpgradePool == null)
		{
			this.verticalListContentPool.PaddingTop = 100f;
		}
		else if (this.quikBuyUnlocked)
		{
			this.verticalListContentPool.PaddingTop = 200f;
			this.quickBuyUpgradePool.gameObject.SetActive(true);
		}
		else
		{
			this.verticalListContentPool.PaddingTop = 100f;
			this.quickBuyUpgradePool.gameObject.SetActive(false);
		}
		this.rebuildOnRowAdded = false;
		this.disposables.Clear();
		this.verticalListContentPool.ClearAndReset();
		if (this.rowPrefab != null)
		{
			Object.Destroy(this.rowPrefab.gameObject);
		}
		if (this.upgradesReactiveGrid != null)
		{
			this.upgradesReactiveGrid.Clear();
		}
		GameObject gameObject = new GameObject();
		this.rowPrefab = gameObject.AddComponent<UpgradeGridRowView>();
		this.rowPrefab.Init(this.upgradePrefab, this.tform_UpgradeContent, this.contentPoolSpacing, this.numElementsHorizontal, -1f, this.customWidth);
		this.rowPrefab.name = "UpgradeGridRowView_PREFAB";
		this.rowPrefab.gameObject.SetActive(false);
		this.verticalListContentPool.RegisterChildType<UpgradeGridRowView>("UpgradeGridRowView", this.rowPrefab, new ElementSizing(HeightSizingType.Static, 0f), new Action<string, UpgradeGridRowView>(this.OnInitializeRow), new Action<UpgradeGridRowView>(this.OnCreateRow), null);
		this.upgradesReactiveGrid = new ReactiveGrid<Upgrade>(this.rowPrefab.NumElements, "UpgradeGridRowView");
		this.upgradesReactiveGrid.Rows.ObserveAdd().Subscribe(new Action<CollectionAddEvent<ReactiveGridRow<Upgrade>>>(this.OnRowAdded)).AddTo(this.disposables);
		this.upgradesReactiveGrid.Rows.ObserveRemove().Subscribe(new Action<CollectionRemoveEvent<ReactiveGridRow<Upgrade>>>(this.OnRowRemoved)).AddTo(this.disposables);
		(from x in this.availableUpgrades
		orderby x.order, x.cost
		select x).ToList<Upgrade>().ForEach(delegate(Upgrade x)
		{
			this.upgradesReactiveGrid.Add(x);
		});
		this.verticalListContentPool.RebuildLayoutAndDraw();
		this.rebuildOnRowAdded = true;
	}

	// Token: 0x040006D4 RID: 1748
	private const string ROW_VIEW_ID_PREFIX = "UpgradeGridRowView";

	// Token: 0x040006D5 RID: 1749
	private const string ROW_VIEW_PREFAB_NAME = "UpgradeGridRowView_PREFAB";

	// Token: 0x040006D6 RID: 1750
	public Upgrade.Currency CurrencyType;

	// Token: 0x040006D7 RID: 1751
	[SerializeField]
	private float contentPoolSpacing;

	// Token: 0x040006D8 RID: 1752
	[SerializeField]
	private RectTransform tform_UpgradeContent;

	// Token: 0x040006D9 RID: 1753
	[SerializeField]
	private UpgradeView upgradePrefab;

	// Token: 0x040006DA RID: 1754
	[SerializeField]
	private VerticalListContentPool verticalListContentPool;

	// Token: 0x040006DB RID: 1755
	[SerializeField]
	private QuickBuyUpgradeView quickBuyUpgradePool;

	// Token: 0x040006DC RID: 1756
	private IconDataScriptableObject iconData;

	// Token: 0x040006DD RID: 1757
	private Dictionary<string, Upgrade> upgradeIdToModel;

	// Token: 0x040006DE RID: 1758
	private ReactiveCollection<Upgrade> availableUpgrades = new ReactiveCollection<Upgrade>();

	// Token: 0x040006DF RID: 1759
	private ReactiveGrid<Upgrade> upgradesReactiveGrid;

	// Token: 0x040006E0 RID: 1760
	private UpgradeGridRowView rowPrefab;

	// Token: 0x040006E1 RID: 1761
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040006E2 RID: 1762
	private bool rebuildOnRowAdded;

	// Token: 0x040006E3 RID: 1763
	private bool quikBuyUnlocked;

	// Token: 0x040006E4 RID: 1764
	private float customWidth = -1f;

	// Token: 0x040006E5 RID: 1765
	private int numElementsHorizontal;

	// Token: 0x040006E6 RID: 1766
	private bool dataWired;
}
