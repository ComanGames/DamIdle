using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using Platforms.Logger;
using UniRx;
using UnityEngine;
using Utils;

// Token: 0x02000082 RID: 130
public class EventService : IEventService, IDisposable
{
	// Token: 0x1700005C RID: 92
	// (get) Token: 0x060003C1 RID: 961 RVA: 0x00014827 File Offset: 0x00012A27
	// (set) Token: 0x060003C2 RID: 962 RVA: 0x0001482F File Offset: 0x00012A2F
	public ReactiveProperty<bool> EventUnlocked { get; private set; }

	// Token: 0x1700005D RID: 93
	// (get) Token: 0x060003C3 RID: 963 RVA: 0x00014838 File Offset: 0x00012A38
	// (set) Token: 0x060003C4 RID: 964 RVA: 0x00014840 File Offset: 0x00012A40
	public ReactiveCollection<EventModel> ActiveEvents { get; private set; }

	// Token: 0x1700005E RID: 94
	// (get) Token: 0x060003C5 RID: 965 RVA: 0x00014849 File Offset: 0x00012A49
	// (set) Token: 0x060003C6 RID: 966 RVA: 0x00014851 File Offset: 0x00012A51
	public ReactiveCollection<EventModel> FutureEvents { get; private set; }

	// Token: 0x1700005F RID: 95
	// (get) Token: 0x060003C7 RID: 967 RVA: 0x0001485A File Offset: 0x00012A5A
	// (set) Token: 0x060003C8 RID: 968 RVA: 0x00014862 File Offset: 0x00012A62
	public ReactiveCollection<EventModel> PastEvents { get; private set; }

	// Token: 0x17000060 RID: 96
	// (get) Token: 0x060003C9 RID: 969 RVA: 0x0001486B File Offset: 0x00012A6B
	// (set) Token: 0x060003CA RID: 970 RVA: 0x00014873 File Offset: 0x00012A73
	public ReactiveCollection<PendingEventRewards> PendingEventRewards { get; private set; }

	// Token: 0x060003CB RID: 971 RVA: 0x0001487C File Offset: 0x00012A7C
	public EventService()
	{
		this.ActiveEvents = new ReactiveCollection<EventModel>();
		this.FutureEvents = new ReactiveCollection<EventModel>();
		this.PastEvents = new ReactiveCollection<EventModel>();
		this.EventUnlocked = new ReactiveProperty<bool>(false);
		this.PendingEventRewards = new ReactiveCollection<PendingEventRewards>();
	}

	// Token: 0x060003CC RID: 972 RVA: 0x000148FE File Offset: 0x00012AFE
	public void Dispose()
	{
		this.disposables.Dispose();
		this.timerServiceDisposable.Dispose();
		this.stateDisposables.Dispose();
	}

	// Token: 0x060003CD RID: 973 RVA: 0x00014924 File Offset: 0x00012B24
	public void Init(IGameController gameController, IEventDataService eventDataService, IEventServiceServerRequests eventServiceServerRequests, IDateTimeService dateTimeService, ITriggerService triggerService, TimerService timerService, IUserDataService userDataService)
	{
		this.gameController = gameController;
		this.eventDataService = eventDataService;
		this.eventServiceServerRequests = eventServiceServerRequests;
		this.dateTimeService = dateTimeService;
		this.triggerService = triggerService;
		this.timerService = timerService;
		this.userDataService = userDataService;
		this.LoadEnrolledEvents();
		this.eventDataService.Init(eventServiceServerRequests, new Action(this.OnEventDataServiceInitSuccess), new Action<string>(this.OnDataServiceInitError));
		if (!this.gameController.GlobalPlayerData.GetBool(EventService.EVENT_ACCESSIBLE_PREFS_STRING))
		{
			this.MonitorUnlockCondition();
		}
		else
		{
			this.OnEventsUnlocked(true);
		}
		(from _ in MessageBroker.Default.Receive<AngelsClaimedEvent>()
		where this.gameController.game.progressionType == PlanetProgressionType.Angels
		select _).Subscribe(new Action<AngelsClaimedEvent>(this.OnAngelClaimEvent)).AddTo(this.stateDisposables);
		(from _ in MessageBroker.Default.Receive<EventMissionPointsEarnedEvent>()
		where this.gameController.game.progressionType == PlanetProgressionType.Missions
		select _).Subscribe(new Action<EventMissionPointsEarnedEvent>(this.OnEventMissionPointsClaimedEvent)).AddTo(this.stateDisposables);
		this.MonitorEventEndTime();
	}

	// Token: 0x060003CE RID: 974 RVA: 0x00014A34 File Offset: 0x00012C34
	public bool HasPlayerPerformedEventReset(string eventId)
	{
		return this.enrolledEventList.Contains(eventId);
	}

	// Token: 0x060003CF RID: 975 RVA: 0x00014A44 File Offset: 0x00012C44
	private void MonitorUnlockCondition()
	{
		if (!GameController.Instance.UnfoldingService.HasUnfoldingTrigger(EventService.UNFOLDING_ID))
		{
			this.triggerService.MonitorTriggers(new List<TriggerData>
			{
				EventService.EVENTS_UNLOCKED_TRIGGER
			}, false).Subscribe(new Action<bool>(this.OnEventsUnlockedFirstTime)).AddTo(this.disposables);
			return;
		}
		if (!GameController.Instance.UnfoldingService.CompletedUnfoldingStepIds.Contains(EventService.UNFOLDING_ID.Id))
		{
			(from x in GameController.Instance.UnfoldingService.CompletedUnfoldingStepIds.ObserveAdd()
			where x.Value == EventService.UNFOLDING_ID.Id
			select x).Take(1).Subscribe(delegate(CollectionAddEvent<string> _)
			{
				this.OnEventsUnlockedFirstTime(true);
			}).AddTo(this.disposables);
			return;
		}
		this.OnEventsUnlockedFirstTime(true);
	}

	// Token: 0x060003D0 RID: 976 RVA: 0x00014B28 File Offset: 0x00012D28
	public List<RewardData> GetRewardsFromTier(EventRewardTier rewardTier)
	{
		List<RewardData> list = new List<RewardData>();
		foreach (EventRewardItem eventRewardItem in rewardTier.leaderboardRewardItems)
		{
			Item itemById = this.gameController.GlobalPlayerData.inventory.GetItemById(eventRewardItem.rewardId);
			RewardData item = new RewardData
			{
				Id = itemById.ItemId,
				Qty = eventRewardItem.qty,
				RewardType = ((itemById.Product == Product.Gold) ? ERewardType.Gold : ERewardType.Item)
			};
			list.Add(item);
		}
		return list;
	}

	// Token: 0x060003D1 RID: 977 RVA: 0x00014BD4 File Offset: 0x00012DD4
	public void OnRewardsClaimed(string eventId)
	{
		this.PendingEventRewards.Remove(this.PendingEventRewards.FirstOrDefault(x => x.eventId == eventId));
		if (this.enrolledEventList.Contains(eventId))
		{
			this.enrolledEventList.Remove(eventId);
			this.SaveEnrolledEvents();
		}
	}

	// Token: 0x060003D2 RID: 978 RVA: 0x00014C3C File Offset: 0x00012E3C
	private void OnAngelClaimEvent(AngelsClaimedEvent angelClaim)
	{
		int angelsLeaderboardScore = this.gameController.game.GetAngelsLeaderboardScore();
		string eventId = this.gameController.game.planetName;
		EventData eventData = this.eventDataService.EventDataList.FirstOrDefault(x => x.id == eventId);
//		this.leaderboardService.PostLeaderboardValue(eventId, eventData.leaderboardType, eventData.leaderboardSize, angelsLeaderboardScore).Take(1).Subscribe(new Action<Tuple<string, int>>(this.OnPostLeaderboardValueSuccess), new Action<Exception>(this.OnPostLeaderboardValueError)).AddTo(this.disposables);
		this.enrolledEventList.Add(this.gameController.game.planetName);
		this.SaveEnrolledEvents();
	}

	// Token: 0x060003D3 RID: 979 RVA: 0x00014D04 File Offset: 0x00012F04
	private void OnEventMissionPointsClaimedEvent(EventMissionPointsEarnedEvent pointsEarnedEvent)
	{
		string eventId = this.gameController.game.planetName;
		EventData eventData = this.eventDataService.EventDataList.FirstOrDefault(x => x.id == eventId);
//		this.leaderboardService.PostLeaderboardValue(this.gameController.game.planetName, eventData.leaderboardType, eventData.leaderboardSize, (int)pointsEarnedEvent.Total).Take(1).Subscribe(new Action<Tuple<string, int>>(this.OnPostLeaderboardValueSuccess), new Action<Exception>(this.OnPostLeaderboardValueError)).AddTo(this.disposables);
		this.enrolledEventList.Add(this.gameController.game.planetName);
		this.SaveEnrolledEvents();
	}

	// Token: 0x060003D4 RID: 980 RVA: 0x00014DCC File Offset: 0x00012FCC
	private void OnPostLeaderboardValueSuccess(Tuple<string, int> result)
	{
		MessageBroker.Default.Publish<EventLeaderboardScorePosted>(new EventLeaderboardScorePosted
		{
			eventId = result.Item1,
			score = result.Item2
		});
	}

	// Token: 0x060003D5 RID: 981 RVA: 0x00014E06 File Offset: 0x00013006
	private void OnPostLeaderboardValueError(Exception error)
	{
		Platforms.Logger.Logger.GetLogger(this).Error("[Event Service] " + error.Message);
		this.gameController.AnalyticService.SendNavActionAnalytics("Events", "failed to postScore", "");
	}

	// Token: 0x060003D6 RID: 982 RVA: 0x00014E44 File Offset: 0x00013044
	private void OnEventDataServiceInitSuccess()
	{
		foreach (EventData eventData in this.eventDataService.EventDataList)
		{
			this.SetupEvent(eventData);
		}
		this.timerServiceDisposable = this.timerService.GetTimer(TimerService.TimerGroups.Global).Subscribe(new Action<TimeSpan>(this.Update));
	}

	// Token: 0x060003D7 RID: 983 RVA: 0x00014EBC File Offset: 0x000130BC
	private void OnDataServiceInitError(string error)
	{
		Platforms.Logger.Logger.GetLogger(this).Error("[Event Service] Failed to Initialize with error: [{0}]", new object[]
		{
			error
		});
	}

	// Token: 0x060003D8 RID: 984 RVA: 0x00014ED8 File Offset: 0x000130D8
	private void Update(TimeSpan time)
	{
		this.EventTimer.Value -= time.TotalSeconds;
	}

	// Token: 0x060003D9 RID: 985 RVA: 0x00014EF3 File Offset: 0x000130F3
	private void OnEventsUnlockedFirstTime(bool isUnlocked)
	{
		if (isUnlocked)
		{
			this.OnEventsUnlocked(true);
		}
	}

	// Token: 0x060003DA RID: 986 RVA: 0x00014EFF File Offset: 0x000130FF
	private void OnEventsUnlocked(bool isUnlocked)
	{
		if (isUnlocked)
		{
			this.gameController.GlobalPlayerData.SetBool(EventService.EVENT_ACCESSIBLE_PREFS_STRING, true);
			this.EventUnlocked.SetValueAndForceNotify(true);
		}
	}

	// Token: 0x060003DB RID: 987 RVA: 0x00014F28 File Offset: 0x00013128
	private void SetupEvent(EventData eventData)
	{
		if (string.IsNullOrEmpty(eventData.abTestGroup) || this.userDataService.ListAbTestGroups().Contains(eventData.abTestGroup))
		{
			EventModel eventModel = new EventModel(eventData);
			if (eventData.endDate.CompareTo(this.dateTimeService.UtcNow) > 0)
			{
				eventModel.TimeRemaining.Value = (eventModel.EndDate - this.dateTimeService.UtcNow).TotalSeconds;
				if (eventData.startDate.CompareTo(this.dateTimeService.UtcNow) > 0)
				{
					this.AddToFutureEvents(eventModel);
					return;
				}
				this.AddToActiveEvents(eventModel);
				return;
			}
			else
			{
				this.AddToPastEvents(eventModel);
			}
		}
	}

	// Token: 0x060003DC RID: 988 RVA: 0x00014FD4 File Offset: 0x000131D4
	private void MonitorEventEndTime()
	{
		Observable.Interval(TimeSpan.FromSeconds(1.0)).StartWith(0L).Subscribe(delegate(long _)
		{
			DateTime utcNow = this.dateTimeService.UtcNow;
			for (int i = this.ActiveEvents.Count - 1; i >= 0; i--)
			{
				this.ActiveEvents[i].TimeRemaining.Value = (this.ActiveEvents[i].EndDate - utcNow).TotalSeconds;
				if (this.ActiveEvents[i].TimeRemaining.Value <= 0.0)
				{
					MessageBroker.Default.Publish<LimitedTimeEventEnded>(new LimitedTimeEventEnded(this.ActiveEvents[i].Id));
					this.OnEventEnded(this.ActiveEvents[i]);
				}
			}
			for (int j = this.FutureEvents.Count - 1; j >= 0; j--)
			{
				this.FutureEvents[j].TimeRemaining.Value = (this.FutureEvents[j].StartDate - utcNow).TotalSeconds;
				if (this.FutureEvents[j].TimeRemaining.Value <= 0.0)
				{
					EventModel eventModel = this.FutureEvents[j];
					this.FutureEvents.RemoveAt(j);
					this.AddToActiveEvents(eventModel);
				}
			}
		}, delegate(Exception x)
		{
			Debug.LogError(x.Message);
		}).AddTo(this.disposables);
	}

	// Token: 0x060003DD RID: 989 RVA: 0x00015038 File Offset: 0x00013238
	private void GetRewardsForEvent(string eventId)
	{
		EventData eventData = this.eventDataService.EventDataList.FirstOrDefault(x => x.id == eventId);
	}

	// Token: 0x060003DE RID: 990 RVA: 0x000150B8 File Offset: 0x000132B8

	// Token: 0x060003DF RID: 991 RVA: 0x0001511F File Offset: 0x0001331F
	private void OnPlayerRankError(Exception error)
	{
		Platforms.Logger.Logger.GetLogger(this).Error(error.Message);
	}

	// Token: 0x060003E0 RID: 992 RVA: 0x00015132 File Offset: 0x00013332
	private void OnEventEnded(EventModel eventModel)
	{
		this.ActiveEvents.Remove(eventModel);
		this.AddToPastEvents(eventModel);
	}

	// Token: 0x060003E1 RID: 993 RVA: 0x00015148 File Offset: 0x00013348
	private void AddToPastEvents(EventModel eventModel)
	{
		if (PlayerPrefs.HasKey("GameStateData_" + eventModel.Id))
		{
			eventModel.State.Value = EventState.past;
			this.PastEvents.Add(eventModel);
		}
		eventModel.ShouldShowEventPromo.Value = false;
		if (this.enrolledEventList.Contains(eventModel.Id))
		{
			this.GetRewardsForEvent(eventModel.Id);
		}
	}

	// Token: 0x060003E2 RID: 994 RVA: 0x000151B0 File Offset: 0x000133B0
	private void AddToActiveEvents(EventModel eventModel)
	{
		eventModel.State.Value = EventState.current;
		bool flag = GameController.Instance.GlobalPlayerData.Get("last_promo_event_id_seen", "").StartsWith(eventModel.Id);
		eventModel.ShouldShowEventPromo.Value = !flag;
		this.ActiveEvents.Add(eventModel);
	}

	// Token: 0x060003E3 RID: 995 RVA: 0x00015209 File Offset: 0x00013409
	private void AddToFutureEvents(EventModel eventModel)
	{
		eventModel.State.Value = EventState.future;
		this.FutureEvents.Add(eventModel);
	}

	// Token: 0x060003E4 RID: 996 RVA: 0x00015224 File Offset: 0x00013424
	private void SaveEnrolledEvents()
	{
		string value = string.Join(",", this.enrolledEventList.ToArray<string>());
		this.gameController.GlobalPlayerData.Set(EventService.ENROLLED_KEY, value);
	}

	// Token: 0x060003E5 RID: 997 RVA: 0x00015260 File Offset: 0x00013460
	private void LoadEnrolledEvents()
	{
		string text = this.gameController.GlobalPlayerData.Get(EventService.ENROLLED_KEY, "");
		if (!string.IsNullOrEmpty(text))
		{
			string[] array = text.Split(new char[]
			{
				','
			});
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Replace("_steam", "");
				this.enrolledEventList.Add(array[i]);
			}
		}
	}

	// Token: 0x060003E6 RID: 998 RVA: 0x000152D4 File Offset: 0x000134D4
	public void MarkAllEventsAsParticipatedCheat()
	{
		foreach (EventData eventData in this.eventDataService.EventDataList)
		{
			PlayerPrefs.SetString("GameStateData_" + eventData.id, "");
		}
	}

	// Token: 0x04000342 RID: 834
	public ReactiveProperty<double> EventTimer = new DoubleReactiveProperty();

	// Token: 0x04000347 RID: 839
	public static readonly string EVENT_ACCESSIBLE_PREFS_STRING = "EventAccessible";

	// Token: 0x04000348 RID: 840
	public static readonly string ENROLLED_KEY = "eventPlanetEnrollmentName";

    // Token: 0x04000349 RID: 841
    private static readonly UnfoldingId UNFOLDING_ID = new UnfoldingId
    {
        Id = "ShowEvent"
    };

    // Token: 0x0400034A RID: 842
    public static readonly TriggerData EVENTS_UNLOCKED_TRIGGER = new TriggerData
	{
		Value = "5",
		Id = "oil",
		Operator = ">=",
		PlanetId = "Earth",
		TriggerGroup = 0,
		TriggerType = ETriggerType.VentureQuantity
	};

	// Token: 0x0400034B RID: 843
	private IGameController gameController;

	// Token: 0x0400034C RID: 844
	private IEventDataService eventDataService;

	// Token: 0x0400034D RID: 845
	private IEventServiceServerRequests eventServiceServerRequests;

	// Token: 0x0400034E RID: 846
	private IDateTimeService dateTimeService;

	// Token: 0x0400034F RID: 847
	private ITriggerService triggerService;


	// Token: 0x04000351 RID: 849
	private IUserDataService userDataService;

	// Token: 0x04000352 RID: 850
	private TimerService timerService;

	// Token: 0x04000353 RID: 851
	private HashSet<string> enrolledEventList = new HashSet<string>();

	// Token: 0x04000354 RID: 852
	private IDisposable timerServiceDisposable = Disposable.Empty;

	// Token: 0x04000355 RID: 853
	private readonly CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000356 RID: 854
	private readonly CompositeDisposable stateDisposables = new CompositeDisposable();
}
