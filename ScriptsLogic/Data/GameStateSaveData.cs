using System;
using System.Collections.Generic;

// Token: 0x02000154 RID: 340
public class GameStateSaveData
{
	// Token: 0x06000AC1 RID: 2753 RVA: 0x00030DEC File Offset: 0x0002EFEC
	public GameStateSaveData()
	{
	}

	// Token: 0x06000AC2 RID: 2754 RVA: 0x00030E88 File Offset: 0x0002F088
	public GameStateSaveData(GameState state, IAngelInvestorService angelInvestorService, List<VentureSaveData> ventures, List<UpgradeSaveData> upgrades, List<UpgradeSaveData> managers, List<UnlockSaveData> permanentUnlocks, List<PlanetMilestoneSaveData> milestones, List<LastClaimedUnlockSaveData> claimedUnlockThresholds)
	{
		this.timestamp = state.timestamp;
		this.ventures = ventures;
		this.upgrades = upgrades;
		this.managers = managers;
		this.permanentUnlocks = permanentUnlocks;
		this.claimedUnlockThresholds = claimedUnlockThresholds;
		this.planetMilestones = milestones;
		this.planetName = state.planetName;
		this.isMuted = GameController.Instance.IsMuted.Value;
		this.cashOnHand = state.CashOnHand.Value;
		this.sessionCash = state.SessionCash.Value;
		this.totalPreviousCash = state.TotalPreviousCash.Value;
		this.angelInvestors = angelInvestorService.AngelsOnHand.Value;
		this.angelInvestorsSpent = angelInvestorService.AngelsSpent.Value;
		this.angelResetCount = angelInvestorService.AngelResetCount.Value;
		this.neverRate = state.neverRate;
		this.managerRateOnce = state.managerRateOnce;
		this.videosAvailible = GameController.Instance.ProfitBoostAdService.AvailableProfitAds.Value;
		this.videosCurrentDay = GameController.Instance.ProfitBoostAdService.VideosCurrentDay;
		this.ProfitAdExpiry = GameController.Instance.ProfitBoostAdService.ProfitAdExpiry.ToString();
		this.highestMultiplierPurchased = state.HighestMultiplierPurchased.Value;
		this.hasProfitMartiansBeenRun = state.hasProfitMartiansBeenRun.Value;
		this.hasUserClaimedFreeGold = state.hasUserClaimedFreeGold;
	}

	// Token: 0x040008EF RID: 2287
	public List<VentureSaveData> ventures;

	// Token: 0x040008F0 RID: 2288
	public List<UpgradeSaveData> upgrades;

	// Token: 0x040008F1 RID: 2289
	public List<UpgradeSaveData> managers;

	// Token: 0x040008F2 RID: 2290
	public List<UnlockSaveData> permanentUnlocks = new List<UnlockSaveData>();

	// Token: 0x040008F3 RID: 2291
	public List<LastClaimedUnlockSaveData> claimedUnlockThresholds = new List<LastClaimedUnlockSaveData>();

	// Token: 0x040008F4 RID: 2292
	public List<PlanetMilestoneSaveData> planetMilestones = new List<PlanetMilestoneSaveData>();

	// Token: 0x040008F5 RID: 2293
	public double planetMilestoneScore;

	// Token: 0x040008F6 RID: 2294
	public string planetName;

	// Token: 0x040008F7 RID: 2295
	public double cashOnHand;

	// Token: 0x040008F8 RID: 2296
	public double sessionCash;

	// Token: 0x040008F9 RID: 2297
	public double totalPreviousCash;

	// Token: 0x040008FA RID: 2298
	public bool isMuted;

	// Token: 0x040008FB RID: 2299
	public double timestamp;

	// Token: 0x040008FC RID: 2300
	public double angelInvestors;

	// Token: 0x040008FD RID: 2301
	public double angelInvestorsSpent;

	// Token: 0x040008FE RID: 2302
	public int angelResetCount;

	// Token: 0x040008FF RID: 2303
	public int neverRate;

	// Token: 0x04000900 RID: 2304
	public int managerRateOnce;

	// Token: 0x04000901 RID: 2305
	public int videosAvailible = 5;

	// Token: 0x04000902 RID: 2306
	public int videosCurrentDay = -1;

	// Token: 0x04000903 RID: 2307
	public string ProfitAdExpiry;

	// Token: 0x04000904 RID: 2308
	public string AppOnBoardBoostExpiry;

	// Token: 0x04000905 RID: 2309
	public int AppOnboardAdsRemaining;

	// Token: 0x04000906 RID: 2310
	public int AppOnBoardLastWatchDay;

	// Token: 0x04000907 RID: 2311
	public double videoStartTime;

	// Token: 0x04000908 RID: 2312
	public bool facebookMultiplierActive;

	// Token: 0x04000909 RID: 2313
	public double facebookBonusStartTime;

	// Token: 0x0400090A RID: 2314
	public double facebookBonusOfflineTime;

	// Token: 0x0400090B RID: 2315
	public double facebookBonusTimer;

	// Token: 0x0400090C RID: 2316
	public int highestMultiplierPurchased;

	// Token: 0x0400090D RID: 2317
	public bool hasProfitMartiansBeenRun;

	// Token: 0x0400090E RID: 2318
	public bool hasUserClaimedFreeGold;

	// Token: 0x0400090F RID: 2319
	public Dictionary<string, int> eventItems = new Dictionary<string, int>();

	// Token: 0x04000910 RID: 2320
	public Dictionary<string, int> eventItemMax = new Dictionary<string, int>();

	// Token: 0x04000911 RID: 2321
	public Dictionary<string, int> unopendGifts = new Dictionary<string, int>();

	// Token: 0x04000912 RID: 2322
	public Dictionary<string, int> unclickedItems = new Dictionary<string, int>();

	// Token: 0x04000913 RID: 2323
	public Dictionary<string, int> eventGifts = new Dictionary<string, int>();

	// Token: 0x04000914 RID: 2324
	public Dictionary<string, int> eventGiftsItemAmounts = new Dictionary<string, int>();

	// Token: 0x04000915 RID: 2325
	public Dictionary<string, int> holidayVentureGifts = new Dictionary<string, int>();

	// Token: 0x04000916 RID: 2326
	public Dictionary<string, BadgeInfo> eventBadges = new Dictionary<string, BadgeInfo>();
}
