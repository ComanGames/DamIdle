using System;

// Token: 0x02000172 RID: 370
public class UnlockRewardGold : IUnlockReward
{
	// Token: 0x1700010E RID: 270
	// (get) Token: 0x06000BC4 RID: 3012 RVA: 0x000359DA File Offset: 0x00033BDA
	// (set) Token: 0x06000BC5 RID: 3013 RVA: 0x000359E2 File Offset: 0x00033BE2
	public int goldAmount { get; private set; }

	// Token: 0x06000BC6 RID: 3014 RVA: 0x000359EB File Offset: 0x00033BEB
	public UnlockRewardGold(int goldAmount)
	{
		this.goldAmount = goldAmount;
		this.rewardData = new RewardData("gold", ERewardType.Gold, this.goldAmount);
	}

	// Token: 0x06000BC7 RID: 3015 RVA: 0x00035A14 File Offset: 0x00033C14
	public string Description(GameState state)
	{
		int modifiedGoldAmount = state.GetModifiedGoldAmount(this.goldAmount);
		return string.Format("Earn {0} Gold", modifiedGoldAmount);
	}

	// Token: 0x06000BC8 RID: 3016 RVA: 0x00035A40 File Offset: 0x00033C40
	public string ShortDescription(GameState state)
	{
		int modifiedGoldAmount = state.GetModifiedGoldAmount(this.goldAmount);
		return string.Format("{0}\nGold", modifiedGoldAmount);
	}

	// Token: 0x06000BC9 RID: 3017 RVA: 0x00035A6A File Offset: 0x00033C6A
	public void Apply(GameState state)
	{
		GameController.Instance.GrantRewardService.GrantReward(this.rewardData, "Gold_Unlock_Reward", GameController.Instance.game.planetName, false);
	}

	// Token: 0x04000A22 RID: 2594
	private RewardData rewardData;
}
