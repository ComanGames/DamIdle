using System;

// Token: 0x0200016C RID: 364
public interface IUnlockReward
{
	// Token: 0x06000BA4 RID: 2980
	string Description(GameState state);

	// Token: 0x06000BA5 RID: 2981
	string ShortDescription(GameState state);

	// Token: 0x06000BA6 RID: 2982
	void Apply(GameState state);
}
