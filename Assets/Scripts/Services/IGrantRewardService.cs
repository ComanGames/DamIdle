using System;
using System.Collections.Generic;

// Token: 0x020000DC RID: 220
public interface IGrantRewardService : IDisposable
{
	// Token: 0x060005F9 RID: 1529
	void Init(IGameController gameController, IInventoryService inventory, IAngelInvestorService angelService, SubscriptionService subscriptionService, GildingService gildingService);

	// Token: 0x060005FA RID: 1530
	List<RewardData> GrantRewards(List<RewardData> rewards, string source, string context, bool isFromPurchase);

	// Token: 0x060005FB RID: 1531
	RewardData GrantReward(RewardData rewardData, string source, string context, bool isFromPurchase);

	// Token: 0x060005FC RID: 1532
	List<RewardData> GrantRewardsWithoutCelebration(List<RewardData> rewards, bool isFromPurchase);

	// Token: 0x060005FD RID: 1533
	RewardData GrantRewardWithoutCelebration(RewardData rewardData, bool isFromPurchase);
}
