using System;
using LitJson;

// Token: 0x02000112 RID: 274
[Serializable]
public class EverythingUpgrade : Upgrade
{
	// Token: 0x1700008A RID: 138
	// (get) Token: 0x06000739 RID: 1849 RVA: 0x0002681D File Offset: 0x00024A1D
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public override string Effect
	{
		get
		{
			return "All profits x" + this.profitBonus;
		}
	}

	// Token: 0x0600073A RID: 1850 RVA: 0x00026834 File Offset: 0x00024A34
	public override void Apply(GameState state)
	{
		this.IsPurchased.Value = true;
		foreach (VentureModel ventureModel in state.VentureModels)
		{
			ventureModel.ProfitPer.Value *= this.profitBonus;
		}
	}

	// Token: 0x0600073B RID: 1851 RVA: 0x0002689C File Offset: 0x00024A9C
	public override void Copy(object o)
	{
		Upgrade upgrade = (Upgrade)o;
		this.IsPurchased.Value = upgrade.IsPurchased.Value;
	}

	// Token: 0x040006BD RID: 1725
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public double profitBonus;
}
