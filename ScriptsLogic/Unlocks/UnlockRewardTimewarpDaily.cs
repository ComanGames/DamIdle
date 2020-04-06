using System;
using System.Linq;

// Token: 0x02000178 RID: 376
public class UnlockRewardTimewarpDaily : IUnlockReward
{
	// Token: 0x17000117 RID: 279
	// (get) Token: 0x06000BF0 RID: 3056 RVA: 0x00035E78 File Offset: 0x00034078
	public double SurgeProfit
	{
		get
		{
			return GameController.Instance.game.VentureModels.Sum(v => v.ProfitSurgeAmount((double)(this.warpTimeDays * 86400), false));
		}
	}

	// Token: 0x06000BF1 RID: 3057 RVA: 0x00035E9C File Offset: 0x0003409C
	public UnlockRewardTimewarpDaily(int warpTimeDays)
	{
		this.warpTimeDays = warpTimeDays;
		this.description = string.Format(warpTimeDays + " Day Time Warp", Array.Empty<object>());
		this.shortDescription = string.Format("{0} Day Time Warp", warpTimeDays);
	}

	// Token: 0x06000BF2 RID: 3058 RVA: 0x00035EEC File Offset: 0x000340EC
	public void Apply(GameState state)
	{
		string id = "time_warp_" + this.warpTimeDays;
		GameController.Instance.GlobalPlayerData.inventory.AddItem(id, 1, true, true);
	}

	// Token: 0x06000BF3 RID: 3059 RVA: 0x00035F27 File Offset: 0x00034127
	public string Description(GameState state)
	{
		return this.description;
	}

	// Token: 0x06000BF4 RID: 3060 RVA: 0x00035F2F File Offset: 0x0003412F
	public string ShortDescription(GameState state)
	{
		return this.shortDescription;
	}

	// Token: 0x06000BF5 RID: 3061 RVA: 0x00035F37 File Offset: 0x00034137
	public override string ToString()
	{
		return string.Format("Time Warp {0}, described as '{1}'", this.SurgeProfit, this.shortDescription);
	}

	// Token: 0x04000A31 RID: 2609
	private string description;

	// Token: 0x04000A32 RID: 2610
	private string shortDescription;

	// Token: 0x04000A33 RID: 2611
	public int warpTimeDays;
}
