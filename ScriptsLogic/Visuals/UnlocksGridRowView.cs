using System;
using UniRx;
using UnityEngine;

// Token: 0x0200025C RID: 604
public class UnlocksGridRowView : GridRowViewBase<UnlockItemView>
{
	// Token: 0x060010EA RID: 4330 RVA: 0x0004DC84 File Offset: 0x0004BE84
	public void OnDestroy()
	{
		this.disposables.Clear();
	}

	// Token: 0x060010EB RID: 4331 RVA: 0x0004DC91 File Offset: 0x0004BE91
	private void SetupActions()
	{
		this.cachedAddAction = new Action<CollectionAddEvent<Unlock>>(this.OnUnlockAdded);
		this.cachedRemoveAction = new Action<CollectionRemoveEvent<Unlock>>(this.OnUnlockRemoved);
		this.cachedReplaceAction = new Action<CollectionReplaceEvent<Unlock>>(this.OnUnlockReplaced);
	}

	// Token: 0x060010EC RID: 4332 RVA: 0x0004DCCC File Offset: 0x0004BECC
	public void WireData(ReactiveGridRow<Unlock> reactiveGridRow, GameState gameState, MainUIController uiController)
	{
		if (this.cachedAddAction == null)
		{
			this.SetupActions();
		}
		this.disposables.Clear();
		this.gameState = gameState;
		this.uiController = uiController;
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

	// Token: 0x060010ED RID: 4333 RVA: 0x0004DDB8 File Offset: 0x0004BFB8
	private void WireElement(Unlock unlock, UnlockItemView unlockItemView)
	{
		string text = unlock.Reward.ShortDescription(this.gameState);
		Sprite unlockSprite = this.uiController.GetUnlockSprite(unlock);
		text = text.Replace("Newspaper Delivery", "Newspaper");
		unlockItemView.WireData(unlockSprite, unlock.amountToEarn.ToString(), text);
	}

	// Token: 0x060010EE RID: 4334 RVA: 0x0004DE08 File Offset: 0x0004C008
	private void OnUnlockAdded(CollectionAddEvent<Unlock> addEvent)
	{
		this.WireElement(addEvent.Value, base.ElementViews[addEvent.Index]);
	}

	// Token: 0x060010EF RID: 4335 RVA: 0x0004DE25 File Offset: 0x0004C025
	private void OnUnlockRemoved(CollectionRemoveEvent<Unlock> removeEvent)
	{
		base.ElementViews[removeEvent.Index].gameObject.SetActive(false);
	}

	// Token: 0x060010F0 RID: 4336 RVA: 0x0004DE40 File Offset: 0x0004C040
	private void OnUnlockReplaced(CollectionReplaceEvent<Unlock> replaceEvent)
	{
		this.WireElement(replaceEvent.NewValue, base.ElementViews[replaceEvent.Index]);
	}

	// Token: 0x04000E8E RID: 3726
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000E8F RID: 3727
	private Action<CollectionAddEvent<Unlock>> cachedAddAction;

	// Token: 0x04000E90 RID: 3728
	private Action<CollectionRemoveEvent<Unlock>> cachedRemoveAction;

	// Token: 0x04000E91 RID: 3729
	private Action<CollectionReplaceEvent<Unlock>> cachedReplaceAction;

	// Token: 0x04000E92 RID: 3730
	private GameState gameState;

	// Token: 0x04000E93 RID: 3731
	private MainUIController uiController;
}
