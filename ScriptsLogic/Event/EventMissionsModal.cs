using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdCap;
using AdCap.Store;
using DG.Tweening;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

// Token: 0x0200005A RID: 90
public class EventMissionsModal : AnimatedModal
{
	// Token: 0x0600030E RID: 782 RVA: 0x000120E4 File Offset: 0x000102E4
	public override void Init(NavigationService navService)
	{
		base.Init(navService);
		this.navigationService = navService;
		this.eventService = GameController.Instance.EventService;
		this.eventMissionService = GameController.Instance.EventMissionsService;
		EventModel eventModel = this.eventService.ActiveEvents.FirstOrDefault(x => x.Id == GameController.Instance.game.planetName);
		if (eventModel != null)
		{
			eventModel.TimeRemaining.Subscribe(delegate(double time)
			{
				this.txt_time_remaining.text = string.Format("Event Ends In: {0}", time.ToCountdownTrim());
			}).AddTo(base.gameObject);
		}
		this.listItemPrefab.gameObject.SetActive(false);
		this.timedListItemPrefab.gameObject.SetActive(false);
		this.btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.CloseModal)).AddTo(base.gameObject);
		ReactiveCollection<UserEventMission> currentMissions = this.eventMissionService.currentMissions;
		this.listItemContainerPrefab.transform.SetSiblingIndex(1);
		this.listItemContainers.Add(this.listItemContainerPrefab.transform);
		for (int i = 1; i < currentMissions.Count; i++)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.listItemContainerPrefab, this.isCurrentOrientaionPortrait ? this.tform_portrait_itemParent : this.tform_landscape_itemParent, false);
			gameObject.transform.SetSiblingIndex(i + 1);
			this.listItemContainers.Add(gameObject.transform);
		}
		for (int j = 0; j < currentMissions.Count; j++)
		{
			UserEventMission userEventMission = currentMissions[j];
			if (!userEventMission.IsTimed)
			{
				this.InitPrefabForMission(userEventMission, j);
			}
			else
			{
				this.InitPrefabForTimedMission(userEventMission, j);
			}
		}
		this.tform_portrait_container.gameObject.SetActive(this.isCurrentOrientaionPortrait);
		this.tform_landscape_container.gameObject.SetActive(!this.isCurrentOrientaionPortrait);
		this.btn_refresh_landscape.interactable = (this.btn_refresh.interactable = false);
		this.btn_refresh.OnClickAsObservable().Subscribe(new Action<Unit>(this.RefreshCurrentMissions)).AddTo(base.gameObject);
		this.btn_refresh_landscape.OnClickAsObservable().Subscribe(new Action<Unit>(this.RefreshCurrentMissions)).AddTo(base.gameObject);
		MessageBroker.Default.Receive<EventMissionChangedEvent>().Subscribe(delegate(EventMissionChangedEvent evt)
		{
			this.tasksRefreshing++;
			Sequence sequence = DOTween.Sequence();
			if (evt.oldMission.State.Value != EventMissionState.EXPIRED)
			{
				sequence.SetDelay(0.5f);
			}
			this.PreAnimationDisableUIElements();
			sequence.Append(this.ExchangeTaskWithAnimation(evt.oldMission, evt.newMission));
			sequence.AppendCallback(delegate
			{
				this.tasksRefreshing--;
				if (this.tasksRefreshing == 0)
				{
					this.PostAnimationsRestoreUIElements();
				}
			});
		}).AddTo(base.gameObject);
		MessageBroker.Default.Receive<EventMissionsRefreshedEvent>().Subscribe(delegate(EventMissionsRefreshedEvent evt)
		{
			this.btn_refresh_landscape.interactable = (this.btn_refresh.interactable = false);
			Sequence s = DOTween.Sequence();
			this.goalListLayout.enabled = false;
			this.outerGoalsLayout.enabled = false;
			this.goalsScrollRect.enabled = false;
			for (int k = 0; k < evt.oldMissions.Count; k++)
			{
				s.Join(this.ExchangeTaskWithAnimation(evt.oldMissions[k], evt.newMissions[k]).SetDelay((float)k * 0.1f));
			}
			s.AppendCallback(delegate
			{
				this.PostAnimationsRestoreUIElements();
			});
		}).AddTo(base.gameObject);
		if (FTUE_Manager.GetSeenFTUE("EventMission_Step1"))
		{
			this.RegisterStoreItemListener();
		}
		this.isDataWired = true;
		OrientationController.Instance.OrientationStream.DelayFrame(1, FrameCountType.Update).Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChangedLocal)).AddTo(base.gameObject);
	}

	// Token: 0x0600030F RID: 783 RVA: 0x000123B6 File Offset: 0x000105B6
	private void PreAnimationDisableUIElements()
	{
		this.goalListLayout.enabled = false;
		this.outerGoalsLayout.enabled = false;
		this.goalsScrollRect.enabled = false;
		this.btn_refresh_landscape.interactable = false;
	}

	// Token: 0x06000310 RID: 784 RVA: 0x000123E8 File Offset: 0x000105E8
	private void PostAnimationsRestoreUIElements()
	{
		if (this.refreshStoreItem == null)
		{
			this.RegisterStoreItemListener();
		}
		this.goalListLayout.enabled = true;
		this.outerGoalsLayout.enabled = true;
		this.goalsScrollRect.enabled = true;
		this.btn_refresh_landscape.interactable = (this.btn_refresh.interactable = (this.refreshStoreItem != null));
	}

	// Token: 0x06000311 RID: 785 RVA: 0x0001244C File Offset: 0x0001064C
	private void RegisterStoreItemListener()
	{
		(from x in this.eventMissionService.RefreshStoreItem
		where x != null
		select x).Subscribe(delegate(AdCapStoreItem item)
		{
			this.refreshStoreItem = item;
			this.btn_refresh_landscape.interactable = (this.btn_refresh.interactable = (this.refreshStoreItem != null));
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000312 RID: 786 RVA: 0x000124A8 File Offset: 0x000106A8
	private Sequence ExchangeTaskWithAnimation(UserEventMission oldMission, UserEventMission newMission)
	{
		BaseMissionListItem item = this.listElementsByMission[oldMission];
		Transform parent = item.transform.parent;
		int containerIndex = this.listItemContainers.IndexOf(parent);
		float duration = 0.5f;
		Sequence sequence = DOTween.Sequence();
		float x = parent.transform.localPosition.x;
		sequence.AppendCallback(delegate
		{
			MessageBroker.Default.Publish<PlaySoundFxRequest>(new PlaySoundFxRequest("swipe_sound"));
		});
		sequence.Append(parent.transform.DOLocalMoveX(-800f, duration, false));
		sequence.AppendCallback(delegate
		{
			this.ReturnListItemToPool(item);
			item = this.GetObjectFromPool(newMission, containerIndex);
		});
		sequence.AppendCallback(delegate
		{
			MessageBroker.Default.Publish<PlaySoundFxRequest>(new PlaySoundFxRequest("swipe_sound"));
		});
		sequence.Append(parent.transform.DOLocalMoveX(x, duration, false));
		if (newMission.IsTimed)
		{
			sequence.AppendCallback(delegate
			{
				MessageBroker.Default.Publish<PlaySoundFxRequest>(new PlaySoundFxRequest("tada_sound"));
			}).SetDelay(1f);
		}
		return sequence;
	}

	// Token: 0x06000313 RID: 787 RVA: 0x000125E8 File Offset: 0x000107E8
	public override void OnIntroFinished()
	{
		base.OnIntroFinished();
		if (FeatureConfig.IsFlagSet("ShowEventMissionsModalFtue"))
		{
			this.animationCR = base.StartCoroutine(this.DoFTUESequence());
			return;
		}
		(from x in GameController.Instance.TriggerService.MonitorTriggers(new List<TriggerData>
		{
			new TriggerData
			{
				TriggerType = ETriggerType.TutorialStepComplete,
				Id = "EventMissionRewardClaim1"
			}
		}, true)
		where x
		select x).Take(1).Subscribe(delegate(bool x)
		{
			this.RegisterStoreItemListener();
			this.PostAnimationsRestoreUIElements();
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000314 RID: 788 RVA: 0x00012694 File Offset: 0x00010894
	private void OnDestroy()
	{
		if (FTUE_Manager.Instance != null)
		{
			FTUE_Manager.Instance.OnMissionModalClosed();
		}
		if (this.animationCR != null)
		{
			base.StopCoroutine(this.animationCR);
		}
	}

	// Token: 0x06000315 RID: 789 RVA: 0x000126C1 File Offset: 0x000108C1
	private IEnumerator DoFTUESequence()
	{
		UserEventMission firstCompleteMission = this.eventMissionService.currentMissions.FirstOrDefault(x => x.Type == EventMissionType.FIRST_AUTO_COMPLETE);
		bool isFreeTaskClaimable = firstCompleteMission != null;
		if (!FTUE_Manager.GetSeenFTUE("EventMission_Step1") || isFreeTaskClaimable)
		{
			EventMissionsModal.<>c__DisplayClass39_1 CS$<>8__locals2 = new EventMissionsModal.<>c__DisplayClass39_1();
			foreach (KeyValuePair<UserEventMission, BaseMissionListItem> keyValuePair in this.listElementsByMission)
			{
				if (keyValuePair.Key.Type != EventMissionType.FIRST_AUTO_COMPLETE)
				{
					keyValuePair.Value.transform.gameObject.SetActive(false);
				}
			}
			CS$<>8__locals2.isComplete = !FTUE_Manager.ShowFTUE("EventMission_Step1", delegate()
			{
				CS$<>8__locals2.isComplete = true;
			});
			while (!CS$<>8__locals2.isComplete)
			{
				yield return null;
			}
			CS$<>8__locals2 = null;
		}
		bool isSecondStepActive = false;
		if (!FTUE_Manager.GetSeenFTUE("EventMission_Step2") || isFreeTaskClaimable)
		{
			isSecondStepActive = true;
			if (FTUE_Manager.ShowFTUE("EventMission_Step2", delegate()
			{
			}))
			{
				while (firstCompleteMission != null && firstCompleteMission.State.Value != EventMissionState.CLAIMED)
				{
					yield return null;
				}
				FTUE_Manager.Instance.OnFirstMissionPointsClaimed();
			}
			Sequence s = DOTween.Sequence();
			this.PreAnimationDisableUIElements();
			foreach (KeyValuePair<UserEventMission, BaseMissionListItem> keyValuePair2 in this.listElementsByMission)
			{
				if (keyValuePair2.Key.Type != EventMissionType.FIRST_AUTO_COMPLETE)
				{
					BaseMissionListItem value = keyValuePair2.Value;
					Transform parent = value.transform.parent;
					Vector3 localPosition = parent.transform.localPosition;
					parent.transform.localPosition = new Vector3(localPosition.x - 800f, localPosition.y, localPosition.z);
					value.transform.gameObject.SetActive(true);
					s.Insert(1f, parent.transform.DOLocalMoveX(localPosition.x, 0.5f, false));
				}
			}
			s.AppendCallback(delegate
			{
				this.RegisterStoreItemListener();
				this.PostAnimationsRestoreUIElements();
				isSecondStepActive = false;
			});
		}
		while (isSecondStepActive)
		{
			yield return null;
		}
		if (!FTUE_Manager.GetSeenFTUE("EventMission_Step3"))
		{
			List<UserPlanetMilestone> userMilestonesForCurrentPlanet = GameController.Instance.PlanetMilestoneService.GetUserMilestonesForCurrentPlanet();
			UserPlanetMilestone firstReward = (userMilestonesForCurrentPlanet.Count > 0) ? userMilestonesForCurrentPlanet[0] : null;
			bool flag = firstReward != null && firstReward.State.Value == UserPlanetMilestone.PlanetMilestoneState.COMPLETE;
			if (firstReward != null && flag)
			{
				if (FTUE_Manager.ShowFTUE("EventMission_Step3", delegate()
				{
				}))
				{
					while (firstReward != null && firstReward.State.Value != UserPlanetMilestone.PlanetMilestoneState.CLAIMED)
					{
						yield return null;
					}
					FTUE_Manager.Instance.OnFirstMissionRewardClaimed();
				}
			}
			firstReward = null;
		}
		yield break;
	}

	// Token: 0x06000316 RID: 790 RVA: 0x000126D0 File Offset: 0x000108D0
	private void RefreshCurrentMissions(Unit u)
	{
		if (this.eventMissionService.currentMissions.Any(x => x.State.Value == EventMissionState.COMPLETE))
		{
			this.navigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Wait..", "You have unclaimed points! Claim your points before refreshing your goals", null, PopupModal.PopupOptions.OK, "OK", "No", true, null, "");
			return;
		}
		GameController.Instance.AnalyticService.SendNavActionAnalytics(this.refreshStoreItem.Id, GameController.Instance.game.planetName + "_EventMissionModal", "prebuy");
		this.navigationService.CreateModal<ConfirmPurchasePopup, ConfirmPurchasePopup.ConfirmPurchaseParams>(NavModals.CONFIRM_PURCHASE_POPUP_CONFIRM_PURCHASE_PARAMS, new ConfirmPurchasePopup.ConfirmPurchaseParams("Refresh Goals using Gold?", "Need a fresh set? Your current Goals will be replaced with brand new ones! All unfinished progress on current Goals will be lost.", "PURCHASE NEW GOALS", this.refreshStoreItem.Cost, delegate(bool x)
		{
			if (x)
			{
				GameController.Instance.AnalyticService.SendNavActionAnalytics(this.refreshStoreItem.Id, GameController.Instance.game.planetName + "_EventMissionModal", "buy");
				this.eventMissionService.RefreshTasksForGold();
			}
		}), false);
	}

	// Token: 0x06000317 RID: 791 RVA: 0x000127B8 File Offset: 0x000109B8
	private void ReturnListItemToPool(BaseMissionListItem listItem)
	{
		BaseMissionListItem listItem2 = listItem;
		EventMissionsListItem component = listItem2.gameObject.GetComponent<EventMissionsListItem>();
		if (component)
		{
			this.PooledPrefabs.Add(component);
		}
		else
		{
			EventTimedMissionsListItem component2 = listItem2.gameObject.GetComponent<EventTimedMissionsListItem>();
			if (component2)
			{
				this.PooledTimdPrefabs.Add(component2);
			}
		}
		listItem2.transform.SetSiblingIndex(999);
		listItem2.gameObject.SetActive(false);
		UserEventMission key = this.listElementsByMission.First(x => x.Value == listItem).Key;
		this.listElementsByMission.Remove(key);
	}

	// Token: 0x06000318 RID: 792 RVA: 0x00012868 File Offset: 0x00010A68
	private BaseMissionListItem GetObjectFromPool(UserEventMission mission, int index)
	{
		if (!mission.IsTimed)
		{
			return this.InitPrefabForMission(mission, index);
		}
		return this.InitPrefabForTimedMission(mission, index);
	}

	// Token: 0x06000319 RID: 793 RVA: 0x00012884 File Offset: 0x00010A84
	private EventMissionsListItem InitPrefabForMission(UserEventMission mission, int index)
	{
		Transform transform = this.listItemContainers[index];
		EventMissionsListItem eventMissionsListItem;
		if (this.PooledPrefabs.Count > 0)
		{
			eventMissionsListItem = this.PooledPrefabs[0];
			this.PooledPrefabs.RemoveAt(0);
			eventMissionsListItem.gameObject.SetActive(true);
			eventMissionsListItem.transform.SetParent(transform, false);
		}
		else
		{
			eventMissionsListItem = Object.Instantiate<EventMissionsListItem>(this.listItemPrefab, transform.transform, false);
			eventMissionsListItem.gameObject.SetActive(true);
		}
		eventMissionsListItem.gameObject.name = "Mission" + mission.ID;
		eventMissionsListItem.Init(mission, new Action<Transform, UserEventMission>(this.OnClaimClicked));
		if (this.listElementsByMission.ContainsKey(mission))
		{
			this.listElementsByMission[mission] = eventMissionsListItem;
		}
		else
		{
			this.listElementsByMission.Add(mission, eventMissionsListItem);
		}
		return eventMissionsListItem;
	}

	// Token: 0x0600031A RID: 794 RVA: 0x00012960 File Offset: 0x00010B60
	private EventTimedMissionsListItem InitPrefabForTimedMission(UserEventMission mission, int index)
	{
		Transform transform = this.listItemContainers[index];
		EventTimedMissionsListItem eventTimedMissionsListItem;
		if (this.PooledTimdPrefabs.Count > 0)
		{
			eventTimedMissionsListItem = this.PooledTimdPrefabs[0];
			this.PooledTimdPrefabs.RemoveAt(0);
			eventTimedMissionsListItem.gameObject.SetActive(true);
			eventTimedMissionsListItem.transform.SetParent(transform, false);
		}
		else
		{
			eventTimedMissionsListItem = Object.Instantiate<EventTimedMissionsListItem>(this.timedListItemPrefab, transform.transform, false);
			eventTimedMissionsListItem.gameObject.SetActive(true);
		}
		eventTimedMissionsListItem.gameObject.name = "Mission" + mission.ID;
		eventTimedMissionsListItem.Init(mission, new Action<Transform, UserEventMission>(this.OnClaimClicked));
		if (this.listElementsByMission.ContainsKey(mission))
		{
			this.listElementsByMission[mission] = eventTimedMissionsListItem;
		}
		else
		{
			this.listElementsByMission.Add(mission, eventTimedMissionsListItem);
		}
		return eventTimedMissionsListItem;
	}

	// Token: 0x0600031B RID: 795 RVA: 0x00012A3C File Offset: 0x00010C3C
	private void OnClaimClicked(Transform source, UserEventMission mission)
	{
		if (mission.State.Value == EventMissionState.EXPIRED)
		{
			this.eventMissionService.ClearMission(mission);
			return;
		}
		if (this.eventMissionService.ClaimMissionReward(mission))
		{
			Sequence s = DOTween.Sequence();
			List<Transform> progressedElements = this.planetMilestoneRewardPanel.GetProgressedElements();
			if (progressedElements.Count > 0)
			{
				s.Join(this.planetMilestoneRewardPanel.AnimatePointsEarnedSequence((float)progressedElements.Count * 0.1f).SetDelay(0.5f));
				for (int i = 0; i < progressedElements.Count; i++)
				{
					UIParticleSystem particles = Object.Instantiate<UIParticleSystem>(this.claimPointsParticlesPrefab, base.transform, false);
					particles.transform.position = source.position;
					s.Insert(0.25f + 0.1f * (float)i, particles.transform.DOMove(progressedElements[i].position, 0.25f, false));
					s.InsertCallback(0.5f + 0.1f * (float)i + 0.4f + 0.2f, delegate
					{
						Object.Destroy(particles.gameObject);
					});
				}
				this.planetMilestoneRewardPanel.ResetProgressElements();
			}
			s.Join(this.planetMilestoneRewardPanel.AnimatePointsIncreased(0.8f));
		}
	}

	// Token: 0x0600031C RID: 796 RVA: 0x00012B90 File Offset: 0x00010D90
	private void OnOrientationChangedLocal(OrientationChangedEvent orientation)
	{
		this.isCurrentOrientaionPortrait = orientation.IsPortrait;
		if (!this.isDataWired)
		{
			return;
		}
		for (int i = 0; i < this.listItemContainers.Count; i++)
		{
			Transform transform = this.isCurrentOrientaionPortrait ? this.tform_portrait_itemParent : this.tform_landscape_itemParent;
			this.listItemContainers[i].gameObject.transform.SetParent(transform.transform);
			this.listItemContainers[i].SetSiblingIndex(i + 1);
		}
		this.tform_portrait_container.gameObject.SetActive(this.isCurrentOrientaionPortrait);
		this.tform_landscape_container.gameObject.SetActive(!this.isCurrentOrientaionPortrait);
		this.planetMilestoneRewardPanel.transform.SetParent(this.isCurrentOrientaionPortrait ? this.tform_portrait_rewards_parent : this.tform_landscape_rewards_parent);
		this.planetMilestoneRewardPanel.RedistributeListItems(this.isCurrentOrientaionPortrait ? 2 : 3);
		RectTransform component = this.planetMilestoneRewardPanel.GetComponent<RectTransform>();
		Vector2 anchorMin = new Vector2(0f, 0f);
		Vector2 anchorMax = new Vector2(1f, 1f);
		component.anchorMin = anchorMin;
		component.anchorMax = anchorMax;
		Vector2 vector = new Vector2(0f, 0f);
		component.offsetMax = vector;
		component.offsetMin = vector;
	}

	// Token: 0x0400028E RID: 654
	[SerializeField]
	private Button btn_Close;

	// Token: 0x0400028F RID: 655
	[SerializeField]
	private Text txt_time_remaining;

	// Token: 0x04000290 RID: 656
	[SerializeField]
	private EventMissionsListItem listItemPrefab;

	// Token: 0x04000291 RID: 657
	[SerializeField]
	private EventTimedMissionsListItem timedListItemPrefab;

	// Token: 0x04000292 RID: 658
	[SerializeField]
	private GameObject listItemContainerPrefab;

	// Token: 0x04000293 RID: 659
	[SerializeField]
	private VerticalLayoutGroup goalListLayout;

	// Token: 0x04000294 RID: 660
	[SerializeField]
	private VerticalLayoutGroup outerGoalsLayout;

	// Token: 0x04000295 RID: 661
	[SerializeField]
	private ScrollRect goalsScrollRect;

	// Token: 0x04000296 RID: 662
	[SerializeField]
	private PlanetMilestoneRewardPanel planetMilestoneRewardPanel;

	// Token: 0x04000297 RID: 663
	[SerializeField]
	private UIParticleSystem claimPointsParticlesPrefab;

	// Token: 0x04000298 RID: 664
	[SerializeField]
	private Transform tform_portrait_container;

	// Token: 0x04000299 RID: 665
	[SerializeField]
	private Transform tform_portrait_itemParent;

	// Token: 0x0400029A RID: 666
	[SerializeField]
	private Button btn_refresh;

	// Token: 0x0400029B RID: 667
	[SerializeField]
	private Transform tform_portrait_rewards_parent;

	// Token: 0x0400029C RID: 668
	[SerializeField]
	private Transform tform_landscape_container;

	// Token: 0x0400029D RID: 669
	[SerializeField]
	private Transform tform_landscape_itemParent;

	// Token: 0x0400029E RID: 670
	[SerializeField]
	private Button btn_refresh_landscape;

	// Token: 0x0400029F RID: 671
	[SerializeField]
	private Transform tform_landscape_rewards_parent;

	// Token: 0x040002A0 RID: 672
	private const float ANIM_TASK_IN_OUT_TIME = 0.5f;

	// Token: 0x040002A1 RID: 673
	private const float ANIM_TASK_CHANGED_DELAY = 0.5f;

	// Token: 0x040002A2 RID: 674
	private IEventMissionService eventMissionService;

	// Token: 0x040002A3 RID: 675
	private IEventService eventService;

	// Token: 0x040002A4 RID: 676
	private NavigationService navigationService;

	// Token: 0x040002A5 RID: 677
	private AdCapStoreItem refreshStoreItem;

	// Token: 0x040002A6 RID: 678
	private List<EventMissionsListItem> PooledPrefabs = new List<EventMissionsListItem>();

	// Token: 0x040002A7 RID: 679
	private List<EventTimedMissionsListItem> PooledTimdPrefabs = new List<EventTimedMissionsListItem>();

	// Token: 0x040002A8 RID: 680
	private Dictionary<UserEventMission, BaseMissionListItem> listElementsByMission = new Dictionary<UserEventMission, BaseMissionListItem>();

	// Token: 0x040002A9 RID: 681
	private List<Transform> listItemContainers = new List<Transform>();

	// Token: 0x040002AA RID: 682
	private bool isCurrentOrientaionPortrait;

	// Token: 0x040002AB RID: 683
	private Coroutine animationCR;

	// Token: 0x040002AC RID: 684
	private bool isDataWired;

	// Token: 0x040002AD RID: 685
	private int tasksRefreshing;
}
