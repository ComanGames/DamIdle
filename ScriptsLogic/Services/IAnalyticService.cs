using System;
using System.Collections.Generic;

// Token: 0x02000046 RID: 70
public interface IAnalyticService : IDisposable
{
	// Token: 0x0600019D RID: 413
	void Init(IGameController gameController);

	// Token: 0x0600019E RID: 414
	void InitializeAnalyticsHooks();

	// Token: 0x1700001B RID: 27
	// (get) Token: 0x0600019F RID: 415
	// (set) Token: 0x060001A0 RID: 416
	bool SendAnalyticsToPlayfab { get; set; }

	// Token: 0x060001A1 RID: 417
	void SendTaskCompleteEvent(string taskId = "", string taskType = "", string taskDescription = "");

	// Token: 0x060001A2 RID: 418
	void SendNavActionAnalytics(string navElementName, string origin, string result);

	// Token: 0x060001A3 RID: 419
	void SendSwagAcquired(string swagName, int previousLevel, int addedLevels, int rarity);

	// Token: 0x060001A4 RID: 420
	void SendAdStartEvent(string origin, string adType, string contextOfOffer);

	// Token: 0x060001A5 RID: 421
	void SendAdFinished(string origin, string adType, string contextOfOffer, string reward = "");

	// Token: 0x060001A6 RID: 422
	void SendFTUEAnalyticEvent(string stepDescription, int step);

	// Token: 0x060001A7 RID: 423
	void SendUpgradeAnalytic(Upgrade upgrade);

	// Token: 0x060001A8 RID: 424
	void SendAdsAvailableChangedEvent(bool isAvialable);

	// Token: 0x060001A9 RID: 425
	void SendAdjustAnalyticEvent(string key, Dictionary<string, object> data = null);

	// Token: 0x060001AA RID: 426
	void SendVentureBoostedAnalytic(VentureModel model);

	// Token: 0x060001AB RID: 427
	void SendShortLeaderboardError(int errorNumber, string errorMessage);

	// Token: 0x060001AC RID: 428
	void SendPlatniumUpgradeFixedEvent(string planetName, int venturesReset);

	// Token: 0x060001AD RID: 429
	void SendMissionEvent(string analyticId, UserEventMission mission = null, string description = "");

	// Token: 0x060001AE RID: 430
	void SendMilestoneEvent(string analyticId, UserPlanetMilestone milestone = null, string description = "");

	// Token: 0x060001AF RID: 431
	void SendFailedToGenerateMissionTask(string eventId, string cashToSpend, int taskCompletes);

	// Token: 0x060001B0 RID: 432
	void SendCampaignEvent(string ddna_event_id);

	// Token: 0x060001B1 RID: 433
	void SendTutorialEvent(string stepId, int stepNumber, bool isCompleted);
}
