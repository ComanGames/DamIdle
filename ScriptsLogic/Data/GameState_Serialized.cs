using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000157 RID: 343
[CreateAssetMenu(fileName = "GameState", menuName = "AdVentureCapitalist/GameState", order = 1)]
[Serializable]
public class GameState_Serialized : ScriptableObject
{
	// Token: 0x0400091E RID: 2334
	public bool hasLoaded;

	// Token: 0x0400091F RID: 2335
	public string planetName = "";

	// Token: 0x04000920 RID: 2336
	public string planetTheme = "";

	// Token: 0x04000921 RID: 2337
	public string planetTitle = "";

	// Token: 0x04000922 RID: 2338
	public string currencyName = "";

	// Token: 0x04000923 RID: 2339
	public double megaBucksExchangeBaseCost;

	// Token: 0x04000924 RID: 2340
	public double megaBucksExchangeRatePercent;

	// Token: 0x04000925 RID: 2341
	public double angelAccumulationRate;

	// Token: 0x04000926 RID: 2342
	public PlanetProgressionType progressionType;

	// Token: 0x04000927 RID: 2343
	public bool dockPortraitProfitBooster;

	// Token: 0x04000928 RID: 2344
	[Header("Ventures")]
	public List<Venture> planetVentures = new List<Venture>();

	// Token: 0x04000929 RID: 2345
	[Header("Managers")]
	public List<OrderLogic> managerOrder = new List<OrderLogic>();

	// Token: 0x0400092A RID: 2346
	public List<RunVentureManager> runManagers = new List<RunVentureManager>();

	// Token: 0x0400092B RID: 2347
	public List<AccountantManager> accountManagers = new List<AccountantManager>();

	// Token: 0x0400092C RID: 2348
	public List<ManagerFeature> featureManagers = new List<ManagerFeature>();

	// Token: 0x0400092D RID: 2349
	[Header("Unlocks")]
	public List<OrderLogic> unlockOrder = new List<OrderLogic>();

	// Token: 0x0400092E RID: 2350
	public List<RewardSerialInformation> rewardSerialInformation = new List<RewardSerialInformation>();

	// Token: 0x0400092F RID: 2351
	public List<SingleVentureUnlock> singleUnlocks = new List<SingleVentureUnlock>();

	// Token: 0x04000930 RID: 2352
	public List<EveryVentureUnlock> everyUnlocks = new List<EveryVentureUnlock>();

	// Token: 0x04000931 RID: 2353
	public List<EventUnlock> eventUnlocks = new List<EventUnlock>();

	// Token: 0x04000932 RID: 2354
	[Header("Upgrades")]
	public List<OrderLogic> upgradeOrder = new List<OrderLogic>();

	// Token: 0x04000933 RID: 2355
	public List<VentureUpgrade> ventureUpgrades = new List<VentureUpgrade>();

	// Token: 0x04000934 RID: 2356
	public List<AIUpgrade> aiUpgrades = new List<AIUpgrade>();

	// Token: 0x04000935 RID: 2357
	public List<BuyVenturesUpgrade> buyVentureUpgrades = new List<BuyVenturesUpgrade>();

	// Token: 0x04000936 RID: 2358
	public List<EverythingUpgrade> everythingUpgrades = new List<EverythingUpgrade>();

	// Token: 0x04000937 RID: 2359
	[Header("Planet Milestones")]
	public List<PlanetMilestone> planetMilestones = new List<PlanetMilestone>();

	// Token: 0x04000938 RID: 2360
	[Header("Missions")]
	public List<EventMissionContants> missionConstants = new List<EventMissionContants>();

	// Token: 0x04000939 RID: 2361
	public List<EventMissionDifficulty> missionDifficulties = new List<EventMissionDifficulty>();

	// Token: 0x0400093A RID: 2362
	public List<EventMissionDayBonus> missionDayBonuses = new List<EventMissionDayBonus>();

	// Token: 0x0400093B RID: 2363
	public List<EventMissionTaskTypeWeights> missionTaskTypeWeights = new List<EventMissionTaskTypeWeights>();
}
