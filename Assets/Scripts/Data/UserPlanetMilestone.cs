using System;
using UniRx;

// Token: 0x020000C6 RID: 198
public class UserPlanetMilestone : IDisposable
{
	// Token: 0x17000079 RID: 121
	// (get) Token: 0x06000571 RID: 1393 RVA: 0x0001C9F9 File Offset: 0x0001ABF9
	public double DisplayCurrentTarget
	{
		get
		{
			return this.TargetAmount - this.progressStart;
		}
	}

	// Token: 0x1700007A RID: 122
	// (get) Token: 0x06000572 RID: 1394 RVA: 0x0001CA08 File Offset: 0x0001AC08
	// (set) Token: 0x06000573 RID: 1395 RVA: 0x0001CA10 File Offset: 0x0001AC10
	public int Index { get; private set; }

	// Token: 0x06000574 RID: 1396 RVA: 0x0001CA1C File Offset: 0x0001AC1C
	public UserPlanetMilestone(PlanetMilestone milestone, PlanetMilestoneSaveData saveData, double progressStart, int index)
	{
		this.planetMilestone = milestone;
		this.progressStart = progressStart;
		this.Index = index;
		this.CurrentCount.Value = saveData.CurrentCount;
		this.IsClaimed.Value = saveData.IsClaimed;
		this.CurrentCount.Subscribe(delegate(double _)
		{
			this.UpdateState();
		}).AddTo(this.disposables);
		this.IsClaimed.Subscribe(delegate(bool _)
		{
			this.UpdateState();
		}).AddTo(this.disposables);
		this.UpdateState();
	}

	// Token: 0x06000575 RID: 1397 RVA: 0x0001CAFD File Offset: 0x0001ACFD
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
		if (this.disposables != null)
		{
			this.disposables.Dispose();
		}
	}

	// Token: 0x1700007B RID: 123
	// (get) Token: 0x06000576 RID: 1398 RVA: 0x0001CB38 File Offset: 0x0001AD38
	public string MilestoneId
	{
		get
		{
			return this.planetMilestone.Id;
		}
	}

	// Token: 0x1700007C RID: 124
	// (get) Token: 0x06000577 RID: 1399 RVA: 0x0001CB45 File Offset: 0x0001AD45
	public string DisplayName
	{
		get
		{
			return this.planetMilestone.DisplayName;
		}
	}

	// Token: 0x1700007D RID: 125
	// (get) Token: 0x06000578 RID: 1400 RVA: 0x0001CB52 File Offset: 0x0001AD52
	public double TargetAmount
	{
		get
		{
			return this.planetMilestone.TargetAmount;
		}
	}

	// Token: 0x1700007E RID: 126
	// (get) Token: 0x06000579 RID: 1401 RVA: 0x0001CB5F File Offset: 0x0001AD5F
	public PlanetMilestoneRewardData Reward
	{
		get
		{
			return this.planetMilestone.Rewards[0];
		}
	}

	// Token: 0x0600057A RID: 1402 RVA: 0x0001CB6E File Offset: 0x0001AD6E
	public PlanetMilestoneSaveData Save()
	{
		return new PlanetMilestoneSaveData
		{
			MilestoneId = this.MilestoneId,
			IsClaimed = this.IsClaimed.Value,
			CurrentCount = this.CurrentCount.Value
		};
	}

	// Token: 0x0600057B RID: 1403 RVA: 0x0001CBA4 File Offset: 0x0001ADA4
	private void UpdateState()
	{
		if (this.IsClaimed.Value)
		{
			this.State.Value = UserPlanetMilestone.PlanetMilestoneState.CLAIMED;
		}
		else if (this.CurrentCount.Value >= this.TargetAmount)
		{
			this.State.Value = UserPlanetMilestone.PlanetMilestoneState.COMPLETE;
		}
		else
		{
			this.State.Value = UserPlanetMilestone.PlanetMilestoneState.ACTIVE;
		}
		this.DisplayCurrentCount.Value = Math.Max(0.0, Math.Min(this.CurrentCount.Value - this.progressStart, this.DisplayCurrentTarget));
	}

	// Token: 0x040004D8 RID: 1240
	public readonly ClampedReactiveDouble CurrentCount = new ClampedReactiveDouble(0.0);

	// Token: 0x040004D9 RID: 1241
	public readonly ReactiveProperty<double> DisplayCurrentCount = new ReactiveProperty<double>(0.0);

	// Token: 0x040004DA RID: 1242
	public readonly ReactiveProperty<bool> IsClaimed = new ReactiveProperty<bool>();

	// Token: 0x040004DC RID: 1244
	private PlanetMilestone planetMilestone;

	// Token: 0x040004DD RID: 1245
	public readonly ReactiveProperty<UserPlanetMilestone.PlanetMilestoneState> State = new ReactiveProperty<UserPlanetMilestone.PlanetMilestoneState>(UserPlanetMilestone.PlanetMilestoneState.NONE);

	// Token: 0x040004DE RID: 1246
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040004DF RID: 1247
	private double progressStart;

	// Token: 0x020007B2 RID: 1970
	public enum PlanetMilestoneState
	{
		// Token: 0x0400286F RID: 10351
		NONE,
		// Token: 0x04002870 RID: 10352
		ACTIVE,
		// Token: 0x04002871 RID: 10353
		COMPLETE,
		// Token: 0x04002872 RID: 10354
		CLAIMED
	}
}
