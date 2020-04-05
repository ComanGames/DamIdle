using System;
using System.Collections.Generic;

// Token: 0x0200015B RID: 347
public static class GameStateTools
{
	// Token: 0x06000ACE RID: 2766 RVA: 0x000314C0 File Offset: 0x0002F6C0
	public static Venture BuildVenture(Dictionary<string, string> vd)
	{
		return new Venture
		{
			id = vd["id"],
			name = vd["name"],
			plural = vd["plural"],
			bonusName = vd["bonusName"],
			baseAmount = Convert.ToInt32(vd["baseAmount"]),
			profitPer = Convert.ToDouble(vd["profitPer"]),
			costPer = Convert.ToDouble(vd["costPer"]),
			cooldownTime = Convert.ToDouble(vd["cooldownTime"]),
			expenseRate = Convert.ToDouble(vd["expenseRate"])
		};
	}

	// Token: 0x06000ACF RID: 2767 RVA: 0x00031584 File Offset: 0x0002F784
	public static ManagerUpgrade BuildManager(Dictionary<string, string> md)
	{
		ManagerUpgrade managerUpgrade = null;
		string a = md["managerType"];
		if (!(a == "RunVentureManager"))
		{
			if (!(a == "AccountantManager"))
			{
				if (a == "ManagerFeature")
				{
					managerUpgrade = new ManagerFeature
					{
						name = md["name"],
						description = md["description"],
						onPurchaseInstantiate = md["onPurchaseInstantiate"]
					};
				}
			}
			else
			{
				managerUpgrade = new AccountantManager
				{
					ventureName = md["ventureName"],
					effect = Convert.ToDouble(md["effect"]),
					showsCPS = Convert.ToBoolean(md["showCPS"])
				};
			}
		}
		else
		{
			managerUpgrade = new RunVentureManager
			{
				ventureName = md["ventureName"]
			};
		}
		if (managerUpgrade != null)
		{
			managerUpgrade.id = md["id"];
			managerUpgrade.name = md["name"];
			managerUpgrade.imageName = md["imageName"];
			managerUpgrade.cost = Convert.ToDouble(md["cost"]);
			managerUpgrade.currency = (Upgrade.Currency)Convert.ToInt32(md["currency"]);
		}
		return managerUpgrade;
	}
}
