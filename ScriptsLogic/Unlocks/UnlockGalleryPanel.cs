using System;
using System.Linq;
using ListContentPools;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

// Token: 0x0200025A RID: 602
public class UnlockGalleryPanel : MonoBehaviour
{
	// Token: 0x060010DF RID: 4319 RVA: 0x0004D93F File Offset: 0x0004BB3F
	public void Init(GameState gameState, MainUIController uiController)
	{
		this.gameState = gameState;
		this.uiController = uiController;
		OrientationController.Instance.OrientationStream.DelayFrame(1, FrameCountType.Update).Subscribe(delegate(OrientationChangedEvent x)
		{
			this.RebuildList();
		}).AddTo(base.gameObject);
	}

	// Token: 0x060010E0 RID: 4320 RVA: 0x0004D980 File Offset: 0x0004BB80
	private void RebuildList()
	{
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
		this.rowPrefab = gameObject.AddComponent<UnlocksDetailedGridRowView>();
		this.rowPrefab.Init(this.unlockPrefab, this.tform_UnlockContent, 10f, 0, -1f, -1f);
		this.rowPrefab.name = "UnlocksDetailedGridRowView_PREFAB";
		this.rowPrefab.gameObject.SetActive(false);
		this.verticalListContentPool.RegisterChildType<UnlocksDetailedGridRowView>("UnlocksDetailedGridRowView", this.rowPrefab, new ElementSizing(HeightSizingType.Static, 0f), new Action<string, UnlocksDetailedGridRowView>(this.OnInitializeRow), new Action<UnlocksDetailedGridRowView>(this.OnCreateRow), null);
		IOrderedEnumerable<Unlock> source = from x in GameController.Instance.UnlockService.Unlocks
		orderby x.order
		select x;
		if (!source.Any<Unlock>())
		{
			return;
		}
		Unlock[] array = source.ToArray<Unlock>();
		this.unlocksReactiveGrid = new ReactiveGrid<Unlock>(this.rowPrefab.NumElements, "UnlocksDetailedGridRowView");
		for (int i = 0; i < array.Length; i++)
		{
			this.unlocksReactiveGrid.Add(array[i]);
		}
		for (int j = 0; j < this.unlocksReactiveGrid.Rows.Count; j++)
		{
			this.verticalListContentPool.AddChild(this.unlocksReactiveGrid.Rows[j].Id, "UnlocksDetailedGridRowView", false);
		}
		this.verticalListContentPool.RebuildLayoutAndDraw();
	}

	// Token: 0x060010E1 RID: 4321 RVA: 0x0004DB2B File Offset: 0x0004BD2B
	private void OnCreateRow(UnlocksDetailedGridRowView unlockRowView)
	{
		unlockRowView.gameObject.SetActive(true);
		unlockRowView.UnlockClicked += this.OnUnlockClicked;
	}

	// Token: 0x060010E2 RID: 4322 RVA: 0x0004DB4C File Offset: 0x0004BD4C
	private void OnInitializeRow(string rowId, UnlocksDetailedGridRowView unlockRowView)
	{
		ReactiveGridRow<Unlock> rowById = this.unlocksReactiveGrid.GetRowById(rowId);
		unlockRowView.WireData(rowById, this.gameState, this.uiController);
	}

	// Token: 0x060010E3 RID: 4323 RVA: 0x0004DB79 File Offset: 0x0004BD79
	private void OnUnlockClicked(Unlock unlock)
	{
		this.descriptionView.WireData(unlock.GetDescription(), unlock.name);
	}

	// Token: 0x04000E83 RID: 3715
	private const string ROW_VIEW_ID_PREFIX = "UnlocksDetailedGridRowView";

	// Token: 0x04000E84 RID: 3716
	private const string ROW_VIEW_PREFAB_NAME = "UnlocksDetailedGridRowView_PREFAB";

	// Token: 0x04000E85 RID: 3717
	[SerializeField]
	private UnlockDetailedItemView unlockPrefab;

	// Token: 0x04000E86 RID: 3718
	[SerializeField]
	private RectTransform tform_UnlockContent;

	// Token: 0x04000E87 RID: 3719
	[SerializeField]
	private UnlockDescriptionView descriptionView;

	// Token: 0x04000E88 RID: 3720
	[SerializeField]
	private VerticalListContentPool verticalListContentPool;

	// Token: 0x04000E89 RID: 3721
	private MainUIController uiController;

	// Token: 0x04000E8A RID: 3722
	private GameState gameState;

	// Token: 0x04000E8B RID: 3723
	private UnlocksDetailedGridRowView rowPrefab;

	// Token: 0x04000E8C RID: 3724
	private ReactiveGrid<Unlock> unlocksReactiveGrid;
}
