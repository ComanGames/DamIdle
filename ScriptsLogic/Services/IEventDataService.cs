using System;
using System.Collections.Generic;
using UniRx;

// Token: 0x02000084 RID: 132
public interface IEventDataService : IDisposable
{
	// Token: 0x17000061 RID: 97
	// (get) Token: 0x060003F0 RID: 1008
	ReactiveCollection<EventData> EventDataList { get; }

	// Token: 0x17000062 RID: 98
	// (get) Token: 0x060003F1 RID: 1009
	EventCatalogEntry CatalogEntry { get; }

	// Token: 0x17000063 RID: 99
	// (get) Token: 0x060003F2 RID: 1010
	bool WasEventScheduleDownloadedSuccessfully { get; }

	// Token: 0x060003F3 RID: 1011
	void Init(IEventServiceServerRequests eventServiceServerRequests, Action onSucces, Action<string> onError);

	// Token: 0x060003F4 RID: 1012
	List<EventRewardTier> GetLeaderBoardTiersForEvent(string eventID);

	// Token: 0x060003F5 RID: 1013
	EventRewardTier GetRewardTierByRank(string eventID, double rank);
}
