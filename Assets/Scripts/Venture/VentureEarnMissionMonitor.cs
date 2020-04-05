using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

// Token: 0x0200007B RID: 123
public class VentureEarnMissionMonitor : IEventMissionMonitor
{
	// Token: 0x06000388 RID: 904 RVA: 0x00013995 File Offset: 0x00011B95
	public VentureEarnMissionMonitor(ReactiveCollection<VentureModel> ventureModels)
	{
		this.ventureModels = ventureModels;
	}

	// Token: 0x06000389 RID: 905 RVA: 0x000139B0 File Offset: 0x00011BB0
	public IObservable<double> MonitorEventMission(UserEventMission mission)
	{
		return this.ventureModels.First((VentureModel x) => x.Id == mission.Venture).TotalOwned.Pairwise(delegate(double x, double y)
		{
			double num = y - x;
			if (num > 0.0)
			{
				return mission.CurrentCount.Value + num;
			}
			return mission.CurrentCount.Value;
		});
	}

	// Token: 0x0600038A RID: 906 RVA: 0x000139F8 File Offset: 0x00011BF8
	public string GetMissionDescription(UserEventMission mission)
	{
		string plural = this.ventureModels.FirstOrDefault((VentureModel x) => x.Id == mission.Venture).Plural;
		return string.Format("Buy More <color=\"#3D5C14\">{0}</color>", plural);
	}

	// Token: 0x0600038B RID: 907 RVA: 0x00013A3A File Offset: 0x00011C3A
	public List<EventMissionType> GetTypesToFilterOut()
	{
		return this.filterTypes;
	}

	// Token: 0x04000321 RID: 801
	private ReactiveCollection<VentureModel> ventureModels;

	// Token: 0x04000322 RID: 802
	private List<EventMissionType> filterTypes = new List<EventMissionType>();
}
