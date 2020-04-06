using System;
using UnityEngine;
using Object = UnityEngine.Object;

// Token: 0x02000235 RID: 565
[RequireComponent(typeof(RectTransform))]
public abstract class GridRowViewBase<ViewType> : MonoBehaviour where ViewType : Component
{
	// Token: 0x1700015D RID: 349
	// (get) Token: 0x06001013 RID: 4115 RVA: 0x000496C6 File Offset: 0x000478C6
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

	// Token: 0x1700015E RID: 350
	// (get) Token: 0x06001014 RID: 4116 RVA: 0x000496EB File Offset: 0x000478EB
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

	// Token: 0x06001015 RID: 4117 RVA: 0x00049700 File Offset: 0x00047900
	public void Init(ViewType elementPrefab, RectTransform container, float spacing, int forceNumElements = 0, float customHeight = -1f, float customWidth = -1f)
	{
		RectTransform component = elementPrefab.GetComponent<RectTransform>();
		if (component == null)
		{
			throw new Exception("elementPrefab must hav RectTransform.");
		}
		float num = (forceNumElements > 0) ? (container.rect.width / (float)forceNumElements - spacing * (float)(forceNumElements - 1)) : ((customWidth == -1f) ? component.rect.width : customWidth);
		float width = container.rect.width;
		if (num > width)
		{
			num = width;
		}
		int num2 = 0;
		if (forceNumElements == 0)
		{
			while ((float)(num2 + 1) * num + (float)num2 * spacing <= width)
			{
				num2++;
			}
		}
		else
		{
			num2 = forceNumElements;
		}
		RectTransform component2 = base.GetComponent<RectTransform>();
		component2.SetParent(container, false);
		component2.pivot = new Vector2(0.5f, 1f);
		float y = (customHeight > -1f) ? customHeight : component.rect.height;
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
			component3.sizeDelta = new Vector2(num, component3.sizeDelta.y);
			num6 += num4 + num5;
		}
	}

	// Token: 0x04000DAE RID: 3502
	private ViewType[] elementViews;
}
