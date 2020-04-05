using System;
using System.Collections.Generic;
using AdCap;
using AdCap.Store;

// Token: 0x020001FC RID: 508
public interface ITriggerService : IDisposable
{
	// Token: 0x06000ECF RID: 3791
	void Init(IGameController gameController, IUserDataService UserDataService, DataService dataService, IStoreService storeService, IAngelInvestorService angelService, IDateTimeService dateTimeService, NavigationService navigationService, IEventMissionService eventMissionService, PlanetMilestoneService planetMilestoneService, FirstTimeBuyerService firstTimeBuyerService);

	// Token: 0x06000ED0 RID: 3792
	IObservable<bool> MonitorTriggers(List<TriggerData> triggerDatas, bool defaultValueWhenNoTriggersPresent);

	// Token: 0x06000ED1 RID: 3793
	bool CheckTrigger(TriggerData triggerData);
}
