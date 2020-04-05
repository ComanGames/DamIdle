using System;
using AdCap.Store;
using UniRx;

// Token: 0x02000119 RID: 281
public class UpgradeStoreGridRowView : GridRowViewBase<AdCapStoreItemView>
{
	// Token: 0x0600077C RID: 1916 RVA: 0x00027F31 File Offset: 0x00026131
	public void OnDestroy()
	{
		this.disposables.Clear();
	}

	// Token: 0x0600077D RID: 1917 RVA: 0x00027F3E File Offset: 0x0002613E
	private void SetupActions()
	{
		this.cachedAddAction = new Action<CollectionAddEvent<AdCapStoreItem>>(this.OnUpgradeAdded);
		this.cachedRemoveAction = new Action<CollectionRemoveEvent<AdCapStoreItem>>(this.OnUpgradeRemoved);
		this.cachedReplaceAction = new Action<CollectionReplaceEvent<AdCapStoreItem>>(this.OnUpgradeReplaced);
	}

	// Token: 0x0600077E RID: 1918 RVA: 0x00027F78 File Offset: 0x00026178
	public void WireData(ReactiveGridRow<AdCapStoreItem> reactiveGridRow, Action<AdCapStoreItem> upgradeAction)
	{
		if (this.cachedAddAction == null)
		{
			this.SetupActions();
		}
		this.onUpgradeClickedAction = upgradeAction;
		this.disposables.Clear();
		for (int i = 0; i < base.ElementViews.Length; i++)
		{
			bool flag = i < reactiveGridRow.Elements.Count;
			base.ElementViews[i].gameObject.SetActive(flag);
			if (flag)
			{
				this.WireElement(reactiveGridRow.Elements[i], base.ElementViews[i]);
			}
		}
		reactiveGridRow.Elements.ObserveAdd().Subscribe(this.cachedAddAction).AddTo(this.disposables);
		reactiveGridRow.Elements.ObserveRemove().Subscribe(this.cachedRemoveAction).AddTo(this.disposables);
		reactiveGridRow.Elements.ObserveReplace().Subscribe(this.cachedReplaceAction).AddTo(this.disposables);
	}

	// Token: 0x0600077F RID: 1919 RVA: 0x0002805D File Offset: 0x0002625D
	private void WireElement(AdCapStoreItem upgrade, AdCapStoreItemView upgradeView)
	{
		upgradeView.WireData(upgrade, this.onUpgradeClickedAction);
	}

	// Token: 0x06000780 RID: 1920 RVA: 0x0002806C File Offset: 0x0002626C
	private void OnUpgradeAdded(CollectionAddEvent<AdCapStoreItem> addEvent)
	{
		this.WireElement(addEvent.Value, base.ElementViews[addEvent.Index]);
	}

	// Token: 0x06000781 RID: 1921 RVA: 0x00028089 File Offset: 0x00026289
	private void OnUpgradeRemoved(CollectionRemoveEvent<AdCapStoreItem> removeEvent)
	{
		base.ElementViews[removeEvent.Index].gameObject.SetActive(false);
	}

	// Token: 0x06000782 RID: 1922 RVA: 0x000280A4 File Offset: 0x000262A4
	private void OnUpgradeReplaced(CollectionReplaceEvent<AdCapStoreItem> replaceEvent)
	{
		this.WireElement(replaceEvent.NewValue, base.ElementViews[replaceEvent.Index]);
	}

	// Token: 0x040006F8 RID: 1784
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040006F9 RID: 1785
	private Action<CollectionAddEvent<AdCapStoreItem>> cachedAddAction;

	// Token: 0x040006FA RID: 1786
	private Action<CollectionRemoveEvent<AdCapStoreItem>> cachedRemoveAction;

	// Token: 0x040006FB RID: 1787
	private Action<CollectionReplaceEvent<AdCapStoreItem>> cachedReplaceAction;

	// Token: 0x040006FC RID: 1788
	private Action<AdCapStoreItem> onUpgradeClickedAction;
}
