using System;
using System.Collections.Generic;
using System.Linq;
using AdCap;
using PlayFab;
using UniRx;
using UnityEngine;

// Token: 0x0200007E RID: 126
public class EventDataService : IEventDataService, IDisposable
{
	// Token: 0x1700004C RID: 76
	// (get) Token: 0x06000394 RID: 916 RVA: 0x00013B6A File Offset: 0x00011D6A
	// (set) Token: 0x06000395 RID: 917 RVA: 0x00013B72 File Offset: 0x00011D72
	public ReactiveCollection<EventData> EventDataList { get; private set; }

	// Token: 0x1700004D RID: 77
	// (get) Token: 0x06000396 RID: 918 RVA: 0x00013B7B File Offset: 0x00011D7B
	// (set) Token: 0x06000397 RID: 919 RVA: 0x00013B83 File Offset: 0x00011D83
	public EventCatalogEntry CatalogEntry { get; private set; }

	// Token: 0x1700004E RID: 78
	// (get) Token: 0x06000398 RID: 920 RVA: 0x00013B8C File Offset: 0x00011D8C
	// (set) Token: 0x06000399 RID: 921 RVA: 0x00013B94 File Offset: 0x00011D94
	public bool WasEventScheduleDownloadedSuccessfully { get; private set; }

	// Token: 0x0600039A RID: 922 RVA: 0x00013B9D File Offset: 0x00011D9D
	public EventDataService()
	{
		this.EventDataList = new ReactiveCollection<EventData>();
		this.WasEventScheduleDownloadedSuccessfully = false;
	}

	// Token: 0x0600039B RID: 923 RVA: 0x00013BCD File Offset: 0x00011DCD
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x0600039C RID: 924 RVA: 0x00013BDC File Offset: 0x00011DDC
	public void Init(IEventServiceServerRequests eventServiceServerRequests, Action onSucces, Action<string> onError)
	{
		this.eventServiceServerRequests = eventServiceServerRequests;
		this.cachedInitOnSuccess = onSucces;
		this.cachedInitOnError = onError;
		eventServiceServerRequests.GetEventCatalogueEntry().Subscribe(new Action<EventCatalogEntry>(this.OnEventCatalogEntry), new Action<Exception>(this.OnEventCatalogueEntryError)).AddTo(this.disposables);
	}

	// Token: 0x0600039D RID: 925 RVA: 0x00013C30 File Offset: 0x00011E30
	private void OnEventCatalogEntry(EventCatalogEntry eventCatalogEntry)
	{
		if (string.IsNullOrEmpty(eventCatalogEntry.CatalogId))
		{
			this.OnEventCatalogueEntryError(new Exception("Event Title Data Is is empty"));
			return;
		}
		this.CatalogEntry = eventCatalogEntry;
		this.eventServiceServerRequests.GetMasterEventData(eventCatalogEntry.CatalogId).Subscribe(new Action<MasterEventDataObject>(this.OnMasterEventData), new Action<Exception>(this.OnMasterEventDataError)).AddTo(this.disposables);
	}

	// Token: 0x0600039E RID: 926 RVA: 0x00013C9C File Offset: 0x00011E9C
	private void OnEventCatalogueEntryError(Exception error)
	{
		this.WasEventScheduleDownloadedSuccessfully = false;
		Debug.LogError(error.Message);
		if (this.cachedInitOnError != null)
		{
			this.cachedInitOnError(error.Message);
		}
	}

	// Token: 0x0600039F RID: 927 RVA: 0x00013CC9 File Offset: 0x00011EC9
	private void OnMasterEventData(MasterEventDataObject masterEventData)
	{
		this.WasEventScheduleDownloadedSuccessfully = true;
		this.ExtractEventData(masterEventData);
		if (this.cachedInitOnSuccess != null)
		{
			this.cachedInitOnSuccess();
		}
	}

	// Token: 0x060003A0 RID: 928 RVA: 0x00013C9C File Offset: 0x00011E9C
	private void OnMasterEventDataError(Exception error)
	{
		this.WasEventScheduleDownloadedSuccessfully = false;
		Debug.LogError(error.Message);
		if (this.cachedInitOnError != null)
		{
			this.cachedInitOnError(error.Message);
		}
	}

	// Token: 0x060003A1 RID: 929 RVA: 0x00013CEC File Offset: 0x00011EEC
	private EventRewardTiers GetEventRewards(string eventId)
	{
		EventRewardTiers result = null;
		if (this.eventRewards.ContainsKey(eventId))
		{
			result = this.eventRewards[eventId];
		}
		else
		{
			Debug.LogWarningFormat("Could not find rewards for event [{0}]", new object[]
			{
				eventId
			});
			GameController.Instance.AnalyticService.SendNavActionAnalytics("No rewards for event", eventId, "");
		}
		return result;
	}

	// Token: 0x060003A2 RID: 930 RVA: 0x00013D48 File Offset: 0x00011F48
	public List<EventRewardTier> GetLeaderBoardTiersForEvent(string eventId)
	{
		List<EventRewardTier> list = new List<EventRewardTier>();
		EventRewardTiers eventRewardTiers = this.GetEventRewards(eventId);
		if (eventRewardTiers != null)
		{
			foreach (EventRewardTier item in eventRewardTiers.rewardTiers)
			{
				list.Add(item);
			}
		}
		return list;
	}

	// Token: 0x060003A3 RID: 931 RVA: 0x00013DB0 File Offset: 0x00011FB0
	public EventRewardTier GetRewardTierByRank(string eventID, double playerRank)
	{
		List<EventRewardTier> leaderBoardTiersForEvent = this.GetLeaderBoardTiersForEvent(eventID);
		EventRewardTier eventRewardTier = leaderBoardTiersForEvent.FirstOrDefault((EventRewardTier x) => (double)x.bottomRank >= playerRank && playerRank >= (double)x.topRank);
		if (eventRewardTier == null)
		{
			EventRewardTier eventRewardTier2 = leaderBoardTiersForEvent.Aggregate(delegate(EventRewardTier i1, EventRewardTier i2)
			{
				if (i1.bottomRank <= i2.bottomRank)
				{
					return i2;
				}
				return i1;
			});
			if (playerRank > (double)eventRewardTier2.bottomRank)
			{
				eventRewardTier = eventRewardTier2;
			}
		}
		return eventRewardTier;
	}

	// Token: 0x060003A4 RID: 932 RVA: 0x00013E20 File Offset: 0x00012020
	private void OnJson(string metadata, string data)
	{
		ISerializerPlugin plugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer, "");
		Dictionary<string, object> dictionary = plugin.DeserializeObject<Dictionary<string, object>>(metadata);
		Dictionary<string, object> dictionary2 = plugin.DeserializeObject<Dictionary<string, object>>(data);
		if (dictionary2.ContainsKey("error"))
		{
			Debug.LogError("<b><color=red>Error downloading JSON data.</color></b>  Maybe incorrect sheet id or password? " + dictionary2["error"]);
			this.WasEventScheduleDownloadedSuccessfully = false;
			if (this.cachedInitOnError != null)
			{
				this.cachedInitOnError("Error downloading event JSON data:" + dictionary2["error"]);
				return;
			}
		}
		else
		{
			Debug.LogFormat("Downloaded <b>{0}</b>, validating...", new object[]
			{
				dictionary["Name"]
			});
			MasterEventDataObject masterEventData = plugin.DeserializeObject<MasterEventDataObject>(data, new DataService.CustomPocoJsonSerializerStrategy());
			this.ExtractEventData(masterEventData);
			this.WasEventScheduleDownloadedSuccessfully = true;
			if (this.cachedInitOnSuccess != null)
			{
				this.cachedInitOnSuccess();
			}
		}
	}

	// Token: 0x060003A5 RID: 933 RVA: 0x00013EEC File Offset: 0x000120EC
	private void ExtractEventData(MasterEventDataObject masterEventData)
	{
		foreach (EventData item in masterEventData.events)
		{
			this.EventDataList.Add(item);
		}
		foreach (EventRewardTiers eventRewardTiers in masterEventData.tiers)
		{
			this.eventRewards.Add(eventRewardTiers.eventId, eventRewardTiers);
			using (List<EventRewardTier>.Enumerator enumerator3 = eventRewardTiers.rewardTiers.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					EventRewardTier tier = enumerator3.Current;
					EventRewardTierRewards eventRewardTierRewards = masterEventData.rewards.FirstOrDefault((EventRewardTierRewards x) => x.tierId == tier.tierId);
					if (eventRewardTierRewards != null)
					{
						tier.leaderboardRewardItems.AddRange(eventRewardTierRewards.rewards);
					}
					else
					{
						Debug.LogError("Error deserializing leaderboard tier rewards for tier " + tier.tierId);
					}
				}
			}
		}
	}

	// Token: 0x0400032A RID: 810
	public const string MASTER_EVENT_DATA_FILE = "MasterEventData.txt";

	// Token: 0x0400032B RID: 811
	public static string EVENT_SCHEDULE_KEY = "event_schedule_steam";

	// Token: 0x0400032C RID: 812
	private const string SPREADSHEET_ID = "1Q2ZQjCvUYLJTZE0MeSSu1d-FBwWqwliOcLabH8LxBBw";

	// Token: 0x0400032D RID: 813
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x0400032E RID: 814
	private Action cachedInitOnSuccess;

	// Token: 0x0400032F RID: 815
	private Action<string> cachedInitOnError;

	// Token: 0x04000330 RID: 816
	private IEventServiceServerRequests eventServiceServerRequests;

	// Token: 0x04000331 RID: 817
	private Dictionary<string, EventRewardTiers> eventRewards = new Dictionary<string, EventRewardTiers>();
}
