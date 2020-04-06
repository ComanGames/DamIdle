using System;
using System.Collections.Generic;
using UnityEngine;

namespace ListContentPools
{
	// Token: 0x02000704 RID: 1796
	public class ComponentPool
	{
		// Token: 0x06002545 RID: 9541 RVA: 0x000A2250 File Offset: 0x000A0450
		public ComponentPool(Component elemRef, float initialY, Transform parent, Action<Component> creationHandler)
		{
			this.elemRef = elemRef;
			this.initialY = initialY;
			this.parentTrans = parent;
			this.creationHandler = creationHandler;
		}

		// Token: 0x06002546 RID: 9542 RVA: 0x000A22A8 File Offset: 0x000A04A8
		public RectTransform GetRectTransForPoolElem(Component elem)
		{
			return this.pooledComponentToRectTransform[elem];
		}

		// Token: 0x06002547 RID: 9543 RVA: 0x000A22B8 File Offset: 0x000A04B8
		public void Clear()
		{
			for (int i = 0; i < this.elems.Count; i++)
			{
				Object.DestroyImmediate(this.elems[i].gameObject);
			}
			this.elems.Clear();
			this.elemRef = null;
			this.creationHandler = null;
			this.pooledComponentToRectTransform.Clear();
		}

		// Token: 0x06002548 RID: 9544 RVA: 0x000A2315 File Offset: 0x000A0515
		private void Add(Component elem, bool used)
		{
			this.elems.Add(elem);
			this.occupied.Add(used);
			this.lastFreeIndex = this.occupied.Count - 1;
		}

		// Token: 0x06002549 RID: 9545 RVA: 0x000A2344 File Offset: 0x000A0544
		public Component Get()
		{
			for (int i = 0; i < this.elems.Count; i++)
			{
				this.lastFreeIndex++;
				if (this.lastFreeIndex == this.elems.Count)
				{
					this.lastFreeIndex = 0;
				}
				if (!this.occupied[this.lastFreeIndex])
				{
					this.occupied[this.lastFreeIndex] = true;
					return this.elems[this.lastFreeIndex];
				}
			}
			Component component = Object.Instantiate<Component>(this.elemRef, this.parentTrans);
			RectTransform component2 = component.GetComponent<RectTransform>();
			Vector2 anchorMin = component2.anchorMin;
			anchorMin.y = this.initialY;
			component2.anchorMin = anchorMin;
			anchorMin.x = component2.anchorMax.x;
			component2.anchorMax = anchorMin;
			component2.SetAsLastSibling();
			this.pooledComponentToRectTransform.Add(component, component2);
			component2.pivot = new Vector2(0.5f, 1f);
			this.Add(component, true);
			if (this.creationHandler != null)
			{
				this.creationHandler(component);
			}
			return component;
		}

		// Token: 0x0600254A RID: 9546 RVA: 0x000A2458 File Offset: 0x000A0658
		public void Release(Component elem)
		{
			RectTransform rectTransForPoolElem = this.GetRectTransForPoolElem(elem);
			Vector2 anchorMin = rectTransForPoolElem.anchorMin;
			anchorMin.y = this.initialY;
			rectTransForPoolElem.anchorMin = anchorMin;
			anchorMin.x = rectTransForPoolElem.anchorMax.x;
			anchorMin.y = this.initialY;
			rectTransForPoolElem.anchorMax = anchorMin;
			rectTransForPoolElem.SetAsLastSibling();
			for (int i = 0; i < this.elems.Count; i++)
			{
				if (this.elems[i] == elem)
				{
					this.occupied[i] = false;
					this.lastFreeIndex = i;
					return;
				}
			}
			throw new Exception("Component not in pool");
		}

		// Token: 0x0400263C RID: 9788
		private List<Component> elems = new List<Component>();

		// Token: 0x0400263D RID: 9789
		private List<bool> occupied = new List<bool>();

		// Token: 0x0400263E RID: 9790
		private int lastFreeIndex = -1;

		// Token: 0x0400263F RID: 9791
		private Component elemRef;

		// Token: 0x04002640 RID: 9792
		private float initialY;

		// Token: 0x04002641 RID: 9793
		private Transform parentTrans;

		// Token: 0x04002642 RID: 9794
		private Action<Component> creationHandler;

		// Token: 0x04002643 RID: 9795
		private Dictionary<Component, RectTransform> pooledComponentToRectTransform = new Dictionary<Component, RectTransform>();
	}
}
