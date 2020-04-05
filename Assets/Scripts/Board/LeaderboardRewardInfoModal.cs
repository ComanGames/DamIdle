using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001CD RID: 461
public class LeaderboardRewardInfoModal : AnimatedModal
{
	// Token: 0x06000DA4 RID: 3492 RVA: 0x0003D045 File Offset: 0x0003B245
	protected override void Awake()
	{
		this.Btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.CloseModal)).AddTo(base.gameObject);
		base.Awake();
	}

	// Token: 0x06000DA5 RID: 3493 RVA: 0x0003D078 File Offset: 0x0003B278
	public void WireData(EventRewardTier rewardTier)
	{
		this.Txt_Title.text = rewardTier.tierName;
		this.Txt_Rank.text = string.Format("Tier Rewards : Rank {0:n0} - {1:n0}", rewardTier.topRank, rewardTier.bottomRank);
		this.trophyIcon.sprite = GameController.Instance.IconService.GetSprite(rewardTier.trophyName);
		foreach (object obj in this.Trn_Container)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		rewardTier.leaderboardRewardItems.ForEach(delegate(EventRewardItem item)
		{
			LeaderboardRewardItemView component = Object.Instantiate<GameObject>(this.RewardPrefab).GetComponent<LeaderboardRewardItemView>();
			component.Init(item);
			component.transform.SetParent(this.Trn_Container, false);
		});
	}

	// Token: 0x04000BAD RID: 2989
	public Text Txt_Title;

	// Token: 0x04000BAE RID: 2990
	public Text Txt_Rank;

	// Token: 0x04000BAF RID: 2991
	public Transform Trn_Container;

	// Token: 0x04000BB0 RID: 2992
	public Button Btn_Close;

	// Token: 0x04000BB1 RID: 2993
	public Image trophyIcon;

	// Token: 0x04000BB2 RID: 2994
	public GameObject RewardPrefab;
}
