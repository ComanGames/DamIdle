using System;
using System.Linq;
using LitJson;

// Token: 0x020000AD RID: 173
[Serializable]
public class AccountantManager : ManagerUpgrade
{
	// Token: 0x17000071 RID: 113
	// (get) Token: 0x060004AE RID: 1198 RVA: 0x00018FB4 File Offset: 0x000171B4
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public override string Effect
	{
		get
		{
			string plural = GameController.Instance.game.VentureModels.FirstOrDefault((VentureModel v) => v.Name == this.ventureName).Plural;
			string text = string.Format("{0} {1}% Cheaper", plural, this.effect * 100.0);
			if (this.showsCPS)
			{
				text += " & Shows Cash Per Sec";
			}
			return text;
		}
	}

	// Token: 0x060004AF RID: 1199 RVA: 0x00019020 File Offset: 0x00017220
	public override void Apply(GameState state)
	{
		this.IsPurchased.Value = true;
		VentureModel ventureModel = state.VentureModels.FirstOrDefault((VentureModel v) => v.Name == this.ventureName);
		if (this.showsCPS)
		{
			ventureModel.ShowCPS.Value = true;
		}
		ventureModel.AccountantEffect.Value *= 1.0 - this.effect;
	}

	// Token: 0x060004B0 RID: 1200 RVA: 0x00019088 File Offset: 0x00017288
	public override void Copy(object o)
	{
		Upgrade upgrade = (Upgrade)o;
		this.IsPurchased.Value = upgrade.IsPurchased.Value;
	}

	// Token: 0x04000439 RID: 1081
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public string ventureName;

	// Token: 0x0400043A RID: 1082
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public string pluralVentureName;

	// Token: 0x0400043B RID: 1083
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public double effect;

	// Token: 0x0400043C RID: 1084
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public bool showsCPS;
}
