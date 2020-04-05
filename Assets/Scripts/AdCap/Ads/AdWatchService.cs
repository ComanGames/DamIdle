using System;
using Platforms;
using Platforms.Ad;
using Platforms.Logger;
using UniRx;

namespace AdCap.Ads
{
	// Token: 0x02000702 RID: 1794
	public class AdWatchService : IDisposable
	{
		// Token: 0x0600251C RID: 9500 RVA: 0x000A1436 File Offset: 0x0009F636
		public void Init()
		{
			this.logger = Logger.GetLogger(this);
			this.logger.Info("Initializing");
			this.InitPlatformAds();
			this.logger.Info("Initialized");
		}

		// Token: 0x0600251D RID: 9501 RVA: 0x000A146A File Offset: 0x0009F66A
		public void Dispose()
		{
			this.disposables.Dispose();
		}

		// Token: 0x0600251E RID: 9502 RVA: 0x000A1477 File Offset: 0x0009F677
		private void InitPlatformAds()
		{
			this.platformAd = Helper.GetPlatformAd();
			this.platformAd.AdReadyMap[AdType.RewardedVideo].Subscribe(delegate(bool x)
			{
				this.RewardedVideoAdAvailable.Value = x;
			}).AddTo(this.disposables);
		}

		// Token: 0x0600251F RID: 9503 RVA: 0x000A14B2 File Offset: 0x0009F6B2
		public IObservable<Unit> WatchAd(AdType type, string adPlacementName)
		{
			this.AdState.Value = AdWatchService.EAdServiceState.Watching;
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				Action<ShowResult> <>9__4;
				Action<Exception> <>9__5;
				this.platformAd.AdReadyMap[type].First((bool x) => x).Timeout(TimeSpan.FromSeconds(5.0)).Subscribe(delegate(bool _)
				{
					IObservable<ShowResult> source = this.platformAd.ShowAd(type, adPlacementName);
					Action<ShowResult> onNext;
					if ((onNext = <>9__4) == null)
					{
						onNext = (<>9__4 = delegate(ShowResult result)
						{
							this.AdState.Value = AdWatchService.EAdServiceState.Idle;
							if (result == ShowResult.Finished)
							{
								observer.OnNext(Unit.Default);
								observer.OnCompleted();
								MessageBroker.Default.Publish<AdWatchedEvent>(new AdWatchedEvent
								{
									AdWatchEventType = EAdWatchEventType.Completed
								});
								return;
							}
							if (result == ShowResult.Skipped)
							{
								observer.OnCompleted();
								return;
							}
							string text = string.Format("Error {0} occured watching ad with placement {1}", result, adPlacementName);
							observer.OnError(new Exception(text));
							MessageBroker.Default.Publish<AdWatchedEvent>(new AdWatchedEvent
							{
								AdWatchEventType = EAdWatchEventType.Failed,
								Error = text
							});
						});
					}
					Action<Exception> onError;
					if ((onError = <>9__5) == null)
					{
						onError = (<>9__5 = delegate(Exception e)
						{
							this.AdState.Value = AdWatchService.EAdServiceState.Idle;
							observer.OnError(e);
							MessageBroker.Default.Publish<AdWatchedEvent>(new AdWatchedEvent
							{
								AdWatchEventType = EAdWatchEventType.Failed,
								Error = e.Message
							});
						});
					}
					source.Subscribe(onNext, onError);
				}, delegate(Exception e)
				{
					this.AdState.Value = AdWatchService.EAdServiceState.Idle;
					observer.OnError(new Exception("No Ads Available"));
					MessageBroker.Default.Publish<AdWatchedEvent>(new AdWatchedEvent
					{
						AdWatchEventType = EAdWatchEventType.Failed,
						Error = "No Ads Available"
					});
				});
				return Disposable.Empty;
			});
		}

		// Token: 0x04002613 RID: 9747
		public readonly ReactiveProperty<bool> RewardedVideoAdAvailable = new ReactiveProperty<bool>();

		// Token: 0x04002614 RID: 9748
		public readonly ReactiveProperty<AdWatchService.EAdServiceState> AdState = new ReactiveProperty<AdWatchService.EAdServiceState>(AdWatchService.EAdServiceState.Idle);

		// Token: 0x04002615 RID: 9749
		private const int AD_TIMEOUT = 5;

		// Token: 0x04002616 RID: 9750
		private GameState gameState;

		// Token: 0x04002617 RID: 9751
		private Logger logger;

		// Token: 0x04002618 RID: 9752
		private IPlatformAd platformAd;

		// Token: 0x04002619 RID: 9753
		private CompositeDisposable disposables = new CompositeDisposable();

		// Token: 0x02000A58 RID: 2648
		public enum EAdServiceState
		{
			// Token: 0x040030A6 RID: 12454
			Idle,
			// Token: 0x040030A7 RID: 12455
			Watching
		}
	}
}
