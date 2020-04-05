using System;
using System.Collections.Generic;

// Token: 0x020001D7 RID: 471
[Serializable]
public class LeaderboardRankContainer
{
	// Token: 0x06000DD1 RID: 3537 RVA: 0x0003DB2F File Offset: 0x0003BD2F
	public LeaderboardRankContainer()
	{
		this.historicEventData = new List<LeaderboardRankData>();
	}

	// Token: 0x04000BD8 RID: 3032
	public List<LeaderboardRankData> historicEventData;
}
