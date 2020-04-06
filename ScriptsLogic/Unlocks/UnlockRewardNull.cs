using System;

// Token: 0x02000176 RID: 374
public class UnlockRewardNull : IUnlockReward
{
	// Token: 0x06000BE4 RID: 3044 RVA: 0x00002718 File Offset: 0x00000918
	public void Apply(GameState state)
	{
	}

	// Token: 0x06000BE5 RID: 3045 RVA: 0x00035D2F File Offset: 0x00033F2F
	public string Description(GameState state)
	{
		return "Null";
	}

	// Token: 0x06000BE6 RID: 3046 RVA: 0x00035D2F File Offset: 0x00033F2F
	public string ShortDescription(GameState state)
	{
		return "Null";
	}
}
