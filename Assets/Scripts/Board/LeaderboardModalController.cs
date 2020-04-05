using System;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001CC RID: 460
public class LeaderboardModalController : ModalController
{
	// Token: 0x06000D9D RID: 3485 RVA: 0x0003CCAC File Offset: 0x0003AEAC
	protected override void Awake()
	{
		base.Awake();
		PlanetProgressionType progressionType = GameController.Instance.game.progressionType;
		this.btn_info_angels.gameObject.SetActive(progressionType == PlanetProgressionType.Angels);
		this.btn_info_missions.gameObject.SetActive(progressionType == PlanetProgressionType.Missions);
		this.btn_show_goals.gameObject.SetActive(progressionType == PlanetProgressionType.Missions);
		if (progressionType == PlanetProgressionType.Missions)
		{
			this.btn_show_goals.OnClickAsObservable().Subscribe(delegate(Unit _)
			{
				this.OpenEventMissions();
			}).AddTo(base.gameObject);
		}
	}

	// Token: 0x06000D9E RID: 3486 RVA: 0x0003CD38 File Offset: 0x0003AF38
	protected override void ToggleCanvasGroup(bool isOn, CanvasGroup cGroup)
	{
		if (isOn)
		{
			KeyValuePair<string, CanvasGroup> keyValuePair = this.panelMap.FirstOrDefault((KeyValuePair<string, CanvasGroup> panel) => panel.Value == cGroup);
			string planetName = GameController.Instance.planetName;
			LeaderboardType leaderboardType = GameController.Instance.EventDataService.EventDataList.FirstOrDefault((EventData x) => x.id == planetName).leaderboardType;
			string key = keyValuePair.Key;
			if (!(key == "MyRank"))
			{
				if (!(key == "TopRank"))
				{
					if (key == "Rewards")
					{
						if (!this.rewardPanel.IsInitialized)
						{
							this.rewardPanel.Init(GameController.Instance.EventDataService.GetLeaderBoardTiersForEvent(planetName));
							if (GameController.Instance.EventService.HasPlayerPerformedEventReset(planetName))
							{
								GameController.Instance.LeaderboardService.GetPlayerRank(planetName, leaderboardType, false).Subscribe(new Action<LeaderboardRankData>(this.rewardPanel.WireData), new Action<Exception>(this.OnMyRankError)).AddTo(base.gameObject);
							}
							else
							{
								this.rewardPanel.WireData(null);
							}
						}
					}
				}
				else if (!this.top100Panel.IsInitialized)
				{
					this.top100Panel.Init();
					GameController.Instance.LeaderboardService.GetLeaderboardTop100(planetName, leaderboardType).Subscribe(new Action<List<LeaderboardItem>>(this.top100Panel.WireData), new Action<Exception>(this.OnGetLeaderboardError)).AddTo(base.gameObject);
				}
			}
			else if (!this.myRankPanel.IsInitialized)
			{
				this.myRankPanel.Init();
				if (GameController.Instance.EventService.HasPlayerPerformedEventReset(planetName))
				{
					GameController.Instance.LeaderboardService.GetLeaderboardAroundPlayer(planetName, leaderboardType, 100).Subscribe(new Action<List<LeaderboardItem>>(this.myRankPanel.WireData), new Action<Exception>(this.OnGetLeaderboardError)).AddTo(base.gameObject);
				}
				else
				{
					this.myRankPanel.WireData(null);
				}
			}
		}
		base.ToggleCanvasGroup(isOn, cGroup);
	}

	// Token: 0x06000D9F RID: 3487 RVA: 0x0003CF84 File Offset: 0x0003B184
	private void OnGetLeaderboardError(Exception error)
	{
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Oh No!", string.Format("We ran into a problem retrieving the leaderboard!\n[{0}]", error.Message), null, null, PopupModal.PopupOptions.OK, "Dang!", null, false);
	}

	// Token: 0x06000DA0 RID: 3488 RVA: 0x0003CFCA File Offset: 0x0003B1CA
	private void OnMyRankError(Exception error)
	{
		Debug.LogError("[LeaderboardModalController](OnMyRankError) " + error.Message + ", " + error.StackTrace);
	}

	// Token: 0x06000DA1 RID: 3489 RVA: 0x0003CFEC File Offset: 0x0003B1EC
	private void OpenEventMissions()
	{
		this.CloseModal(Unit.Default);
		GameController.Instance.AnalyticService.SendNavActionAnalytics("OpenMissionModal", "LeaderboardModal", GameController.Instance.planetName);
		GameController.Instance.NavigationService.CreateModal<EventMissionsModal>(NavModals.EVENT_MISSIONS_MODAL, true);
	}

	// Token: 0x04000BA7 RID: 2983
	[SerializeField]
	private LeaderboardPanel myRankPanel;

	// Token: 0x04000BA8 RID: 2984
	[SerializeField]
	private LeaderboardPanel top100Panel;

	// Token: 0x04000BA9 RID: 2985
	[SerializeField]
	private LeaderboardRewardPanel rewardPanel;

	// Token: 0x04000BAA RID: 2986
	[SerializeField]
	private Button btn_info_angels;

	// Token: 0x04000BAB RID: 2987
	[SerializeField]
	private Button btn_info_missions;

	// Token: 0x04000BAC RID: 2988
	[SerializeField]
	private Button btn_show_goals;
}
