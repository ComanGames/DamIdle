using System;
using UniRx;
using UnityEngine;

// Token: 0x02000115 RID: 277
public class UpgradeGridRowView : GridRowViewBase<UpgradeView>
{
	// Token: 0x06000746 RID: 1862 RVA: 0x0002694E File Offset: 0x00024B4E
	public void OnDestroy()
	{
		this.disposables.Clear();
	}

	// Token: 0x06000747 RID: 1863 RVA: 0x0002695B File Offset: 0x00024B5B
	private void SetupActions()
	{
		this.cachedAddAction = new Action<CollectionAddEvent<Upgrade>>(this.OnUpgradeAdded);
		this.cachedRemoveAction = new Action<CollectionRemoveEvent<Upgrade>>(this.OnUpgradeRemoved);
		this.cachedReplaceAction = new Action<CollectionReplaceEvent<Upgrade>>(this.OnUpgradeReplaced);
	}

	// Token: 0x06000748 RID: 1864 RVA: 0x00026994 File Offset: 0x00024B94
	public void WireData(ReactiveGridRow<Upgrade> reactiveGridRow, StringSpriteDictionary iconMap, Action<Upgrade> upgradeAction)
	{
		if (this.cachedAddAction == null)
		{
			this.SetupActions();
		}
		this.iconMap = iconMap;
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

	// Token: 0x06000749 RID: 1865 RVA: 0x00026A80 File Offset: 0x00024C80
	private void WireElement(Upgrade upgrade, UpgradeView upgradeView)
	{
		if (!this.iconMap.ContainsKey(upgrade.imageName.ToLower()))
		{
			Debug.LogErrorFormat("[{0}] Icon name {1}", new object[]
			{
				base.gameObject.name,
				upgrade.imageName.ToLower()
			});
		}
		Sprite icon = this.iconMap[upgrade.imageName.ToLower()];
		upgradeView.WireData(upgrade, icon, this.onUpgradeClickedAction);
	}

	// Token: 0x0600074A RID: 1866 RVA: 0x00026AF6 File Offset: 0x00024CF6
	private void OnUpgradeAdded(CollectionAddEvent<Upgrade> addEvent)
	{
		this.WireElement(addEvent.Value, base.ElementViews[addEvent.Index]);
	}

	// Token: 0x0600074B RID: 1867 RVA: 0x00026B13 File Offset: 0x00024D13
	private void OnUpgradeRemoved(CollectionRemoveEvent<Upgrade> removeEvent)
	{
		base.ElementViews[removeEvent.Index].gameObject.SetActive(false);
	}

	// Token: 0x0600074C RID: 1868 RVA: 0x00026B2E File Offset: 0x00024D2E
	private void OnUpgradeReplaced(CollectionReplaceEvent<Upgrade> replaceEvent)
	{
		this.WireElement(replaceEvent.NewValue, base.ElementViews[replaceEvent.Index]);
	}

	// Token: 0x040006C9 RID: 1737
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040006CA RID: 1738
	private Action<CollectionAddEvent<Upgrade>> cachedAddAction;

	// Token: 0x040006CB RID: 1739
	private Action<CollectionRemoveEvent<Upgrade>> cachedRemoveAction;

	// Token: 0x040006CC RID: 1740
	private Action<CollectionReplaceEvent<Upgrade>> cachedReplaceAction;

	// Token: 0x040006CD RID: 1741
	private Action<Upgrade> onUpgradeClickedAction;

	// Token: 0x040006CE RID: 1742
	private StringSpriteDictionary iconMap;
}
