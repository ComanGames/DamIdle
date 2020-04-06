using System;
using System.Linq;
using UnityEngine;

// Token: 0x0200017F RID: 383
[Serializable]
public class SingleVentureUnlock : Unlock
{
	// Token: 0x06000C38 RID: 3128 RVA: 0x0003518D File Offset: 0x0003338D
	public override string Bonus(GameState state)
	{
		return this.Reward.Description(state);
	}

	// Token: 0x06000C39 RID: 3129 RVA: 0x00037472 File Offset: 0x00035672
	public override string Goal(GameState state)
	{
		return string.Format("Own {0}", this.amountToEarn);
	}

	// Token: 0x06000C3A RID: 3130 RVA: 0x0003748C File Offset: 0x0003568C
	public override bool Check(GameState state)
	{
		if (this.Earned.Value)
		{
			return false;
		}
		VentureModel ventureModel = state.VentureModels.FirstOrDefault(v => v.Name == this.ventureName);
		if (ventureModel == null)
		{
			Debug.LogWarningFormat("Could not find venture [{0}] for VentureBasedUnlock [{1}]", new object[]
			{
				this.ventureName,
				this.name
			});
			return false;
		}
		return ventureModel.TotalOwned.Value >= (double)this.amountToEarn;
	}

	// Token: 0x06000C3B RID: 3131 RVA: 0x000374FE File Offset: 0x000356FE
	public override void Apply(GameState state)
	{
		if (this.Claimed.Value)
		{
			return;
		}
		this.Claimed.Value = true;
		this.EverClaimed.Value = true;
		if (this.Reward != null)
		{
			this.Reward.Apply(state);
		}
	}

	// Token: 0x06000C3C RID: 3132 RVA: 0x0003753C File Offset: 0x0003573C
	public override string GetDescription()
	{
		string plural = GameController.Instance.game.VentureModels.FirstOrDefault(v => v.Name == this.ventureName).Plural;
		return string.Format("{0} {1} - {2}!", this.amountToEarn, plural, this.Bonus(GameController.Instance.game));
	}

	// Token: 0x04000A6D RID: 2669
	public string ventureName;
}
