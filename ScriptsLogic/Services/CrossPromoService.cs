using System;
using UniRx;

// Token: 0x02000052 RID: 82
public class CrossPromoService : IDisposable
{
	// Token: 0x060002A8 RID: 680 RVA: 0x0000F09F File Offset: 0x0000D29F
	public void Init(IGameController gameController, NavigationService navigationService, IAnalyticService analyticService, int secondsToWait)
	{
		this.gameController = gameController;
		this.navigationService = navigationService;
		this.analyticService = analyticService;
		this.secondsToWait = secondsToWait;
		this.gameController.OnLoadNewPlanetPre += this.OnPreStateChange;
	}

	// Token: 0x060002A9 RID: 681 RVA: 0x0000F0D8 File Offset: 0x0000D2D8
	public void Dispose()
	{
		CompositeDisposable compositeDisposable = this.stateDisposables;
		if (compositeDisposable != null)
		{
			compositeDisposable.Dispose();
		}
		CompositeDisposable compositeDisposable2 = this.timerDisposables;
		if (compositeDisposable2 != null)
		{
			compositeDisposable2.Dispose();
		}
		CompositeDisposable compositeDisposable3 = this.disposables;
		if (compositeDisposable3 != null)
		{
			compositeDisposable3.Dispose();
		}
		if (this.gameController != null)
		{
			this.gameController.OnLoadNewPlanetPre -= this.OnPreStateChange;
		}
	}

	// Token: 0x060002AA RID: 682 RVA: 0x0000F137 File Offset: 0x0000D337
	private void OnPreStateChange()
	{
		this.timerDisposables.Clear();
		this.stateDisposables.Clear();
		MessageBroker.Default.Receive<WelcomeBackSequenceCompleted>().Subscribe(delegate(WelcomeBackSequenceCompleted _)
		{
			if (this.navigationService.CurrentLocation.Value == "Root" && this.secondsToWait <= 0)
			{
				this.FireRootAnalytic();
				return;
			}
			this.navigationService.CurrentLocation.Subscribe(new Action<string>(this.OnLocationChanged)).AddTo(this.stateDisposables);
		}).AddTo(this.stateDisposables);
	}

	// Token: 0x060002AB RID: 683 RVA: 0x0000F178 File Offset: 0x0000D378
	private void OnLocationChanged(string newLocation)
	{
		this.timerDisposables.Clear();
		if (newLocation == "Root")
		{
			if (this.secondsToWait > 0)
			{
				Observable.Interval(TimeSpan.FromSeconds((double)this.secondsToWait)).Subscribe(delegate(long _)
				{
					this.FireRootAnalytic();
				}).AddTo(this.timerDisposables);
				return;
			}
			this.FireRootAnalytic();
		}
	}

	// Token: 0x060002AC RID: 684 RVA: 0x0000F1DB File Offset: 0x0000D3DB
	private void FireRootAnalytic()
	{
		this.timerDisposables.Clear();
		this.stateDisposables.Clear();
		this.analyticService.SendCampaignEvent("RootInactive");
	}

	// Token: 0x04000248 RID: 584
	private IGameController gameController;

	// Token: 0x04000249 RID: 585
	private NavigationService navigationService;

	// Token: 0x0400024A RID: 586
	private IAnalyticService analyticService;

	// Token: 0x0400024B RID: 587
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x0400024C RID: 588
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x0400024D RID: 589
	private CompositeDisposable timerDisposables = new CompositeDisposable();

	// Token: 0x0400024E RID: 590
	private int secondsToWait;
}
