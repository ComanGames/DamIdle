using System;

// Token: 0x02000175 RID: 373
public class UnlockRewardMultiplier : IUnlockReward
{
	// Token: 0x17000115 RID: 277
	// (get) Token: 0x06000BDE RID: 3038 RVA: 0x00035C9B File Offset: 0x00033E9B
	// (set) Token: 0x06000BDF RID: 3039 RVA: 0x00035CA3 File Offset: 0x00033EA3
	public int multipliers { get; private set; }

	// Token: 0x06000BE0 RID: 3040 RVA: 0x00035CAC File Offset: 0x00033EAC
	public UnlockRewardMultiplier(int multipliers)
	{
		this.multipliers = multipliers * 3;
		this.rewardData = new RewardData("multiplier_x3", ERewardType.InstantItem, this.multipliers);
	}

	// Token: 0x06000BE1 RID: 3041 RVA: 0x00035CD4 File Offset: 0x00033ED4
	public string Description(GameState state)
	{
		return string.Format("Earn a x{0} Multiplier", this.multipliers);
	}

	// Token: 0x06000BE2 RID: 3042 RVA: 0x00035CEB File Offset: 0x00033EEB
	public string ShortDescription(GameState state)
	{
		return string.Format("x{0} Multiplier", this.multipliers);
	}

	// Token: 0x06000BE3 RID: 3043 RVA: 0x00035D02 File Offset: 0x00033F02
	public void Apply(GameState state)
	{
		GameController.Instance.GrantRewardService.GrantReward(this.rewardData, "Unlock Multiplier Reward", GameController.Instance.game.planetName, false);
	}

	// Token: 0x04000A2C RID: 2604
	private RewardData rewardData;
}
