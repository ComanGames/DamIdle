using System;

// Token: 0x02000170 RID: 368
public class UnlockRewardEveryVentureProfitPer : IUnlockReward
{
	// Token: 0x1700010D RID: 269
	// (get) Token: 0x06000BBA RID: 3002 RVA: 0x000358B4 File Offset: 0x00033AB4
	// (set) Token: 0x06000BBB RID: 3003 RVA: 0x000358BC File Offset: 0x00033ABC
	public double profitBonus { get; private set; }

	// Token: 0x06000BBC RID: 3004 RVA: 0x000358C5 File Offset: 0x00033AC5
	public UnlockRewardEveryVentureProfitPer(double profitBonus)
	{
		this.profitBonus = profitBonus;
	}

	// Token: 0x06000BBD RID: 3005 RVA: 0x000358D4 File Offset: 0x00033AD4
	public string Description(GameState state)
	{
		return string.Format("Profits of everything <color=\"#3D5C14\"><b>x{0}</b></color>", this.profitBonus);
	}

	// Token: 0x06000BBE RID: 3006 RVA: 0x000358EB File Offset: 0x00033AEB
	public string ShortDescription(GameState state)
	{
		return string.Format("Everything\nx{0}", this.profitBonus);
	}

	// Token: 0x06000BBF RID: 3007 RVA: 0x00035904 File Offset: 0x00033B04
	public void Apply(GameState state)
	{
		foreach (VentureModel ventureModel in state.VentureModels)
		{
			ventureModel.ProfitPer.Value *= this.profitBonus;
		}
	}
}
