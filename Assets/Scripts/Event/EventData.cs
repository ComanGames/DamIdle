using System;
using UnityEngine;

// Token: 0x02000086 RID: 134
[Serializable]
public class EventData
{
	// Token: 0x0400035A RID: 858
	public string id;

	// Token: 0x0400035B RID: 859
	public string name;

	// Token: 0x0400035C RID: 860
	public DateTime startDate;

	// Token: 0x0400035D RID: 861
	public LeaderboardType leaderboardType = LeaderboardType.Playfab;

	// Token: 0x0400035E RID: 862
	public int leaderboardSize = 200;

	// Token: 0x0400035F RID: 863
	public DateTime endDate;

	// Token: 0x04000360 RID: 864
	public string planetTheme;

	// Token: 0x04000361 RID: 865
	public string featureTutorialText;

	// Token: 0x04000362 RID: 866
	public string featureTutorialImageURL;

	// Token: 0x04000363 RID: 867
	public int unlockCount;

	// Token: 0x04000364 RID: 868
	public Color tintColor;

	// Token: 0x04000365 RID: 869
	public bool hasLeaderboard;

	// Token: 0x04000366 RID: 870
	public string abTestGroup;

	// Token: 0x04000367 RID: 871
	public string promoTitle;

	// Token: 0x04000368 RID: 872
	public string promoBody;

	// Token: 0x04000369 RID: 873
	public PlanetProgressionType progressionType;
}
