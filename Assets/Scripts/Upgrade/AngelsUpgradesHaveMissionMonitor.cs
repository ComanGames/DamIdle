using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

// Token: 0x02000072 RID: 114
public class AngelsUpgradesHaveMissionMonitor : IEventMissionMonitor
{
	// Token: 0x06000365 RID: 869 RVA: 0x000135A3 File Offset: 0x000117A3
	public AngelsUpgradesHaveMissionMonitor(List<ReactiveProperty<bool>> angelUpgradePurchaseStates)
	{
		this.angelUpgradePurchaseStates = angelUpgradePurchaseStates;
	}

	// Token: 0x06000366 RID: 870 RVA: 0x000135C8 File Offset: 0x000117C8
	public IObservable<double> MonitorEventMission(UserEventMission mission)
	{
		return from v in (from up in this.angelUpgradePurchaseStates
		select up.AsObservable<bool>()).CombineLatest<bool>()
		select (double)v.Count((bool w) => w);
	}

	// Token: 0x06000367 RID: 871 RVA: 0x00013628 File Offset: 0x00011828
	public string GetMissionDescription(UserEventMission mission)
	{
		return "Have <color=\"#3D5C14\">Angel Upgrades</color>";
	}

	// Token: 0x06000368 RID: 872 RVA: 0x0001362F File Offset: 0x0001182F
	public List<EventMissionType> GetTypesToFilterOut()
	{
		return this.filterTypes;
	}

	// Token: 0x04000312 RID: 786
	private List<ReactiveProperty<bool>> angelUpgradePurchaseStates;

	// Token: 0x04000313 RID: 787
	private List<EventMissionType> filterTypes = new List<EventMissionType>
	{
		EventMissionType.ANGEL_UPGRADES_HAVE
	};
}
