using System;
using ListContentPools;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Token: 0x0200025D RID: 605
public class UnlocksPanel : PanelBaseClass
{
	// Token: 0x060010F2 RID: 4338 RVA: 0x0004DE70 File Offset: 0x0004C070
	public void Init(GameState state, MainUIController uiController)
	{
		this.gameState = state;
		this.mainUiController = uiController;
		this.scrollRect.enabled = false;
		GameController.Instance.UnlockService.ReactiveNextUnlocks.ObserveCountChanged(true).ThrottleFrame(1, FrameCountType.Update).Subscribe(delegate(int _)
		{
			this.RebuildList();
		}).AddTo(base.gameObject);
		OrientationController.Instance.OrientationStream.DelayFrame(1, FrameCountType.Update).Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
	}

	// Token: 0x060010F3 RID: 4339 RVA: 0x0004DF00 File Offset: 0x0004C100
	private void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		if (orientation.IsPortrait)
		{
			this.elementHeight = ((RectTransform)this.unlockPrefab.transform).rect.height;
			((RectTransform)this.unlockPrefab.transform).localScale = Vector3.one;
			this.verticalListContentPool.Spacing = 20f;
			this.horizontalSpacing = 20f;
		}
		else
		{
			this.elementHeight = ((RectTransform)this.unlockPrefab.transform).rect.height * 0.85f;
			((RectTransform)this.unlockPrefab.transform).localScale = new Vector3(0.85f, 0.85f, 1f);
			this.verticalListContentPool.Spacing = 15f;
			this.horizontalSpacing = 0f;
		}
		this.RebuildList();
	}

	// Token: 0x060010F4 RID: 4340 RVA: 0x0004DFE4 File Offset: 0x0004C1E4
	private void RebuildList()
	{
		this.rebuildOnRowAdded = false;
		this.disposables.Clear();
		this.verticalListContentPool.ClearAndReset();
		if (this.rowPrefab != null)
		{
			Object.Destroy(this.rowPrefab.gameObject);
		}
		if (this.unlocksReactiveGrid != null)
		{
			this.unlocksReactiveGrid.Clear();
		}
		GameObject gameObject = new GameObject();
		this.rowPrefab = gameObject.AddComponent<UnlocksGridRowView>();
		this.rowPrefab.Init(this.unlockPrefab, this.tform_UnlockContent, this.horizontalSpacing, 0, this.elementHeight, -1f);
		this.rowPrefab.name = "UnlocksGridRowView_PREFAB";
		this.rowPrefab.gameObject.SetActive(false);
		this.verticalListContentPool.RegisterChildType<UnlocksGridRowView>("UnlocksGridRowView", this.rowPrefab, new ElementSizing(HeightSizingType.Static, 0f), new Action<string, UnlocksGridRowView>(this.OnInitializeRow), new Action<UnlocksGridRowView>(this.OnCreateRow), null);
		this.unlocksReactiveGrid = new ReactiveGrid<Unlock>(this.rowPrefab.NumElements, "UnlocksGridRowView");
		this.unlocksReactiveGrid.Rows.ObserveAdd().Subscribe(new Action<CollectionAddEvent<ReactiveGridRow<Unlock>>>(this.OnRowAdded)).AddTo(this.disposables);
		this.unlocksReactiveGrid.Rows.ObserveRemove().Subscribe(new Action<CollectionRemoveEvent<ReactiveGridRow<Unlock>>>(this.OnRowRemoved)).AddTo(this.disposables);
		ReactiveCollection<Unlock> reactiveNextUnlocks = GameController.Instance.UnlockService.ReactiveNextUnlocks;
		for (int i = 0; i < reactiveNextUnlocks.Count; i++)
		{
			this.unlocksReactiveGrid.Add(reactiveNextUnlocks[i]);
		}
		this.verticalListContentPool.RebuildLayoutAndDraw();
		this.rebuildOnRowAdded = true;
	}

	// Token: 0x060010F5 RID: 4341 RVA: 0x0004E190 File Offset: 0x0004C390
	private void OnRowAdded(CollectionAddEvent<ReactiveGridRow<Unlock>> addEvent)
	{
		this.verticalListContentPool.AddChild(addEvent.Value.Id, "UnlocksGridRowView", this.rebuildOnRowAdded);
	}

	// Token: 0x060010F6 RID: 4342 RVA: 0x0004E1B4 File Offset: 0x0004C3B4
	private void OnRowRemoved(CollectionRemoveEvent<ReactiveGridRow<Unlock>> removeEvent)
	{
		this.verticalListContentPool.RemoveChild(removeEvent.Value.Id, true);
	}

	// Token: 0x060010F7 RID: 4343 RVA: 0x00026FEF File Offset: 0x000251EF
	private void OnCreateRow(UnlocksGridRowView unlockRowView)
	{
		unlockRowView.gameObject.SetActive(true);
	}

	// Token: 0x060010F8 RID: 4344 RVA: 0x0004E1D0 File Offset: 0x0004C3D0
	private void OnInitializeRow(string rowId, UnlocksGridRowView unlockRowView)
	{
		ReactiveGridRow<Unlock> rowById = this.unlocksReactiveGrid.GetRowById(rowId);
		unlockRowView.WireData(rowById, this.gameState, this.mainUiController);
	}

	// Token: 0x04000E94 RID: 3732
	private const string ROW_VIEW_ID_PREFIX = "UnlocksGridRowView";

	// Token: 0x04000E95 RID: 3733
	private const string ROW_VIEW_PREFAB_NAME = "UnlocksGridRowView_PREFAB";

	// Token: 0x04000E96 RID: 3734
	[SerializeField]
	private UnlockItemView unlockPrefab;

	// Token: 0x04000E97 RID: 3735
	[SerializeField]
	private RectTransform tform_UnlockContent;

	// Token: 0x04000E98 RID: 3736
	[SerializeField]
	private VerticalListContentPool verticalListContentPool;

	// Token: 0x04000E99 RID: 3737
	[SerializeField]
	private float horizontalSpacing;

	// Token: 0x04000E9A RID: 3738
	[SerializeField]
	private ScrollRect scrollRect;

	// Token: 0x04000E9B RID: 3739
	private MainUIController mainUiController;

	// Token: 0x04000E9C RID: 3740
	private GameState gameState;

	// Token: 0x04000E9D RID: 3741
	private UnlocksGridRowView rowPrefab;

	// Token: 0x04000E9E RID: 3742
	private ReactiveGrid<Unlock> unlocksReactiveGrid;

	// Token: 0x04000E9F RID: 3743
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000EA0 RID: 3744
	private bool rebuildOnRowAdded;

	// Token: 0x04000EA1 RID: 3745
	private float elementHeight;
}
