using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AdCap.Store;
using Platforms.Logger;
using UniRx;
using UnityEngine;

// Token: 0x020000FB RID: 251
public class SubscriptionService : IDisposable
{
	// Token: 0x17000080 RID: 128
	// (get) Token: 0x0600068E RID: 1678 RVA: 0x000230F0 File Offset: 0x000212F0
	private bool IsSubscriptionActive
	{
		get
		{
			return PlayerPrefs.GetInt("ARD", 0) > 0;
		}
	}

	// Token: 0x0600068F RID: 1679 RVA: 0x00023100 File Offset: 0x00021300
	public SubscriptionService()
	{
		this.dailyRewardData = new RewardData("gold", ERewardType.Gold, 10);
		this.activeRewardData = new RewardData("multiplier_x9", ERewardType.InstantItem, 9);
	}

	// Token: 0x06000690 RID: 1680 RVA: 0x0002314F File Offset: 0x0002134F
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000691 RID: 1681 RVA: 0x0002315C File Offset: 0x0002135C
	public void Init(IDateTimeService dateTimeService, IGrantRewardService grantRewardService)
	{
		this.dateTimeService = dateTimeService;
		this.grantRewardService = grantRewardService;
		this.IsActive.Value = this.IsSubscriptionActive;
		MessageBroker.Default.Receive<WelcomeBackSequenceCompleted>().Subscribe(delegate(WelcomeBackSequenceCompleted _)
		{
			this.OnWelcomeBackSequenceCompleted();
		}).AddTo(this.disposables);
	}

	// Token: 0x06000692 RID: 1682 RVA: 0x000231AF File Offset: 0x000213AF
	private void OnWelcomeBackSequenceCompleted()
	{
		if (this.IsSubscriptionActive && this.NumDaysSinceLastReward() > 0)
		{
			this.GrantDailyReward();
		}
	}

	// Token: 0x06000693 RID: 1683 RVA: 0x000231C8 File Offset: 0x000213C8
	public void OnSubscriptionPurchased()
	{
		this.OnSubscriptionPurchaseRewardsGranted();
		if (this.grantRewardService.GrantRewardsWithoutCelebration(new List<RewardData>
		{
			this.dailyRewardData
		}, true) != null)
		{
			RewardData item = new RewardData
			{
				Id = this.activeRewardData.Id,
				Qty = 1,
				RewardType = ERewardType.Item
			};
			MessageBroker.Default.Publish<RewardsGrantedEvent>(new RewardsGrantedEvent(new List<RewardData>
			{
				this.dailyRewardData,
				item
			}, "Daily Subscription Reward", string.Format("Daily Reward Payout {0}x{1}", this.dailyRewardData.Id, this.dailyRewardData.Qty)));
			this.OnSubscriptionPurchaseRewardsGranted();
			return;
		}
		this.OnGrantRewardFailure(new Exception("Error granting subscription + rewards"));
	}

	// Token: 0x06000694 RID: 1684 RVA: 0x00023287 File Offset: 0x00021487
	private void OnSubscriptionPurchaseRewardsGranted()
	{
		PlayerPrefs.SetInt("ARD", 31);
		PlayerPrefs.Save();
		this.SubstractDaysFromSubscription(1);
	}

	// Token: 0x06000695 RID: 1685 RVA: 0x000232A4 File Offset: 0x000214A4
	private void GrantDailyReward()
	{
		RewardData rewardData = this.grantRewardService.GrantReward(this.dailyRewardData, "Daily Subscription Reward", string.Format("Daily Reward Payout {0}x{1}", this.dailyRewardData.Id, this.dailyRewardData.Qty), true);
		if (rewardData != null)
		{
			this.OnGrantDailyRewardSuccess(rewardData);
			return;
		}
		this.OnGrantRewardFailure(new Exception("Error granting daily subscription rewards"));
	}

	// Token: 0x06000696 RID: 1686 RVA: 0x00023309 File Offset: 0x00021509
	private void OnGrantDailyRewardSuccess(RewardData rewardData)
	{
		this.SubstractDaysFromSubscription(this.NumDaysSinceLastReward());
	}

	// Token: 0x06000697 RID: 1687 RVA: 0x0001511F File Offset: 0x0001331F
	private void OnGrantRewardFailure(Exception error)
	{
		Platforms.Logger.Logger.GetLogger(this).Error(error.Message);
	}

	// Token: 0x06000698 RID: 1688 RVA: 0x00023318 File Offset: 0x00021518
	private void SubstractDaysFromSubscription(int numDays)
	{
		int value = Math.Max(PlayerPrefs.GetInt("ARD", 0) - numDays, 0);
		PlayerPrefs.SetInt("ARD", value);
		PlayerPrefs.Save();
		PlayerPrefs.SetString("LRT", this.dateTimeService.UtcNow.ToString());
		this.IsActive.SetValueAndForceNotify(this.IsSubscriptionActive);
		if (this.IsActive.Value)
		{
			this.ScheduleNotification().ToObservable(false).StartAsCoroutine(default(CancellationToken));
		}
	}

	// Token: 0x06000699 RID: 1689 RVA: 0x000233A0 File Offset: 0x000215A0
	private DateTime LastRewardTime()
	{
		return DateTime.Parse(PlayerPrefs.GetString("LRT", this.dateTimeService.UtcNow.Subtract(TimeSpan.FromDays(730.0)).ToString()));
	}

	// Token: 0x0600069A RID: 1690 RVA: 0x000233E8 File Offset: 0x000215E8
	private int NumDaysSinceLastReward()
	{
		return Mathf.Max(0, this.dateTimeService.UtcNow.Subtract(this.LastRewardTime()).Days);
	}

	// Token: 0x0600069B RID: 1691 RVA: 0x0002341C File Offset: 0x0002161C
	private IEnumerator ScheduleNotification()
	{
		DateTime notificationTime = this.dateTimeService.UtcNow.AddDays((double)PlayerPrefs.GetInt("ARD", 0));
		Notifications.ClearLocalNotification(79);
		yield return new WaitForSeconds(0.1f);
		Notifications.Schedule("Gold Subscription expires tomorrow!", "Your last Daily Gold Delivery arrives tomorrow! Renew your Gold Subscription now to continue receiving sweet, sweet gold!", notificationTime, 79);
		yield break;
	}

	// Token: 0x04000604 RID: 1540
	public const int SUBSCRIPTION_LENGTH_DAYS = 31;

	// Token: 0x04000605 RID: 1541
	public const int PROFIT_BOOST_VALUE = 9;

	// Token: 0x04000606 RID: 1542
	public const int DAILY_LOGIN_GOLD_REWARD_AMOUNT = 10;

	// Token: 0x04000607 RID: 1543
	public readonly ReactiveProperty<bool> IsActive = new ReactiveProperty<bool>();

	// Token: 0x04000608 RID: 1544
	public const string SUBSCRIPTION_REWARD_ID = "gold";

	// Token: 0x04000609 RID: 1545
	public const string SUBSCRIPTION_ACTIVE_REWARD_ID = "multiplier_x9";

	// Token: 0x0400060A RID: 1546
	public const string AVAILABLE_REWARD_DAYS = "ARD";

	// Token: 0x0400060B RID: 1547
	public const string LAST_REWARD_TIME = "LRT";

	// Token: 0x0400060C RID: 1548
	private const int NOTIFICATION_ID = 79;

	// Token: 0x0400060D RID: 1549
	private IGrantRewardService grantRewardService;

	// Token: 0x0400060E RID: 1550
	private IDateTimeService dateTimeService;

	// Token: 0x0400060F RID: 1551
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000610 RID: 1552
	private RewardData dailyRewardData;

	// Token: 0x04000611 RID: 1553
	private RewardData activeRewardData;
}
