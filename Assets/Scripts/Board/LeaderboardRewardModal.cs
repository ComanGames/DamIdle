using System;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001CF RID: 463
public class LeaderboardRewardModal : AnimatedModal
{
	// Token: 0x06000DAB RID: 3499 RVA: 0x0003D233 File Offset: 0x0003B433
	protected override void Awake()
	{
		this.Btn_ClaimReward.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnClickClaim();
		}).AddTo(base.gameObject);
		base.Awake();
	}

	// Token: 0x06000DAC RID: 3500 RVA: 0x0003D263 File Offset: 0x0003B463
	private void Start()
	{
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.HandleOrientationChanged)).AddTo(this);
	}

	// Token: 0x06000DAD RID: 3501 RVA: 0x0003D287 File Offset: 0x0003B487
	private void HandleOrientationChanged(OrientationChangedEvent orientation)
	{
		this.panel.localScale = (orientation.IsPortrait ? Vector3.one : (Vector3.one * 0.75f));
	}

	// Token: 0x06000DAE RID: 3502 RVA: 0x0003D2B4 File Offset: 0x0003B4B4
	public void WireData(string eventId, EventRewardTier rewardTier)
	{
		this.eventId = eventId;
		this.Trn_Container.DestroyChildrenImmediate();
		this.Txt_TierName.text = rewardTier.tierName;
		this.Txt_Rank.text = string.Format("Rank {0:n0} - {1:n0}", rewardTier.topRank, rewardTier.bottomRank);
		this.Img_TrophyImage.sprite = GameController.Instance.IconService.GetSprite(rewardTier.trophyName);
		rewardTier.leaderboardRewardItems.ForEach(delegate(EventRewardItem item)
		{
			LeaderboardRewardItemView component = Object.Instantiate<GameObject>(this.rewardItemPrefab).GetComponent<LeaderboardRewardItemView>();
			component.Init(item);
			component.transform.SetParent(this.rewardItemContainer, false);
		});
	}

	// Token: 0x06000DAF RID: 3503 RVA: 0x0003D348 File Offset: 0x0003B548
	private void OnClickClaim()
	{
		this.Btn_ClaimReward.interactable = false;
		PendingEventRewards pendingEventRewards = GameController.Instance.EventService.PendingEventRewards.FirstOrDefault((PendingEventRewards x) => x.eventId == this.eventId);
		if (pendingEventRewards != null)
		{
			List<RewardData> rewardsFromTier = GameController.Instance.EventService.GetRewardsFromTier(pendingEventRewards.rewardTier);
			List<RewardData> list = GameController.Instance.GrantRewardService.GrantRewards(rewardsFromTier, "Leaderboard Rewards", this.eventId, false);
			if (list != null)
			{
				this.OnRewardsClaimedSuccess(list, pendingEventRewards);
				return;
			}
			GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Well, this is odd...", "Something went wrong trying to claim your event rewards", null, "", PopupModal.PopupOptions.OK, "Darn!", "", true);
		}
	}

	// Token: 0x06000DB0 RID: 3504 RVA: 0x0003D3FC File Offset: 0x0003B5FC
	private void OnRewardsClaimedSuccess(List<RewardData> rewards, PendingEventRewards pendingRewards)
	{
		string taskType = this.eventId;
		this.CloseModal(Unit.Default);
		GameController.Instance.EventService.OnRewardsClaimed(taskType);
		float num = (float)GameController.Instance.UnlockService.AchievedUnlocks.Count;
		GameController.Instance.AnalyticService.SendTaskCompleteEvent("play_prestige", taskType, string.Concat(new object[]
		{
			"Achievement:",
			num,
			",Rank:",
			pendingRewards.rewardTier.bottomRank,
			"-",
			pendingRewards.rewardTier.topRank
		}));
	}

	// Token: 0x04000BB8 RID: 3000
	public Text Txt_TierName;

	// Token: 0x04000BB9 RID: 3001
	public Text Txt_Rank;

	// Token: 0x04000BBA RID: 3002
	public Image Img_TrophyImage;

	// Token: 0x04000BBB RID: 3003
	public Transform Trn_Container;

	// Token: 0x04000BBC RID: 3004
	public Button Btn_ClaimReward;

	// Token: 0x04000BBD RID: 3005
	public GameObject rewardItemPrefab;

	// Token: 0x04000BBE RID: 3006
	public Transform rewardItemContainer;

	// Token: 0x04000BBF RID: 3007
	public Transform panel;

	// Token: 0x04000BC0 RID: 3008
	private string eventId;
}
