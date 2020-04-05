using System;
using LitJson;
using UnityEngine;

// Token: 0x020000AE RID: 174
[Serializable]
public class ManagerFeature : ManagerUpgrade
{
	// Token: 0x17000072 RID: 114
	// (get) Token: 0x060004B4 RID: 1204 RVA: 0x000190CD File Offset: 0x000172CD
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public override string Effect
	{
		get
		{
			return this.description;
		}
	}

	// Token: 0x060004B5 RID: 1205 RVA: 0x000190D8 File Offset: 0x000172D8
	public override void Apply(GameState state)
	{
		this.IsPurchased.Value = true;
		string a = this.onPurchaseInstantiate;
		if (a == "QuikBuy")
		{
			GameController.Instance.UpgradeService.QuikBuyUnlocked.Value = true;
			return;
		}
		if (!(a == "NextUpgradeView"))
		{
			return;
		}
		if (GameObject.Find(this.onPurchaseInstantiate))
		{
			return;
		}
		GameObject gameObject = Resources.Load(this.onPurchaseInstantiate) as GameObject;
		if (gameObject)
		{
			Transform transform = GameObject.Find("NextUpgradeViewParent").transform;
			Object.Instantiate<GameObject>(gameObject, transform, false).name = this.onPurchaseInstantiate;
		}
	}

	// Token: 0x060004B6 RID: 1206 RVA: 0x00019178 File Offset: 0x00017378
	public override void Copy(object other)
	{
		this.IsPurchased.Value = ((Upgrade)other).IsPurchased.Value;
	}

	// Token: 0x0400043D RID: 1085
	public static string INSTANTIATE_QUIK_BUY = "QuikBuy";

	// Token: 0x0400043E RID: 1086
	public static string INSTANTIATE_NEXT_UPGRADE_VIEW = "NextUpgradeView";

	// Token: 0x0400043F RID: 1087
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public string description = "What do I do?";

	// Token: 0x04000440 RID: 1088
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public string onPurchaseInstantiate;
}
