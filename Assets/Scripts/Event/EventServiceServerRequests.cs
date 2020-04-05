using System;
using AdCap;
using PlayFab;
using PlayFab.ClientModels;
using UniRx;

// Token: 0x02000083 RID: 131
public class EventServiceServerRequests : IEventServiceServerRequests, IDisposable
{
	// Token: 0x060003EC RID: 1004 RVA: 0x00002718 File Offset: 0x00000918
	public void Dispose()
	{
	}

	// Token: 0x060003ED RID: 1005 RVA: 0x0001553F File Offset: 0x0001373F
	public IObservable<EventCatalogEntry> GetEventCatalogueEntry()
	{
		return Observable.Create<EventCatalogEntry>(delegate(IObserver<EventCatalogEntry> observer)
		{
			PlayfabRequestHelpers.GetPlayfabOverridableTitleData<EventCatalogEntry>(EventDataService.EVENT_SCHEDULE_KEY, delegate(EventCatalogEntry x)
			{
				observer.OnNext(x);
				observer.OnCompleted();
			}, delegate(string error)
			{
				observer.OnError(new Exception(error));
			});
			return Disposable.Empty;
		});
	}

	// Token: 0x060003EE RID: 1006 RVA: 0x00015565 File Offset: 0x00013765
	public IObservable<MasterEventDataObject> GetMasterEventData(string catalogId)
	{
		return Observable.Create<MasterEventDataObject>(delegate(IObserver<MasterEventDataObject> observer)
		{
			PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest
			{
				CatalogVersion = catalogId
			}, delegate(GetCatalogItemsResult eventCatalog)
			{
				CatalogItem catalogItem = eventCatalog.Catalog.Find((CatalogItem i) => i.ItemId == "content");
				if (catalogItem != null)
				{
					MasterEventDataObject value = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer, "").DeserializeObject<MasterEventDataObject>(catalogItem.CustomData, new DataService.CustomPocoJsonSerializerStrategy());
					observer.OnNext(value);
					observer.OnCompleted();
					return;
				}
				observer.OnError(new Exception("Could not find content in catalog with version " + catalogId));
			}, delegate(PlayFabError error)
			{
				observer.OnError(new Exception(error.ErrorMessage));
			}, null, null);
			return Disposable.Empty;
		});
	}
}
