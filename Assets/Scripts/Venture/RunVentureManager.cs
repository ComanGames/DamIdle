using System;
using System.Linq;
using LitJson;

// Token: 0x020000B1 RID: 177
[Serializable]
public class RunVentureManager : ManagerUpgrade
{
	// Token: 0x17000074 RID: 116
	// (get) Token: 0x060004C3 RID: 1219 RVA: 0x00019357 File Offset: 0x00017557
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public override string Effect
	{
		get
		{
			return string.Format("Runs {0}", GameController.Instance.game.VentureModels.FirstOrDefault((VentureModel v) => v.Name == this.ventureName).Plural);
		}
	}

	// Token: 0x060004C4 RID: 1220 RVA: 0x00019388 File Offset: 0x00017588
	public override void Apply(GameState state)
	{
		this.IsPurchased.Value = true;
		VentureModel ventureModel = state.VentureModels.FirstOrDefault((VentureModel v) => v.Name == this.ventureName);
		ventureModel.IsManaged.Value = true;
		if (!ventureModel.IsRunning.Value && ventureModel.TotalOwned.Value > 0.0)
		{
			ventureModel.Run();
		}
	}

	// Token: 0x060004C5 RID: 1221 RVA: 0x000193F0 File Offset: 0x000175F0
	public override void Copy(object o)
	{
		Upgrade upgrade = (Upgrade)o;
		this.IsPurchased.Value = upgrade.IsPurchased.Value;
	}

	// Token: 0x04000446 RID: 1094
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public string ventureName;
}
