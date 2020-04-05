using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000C2 RID: 194
public class PlanetMilestoneRewardPanel : MonoBehaviour
{
	// Token: 0x0600052B RID: 1323 RVA: 0x0001AB74 File Offset: 0x00018D74
	private void Awake()
	{
		this.gameController = GameController.Instance;
		this.planetMilestoneService = this.gameController.PlanetMilestoneService;
		this.navigationService = this.gameController.NavigationService;
		IEnumerable<UserPlanetMilestone> userMilestonesForCurrentPlanet = this.planetMilestoneService.GetUserMilestonesForCurrentPlanet();
		bool areAllClaimed = false;
		(from x in (from x in userMilestonesForCurrentPlanet
		select x.State).ToArray<ReactiveProperty<UserPlanetMilestone.PlanetMilestoneState>>().CombineLatestValuesAreAll(UserPlanetMilestone.PlanetMilestoneState.CLAIMED)
		where x
		select x).Subscribe(delegate(bool x)
		{
			this.go_reward_grid_active_container.SetActive(!x);
			this.go_all_rewards_claimed_container.SetActive(x);
			areAllClaimed = true;
		}).AddTo(base.gameObject);
		this.go_reward_grid_active_container.SetActive(!areAllClaimed);
		this.go_all_rewards_claimed_container.SetActive(areAllClaimed);
		if (!areAllClaimed)
		{
			this.CreateRewardItems(OrientationController.Instance.CurrentOrientation.IsPortrait ? 2 : 3);
		}
		this.currentDisplayedPoints = this.planetMilestoneService.CurrentScore.Value;
		if (this.currentDisplayedPoints < 999999.0)
		{
			this.txt_current_score.text = NumberFormat.Convert(this.currentDisplayedPoints, 999999.0, false, 3);
		}
		else
		{
			this.txt_current_score.text = NumberFormat.Convert(this.currentDisplayedPoints, 999999.0, true, 3);
		}
		this.btn_goto_leaderboards.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.navigationService.CloseModal();
			this.navigationService.CreateModal<LeaderboardModalController>(NavModals.LEADERBOARDS_SHORT, false);
		}).AddTo(base.gameObject);
		this.UpdateCurrentSelectedReward(this.planetMilestoneService.CurrentMilestone.Value);
	}

	// Token: 0x0600052C RID: 1324 RVA: 0x0001AD30 File Offset: 0x00018F30
	public void ResetProgressElements()
	{
		for (int i = 0; i < this.listItems.Count; i++)
		{
			this.listItems[i].HasMadeProgress = false;
		}
	}

	// Token: 0x0600052D RID: 1325 RVA: 0x0001AD68 File Offset: 0x00018F68
	public List<Transform> GetProgressedElements()
	{
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < this.listItems.Count; i++)
		{
			if (this.listItems[i].HasMadeProgress)
			{
				list.Add(this.listItems[i].Sparkles_target);
			}
		}
		return list;
	}

	// Token: 0x0600052E RID: 1326 RVA: 0x0001ADBC File Offset: 0x00018FBC
	public Tween AnimatePointsIncreased(float time)
	{
		this.pointsShineAnimator.Animate();
		if (this.pointsTween != null)
		{
			this.pointsTween.Kill(false);
		}
		this.pointsTween = DOTween.To(() => this.currentDisplayedPoints, delegate(double x)
		{
			double num = Math.Floor(x);
			if (num < 999999.0)
			{
				this.txt_current_score.text = NumberFormat.Convert(num, 999999.0, false, 3);
			}
			else
			{
				this.txt_current_score.text = NumberFormat.Convert(num, 999999.0, true, 3);
			}
			this.currentDisplayedPoints = x;
		}, this.planetMilestoneService.CurrentScore.Value, time);
		return this.pointsTween;
	}

	// Token: 0x0600052F RID: 1327 RVA: 0x0001AE24 File Offset: 0x00019024
	public Sequence AnimatePointsEarnedSequence(float animLength)
	{
		Sequence sequence = DOTween.Sequence();
		int i;
		int j;
		for (i = 0; i < this.listItems.Count; i = j + 1)
		{
			if (this.listItems[i].HasMadeProgress)
			{
				sequence.Append(this.listItems[i].TweenToCurrentState(animLength));
				UserPlanetMilestone key = this.listItemViewsByMilestones.FirstOrDefault((KeyValuePair<UserPlanetMilestone, PlanetMilestoneRewardsListItem> x) => x.Value == this.listItems[i]).Key;
				if (key == this.currentSelectedMilestone && key.DisplayCurrentCount.Value > this.currentProgress)
				{
					sequence.Join(this.milestoneRewardPreview.TweenToCurrentState(animLength));
				}
				if (key.State.Value == UserPlanetMilestone.PlanetMilestoneState.COMPLETE && i + 1 < this.listItems.Count)
				{
					PlanetMilestoneRewardsListItem nextListItem = this.listItems[i + 1];
					sequence.AppendCallback(delegate
					{
						nextListItem.UpdateIconState();
					});
				}
			}
			j = i;
		}
		return sequence;
	}

	// Token: 0x06000530 RID: 1328 RVA: 0x0001AF59 File Offset: 0x00019159
	private void OnDestroy()
	{
		this.selectedMilestoneDisposables.Dispose();
	}

	// Token: 0x06000531 RID: 1329 RVA: 0x0001AF68 File Offset: 0x00019168
	private void CreateRewardItems(int rows)
	{
		List<UserPlanetMilestone> userMilestonesForCurrentPlanet = this.planetMilestoneService.GetUserMilestonesForCurrentPlanet();
		UserPlanetMilestone pPreviousMilestone = null;
		for (int i = 0; i < userMilestonesForCurrentPlanet.Count; i++)
		{
			Transform parentByRow = this.GetParentByRow(rows, userMilestonesForCurrentPlanet.Count, i);
			PlanetMilestoneRewardsListItem planetMilestoneRewardsListItem = Object.Instantiate<PlanetMilestoneRewardsListItem>(this.listItemPrefab, parentByRow, false);
			planetMilestoneRewardsListItem.transform.SetSiblingIndex(i);
			planetMilestoneRewardsListItem.WireData(userMilestonesForCurrentPlanet[i], pPreviousMilestone, new Action<UserPlanetMilestone>(this.OnListItemClicked), i + 1);
			this.listItems.Add(planetMilestoneRewardsListItem);
			this.listItemViewsByMilestones.Add(userMilestonesForCurrentPlanet[i], planetMilestoneRewardsListItem);
			pPreviousMilestone = userMilestonesForCurrentPlanet[i];
		}
	}

	// Token: 0x06000532 RID: 1330 RVA: 0x0001B008 File Offset: 0x00019208
	public void RedistributeListItems(int rows)
	{
		int count = this.listItems.Count;
		float num = (float)count / (float)rows;
		for (int i = 0; i < this.tform_reward_parents.Length; i++)
		{
			this.tform_reward_parents[i].gameObject.SetActive(i < rows);
		}
		for (int j = 0; j < count; j++)
		{
			int num2 = (int)Math.Floor((double)((float)j / num));
			Transform parent = this.tform_reward_parents[num2];
			this.listItems[j].transform.SetParent(parent);
			this.listItems[j].transform.SetSiblingIndex(j);
		}
	}

	// Token: 0x06000533 RID: 1331 RVA: 0x0001B0A4 File Offset: 0x000192A4
	private void OnClaimClicked()
	{
		if (this.currentSelectedMilestone != null)
		{
			this.planetMilestoneService.ClaimMilestoneRewardsForCurrentPlanet(this.currentSelectedMilestone.MilestoneId).Subscribe(delegate(List<PlanetMilestoneRewardData> rewards)
			{
				List<RewardData> converted = new List<RewardData>();
				rewards.ForEach(delegate(PlanetMilestoneRewardData x)
				{
					converted.Add(x);
				});
			}, delegate(Exception error)
			{
				Debug.LogError("[PlanetMilestonesRewardPanel] Error Claiming rewards - " + error.Message);
			});
		}
	}

	// Token: 0x06000534 RID: 1332 RVA: 0x0001B113 File Offset: 0x00019313
	private void OnListItemClicked(UserPlanetMilestone milestone)
	{
		PlanetMilestoneRewardsListItem planetMilestoneRewardsListItem = this.listItemViewsByMilestones[milestone];
		this.UpdateCurrentSelectedReward(milestone);
	}

	// Token: 0x06000535 RID: 1333 RVA: 0x0001B12C File Offset: 0x0001932C
	private void UpdateCurrentSelectedReward(UserPlanetMilestone milestone)
	{
		this.selectedMilestoneDisposables.Clear();
		if (this.currentSelectedMilestone != null)
		{
			this.listItemViewsByMilestones[this.currentSelectedMilestone].SetSelected(false);
		}
		this.currentSelectedMilestone = milestone;
		if (this.currentSelectedMilestone != null)
		{
			this.currentProgress = milestone.DisplayCurrentCount.Value;
			if (this.currentSelectedMilestone != null)
			{
				this.listItemViewsByMilestones[milestone].SetSelected(true);
			}
			this.milestoneRewardPreview.WireData(milestone, this.GetPreviousMilestone(milestone), delegate(UserPlanetMilestone _)
			{
				this.OnClaimClicked();
			});
		}
	}

	// Token: 0x06000536 RID: 1334 RVA: 0x0001B1BC File Offset: 0x000193BC
	private UserPlanetMilestone GetPreviousMilestone(UserPlanetMilestone milestone)
	{
		List<UserPlanetMilestone> userMilestonesForCurrentPlanet = this.planetMilestoneService.GetUserMilestonesForCurrentPlanet();
		int num = userMilestonesForCurrentPlanet.IndexOf(milestone);
		if (num > 0)
		{
			return userMilestonesForCurrentPlanet[num - 1];
		}
		return null;
	}

	// Token: 0x06000537 RID: 1335 RVA: 0x0001B1EC File Offset: 0x000193EC
	private Transform GetParentByRow(int rows, int totalItems, int index)
	{
		float num = (float)totalItems / (float)rows;
		Transform result = this.tform_reward_parents[0];
		for (int i = 0; i < totalItems; i++)
		{
			int num2 = (int)Math.Floor((double)((float)index / num));
			result = this.tform_reward_parents[num2];
		}
		return result;
	}

	// Token: 0x04000486 RID: 1158
	[SerializeField]
	private PlanetMilestoneRewardsListItem listItemPrefab;

	// Token: 0x04000487 RID: 1159
	[SerializeField]
	private Transform[] tform_reward_parents;

	// Token: 0x04000488 RID: 1160
	[SerializeField]
	private Text txt_current_score;

	// Token: 0x04000489 RID: 1161
	[SerializeField]
	private Button btn_goto_leaderboards;

	// Token: 0x0400048A RID: 1162
	[SerializeField]
	private GameObject go_reward_grid_active_container;

	// Token: 0x0400048B RID: 1163
	[SerializeField]
	private GameObject go_all_rewards_claimed_container;

	// Token: 0x0400048C RID: 1164
	[SerializeField]
	private PlanetMilestoneRewardPreview milestoneRewardPreview;

	// Token: 0x0400048D RID: 1165
	[SerializeField]
	private MaskedSpriteShineImageAnimator pointsShineAnimator;

	// Token: 0x0400048E RID: 1166
	private IGameController gameController;

	// Token: 0x0400048F RID: 1167
	private PlanetMilestoneService planetMilestoneService;

	// Token: 0x04000490 RID: 1168
	private NavigationService navigationService;

	// Token: 0x04000491 RID: 1169
	private UserPlanetMilestone currentSelectedMilestone;

	// Token: 0x04000492 RID: 1170
	private List<PlanetMilestoneRewardsListItem> listItems = new List<PlanetMilestoneRewardsListItem>();

	// Token: 0x04000493 RID: 1171
	private Dictionary<UserPlanetMilestone, PlanetMilestoneRewardsListItem> listItemViewsByMilestones = new Dictionary<UserPlanetMilestone, PlanetMilestoneRewardsListItem>();

	// Token: 0x04000494 RID: 1172
	private double currentProgress;

	// Token: 0x04000495 RID: 1173
	private Tween pointsTween;

	// Token: 0x04000496 RID: 1174
	private double currentDisplayedPoints;

	// Token: 0x04000497 RID: 1175
	private CompositeDisposable selectedMilestoneDisposables = new CompositeDisposable();
}
