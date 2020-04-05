using System;
using System.Collections.Generic;

// Token: 0x020001D4 RID: 468
public class LeaderboardPositioning
{
	// Token: 0x1700012A RID: 298
	// (get) Token: 0x06000DCD RID: 3533 RVA: 0x0003DB08 File Offset: 0x0003BD08
	// (set) Token: 0x06000DCE RID: 3534 RVA: 0x0003DB10 File Offset: 0x0003BD10
	public List<LeaderboardItem> Leaderboard { get; private set; }

	// Token: 0x06000DCF RID: 3535 RVA: 0x0003DB19 File Offset: 0x0003BD19
	public LeaderboardPositioning(List<LeaderboardItem> leaderboard, string id)
	{
		this.Leaderboard = leaderboard;
		this.Id = id;
	}

	// Token: 0x04000BD7 RID: 3031
	public string Id;
}
