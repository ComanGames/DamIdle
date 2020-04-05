using System;
using System.Collections.Generic;
using AdCap.Store;
using UniRx;
using Utils;

// Token: 0x0200005E RID: 94
public interface IEventMissionService : IDisposable
{
	// Token: 0x0600032D RID: 813
	void Init(IGameController gameController, IDateTimeService dateTimeService, TimerService timerService, IAngelInvestorService angelInvestorService, IStoreService storeService, UpgradeService upgradeService, IEventService eventService, IAnalyticService analyticService, ProfitBoostAdService profitBoostAdService);

	// Token: 0x0600032E RID: 814
	bool RefreshTasksForGold();

	// Token: 0x0600032F RID: 815
	bool ClearMission(UserEventMission mission);

	// Token: 0x06000330 RID: 816
	bool ClaimMissionReward(UserEventMission mission);

	// Token: 0x06000331 RID: 817
	string GetMissionDescription(UserEventMission mission);

	// Token: 0x1700003F RID: 63
	// (get) Token: 0x06000332 RID: 818
	ReactiveCollection<UserEventMission> currentMissions { get; }

	// Token: 0x17000040 RID: 64
	// (get) Token: 0x06000333 RID: 819
	ReactiveProperty<int> CurentScore { get; }

	// Token: 0x17000041 RID: 65
	// (get) Token: 0x06000334 RID: 820
	ReactiveProperty<AdCapStoreItem> RefreshStoreItem { get; }

	// Token: 0x17000042 RID: 66
	// (get) Token: 0x06000335 RID: 821
	Dictionary<int, ReactiveProperty<double>> MissionTimersById { get; }
}
