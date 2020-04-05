using System;
using AdCap;
using AdCap.Ads;
using AdCap.Redemption;
using AdCap.Store;
using Platforms;
using Utils;

// Token: 0x0200015C RID: 348
public interface IGameController : IDisposable
{
    // Token: 0x14000061 RID: 97
    // (add) Token: 0x06000AD0 RID: 2768
    // (remove) Token: 0x06000AD1 RID: 2769
    event Action OnSoftResetPre;

    // Token: 0x14000062 RID: 98
    // (add) Token: 0x06000AD2 RID: 2770
    // (remove) Token: 0x06000AD3 RID: 2771
    event Action OnSoftResetPost;

    // Token: 0x14000063 RID: 99
    // (add) Token: 0x06000AD4 RID: 2772
    // (remove) Token: 0x06000AD5 RID: 2773
    event Action OnHardResetPre;

    // Token: 0x14000064 RID: 100
    // (add) Token: 0x06000AD6 RID: 2774
    // (remove) Token: 0x06000AD7 RID: 2775
    event Action OnHardResetPost;

    // Token: 0x14000065 RID: 101
    // (add) Token: 0x06000AD8 RID: 2776
    // (remove) Token: 0x06000AD9 RID: 2777
    event Action OnLoadNewPlanetPre;

    // Token: 0x14000066 RID: 102
    // (add) Token: 0x06000ADA RID: 2778
    // (remove) Token: 0x06000ADB RID: 2779
    event Action OnLoadNewPlanetPost;

    // Token: 0x14000067 RID: 103
    // (add) Token: 0x06000ADC RID: 2780
    // (remove) Token: 0x06000ADD RID: 2781
    event Action OnPreSavePlanetData;

    // Token: 0x170000D4 RID: 212
    // (get) Token: 0x06000ADE RID: 2782
    IUserDataService UserDataService { get; }

    // Token: 0x170000D5 RID: 213
    // (get) Token: 0x06000ADF RID: 2783
    UpgradeService UpgradeService { get; }

    // Token: 0x170000D6 RID: 214
    // (get) Token: 0x06000AE0 RID: 2784
    UnlockService UnlockService { get; }

    // Token: 0x170000D7 RID: 215
    // (get) Token: 0x06000AE1 RID: 2785
    IEventService EventService { get; }

    // Token: 0x170000D8 RID: 216
    // (get) Token: 0x06000AE2 RID: 2786
    IEventDataService EventDataService { get; }

    // Token: 0x170000D9 RID: 217
    // (get) Token: 0x06000AE3 RID: 2787
    IDateTimeService DateTimeService { get; }

    // Token: 0x170000DA RID: 218
    // (get) Token: 0x06000AE4 RID: 2788
    DataService DataService { get; }

    // Token: 0x170000DB RID: 219
    // (get) Token: 0x06000AE5 RID: 2789
    TimerService TimerService { get; }

    // Token: 0x170000DC RID: 220
    // (get) Token: 0x06000AE6 RID: 2790
    NavigationService NavigationService { get; }

    // Token: 0x170000DD RID: 221
    // (get) Token: 0x06000AE7 RID: 2791
    GildingService GildingService { get; }

    // Token: 0x170000DE RID: 222
    // (get) Token: 0x06000AE8 RID: 2792
    TimeWarpService TimeWarpService { get; }

    // Token: 0x170000DF RID: 223
    // (get) Token: 0x06000AE9 RID: 2793
    IStoreService StoreService { get; }

    // Token: 0x170000E0 RID: 224
    // (get) Token: 0x06000AEA RID: 2794
    ITriggerService TriggerService { get; }

    // Token: 0x170000E1 RID: 225
    // (get) Token: 0x06000AEB RID: 2795
    SubscriptionService SubscriptionService { get; }

    // Token: 0x170000E2 RID: 226
    // (get) Token: 0x06000AEC RID: 2796
    AdWatchService AdWatchService { get; }

    // Token: 0x170000E3 RID: 227
    // (get) Token: 0x06000AED RID: 2797
    ProfitBoostAdService ProfitBoostAdService { get; }

    // Token: 0x170000E4 RID: 228
    // (get) Token: 0x06000AEE RID: 2798
    AdHocRewardService AdHocRewardService { get; }

    // Token: 0x170000E5 RID: 229
    // (get) Token: 0x06000AEF RID: 2799
    OfferwallService OfferwallService { get; }

    // Token: 0x170000E6 RID: 230
    // (get) Token: 0x06000AF0 RID: 2800
    RedemptionService RedemptionService { get; }

    // Token: 0x170000E7 RID: 231
    // (get) Token: 0x06000AF1 RID: 2801
    NewsFeedService NewsFeedService { get; }

    // Token: 0x170000E8 RID: 232
    // (get) Token: 0x06000AF2 RID: 2802
    MicroManagerService MicroManagerService { get; }

    // Token: 0x170000E9 RID: 233
    // (get) Token: 0x06000AF3 RID: 2803
    IAppOnboardService AppOnboardService { get; }

    // Token: 0x170000EA RID: 234
    // (get) Token: 0x06000AF4 RID: 2804
    IAngelInvestorService AngelService { get; }

    // Token: 0x170000EB RID: 235
    // (get) Token: 0x06000AF5 RID: 2805
    PlatformAchievementService PlatformAchievementService { get; }

    // Token: 0x170000EC RID: 236
    // (get) Token: 0x06000AF6 RID: 2806
    ServerMessageService ServerMessageService { get; }

    // Token: 0x170000ED RID: 237
    // (get) Token: 0x06000AF7 RID: 2807
    LeaderboardService LeaderboardService { get; }

    // Token: 0x170000EE RID: 238
    // (get) Token: 0x06000AF8 RID: 2808
    PlanetMilestoneService PlanetMilestoneService { get; }

    // Token: 0x170000EF RID: 239
    // (get) Token: 0x06000AF9 RID: 2809
    IEventMissionService EventMissionsService { get; }

    // Token: 0x170000F0 RID: 240
    // (get) Token: 0x06000AFA RID: 2810
    IGrantRewardService GrantRewardService { get; }

    // Token: 0x170000F1 RID: 241
    // (get) Token: 0x06000AFB RID: 2811
    TutorialService TutorialService { get; }

    // Token: 0x170000F2 RID: 242
    // (get) Token: 0x06000AFC RID: 2812
    IAnalyticService AnalyticService { get; }

    // Token: 0x170000F3 RID: 243
    // (get) Token: 0x06000AFD RID: 2813
    PlatformAccount PlatformAccount { get; }

    // Token: 0x170000F4 RID: 244
    // (get) Token: 0x06000AFE RID: 2814
    UnfoldingService UnfoldingService { get; }

    // Token: 0x170000F5 RID: 245
    // (get) Token: 0x06000AFF RID: 2815
    BuyMultiplierService BuyMultiplierService { get; }

    // Token: 0x170000F6 RID: 246
    // (get) Token: 0x06000B00 RID: 2816
    GiftService GiftService { get; }

    // Token: 0x170000F7 RID: 247
    // (get) Token: 0x06000B01 RID: 2817
    GameState game { get; }

    // Token: 0x170000F8 RID: 248
    // (get) Token: 0x06000B02 RID: 2818
    IObservable<GameState> State { get; }

    // Token: 0x170000F9 RID: 249
    // (get) Token: 0x06000B03 RID: 2819
    PlayerData GlobalPlayerData { get; }

    // Token: 0x170000FA RID: 250
    // (get) Token: 0x06000B04 RID: 2820
    string PlatformId { get; }

    // Token: 0x06000B05 RID: 2821
    void ResetGame(bool isAdwatch = false);
}
