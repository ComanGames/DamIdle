using System;

// Token: 0x02000171 RID: 369
public class UnlockRewardFluxCap : IUnlockReward
{
	// Token: 0x06000BC0 RID: 3008 RVA: 0x00035960 File Offset: 0x00033B60
	public UnlockRewardFluxCap(int fluxCapQuantity)
	{
		this.fluxCapQuantity = fluxCapQuantity;
	}

	// Token: 0x06000BC1 RID: 3009 RVA: 0x0003596F File Offset: 0x00033B6F
	public void Apply(GameState state)
	{
		state.fluxCapitalorQuantity.Value += this.fluxCapQuantity;
		state.planetPlayerData.Add("Flux Capacitor", (double)this.fluxCapQuantity);
		state.planetPlayerData.Save();
	}

	// Token: 0x06000BC2 RID: 3010 RVA: 0x000359AC File Offset: 0x00033BAC
	public string Description(GameState state)
	{
		return string.Format("{0}\nFluxcap", this.fluxCapQuantity);
	}

	// Token: 0x06000BC3 RID: 3011 RVA: 0x000359C3 File Offset: 0x00033BC3
	public string ShortDescription(GameState state)
	{
		return string.Format("{0}\nFluxCap", this.fluxCapQuantity);
	}

	// Token: 0x04000A20 RID: 2592
	private int fluxCapQuantity;
}
