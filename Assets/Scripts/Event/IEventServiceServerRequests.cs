using System;

// Token: 0x02000093 RID: 147
public interface IEventServiceServerRequests : IDisposable
{
	// Token: 0x06000408 RID: 1032
	IObservable<EventCatalogEntry> GetEventCatalogueEntry();

	// Token: 0x06000409 RID: 1033
	IObservable<MasterEventDataObject> GetMasterEventData(string catalogId);
}
