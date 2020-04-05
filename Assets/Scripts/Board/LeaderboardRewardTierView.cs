using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001D0 RID: 464
public class LeaderboardRewardTierView : MonoBehaviour
{
	// Token: 0x06000DB5 RID: 3509 RVA: 0x0003D4F0 File Offset: 0x0003B6F0
	public void Init(EventRewardTier rewardTier, Action<EventRewardTier> onClick)
	{
		this.Banner.SetActive(false);
		this.rewardTier = rewardTier;
		this.onClick = onClick;
		this.trophyTierImage.sprite = GameController.Instance.IconService.GetSprite(rewardTier.trophyName);
		this.Txt_TierName.text = rewardTier.tierName;
		this.Txt_Ranks.text = string.Format("{0:n0} - {1:n0}", rewardTier.topRank, rewardTier.bottomRank);
		this.Btn_ViewRewards.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnViewTier();
		}).AddTo(base.gameObject);
		if (!string.IsNullOrEmpty(rewardTier.banner))
		{
			this.BannerText.text = rewardTier.banner;
			this.Banner.SetActive(true);
		}
		this.IsSelected(false);
	}

	// Token: 0x06000DB6 RID: 3510 RVA: 0x0003D5CC File Offset: 0x0003B7CC
	private void OnViewTier()
	{
		if (this.onClick != null)
		{
			this.onClick(this.rewardTier);
		}
	}

	// Token: 0x06000DB7 RID: 3511 RVA: 0x0003D5E8 File Offset: 0x0003B7E8
	public void IsSelected(bool isSelected)
	{
		if (isSelected && this.IsInTier())
		{
			this.Img_RewardButtonBackground.color = new Color32(230, 167, 119, byte.MaxValue);
			this.Img_RowBackground.color = new Color32(230, 167, 119, 57);
			this.Txt_TierName.color = new Color32(201, 87, 0, byte.MaxValue);
			this.Txt_Ranks.color = new Color32(201, 87, 0, byte.MaxValue);
			return;
		}
		this.Img_RewardButtonBackground.color = new Color32(94, 93, 137, byte.MaxValue);
		this.Img_RowBackground.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 194);
		this.Txt_TierName.color = new Color32(94, 93, 137, byte.MaxValue);
		this.Txt_Ranks.color = new Color32(54, 49, 45, byte.MaxValue);
	}

	// Token: 0x06000DB8 RID: 3512 RVA: 0x0003D724 File Offset: 0x0003B924
	private bool IsInTier()
	{
		PlanetProgressionType progressionType = GameController.Instance.game.progressionType;
		if (progressionType == PlanetProgressionType.Angels)
		{
			return GameController.Instance.AngelService.AngelsOnHand.Value > 0.0;
		}
		return progressionType == PlanetProgressionType.Missions && GameController.Instance.EventMissionsService.CurentScore.Value > 0;
	}

	// Token: 0x04000BC1 RID: 3009
	public Text Txt_PlayerName;

	// Token: 0x04000BC2 RID: 3010
	public Text Txt_PLayerRank;

	// Token: 0x04000BC3 RID: 3011
	public Button Btn_ViewRewards;

	// Token: 0x04000BC4 RID: 3012
	public Text Txt_TierName;

	// Token: 0x04000BC5 RID: 3013
	public Text Txt_Ranks;

	// Token: 0x04000BC6 RID: 3014
	public Text Txt_RewardLabel;

	// Token: 0x04000BC7 RID: 3015
	public Image Img_RowBackground;

	// Token: 0x04000BC8 RID: 3016
	public Image trophyTierImage;

	// Token: 0x04000BC9 RID: 3017
	public Image Img_RewardButtonBackground;

	// Token: 0x04000BCA RID: 3018
	public GameObject Banner;

	// Token: 0x04000BCB RID: 3019
	public Text BannerText;

	// Token: 0x04000BCC RID: 3020
	public EventRewardTier rewardTier;

	// Token: 0x04000BCD RID: 3021
	private Action<EventRewardTier> onClick;
}
