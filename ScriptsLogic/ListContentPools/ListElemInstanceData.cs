using System;
using UnityEngine;

namespace ListContentPools
{
	// Token: 0x02000708 RID: 1800
	public class ListElemInstanceData
	{
		// Token: 0x06002550 RID: 9552 RVA: 0x000A26DF File Offset: 0x000A08DF
		public ListElemInstanceData(string uniqueId, string typeId, int index, Rect boundsRect, bool isDefaultDimensions)
		{
			this.UniqueId = uniqueId;
			this.TypeId = typeId;
			this.Index = index;
			this.BoundsRect = boundsRect;
			this.IsDefaultDimensions = isDefaultDimensions;
		}

		// Token: 0x0400264A RID: 9802
		public string UniqueId;

		// Token: 0x0400264B RID: 9803
		public string TypeId;

		// Token: 0x0400264C RID: 9804
		public int Index;

		// Token: 0x0400264D RID: 9805
		public bool Updated;

		// Token: 0x0400264E RID: 9806
		public Rect BoundsRect;

		// Token: 0x0400264F RID: 9807
		public bool IsDefaultDimensions;

		// Token: 0x04002650 RID: 9808
		public float NormalizedLocalTop;

		// Token: 0x04002651 RID: 9809
		public float NormalizedLocalBottom;

		// Token: 0x04002652 RID: 9810
		public Component PooledComponent;
	}
}
