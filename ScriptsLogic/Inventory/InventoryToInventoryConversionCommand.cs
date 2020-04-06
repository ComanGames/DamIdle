using System;
using System.Collections.Generic;

// Token: 0x02000017 RID: 23
public class InventoryToInventoryConversionCommand
{
	// Token: 0x0600005C RID: 92 RVA: 0x00003414 File Offset: 0x00001614
	public void Execute(List<Inv2InvConversion> conversions, PlayerData playerData)
	{
		for (int i = 0; i < conversions.Count; i++)
		{
			Inv2InvConversion inv2InvConversion = conversions[i];
			if (playerData.Has(inv2InvConversion.OldId))
			{
				int @int = playerData.GetInt(inv2InvConversion.OldId, 0);
				playerData.Remove(inv2InvConversion.OldId);
				if (playerData.Has(inv2InvConversion.NewId))
				{
					int num = playerData.GetInt(inv2InvConversion.NewId, 0) + @int;
					playerData.Set(inv2InvConversion.NewId, string.Concat(num));
				}
				else
				{
					playerData.Add(inv2InvConversion.NewId, (double)@int);
				}
			}
		}
	}
}
