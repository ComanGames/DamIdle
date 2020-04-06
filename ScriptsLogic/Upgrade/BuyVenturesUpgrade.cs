using System;
using System.Linq;
using LitJson;

// Token: 0x02000111 RID: 273
[Serializable]
public class BuyVenturesUpgrade : Upgrade
{
	// Token: 0x17000089 RID: 137
	// (get) Token: 0x06000734 RID: 1844 RVA: 0x00026786 File Offset: 0x00024986
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public override string Effect
	{
		get
		{
			return string.Format("+{0} {1}", this.purchaseAmount, this.ventureName);
		}
	}

	// Token: 0x06000735 RID: 1845 RVA: 0x000267A3 File Offset: 0x000249A3
	public override void Apply(GameState state)
	{
		this.IsPurchased.Value = true;
		state.VentureModels.FirstOrDefault(v => v.Name == this.ventureName).NumOwned_Upgrades.Value += (double)this.purchaseAmount;
	}

	// Token: 0x06000736 RID: 1846 RVA: 0x000267E0 File Offset: 0x000249E0
	public override void Copy(object o)
	{
		Upgrade upgrade = (Upgrade)o;
		this.IsPurchased.Value = upgrade.IsPurchased.Value;
	}

	// Token: 0x040006BB RID: 1723
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public int purchaseAmount;

	// Token: 0x040006BC RID: 1724
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public string ventureName;
}
