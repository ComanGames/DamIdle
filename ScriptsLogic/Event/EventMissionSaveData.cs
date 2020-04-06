using System;

// Token: 0x02000063 RID: 99
[Serializable]
public class EventMissionSaveData
{
	// Token: 0x040002D5 RID: 725
	public int ID;

	// Token: 0x040002D6 RID: 726
	public EventMissionType Type;

	// Token: 0x040002D7 RID: 727
	public string Venture;

	// Token: 0x040002D8 RID: 728
	public double CurrentCount;

	// Token: 0x040002D9 RID: 729
	public double TargetAmount;

	// Token: 0x040002DA RID: 730
	public int RewardAmount;

	// Token: 0x040002DB RID: 731
	public bool IsClaimed;
}
