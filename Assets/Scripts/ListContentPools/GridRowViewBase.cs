using System;
using UnityEngine;

namespace ListContentPools
{
	// Token: 0x02000707 RID: 1799
	[RequireComponent(typeof(RectTransform))]
	public abstract class GridRowViewBase<ViewType> : MonoBehaviour where ViewType : Component
	{
		// Token: 0x17000317 RID: 791
		// (get) Token: 0x0600254C RID: 9548 RVA: 0x000A250D File Offset: 0x000A070D
		public ViewType[] ElementViews
		{
			get
			{
				if (this.elementViews == null || this.elementViews.Length == 0)
				{
					this.elementViews = base.GetComponentsInChildren<ViewType>();
				}
				return this.elementViews;
			}
		}

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x0600254D RID: 9549 RVA: 0x000A2532 File Offset: 0x000A0732
		public int NumElements
		{
			get
			{
				if (this.ElementViews == null)
				{
					return 0;
				}
				return this.ElementViews.Length;
			}
		}

		// Token: 0x0600254E RID: 9550 RVA: 0x000A2548 File Offset: 0x000A0748
		public void Init(ViewType elementPrefab, RectTransform container, float spacing, float customElemWidth = 0f, float customElemHeight = 0f)
		{
			RectTransform component = elementPrefab.GetComponent<RectTransform>();
			if (component == null)
			{
				throw new Exception("elementPrefab must hav RectTransform.");
			}
			float num = (customElemWidth > 0f) ? customElemWidth : component.rect.width;
			float width = container.rect.width;
			if (num > width)
			{
				throw new Exception("elemPrefab width must be smaller than containerWidth.");
			}
			int num2 = 0;
			while ((float)(num2 + 1) * num + (float)num2 * spacing <= width)
			{
				num2++;
			}
			RectTransform component2 = base.GetComponent<RectTransform>();
			component2.SetParent(container, false);
			component2.pivot = new Vector2(0.5f, 1f);
			float y = (customElemHeight > 0f) ? customElemHeight : component.rect.height;
			float num3 = (float)num2 * num + (float)(num2 - 1) * spacing;
			component2.sizeDelta = new Vector2(num3, y);
			this.elementViews = new ViewType[num2];
			float num4 = num / num3;
			float num5 = spacing / num3;
			float num6 = num4 * 0.5f;
			for (int i = 0; i < num2; i++)
			{
				ViewType viewType = Object.Instantiate<ViewType>(elementPrefab);
				this.elementViews[i] = viewType;
				RectTransform component3 = viewType.GetComponent<RectTransform>();
				component3.transform.SetParent(component2, false);
				component3.pivot = new Vector2(0.5f, 1f);
				component3.anchorMin = new Vector2(num6, 1f);
				component3.anchorMax = new Vector2(num6, 1f);
				component3.anchoredPosition = Vector2.zero;
				num6 += num4 + num5;
			}
		}

		// Token: 0x04002649 RID: 9801
		private ViewType[] elementViews;
	}
}
