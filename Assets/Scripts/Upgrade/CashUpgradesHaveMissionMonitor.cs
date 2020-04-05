using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

// Token: 0x02000076 RID: 118
public class CashUpgradesHaveMissionMonitor : IEventMissionMonitor
{
	// Token: 0x06000375 RID: 885 RVA: 0x00013760 File Offset: 0x00011960
	public CashUpgradesHaveMissionMonitor(List<ReactiveProperty<bool>> cashUpgradePurchaseStates)
	{
		this.cashUpgradePurchaseStates = cashUpgradePurchaseStates;
	}

	// Token: 0x06000376 RID: 886 RVA: 0x00013784 File Offset: 0x00011984
	public IObservable<double> MonitorEventMission(UserEventMission mission)
	{
		return from v in (from up in this.cashUpgradePurchaseStates
		select up.AsObservable<bool>()).CombineLatest<bool>()
		select (double)v.Count((bool w) => w);
	}

	// Token: 0x06000377 RID: 887 RVA: 0x000137E4 File Offset: 0x000119E4
	public string GetMissionDescription(UserEventMission mission)
	{
		return "Own <color=\"#3D5C14\">Cash Upgrades</color>";
	}

	// Token: 0x06000378 RID: 888 RVA: 0x000137EB File Offset: 0x000119EB
	public List<EventMissionType> GetTypesToFilterOut()
	{
		return this.filterTypes;
	}

	// Token: 0x0400031A RID: 794
	private List<ReactiveProperty<bool>> cashUpgradePurchaseStates;

	// Token: 0x0400031B RID: 795
	private List<EventMissionType> filterTypes = new List<EventMissionType>
	{
		EventMissionType.CASH_UPGRADES_HAVE
	};
}
