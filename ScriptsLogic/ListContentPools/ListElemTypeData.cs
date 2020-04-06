using System;
using UnityEngine;

namespace ListContentPools
{
	// Token: 0x02000709 RID: 1801
	public class ListElemTypeData
	{
		// Token: 0x06002551 RID: 9553 RVA: 0x000A270C File Offset: 0x000A090C
		public ListElemTypeData(ComponentPool componentPool, Rect defaultBounds, Action<string, Component> initHandler, Action<string, int, Component> topElementHandler = null)
		{
			this.ComponentPool = componentPool;
			this.DefaultBounds = defaultBounds;
			this.InitHandler = initHandler;
			this.TopElementHandler = topElementHandler;
		}

		// Token: 0x04002653 RID: 9811
		public ComponentPool ComponentPool;

		// Token: 0x04002654 RID: 9812
		public Rect DefaultBounds;

		// Token: 0x04002655 RID: 9813
		public Action<string, Component> InitHandler;

		// Token: 0x04002656 RID: 9814
		public Action<string, int, Component> TopElementHandler;
	}
}
