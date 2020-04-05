using System;
using LitJson;
using UniRx;

// Token: 0x02000180 RID: 384
public abstract class Unlock
{
	// Token: 0x06000C40 RID: 3136
	public abstract string Bonus(GameState state);

	// Token: 0x06000C41 RID: 3137
	public abstract string Goal(GameState state);

	// Token: 0x06000C42 RID: 3138
	public abstract bool Check(GameState state);

	// Token: 0x06000C43 RID: 3139
	public abstract void Apply(GameState state);

	// Token: 0x06000C44 RID: 3140
	public abstract string GetDescription();

	// Token: 0x06000C45 RID: 3141 RVA: 0x000375A8 File Offset: 0x000357A8
	public UnlockSaveData Save()
	{
		return new UnlockSaveData
		{
			id = this.name
		};
	}

	// Token: 0x04000A6E RID: 2670
	public string name;

	// Token: 0x04000A6F RID: 2671
	public float order;

	// Token: 0x04000A70 RID: 2672
	public int amountToEarn;

	// Token: 0x04000A71 RID: 2673
	public bool showAllTimes = true;

	// Token: 0x04000A72 RID: 2674
	public bool analytics;

	// Token: 0x04000A73 RID: 2675
	public bool permanent;

	// Token: 0x04000A74 RID: 2676
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public IUnlockReward Reward;

	// Token: 0x04000A75 RID: 2677
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public readonly BoolReactiveProperty Earned = new BoolReactiveProperty(false);

	// Token: 0x04000A76 RID: 2678
	public readonly BoolReactiveProperty Claimed = new BoolReactiveProperty(false);

	// Token: 0x04000A77 RID: 2679
	public readonly BoolReactiveProperty EverClaimed = new BoolReactiveProperty(false);
}
