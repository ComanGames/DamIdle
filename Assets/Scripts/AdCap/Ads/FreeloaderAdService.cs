using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using Platforms;
using Platforms.Ad;
using Platforms.Logger;
using UniRx;
using UnityEngine;

namespace AdCap.Ads
{
	// Token: 0x02000703 RID: 1795
	public class FreeloaderAdService : IDisposable
	{
		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06002522 RID: 9506 RVA: 0x000A1522 File Offset: 0x0009F722
		// (set) Token: 0x06002523 RID: 9507 RVA: 0x000A152A File Offset: 0x0009F72A
		public ReadOnlyReactiveProperty<bool> ShouldShowAd { get; private set; }

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06002524 RID: 9508 RVA: 0x000A1533 File Offset: 0x0009F733
		// (set) Token: 0x06002525 RID: 9509 RVA: 0x000A153B File Offset: 0x0009F73B
		public bool TryAndShowAdAfterWelcomeBack { get; private set; }

		// Token: 0x17000316 RID: 790
		// (get) Token: 0x06002526 RID: 9510 RVA: 0x000A1544 File Offset: 0x0009F744
		// (set) Token: 0x06002527 RID: 9511 RVA: 0x000A154C File Offset: 0x0009F74C
		public bool TryAndShowAdAfterAngelClaim { get; private set; }

		// Token: 0x06002528 RID: 9512 RVA: 0x000A1558 File Offset: 0x0009F758
		public void Init(IObservable<TimeSpan> timer, IDateTimeService dateTimeService, FreeloaderConfig[] configs, IUserDataService userDataService, ReactiveProperty<int> totalRMPurchaseCount, IPlatformAd platformAd)
		{
			this.logger = Platforms.Logger.Logger.GetLogger(this);
			this.logger.Info("Initializing");
			this.timer = timer;
			this.dateTimeService = dateTimeService;
			this.userDataService = userDataService;
			this.platformAd = platformAd;
			IObservable<bool>[] array = new IObservable<bool>[6];
			array[0] = this.isActive;
			array[1] = from x in this.isPerminantlyExemptFromAds
			select !x;
			array[2] = this.AdAvailable;
			array[3] = this.AdReady;
			array[4] = this.IsWithinAdWatchLimit;
			array[5] = from x in this.isWatchingAd
			select !x;
			IObservable<bool>[] sources = array;
			this.ShouldShowAd = (from list in sources.CombineLatest<bool>()
			select list.All((bool v) => v)).ToReadOnlyReactiveProperty<bool>();
			this.isActive.Skip(1).Subscribe(delegate(bool _)
			{
				this.LogState("IsActive");
			}).AddTo(this.disposables);
			this.isPerminantlyExemptFromAds.Skip(1).Subscribe(delegate(bool _)
			{
				this.LogState("Perminant");
			}).AddTo(this.disposables);
			this.AdAvailable.Skip(1).Subscribe(delegate(bool _)
			{
				this.LogState("AdAvailable");
			}).AddTo(this.disposables);
			this.AdReady.Skip(1).Subscribe(delegate(bool _)
			{
				this.LogState("AdReady");
			}).AddTo(this.disposables);
			this.IsWithinAdWatchLimit.Skip(1).Subscribe(delegate(bool _)
			{
				this.LogState("IsWithinAdWatchLimit");
			}).AddTo(this.disposables);
			this.isWatchingAd.Skip(1).Subscribe(delegate(bool _)
			{
				this.LogState("isWatchingAd");
			}).AddTo(this.disposables);
			if (configs == null || configs.Length == 0)
			{
				this.logger.Error("Unable to initialize did not receive any freeloader configs");
				return;
			}
			FreeloaderConfig freeloaderConfig = null;
			int i = 0;
			IL_264:
			while (i < configs.Length)
			{
				FreeloaderConfig freeloaderConfig2 = configs[i];
				if (!string.IsNullOrEmpty(freeloaderConfig2.ABTestGroups))
				{
					string[] array2 = freeloaderConfig2.ABTestGroups.Split(new char[]
					{
						','
					});
					for (int j = 0; j < array2.Length; j++)
					{
						if (this.userDataService.IsTestGroupMember(array2[j]))
						{
							freeloaderConfig = freeloaderConfig2;
							IL_260:
							i++;
							goto IL_264;
						}
					}
					goto IL_260;
				}
				freeloaderConfig = freeloaderConfig2;
				break;
			}
			if (freeloaderConfig == null)
			{
				return;
			}
			this.RetrieveValuesFromConfig(freeloaderConfig);
			if (this.isActive.Value)
			{
				this.RestoreSavedValues();
				if (!this.isPerminantlyExemptFromAds.Value)
				{
					this.InitPlatformAds();
					(from x in MessageBroker.Default.Receive<AdWatchedEvent>()
					where x.AdWatchEventType == EAdWatchEventType.Completed
					select x).Subscribe(delegate(AdWatchedEvent _)
					{
						this.AddTimeToClock(this.REWARDED_VIDEO_TIME_ADDED_IN_SECONDS);
					}).AddTo(this.freeloadingDisposables);
					(from x in MessageBroker.Default.Receive<StorePurchaseEvent>()
					where x.PurchaseCurrency == AdCap.Store.Currency.Gold && x.PurchaseState == EStorePurchaseState.Success
					select x).Subscribe(delegate(StorePurchaseEvent x)
					{
						if (this.DOES_GOLD_PURCHASE_STOP_FREELOADING)
						{
							this.StopFreeloading();
							return;
						}
						this.AddTimeToClock(this.GOLD_PURCHASE_TIME_ADDED_IN_SECONDS);
					}).AddTo(this.freeloadingDisposables);
					(from x in totalRMPurchaseCount
					where x > 0
					select x).Subscribe(delegate(int x)
					{
						if (this.DOES_RM_PURCHASE_STOP_FREELOADING)
						{
							this.StopFreeloading();
							return;
						}
						this.AddTimeToClock(this.RM_PURCHASE_TIME_ADDED_IN_SECONDS);
					}).AddTo(this.freeloadingDisposables);
					(from x in MessageBroker.Default.Receive<StorePurchaseEvent>()
					where x.PurchaseCurrency == AdCap.Store.Currency.Cash && x.PurchaseState == EStorePurchaseState.Success
					select x).Subscribe(delegate(StorePurchaseEvent x)
					{
					}).AddTo(this.freeloadingDisposables);
					if (this.timeRemaining.Value > 0.0)
					{
						this.StartTimerIfNeeded();
					}
					else
					{
						this.AdAvailable.Value = true;
					}
				}
				this.logger.Info("Initialized");
			}
		}

		// Token: 0x06002529 RID: 9513 RVA: 0x000A1978 File Offset: 0x0009FB78
		public void Dispose()
		{
			if (this.timerDisposable != null)
			{
				this.timerDisposable.Dispose();
				this.timerDisposable = null;
			}
			if (this.freeloaderQueueDisposable != null)
			{
				this.freeloaderQueueDisposable.Dispose();
				this.freeloaderQueueDisposable = null;
			}
			this.freeloadingDisposables.Dispose();
			this.disposables.Dispose();
		}

		// Token: 0x0600252A RID: 9514 RVA: 0x000A19CF File Offset: 0x0009FBCF
		public IObservable<Unit> WatchAd(string adPlacementName)
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				this.isWatchingAd.Value = true;
				Action<ShowResult> <>9__4;
				Action<Exception> <>9__5;
				this.platformAd.AdReadyMap[AdType.Interstitial].First((bool x) => x).Timeout(TimeSpan.FromSeconds(5.0)).Subscribe(delegate(bool _)
				{
					GameController.Instance.AnalyticService.SendAdStartEvent("Freeloader", "Interstitial", GameController.Instance.game.planetName);
					IObservable<ShowResult> source = this.platformAd.ShowAd(AdType.Interstitial, adPlacementName);
					Action<ShowResult> onNext;
					if ((onNext = <>9__4) == null)
					{
						onNext = (<>9__4 = delegate(ShowResult __)
						{
							this.EnqueAdWatchOccurance();
							this.AddTimeToClock(this.FREELOADER_AD_WATCH_TIME_ADDED_IN_SECONDS);
							observer.OnNext(Unit.Default);
							observer.OnCompleted();
							this.isWatchingAd.Value = false;
							GameController.Instance.AnalyticService.SendAdFinished("Freeloader", "Interstitial", GameController.Instance.game.planetName, "");
						});
					}
					Action<Exception> onError;
					if ((onError = <>9__5) == null)
					{
						onError = (<>9__5 = delegate(Exception e)
						{
							observer.OnError(e);
							this.isWatchingAd.Value = false;
						});
					}
					source.Subscribe(onNext, onError);
				}, delegate(Exception e)
				{
					observer.OnError(new Exception("No Ads Available"));
					this.isWatchingAd.Value = false;
				});
				return Disposable.Empty;
			});
		}

		// Token: 0x0600252B RID: 9515 RVA: 0x000A19F4 File Offset: 0x0009FBF4
		private void RetrieveValuesFromConfig(FreeloaderConfig config)
		{
			this.isActive.Value = config.IsActive;
			this.INITIAL_AD_FREE_TIME_IN_SECONDS = config.InitialAdFreeTimeInSeconds;
			this.REWARDED_VIDEO_TIME_ADDED_IN_SECONDS = config.RewardedVideoTimeAddedInSeconds;
			this.DOES_RM_PURCHASE_STOP_FREELOADING = config.DoesRmPurchaseStopFreeloading;
			this.RM_PURCHASE_TIME_ADDED_IN_SECONDS = config.RmPurchaseTimeAddedInSeconds;
			this.DOES_GOLD_PURCHASE_STOP_FREELOADING = config.DoesGoldPurchaseStopFreeloading;
			this.GOLD_PURCHASE_TIME_ADDED_IN_SECONDS = config.GoldPurchaseTimeAddedInSeconds;
			this.FREELOADER_AD_WATCH_TIME_ADDED_IN_SECONDS = config.FreeloaderAdWatchTimeAddedInSeconds;
			this.MAX_ADS_TIME_FRAME_IN_SECONDS = config.MaxAdsTimeframeInSeconds;
			this.MAX_ADS_TIME_FRAME_AMOUNT = config.MaxAdsTimeframeAmount;
			this.TryAndShowAdAfterWelcomeBack = config.TryAndShowAdAfterWelcomeBack;
			this.TryAndShowAdAfterAngelClaim = config.TryAndShowAdAfterAngelClaim;
			this.MAX_ADFREE_TIME_IN_SECONDS = config.MaxAdfreeTimeInSeconds;
		}

		// Token: 0x0600252C RID: 9516 RVA: 0x000A1AA4 File Offset: 0x0009FCA4
		private void RestoreSavedValues()
		{
			this.isPerminantlyExemptFromAds.Value = (PlayerPrefs.GetString(FreeloaderAdService.IS_EXEMPT_FROM_FREELOADER_ADS_KEY, "false").ToLower() == "true");
			if (PlayerPrefs.HasKey(FreeloaderAdService.NEXT_FREELOADER_AD_TIME_KEY))
			{
				DateTime dateTime = DateTime.Parse(PlayerPrefs.GetString(FreeloaderAdService.NEXT_FREELOADER_AD_TIME_KEY));
				this.timeRemaining.Value = Math.Min(dateTime.Subtract(this.dateTimeService.UtcNow).TotalSeconds, this.MAX_ADFREE_TIME_IN_SECONDS);
			}
			else
			{
				this.timeRemaining.Value = (double)this.INITIAL_AD_FREE_TIME_IN_SECONDS;
			}
			if (PlayerPrefs.HasKey(FreeloaderAdService.PREVIOUS_FREELOADER_AD_WATCH_TIMES_KEY))
			{
				foreach (string text in PlayerPrefs.GetString(FreeloaderAdService.PREVIOUS_FREELOADER_AD_WATCH_TIMES_KEY).Split(new char[]
				{
					'|'
				}))
				{
					if (!string.IsNullOrEmpty(text))
					{
						DateTime item = DateTime.Parse(text);
						this.previousFreeloaderAdwatchTimes.Enqueue(item);
					}
				}
			}
			this.TrimQueue();
			this.SaveState();
		}

		// Token: 0x0600252D RID: 9517 RVA: 0x000A1B9F File Offset: 0x0009FD9F
		private void InitPlatformAds()
		{
			this.platformAd.AdReadyMap[AdType.Interstitial].Subscribe(delegate(bool x)
			{
				this.AdReady.Value = x;
			}).AddTo(this.disposables);
		}

		// Token: 0x0600252E RID: 9518 RVA: 0x000A1BCF File Offset: 0x0009FDCF
		private void StopFreeloading()
		{
			this.isPerminantlyExemptFromAds.Value = true;
			this.SaveState();
			this.freeloadingDisposables.Clear();
			if (this.timerDisposable != null)
			{
				this.timerDisposable.Dispose();
				this.timerDisposable = null;
			}
		}

		// Token: 0x0600252F RID: 9519 RVA: 0x000A1C08 File Offset: 0x0009FE08
		private void SaveState()
		{
			PlayerPrefs.SetString(FreeloaderAdService.IS_EXEMPT_FROM_FREELOADER_ADS_KEY, this.isPerminantlyExemptFromAds.Value.ToString());
			PlayerPrefs.SetString(FreeloaderAdService.NEXT_FREELOADER_AD_TIME_KEY, this.dateTimeService.UtcNow.AddSeconds(this.timeRemaining.Value).ToString());
			string text = "";
			if (this.previousFreeloaderAdwatchTimes.Count > 0)
			{
				DateTime[] array = this.previousFreeloaderAdwatchTimes.ToArray();
				text += array[0];
				for (int i = 1; i < array.Length; i++)
				{
					text += "|";
					text += array[i];
				}
			}
			PlayerPrefs.SetString(FreeloaderAdService.PREVIOUS_FREELOADER_AD_WATCH_TIMES_KEY, text);
		}

		// Token: 0x06002530 RID: 9520 RVA: 0x000A1CD4 File Offset: 0x0009FED4
		private void AddTimeToClock(int timeToAdd)
		{
			if (this.timeRemaining.Value < 0.0)
			{
				this.timeRemaining.Value = 0.0;
			}
			if (timeToAdd > 0)
			{
				this.timeRemaining.Value = Math.Min(this.timeRemaining.Value + (double)timeToAdd, this.MAX_ADFREE_TIME_IN_SECONDS);
				this.AdAvailable.Value = false;
				this.StartTimerIfNeeded();
			}
			this.LogState("[AddTime]");
			this.SaveState();
		}

		// Token: 0x06002531 RID: 9521 RVA: 0x000A1D56 File Offset: 0x0009FF56
		private void StartTimerIfNeeded()
		{
			if (this.timerDisposable == null)
			{
				this.timerDisposable = this.timer.Subscribe(new Action<TimeSpan>(this.Update));
			}
		}

		// Token: 0x06002532 RID: 9522 RVA: 0x000A1D80 File Offset: 0x0009FF80
		private void Update(TimeSpan deltaSpan)
		{
			if (this.timeRemaining.Value > 0.0)
			{
				double totalSeconds = deltaSpan.TotalSeconds;
				this.timeRemaining.Value -= totalSeconds;
				if (this.timeRemaining.Value <= 0.0)
				{
					this.AdAvailable.Value = true;
					this.timerDisposable.Dispose();
					this.timerDisposable = null;
					return;
				}
			}
			else
			{
				this.AdAvailable.Value = true;
			}
		}

		// Token: 0x06002533 RID: 9523 RVA: 0x000A1DFF File Offset: 0x0009FFFF
		private void EnqueAdWatchOccurance()
		{
			this.previousFreeloaderAdwatchTimes.Enqueue(this.dateTimeService.UtcNow);
			this.TrimQueue();
			this.SaveState();
		}

		// Token: 0x06002534 RID: 9524 RVA: 0x000A1E24 File Offset: 0x000A0024
		private void StartMonitoringQueue()
		{
			if (this.freeloaderQueueDisposable != null)
			{
				this.freeloaderQueueDisposable.Dispose();
				this.freeloaderQueueDisposable = null;
			}
			if (this.previousFreeloaderAdwatchTimes.Count > 0)
			{
				DateTime value = this.previousFreeloaderAdwatchTimes.Peek();
				double totalSeconds = this.dateTimeService.UtcNow.Subtract(value).TotalSeconds;
				int waittime = (int)((double)this.MAX_ADS_TIME_FRAME_IN_SECONDS - totalSeconds);
				this.freeloaderQueueDisposable = (from x in this.timer.Scan((TimeSpan x, TimeSpan y) => x + y)
				where x.TotalSeconds >= (double)waittime
				select x).Take(1).Subscribe(delegate(TimeSpan x)
				{
					this.TrimQueue();
				});
				return;
			}
			this.IsWithinAdWatchLimit.Value = !this.HasExceededAdwatchLimit();
		}

		// Token: 0x06002535 RID: 9525 RVA: 0x000A1F0C File Offset: 0x000A010C
		private void TrimQueue()
		{
			if (this.freeloaderQueueDisposable != null)
			{
				this.freeloaderQueueDisposable.Dispose();
				this.freeloaderQueueDisposable = null;
			}
			DateTime utcNow = this.dateTimeService.UtcNow;
			while (this.previousFreeloaderAdwatchTimes.Count > 0)
			{
				DateTime value = this.previousFreeloaderAdwatchTimes.Peek();
				if (utcNow.Subtract(value).TotalSeconds <= (double)this.MAX_ADS_TIME_FRAME_IN_SECONDS)
				{
					break;
				}
				this.previousFreeloaderAdwatchTimes.Dequeue();
			}
			this.IsWithinAdWatchLimit.Value = !this.HasExceededAdwatchLimit();
			if (this.previousFreeloaderAdwatchTimes.Count > 0)
			{
				this.StartMonitoringQueue();
			}
		}

		// Token: 0x06002536 RID: 9526 RVA: 0x000A1FA8 File Offset: 0x000A01A8
		private bool HasExceededAdwatchLimit()
		{
			return this.previousFreeloaderAdwatchTimes.Count >= this.MAX_ADS_TIME_FRAME_AMOUNT;
		}

		// Token: 0x06002537 RID: 9527 RVA: 0x000A1FC0 File Offset: 0x000A01C0
		private void LogState(string where)
		{
			this.logger.Info(string.Concat(new string[]
			{
				"(",
				where,
				") ShouldShowAd:",
				this.ShouldShowAd.Value.ToString(),
				" isActive=",
				this.isActive.Value.ToString(),
				", isPerminantlyExemptFromAds=",
				this.isPerminantlyExemptFromAds.Value.ToString(),
				", AdAvailable=",
				this.AdAvailable.Value.ToString(),
				", AdReady=",
				this.AdReady.Value.ToString(),
				", IsWithinAdWatchLimit=",
				this.IsWithinAdWatchLimit.Value.ToString()
			}));
		}

		// Token: 0x0400261D RID: 9757
		public static readonly string NEXT_FREELOADER_AD_TIME_KEY = "NEXT_FREELOADER_AD_TIME";

		// Token: 0x0400261E RID: 9758
		public static readonly string IS_EXEMPT_FROM_FREELOADER_ADS_KEY = "IS_EXEMPT_FROM_FREELOADER_ADS";

		// Token: 0x0400261F RID: 9759
		public static readonly string PREVIOUS_FREELOADER_AD_WATCH_TIMES_KEY = "PREVIOUS_FREELOADER_AD_WATCH_TIMES";

		// Token: 0x04002620 RID: 9760
		private const int AD_TIMEOUT = 5;

		// Token: 0x04002621 RID: 9761
		private Platforms.Logger.Logger logger;

		// Token: 0x04002622 RID: 9762
		private IPlatformAd platformAd;

		// Token: 0x04002623 RID: 9763
		private IDateTimeService dateTimeService;

		// Token: 0x04002624 RID: 9764
		private IUserDataService userDataService;

		// Token: 0x04002625 RID: 9765
		private IObservable<TimeSpan> timer;

		// Token: 0x04002626 RID: 9766
		private int INITIAL_AD_FREE_TIME_IN_SECONDS = 60;

		// Token: 0x04002627 RID: 9767
		private int REWARDED_VIDEO_TIME_ADDED_IN_SECONDS = 15;

		// Token: 0x04002628 RID: 9768
		private bool DOES_RM_PURCHASE_STOP_FREELOADING;

		// Token: 0x04002629 RID: 9769
		private int RM_PURCHASE_TIME_ADDED_IN_SECONDS = 15;

		// Token: 0x0400262A RID: 9770
		private bool DOES_GOLD_PURCHASE_STOP_FREELOADING;

		// Token: 0x0400262B RID: 9771
		private int GOLD_PURCHASE_TIME_ADDED_IN_SECONDS = 15;

		// Token: 0x0400262C RID: 9772
		private int FREELOADER_AD_WATCH_TIME_ADDED_IN_SECONDS = 10;

		// Token: 0x0400262D RID: 9773
		private int MAX_ADS_TIME_FRAME_IN_SECONDS = 60;

		// Token: 0x0400262E RID: 9774
		private int MAX_ADS_TIME_FRAME_AMOUNT = 2;

		// Token: 0x0400262F RID: 9775
		private double MAX_ADFREE_TIME_IN_SECONDS = 259200.0;

		// Token: 0x04002630 RID: 9776
		private readonly ReactiveProperty<bool> isActive = new ReactiveProperty<bool>(false);

		// Token: 0x04002631 RID: 9777
		private readonly ReactiveProperty<bool> AdAvailable = new ReactiveProperty<bool>(false);

		// Token: 0x04002632 RID: 9778
		private readonly ReactiveProperty<bool> AdReady = new ReactiveProperty<bool>(false);

		// Token: 0x04002633 RID: 9779
		private readonly ReactiveProperty<bool> IsWithinAdWatchLimit = new ReactiveProperty<bool>(false);

		// Token: 0x04002634 RID: 9780
		private readonly ReactiveProperty<bool> isPerminantlyExemptFromAds = new ReactiveProperty<bool>(false);

		// Token: 0x04002635 RID: 9781
		private readonly ReactiveProperty<bool> isWatchingAd = new ReactiveProperty<bool>(false);

		// Token: 0x04002636 RID: 9782
		private ReactiveProperty<double> timeRemaining = new ReactiveProperty<double>(0.0);

		// Token: 0x04002637 RID: 9783
		private Queue<DateTime> previousFreeloaderAdwatchTimes = new Queue<DateTime>();

		// Token: 0x04002638 RID: 9784
		private CompositeDisposable disposables = new CompositeDisposable();

		// Token: 0x04002639 RID: 9785
		private CompositeDisposable freeloadingDisposables = new CompositeDisposable();

		// Token: 0x0400263A RID: 9786
		private IDisposable timerDisposable;

		// Token: 0x0400263B RID: 9787
		private IDisposable freeloaderQueueDisposable;
	}
}
