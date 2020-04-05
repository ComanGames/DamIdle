using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000DD RID: 221
public class RewardView : MonoBehaviour
{
	// Token: 0x060005FE RID: 1534 RVA: 0x0001FC84 File Offset: 0x0001DE84
	public void WireData(IconService iconService, IInventoryService inventory, RewardData reward)
	{
		RewardView.<>c__DisplayClass9_0 CS$<>8__locals1 = new RewardView.<>c__DisplayClass9_0();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.reward = reward;
		this.iconService = iconService;
		this.inventory = inventory;
		this.go_Badge.SetActive(false);
		this.txt_Qty.text = string.Format("x{0}", CS$<>8__locals1.reward.Qty);
		this.img_BG.color = Color.white;
		if (this.go_newBadge != null)
		{
			this.go_newBadge.SetActive(false);
		}
		switch (CS$<>8__locals1.reward.RewardType)
		{
		case ERewardType.None:
			break;
		case ERewardType.Gold:
			this.img_Icon.sprite = this.iconService.GetGoldIcon(CS$<>8__locals1.reward.Qty);
			this.txt_RewardName.text = string.Format("{0} Gold", CS$<>8__locals1.reward.Qty);
			return;
		case ERewardType.Item:
		{
			Item item = this.inventory.GetItemById(CS$<>8__locals1.reward.Id);
			if (item != null)
			{
				Color colourForRarity = GameController.Instance.GlobalPlayerData.inventory.GetColourForRarity(item.RarityRank);
				switch (item.ItemType)
				{
				case ItemType.Badge:
				case ItemType.Trophy:
					this.go_Badge.SetActive(CS$<>8__locals1.reward.Qty > 1);
					(from v in GameController.Instance.IconService.AreBadgesLoaded
					where v
					select v).Subscribe(delegate(bool x)
					{
						CS$<>8__locals1.<>4__this.img_Icon.sprite = CS$<>8__locals1.<>4__this.iconService.GetBadgeIcon(item.IconName);
					}).AddTo(this);
					this.txt_RewardName.text = item.GetNameString();
					goto IL_2AB;
				case ItemType.Currency:
					this.img_Icon.sprite = this.iconService.GetGoldIcon(CS$<>8__locals1.reward.Qty);
					this.txt_RewardName.text = string.Format("{0} Gold", CS$<>8__locals1.reward.Qty);
					goto IL_2AB;
				}
				this.go_Badge.SetActive(CS$<>8__locals1.reward.Qty > 1);
				this.img_Icon.sprite = Resources.Load<Sprite>(item.GetPathToIcon());
				this.txt_RewardName.text = item.GetNameString();
				IL_2AB:
				if (this.img_QtyBadge != null)
				{
					this.img_QtyBadge.color = colourForRarity;
				}
				if (item.ItemType != ItemType.None)
				{
					this.img_BG.color = colourForRarity;
				}
				if (this.go_newBadge != null)
				{
					this.go_newBadge.SetActive(item.IsEquipable() && item.PreviousOwned == 0);
					return;
				}
			}
			break;
		}
		case ERewardType.InvestmentQty:
		{
			this.go_Badge.SetActive(CS$<>8__locals1.reward.Qty > 1);
			ReactiveCollection<VentureModel> ventureModels = GameController.Instance.game.VentureModels;
			int index;
			VentureModel investment = int.TryParse(CS$<>8__locals1.reward.Id, out index) ? ventureModels[index] : ventureModels.FirstOrDefault((VentureModel x) => x.Id == CS$<>8__locals1.reward.Id);
			if (investment != null)
			{
				GameController.Instance.PlanetThemeService.VentureIcons.Subscribe(delegate(IconDataScriptableObject iconData)
				{
					CS$<>8__locals1.<>4__this.img_Icon.sprite = iconData.iconMap[investment.ImageName];
					CS$<>8__locals1.<>4__this.txt_RewardName.text = investment.Name;
				}).AddTo(base.gameObject);
				return;
			}
			break;
		}
		case ERewardType.AngelsOnHand:
			GameController.Instance.PlanetThemeService.AngelImage.Subscribe(delegate(Sprite x)
			{
				CS$<>8__locals1.<>4__this.img_Icon.sprite = x;
			}).AddTo(base.gameObject);
			this.go_Badge.SetActive(CS$<>8__locals1.reward.Qty > 1);
			this.txt_RewardName.text = "Angel Investors";
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	// Token: 0x0400055C RID: 1372
	[SerializeField]
	private Image img_Icon;

	// Token: 0x0400055D RID: 1373
	[SerializeField]
	private Image img_BG;

	// Token: 0x0400055E RID: 1374
	[SerializeField]
	private Text txt_RewardName;

	// Token: 0x0400055F RID: 1375
	[SerializeField]
	private GameObject go_Badge;

	// Token: 0x04000560 RID: 1376
	[SerializeField]
	private Image img_QtyBadge;

	// Token: 0x04000561 RID: 1377
	[SerializeField]
	private Text txt_Qty;

	// Token: 0x04000562 RID: 1378
	[SerializeField]
	private GameObject go_newBadge;

	// Token: 0x04000563 RID: 1379
	private IconService iconService;

	// Token: 0x04000564 RID: 1380
	private IInventoryService inventory;
}
