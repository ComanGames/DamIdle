using System;

// Token: 0x0200016E RID: 366
public class UnlockRewardBadge : IUnlockReward
{
	// Token: 0x1700010A RID: 266
	// (get) Token: 0x06000BAD RID: 2989 RVA: 0x000356E8 File Offset: 0x000338E8
	// (set) Token: 0x06000BAE RID: 2990 RVA: 0x000356F0 File Offset: 0x000338F0
	public string badgeId { get; private set; }

	// Token: 0x1700010B RID: 267
	// (get) Token: 0x06000BAF RID: 2991 RVA: 0x0002C8BE File Offset: 0x0002AABE
	public int qty
	{
		get
		{
			return 1;
		}
	}

	// Token: 0x06000BB0 RID: 2992 RVA: 0x000356FC File Offset: 0x000338FC
	public UnlockRewardBadge(string badgeId)
	{
		this.badgeId = badgeId;
		this.badge = GameController.Instance.GlobalPlayerData.inventory.GetItemById(this.badgeId);
		this.badgeName = ((this.badge != null) ? this.badge.ItemName : this.badgeId);
		this.rewardData = new RewardData(this.badgeId, ERewardType.Item, this.qty);
	}

	// Token: 0x06000BB1 RID: 2993 RVA: 0x0003576F File Offset: 0x0003396F
	public string Description(GameState state)
	{
		return string.Format("Earn the {0} badge", this.badgeName);
	}

	// Token: 0x06000BB2 RID: 2994 RVA: 0x00035781 File Offset: 0x00033981
	public string ShortDescription(GameState state)
	{
		return string.Format("{0}\nBadge", this.badgeName);
	}

	// Token: 0x06000BB3 RID: 2995 RVA: 0x00035794 File Offset: 0x00033994
	public void Apply(GameState state)
	{
		GameController.Instance.GrantRewardService.GrantReward(this.rewardData, string.Concat(new object[]
		{
			"Badge_Reward_",
			this.rewardData.Id,
			"_x",
			this.rewardData.Qty
		}), GameController.Instance.planetName, false);
	}

	// Token: 0x04000A1B RID: 2587
	private RewardData rewardData;

	// Token: 0x04000A1C RID: 2588
	private Item badge;

	// Token: 0x04000A1D RID: 2589
	private string badgeName;
}
