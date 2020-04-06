using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200007F RID: 127
public class EventIntroModal : AnimatedModal
{
	// Token: 0x060003A7 RID: 935 RVA: 0x00014040 File Offset: 0x00012240
	public override void Init(NavigationService navService)
	{
		base.Init(navService);
		this.eventService = GameController.Instance.EventService;
		this.txt_titleText.text = string.Format("{0}", GameController.Instance.game.planetTitle);
		EventModel eventModel = this.eventService.ActiveEvents.FirstOrDefault(x => x.Id == GameController.Instance.game.planetName);
		if (eventModel != null)
		{
			eventModel.TimeRemaining.Subscribe(delegate(double time)
			{
				this.txt_timeRemainingText.text = time.ToCountdownTrim();
			}).AddTo(base.gameObject);
		}
		PlanetProgressionType progressionType = GameController.Instance.game.progressionType;
		if (progressionType == PlanetProgressionType.Angels)
		{
			GameController.Instance.PlanetThemeService.AngelImage.Subscribe(delegate(Sprite x)
			{
				this.img_AngelsIcon.sprite = x;
			}).AddTo(base.gameObject);
		}
		this.btn_clickBlocker.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.animator.speed = 8f;
		}).AddTo(base.gameObject);
		this.animator.speed = 2f;
		this.animator.SetTrigger((progressionType == PlanetProgressionType.Missions) ? "DoGoalsFlow" : "DoAngelFlow");
		List<Item> list = new List<Item>();
		List<PlanetMilestone> planetMilestones = GameController.Instance.game.PlanetMilestones;
		if (planetMilestones.Count > 0)
		{
			for (int i = 0; i < planetMilestones.Count; i++)
			{
				PlanetMilestoneRewardData planetMilestoneRewardData = planetMilestones[i].Rewards[0];
				Item itemById = GameController.Instance.GlobalPlayerData.inventory.GetItemById(planetMilestoneRewardData.Id);
				if (itemById != null && itemById.ItemType == ItemType.Badge)
				{
					list.Add(itemById);
				}
			}
		}
		else
		{
			List<UnlockRewardBadge> list2 = (from x in GameController.Instance.UnlockService.Unlocks
			where x.Reward is UnlockRewardBadge
			select x.Reward as UnlockRewardBadge).ToList<UnlockRewardBadge>();
			for (int j = 0; j < list2.Count; j++)
			{
				Item itemById2 = GameController.Instance.GlobalPlayerData.inventory.GetItemById(list2[j].badgeId);
				list.Add(itemById2);
			}
		}
		Item badge = null;
		for (int k = 0; k < list.Count; k++)
		{
			Item item = list[k];
			if (item != null && item.ItemType == ItemType.Badge && !string.IsNullOrEmpty(item.BonusCustomData))
			{
				string[] array = item.BonusCustomData.Split(new char[]
				{
					':'
				});
				if (array.Length >= 1 && (array[0] == GameController.Instance.game.PlanetData.PlanetName || array[0] != "AllEvents"))
				{
					badge = item;
					break;
				}
			}
		}
		this.img_rewardsBadge.gameObject.SetActive(badge != null);
		if (badge != null)
		{
			(from v in GameController.Instance.IconService.AreBadgesLoaded
			where v
			select v).Subscribe(delegate(bool x)
			{
				this.img_rewardsBadge.sprite = GameController.Instance.IconService.GetBadgeIcon(badge.IconName);
			}).AddTo(this);
		}
		this.btn_continue.OnClickAsObservable().Subscribe(new Action<Unit>(this.CloseModalLocal)).AddTo(base.gameObject);
	}

	// Token: 0x060003A8 RID: 936 RVA: 0x000143D4 File Offset: 0x000125D4
	private void CloseModalLocal(Unit u)
	{
		GameController.Instance.GlobalPlayerData.SetBool(GameController.Instance.game.planetName + "_hasSeenPlanetIntro", true);
		this.CloseModal(u);
		if (GameController.Instance.game.progressionType == PlanetProgressionType.Angels)
		{
			FTUE_Manager.ShowFTUE("Leaderboards", null);
			return;
		}
		FTUE_Manager.ShowFTUE("EventMission_Intro", null);
	}

	// Token: 0x04000332 RID: 818
	[SerializeField]
	private Text txt_titleText;

	// Token: 0x04000333 RID: 819
	[SerializeField]
	private Text txt_timeRemainingText;

	// Token: 0x04000334 RID: 820
	[SerializeField]
	private GameObject go_AngelsStep1;

	// Token: 0x04000335 RID: 821
	[SerializeField]
	private Image img_AngelsIcon;

	// Token: 0x04000336 RID: 822
	[SerializeField]
	private Image img_rewardsBadge;

	// Token: 0x04000337 RID: 823
	[SerializeField]
	private Button btn_continue;

	// Token: 0x04000338 RID: 824
	[SerializeField]
	private Button btn_clickBlocker;

	// Token: 0x04000339 RID: 825
	[SerializeField]
	private Animator animator;

	// Token: 0x0400033A RID: 826
	private IEventService eventService;
}
