using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001CB RID: 459
public class LeaderboardItemView : MonoBehaviour
{
	// Token: 0x06000D9B RID: 3483 RVA: 0x0003CBB4 File Offset: 0x0003ADB4
	public void Init(LeaderboardItem item)
	{
		if (string.IsNullOrEmpty(item.name))
		{
			item.name = string.Format("Player_{0}", item.pfabId.Substring(0, 5));
		}
		this.Txt_Name.text = item.name;
		double num;
		if (GameController.Instance.game.progressionType == PlanetProgressionType.Missions)
		{
			num = (double)item.val;
		}
		else
		{
			num = GameState.GetValueFromLeaderboardScore(item.val);
		}
		if (num < 999999.0)
		{
			this.Txt_Cash.text = NumberFormat.Convert(num, 999999.0, false, 3);
		}
		else
		{
			this.Txt_Cash.text = NumberFormat.Convert(num, 999999.0, true, 3);
		}
		this.Txt_Rank.text = item.position.ToString();
		if (item.me)
		{
			this.Img_BgName.color = this.NameColorMe;
			this.Img_BgRank.color = this.RankColorMe;
		}
	}

	// Token: 0x04000BA0 RID: 2976
	public Text Txt_Name;

	// Token: 0x04000BA1 RID: 2977
	public Text Txt_Cash;

	// Token: 0x04000BA2 RID: 2978
	public Text Txt_Rank;

	// Token: 0x04000BA3 RID: 2979
	public Image Img_BgName;

	// Token: 0x04000BA4 RID: 2980
	public Image Img_BgRank;

	// Token: 0x04000BA5 RID: 2981
	public Color NameColorMe;

	// Token: 0x04000BA6 RID: 2982
	public Color RankColorMe;
}
