using System;
using System.Linq;

// Token: 0x0200016A RID: 362
[Serializable]
public class EveryVentureUnlock : Unlock
{
	// Token: 0x06000B93 RID: 2963 RVA: 0x0003518D File Offset: 0x0003338D
	public override string Bonus(GameState state)
	{
		return this.Reward.Description(state);
	}

	// Token: 0x06000B94 RID: 2964 RVA: 0x0003519B File Offset: 0x0003339B
	public override string Goal(GameState state)
	{
		return string.Format("Own {0} of Everything", this.amountToEarn);
	}

	// Token: 0x06000B95 RID: 2965 RVA: 0x000351B2 File Offset: 0x000333B2
	public override bool Check(GameState state)
	{
		return state.VentureModels.All(venture => venture.TotalOwned.Value >= (double)this.amountToEarn);
	}

	// Token: 0x06000B96 RID: 2966 RVA: 0x000351CB File Offset: 0x000333CB
	public override void Apply(GameState state)
	{
		if (this.Claimed.Value)
		{
			return;
		}
		this.Claimed.Value = true;
		this.EverClaimed.Value = true;
		this.Reward.Apply(state);
	}

	// Token: 0x06000B97 RID: 2967 RVA: 0x000351FF File Offset: 0x000333FF
	public override string GetDescription()
	{
		return string.Format("{0} of everything - {1}!", this.amountToEarn, this.Bonus(GameController.Instance.game));
	}
}
