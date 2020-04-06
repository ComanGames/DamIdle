using System;
using System.Collections.Generic;

// Token: 0x0200010B RID: 267
[Serializable]
public class UnfoldingData
{
	// Token: 0x0400069D RID: 1693
	public string ABTestGroup;

	// Token: 0x0400069E RID: 1694
	public string StepId;

	// Token: 0x0400069F RID: 1695
	public string Target;

	// Token: 0x040006A0 RID: 1696
	public string Platforms;

	// Token: 0x040006A1 RID: 1697
	public UnfoldingData.EUnfoldingType Type = UnfoldingData.EUnfoldingType.ShowUI;

	// Token: 0x040006A2 RID: 1698
	public List<TriggerData> TriggerDatas = new List<TriggerData>();

	// Token: 0x02000804 RID: 2052
	public enum EUnfoldingType
	{
		// Token: 0x0400295D RID: 10589
		Defualt,
		// Token: 0x0400295E RID: 10590
		ShowUI,
		// Token: 0x0400295F RID: 10591
		ShowVenture,
		// Token: 0x04002960 RID: 10592
		UnlockVenture
	}
}
