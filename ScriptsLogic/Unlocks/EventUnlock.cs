using System;
using UnityEngine;

// Token: 0x02000169 RID: 361
[Serializable]
public class EventUnlock : Unlock
{
	// Token: 0x06000B8C RID: 2956 RVA: 0x00034F74 File Offset: 0x00033174
	public override string Bonus(GameState state)
	{
		return "Angel Investor effectiveness + 1%";
	}

	// Token: 0x06000B8D RID: 2957 RVA: 0x00034F7B File Offset: 0x0003317B
	public override string Goal(GameState state)
	{
		return string.Format("Get {0} {1}", this.amountToEarn, this.eventItemsName);
	}

	// Token: 0x06000B8E RID: 2958 RVA: 0x00034F98 File Offset: 0x00033198
	public override bool Check(GameState state)
	{
		return !this.Earned.Value && state.eventItems.ContainsKey(this.eventItemsName) && state.eventItems[this.eventItemsName] > this.amountToEarn;
	}

	// Token: 0x06000B8F RID: 2959 RVA: 0x00034FD8 File Offset: 0x000331D8
	public override void Apply(GameState state)
	{
		this.Claimed.Value = true;
		if (this.givesABadge)
		{
			this.GiveBadge();
		}
		GameController.Instance.AngelService.AngelInvestorEffectiveness.Value += Convert.ToDouble(this.percentAngleIncrease);
	}

	// Token: 0x06000B90 RID: 2960 RVA: 0x00035025 File Offset: 0x00033225
	public override string GetDescription()
	{
		return string.Format("{0} {1} - Angel Investor effectiveness increased by 1%", this.amountToEarn, this.eventItemsNamePlural);
	}

	// Token: 0x06000B91 RID: 2961 RVA: 0x00035044 File Offset: 0x00033244
	private void GiveBadge()
	{
		int timesAwarded = 1;
		GameState game = GameController.Instance.game;
		if (game.eventBadges.ContainsKey(this.badge.name))
		{
			timesAwarded = game.eventBadges[this.badge.name].timesAwarded + 1;
		}
		if (!game.eventBadges.ContainsKey(this.badge.name))
		{
			game.eventBadges.Add(this.badge.name, this.badge);
		}
		game.eventBadges[this.badge.name].awarded = true;
		game.eventBadges[this.badge.name].timesAwarded = timesAwarded;
		if (game.eventBadges[this.badge.name].level <= this.badge.level)
		{
			Debug.Log("Badge level = " + game.eventBadges[this.badge.name].level);
			game.eventBadges[this.badge.name] = this.badge;
		}
	}

	// Token: 0x04000A02 RID: 2562
	public string eventItemsName;

	// Token: 0x04000A03 RID: 2563
	public string eventItemsNamePlural;

	// Token: 0x04000A04 RID: 2564
	public string percentAngleIncrease = "0.01";

	// Token: 0x04000A05 RID: 2565
	public string eventName;

	// Token: 0x04000A06 RID: 2566
	public bool givesABadge;

	// Token: 0x04000A07 RID: 2567
	public BadgeInfo badge;

	// Token: 0x04000A08 RID: 2568
	public Sprite theSprite;

	// Token: 0x04000A09 RID: 2569
	public bool showToastFirstTimeOnly = true;
}
