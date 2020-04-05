using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000247 RID: 583
public class ModalOrientation : MonoBehaviour
{
	// Token: 0x06001067 RID: 4199 RVA: 0x00002718 File Offset: 0x00000918
	private void Awake()
	{
	}

	// Token: 0x06001068 RID: 4200 RVA: 0x0004ABA1 File Offset: 0x00048DA1
	private void Start()
	{
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.HandleOrientationChanged)).AddTo(this);
	}

	// Token: 0x06001069 RID: 4201 RVA: 0x0004ABC8 File Offset: 0x00048DC8
	private void HandleOrientationChanged(OrientationChangedEvent orientation)
	{
		if (orientation.IsPortrait)
		{
			if (this.scalable != null)
			{
				this.scalable.sizeDelta = new Vector2(this.scalable.sizeDelta.x, this.portraitHeight);
			}
			for (int i = 0; i < this.scaledObjects.Count; i++)
			{
				this.scaledObjects[i].localScale = Vector2.one * this.scaleAmount[i];
			}
			using (List<GridLayoutGroup>.Enumerator enumerator = this.itemLists.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GridLayoutGroup gridLayoutGroup = enumerator.Current;
					gridLayoutGroup.cellSize = this.itemSizePortrait;
					gridLayoutGroup.constraint = this.itemListConstraintPortrait;
					gridLayoutGroup.constraintCount = this.itemListConstraintNumberPortrait;
				}
				goto IL_18B;
			}
		}
		if (this.scalable != null)
		{
			this.scalable.sizeDelta = new Vector2(this.scalable.sizeDelta.x, this.landscapeHeight);
		}
		for (int j = 0; j < this.scaledObjects.Count; j++)
		{
			this.scaledObjects[j].localScale = Vector2.one;
		}
		foreach (GridLayoutGroup gridLayoutGroup2 in this.itemLists)
		{
			gridLayoutGroup2.cellSize = this.itemSizeLandscape;
			gridLayoutGroup2.constraint = this.itemListConstraintLandscape;
			gridLayoutGroup2.constraintCount = this.itemListConstraintNumberLandscape;
		}
		IL_18B:
		int num = orientation.IsPortrait ? this.itemListConstraintNumberPortrait : this.itemListConstraintNumberLandscape;
		for (int k = 0; k < this.itemListRectTransforms.Count; k++)
		{
			RectTransform rectTransform = this.itemListRectTransforms[k];
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, (this.itemLists[k].cellSize.y + this.itemLists[k].spacing.y) * (float)(rectTransform.childCount / num + 1));
			rectTransform.SendMessage("ResizeScrollList", SendMessageOptions.DontRequireReceiver);
		}
	}

	// Token: 0x04000DEF RID: 3567
	public RectTransform scalable;

	// Token: 0x04000DF0 RID: 3568
	public float landscapeHeight;

	// Token: 0x04000DF1 RID: 3569
	public float portraitHeight;

	// Token: 0x04000DF2 RID: 3570
	public List<RectTransform> scaledObjects;

	// Token: 0x04000DF3 RID: 3571
	public List<float> scaleAmount;

	// Token: 0x04000DF4 RID: 3572
	public List<RectTransform> itemListRectTransforms;

	// Token: 0x04000DF5 RID: 3573
	public List<GridLayoutGroup> itemLists;

	// Token: 0x04000DF6 RID: 3574
	public Vector2 itemSizeLandscape;

	// Token: 0x04000DF7 RID: 3575
	public Vector2 itemSizePortrait;

	// Token: 0x04000DF8 RID: 3576
	public GridLayoutGroup.Constraint itemListConstraintPortrait = GridLayoutGroup.Constraint.FixedColumnCount;

	// Token: 0x04000DF9 RID: 3577
	public int itemListConstraintNumberPortrait = 1;

	// Token: 0x04000DFA RID: 3578
	public GridLayoutGroup.Constraint itemListConstraintLandscape = GridLayoutGroup.Constraint.FixedColumnCount;

	// Token: 0x04000DFB RID: 3579
	public int itemListConstraintNumberLandscape = 1;

	// Token: 0x04000DFC RID: 3580
	public Vector2 itemListSizeLandscape;

	// Token: 0x04000DFD RID: 3581
	public Vector2 itemListSizePortrait;
}
