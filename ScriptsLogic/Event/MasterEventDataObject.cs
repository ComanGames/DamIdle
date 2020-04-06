using System;
using System.Collections.Generic;

// Token: 0x02000085 RID: 133
[Serializable]
public class MasterEventDataObject
{
	// Token: 0x04000357 RID: 855
	public List<EventData> events;

	// Token: 0x04000358 RID: 856
	public List<EventRewardTiers> tiers;

	// Token: 0x04000359 RID: 857
	public List<EventRewardTierRewards> rewards;
}
