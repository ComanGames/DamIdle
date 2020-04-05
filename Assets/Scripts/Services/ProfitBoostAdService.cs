using System;
using System.Collections.Generic;
using System.Linq;
using Platforms.Logger;
using UniRx;
using Utils;

// Token: 0x02000043 RID: 67
public class ProfitBoostAdService : IDisposable
{
	// Token: 0x1700001A RID: 26
	// (get) Token: 0x06000184 RID: 388 RVA: 0x00009543 File Offset: 0x00007743
	public bool VideoMultiplierActive
	{
		get
		{
			return this.AdProfitBoostTimer.Value > 0.0;
		}
	}

	// Token: 0x06000185 RID: 389 RVA: 0x0000955C File Offset: 0x0000775C
	public void Init(IGameController gameController, IDateTimeService dateTimeService)
	{
		this.logger = Logger.GetLogger(this);
		this.logger.Info("Initializing");
		this.gameController = gameController;
		this.dateTimeService = dateTimeService;
		this.gameController.State.Subscribe(new Action<GameState>(this.OnGameStateChanged)).AddTo(this.disposables);
		MessageBroker.Default.Receive<InventoryEquipMessage>().Subscribe(new Action<InventoryEquipMessage>(this.OnItemEquipStateChanged)).AddTo(this.disposables);
		this.timerServiceDisposable = this.gameController.TimerService.GetTimer(TimerService.TimerGroups.Global).Subscribe(new Action<TimeSpan>(this.Update)).AddTo(this.disposables);
		this.gameController.OnLoadNewPlanetPre += this.OnPrePlanetLoad;
		this.logger.Info("Initialized");
	}

	// Token: 0x06000186 RID: 390 RVA: 0x0000963C File Offset: 0x0000783C
	public void Dispose()
	{
		this.disposables.Dispose();
		this.stateDisposables.Dispose();
		this.timerServiceDisposable.Dispose();
	}

	// Token: 0x06000187 RID: 391 RVA: 0x00009660 File Offset: 0x00007860
	public void OnProfitBoostAdWatched()
	{
		this.AdProfitBoostTimer.Value += 14400.0;
		this.ProfitAdExpiry = this.dateTimeService.UtcNow.AddSeconds(this.AdProfitBoostTimer.Value);
		this.gameState.SetMultiplierBonus(GameState.MultiplierBonusType.Ad, this.AdMultiplierBonus.Value);
		ReactiveProperty<int> availableProfitAds = this.AvailableProfitAds;
		int value = availableProfitAds.Value - 1;
		availableProfitAds.Value = value;
	}

	// Token: 0x06000188 RID: 392 RVA: 0x000096D8 File Offset: 0x000078D8
	private void OnGameStateChanged(GameState state)
	{
		this.stateDisposables.Clear();
		this.gameState = state;
		this.AvailableProfitAds.Value = this.gameState.AvailableAds;
		this.ProfitAdExpiry = this.gameState.ProfitAdExpiry;
		this.AdMultiplierBonus.Value = this.gameState.AdMultiplierBonus;
		this.VideosCurrentDay = this.gameState.VideosCurrentDay;
		if (!this.gameState.IsEventPlanet)
		{
			this.gameController.GlobalPlayerData.GetObservable("ad_watch_boost", 2.0).Subscribe(new Action<double>(this.SetAdWatchBonus)).AddTo(this.stateDisposables);
		}
		else
		{
			this.SetAdWatchBonus(2.0);
		}
		this.SetupProfitBoost();
		(from x in this.AdProfitBoostTimer
		where x <= 0.0
		select x).DistinctUntilChanged<double>().Subscribe(delegate(double x)
		{
			this.RemoveProfitBoostBonus();
		}).AddTo(this.stateDisposables);
		foreach (Item item in this.gameController.GlobalPlayerData.inventory.GetAllEquippedItems())
		{
			this.AdjustItemBonuses(item, true);
		}
		this.itemAdWatchBonus.Subscribe(delegate(float itemBonus)
		{
			this.SetGameStateAdWatchBonus();
		}).AddTo(this.stateDisposables);
	}

	// Token: 0x06000189 RID: 393 RVA: 0x0000986C File Offset: 0x00007A6C
	private void SetAdWatchBonus(double baseBonus)
	{
		this.baseAdWatchBonus = baseBonus;
		this.SetGameStateAdWatchBonus();
	}

	// Token: 0x0600018A RID: 394 RVA: 0x0000987C File Offset: 0x00007A7C
	private void SetGameStateAdWatchBonus()
	{
		this.AdMultiplierBonus.Value = (float)(this.baseAdWatchBonus * (double)this.itemAdWatchBonus.Value);
		if (this.AdProfitBoostTimer.Value > 0.0)
		{
			this.gameState.SetMultiplierBonus(GameState.MultiplierBonusType.Ad, this.AdMultiplierBonus.Value);
		}
	}

	// Token: 0x0600018B RID: 395 RVA: 0x000098D8 File Offset: 0x00007AD8
	private void SetupProfitBoost()
	{
		int dayOfYear = this.dateTimeService.UtcNow.DayOfYear;
		if (this.VideosCurrentDay != dayOfYear)
		{
			this.VideosCurrentDay = dayOfYear;
			this.AvailableProfitAds.Value = 6;
		}
		this.AdProfitBoostTimer.Value = this.ProfitAdExpiry.Subtract(this.dateTimeService.UtcNow).TotalSeconds;
	}

	// Token: 0x0600018C RID: 396 RVA: 0x0000993E File Offset: 0x00007B3E
	private void RemoveProfitBoostBonus()
	{
		this.gameState.RemoveMultiplierBonus(GameState.MultiplierBonusType.Ad);
		this.AdProfitBoostTimer.Value = 0.0;
	}

	// Token: 0x0600018D RID: 397 RVA: 0x00009960 File Offset: 0x00007B60
	private void Update(TimeSpan deltaSpan)
	{
		double totalSeconds = deltaSpan.TotalSeconds;
		if (this.AdProfitBoostTimer.Value > 0.0)
		{
			this.AdProfitBoostTimer.Value -= totalSeconds;
		}
	}

	// Token: 0x0600018E RID: 398 RVA: 0x0000999E File Offset: 0x00007B9E
	private void OnPrePlanetLoad()
	{
		if (this.itemAdWatchDisposable != null)
		{
			this.itemAdWatchDisposable.Dispose();
		}
		this.itemAdWatchBonuses.Clear();
		this.itemAdWatchBonus.Value = 1f;
	}

	// Token: 0x0600018F RID: 399 RVA: 0x000099CE File Offset: 0x00007BCE
	private void OnItemEquipStateChanged(InventoryEquipMessage inventoryEquipMessage)
	{
		this.AdjustItemBonuses(inventoryEquipMessage.item, inventoryEquipMessage.equipped);
	}

	// Token: 0x06000190 RID: 400 RVA: 0x000099E4 File Offset: 0x00007BE4
	private void AdjustItemBonuses(Item item, bool equipped)
	{
		if (item.ItemBonusTarget != ItemBonusTarget.Ads)
		{
			return;
		}
		if (!string.IsNullOrEmpty(item.BonusCustomData))
		{
			string[] array = item.BonusCustomData.Split(new char[]
			{
				':'
			});
			if (array.Length == 1)
			{
				if (this.gameState.IsEventPlanet)
				{
					if (array[0] != this.gameState.PlanetData.PlanetName && array[0] != "AllEvents")
					{
						return;
					}
				}
				else if (array[0] != this.gameState.PlanetData.PlanetName && array[0] != "AllPlanets")
				{
					return;
				}
			}
		}
		this.itemAdWatchDisposable.Dispose();
		if (equipped)
		{
			this.itemAdWatchBonuses.Add(item.CurrentLeveledBonus);
		}
		else
		{
			this.itemAdWatchBonuses.Remove(item.CurrentLeveledBonus);
		}
		if (this.itemAdWatchBonuses.Count == 0)
		{
			this.itemAdWatchBonus.Value = 1f;
			return;
		}
		this.itemAdWatchDisposable = (from v in this.itemAdWatchBonuses
		select v).CombineLatest<float>().Subscribe(delegate(IList<float> v)
		{
			this.itemAdWatchBonus.Value = v.Sum();
		});
	}

	// Token: 0x040001BE RID: 446
	public const int TOTAL_PROFIT_ADS_PER_DAY = 6;

	// Token: 0x040001BF RID: 447
	public const int PROFIT_AD_DURATION_SECONDS = 14400;

	// Token: 0x040001C0 RID: 448
	public DateTime ProfitAdExpiry;

	// Token: 0x040001C1 RID: 449
	public readonly ReactiveProperty<float> AdMultiplierBonus = new ReactiveProperty<float>(2f);

	// Token: 0x040001C2 RID: 450
	public readonly ReactiveProperty<double> AdProfitBoostTimer = new ReactiveProperty<double>();

	// Token: 0x040001C3 RID: 451
	public readonly ReactiveProperty<int> AvailableProfitAds = new ReactiveProperty<int>();

	// Token: 0x040001C4 RID: 452
	public int VideosCurrentDay = -1;

	// Token: 0x040001C5 RID: 453
	private double baseAdWatchBonus;

	// Token: 0x040001C6 RID: 454
	private Logger logger;

	// Token: 0x040001C7 RID: 455
	private GameState gameState;

	// Token: 0x040001C8 RID: 456
	private IGameController gameController;

	// Token: 0x040001C9 RID: 457
	private IDateTimeService dateTimeService;

	// Token: 0x040001CA RID: 458
	private List<ReactiveProperty<float>> itemAdWatchBonuses = new List<ReactiveProperty<float>>();

	// Token: 0x040001CB RID: 459
	private ReactiveProperty<float> itemAdWatchBonus = new ReactiveProperty<float>(1f);

	// Token: 0x040001CC RID: 460
	private IDisposable itemAdWatchDisposable = Disposable.Empty;

	// Token: 0x040001CD RID: 461
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040001CE RID: 462
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x040001CF RID: 463
	private IDisposable timerServiceDisposable = Disposable.Empty;
}
