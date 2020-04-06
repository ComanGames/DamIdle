using System;

namespace ListContentPools
{
	// Token: 0x02000706 RID: 1798
	public struct ElementSizing
	{
		// Token: 0x0600254B RID: 9547 RVA: 0x000A24FD File Offset: 0x000A06FD
		public ElementSizing(HeightSizingType sizingType, float minHeight = 0f)
		{
			this.HeightSizingType = sizingType;
			this.MinHeight = minHeight;
		}

		// Token: 0x04002647 RID: 9799
		public HeightSizingType HeightSizingType;

		// Token: 0x04002648 RID: 9800
		public float MinHeight;
	}
}
