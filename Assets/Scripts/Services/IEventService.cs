using System;
using System.Collections.Generic;
using UniRx;
using Utils;

// Token: 0x0200008F RID: 143
public interface IEventService : IDisposable
{
	// Token: 0x17000064 RID: 100
	// (get) Token: 0x060003FD RID: 1021
	ReactiveProperty<bool> EventUnlocked { get; }

	// Token: 0x17000065 RID: 101
	// (get) Token: 0x060003FE RID: 1022
	ReactiveCollection<PendingEventRewards> PendingEventRewards { get; }

	// Token: 0x17000066 RID: 102
	// (get) Token: 0x060003FF RID: 1023
	ReactiveCollection<EventModel> ActiveEvents { get; }

	// Token: 0x17000067 RID: 103
	// (get) Token: 0x06000400 RID: 1024
	ReactiveCollection<EventModel> FutureEvents { get; }

	// Token: 0x17000068 RID: 104
	// (get) Token: 0x06000401 RID: 1025
	ReactiveCollection<EventModel> PastEvents { get; }

	// Token: 0x06000402 RID: 1026
	void Init(IGameController gameController, IEventDataService eventDataService, IEventServiceServerRequests eventServiceServerRequests, IDateTimeService dateTimeService, ITriggerService triggerService,  TimerService timerService, IUserDataService userDataService);

	// Token: 0x06000403 RID: 1027
	void OnRewardsClaimed(string eventId);

	// Token: 0x06000404 RID: 1028
	List<RewardData> GetRewardsFromTier(EventRewardTier rewardTier);

	// Token: 0x06000405 RID: 1029
	bool HasPlayerPerformedEventReset(string eventId);

	// Token: 0x06000406 RID: 1030
	void MarkAllEventsAsParticipatedCheat();
}
