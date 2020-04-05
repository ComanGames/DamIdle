using System;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000AB RID: 171
public class LeaderboardRewardPanel : MonoBehaviour
{
	// Token: 0x060004A1 RID: 1185 RVA: 0x00018918 File Offset: 0x00016B18
	public void Init(List<EventRewardTier> tiers)
	{
		for (int i = 0; i < tiers.Count; i++)
		{
			LeaderboardRewardTierView component = Object.Instantiate<GameObject>(this.rewardTierPrefab).GetComponent<LeaderboardRewardTierView>();
			component.transform.SetParent(this.container, false);
			component.Init(tiers[i], new Action<EventRewardTier>(this.OnRewardClicked));
			this.leaderBoardRewardTierViews.Add(component);
		}
		GameController.Instance.UserDataService.UserName.Subscribe(delegate(string x)
		{
			this.playerName.text = x;
		}).AddTo(base.gameObject);
		this.IsInitialized = true;
	}

	// Token: 0x060004A2 RID: 1186 RVA: 0x000189B4 File Offset: 0x00016BB4
	public void WireData(LeaderboardRankData eventData)
	{
		if (eventData != null)
		{
			this.playerRank.text = eventData.leaderboardRank.ToString();
			if (this.leaderBoardRewardTierViews != null)
			{
				LeaderboardRewardTierView leaderboardRewardTierView = this.leaderBoardRewardTierViews.FirstOrDefault((LeaderboardRewardTierView x) => eventData.leaderboardRank >= (double)x.rewardTier.topRank && eventData.leaderboardRank <= (double)x.rewardTier.bottomRank);
				if (null != leaderboardRewardTierView)
				{
					leaderboardRewardTierView.IsSelected(true);
					this.playerRankTierName.text = leaderboardRewardTierView.rewardTier.tierName;
					this.playerRankTrophyImage.sprite = GameController.Instance.IconService.GetSprite(leaderboardRewardTierView.rewardTier.trophyName);
					return;
				}
			}
		}
		else
		{
			this.header.SetActive(false);
			LeaderboardTakeActionPanel component = Object.Instantiate<GameObject>(this.leaderboardRewardTakeActionStatePrefab, this.container, false).GetComponent<LeaderboardTakeActionPanel>();
			component.transform.SetAsFirstSibling();
			component.WireData();
		}
	}

	// Token: 0x060004A3 RID: 1187 RVA: 0x00018A96 File Offset: 0x00016C96
	private void OnRewardClicked(EventRewardTier rewardTier)
	{
		GameController.Instance.NavigationService.CreateModal<LeaderboardRewardInfoModal>(NavModals.LEADERBOARD_REWARD_INFO, false).WireData(rewardTier);
	}

	// Token: 0x04000424 RID: 1060
	[SerializeField]
	private GameObject leaderboardRewardTakeActionStatePrefab;

	// Token: 0x04000425 RID: 1061
	[SerializeField]
	private Transform container;

	// Token: 0x04000426 RID: 1062
	[SerializeField]
	private GameObject rewardTierPrefab;

	// Token: 0x04000427 RID: 1063
	[SerializeField]
	private GameObject header;

	// Token: 0x04000428 RID: 1064
	[SerializeField]
	private Text playerRank;

	// Token: 0x04000429 RID: 1065
	[SerializeField]
	private Text playerName;

	// Token: 0x0400042A RID: 1066
	[SerializeField]
	private Text playerRankTierName;

	// Token: 0x0400042B RID: 1067
	[SerializeField]
	private Image playerRankTrophyImage;

	// Token: 0x0400042C RID: 1068
	[HideInInspector]
	public bool IsInitialized;

	// Token: 0x0400042D RID: 1069
	private List<LeaderboardRewardTierView> leaderBoardRewardTierViews = new List<LeaderboardRewardTierView>();
}
