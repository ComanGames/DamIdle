using System;
using System.Collections.Generic;

// Token: 0x02000104 RID: 260
public class TutorialBlock
{
	// Token: 0x0400066F RID: 1647
	public string BlockId;

	// Token: 0x04000670 RID: 1648
	public string Platforms;

	// Token: 0x04000671 RID: 1649
	public string ABTestGroup;

	// Token: 0x04000672 RID: 1650
	public string PlanetId;

	// Token: 0x04000673 RID: 1651
	public List<TriggerData> StartTriggers = new List<TriggerData>();

	// Token: 0x04000674 RID: 1652
	public List<TriggerData> SkipTriggers = new List<TriggerData>();

	// Token: 0x04000675 RID: 1653
	public List<TriggerData> ResetTriggers = new List<TriggerData>();
}
