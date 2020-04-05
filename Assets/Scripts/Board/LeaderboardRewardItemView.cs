using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001CE RID: 462
public class LeaderboardRewardItemView : MonoBehaviour
{
	// Token: 0x06000DA8 RID: 3496 RVA: 0x0003D172 File Offset: 0x0003B372
	public void ShowBanner(string bannerText)
	{
		this.Banner.SetActive(true);
	}

	// Token: 0x06000DA9 RID: 3497 RVA: 0x0003D180 File Offset: 0x0003B380
	internal void Init(EventRewardItem item)
	{
		Item itemById = GameController.Instance.GlobalPlayerData.inventory.GetItemById(item.rewardId);
		this.itemIconView.Setup(itemById, true);
		this.Txt_Qty.text = item.qty.ToString();
		this.Txt_Name.text = itemById.ItemName;
		this.Quantity_image_bg.color = GameController.Instance.GlobalPlayerData.inventory.GetColourForRarity(itemById.RarityRank);
		if (!string.IsNullOrEmpty(item.banner))
		{
			this.ShowBanner(item.banner);
			this.Banner.SetActive(true);
			return;
		}
		this.Banner.SetActive(false);
	}

	// Token: 0x04000BB3 RID: 2995
	public Text Txt_Name;

	// Token: 0x04000BB4 RID: 2996
	public Text Txt_Qty;

	// Token: 0x04000BB5 RID: 2997
	public Image Quantity_image_bg;

	// Token: 0x04000BB6 RID: 2998
	public GameObject Banner;

	// Token: 0x04000BB7 RID: 2999
	public BaseItemIconView itemIconView;
}
