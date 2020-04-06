using System;
using UniRx;
using UnityEngine;

// Token: 0x02000214 RID: 532
public class ToastEventListener : MonoBehaviour
{
	// Token: 0x06000F6B RID: 3947 RVA: 0x0004756C File Offset: 0x0004576C
	private void Awake()
	{
		GameController.Instance.IsInitialized.First(x => x).Subscribe(delegate(bool _)
		{
			GameController.Instance.UnlockService.OnUnlockAchievedFirstTime.Subscribe(new Action<Unlock>(this.OnUnlockAchieved)).AddTo(base.gameObject);
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000F6C RID: 3948 RVA: 0x000475C4 File Offset: 0x000457C4
	private void OnUnlockAchieved(Unlock unlock)
	{
		if (!GameController.Instance.UnlockService.ShowsNewToast)
		{
			if (unlock == null)
			{
				return;
			}
			if (unlock.showAllTimes)
			{
				Color toastBGColor = this.GetToastBGColor(unlock.Reward);
				this.DisplayToast(unlock.name, unlock.Bonus(GameController.Instance.game), MainUIController.instance.GetUnlockSprite(unlock), this.GetToastOutlineColor(unlock.Reward), toastBGColor);
			}
		}
	}

	// Token: 0x06000F6D RID: 3949 RVA: 0x0004762F File Offset: 0x0004582F
	private void DisplayToast(string text, string desc, Sprite icon, Color borderTintColor, Color bgTintColor)
	{
		this.ToastUi.FlashToast(text, desc, 2f, 4f, icon, borderTintColor, bgTintColor);
	}

	// Token: 0x06000F6E RID: 3950 RVA: 0x00047650 File Offset: 0x00045850
	private Color GetToastOutlineColor(IUnlockReward reward)
	{
		if (reward is UnlockRewardBadge)
		{
			return this.ToastColors.Badge;
		}
		if (reward is UnlockRewardEveryVentureCooldownTime)
		{
			return this.ToastColors.EveryVentureCooldownTime;
		}
		if (reward is UnlockRewardEveryVentureProfitPer)
		{
			return this.ToastColors.EveryVentureProfitPer;
		}
		if (reward is UnlockRewardGold)
		{
			return this.ToastColors.Gold;
		}
		if (reward is UnlockRewardMegaBucks)
		{
			return this.ToastColors.MegaBucks;
		}
		if (reward is UnlockRewardVentureCooldownTime)
		{
			return this.ToastColors.VentureCooldownTime;
		}
		if (reward is UnlockRewardVentureProfitPer)
		{
			return this.ToastColors.VentureProfitPer;
		}
		return Color.white;
	}

	// Token: 0x06000F6F RID: 3951 RVA: 0x000476F0 File Offset: 0x000458F0
	private Color GetToastBGColor(IUnlockReward reward)
	{
		Color result = this.NormalToast;
		if (reward is UnlockRewardEveryVentureProfitPer)
		{
			result = ((((UnlockRewardEveryVentureProfitPer)reward).profitBonus >= 1.0) ? this.GreenToast : this.RedToast);
		}
		else if (reward is UnlockRewardVentureProfitPer)
		{
			result = ((((UnlockRewardVentureProfitPer)reward).profitBonus >= 1.0) ? this.GreenToast : this.RedToast);
		}
		else if (reward is UnlockRewardEveryVentureCooldownTime)
		{
			result = ((((UnlockRewardEveryVentureCooldownTime)reward).timeBonus < 1f) ? this.GreenToast : this.RedToast);
		}
		else if (reward is UnlockRewardVentureCooldownTime)
		{
			result = ((((UnlockRewardVentureCooldownTime)reward).timeBonus < 1f) ? this.GreenToast : this.RedToast);
		}
		return result;
	}

	// Token: 0x04000D48 RID: 3400
	public ToastUI ToastUi;

	// Token: 0x04000D49 RID: 3401
	public ToastOutlineColors ToastColors;

	// Token: 0x04000D4A RID: 3402
	public Color GreenToast;

	// Token: 0x04000D4B RID: 3403
	public Color RedToast;

	// Token: 0x04000D4C RID: 3404
	public Color NormalToast;
}
