using System;
using LitJson;

// Token: 0x02000110 RID: 272
[Serializable]
public class AIUpgrade : Upgrade
{
	// Token: 0x17000088 RID: 136
	// (get) Token: 0x06000730 RID: 1840 RVA: 0x0002670A File Offset: 0x0002490A
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public override string Effect
	{
		get
		{
			return string.Format("Angel Investor effectiveness + {0}%", this.effectivenessAmount * 100.0);
		}
	}

	// Token: 0x06000731 RID: 1841 RVA: 0x0002672B File Offset: 0x0002492B
	public override void Apply(GameState state)
	{
		this.IsPurchased.Value = true;
		GameController.Instance.AngelService.AngelInvestorEffectiveness.Value += this.effectivenessAmount;
	}

	// Token: 0x06000732 RID: 1842 RVA: 0x0002675C File Offset: 0x0002495C
	public override void Copy(object o)
	{
		Upgrade upgrade = (Upgrade)o;
		this.IsPurchased.Value = upgrade.IsPurchased.Value;
	}

	// Token: 0x040006BA RID: 1722
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public double effectivenessAmount;
}
