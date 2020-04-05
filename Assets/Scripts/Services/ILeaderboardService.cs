using System;
using System.Collections.Generic;

// Token: 0x020001C9 RID: 457
public interface ILeaderboardService : IDisposable
{
	// Token: 0x06000D93 RID: 3475
	IObservable<LeaderboardRankData> GetPlayerRank(string leaderboardId, LeaderboardType leaderboardType, bool allowFromCache = true);

	// Token: 0x06000D94 RID: 3476
	IObservable<List<LeaderboardItem>> GetLeaderboardAroundPlayer(string leaderboardId, LeaderboardType leaderboardType, int maxCount = 100);

	// Token: 0x06000D95 RID: 3477
	IObservable<List<LeaderboardItem>> GetLeaderboardTop100(string leaderboardId, LeaderboardType leaderboardType);

	// Token: 0x06000D96 RID: 3478
	IObservable<Tuple<string, int>> PostLeaderboardValue(string leaderboardId, LeaderboardType leaderboardType, int leaderboardSize, int value);

	// Token: 0x06000D97 RID: 3479
	void Init(IGameController gameController, ILeaderboardPlatform leaderboardPlatform);

	// Token: 0x06000D98 RID: 3480
	int GetLeaderBoardToken();
}
