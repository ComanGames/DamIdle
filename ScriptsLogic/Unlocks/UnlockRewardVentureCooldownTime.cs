using System;
using System.Linq;

// Token: 0x0200017A RID: 378
public class UnlockRewardVentureCooldownTime : IUnlockReward
{
	// Token: 0x17000118 RID: 280
	// (get) Token: 0x06000BFB RID: 3067 RVA: 0x00036010 File Offset: 0x00034210
	// (set) Token: 0x06000BFC RID: 3068 RVA: 0x00036018 File Offset: 0x00034218
	public string affectedVenture { get; private set; }

	// Token: 0x17000119 RID: 281
	// (get) Token: 0x06000BFD RID: 3069 RVA: 0x00036021 File Offset: 0x00034221
	// (set) Token: 0x06000BFE RID: 3070 RVA: 0x00036029 File Offset: 0x00034229
	public float timeBonus { get; private set; }

	// Token: 0x06000BFF RID: 3071 RVA: 0x00036032 File Offset: 0x00034232
	public UnlockRewardVentureCooldownTime(string affectedVenture, float timeBonus)
	{
		this.affectedVenture = affectedVenture;
		this.timeBonus = timeBonus;
	}

	// Token: 0x06000C00 RID: 3072 RVA: 0x00036048 File Offset: 0x00034248
	public string Description(GameState state)
	{
		string name = state.VentureModels.FirstOrDefault(v => v.Id == this.affectedVenture).Name;
		return string.Format("Speed of <b>{0}</b> profits <color=\"#3D5C14\"><b>x{1}</b></color>", name, 1f / this.timeBonus);
	}

	// Token: 0x06000C01 RID: 3073 RVA: 0x00036090 File Offset: 0x00034290
	public string ShortDescription(GameState state)
	{
		string name = state.VentureModels.FirstOrDefault(v => v.Id == this.affectedVenture).Name;
		return string.Format("{0}\nx{1}", name, 1f / this.timeBonus);
	}

	// Token: 0x06000C02 RID: 3074 RVA: 0x000360D6 File Offset: 0x000342D6
	public void Apply(GameState state)
	{
		state.VentureModels.FirstOrDefault(v => v.Id == this.affectedVenture).CoolDownTime.Value *= this.timeBonus;
	}
}
