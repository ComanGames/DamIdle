using System;
using System.Collections.Generic;

// Token: 0x02000089 RID: 137
[Serializable]
public class EventRewardTier
{
	// Token: 0x04000372 RID: 882
	public string tierId;

	// Token: 0x04000373 RID: 883
	public string tierName;

	// Token: 0x04000374 RID: 884
	public int topRank;

	// Token: 0x04000375 RID: 885
	public int bottomRank;

	// Token: 0x04000376 RID: 886
	public string banner;

	// Token: 0x04000377 RID: 887
	public string trophyName;

	// Token: 0x04000378 RID: 888
	public List<EventRewardItem> leaderboardRewardItems = new List<EventRewardItem>();
}
