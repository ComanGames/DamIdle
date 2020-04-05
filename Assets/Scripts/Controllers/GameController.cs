using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AdCap;
using AdCap.Redemption;
using AdCap.Store;
using AdComm;
using Assets.Scripts.Utils;
using HHTools.Navigation;
using Platforms;
using Platforms.Ad;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

// Token: 0x0200014D RID: 333
public sealed class GameController : IGameController, IDisposable
{
    // Token: 0x14000059 RID: 89
    // (add) Token: 0x060009E6 RID: 2534 RVA: 0x0002CAA8 File Offset: 0x0002ACA8
    // (remove) Token: 0x060009E7 RID: 2535 RVA: 0x0002CAE0 File Offset: 0x0002ACE0
    public event Action OnSoftResetPre = delegate ()
    {
    };

    // Token: 0x1400005A RID: 90
    // (add) Token: 0x060009E8 RID: 2536 RVA: 0x0002CB18 File Offset: 0x0002AD18
    // (remove) Token: 0x060009E9 RID: 2537 RVA: 0x0002CB50 File Offset: 0x0002AD50
    public event Action OnSoftResetPost = delegate ()
    {
    };

    // Token: 0x1400005B RID: 91
    // (add) Token: 0x060009EA RID: 2538 RVA: 0x0002CB88 File Offset: 0x0002AD88
    // (remove) Token: 0x060009EB RID: 2539 RVA: 0x0002CBC0 File Offset: 0x0002ADC0
    public event Action OnHardResetPre = delegate ()
    {
    };

    // Token: 0x1400005C RID: 92
    // (add) Token: 0x060009EC RID: 2540 RVA: 0x0002CBF8 File Offset: 0x0002ADF8
    // (remove) Token: 0x060009ED RID: 2541 RVA: 0x0002CC30 File Offset: 0x0002AE30
    public event Action OnHardResetPost = delegate ()
    {
    };

    // Token: 0x1400005D RID: 93
    // (add) Token: 0x060009EE RID: 2542 RVA: 0x0002CC68 File Offset: 0x0002AE68
    // (remove) Token: 0x060009EF RID: 2543 RVA: 0x0002CCA0 File Offset: 0x0002AEA0
    public event Action OnLoadNewPlanetPre = delegate ()
    {
    };

    // Token: 0x1400005E RID: 94
    // (add) Token: 0x060009F0 RID: 2544 RVA: 0x0002CCD8 File Offset: 0x0002AED8
    // (remove) Token: 0x060009F1 RID: 2545 RVA: 0x0002CD10 File Offset: 0x0002AF10
    public event Action OnLoadNewPlanetPost = delegate ()
    {
    };

    // Token: 0x1400005F RID: 95
    // (add) Token: 0x060009F2 RID: 2546 RVA: 0x0002CD48 File Offset: 0x0002AF48
    // (remove) Token: 0x060009F3 RID: 2547 RVA: 0x0002CD80 File Offset: 0x0002AF80
    public event Action OnPreSavePlanetData = delegate ()
    {
    };

    // Token: 0x1700009A RID: 154
    // (get) Token: 0x060009F4 RID: 2548 RVA: 0x0002CDB5 File Offset: 0x0002AFB5
    // (set) Token: 0x060009F5 RID: 2549 RVA: 0x0002CDBD File Offset: 0x0002AFBD
    public IUserDataService UserDataService { get; private set; }

    // Token: 0x1700009B RID: 155
    // (get) Token: 0x060009F6 RID: 2550 RVA: 0x0002CDC6 File Offset: 0x0002AFC6
    // (set) Token: 0x060009F7 RID: 2551 RVA: 0x0002CDCE File Offset: 0x0002AFCE
    public UpgradeService UpgradeService { get; private set; }

    // Token: 0x1700009C RID: 156
    // (get) Token: 0x060009F8 RID: 2552 RVA: 0x0002CDD7 File Offset: 0x0002AFD7
    // (set) Token: 0x060009F9 RID: 2553 RVA: 0x0002CDDF File Offset: 0x0002AFDF
    public UnlockService UnlockService { get; private set; }

    // Token: 0x1700009D RID: 157
    // (get) Token: 0x060009FA RID: 2554 RVA: 0x0002CDE8 File Offset: 0x0002AFE8
    // (set) Token: 0x060009FB RID: 2555 RVA: 0x0002CDF0 File Offset: 0x0002AFF0
    public IEventService EventService { get; private set; }

    // Token: 0x1700009E RID: 158
    // (get) Token: 0x060009FC RID: 2556 RVA: 0x0002CDF9 File Offset: 0x0002AFF9
    // (set) Token: 0x060009FD RID: 2557 RVA: 0x0002CE01 File Offset: 0x0002B001
    public IEventDataService EventDataService { get; private set; }

    // Token: 0x1700009F RID: 159
    // (get) Token: 0x060009FE RID: 2558 RVA: 0x0002CE0A File Offset: 0x0002B00A
    // (set) Token: 0x060009FF RID: 2559 RVA: 0x0002CE12 File Offset: 0x0002B012
    public IDateTimeService DateTimeService { get; private set; }

    // Token: 0x170000A0 RID: 160
    // (get) Token: 0x06000A00 RID: 2560 RVA: 0x0002CE1B File Offset: 0x0002B01B
    // (set) Token: 0x06000A01 RID: 2561 RVA: 0x0002CE23 File Offset: 0x0002B023
    public DataService DataService { get; private set; }

    // Token: 0x170000A1 RID: 161
    // (get) Token: 0x06000A02 RID: 2562 RVA: 0x0002CE2C File Offset: 0x0002B02C
    // (set) Token: 0x06000A03 RID: 2563 RVA: 0x0002CE34 File Offset: 0x0002B034
    public TimerService TimerService { get; private set; }

    // Token: 0x170000A2 RID: 162
    // (get) Token: 0x06000A04 RID: 2564 RVA: 0x0002CE3D File Offset: 0x0002B03D
    // (set) Token: 0x06000A05 RID: 2565 RVA: 0x0002CE45 File Offset: 0x0002B045
    public NavigationService NavigationService { get; private set; }

    // Token: 0x170000A3 RID: 163
    // (get) Token: 0x06000A06 RID: 2566 RVA: 0x0002CE4E File Offset: 0x0002B04E
    // (set) Token: 0x06000A07 RID: 2567 RVA: 0x0002CE56 File Offset: 0x0002B056
    public GildingService GildingService { get; private set; }

    // Token: 0x170000A4 RID: 164
    // (get) Token: 0x06000A08 RID: 2568 RVA: 0x0002CE5F File Offset: 0x0002B05F
    // (set) Token: 0x06000A09 RID: 2569 RVA: 0x0002CE67 File Offset: 0x0002B067
    public TimeWarpService TimeWarpService { get; private set; }

    // Token: 0x170000A5 RID: 165
    // (get) Token: 0x06000A0A RID: 2570 RVA: 0x0002CE70 File Offset: 0x0002B070
    // (set) Token: 0x06000A0B RID: 2571 RVA: 0x0002CE78 File Offset: 0x0002B078
    public IStoreService StoreService { get; private set; }

    // Token: 0x170000A6 RID: 166
    // (get) Token: 0x06000A0C RID: 2572 RVA: 0x0002CE81 File Offset: 0x0002B081
    // (set) Token: 0x06000A0D RID: 2573 RVA: 0x0002CE89 File Offset: 0x0002B089
    public ITriggerService TriggerService { get; private set; }

    // Token: 0x170000A7 RID: 167
    // (get) Token: 0x06000A0E RID: 2574 RVA: 0x0002CE92 File Offset: 0x0002B092
    // (set) Token: 0x06000A0F RID: 2575 RVA: 0x0002CE9A File Offset: 0x0002B09A
    public SubscriptionService SubscriptionService { get; private set; }

    // Token: 0x170000A8 RID: 168
    // (get) Token: 0x06000A10 RID: 2576 RVA: 0x0002CEA3 File Offset: 0x0002B0A3
    // (set) Token: 0x06000A11 RID: 2577 RVA: 0x0002CEAB File Offset: 0x0002B0AB
    public ProfitBoostAdService ProfitBoostAdService { get; private set; }


    // Token: 0x170000AB RID: 171
    // (get) Token: 0x06000A16 RID: 2582 RVA: 0x0002CED6 File Offset: 0x0002B0D6
    // (set) Token: 0x06000A17 RID: 2583 RVA: 0x0002CEDE File Offset: 0x0002B0DE
    public OfferwallService OfferwallService { get; private set; }

    // (set) Token: 0x06000A1B RID: 2587 RVA: 0x0002CF00 File Offset: 0x0002B100
    public NewsFeedService NewsFeedService { get; private set; }

    // Token: 0x170000AE RID: 174
    // (get) Token: 0x06000A1C RID: 2588 RVA: 0x0002CF09 File Offset: 0x0002B109
    // (set) Token: 0x06000A1D RID: 2589 RVA: 0x0002CF11 File Offset: 0x0002B111
    public MicroManagerService MicroManagerService { get; private set; }

    // Token: 0x170000AF RID: 175
    // (get) Token: 0x06000A1E RID: 2590 RVA: 0x0002CF1A File Offset: 0x0002B11A
    // (set) Token: 0x06000A1F RID: 2591 RVA: 0x0002CF22 File Offset: 0x0002B122
    public IAppOnboardService AppOnboardService { get; private set; }

    // Token: 0x170000B0 RID: 176
    // (get) Token: 0x06000A20 RID: 2592 RVA: 0x0002CF2B File Offset: 0x0002B12B
    // (set) Token: 0x06000A21 RID: 2593 RVA: 0x0002CF33 File Offset: 0x0002B133
    public IAngelInvestorService AngelService { get; private set; }

    public UnfoldingService UnfoldingService { get; private set; }

    // Token: 0x170000B4 RID: 180
    // (get) Token: 0x06000A28 RID: 2600 RVA: 0x0002CF6F File Offset: 0x0002B16F
    public BuyMultiplierService BuyMultiplierService { get; }

    // Token: 0x170000B5 RID: 181
    // (get) Token: 0x06000A29 RID: 2601 RVA: 0x0002CF77 File Offset: 0x0002B177
    public GiftService GiftService { get; }

    // Token: 0x170000B6 RID: 182
    // (get) Token: 0x06000A2A RID: 2602 RVA: 0x0002CF7F File Offset: 0x0002B17F
    public TutorialService TutorialService { get; }

    // Token: 0x170000B7 RID: 183
    // (get) Token: 0x06000A2B RID: 2603 RVA: 0x0002CF87 File Offset: 0x0002B187
    // (set) Token: 0x06000A2C RID: 2604 RVA: 0x0002CF8F File Offset: 0x0002B18F
    public IAnalyticService AnalyticService { get; private set; }

    // Token: 0x170000B8 RID: 184
    // (get) Token: 0x06000A2D RID: 2605 RVA: 0x0002CF98 File Offset: 0x0002B198
    // (set) Token: 0x06000A2E RID: 2606 RVA: 0x0002CFA0 File Offset: 0x0002B1A0
    public IGrantRewardService GrantRewardService { get; private set; }

    // Token: 0x170000B9 RID: 185
    // (get) Token: 0x06000A2F RID: 2607 RVA: 0x0002CFA9 File Offset: 0x0002B1A9
    // (set) Token: 0x06000A30 RID: 2608 RVA: 0x0002CFB1 File Offset: 0x0002B1B1
    public PlatformAccount PlatformAccount { get; private set; }

    // Token: 0x170000BD RID: 189
    // (get) Token: 0x06000A37 RID: 2615 RVA: 0x0002CFED File Offset: 0x0002B1ED
    // (set) Token: 0x06000A38 RID: 2616 RVA: 0x0002CFF5 File Offset: 0x0002B1F5
    public FirstTimeBuyerService FirstTimeBuyerService { get; private set; }

    // Token: 0x170000BE RID: 190
    // (get) Token: 0x06000A39 RID: 2617 RVA: 0x0002CFFE File Offset: 0x0002B1FE
    // (set) Token: 0x06000A3A RID: 2618 RVA: 0x0002D006 File Offset: 0x0002B206
    public PlanetMilestoneService PlanetMilestoneService { get; private set; }

    // Token: 0x170000BF RID: 191
    // (get) Token: 0x06000A3B RID: 2619 RVA: 0x0002D00F File Offset: 0x0002B20F
    // (set) Token: 0x06000A3C RID: 2620 RVA: 0x0002D017 File Offset: 0x0002B217
    public IEventMissionService EventMissionsService { get; private set; }

    // Token: 0x170000C0 RID: 192
    // (get) Token: 0x06000A3D RID: 2621 RVA: 0x0002D020 File Offset: 0x0002B220
    // (set) Token: 0x06000A3E RID: 2622 RVA: 0x0002D028 File Offset: 0x0002B228
    public PlanetThemeService PlanetThemeService { get; private set; }

    // Token: 0x170000C1 RID: 193
    // (get) Token: 0x06000A3F RID: 2623 RVA: 0x0002D031 File Offset: 0x0002B231
    // (set) Token: 0x06000A40 RID: 2624 RVA: 0x0002D039 File Offset: 0x0002B239
    public CrossPromoService CrossPromoService { get; private set; }

    public bool IsLoadingPlanet { get; private set; }

    // Token: 0x170000C4 RID: 196
    // (get) Token: 0x06000A45 RID: 2629 RVA: 0x0002D064 File Offset: 0x0002B264
    public string PlatformId
    {
        get
        {
            return "Steam";
        }
    }

    // Token: 0x170000C5 RID: 197
    // (get) Token: 0x06000A46 RID: 2630 RVA: 0x0002D06B File Offset: 0x0002B26B
    public static GameController Instance
    {
        get
        {
            GameController result;
            if ((result = GameController._Instance) == null)
            {
                result = (GameController._Instance = new GameController());
            }
            return result;
        }
    }

    // Token: 0x170000C6 RID: 198
    // (get) Token: 0x06000A47 RID: 2631 RVA: 0x0002D081 File Offset: 0x0002B281
    [HideInInspector]
    public GameState game
    {
        get
        {
            return this._State.Value;
        }
    }

    // Token: 0x170000C7 RID: 199
    // (get) Token: 0x06000A48 RID: 2632 RVA: 0x0002D08E File Offset: 0x0002B28E
    public IObservable<GameState> State
    {
        get
        {
            return (from x in this._State
                    where x != null
                    select x).AsObservable<GameState>();
        }
    }

    // Token: 0x170000C8 RID: 200
    // (get) Token: 0x06000A49 RID: 2633 RVA: 0x0002D0BF File Offset: 0x0002B2BF
    // (set) Token: 0x06000A4A RID: 2634 RVA: 0x0002D0C7 File Offset: 0x0002B2C7
    public PlayerData GlobalPlayerData { get; private set; }

    // Token: 0x170000C9 RID: 201
    // (get) Token: 0x06000A4B RID: 2635 RVA: 0x0002D0D0 File Offset: 0x0002B2D0
    // (set) Token: 0x06000A4C RID: 2636 RVA: 0x0002D0D8 File Offset: 0x0002B2D8
    public string planetName { get; private set; }

    // Token: 0x06000A4D RID: 2637 RVA: 0x0002D0E4 File Offset: 0x0002B2E4
    public GameController()
    {
        GameController._Instance = this;
        this.PlatformAccount = Helper.GetPlatformAccount();
        this.IsLoadingPlanet = true;
        Helper.GetPlatformStore();
        this.EventService = new EventService();
        this.HhAssetBundleManager = new AssetBundleManager(true);
        this.UserDataService = new UserDataService();
        this.UpgradeService = new UpgradeService();
        this.UnlockService = new UnlockService();
        this.EventService = new EventService();
        this.EventDataService = new EventDataService();
        this.DateTimeService = new DateTimeService();
        this.DataService = new DataService();
        this.TimerService = new TimerService();
        this.NavigationService = new NavigationService();
        this.GildingService = new GildingService();
        this.TimeWarpService = new TimeWarpService();
        this.StoreService = new StoreService();
        this.TriggerService = new TriggerService();
        this.SubscriptionService = new SubscriptionService();
        this.ProfitBoostAdService = new ProfitBoostAdService();
        this.OfferwallService = new OfferwallService();
        this.NewsFeedService = new NewsFeedService();
        this.MicroManagerService = new MicroManagerService();
        this.AngelService = new AngelInvestorService();
        this.PlanetMilestoneService = new PlanetMilestoneService();
        this.EventMissionsService = new EventMissionService();
        this.GrantRewardService = new GrantRewardService();
        this.PlanetThemeService = new PlanetThemeService();
        this.AnalyticService = new KongAnalyticService();
        this.CrossPromoService = new CrossPromoService();
        this.GiftService = new GiftService();
        this.TutorialService = new TutorialService();
        this.BuyMultiplierService = new BuyMultiplierService();
        this.FirstTimeBuyerService = new FirstTimeBuyerService();
        this.UnfoldingService = new UnfoldingService();
    }

    // Token: 0x06000A4E RID: 2638 RVA: 0x0002D47C File Offset: 0x0002B67C
    public void Init(Action onComplete)
    {
        this.logger = Platforms.Logger.Logger.GetLogger(this);
        this.logger.Trace("Initializing....", Array.Empty<object>());
        this.CreateGlobalPlayerData();
        this.GlobalPlayerData.inventory = new InventoryService();
        this.SetPlanetName("Earth");
        this.WireUpPermenantStreams();
        this.DateTimeService.Init(this.PlatformAccount);
        this.DateTimeService.Start(delegate
        {
            DataService dataService = this.DataService;
            Action onComplete2= (delegate ()
                {
                    PlatformAccount.AssetBundlePlatformConfig platformConfig = this.PlatformAccount.TitleDataConfig.AssetBundleConfig.GetPlatformConfig();
                    IObservable<bool> source = this.HhAssetBundleManager.InitializeAsync(platformConfig.DirectoryRoot, platformConfig.ManifestVersion);
                    Action<bool> onNext = ( delegate (bool b)
                        {
                            InventoryJsonDataObject externalData = this.DataService.ExternalData;
                            FeatureConfig.Initialize(externalData.FeatureConfig);
                            UserTargetingService userTargetingService = new UserTargetingService();
                            userTargetingService.Initialize(this.DateTimeService, externalData.Segments, externalData.TestGroups);
                            this.UserDataService.Init(userTargetingService);
                            IInventoryService inventory = this.GlobalPlayerData.inventory;
                            inventory.LoadInventoryData(externalData.Items);
                            inventory.LoadItemSlots(externalData.ItemSlotInfo);
                            this.UpdatePlanetUnlocks(inventory);
                            AdCapColours.LoadColourData(externalData.AdCapColourInfo);
                            AdCapStrings.LoadStringData(externalData.AdCapStrings);
                            AdCapHardResetRewards.LoadHardResetData(externalData.AdCapHardResetRewards);
                            AdCapExternalDataStorage.LoadData(externalData);
                            StoreModalController.PANEL_SERVER_CONTROLED_DEFAULT = externalData.GeneralConfig[0].StoreConfig.DefaultPanelId;
                            PlayerData playerData = PlayerData.GetPlayerData("Global");
                            new InventoryToInventoryConversionCommand().Execute(externalData.LegacyInventorySaveDataConvertion[0].Inv2InvConvert, playerData);
                            new LoadSavedInventoryDataCommand().Execute();
                            this.GlobalPlayerData.StoreItemsLive = externalData.StoreItems;
                            this.IconService.Init();
                            this.AnalyticService.InitializeAnalyticsHooks();
                            this.AnalyticService.SendAnalyticsToPlayfab = externalData.GeneralConfig[0].SendAnalyticsToPlayfab;
                            OrientationController.Instance.Init(this.TimerService);
                            this.TimerService.StartClock();
                            this.TriggerService.Init(this, this.UserDataService, this.DataService, this.StoreService, this.AngelService, this.DateTimeService, this.NavigationService, this.EventMissionsService, this.PlanetMilestoneService, this.FirstTimeBuyerService);
                            this.StoreService.Init(this, this.UserDataService, this.GrantRewardService, this.DateTimeService, this.UpgradeService, this.AngelService, this.TimerService, this.TriggerService, this.GlobalPlayerData, this.GlobalPlayerData.inventory, Helper.GetPlatformStore());
                            this.AngelService.Init(this);
                            this.UpgradeService.Init();
                            this.UnlockService.Init();
                            this.GildingService.Init();
                            this.PlanetUnlockService.Init();
                            this.SubscriptionService.Init(this.DateTimeService, this.GrantRewardService);
                            this.NewsFeedService.Init(externalData.GeneralConfig[0].NewsConfig.NotificationEnabled);
                            this.MicroManagerService.Init();
                            this.GiftService.Init(this, this.DataService, this.UserDataService, this.TriggerService, this.GlobalPlayerData, this.GrantRewardService);
                            this.EventMissionsService.Init(this, this.DateTimeService, this.TimerService, this.AngelService, this.StoreService, this.UpgradeService, this.EventService, this.AnalyticService, this.ProfitBoostAdService);
                            this.CrossPromoService.Init(this, this.NavigationService, this.AnalyticService, externalData.GeneralConfig[0].RootAnalyticDelay);
                            this.TutorialService.Init(this, this.TriggerService, this.DataService, this.UserDataService);
                            this.GrantRewardService.Init(this, this.GlobalPlayerData.inventory, this.AngelService, this.SubscriptionService, this.GildingService);
                            this.UnfoldingService.Init(this, this.DataService, this.TriggerService, this.UserDataService);
                            this.PlanetMilestoneService.Init(this, this.EventService, this.AnalyticService, this.GrantRewardService);
                            this.BuyMultiplierService.Init(this);
                            this.FirstTimeBuyerService.Init(externalData.FirstTimeBuyerGroups, this, this.UserDataService, this.StoreService);
                            this.TimeWarpService.Init(this.UnfoldingService, this.StoreService);
                            IEventServiceServerRequests eventServiceServerRequests = new EventServiceServerRequests();
                            this.EventService.Init(this, this.EventDataService, eventServiceServerRequests, this.DateTimeService, this.TriggerService,  this.TimerService, this.UserDataService);
                            this.PreLoadAllDefaultPlanetData();

                            onComplete();
                            IObservable<WelcomeBackSequenceCompleted> source2 = MessageBroker.Default.Receive<WelcomeBackSequenceCompleted>();
                            Action<WelcomeBackSequenceCompleted>  onNext2 = (delegate (WelcomeBackSequenceCompleted w)
                                {
                                    bool isLoadingPlanet = this.IsLoadingPlanet;
                                    this.IsLoadingPlanet = false;
                                    int num = this.game.planetPlayerData.Has("PlanetLoadCount") ? this.game.planetPlayerData.GetInt("PlanetLoadCount", 0) : 0;
                                    bool flag = !string.IsNullOrEmpty(this.lastPlanetName) && this.planetName != this.lastPlanetName;
                                    if (num > 1 && isLoadingPlanet && flag )
                                    {
                                        Action<Unit> onNext3= ( delegate (Unit adWatchSuccess)
                                            {
                                                this.AnalyticService.SendAdFinished("PlanetLoad", "Interstitial", this.game.planetName, "");
                                            });
                                        Action<Exception> onError = (delegate (Exception adWatchError)
                                            {
                                                bool value = Helper.GetPlatformAd().AdReadyMap[AdType.Interstitial].Value;
                                                bool value2 = Helper.GetPlatformAd().AdReadyMap[AdType.RewardedVideo].Value;
                                                this.AnalyticService.SendAdFinished("PlanetLoad", "Error=" + adWatchError, this.game.planetName, value2.ToString() + ":" + value.ToString());
                                            });
                                    }
                                });
                            source2.Subscribe(onNext2).AddTo(this.instanceDisposables);
                            this.IsInitialized.Value = true;
                            this.logger.Trace("Initialized", Array.Empty<object>());
                        });
                    
                    source.Subscribe(onNext, delegate (Exception error)
                    {
                        Root.ShowConnectionErrorAndForceClose("FailedToLoadAssetbundleManifest", "Startup");
                    });
                });
            dataService.Init(onComplete2, delegate (Exception error)
            {
                Root.ShowConnectionErrorAndForceClose("FailedToInitDataService", error.Message);
            });
        }, delegate
        {
            Root.ShowConnectionErrorAndForceClose("FailedToRetrieveDate", "Startup");
        });
    }

    // Token: 0x06000A4F RID: 2639 RVA: 0x0002D52C File Offset: 0x0002B72C
    public void SetMute(bool isMuted)
    {
        this.IsMuted.Value = isMuted;
        AudioListener.volume = (float)(isMuted ? 0 : 1);
    }

    // Token: 0x06000A50 RID: 2640 RVA: 0x0002D547 File Offset: 0x0002B747
    public void Dispose()
    {
        this.instanceDisposables.Dispose();
        this.stateDisposables.Dispose();
        GameController._Instance = null;
    }

    // Token: 0x06000A51 RID: 2641 RVA: 0x0002D568 File Offset: 0x0002B768
    public void ResetGame(bool isAdwatch = false)
    {
        this.OnSoftResetPre();
        this.game.Reset(isAdwatch);
        this.SaveGame((double)this.DateTimeService.UtcNow.TotalSeconds());
        this.OnSoftResetPost();
    }

    // Token: 0x06000A52 RID: 2642 RVA: 0x0002D5FC File Offset: 0x0002B7FC
    public void LoadPlanetScene(string planetName, bool forceSave = false)
    {
        this.LogTimerDebugging("LoadPlanetScene " + planetName);
        this.stateDisposables.Clear();
        if (this.game != null)
        {
            if (this.game.IsDisposed)
            {
                Debug.LogError("GameState is already disposed will not save");
            }
            else
            {
                if (forceSave || this.game.planetName != planetName)
                {
                    long num = this.DateTimeService.UtcNow.TotalSeconds();
                    this.game.timestamp = (double)num;
                    this.SaveGame((double)num);
                }
                this._State.Value.Dispose();
            }
        }
        this.IsLoadingPlanet = true;
        this.NavigationService.CloseAllModals();
        this.WaitThenLoad(planetName).ToObservable(false).StartAsCoroutine(default(CancellationToken));
    }
    private IEnumerator WaitThenLoad(string planetName)
    {
        this.OnLoadNewPlanetPre();
        LaunchingModal launchingmodal = this.NavigationService.CreateModal<LaunchingModal>(NavModals.LAUNCHING_MODAL, false);
        yield return new WaitForSeconds(0.5f);
        Action postLoadDelegate = delegate ()
        {
            this.OnLoadNewPlanetPost();
            Resources.UnloadUnusedAssets();
        };
        this.SetPlanetName(planetName);
        this.HhAssetBundleManager.GetBundleAsync("gamestate-" + this.planetName.ToLower()).Subscribe(delegate (IAssetBundle gameStateBundle)
        {
            GameState_Serialized[] array = gameStateBundle.LoadAllAssets<GameState_Serialized>();
            if (array == null || array.Length == 0)
            {
                Platforms.Logger.Logger.GetLogger(this).Error("Failed to load gamestate data for " + this.planetName);
                PopupModal popupModal = GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false);
                string title = "Error";
                string body = "We ran into a problem loading the game state!";
                Action okCallback =()=> this.LoadPlanetScene("Earth", false);
                popupModal.WireData(title, body, okCallback, PopupModal.PopupOptions.OK, "Return to Earth", "", false, null, "");
                return;
            }
            IEnumerable<GameState_Serialized> source = array;
            Func<GameState_Serialized, bool> predicate =(GameState_Serialized x) => x.planetName == planetName;

            nextGameStateSerialized = source.FirstOrDefault(predicate);
            if (this.nextGameStateSerialized == null)
            {
                Platforms.Logger.Logger.GetLogger(this).Error("Failed to load gamestate data for " + this.planetName);
                PopupModal popupModal2 = GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false);
                string title2 = "Error";
                string body2 = "We ran into a problem loading the game state!!";
                Action okCallback2 = ()=> this.LoadPlanetScene("Earth", false);
                popupModal2.WireData(title2, body2, okCallback2, PopupModal.PopupOptions.OK, "Return to Earth", "", false, null, "");
                return;
            }
            string planetTheme = this.nextGameStateSerialized.planetTheme;
            string planetBundleId = this.planetName.ToLower();
            if (GameState.IsEvent(planetName))
            {
                IEnumerable<EventModel> activeEvents = this.EventService.ActiveEvents;
                Func<EventModel, bool> predicate2 = (EventModel x) => x.Id == planetName;
                EventModel eventModel = activeEvents.FirstOrDefault(predicate2);
                if (eventModel != null)
                {
                    planetBundleId = eventModel.PlanetTheme.ToLower();
                }
            }
            IObservable<IAssetBundle> bundleAsync = this.HhAssetBundleManager.GetBundleAsync("planetdata-" + planetBundleId);
            Action<IAssetBundle> onNext = delegate (IAssetBundle planetBundle)
            {
                PlanetData[] array2 = planetBundle.LoadAllAssets<PlanetData>();
                if (array2 == null || array2.Length == 0)
                {
                    Platforms.Logger.Logger.GetLogger(this).Error("Failed to load planet data for " + this.planetName);
                    PopupModal popupModal3 = GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false);
                    string title3 = "Error";
                    string body3 = "We ran into a problem loading the planet data!";
                    Action okCallback3 = ()=> this.LoadPlanetScene("Earth", false);
                    popupModal3.WireData(title3, body3, okCallback3, PopupModal.PopupOptions.OK, "Return to Earth", "", false, null, "");
                    return;
                }
                IEnumerable<PlanetData> source2 = array2;
                Func<PlanetData, bool> predicate3 = ((PlanetData x) => x.PlanetName == planetTheme);

                nextPlanetData = source2.FirstOrDefault(predicate3);

                if (this.nextPlanetData == null)
                {
                    Platforms.Logger.Logger.GetLogger(this).Error("Failed to load planet data for " + this.planetName);
                    PopupModal popupModal4 = GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false);
                    string title4 = "Error";
                    string body4 = "We ran into a problem loading the planet data!!";
                    Action okCallback4= ()=> this.LoadPlanetScene("Earth", false);
                    popupModal4.WireData(title4, body4, okCallback4, PopupModal.PopupOptions.OK, "Return to Earth", "", false, null, "");
                    return;
                }
                string bundleName = "planettheme-" + planetBundleId;
                IObservable<IAssetBundle> bundleAsync2 = this.HhAssetBundleManager.GetBundleAsync(bundleName);
                Action<IAssetBundle> onNext2 = delegate (IAssetBundle themeBundle)
                {
                    this.PlanetThemeService.UpdateAssetBundle(themeBundle, bundleName);
                    SceneManager.LoadScene("Main");
                    IObservable<Unit> source3 = Observable.NextFrame(FrameCountType.FixedUpdate);
                    Action<Unit> onNext3 = (Unit u )=>
                        {
                            postLoadDelegate();
                            launchingmodal.CloseModal(Unit.Default);
                        };
                    source3.Subscribe(onNext3);
                    this.AnalyticService.SendTaskCompleteEvent("Blastoff", this.planetName, this.planetName);
                };
                Action<Exception> onError2 = (Exception e) =>
                {
                    Platforms.Logger.Logger.GetLogger(this).Error(e.Message);
                    PopupModal popupModal5 =
                        GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false);
                    string title5 = "Error";
                    string body5 = "We ran into a problem loading the planet! Please check your connection and try again!";

                    Action okCallback5 =() => this.LoadPlanetScene("Earth", false);
                    PopupModal.PopupOptions options = PopupModal.PopupOptions.Two_Green_Buttons;
                    string confirmButtonText = "Return to Earth";
                    string cancelButtonText = "Retry";
                    bool showClose = false;
                    Action cancelCallback = ()=> this.LoadPlanetScene(this.planetName, false);
                    popupModal5.WireData(title5, body5, okCallback5, options, confirmButtonText, cancelButtonText,
                        showClose, cancelCallback, "");
                };
                bundleAsync2.Subscribe(onNext2, onError2);
            };
            Action<Exception> onError = (Exception exception) =>
            {

                Platforms.Logger.Logger.GetLogger(this).Error(exception.Message);
                PopupModal popupModal3 =
                    GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false);
                string title3 = "Error";
                string body3 = "We ran into a problem loading the planet. Please check your connection and try again!";
                Action okCallback3 = () => this.LoadPlanetScene("Earth", false);
                PopupModal.PopupOptions options = PopupModal.PopupOptions.Two_Green_Buttons;
                string confirmButtonText = "Return to Earth";
                string cancelButtonText = "Retry";
                bool showClose = false;
                Action cancelCallback = () => this.LoadPlanetScene(this.planetName, false);
                popupModal3.WireData(title3, body3, okCallback3, options, confirmButtonText, cancelButtonText,
                    showClose, cancelCallback, "");

            };
            bundleAsync.Subscribe(onNext, onError);
        }, delegate (Exception exception)
        {
            Platforms.Logger.Logger.GetLogger(this).Error(exception.Message);
            PopupModal popupModal = GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false);
            string title = "Error";
            string body = "We ran into a problem loading the planet. Please check your connection and try again!";
            Action okCallback = ()=> this.LoadPlanetScene("Earth", false);
            PopupModal.PopupOptions options = PopupModal.PopupOptions.Two_Green_Buttons;
            string confirmButtonText = "Return to Earth";
            string cancelButtonText = "Retry";
            bool showClose = false;
            Action cancelCallback = ()=> this.LoadPlanetScene(this.planetName, false);
            popupModal.WireData(title, body, okCallback, options, confirmButtonText, cancelButtonText, showClose, cancelCallback, "");
        });
    }

    // Token: 0x06000A53 RID: 2643 RVA: 0x0002D6C0 File Offset: 0x0002B8C0
    public void HardResetPlanetOnComplete()
    {
        int num = this._State.Value.planetPlayerData.GetInt(this.planetName + "HardResetCount", 0) + 1;
        this._State.Value.planetPlayerData.Set(this.planetName + "HardResetCount", num.ToString());
        if (!this._State.Value.planetPlayerData.GetBool(this.planetName + "HardResetReward"))
        {
            RewardData rewardData = new RewardData("gold", ERewardType.Gold, AdCapHardResetRewards.GetRewardByKey(this.planetName));
            if (this.GrantRewardService.GrantReward(rewardData, "Planet_Prestige", this.planetName, false) == null)
            {
                this.OnHardResetRewardFailed("Error granting hard reset reward");
            }
            this._State.Value.planetPlayerData.SetBool(this.planetName + "HardResetReward", true);
        }
        this.ResetGamePlanetHard();
    }

    // Token: 0x06000A54 RID: 2644 RVA: 0x0002D7B2 File Offset: 0x0002B9B2
    private void OnHardResetRewardFailed(string error)
    {
        this.logger.Error(error);
    }

    // Token: 0x06000A55 RID: 2645 RVA: 0x0002D7C0 File Offset: 0x0002B9C0
    private void ResetGamePlanetHard()
    {
        this.OnHardResetPre();
        string value = PlayerData.GetPlayerData("Global").Get("Badges", "");
        PlayerData.DeletePlayerData(this._State.Value.planetName);
        this.game.TotalPreviousCash.Value = 0.0;
        this.game.SessionCash.Value = 0.0;
        this.game.HardReset();
        PlayerData.GetPlayerData("Global").Set("Badges", value);
        this.GlobalPlayerData.SetBool(string.Format("{0}HasBeenHardReset", this._State.Value.planetName), true);
        this.GlobalPlayerData.Remove(string.Format("{0}HasSeenPostcard", this._State.Value.planetName));
        this.SaveGame((double)this.DateTimeService.UtcNow.TotalSeconds());
        this.game.neverRate = 1;
        this.OnHardResetPost();
        this.LoadPlanetScene(this.game.planetName, false);
    }

    // Token: 0x06000A56 RID: 2646 RVA: 0x0002D8E8 File Offset: 0x0002BAE8
    private void PreLoadAllDefaultPlanetData()
    {
        for (int i = 0; i < this.DefaultPlanets.Length; i++)
        {
            this.LoadPlanetAssetBundles(this.DefaultPlanets[i], this.DefaultPlanets[i], 0);
        }
    }

    // Token: 0x06000A57 RID: 2647 RVA: 0x0002D920 File Offset: 0x0002BB20
    private void LoadPlanetAssetBundles(string pPlanetName, string pPlanetTheme, int attempts = 0)
    {
        if (this.PlanetDataList.All((PlanetData p) => p.PlanetName != pPlanetName))
        {
            string bundleName = "planetdata-" + pPlanetTheme.ToLower();
            this.HhAssetBundleManager.GetBundleAsync(bundleName).Subscribe(delegate (IAssetBundle bundle)
            {
                PlanetData[] array = bundle.LoadAllAssets<PlanetData>();
                if (array != null)
                {
                    int num = array.Length;
                }
                this.PlanetDataList.Add(array[0]);
                this.PlanetDataList = (from x in this.PlanetDataList
                                       orderby x.DisplayPriority
                                       select x).ToList<PlanetData>();
            }, delegate (Exception exception)
            {
                if (attempts < 2)
                {
                    string pPlanetName2 = pPlanetName;
                    string pPlanetTheme2 = pPlanetTheme;
                    int attempts2 = attempts + 1;
                    attempts = attempts2;

                    LoadPlanetAssetBundles(pPlanetName2, pPlanetTheme2, attempts2);
                    return;
                }
                Platforms.Logger.Logger.GetLogger(this).Error(exception.Message);
            });
        }
    }

    // Token: 0x06000A58 RID: 2648 RVA: 0x0002D9A8 File Offset: 0x0002BBA8
    private void UpdatePlanetUnlocks(IInventoryService inventory)
    {
        inventory.GetItemById("earth_unlock").Owned.Value = 1;
        string str = "PlanetPanel_";
        if (inventory.GetItemById("moon_unlock").Owned.Value == 0 && PlayerPrefs.HasKey(str + "Moon") && Serializer.Deserialize<GameController.LegacyPlanetUnlockState>(PlayerPrefs.GetString(str + "Moon")).currentState == 2)
        {
            inventory.GetItemById("moon_unlock").Owned.Value = 1;
        }
        if (inventory.GetItemById("mars_unlock").Owned.Value == 0 && PlayerPrefs.HasKey(str + "Mars") && Serializer.Deserialize<GameController.LegacyPlanetUnlockState>(PlayerPrefs.GetString(str + "Mars")).currentState == 2)
        {
            inventory.GetItemById("mars_unlock").Owned.Value = 1;
        }
    }

    // Token: 0x06000A59 RID: 2649 RVA: 0x0002DA89 File Offset: 0x0002BC89
       // Token: 0x06000A5A RID: 2650 RVA: 0x0002DA9F File Offset: 0x0002BC9F
    private void SetPlanetName(string newPlanetName)
    {
        this.lastPlanetName = this.planetName;
        this.planetName = newPlanetName;
    }

    // Token: 0x06000A5B RID: 2651 RVA: 0x0002DAB4 File Offset: 0x0002BCB4
    private void CreateGlobalPlayerData()
    {
        this.GlobalPlayerData = (PlayerData.GetPlayerData("Global") ?? new PlayerData("Global"));
        this.SetupDefaultKeys();
    }

    // Token: 0x06000A5C RID: 2652 RVA: 0x0002DADC File Offset: 0x0002BCDC
    public void Setup()
    {
        GameState state = GameState.Create(this.planetName, this.nextGameStateSerialized, this.nextPlanetData);
        GameStateSaveLoad.Load(state.planetName, delegate (GameStateSaveData saveData)
        {
            state.InitVentureModels(from _ in Observable.EveryUpdate()
                                    select Time.time);
            state.LoadGameSaveData(saveData);
            int num = Math.Max(0, state.planetPlayerData.GetInt("Multipliers", 0) * 3);
            state.SetMultiplier((float)num);
            this._State.SetValueAndForceNotify(state);
            if (this.game != null && this.game.planetPlayerData.Has("MegaBucksBalance"))
            {
                int @int = this.game.planetPlayerData.GetInt("MegaBucksBalance", 0);
                this.game.planetPlayerData.Remove("MegaBucksBalance");
                this.GlobalPlayerData.Add("MegaBucksBalance", (double)@int);
            }
            this.SetupStreams();
            bool flag = false;
            foreach (Item item in this.GlobalPlayerData.inventory.GetAllEquippedItems())
            {
                if (item.ItemBonusTarget != ItemBonusTarget.Venture)
                {
                    this.OnItemEquipStateChanged(new InventoryEquipMessage(item, this.GlobalPlayerData.inventory.GetSlotIndexForItem(item), true));
                }
                else
                {
                    flag = true;
                }
            }
            if (flag)
            {
                for (int i = 0; i < state.VentureModels.Count; i++)
                {
                    state.VentureModels[i].ApplyAllEquipmentBonuses();
                }
            }
            this.CalculateElapsedOfflineTime().ToObservable(false).StartAsCoroutine(default(CancellationToken));
        });
    }

    // Token: 0x06000A5D RID: 2653 RVA: 0x0002DB30 File Offset: 0x0002BD30
    private void SetupStreams()
    {
        this.stateDisposables.Clear();
        Observable.Interval(TimeSpan.FromSeconds((double)this.saveTimePeriod)).Subscribe(delegate (long _)
        {
            this.SaveGame((double)this.DateTimeService.UtcNow.TotalSeconds());
        }).AddTo(this.stateDisposables);
        (from __ in Observable.FromEvent<VentureModel>(delegate (Action<VentureModel> h)
        {
            this._State.Value.OnVentureBoosted += h;
        }, delegate (Action<VentureModel> h)
        {
            this._State.Value.OnVentureBoosted -= h;
        })
         where this._State.Value.VentureModels.All((VentureModel v) => v.IsBoosted.Value)
         select __).Subscribe(delegate (VentureModel __)
         {
             this.GildingService.AllVenturesBoosted.Value = true;
             FTUE_Manager.ShowFTUE("PlatinumUpgrades", null);
         }).AddTo(this.stateDisposables);
        (from b in this.GildingService.AllVenturesBoosted
         where b
         select b).Subscribe(delegate (bool b)
         {
             double value = this._State.Value.planetPlayerData.GetDouble("Platinum Upgrade", 0.0);
             if (Math.Abs(value) < 0.001)
             {
                 value = 17.77;
                 this._State.Value.planetPlayerData.Set("Platinum Upgrade", value.ToString());
             }
         }).AddTo(this.stateDisposables);
        if (FeatureConfig.IsFlagSet("FreeGoldOnFirstBankPurchase") && this._State.Value.planetName == "Earth" && !this._State.Value.hasUserClaimedFreeGold)
        {
            VentureModel ventureModel = this._State.Value.VentureModels.FirstOrDefault((VentureModel v) => v.Id == "bank");
            if (ventureModel != null)
            {
                ventureModel.TotalOwned.First((double n) => n >= 1.0).Subscribe(delegate (double n)
                {
                    if (!this._State.Value.hasUserClaimedFreeGold)
                    {
                        RewardData rewardData = new RewardData("gold", ERewardType.Gold, 20);
                        if (this.GrantRewardService.GrantReward(rewardData, "FirstBank", this.game.planetName, false) != null)
                        {
                            this._State.Value.hasUserClaimedFreeGold = true;
                        }
                    }
                }).AddTo(this.stateDisposables);
            }
        }
        this.UnlockService.OnUnlockAchieved.Subscribe(delegate (Unlock _)
        {
            int num = this.UnlockService.Unlocks.Count((Unlock u) => u.Earned.Value);
            int @int = this._State.Value.planetPlayerData.GetInt("Planet_Unlock_Count", 0);
            if (num > @int)
            {
                this._State.Value.planetPlayerData.Set("Planet_Unlock_Count", num.ToString());
                this.AnalyticService.SendTaskCompleteEvent("Planet_Unlock_Count", this._State.Value.planetName, num.ToString());
            }
        }).AddTo(this.stateDisposables);
        Observable.EveryApplicationPause().Subscribe(new Action<bool>(this.OnApplicationPause)).AddTo(this.stateDisposables);
        Observable.OnceApplicationQuit().Subscribe(delegate (Unit _)
        {
            this.OnApplicationQuit();
        }).AddTo(this.stateDisposables);
    }

    // Token: 0x06000A5E RID: 2654 RVA: 0x0002DD3C File Offset: 0x0002BF3C
    private void WireUpPermenantStreams()
    {
        MessageBroker.Default.Receive<InventoryEquipMessage>().Subscribe(new Action<InventoryEquipMessage>(this.OnItemEquipStateChanged)).AddTo(this.instanceDisposables);
    }

    // Token: 0x06000A5F RID: 2655 RVA: 0x0002DD68 File Offset: 0x0002BF68
    private void SetupDefaultKeys()
    {
        if (!this.GlobalPlayerData.Has(GameController.SESSION_COUNT_KEY))
        {
            this.GlobalPlayerData.Set(GameController.SESSION_COUNT_KEY, "1");
        }
        else
        {
            this.GlobalPlayerData.Add(GameController.SESSION_COUNT_KEY, 1.0);
        }
        if (!this.GlobalPlayerData.Has("Gold"))
        {
            this.GlobalPlayerData.Set("Gold", "0");
        }
        if (!this.GlobalPlayerData.Has("MegaBucksBalance"))
        {
            this.GlobalPlayerData.Set("MegaBucksBalance", "0");
        }
        if (!this.GlobalPlayerData.Has("MegaTickets"))
        {
            this.GlobalPlayerData.Set("MegaTickets", "0");
        }
        int num = 0;
        DateTime utcNow = this.DateTimeService.UtcNow;
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        if (this.GlobalPlayerData.Has(GameController.LOGIN_CURRENT_SEQUENTIAL_DAYS_KEY))
        {
            num = int.Parse(this.GlobalPlayerData.Get(GameController.LOGIN_CURRENT_SEQUENTIAL_DAYS_KEY, ""));
        }
        if (num < 7)
        {
            if (this.GlobalPlayerData.Has(GameController.LOGIN_LAST_LOGIN_DATE_KEY))
            {
                dateTime = DateTime.Parse(this.GlobalPlayerData.Get(GameController.LOGIN_LAST_LOGIN_DATE_KEY, ""));
            }
            double totalDays = (utcNow.Date - dateTime.Date).TotalDays;
            if (totalDays == 1.0)
            {
                num++;
                this.GlobalPlayerData.Set(GameController.LOGIN_CURRENT_SEQUENTIAL_DAYS_KEY, num.ToString());
                this.GlobalPlayerData.Set(GameController.LOGIN_LAST_LOGIN_DATE_KEY, utcNow.ToString());
                return;
            }
            if (totalDays > 1.0)
            {
                this.GlobalPlayerData.Set(GameController.LOGIN_CURRENT_SEQUENTIAL_DAYS_KEY, 1.ToString());
                this.GlobalPlayerData.Set(GameController.LOGIN_LAST_LOGIN_DATE_KEY, utcNow.ToString());
            }
        }
    }

    // Token: 0x06000A60 RID: 2656 RVA: 0x0002DF78 File Offset: 0x0002C178
    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            this.pauseTime = this.DateTimeService.UtcNow;
            this.game.timestamp = (double) this.DateTimeService.UtcNow.TotalSeconds();
            this.SaveGame(this.game.timestamp);
            return;
        }

        this.LogTimerDebugging("OnUnpause force time update");
    }

    // Token: 0x06000A61 RID: 2657 RVA: 0x0002E00C File Offset: 0x0002C20C
    private void OnApplicationQuit()
    {
        long num = this.DateTimeService.UtcNow.TotalSeconds();
        int num2 = Math.Max(0, this.game.planetPlayerData.GetInt("Multipliers", 0) * 3);
        this.game.SetMultiplier((float)num2);
        this.game.timestamp = (double)num;
        this.SaveGame((double)num);
        this.Dispose();
    }


    // Token: 0x06000A64 RID: 2660 RVA: 0x0002E0E6 File Offset: 0x0002C2E6
    private IEnumerator CalculateElapsedOfflineTime()
    {
        yield return null;
        this.LogTimerDebugging("[GameController] CalculateElapsedOfflineTime");
        while (!this.DateTimeService.IsStarted)
        {
            yield return null;
        }
        long num = this.DateTimeService.UtcNow.TotalSeconds();
        if (this.game.timestamp <= 0.0)
        {
            this.game.timestamp = (double)num;
        }
        double num2 = (double)num - this.game.timestamp;
        this.LogTimerDebugging(string.Format("CalculateElapsedOfflineTime now={0}, ts={1}:{2}, elapsed={3}", new object[]
        {
            num,
            this.game.timestamp,
            this.game.planetName,
            num2
        }));
        this.offlineTime = num2;
        this.logger.Trace(string.Format("Time elapsed since going offline: [{0}]", num2), Array.Empty<object>());
        if (num2 < 0.0)
        {
            Debug.Log("You went back in time!!! How did you do that?");
            num2 = 0.0;
        }
        this.logger.Trace("Award Offline Time!", Array.Empty<object>());
        this.ApplyElapsedOfflineTime(num2);
        this.TimerService.AddTime(TimerService.TimerGroups.State, TimeSpan.FromSeconds(num2), true);
        yield break;
    }

    // Token: 0x06000A65 RID: 2661 RVA: 0x0002E0F8 File Offset: 0x0002C2F8
    private void ApplyElapsedOfflineTime(double elapsed)
    {
        double num = 0.0;
        if (elapsed > 0.0)
        {
            double num2 = this.CalculateBonusTime(GameState.MultiplierBonusType.Ad, this.ProfitBoostAdService.AdMultiplierBonus.Value, elapsed);
            num += num2;
            double num3 = (double)this.game.ProfitAdExpiry.TotalSeconds() - this.game.timestamp;
            if (num3 <= 0.0)
            {
                num3 = 0.0;
            }
            double elapsed2 = Math.Max(elapsed - num3, 0.0);
            num += this.AddVentureOffline(this.game.CalculateElapsed(elapsed2));
        }
        if (elapsed > 120.0 && num > 0.0)
        {
            this.OfflineEarningsCalculated.Value = new OfflineEarnings(elapsed, num);
            return;
        }
        MessageBroker.Default.Publish<WelcomeBackSequenceCompleted>(default(WelcomeBackSequenceCompleted));
    }

    // Token: 0x06000A66 RID: 2662 RVA: 0x0002E1D4 File Offset: 0x0002C3D4
    private double CalculateAppOnboardBonusEligibleTime(double elapsed)
    {
        DateTime utcNow = this.DateTimeService.UtcNow;
        DateTime boostExpiry = this.AppOnboardService.BoostExpiry;
        double result = 0.0;
        if (boostExpiry > utcNow)
        {
            result = 0.0;
        }
        else
        {
            TimeSpan ts = utcNow - boostExpiry;
            TimeSpan t = TimeSpan.FromSeconds(elapsed).Subtract(ts);
            if (t > TimeSpan.Zero)
            {
                result = t.TotalSeconds;
            }
        }
        return result;
    }

    // Token: 0x06000A67 RID: 2663 RVA: 0x0002E24C File Offset: 0x0002C44C
    private double CalculateBonusTime(GameState.MultiplierBonusType bonusType, float bonusMultiplier, double elapsed)
    {
        double num = 0.0;
        double num2 = (double)this.game.ProfitAdExpiry.TotalSeconds() - this.game.timestamp;
        if (num2 <= 0.0)
        {
            return 0.0;
        }
        double elapsed2 = (num2 > elapsed) ? elapsed : num2;
        this.game.SetMultiplierBonus(bonusType, bonusMultiplier);
        num += this.AddVentureOffline(this.game.CalculateElapsed(elapsed2));
        if (double.IsInfinity(num))
        {
            num = GameState.MAX_CASH_DOUBLE;
        }
        if (num2 <= elapsed)
        {
            this.game.RemoveMultiplierBonus(bonusType);
        }
        this.logger.Trace(string.Format("Received offline earnings of {0} from {1} bonus", num, bonusType.ToString()), Array.Empty<object>());
        return num;
    }

    // Token: 0x06000A68 RID: 2664 RVA: 0x0002E310 File Offset: 0x0002C510
    private double AddVentureOffline(double elapsed)
    {
        double num = this.game.VentureModels.Sum((VentureModel venture) => venture.ProfitSurgeAmount(elapsed, true));
        if (double.IsInfinity(num))
        {
            num = GameState.MAX_CASH_DOUBLE;
        }
        return this.game.AddCash(num, true);
    }

    // Token: 0x06000A69 RID: 2665 RVA: 0x0002E364 File Offset: 0x0002C564
    private void SaveGame(double time)
    {
        this.LogTimerDebugging(string.Concat(new object[]
        {
            "SaveGame( ",
            this.game.planetName,
            ") ",
            time
        }));
        this.LogTimerDebugging(string.Concat(new object[]
        {
            "SaveGame( ",
            this.game.planetName,
            ") ",
            time
        }));
        if (this.OnPreSavePlanetData != null)
        {
            this.OnPreSavePlanetData();
        }
        GameStateSaveLoad.Save(this.game, time);
    }

    // Token: 0x06000A6A RID: 2666 RVA: 0x0002E400 File Offset: 0x0002C600
    private void OnPlatinumUpgradePurchaseCompleted()
    {
        if (this.game.IsEventPlanet && this.GildingService.AllVenturesBoosted.Value)
        {
            this.game.VentureModels.ToList<VentureModel>().ForEach(delegate (VentureModel x)
            {
                IntReactiveProperty gildLevel = x.gildLevel;
                int value = gildLevel.Value;
                gildLevel.Value = value + 1;
            });
            this.game.OnPlayerDataLoaded();
        }
    }

    // Token: 0x06000A6B RID: 2667 RVA: 0x0002E46C File Offset: 0x0002C66C
    public void AwardBadge(BadgeInfo badge)
    {
        if (!this.game.eventBadges.ContainsKey(badge.name))
        {
            this.game.eventBadges.Add(badge.name, badge);
        }
        this.game.eventBadges[badge.name].awarded = true;
        this.game.eventBadges[badge.name].timesAwarded++;
    }

    // Token: 0x06000A6C RID: 2668 RVA: 0x0002E4E8 File Offset: 0x0002C6E8
    private void OnItemEquipStateChanged(InventoryEquipMessage inventoryEquipMessage)
    {
        ItemBonusType itemBonusType = inventoryEquipMessage.item.ItemBonusType;
        if (itemBonusType == ItemBonusType.GoldBonus)
        {
            this._State.Value.goldBonus.Value += inventoryEquipMessage.item.GetLeveledBonus(0) * (float)(inventoryEquipMessage.equipped ? 1 : -1);
            return;
        }
        if (itemBonusType != ItemBonusType.MegaBucks)
        {
            return;
        }
        this._State.Value.megabucksBonus.Value += inventoryEquipMessage.item.GetLeveledBonus(0) * (float)(inventoryEquipMessage.equipped ? 1 : -1);
    }

    // Token: 0x06000A6D RID: 2669 RVA: 0x00002718 File Offset: 0x00000918
    private void LogTimerDebugging(string str)
    {
    }

    // Token: 0x04000855 RID: 2133
    private const int SECONDS_OFFLINE_TO_SHOW_WELCOME_BACK = 120;

    // Token: 0x04000856 RID: 2134
    public static readonly string EARNED_GOLD_PREF_KEY = "EarnedGold";

    // Token: 0x04000857 RID: 2135
    public static readonly string BOUGHT_GOLD_PREF_KEY = "BoughtGold";

    // Token: 0x04000858 RID: 2136
    public static readonly string SHOULD_SIGN_IN_KEY = "ShouldSignInUser";

    // Token: 0x04000859 RID: 2137
    private const double UNPAUSE_CAUSES_REBOOT_MINUTES = 15.0;

    // Token: 0x0400085A RID: 2138
    public static string SESSION_COUNT_KEY = "SessionCount";

    // Token: 0x0400085B RID: 2139
    public static string LOGIN_CURRENT_SEQUENTIAL_DAYS_KEY = "LOGIN_CURRENT_SEQUENTIAL_DAYS_KEY";

    // Token: 0x0400085C RID: 2140
    public static string LOGIN_LAST_LOGIN_DATE_KEY = "LOGIN_LAST_LOGIN_DATE_KEY";

    // Token: 0x04000864 RID: 2148
    public ReactiveProperty<OfflineEarnings> OfflineEarningsCalculated = new ReactiveProperty<OfflineEarnings>();

    // Token: 0x04000865 RID: 2149
    public readonly IconService IconService = new IconService();

    // Token: 0x04000866 RID: 2150
    public IAssetBundleManager HhAssetBundleManager;

    // Token: 0x04000890 RID: 2192
    public readonly PlanetUnlockService PlanetUnlockService = new PlanetUnlockService();

    // Token: 0x04000891 RID: 2193
    public List<PlanetData> PlanetDataList = new List<PlanetData>();


    // Token: 0x04000893 RID: 2195
    public ReactiveProperty<bool> IsMuted = new ReactiveProperty<bool>();

    // Token: 0x04000895 RID: 2197
    private static GameController _Instance;

    // Token: 0x04000896 RID: 2198
    private ReactiveProperty<GameState> _State = new ReactiveProperty<GameState>();

    // Token: 0x04000897 RID: 2199
    [HideInInspector]
    public double offlineTime;

    // Token: 0x04000899 RID: 2201
    private float saveTimePeriod = 30f;

    // Token: 0x0400089B RID: 2203
    private string lastPlanetName;

    // Token: 0x0400089C RID: 2204
    public ReactiveProperty<bool> IsInitialized = new ReactiveProperty<bool>();

    // Token: 0x0400089D RID: 2205
    private CompositeDisposable stateDisposables = new CompositeDisposable();

    // Token: 0x0400089E RID: 2206
    private CompositeDisposable instanceDisposables = new CompositeDisposable();

    // Token: 0x0400089F RID: 2207
    private Platforms.Logger.Logger logger;

    // Token: 0x040008A0 RID: 2208
    private DateTime pauseTime;

    // Token: 0x040008A1 RID: 2209
    private GameState_Serialized nextGameStateSerialized;

    // Token: 0x040008A2 RID: 2210
    private PlanetData nextPlanetData;

    // Token: 0x040008A3 RID: 2211
    public string[] DefaultPlanets = new string[]
    {
        "Earth",
        "Moon",
        "Mars"
    };

    // Token: 0x0200083C RID: 2108
    private struct LegacyPlanetUnlockState
    {
        // Token: 0x04002A12 RID: 10770
        public int currentState;
    }
}
