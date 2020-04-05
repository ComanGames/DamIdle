using System;
using UnityEngine;

// Token: 0x0200025B RID: 603
public class UnlocksDetailedGridRowView : GridRowViewBase<UnlockDetailedItemView>
{
	// Token: 0x1400006D RID: 109
	// (add) Token: 0x060010E6 RID: 4326 RVA: 0x0004DB9C File Offset: 0x0004BD9C
	// (remove) Token: 0x060010E7 RID: 4327 RVA: 0x0004DBD4 File Offset: 0x0004BDD4
	public event Action<Unlock> UnlockClicked;

	// Token: 0x060010E8 RID: 4328 RVA: 0x0004DC0C File Offset: 0x0004BE0C
	public void WireData(ReactiveGridRow<Unlock> reactiveGridRow, GameState gameState, MainUIController uiController)
	{
		for (int i = 0; i < base.ElementViews.Length; i++)
		{
			bool flag = i < reactiveGridRow.Elements.Count;
			base.ElementViews[i].gameObject.SetActive(flag);
			if (flag)
			{
				Unlock unlock = reactiveGridRow.Elements[i];
				Sprite unlockSprite = uiController.GetUnlockSprite(unlock);
				base.ElementViews[i].WireData(unlock, gameState, unlockSprite, this.UnlockClicked);
			}
		}
	}
}
