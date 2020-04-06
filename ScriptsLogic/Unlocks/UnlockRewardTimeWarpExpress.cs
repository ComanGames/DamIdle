using System;

// Token: 0x02000179 RID: 377
public class UnlockRewardTimeWarpExpress : IUnlockReward
{
	// Token: 0x06000BF7 RID: 3063 RVA: 0x00035F6C File Offset: 0x0003416C
	public UnlockRewardTimeWarpExpress(int qty)
	{
		this.Qty = qty;
		this.description = string.Format("{0}x Timewarp Express", this.Qty);
		this.shortDescription = string.Format("{0}x Timewarp Express", this.Qty);
		this.rewardData = new RewardData(TimeWarpService.TIME_WARP_EXPRESS_ITEM_ID, ERewardType.Item, this.Qty);
	}

	// Token: 0x06000BF8 RID: 3064 RVA: 0x00035FD3 File Offset: 0x000341D3
	public void Apply(GameState state)
	{
		GameController.Instance.GrantRewardService.GrantReward(this.rewardData, "Time_Warp_Reward_Express", GameController.Instance.game.planetName, false);
	}

	// Token: 0x06000BF9 RID: 3065 RVA: 0x00036000 File Offset: 0x00034200
	public string Description(GameState state)
	{
		return this.description;
	}

	// Token: 0x06000BFA RID: 3066 RVA: 0x00036008 File Offset: 0x00034208
	public string ShortDescription(GameState state)
	{
		return this.shortDescription;
	}

	// Token: 0x04000A34 RID: 2612
	private string description;

	// Token: 0x04000A35 RID: 2613
	private string shortDescription;

	// Token: 0x04000A36 RID: 2614
	public int Qty;

	// Token: 0x04000A37 RID: 2615
	private RewardData rewardData;
}
