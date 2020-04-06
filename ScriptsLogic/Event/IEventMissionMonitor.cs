using System;
using System.Collections.Generic;

// Token: 0x0200007A RID: 122
public interface IEventMissionMonitor
{
	// Token: 0x06000385 RID: 901
	IObservable<double> MonitorEventMission(UserEventMission mission);

	// Token: 0x06000386 RID: 902
	string GetMissionDescription(UserEventMission mission);

	// Token: 0x06000387 RID: 903
	List<EventMissionType> GetTypesToFilterOut();
}
