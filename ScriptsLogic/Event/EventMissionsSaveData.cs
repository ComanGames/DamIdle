using System;
using System.Collections.Generic;

// Token: 0x02000065 RID: 101
[Serializable]
public class EventMissionsSaveData
{
	// Token: 0x040002DE RID: 734
	public int CurrentPoints;

	// Token: 0x040002DF RID: 735
	public int NumberOfClaimedTasks;

	// Token: 0x040002E0 RID: 736
	public int NumberOfOfferedTasks;

	// Token: 0x040002E1 RID: 737
	public List<EventMissionSaveData> SaveDatas = new List<EventMissionSaveData>();

	// Token: 0x040002E2 RID: 738
	public List<TimedEventMissionSaveData> TimedEventMissionSaveDatas = new List<TimedEventMissionSaveData>();

	// Token: 0x040002E3 RID: 739
	public List<string> EquipItemMissionsCompleted = new List<string>();

	// Token: 0x040002E4 RID: 740
	public double PreviousCashPerSecondAtLastReset;
}
