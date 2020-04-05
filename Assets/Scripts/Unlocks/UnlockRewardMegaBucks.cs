using System;

// Token: 0x02000174 RID: 372
public class UnlockRewardMegaBucks : IUnlockReward
{
	// Token: 0x17000113 RID: 275
	// (get) Token: 0x06000BD6 RID: 3030 RVA: 0x00035BC6 File Offset: 0x00033DC6
	// (set) Token: 0x06000BD7 RID: 3031 RVA: 0x00035BCE File Offset: 0x00033DCE
	public int MegaBucksAmount { get; private set; }

	// Token: 0x17000114 RID: 276
	// (get) Token: 0x06000BD8 RID: 3032 RVA: 0x00035BD7 File Offset: 0x00033DD7
	// (set) Token: 0x06000BD9 RID: 3033 RVA: 0x00035BDF File Offset: 0x00033DDF
	public string UnlockName { get; private set; }

	// Token: 0x06000BDA RID: 3034 RVA: 0x00035BE8 File Offset: 0x00033DE8
	public UnlockRewardMegaBucks(int megaBucksAmount, string name)
	{
		this.MegaBucksAmount = megaBucksAmount;
		this.UnlockName = name;
		this.rewardData = new RewardData("megabucks", ERewardType.Item, this.MegaBucksAmount);
	}

	// Token: 0x06000BDB RID: 3035 RVA: 0x00035C18 File Offset: 0x00033E18
	public string Description(GameState state)
	{
		int modifiedMegaBucksAmount = state.GetModifiedMegaBucksAmount(this.MegaBucksAmount);
		return string.Format("Earn {0} MegaBucks", modifiedMegaBucksAmount);
	}

	// Token: 0x06000BDC RID: 3036 RVA: 0x00035C44 File Offset: 0x00033E44
	public string ShortDescription(GameState state)
	{
		int modifiedMegaBucksAmount = state.GetModifiedMegaBucksAmount(this.MegaBucksAmount);
		return string.Format("{0}\nMegaBucks", modifiedMegaBucksAmount);
	}

	// Token: 0x06000BDD RID: 3037 RVA: 0x00035C6E File Offset: 0x00033E6E
	public void Apply(GameState state)
	{
		GameController.Instance.GrantRewardService.GrantReward(this.rewardData, "Megabucks_Unlock_Reward", GameController.Instance.game.planetName, false);
	}

	// Token: 0x04000A2A RID: 2602
	private RewardData rewardData;
}
