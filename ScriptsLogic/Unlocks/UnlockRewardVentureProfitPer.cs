using System;
using System.Linq;

// Token: 0x0200017B RID: 379
public class UnlockRewardVentureProfitPer : IUnlockReward
{
	// Token: 0x1700011A RID: 282
	// (get) Token: 0x06000C06 RID: 3078 RVA: 0x00036119 File Offset: 0x00034319
	// (set) Token: 0x06000C07 RID: 3079 RVA: 0x00036121 File Offset: 0x00034321
	public string affectedVenture { get; private set; }

	// Token: 0x1700011B RID: 283
	// (get) Token: 0x06000C08 RID: 3080 RVA: 0x0003612A File Offset: 0x0003432A
	// (set) Token: 0x06000C09 RID: 3081 RVA: 0x00036132 File Offset: 0x00034332
	public double profitBonus { get; private set; }

	// Token: 0x06000C0A RID: 3082 RVA: 0x0003613B File Offset: 0x0003433B
	public UnlockRewardVentureProfitPer(string affectedVenture, double profitBonus)
	{
		this.affectedVenture = affectedVenture;
		this.profitBonus = profitBonus;
	}

	// Token: 0x06000C0B RID: 3083 RVA: 0x00036154 File Offset: 0x00034354
	public string Description(GameState state)
	{
		string plural = state.VentureModels.FirstOrDefault(v => v.Id == this.affectedVenture).Plural;
		return string.Format("Profits of <b>{0}</b> <color=\"#3D5C14\"><b>x{1}</b></color>", plural, this.profitBonus);
	}

	// Token: 0x06000C0C RID: 3084 RVA: 0x00036194 File Offset: 0x00034394
	public string ShortDescription(GameState state)
	{
		string name = state.VentureModels.FirstOrDefault(v => v.Id == this.affectedVenture).Name;
		return string.Format("{0}\nx{1}", name, this.profitBonus);
	}

	// Token: 0x06000C0D RID: 3085 RVA: 0x000361D4 File Offset: 0x000343D4
	public void Apply(GameState state)
	{
		state.VentureModels.FirstOrDefault(v => v.Id == this.affectedVenture).ProfitPer.Value *= this.profitBonus;
	}
}
