using System;
using UniRx;

// Token: 0x0200004B RID: 75
public interface IAngelInvestorService : IDisposable
{
	// Token: 0x1700002A RID: 42
	// (get) Token: 0x06000236 RID: 566
	ReactiveProperty<double> AngelsOnHand { get; }

	// Token: 0x1700002B RID: 43
	// (get) Token: 0x06000237 RID: 567
	ReactiveProperty<double> AngelsSpent { get; }

	// Token: 0x1700002C RID: 44
	// (get) Token: 0x06000238 RID: 568
	ReactiveProperty<int> AngelResetCount { get; }

	// Token: 0x1700002D RID: 45
	// (get) Token: 0x06000239 RID: 569
	ReactiveProperty<double> RewardAngelsAtInterval { get; }

	// Token: 0x1700002E RID: 46
	// (get) Token: 0x0600023A RID: 570
	ReactiveProperty<double> AngelInvestorEffectiveness { get; }

	// Token: 0x1700002F RID: 47
	// (get) Token: 0x0600023B RID: 571
	ReactiveProperty<double> AngelInvestorEffectivenessBonus { get; }

	// Token: 0x17000030 RID: 48
	// (get) Token: 0x0600023C RID: 572
	ReadOnlyReactiveProperty<double> TotalAngelBonus { get; }

	// Token: 0x17000031 RID: 49
	// (get) Token: 0x0600023D RID: 573
	ReactiveProperty<bool> AngelsDoubledNotification { get; }

	// Token: 0x17000032 RID: 50
	// (get) Token: 0x0600023E RID: 574
	ReactiveProperty<bool> IsAngelThresholdReached { get; }

	// Token: 0x17000033 RID: 51
	// (get) Token: 0x0600023F RID: 575
	ReactiveProperty<bool> FirstTimeAngelReset { get; }

	// Token: 0x17000034 RID: 52
	// (get) Token: 0x06000240 RID: 576
	ReactiveProperty<bool> IsFirstEventAngelResetComplete { get; }

	// Token: 0x17000035 RID: 53
	// (get) Token: 0x06000241 RID: 577
	double angelResetThreshold { get; }

	// Token: 0x17000036 RID: 54
	// (get) Token: 0x06000242 RID: 578
	double eventAngelResetThreshold { get; }

	// Token: 0x06000243 RID: 579
	void Init(IGameController gameController);

	// Token: 0x06000244 RID: 580
	double GetRewardAngelCount();

	// Token: 0x06000245 RID: 581
	void SpendAngelInvestors(double amount);

	// Token: 0x06000246 RID: 582
	double CalculateAngelInvestors(double cash);

	// Token: 0x06000247 RID: 583
	double CalculateCashFromAngels(double angels, double passedAngelAccumulationRate = 1.0);

	// Token: 0x06000248 RID: 584
	double GetAdWatchAngelBonus();

	// Token: 0x06000249 RID: 585
	void Reset(bool isAdWatch = false);

	// Token: 0x0600024A RID: 586
	void HardReset();

	// Token: 0x0600024B RID: 587
	void AddAngelInvestorBonus(string key, double value);

	// Token: 0x0600024C RID: 588
	void RemoveAngelInvestorBonus(string key);

	// Token: 0x0600024D RID: 589
	void OnAngelClaimPurchaseCompleted();

	// Token: 0x0600024E RID: 590
	bool ShouldPromptOnAngelPurchase(double cost);

	// Token: 0x0600024F RID: 591
	void AddAngelsOnHand(double amount);
}
