using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

// Token: 0x0200007C RID: 124
public class VentureHaveMissionMonitor : IEventMissionMonitor
{
	// Token: 0x0600038C RID: 908 RVA: 0x00013A42 File Offset: 0x00011C42
	public VentureHaveMissionMonitor(ReactiveCollection<VentureModel> ventureModels)
	{
		this.ventureModels = ventureModels;
	}

	// Token: 0x0600038D RID: 909 RVA: 0x00013A5C File Offset: 0x00011C5C
	public IObservable<double> MonitorEventMission(UserEventMission mission)
	{
		return this.ventureModels.First(x => x.Id == mission.Venture).TotalOwned;
	}

	// Token: 0x0600038E RID: 910 RVA: 0x00013A94 File Offset: 0x00011C94
	public string GetMissionDescription(UserEventMission mission)
	{
		string plural = this.ventureModels.FirstOrDefault(x => x.Id == mission.Venture).Plural;
		return string.Format("Own <color=\"#3D5C14\">{0}</color>", plural);
	}

	// Token: 0x0600038F RID: 911 RVA: 0x00013AD6 File Offset: 0x00011CD6
	public List<EventMissionType> GetTypesToFilterOut()
	{
		return this.filterTypes;
	}

	// Token: 0x04000323 RID: 803
	private ReactiveCollection<VentureModel> ventureModels;

	// Token: 0x04000324 RID: 804
	private List<EventMissionType> filterTypes = new List<EventMissionType>();
}
