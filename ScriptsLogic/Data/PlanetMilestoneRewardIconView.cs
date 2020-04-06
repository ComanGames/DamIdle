using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000C1 RID: 193
public class PlanetMilestoneRewardIconView : MonoBehaviour
{
	// Token: 0x06000525 RID: 1317 RVA: 0x0001A988 File Offset: 0x00018B88
	public void Setup(RewardData rewardData)
	{
		Item item = GameController.Instance.GlobalPlayerData.inventory.GetItemById(rewardData.Id);
		this.Item = item;
		if (item.ItemType == ItemType.Badge || item.ItemType == ItemType.Trophy)
		{
			(from v in GameController.Instance.IconService.AreBadgesLoaded
			where v
			select v).Subscribe(delegate(bool x)
			{
				this.itemImage.sprite = GameController.Instance.IconService.GetBadgeIcon(item.IconName);
			}).AddTo(this);
		}
		else
		{
			this.itemImage.sprite = Resources.Load<Sprite>(item.GetPathToIcon());
		}
		if (this.Item != null && this.txt_ItemName != null)
		{
			this.txt_ItemName.text = this.Item.ItemName;
			this.txt_ItemName.enabled = true;
		}
		Color color = GameController.Instance.GlobalPlayerData.inventory.GetColourForRarity(item.RarityRank);
		this.rarityTintables.ForEach(delegate(Image x)
		{
			x.color = color;
		});
	}

	// Token: 0x06000526 RID: 1318 RVA: 0x0001AAC6 File Offset: 0x00018CC6
	public void DarkOutImage()
	{
		this.itemImage.material = null;
		this.itemImage.color = PlanetMilestoneRewardIconView.DARK_COLOR;
	}

	// Token: 0x06000527 RID: 1319 RVA: 0x0001AAE4 File Offset: 0x00018CE4
	public void GreyOutImage()
	{
		this.itemImage.material = this.material;
		Color grey = Color.grey;
		grey.a = 0.3f;
		this.itemImage.color = grey;
	}

	// Token: 0x06000528 RID: 1320 RVA: 0x0001AB20 File Offset: 0x00018D20
	public void RestoreImage()
	{
		this.itemImage.material = null;
		this.itemImage.color = Color.white;
	}

	// Token: 0x04000480 RID: 1152
	[SerializeField]
	private Material material;

	// Token: 0x04000481 RID: 1153
	[SerializeField]
	private Image itemImage;

	// Token: 0x04000482 RID: 1154
	[SerializeField]
	private Text txt_ItemName;

	// Token: 0x04000483 RID: 1155
	[SerializeField]
	private List<Image> rarityTintables = new List<Image>();

	// Token: 0x04000484 RID: 1156
	[HideInInspector]
	private Item Item;

	// Token: 0x04000485 RID: 1157
	private static readonly Color DARK_COLOR = new Color(0.0470588244f, 0.08235294f, 0f, 1f);
}
