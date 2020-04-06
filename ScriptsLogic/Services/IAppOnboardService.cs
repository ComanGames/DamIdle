using System;
using System.Collections.Generic;
using UniRx;
using Utils;

// Token: 0x02000041 RID: 65
public interface IAppOnboardService : IDisposable
{
	// Token: 0x17000011 RID: 17
	// (get) Token: 0x06000175 RID: 373
	// (set) Token: 0x06000176 RID: 374
	DateTime BoostExpiry { get; set; }

	// Token: 0x17000012 RID: 18
	// (get) Token: 0x06000177 RID: 375
	// (set) Token: 0x06000178 RID: 376
	int LastBoostWatchedDay { get; set; }

	// Token: 0x17000013 RID: 19
	// (get) Token: 0x06000179 RID: 377
	ReactiveProperty<double> TimeRemaining { get; }

	// Token: 0x17000014 RID: 20
	// (get) Token: 0x0600017A RID: 378
	ReactiveProperty<bool> AdAvailable { get; }

	// Token: 0x17000015 RID: 21
	// (get) Token: 0x0600017B RID: 379
	ReactiveProperty<int> DailyAdsRemaining { get; }

	// Token: 0x17000016 RID: 22
	// (get) Token: 0x0600017C RID: 380
	List<string> PlanetIds { get; }

	// Token: 0x17000017 RID: 23
	// (get) Token: 0x0600017D RID: 381
	int MaxAds { get; }

	// Token: 0x17000018 RID: 24
	// (get) Token: 0x0600017E RID: 382
	int BoostMultiplier { get; }

	// Token: 0x17000019 RID: 25
	// (get) Token: 0x0600017F RID: 383
	int BoostDurationHours { get; }

	// Token: 0x06000180 RID: 384
	void Init(IGameController gameController, ProfitBoostAdService profitBoostAdService, IDateTimeService dateTimeService, TimerService timerService, ITriggerService triggerService, List<AppOnboardConfig> configs);

	// Token: 0x06000181 RID: 385
	void OnAdComplete();

	// Token: 0x06000182 RID: 386
	bool IsButtonAvailable();
}
