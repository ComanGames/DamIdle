using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ListContentPools
{
	// Token: 0x0200070A RID: 1802
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(ScrollRect))]
	public class VerticalListContentPool : MonoBehaviour
	{
		// Token: 0x17000319 RID: 793
		// (get) Token: 0x06002552 RID: 9554 RVA: 0x000A2734 File Offset: 0x000A0934
		private ScrollRect scrollRect
		{
			get
			{
				if (this._scrollRect == null)
				{
					this._scrollRect = base.GetComponent<ScrollRect>();
					this._scrollRect.content.pivot = new Vector2(0.5f, 1f);
					Vector2 anchorMin = this._scrollRect.content.anchorMin;
					anchorMin.y = 1f;
					Vector2 anchorMax = this._scrollRect.content.anchorMax;
					anchorMax.y = 1f;
					this._scrollRect.content.anchorMin = anchorMin;
					this._scrollRect.content.anchorMax = anchorMax;
				}
				return this._scrollRect;
			}
		}

		// Token: 0x06002553 RID: 9555 RVA: 0x000A27E0 File Offset: 0x000A09E0
		private void Start()
		{
			this.viewport = ((this.scrollRect.viewport != null) ? this.scrollRect.viewport : this.scrollRect.GetComponent<RectTransform>());
			this.scrollRect.onValueChanged.AddListener(delegate(Vector2 v)
			{
				this.scrolled = true;
			});
			this.isDrawing = true;
			this.DrawListElements();
		}

		// Token: 0x06002554 RID: 9556 RVA: 0x000A2848 File Offset: 0x000A0A48
		public void RegisterChildType<T>(string typeId, T prefab, ElementSizing elementSizing, Action<string, T> initializationHandler, Action<T> creationHandler = null, Action<string, int, T> topElementHandler = null) where T : Component
		{
			if (this.typeIdToData.ContainsKey(typeId))
			{
				throw new Exception("Type already registered");
			}
			if (prefab.GetComponent<RectTransform>() == null)
			{
				throw new Exception("Prefab must have RectTransform Component");
			}
			Rect rect = prefab.GetComponent<RectTransform>().rect;
			if (elementSizing.HeightSizingType == HeightSizingType.Dynamic && elementSizing.MinHeight > 0f && rect.height < elementSizing.MinHeight)
			{
				rect.height = elementSizing.MinHeight;
			}
			this.isUsingDynamicElementHeights |= (elementSizing.HeightSizingType == HeightSizingType.Dynamic);
			Action<string, Component> initHandler = delegate(string uniqueId, Component component)
			{
				initializationHandler(uniqueId, component as T);
			};
			Action<Component> creationHandler2 = delegate(Component component)
			{
				if (creationHandler != null)
				{
					creationHandler(component as T);
				}
			};
			Action<string, int, Component> topElementHandler2 = delegate(string uniqueId, int index, Component component)
			{
				if (topElementHandler != null)
				{
					topElementHandler(uniqueId, index, component as T);
				}
			};
			ListElemTypeData value = new ListElemTypeData(new ComponentPool(prefab, 9999999f, this.scrollRect.content, creationHandler2), rect, initHandler, topElementHandler2);
			this.typeIdToData.Add(typeId, value);
		}

		// Token: 0x06002555 RID: 9557 RVA: 0x000A295C File Offset: 0x000A0B5C
		public void AddChild(string uniqueId, string typeId, bool forceRebuildLayout)
		{
			this.InsertChild(uniqueId, typeId, this.listElements.Count, forceRebuildLayout);
		}

		// Token: 0x06002556 RID: 9558 RVA: 0x000A2974 File Offset: 0x000A0B74
		public void InsertChild(string uniqueId, string typeId, int index, bool forceRebuildLayout)
		{
			if (!this.typeIdToData.ContainsKey(typeId))
			{
				throw new Exception("Unregistered typeId: " + typeId);
			}
			if (this.uniqueIdToInstanceData.ContainsKey(uniqueId))
			{
				throw new Exception(string.Format("Element with uniqueId {0} already registered.", uniqueId));
			}
			if (index > this.listElements.Count)
			{
				index = this.listElements.Count;
			}
			if (this.prevLowestVisibleIndex == index)
			{
				this.prevLowestVisibleIndex = int.MaxValue;
			}
			ListElemTypeData listElemTypeData = this.typeIdToData[typeId];
			ListElemInstanceData listElemInstanceData = new ListElemInstanceData(uniqueId, typeId, index, listElemTypeData.DefaultBounds, true);
			this.uniqueIdToInstanceData.Add(uniqueId, listElemInstanceData);
			this.listElements.Insert(index, listElemInstanceData);
			if (index < this.listElements.Count - 1)
			{
				for (int i = index + 1; i < this.listElements.Count; i++)
				{
					this.listElements[i].Index++;
				}
			}
			this.UpdateContentHeightWithPadding((this.listElements.Count == 1) ? listElemInstanceData.BoundsRect.height : (this.Spacing + listElemInstanceData.BoundsRect.height));
			if (forceRebuildLayout)
			{
				this.RebuildLayoutAndDraw();
			}
		}

		// Token: 0x06002557 RID: 9559 RVA: 0x000A2AA4 File Offset: 0x000A0CA4
		public void RemoveChild(string uniqueId, bool forceRebuildLayout)
		{
			if (!this.uniqueIdToInstanceData.ContainsKey(uniqueId))
			{
				throw new Exception(string.Format("List doesn't contain element with uniqueId {0}", uniqueId));
			}
			ListElemInstanceData listElemInstanceData = this.uniqueIdToInstanceData[uniqueId];
			if (this.prevLowestVisibleIndex == listElemInstanceData.Index)
			{
				this.prevLowestVisibleIndex = int.MaxValue;
			}
			if (listElemInstanceData.PooledComponent != null)
			{
				this.ReleasePooledEntryView(listElemInstanceData);
			}
			this.listElements.RemoveAt(listElemInstanceData.Index);
			this.uniqueIdToInstanceData.Remove(uniqueId);
			float num = listElemInstanceData.Updated ? listElemInstanceData.BoundsRect.height : this.typeIdToData[listElemInstanceData.TypeId].DefaultBounds.height;
			if (listElemInstanceData.Index < this.listElements.Count)
			{
				for (int i = listElemInstanceData.Index + 1; i < this.listElements.Count; i++)
				{
					this.listElements[i].Index--;
				}
			}
			this.UpdateContentHeightWithPadding((this.listElements.Count == 0) ? (-num) : (-(this.Spacing + num)));
			if (forceRebuildLayout)
			{
				this.RebuildLayoutAndDraw();
			}
		}

		// Token: 0x06002558 RID: 9560 RVA: 0x000A2BCC File Offset: 0x000A0DCC
		public int GetChildIndex(string uniqueId)
		{
			ListElemInstanceData listElemInstanceData;
			if (!this.uniqueIdToInstanceData.TryGetValue(uniqueId, out listElemInstanceData))
			{
				return -1;
			}
			return listElemInstanceData.Index;
		}

		// Token: 0x06002559 RID: 9561 RVA: 0x000A2BF4 File Offset: 0x000A0DF4
		private void UpdateContentHeightWithPadding(float heightDelta)
		{
			this.contentHeight += heightDelta;
			if (this.contentHeight < 0f)
			{
				this.contentHeight = 0f;
			}
			this.clampedPaddingTop = ((this.PaddingTop > 1f) ? this.PaddingTop : 1f);
			this.clampedPaddingBottom = ((this.PaddingBottom > 1f) ? this.PaddingBottom : 1f);
			this.contentHeightWithPadding = this.clampedPaddingTop + this.contentHeight + this.clampedPaddingBottom;
		}

		// Token: 0x0600255A RID: 9562 RVA: 0x000A2C84 File Offset: 0x000A0E84
		public void RebuildLayoutAndDraw()
		{
			if (this.rectTransform == null)
			{
				this.rectTransform = base.GetComponent<RectTransform>();
			}
			Vector2 sizeDelta = this.scrollRect.content.sizeDelta;
			sizeDelta.y = this.contentHeightWithPadding;
			this.scrollRect.content.sizeDelta = sizeDelta;
			float num = 1f / this.contentHeightWithPadding;
			float num2 = this.clampedPaddingTop * num;
			float num3 = 1f - num2;
			float num4 = this.Spacing * num;
			for (int i = 0; i < this.listElements.Count; i++)
			{
				ListElemInstanceData listElemInstanceData = this.listElements[i];
				Rect boundsRect = listElemInstanceData.BoundsRect;
				float num5 = boundsRect.height * num;
				listElemInstanceData.NormalizedLocalTop = num3;
				listElemInstanceData.NormalizedLocalBottom = num3 - num5;
				num3 -= num5 + num4;
			}
			this.isLayoutDirty = true;
			this.DrawListElements();
		}

		// Token: 0x0600255B RID: 9563 RVA: 0x000A2D64 File Offset: 0x000A0F64
		private void DrawListElements()
		{
			if (!this.isDrawing)
			{
				return;
			}
			this.viewport.GetWorldCorners(this.viewportWorldCornersCached);
			this.scrollRect.content.GetWorldCorners(this.contentWorldCornersCached);
			float num = this.contentWorldCornersCached[1].y - this.contentWorldCornersCached[0].y;
			float num2 = this.contentWorldCornersCached[1].y - this.viewportWorldCornersCached[1].y;
			if (num2 > num)
			{
				num2 = num - (this.viewportWorldCornersCached[1].y - this.viewportWorldCornersCached[0].y);
			}
			float num3 = 1f - num2 / num;
			bool flag = num3 < this.prevNormalizedScrollRectTop;
			this.prevNormalizedScrollRectTop = num3;
			if (num3 > 1f)
			{
				num3 = 1f;
			}
			float num4 = this.contentWorldCornersCached[1].y - this.viewportWorldCornersCached[0].y;
			float num5 = 1f - num4 / num;
			if (num5 < 0f)
			{
				num5 = 0f;
			}
			int num6 = int.MaxValue;
			bool flag2 = false;
			for (int i = 0; i < this.listElements.Count; i++)
			{
				int num7 = flag ? i : (this.listElements.Count - 1 - i);
				ListElemInstanceData listElemInstanceData = this.listElements[num7];
				if ((listElemInstanceData.NormalizedLocalTop <= num3 && listElemInstanceData.NormalizedLocalTop >= num5) || (listElemInstanceData.NormalizedLocalBottom <= num3 && listElemInstanceData.NormalizedLocalBottom >= num5))
				{
					if (num7 < num6)
					{
						num6 = num7;
					}
					bool flag3 = listElemInstanceData.PooledComponent != null;
					if (!flag3 || this.isLayoutDirty)
					{
						Component component;
						if (flag3)
						{
							component = listElemInstanceData.PooledComponent;
						}
						else
						{
							component = this.typeIdToData[listElemInstanceData.TypeId].ComponentPool.Get();
							listElemInstanceData.PooledComponent = component;
						}
						RectTransform rectTransForPoolElem = this.typeIdToData[listElemInstanceData.TypeId].ComponentPool.GetRectTransForPoolElem(listElemInstanceData.PooledComponent);
						Vector2 anchorMin = rectTransForPoolElem.anchorMin;
						anchorMin.y = listElemInstanceData.NormalizedLocalTop;
						rectTransForPoolElem.anchorMin = anchorMin;
						Vector2 anchorMax = rectTransForPoolElem.anchorMax;
						anchorMax.y = listElemInstanceData.NormalizedLocalTop;
						rectTransForPoolElem.anchorMax = anchorMax;
						Vector2 anchoredPosition = rectTransForPoolElem.anchoredPosition;
						anchoredPosition.y = 0f;
						rectTransForPoolElem.anchoredPosition = anchoredPosition;
						if (!flag3)
						{
							flag2 = true;
							component.name = listElemInstanceData.UniqueId;
							this.typeIdToData[listElemInstanceData.TypeId].InitHandler(listElemInstanceData.UniqueId, component);
						}
					}
				}
				else if (listElemInstanceData.PooledComponent != null)
				{
					this.ReleasePooledEntryView(listElemInstanceData);
				}
			}
			this.isLayoutDirty = false;
			if (num6 == 2147483647)
			{
				return;
			}
			if (num6 != this.prevLowestVisibleIndex)
			{
				this.prevLowestVisibleIndex = num6;
				ListElemInstanceData listElemInstanceData2 = this.listElements[num6];
				string typeId = listElemInstanceData2.TypeId;
				if (this.typeIdToData[typeId].TopElementHandler != null)
				{
					this.typeIdToData[typeId].TopElementHandler(listElemInstanceData2.UniqueId, num6, listElemInstanceData2.PooledComponent);
				}
			}
			if (this.SortChildTransforms && flag2)
			{
				this.UpdateSiblingIndices();
			}
		}

		// Token: 0x0600255C RID: 9564 RVA: 0x000A30B4 File Offset: 0x000A12B4
		private void ReleasePooledEntryView(ListElemInstanceData elemInstanceData)
		{
			this.typeIdToData[elemInstanceData.TypeId].ComponentPool.Release(elemInstanceData.PooledComponent);
			elemInstanceData.PooledComponent.name = elemInstanceData.TypeId + "_FreeElement";
			elemInstanceData.PooledComponent = null;
		}

		// Token: 0x0600255D RID: 9565 RVA: 0x000A3104 File Offset: 0x000A1304
		private void UpdateSiblingIndices()
		{
			if (this.prevLowestVisibleIndex == 2147483647)
			{
				return;
			}
			int num = this.prevLowestVisibleIndex;
			ListElemInstanceData listElemInstanceData = this.listElements[num];
			int num2 = 0;
			while (listElemInstanceData.PooledComponent != null)
			{
				this.typeIdToData[listElemInstanceData.TypeId].ComponentPool.GetRectTransForPoolElem(listElemInstanceData.PooledComponent).SetSiblingIndex(num2++);
				num++;
				if (num == this.listElements.Count)
				{
					break;
				}
				listElemInstanceData = this.listElements[num];
			}
		}

		// Token: 0x0600255E RID: 9566 RVA: 0x000A3190 File Offset: 0x000A1390
		private void Update()
		{
			if (this.prevLowestVisibleIndex == 2147483647 || this.prevLowestVisibleIndex >= this.listElements.Count)
			{
				return;
			}
			if (this.isUsingDynamicElementHeights)
			{
				int num = this.prevLowestVisibleIndex;
				ListElemInstanceData listElemInstanceData = this.listElements[num];
				bool flag = false;
				while (listElemInstanceData.PooledComponent != null)
				{
					Rect rect = this.typeIdToData[listElemInstanceData.TypeId].ComponentPool.GetRectTransForPoolElem(listElemInstanceData.PooledComponent).rect;
					if (listElemInstanceData.IsDefaultDimensions)
					{
						Rect boundsRect = listElemInstanceData.BoundsRect;
						boundsRect.height = -1f;
						listElemInstanceData.BoundsRect = boundsRect;
						listElemInstanceData.IsDefaultDimensions = false;
					}
					else if (listElemInstanceData.BoundsRect.height == -1f)
					{
						flag = true;
						listElemInstanceData.BoundsRect = rect;
					}
					num++;
					if (num == this.listElements.Count)
					{
						break;
					}
					listElemInstanceData = this.listElements[num];
				}
				if (flag)
				{
					this.RefreshContentHeightAndLayout();
					return;
				}
			}
			if (this.scrolled)
			{
				this.DrawListElements();
				this.scrolled = false;
			}
		}

		// Token: 0x0600255F RID: 9567 RVA: 0x000A32A4 File Offset: 0x000A14A4
		private void RefreshContentHeightAndLayout()
		{
			this.contentHeight = 0f;
			this.UpdateContentHeightWithPadding(0f);
			for (int i = 0; i < this.listElements.Count; i++)
			{
				float height = this.listElements[i].BoundsRect.height;
				this.UpdateContentHeightWithPadding((i == 0) ? height : (this.Spacing + height));
			}
			this.RebuildLayoutAndDraw();
		}

		// Token: 0x06002560 RID: 9568 RVA: 0x000A3310 File Offset: 0x000A1510
		public void Clear()
		{
			for (int i = 0; i < this.listElements.Count; i++)
			{
				if (this.listElements[i].PooledComponent != null)
				{
					this.ReleasePooledEntryView(this.listElements[i]);
				}
			}
			this.prevLowestVisibleIndex = int.MaxValue;
			this.uniqueIdToInstanceData.Clear();
			this.listElements.Clear();
			this.contentHeight = 0f;
			this.UpdateContentHeightWithPadding(0f);
			this.RebuildLayoutAndDraw();
		}

		// Token: 0x06002561 RID: 9569 RVA: 0x000A339C File Offset: 0x000A159C
		public void ClearAndReset()
		{
			this.Clear();
			foreach (ListElemTypeData listElemTypeData in this.typeIdToData.Values)
			{
				listElemTypeData.ComponentPool.Clear();
			}
			this.typeIdToData.Clear();
			this.scrolled = false;
			this.prevNormalizedScrollRectTop = 1f;
			this.isLayoutDirty = false;
		}

		// Token: 0x06002562 RID: 9570 RVA: 0x000A3420 File Offset: 0x000A1620
		public void JumpToIndex(int index, float normalizedOffset = 0f)
		{
			if (this.listElements.Count == 0)
			{
				return;
			}
			if (index >= this.listElements.Count)
			{
				index = this.listElements.Count - 1;
			}
			else if (index < 0)
			{
				index = 0;
			}
			this.viewport.GetWorldCorners(this.jumpCornersCached);
			float num = this.jumpCornersCached[1].y - this.jumpCornersCached[0].y;
			this.scrollRect.content.GetWorldCorners(this.jumpCornersCached);
			float num2 = this.jumpCornersCached[1].y - this.jumpCornersCached[0].y;
			float num3 = num2 - num;
			float num4 = (1f - this.listElements[index].NormalizedLocalTop) * num2;
			float num5 = 1f - num4 / num3;
			if (num5 > 1f)
			{
				num5 = 1f;
			}
			if (num5 < 0f)
			{
				num5 = 0f;
			}
			this.scrollRect.normalizedPosition = new Vector2(0f, num5 + normalizedOffset);
		}

		// Token: 0x04002657 RID: 9815
		private const float OFF_SCREEN_Y = 9999999f;

		// Token: 0x04002658 RID: 9816
		public float Spacing;

		// Token: 0x04002659 RID: 9817
		public float PaddingTop;

		// Token: 0x0400265A RID: 9818
		public float PaddingBottom;

		// Token: 0x0400265B RID: 9819
		public bool SortChildTransforms = true;

		// Token: 0x0400265C RID: 9820
		private RectTransform rectTransform;

		// Token: 0x0400265D RID: 9821
		private Dictionary<string, ListElemTypeData> typeIdToData = new Dictionary<string, ListElemTypeData>();

		// Token: 0x0400265E RID: 9822
		private Dictionary<string, ListElemInstanceData> uniqueIdToInstanceData = new Dictionary<string, ListElemInstanceData>();

		// Token: 0x0400265F RID: 9823
		private List<ListElemInstanceData> listElements = new List<ListElemInstanceData>();

		// Token: 0x04002660 RID: 9824
		private bool isDrawing;

		// Token: 0x04002661 RID: 9825
		private RectTransform viewport;

		// Token: 0x04002662 RID: 9826
		private float contentHeight;

		// Token: 0x04002663 RID: 9827
		private float clampedPaddingTop;

		// Token: 0x04002664 RID: 9828
		private float clampedPaddingBottom;

		// Token: 0x04002665 RID: 9829
		private float contentHeightWithPadding;

		// Token: 0x04002666 RID: 9830
		private float prevNormalizedScrollRectTop = 1f;

		// Token: 0x04002667 RID: 9831
		private bool isLayoutDirty;

		// Token: 0x04002668 RID: 9832
		private int prevLowestVisibleIndex = int.MaxValue;

		// Token: 0x04002669 RID: 9833
		private bool scrolled;

		// Token: 0x0400266A RID: 9834
		private bool isUsingDynamicElementHeights;

		// Token: 0x0400266B RID: 9835
		private Vector3[] contentWorldCornersCached = new Vector3[4];

		// Token: 0x0400266C RID: 9836
		private Vector3[] viewportWorldCornersCached = new Vector3[4];

		// Token: 0x0400266D RID: 9837
		private Vector3[] jumpCornersCached = new Vector3[4];

		// Token: 0x0400266E RID: 9838
		private ScrollRect _scrollRect;
	}
}
