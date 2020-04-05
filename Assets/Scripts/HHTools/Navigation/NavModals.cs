using System;
using System.Reflection;

namespace HHTools.Navigation
{
	// Token: 0x020006E1 RID: 1761
	public class NavModals
	{
		// Token: 0x06002475 RID: 9333 RVA: 0x0009DB28 File Offset: 0x0009BD28
		public static NavModalVO<T> GetUIElementByName<T>(string elementName)
		{
			FieldInfo[] fields = typeof(NavModals).GetFields(BindingFlags.Static | BindingFlags.Public);
			NavModalVO<T> result = null;
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.Name == elementName)
				{
					result = (fieldInfo.GetValue(fieldInfo.Name) as NavModalVO<T>);
				}
			}
			return result;
		}

		// Token: 0x0400251B RID: 9499
		public static NavModalVO<AdventuresModalController> ADVENTURES_PANEL = new NavModalVO<AdventuresModalController>("AdventuresPanel", "Prefabs/Modals/");

		// Token: 0x0400251C RID: 9500
		public static NavModalVO<AdHocRewardPanel> AD_WATCH_AD_HOC_REWARD = new NavModalVO<AdHocRewardPanel>("AdWatchAdHocReward", "Prefabs/Modals/");

		// Token: 0x0400251D RID: 9501
		public static NavModalVO<AdProfitBoostModal> AD_WATCH_PROFIT_BOOST = new NavModalVO<AdProfitBoostModal>("AdWatchProfitBoost", "Prefabs/Modals/");

		// Token: 0x0400251E RID: 9502
		public static NavModalVO<WelcomeBackAdMultiplierView> AD_WATCH_WELCOME_BACK = new NavModalVO<WelcomeBackAdMultiplierView>("AdWatchWelcomeBack", "Prefabs/Modals/");

		// Token: 0x0400251F RID: 9503
		public static NavModalVO<AppOnboardModal> APP_ONBOARD = new NavModalVO<AppOnboardModal>("AppOnboard", "Prefabs/Modals/");

		// Token: 0x04002520 RID: 9504
		public static NavModalVO<BundleTakeoverModal> BUNDLE_TAKEOVER = new NavModalVO<BundleTakeoverModal>("BundleTakeover", "Prefabs/Modals/");

		// Token: 0x04002521 RID: 9505
		public static NavModalVO<FtbbTakoverModal> BUNDLE_TAKEOVER_FTBB = new NavModalVO<FtbbTakoverModal>("FTBBundleView", "Prefabs/Modals/");

		// Token: 0x04002522 RID: 9506
		public static NavModalVO<CareerModalController> CAREER = new NavModalVO<CareerModalController>("Career", "Prefabs/Modals/");

		// Token: 0x04002523 RID: 9507
		public static NavModalVO<CelebrationModal> CELEBRATION = new NavModalVO<CelebrationModal>("Celebration", "Prefabs/Modals/");

		// Token: 0x04002524 RID: 9508
		public static NavModalVOWithParams<ConfirmPurchasePopup, ConfirmPurchasePopup.ConfirmPurchaseParams> CONFIRM_PURCHASE_POPUP_CONFIRM_PURCHASE_PARAMS = new NavModalVOWithParams<ConfirmPurchasePopup, ConfirmPurchasePopup.ConfirmPurchaseParams>("ConfirmPurchasePopup", "Prefabs/Modals/");

		// Token: 0x04002525 RID: 9509
		public static NavModalVO<InfoModalController> CONNECT = new NavModalVO<InfoModalController>("Connect", "Prefabs/Modals/");

		// Token: 0x04002526 RID: 9510
		public static NavModalVO<EventIntroModal> EVENT_INTRO_MODAL = new NavModalVO<EventIntroModal>("EventIntroModal", "Prefabs/Modals/");

		// Token: 0x04002527 RID: 9511
		public static NavModalVO<EventMissionsModal> EVENT_MISSIONS_MODAL = new NavModalVO<EventMissionsModal>("EventMissionsModal", "Prefabs/Modals/");

		// Token: 0x04002528 RID: 9512
		public static NavModalVO<EventPromoModal> EVENT_PROMO = new NavModalVO<EventPromoModal>("EventPromo", "Prefabs/Modals/");

		// Token: 0x04002529 RID: 9513
		public static NavModalVO<GiftModal> GIFT_MODAL = new NavModalVO<GiftModal>("GiftModal", "Prefabs/Modals/");

		// Token: 0x0400252A RID: 9514
		public static NavModalVO<GildingPurchaseModal> GILDING_PURCHASE = new NavModalVO<GildingPurchaseModal>("GildingPurchase", "Prefabs/Modals/");

		// Token: 0x0400252B RID: 9515
		public static NavModalVO<HacksModal> HACKS = new NavModalVO<HacksModal>("Hacks", "Prefabs/Modals/");

		// Token: 0x0400252C RID: 9516
		public static NavModalVO<HardResetModal> HARD_RESET = new NavModalVO<HardResetModal>("HardReset", "Prefabs/Modals/");

		// Token: 0x0400252D RID: 9517
		public static NavModalVO<InfoPopup> INFO_POPUP = new NavModalVO<InfoPopup>("InfoPopup", "Prefabs/Modals/");

		// Token: 0x0400252E RID: 9518
		public static NavModalVO<InsufficientGoldStoreModal> INSUFFICIENT_GOLD_STORE = new NavModalVO<InsufficientGoldStoreModal>("InsufficientGoldStore", "Prefabs/Modals/");

		// Token: 0x0400252F RID: 9519
		public static NavModalVO<InsufficientMegaBucksModal> INSUFFICIENT_MEGA_BUCKS = new NavModalVO<InsufficientMegaBucksModal>("InsufficientMegaBucks", "Prefabs/Modals/");

		// Token: 0x04002530 RID: 9520
		public static NavModalVO<InvestorsModal> INVESTORS = new NavModalVO<InvestorsModal>("Investors", "Prefabs/Modals/");

		// Token: 0x04002531 RID: 9521
		public static NavModalVO<ItemDetailModal> ITEM_DETAIL = new NavModalVO<ItemDetailModal>("ItemDetail", "Prefabs/Modals/");

		// Token: 0x04002532 RID: 9522
		public static NavModalVO<LaunchingModal> LAUNCHING_MODAL = new NavModalVO<LaunchingModal>("LaunchingModal", "Prefabs/Modals/");

		// Token: 0x04002533 RID: 9523
		public static NavModalVO<LeaderboardRewardModal> LEADERBOARD_REWARD = new NavModalVO<LeaderboardRewardModal>("LeaderboardReward", "Prefabs/Modals/");

		// Token: 0x04002534 RID: 9524
		public static NavModalVO<LeaderboardRewardInfoModal> LEADERBOARD_REWARD_INFO = new NavModalVO<LeaderboardRewardInfoModal>("LeaderboardRewardInfo", "Prefabs/Modals/");

		// Token: 0x04002535 RID: 9525
		public static NavModalVO<LeaderboardModalController> LEADERBOARDS = new NavModalVO<LeaderboardModalController>("Leaderboards", "Prefabs/Modals/");

		// Token: 0x04002536 RID: 9526
		public static NavModalVO<LeaderboardModalController> LEADERBOARDS_SHORT = new NavModalVO<LeaderboardModalController>("LeaderboardsShort", "Prefabs/Modals/");

		// Token: 0x04002537 RID: 9527
		public static NavModalVO<LoadingModal> LOADING = new NavModalVO<LoadingModal>("Loading", "Prefabs/Modals/");

		// Token: 0x04002538 RID: 9528
		public static NavModalVO<ManagerModalController> MANAGERS = new NavModalVO<ManagerModalController>("Managers", "Prefabs/Modals/");

		// Token: 0x04002539 RID: 9529
		public static NavModalVO<PlanetUnlockModal> PLANET_UNLOCK = new NavModalVO<PlanetUnlockModal>("PlanetUnlock", "Prefabs/Modals/");

		// Token: 0x0400253A RID: 9530
		public static NavModalVO<PopupModal> POPUP = new NavModalVO<PopupModal>("Popup", "Prefabs/Modals/");

		// Token: 0x0400253B RID: 9531
		public static NavModalVO<PostCardModal> POST_CARD = new NavModalVO<PostCardModal>("PostCard", "Prefabs/Modals/");

		// Token: 0x0400253C RID: 9532
		public static NavModalVO<RateUsModal> RATE_US = new NavModalVO<RateUsModal>("RateUs", "Prefabs/Modals/");

		// Token: 0x0400253D RID: 9533
		public static NavModalVO<ServerMessageModal> SERVER_MESSAGE = new NavModalVO<ServerMessageModal>("ServerMessage", "Prefabs/Modals/");

		// Token: 0x0400253E RID: 9534
		public static NavModalVOWithParams<StoreModalController, StoreModalController.Parameters> STORE_PARAMETERS = new NavModalVOWithParams<StoreModalController, StoreModalController.Parameters>("Store", "Prefabs/Modals/");

		// Token: 0x0400253F RID: 9535
		public static NavModalVO<TimeWarpCelebrationPanel> TIME_WARP_CELEBRATION = new NavModalVO<TimeWarpCelebrationPanel>("TimeWarpCelebration", "Prefabs/Modals/");

		// Token: 0x04002540 RID: 9536
		public static NavModalVO<TimeWarpExpressPurchaseModal> TIMEWARP_EXPRESS_PURCHASE = new NavModalVO<TimeWarpExpressPurchaseModal>("TimewarpExpressPurchase", "Prefabs/Modals/");

		// Token: 0x04002541 RID: 9537
		public static NavModalVO<TintModal> TINT = new NavModalVO<TintModal>("Tint", "Prefabs/Modals/");

		// Token: 0x04002542 RID: 9538
		public static NavModalVO<UnlocksModalController> UNLOCKS = new NavModalVO<UnlocksModalController>("Unlocks", "Prefabs/Modals/");

		// Token: 0x04002543 RID: 9539
		public static NavModalVO<UpgradeModalController> UPGRADES = new NavModalVO<UpgradeModalController>("Upgrades", "Prefabs/Modals/");

		// Token: 0x04002544 RID: 9540
		public static NavModalVO<WelcomeBack> WELCOME_BACK = new NavModalVO<WelcomeBack>("WelcomeBack", "Prefabs/Modals/");

		// Token: 0x04002545 RID: 9541
		public static NavModalVO<AdProfitBoostModal> AD_PROFITS_START_PANEL = new NavModalVO<AdProfitBoostModal>("Ad_profits_start_panel", "Prefabs/UI/Ads/");

		// Token: 0x04002546 RID: 9542
		public static NavModalVO<InsufficientGoldStoreModal> RECOMMENDED_GOLD_STORE = new NavModalVO<InsufficientGoldStoreModal>("RecommendedGoldStore", "Prefabs/UI/PurchaseConfirmation/");
	}
}
