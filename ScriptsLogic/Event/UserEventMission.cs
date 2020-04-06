using System;
using UniRx;

// Token: 0x0200005F RID: 95
public class UserEventMission : IDisposable
{
	// Token: 0x17000043 RID: 67
	// (get) Token: 0x06000336 RID: 822 RVA: 0x00013110 File Offset: 0x00011310
	// (set) Token: 0x06000337 RID: 823 RVA: 0x00013118 File Offset: 0x00011318
	public ReactiveProperty<double> TimeRemaining { get; private set; }

	// Token: 0x17000044 RID: 68
	// (get) Token: 0x06000338 RID: 824 RVA: 0x00013121 File Offset: 0x00011321
	// (set) Token: 0x06000339 RID: 825 RVA: 0x00013129 File Offset: 0x00011329
	public int ID { get; private set; }

	// Token: 0x17000045 RID: 69
	// (get) Token: 0x0600033A RID: 826 RVA: 0x00013132 File Offset: 0x00011332
	// (set) Token: 0x0600033B RID: 827 RVA: 0x0001313A File Offset: 0x0001133A
	public double TargetAmount { get; private set; }

	// Token: 0x17000046 RID: 70
	// (get) Token: 0x0600033C RID: 828 RVA: 0x00013143 File Offset: 0x00011343
	// (set) Token: 0x0600033D RID: 829 RVA: 0x0001314B File Offset: 0x0001134B
	public int RewardAmount { get; private set; }

	// Token: 0x0600033E RID: 830 RVA: 0x00013154 File Offset: 0x00011354
	public UserEventMission(EventMissionSaveData saveData, ReactiveProperty<double> timeRemaining = null)
	{
		this.TimeRemaining = timeRemaining;
		this.CurrentCount.Value = saveData.CurrentCount;
		this.IsClaimed.Value = saveData.IsClaimed;
		this.ID = saveData.ID;
		this.TargetAmount = saveData.TargetAmount;
		this.RewardAmount = saveData.RewardAmount;
		this.Type = saveData.Type;
		this.Venture = saveData.Venture;
		this.CurrentCount.Subscribe(delegate(double _)
		{
			this.UpdateState();
		}).AddTo(this.disposables);
		this.IsClaimed.Subscribe(delegate(bool _)
		{
			this.UpdateState();
		}).AddTo(this.disposables);
		if (this.TimeRemaining != null && this.State.Value == EventMissionState.ACTIVE)
		{
			this.TimeRemaining.TakeUntil((double _) => this.State.Value == EventMissionState.EXPIRED || this.State.Value == EventMissionState.COMPLETE).Subscribe(new Action<double>(this.OnTimeRemainingChanged)).AddTo(this.disposables);
		}
		this.UpdateState();
	}

	// Token: 0x0600033F RID: 831 RVA: 0x00013298 File Offset: 0x00011498
	public void Dispose()
	{
		if (this.CurrentCount != null)
		{
			this.CurrentCount.Dispose();
		}
		if (this.IsClaimed != null)
		{
			this.IsClaimed.Dispose();
		}
		if (this.State != null)
		{
			this.State.Dispose();
		}
		if (this.disposables != null)
		{
			this.disposables.Dispose();
		}
	}

	// Token: 0x17000047 RID: 71
	// (get) Token: 0x06000340 RID: 832 RVA: 0x000132F1 File Offset: 0x000114F1
	public bool IsTimed
	{
		get
		{
			return this.TimeRemaining != null;
		}
	}

	// Token: 0x17000048 RID: 72
	// (get) Token: 0x06000341 RID: 833 RVA: 0x000132FC File Offset: 0x000114FC
	// (set) Token: 0x06000342 RID: 834 RVA: 0x00013304 File Offset: 0x00011504
	public EventMissionType Type { get; private set; }

	// Token: 0x17000049 RID: 73
	// (get) Token: 0x06000343 RID: 835 RVA: 0x0001330D File Offset: 0x0001150D
	// (set) Token: 0x06000344 RID: 836 RVA: 0x00013315 File Offset: 0x00011515
	public string Venture { get; private set; }

	// Token: 0x06000345 RID: 837 RVA: 0x00013320 File Offset: 0x00011520
	private void UpdateState()
	{
		if (this.State.Value != EventMissionState.EXPIRED)
		{
			if (this.IsClaimed.Value)
			{
				this.State.Value = EventMissionState.CLAIMED;
				return;
			}
			if (this.CurrentCount.Value >= this.TargetAmount)
			{
				this.State.Value = EventMissionState.COMPLETE;
				return;
			}
			this.State.Value = EventMissionState.ACTIVE;
		}
	}

	// Token: 0x06000346 RID: 838 RVA: 0x00013381 File Offset: 0x00011581
	private void OnTimeRemainingChanged(double timeRemaining)
	{
		if (timeRemaining <= 0.0)
		{
			this.State.Value = EventMissionState.EXPIRED;
			MessageBroker.Default.Publish<PlaySoundFxRequest>(new PlaySoundFxRequest("mission_complete_sound"));
		}
	}

	// Token: 0x040002B3 RID: 691
	public readonly ClampedReactiveDouble CurrentCount = new ClampedReactiveDouble(0.0);

	// Token: 0x040002B4 RID: 692
	public readonly ReactiveProperty<bool> IsClaimed = new ReactiveProperty<bool>();

	// Token: 0x040002B9 RID: 697
	public readonly ReactiveProperty<EventMissionState> State = new ReactiveProperty<EventMissionState>(EventMissionState.NONE);

	// Token: 0x040002BA RID: 698
	public readonly CompositeDisposable disposables = new CompositeDisposable();
}
