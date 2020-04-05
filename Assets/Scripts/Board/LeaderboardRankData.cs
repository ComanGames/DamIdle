using System;

// Token: 0x020001CA RID: 458
[Serializable]
public class LeaderboardRankData
{
	// Token: 0x06000D99 RID: 3481 RVA: 0x0003CB73 File Offset: 0x0003AD73
	public LeaderboardRankData(string eventId, double leaderboardRank)
	{
		this.eventId = eventId;
		this.leaderboardRank = leaderboardRank;
	}

	// Token: 0x06000D9A RID: 3482 RVA: 0x0003CB89 File Offset: 0x0003AD89
	public override string ToString()
	{
		return string.Format("EventId:{0}\nRank:{1}\nScore:{2}", this.eventId, this.leaderboardRank, this.score);
	}

	// Token: 0x04000B9D RID: 2973
	public string eventId;

	// Token: 0x04000B9E RID: 2974
	public double leaderboardRank;

	// Token: 0x04000B9F RID: 2975
	public double score;
}
