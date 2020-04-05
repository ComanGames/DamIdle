using System;
using Platforms;
using PlayFab;
using PlayFab.ClientModels;
using UniRx;
using UnityEngine;

// Token: 0x020000A4 RID: 164
public class DateTimeService : IDateTimeService, IDisposable
{
	// Token: 0x1700006A RID: 106
	// (get) Token: 0x06000471 RID: 1137 RVA: 0x00017E79 File Offset: 0x00016079
	// (set) Token: 0x06000472 RID: 1138 RVA: 0x00017E81 File Offset: 0x00016081
	public PlatformAccount PlatformAccount { get; private set; }

	// Token: 0x1700006B RID: 107
	// (get) Token: 0x06000473 RID: 1139 RVA: 0x00017E8A File Offset: 0x0001608A
	public DateTime UtcNow
	{
		get
		{
			return this.now + TimeSpan.FromSeconds((double)this.nowOffset);
		}
	}

	// Token: 0x1700006C RID: 108
	// (get) Token: 0x06000474 RID: 1140 RVA: 0x00017EA3 File Offset: 0x000160A3
	public bool IsStarted
	{
		get
		{
			return this.started;
		}
	}

	// Token: 0x06000475 RID: 1141 RVA: 0x00017EAB File Offset: 0x000160AB
	public void Init(PlatformAccount pAccount)
	{
		this.platformAccount = pAccount;
	}

	// Token: 0x06000476 RID: 1142 RVA: 0x00017EB4 File Offset: 0x000160B4
	public void Start(Action onComplete, Action onError)
	{
		if (this.started)
		{
			if (onComplete != null)
			{
				onComplete();
			}
			return;
		}
		if (this.starting)
		{
			return;
		}
		this.starting = true;
		Observable.EveryUpdate().Subscribe(delegate(long _)
		{
			this.Update();
		});
		Action<Unit> <>9__2;
		Action<Exception> <>9__3;
		Observable.Interval(TimeSpan.FromMinutes(10.0)).StartWith(0L).Subscribe(delegate(long _)
		{
			IObservable<Unit> source = this.RetrieveAndUpdateUTCNowWithServerTime();
			Action<Unit> onNext;
			if ((onNext = <>9__2) == null)
			{
				onNext = (<>9__2 = delegate(Unit ___)
				{
					if (onComplete != null)
					{
						onComplete();
					}
					onComplete = null;
					onError = null;
				});
			}
			Action<Exception> onError2;
			if ((onError2 = <>9__3) == null)
			{
				onError2 = (<>9__3 = delegate(Exception error)
				{
					if (onError != null)
					{
						onError();
					}
					onComplete = null;
					onError = null;
				});
			}
			source.Subscribe(onNext, onError2).AddTo(this.intervalDisposables);
		}).AddTo(this.disposables);
	}

	// Token: 0x06000477 RID: 1143 RVA: 0x00017F56 File Offset: 0x00016156
	public IObservable<Unit> ForceServerTimeUpdate()
	{
		return this.RetrieveAndUpdateUTCNowWithServerTime();
	}

	// Token: 0x06000478 RID: 1144 RVA: 0x00017F5E File Offset: 0x0001615E
	public void SetNow(DateTime now)
	{
		this.now = now.ToUniversalTime();
		this.nowOffset = 0f;
	}

	// Token: 0x06000479 RID: 1145 RVA: 0x00017F78 File Offset: 0x00016178
	public void Dispose()
	{
		this.disposables.Dispose();
		this.intervalDisposables.Dispose();
	}

	// Token: 0x0600047A RID: 1146 RVA: 0x00017F90 File Offset: 0x00016190
	private IObservable<Unit> RetrieveAndUpdateUTCNowWithServerTime()
	{
		if (!this.platformAccount.IsLoggedIn)
		{
			Debug.LogError("[DateTimeService](SetNowWithServerTime) CLIENT IS NOT LOGGED IN!!!");
		}
		return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
		{
			this.platformAccount.PlayFab.GetTime(delegate(GetTimeResult result)
			{
				this.SetNow(result.Time);
				this.started = true;
				this.starting = false;
				observer.OnNext(Unit.Default);
				observer.OnCompleted();
			}, delegate(PlayFabError _)
			{
				observer.OnError(_.ToException());
			});
			return Disposable.Empty;
		}).Retry(2);
	}

	// Token: 0x0600047B RID: 1147 RVA: 0x00017FC0 File Offset: 0x000161C0
	private void Update()
	{
		this.nowOffset += Time.deltaTime;
	}

	// Token: 0x040003F1 RID: 1009
	private const float SERVER_UPDATE_INTERVAL_MINUTES = 10f;

	// Token: 0x040003F2 RID: 1010
	private DateTime now = DateTime.UtcNow;

	// Token: 0x040003F3 RID: 1011
	private float nowOffset;

	// Token: 0x040003F4 RID: 1012
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040003F5 RID: 1013
	private CompositeDisposable intervalDisposables = new CompositeDisposable();

	// Token: 0x040003F6 RID: 1014
	private bool starting;

	// Token: 0x040003F7 RID: 1015
	private bool started;

	// Token: 0x040003F8 RID: 1016
	private PlatformAccount platformAccount;
}
