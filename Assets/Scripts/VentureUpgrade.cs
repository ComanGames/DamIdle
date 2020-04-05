using System;
using System.Linq;
using LitJson;

// Token: 0x0200011C RID: 284
[Serializable]
public class VentureUpgrade : Upgrade
{
	// Token: 0x1700008D RID: 141
	// (get) Token: 0x0600079B RID: 1947 RVA: 0x00028977 File Offset: 0x00026B77
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public override string Effect
	{
		get
		{
			return string.Format("{0} profit x{1}", this.ventureName, this.profitBonus);
		}
	}

	// Token: 0x0600079C RID: 1948 RVA: 0x00028994 File Offset: 0x00026B94
	public override void Apply(GameState state)
	{
		this.IsPurchased.Value = true;
		state.VentureModels.FirstOrDefault((VentureModel v) => v.Name == this.ventureName).ProfitPer.Value *= this.profitBonus;
	}

	// Token: 0x0600079D RID: 1949 RVA: 0x000289D0 File Offset: 0x00026BD0
	public override void Copy(object o)
	{
		Upgrade upgrade = (Upgrade)o;
		this.IsPurchased.Value = upgrade.IsPurchased.Value;
	}

	// Token: 0x04000714 RID: 1812
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public string ventureName;

	// Token: 0x04000715 RID: 1813
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public double profitBonus;
}
