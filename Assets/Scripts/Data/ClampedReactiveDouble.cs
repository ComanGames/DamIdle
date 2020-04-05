using System;
using Platforms.Logger;
using UniRx;

// Token: 0x02000097 RID: 151
public class ClampedReactiveDouble : ReactiveProperty<double>
{
	// Token: 0x0600040C RID: 1036 RVA: 0x000155F8 File Offset: 0x000137F8
	public ClampedReactiveDouble(double d) : base(d)
	{
	}

	// Token: 0x0600040D RID: 1037 RVA: 0x0001560C File Offset: 0x0001380C
	protected override void SetValue(double v)
	{
		if (v > this.upperMax)
		{
			v = this.upperMax;
		}
		if (v.ToString().ToLower() == "nan")
		{
			Logger.GetLogger(this).Error("Setting a value to Nan in Clamped Reactive Double");
		}
		base.SetValue(v);
	}

	// Token: 0x0600040E RID: 1038 RVA: 0x00015659 File Offset: 0x00013859
	public void SetMax(double newMax)
	{
		this.upperMax = newMax;
		this.SetValue(base.Value);
	}

	// Token: 0x0400039B RID: 923
	private double upperMax = GameState.MAX_CASH_DOUBLE;
}
