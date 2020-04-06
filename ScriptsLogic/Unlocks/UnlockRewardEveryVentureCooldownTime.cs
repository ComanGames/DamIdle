using System;

// Token: 0x0200016F RID: 367
public class UnlockRewardEveryVentureCooldownTime : IUnlockReward
{
	// Token: 0x1700010C RID: 268
	// (get) Token: 0x06000BB4 RID: 2996 RVA: 0x000357FE File Offset: 0x000339FE
	// (set) Token: 0x06000BB5 RID: 2997 RVA: 0x00035806 File Offset: 0x00033A06
	public float timeBonus { get; private set; }

	// Token: 0x06000BB6 RID: 2998 RVA: 0x0003580F File Offset: 0x00033A0F
	public UnlockRewardEveryVentureCooldownTime(float timeBonus)
	{
		this.timeBonus = timeBonus;
	}

	// Token: 0x06000BB7 RID: 2999 RVA: 0x0003581E File Offset: 0x00033A1E
	public string Description(GameState state)
	{
		return string.Format("Speed of <b>all investments</b> <color=\"#3D5C14\"><b>x{0}</b></color>", 1f / this.timeBonus);
	}

	// Token: 0x06000BB8 RID: 3000 RVA: 0x0003583B File Offset: 0x00033A3B
	public string ShortDescription(GameState state)
	{
		return string.Format("Everything\nx{0}", 1f / this.timeBonus);
	}

	// Token: 0x06000BB9 RID: 3001 RVA: 0x00035858 File Offset: 0x00033A58
	public void Apply(GameState state)
	{
		foreach (VentureModel ventureModel in state.VentureModels)
		{
			ventureModel.CoolDownTime.Value *= this.timeBonus;
		}
	}
}
