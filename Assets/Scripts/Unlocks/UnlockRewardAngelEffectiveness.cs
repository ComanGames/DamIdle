using System;

// Token: 0x0200016D RID: 365
public class UnlockRewardAngelEffectiveness : IUnlockReward
{
	// Token: 0x17000109 RID: 265
	// (get) Token: 0x06000BA7 RID: 2983 RVA: 0x00035670 File Offset: 0x00033870
	// (set) Token: 0x06000BA8 RID: 2984 RVA: 0x00035678 File Offset: 0x00033878
	public float effectivenessAmount { get; private set; }

	// Token: 0x06000BA9 RID: 2985 RVA: 0x00035681 File Offset: 0x00033881
	public UnlockRewardAngelEffectiveness(float effectivenessAmount)
	{
		this.effectivenessAmount = effectivenessAmount;
	}

	// Token: 0x06000BAA RID: 2986 RVA: 0x00035690 File Offset: 0x00033890
	public string Description(GameState state)
	{
		return string.Format("Angel Investor effectiveness + {0}%", this.effectivenessAmount * 100f);
	}

	// Token: 0x06000BAB RID: 2987 RVA: 0x000356AD File Offset: 0x000338AD
	public string ShortDescription(GameState state)
	{
		return string.Format("Angel Effectiveness\n + {0}", this.effectivenessAmount);
	}

	// Token: 0x06000BAC RID: 2988 RVA: 0x000356C4 File Offset: 0x000338C4
	public void Apply(GameState state)
	{
		GameController.Instance.AngelService.AngelInvestorEffectiveness.Value += (double)this.effectivenessAmount;
	}
}
