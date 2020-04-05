using System;
using System.Collections.Generic;
using Platforms.Logger;
using UniRx;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

// Token: 0x0200003B RID: 59
public class AdHocRewardService : IDisposable
{
	// Token: 0x06000140 RID: 320 RVA: 0x00008171 File Offset: 0x00006371
	public void Dispose()
	{
		this.disposables.Dispose();
		this.stateDisposables.Dispose();
		this.timerServiceDisposable.Dispose();
	}

	// Token: 0x06000141 RID: 321 RVA: 0x00008194 File Offset: 0x00006394
	public void Init(IGameController gameController, IDateTimeService dateTimeService)
	{
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		this.logger.Info("Initializing");
		this.gameController = gameController;
		this.dateTimeService = dateTimeService;
		this.SetupRewards();
		this.timerServiceDisposable = this.gameController.TimerService.GetTimer(TimerService.TimerGroups.Global).Subscribe(new Action<TimeSpan>(this.Update)).AddTo(this.disposables);
		this.gameController.State.Subscribe(new Action<GameState>(this.OnGameStateChanged)).AddTo(this.disposables);
		this.logger.Info("Initialized");
	}

	// Token: 0x06000142 RID: 322 RVA: 0x0000823C File Offset: 0x0000643C
	private void SetupRewards()
	{
		this.adHocRewards.Add(new RewardData(TimeWarpService.TIME_WARP_EXPRESS_ITEM_ID, ERewardType.Item, 1));
		this.adHocRewards.Add(new RewardData("megabucks", ERewardType.Item, 2));
		this.adHocRewards.Add(new RewardData("megabucks", ERewardType.Item, 3));
		this.adHocRewards.Add(new RewardData("gold", ERewardType.Gold, 2));
		this.adHocRewards.Add(new RewardData("gold", ERewardType.Gold, 3));
	}

	// Token: 0x06000143 RID: 323 RVA: 0x000082BC File Offset: 0x000064BC
	private void OnGameStateChanged(GameState state)
	{
		this.stateDisposables.Clear();
		this.gameState = state;
		this.CurrentReward.Value = null;
		if (this.gameState.planetName == "Earth")
		{
			this.SetupAdHocTimers();
			(from x in this.rewardTimer
			where x <= 0.0
			select x).DistinctUntilChanged<double>().Subscribe(delegate(double x)
			{
				this.OnAdHocReady();
			}).AddTo(this.stateDisposables);
			MessageBroker.Default.Receive<OnAdHocSkippedEvent>().Subscribe(new Action<OnAdHocSkippedEvent>(this.OnAdHocRewardSkipped)).AddTo(this.stateDisposables);
		}
	}

	// Token: 0x06000144 RID: 324 RVA: 0x00008378 File Offset: 0x00006578
	public void OnAdHocWatched()
	{
		this.NullCheckDebuging();
		if (this.CurrentReward != null && this.CurrentReward.Value != null)
		{
			this.CurrentReward.Value = null;
			this.rewardTimer.Value = 7200.0;
			PlayerPrefs.SetString("NextAdhocRewardTime", this.dateTimeService.UtcNow.AddSeconds(7200.0).ToString());
		}
	}

	// Token: 0x06000145 RID: 325 RVA: 0x000083F0 File Offset: 0x000065F0
	private void NullCheckDebuging()
	{
		string text = "";
		if (this.CurrentReward == null)
		{
			text += "AdHocReward;";
		}
		else if (this.CurrentReward.Value == null)
		{
			text += "AdHocReward.Value;";
		}
		if (this.rewardTimer == null)
		{
			text += "AdHocRewardTimer;";
		}
		if (this.dateTimeService == null)
		{
			text += "dateTimeService;";
		}
		if (!string.IsNullOrEmpty(text))
		{
			this.gameController.AnalyticService.SendNavActionAnalytics("AdService", "DebugNull", text);
		}
	}

	// Token: 0x06000146 RID: 326 RVA: 0x00008480 File Offset: 0x00006680
	private void SetupAdHocTimers()
	{
		double value = 7200.0;
		if (!PlayerPrefs.HasKey("NextAdhocRewardTime"))
		{
			PlayerPrefs.SetString("NextAdhocRewardTime", this.dateTimeService.UtcNow.AddSeconds(7200.0).ToString());
		}
		else
		{
			value = DateTime.Parse(PlayerPrefs.GetString("NextAdhocRewardTime")).Subtract(this.dateTimeService.UtcNow).TotalSeconds;
		}
		this.rewardTimer.Value = value;
	}

	// Token: 0x06000147 RID: 327 RVA: 0x0000850C File Offset: 0x0000670C
	private void OnAdHocReady()
	{
		int index = Random.Range(0, this.adHocRewards.Count);
		this.CurrentReward.Value = this.adHocRewards[index];
	}

	// Token: 0x06000148 RID: 328 RVA: 0x00008544 File Offset: 0x00006744
	private void OnAdHocRewardSkipped(OnAdHocSkippedEvent pressed)
	{
		this.CurrentReward.Value = null;
		this.rewardTimer.Value = 7200.0;
		PlayerPrefs.SetString("NextAdhocRewardTime", this.dateTimeService.UtcNow.AddSeconds(7200.0).ToString());
	}

	// Token: 0x06000149 RID: 329 RVA: 0x000085A0 File Offset: 0x000067A0
	private void Update(TimeSpan deltaSpan)
	{
		double totalSeconds = deltaSpan.TotalSeconds;
		if (this.rewardTimer.Value > 0.0)
		{
			this.rewardTimer.Value -= totalSeconds;
		}
	}

	// Token: 0x0400016A RID: 362
	public ReactiveProperty<RewardData> CurrentReward = new ReactiveProperty<RewardData>();

	// Token: 0x0400016B RID: 363
	private readonly ReactiveProperty<double> rewardTimer = new ReactiveProperty<double>();

	// Token: 0x0400016C RID: 364
	private List<RewardData> adHocRewards = new List<RewardData>();

	// Token: 0x0400016D RID: 365
	private const int ADHOC_AD_INTERVAL_SECONDS = 7200;

	// Token: 0x0400016E RID: 366
	private const string NEXT_ADHOC_REWARD_TIME = "NextAdhocRewardTime";

	// Token: 0x0400016F RID: 367
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000170 RID: 368
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x04000171 RID: 369
	private IDisposable timerServiceDisposable = Disposable.Empty;

	// Token: 0x04000172 RID: 370
	private GameState gameState;

	// Token: 0x04000173 RID: 371
	private IDateTimeService dateTimeService;

	// Token: 0x04000174 RID: 372
	private IGameController gameController;

	// Token: 0x04000175 RID: 373
	private Platforms.Logger.Logger logger;
}
