using System;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000AC RID: 172
public class LeaderboardTakeActionPanel : MonoBehaviour
{
	// Token: 0x060004A6 RID: 1190 RVA: 0x00018AD4 File Offset: 0x00016CD4
	public void WireData()
	{
		this.gameController = GameController.Instance;
		GameController.Instance.UserDataService.UserName.Subscribe(delegate(string x)
		{
			this.txt_playerName.text = x;
		}).AddTo(base.gameObject);
		if (GameController.Instance.game.progressionType == PlanetProgressionType.Missions)
		{
			this.txt_score_label.text = "";
			this.txt_score.text = "0";
			this.txt_win_big_title.text = "Claim Points & Win BIG!";
			this.txt_win_big_description.text = "Complete Goals and claim Points to climb the ranks and you could unlock rewards like these and more!";
			this.txt_btn_label.text = "View Your Goals";
			this.btn_launch_location.OnClickAsObservable().Subscribe(new Action<Unit>(this.LaunchGoals)).AddTo(base.gameObject);
		}
		else
		{
			this.txt_score_label.text = "Potential Angel Investors";
			if (this.txt_score != null)
			{
				this.gameController.game.CashOnHand.Subscribe(delegate(double _)
				{
					this.SetAngelCountText();
				}).AddTo(base.gameObject);
			}
			this.txt_win_big_title.text = "Claim Angels & Win BIG!";
			this.txt_win_big_description.text = "Claim Angel Investors to climb the ranks and you could unlock rewards like these and more!";
			this.txt_btn_label.text = "View Your Angel Investors";
			this.btn_launch_location.OnClickAsObservable().Subscribe(new Action<Unit>(this.LaunchInvestorPanel)).AddTo(base.gameObject);
		}
		Dictionary<int, Sprite> promoSlotImages = this.GetPromoSlotImages(1);
		this.img_promotedRewardSlot1.sprite = (promoSlotImages.ContainsKey(1) ? promoSlotImages[1] : this.img_promotedRewardSlot1.sprite);
		this.img_promotedRewardSlot2.sprite = (promoSlotImages.ContainsKey(2) ? promoSlotImages[2] : this.img_promotedRewardSlot2.sprite);
		this.img_promotedRewardSlot3.sprite = (promoSlotImages.ContainsKey(3) ? promoSlotImages[3] : this.img_promotedRewardSlot3.sprite);
	}

	// Token: 0x060004A7 RID: 1191 RVA: 0x00018CCC File Offset: 0x00016ECC
	private void SetAngelCountText()
	{
		double rewardAngelCount = GameController.Instance.AngelService.GetRewardAngelCount();
		this.txt_score.text = NumberFormat.ConvertNormal(rewardAngelCount, 1000000.0, 3);
	}

	// Token: 0x060004A8 RID: 1192 RVA: 0x00018D04 File Offset: 0x00016F04
	private void LaunchInvestorPanel(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Click", "TakeActionPanel", "LaunchInvestors");
		GameController.Instance.NavigationService.CloseAllModals();
		GameController.Instance.NavigationService.CreateModal<InvestorsModal>(NavModals.INVESTORS, false).WireData();
	}

	// Token: 0x060004A9 RID: 1193 RVA: 0x00018D58 File Offset: 0x00016F58
	private void LaunchGoals(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Click", "TakeActionPanel", "LaunchEventMission");
		GameController.Instance.NavigationService.CloseAllModals();
		GameController.Instance.NavigationService.CreateModal<EventMissionsModal>(NavModals.EVENT_MISSIONS_MODAL, false);
	}

	// Token: 0x060004AA RID: 1194 RVA: 0x00018DA8 File Offset: 0x00016FA8
	private Dictionary<int, Sprite> GetPromoSlotImages(int rank = 1)
	{
		Sprite sprite = null;
		EventRewardTier rewardTierByRank = GameController.Instance.EventDataService.GetRewardTierByRank(GameController.Instance.game.planetName, 1.0);
		Dictionary<int, Sprite> dictionary = new Dictionary<int, Sprite>();
		if (rewardTierByRank != null && rewardTierByRank.leaderboardRewardItems != null)
		{
			EventRewardItem[] array = (from x in rewardTierByRank.leaderboardRewardItems
			where x.promoSlotId > 0
			select x).ToArray<EventRewardItem>();
			for (int i = 0; i < array.Length; i++)
			{
				EventRewardItem reward = array[i];
				EventRewardItem eventRewardItem = rewardTierByRank.leaderboardRewardItems.FirstOrDefault((EventRewardItem x) => x.promoSlotId == reward.promoSlotId);
				if (eventRewardItem != null)
				{
					Item item = GameController.Instance.GlobalPlayerData.inventory.GetItemById(eventRewardItem.rewardId);
					if (item != null)
					{
						if (item.ItemType == ItemType.Badge || item.ItemType == ItemType.Trophy)
						{
							(from v in GameController.Instance.IconService.AreBadgesLoaded
							where v
							select v).Subscribe(delegate(bool x)
							{
								sprite = GameController.Instance.IconService.GetBadgeIcon(item.IconName);
							}).AddTo(this);
							sprite = GameController.Instance.IconService.GetBadgeIcon(item.IconName);
						}
						else
						{
							sprite = Resources.Load<Sprite>(item.GetPathToIcon());
						}
					}
				}
				dictionary.Add(reward.promoSlotId, sprite);
			}
		}
		return dictionary;
	}

	// Token: 0x0400042E RID: 1070
	[SerializeField]
	private Text txt_playerName;

	// Token: 0x0400042F RID: 1071
	[SerializeField]
	private Image img_promotedRewardSlot1;

	// Token: 0x04000430 RID: 1072
	[SerializeField]
	private Image img_promotedRewardSlot2;

	// Token: 0x04000431 RID: 1073
	[SerializeField]
	private Image img_promotedRewardSlot3;

	// Token: 0x04000432 RID: 1074
	[SerializeField]
	private Text txt_score;

	// Token: 0x04000433 RID: 1075
	[SerializeField]
	private Text txt_score_label;

	// Token: 0x04000434 RID: 1076
	[SerializeField]
	private Text txt_win_big_title;

	// Token: 0x04000435 RID: 1077
	[SerializeField]
	private Text txt_win_big_description;

	// Token: 0x04000436 RID: 1078
	[SerializeField]
	private Button btn_launch_location;

	// Token: 0x04000437 RID: 1079
	[SerializeField]
	private Text txt_btn_label;

	// Token: 0x04000438 RID: 1080
	private IGameController gameController;
}
