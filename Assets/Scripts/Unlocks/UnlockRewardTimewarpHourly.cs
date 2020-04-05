using System;
using System.Linq;

// Token: 0x02000177 RID: 375
public class UnlockRewardTimewarpHourly : IUnlockReward
{
	// Token: 0x17000116 RID: 278
	// (get) Token: 0x06000BE8 RID: 3048 RVA: 0x00035D36 File Offset: 0x00033F36
	public double SurgeProfit
	{
		get
		{
			return GameController.Instance.game.VentureModels.Sum((VentureModel v) => v.ProfitSurgeAmount(this.secondsToWarp, false));
		}
	}

	// Token: 0x06000BE9 RID: 3049 RVA: 0x00035D58 File Offset: 0x00033F58
	public UnlockRewardTimewarpHourly(double warpTimeSeconds, bool isExpress = false)
	{
		this.isExpress = isExpress;
		this.secondsToWarp = warpTimeSeconds;
		this.description = string.Format(warpTimeSeconds + " Second Time Warp", Array.Empty<object>());
		this.shortDescription = string.Format("{0} Second Time Warp", warpTimeSeconds);
	}

	// Token: 0x06000BEA RID: 3050 RVA: 0x00035DB0 File Offset: 0x00033FB0
	public UnlockRewardTimewarpHourly(int warpTimeHours, bool isExpress = false)
	{
		this.isExpress = isExpress;
		this.secondsToWarp = (double)(warpTimeHours * 3600);
		this.description = string.Format(warpTimeHours + " Hour Time Warp", Array.Empty<object>());
		this.shortDescription = string.Format("{0} Hour Time Warp", warpTimeHours);
	}

	// Token: 0x06000BEB RID: 3051 RVA: 0x00035E0E File Offset: 0x0003400E
	public void Apply(GameState state)
	{
		if (this.secondsToWarp > 0.0)
		{
			GameController.Instance.TimeWarpService.ApplyTimeWarp(this.secondsToWarp, this.isExpress);
		}
	}

	// Token: 0x06000BEC RID: 3052 RVA: 0x00035E3C File Offset: 0x0003403C
	public string Description(GameState state)
	{
		return this.description;
	}

	// Token: 0x06000BED RID: 3053 RVA: 0x00035E44 File Offset: 0x00034044
	public string ShortDescription(GameState state)
	{
		return this.shortDescription;
	}

	// Token: 0x06000BEE RID: 3054 RVA: 0x00035E4C File Offset: 0x0003404C
	public override string ToString()
	{
		return string.Format("Time Warp {0}, described as '{1}'", this.SurgeProfit, this.shortDescription);
	}

	// Token: 0x04000A2D RID: 2605
	private string description;

	// Token: 0x04000A2E RID: 2606
	private string shortDescription;

	// Token: 0x04000A2F RID: 2607
	public double secondsToWarp;

	// Token: 0x04000A30 RID: 2608
	private bool isExpress;
}
