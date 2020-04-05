using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Ads;
using AdCap.Store;
using UniRx;
using UnityEngine;
using Utils;

// Token: 0x02000058 RID: 88
public class EventMissionService : IEventMissionService, IDisposable
{
	// Token: 0x1700003B RID: 59
	// (get) Token: 0x060002D7 RID: 727 RVA: 0x0000FACA File Offset: 0x0000DCCA
	// (set) Token: 0x060002D8 RID: 728 RVA: 0x0000FAD2 File Offset: 0x0000DCD2
	public ReactiveCollection<UserEventMission> currentMissions { get; private set; }

	// Token: 0x1700003C RID: 60
	// (get) Token: 0x060002D9 RID: 729 RVA: 0x0000FADB File Offset: 0x0000DCDB
	// (set) Token: 0x060002DA RID: 730 RVA: 0x0000FAE3 File Offset: 0x0000DCE3
	public ReactiveProperty<int> CurentScore { get; private set; }

	// Token: 0x1700003D RID: 61
	// (get) Token: 0x060002DB RID: 731 RVA: 0x0000FAEC File Offset: 0x0000DCEC
	// (set) Token: 0x060002DC RID: 732 RVA: 0x0000FAF4 File Offset: 0x0000DCF4
	public ReactiveProperty<AdCapStoreItem> RefreshStoreItem { get; private set; }

	// Token: 0x1700003E RID: 62
	// (get) Token: 0x060002DD RID: 733 RVA: 0x0000FAFD File Offset: 0x0000DCFD
	// (set) Token: 0x060002DE RID: 734 RVA: 0x0000FB05 File Offset: 0x0000DD05
	public Dictionary<int, ReactiveProperty<double>> MissionTimersById { get; private set; }

	// Token: 0x060002DF RID: 735 RVA: 0x0000FB10 File Offset: 0x0000DD10
	public void Init(IGameController gameController, IDateTimeService dateTimeService, TimerService timerService, IAngelInvestorService angelInvestorService, IStoreService storeService, AdWatchService adService, UpgradeService upgradeService, IEventService eventService, IAnalyticService analyticService, ProfitBoostAdService profitBoostAdService)
	{
		this.gameController = gameController;
		this.timerService = timerService;
		this.dateTimeService = dateTimeService;
		this.angelInvestorService = angelInvestorService;
		this.storeService = storeService;
		this.adService = adService;
		this.upgradeService = upgradeService;
		this.eventService = eventService;
		this.analyticService = analyticService;
		this.profitBoostAdService = profitBoostAdService;
		this.currentMissions = new ReactiveCollection<UserEventMission>();
		this.CurentScore = new ReactiveProperty<int>(0);
		this.RefreshStoreItem = new ReactiveProperty<AdCapStoreItem>();
		this.MissionTimersById = new Dictionary<int, ReactiveProperty<double>>();
		AdCapStoreItem adCapStoreItem = this.storeService.CurrentCatalog.FirstOrDefault((AdCapStoreItem x) => x.Id == "event_mission_refresh");
		if (adCapStoreItem == null)
		{
			this.storeService.CurrentCatalog.ObserveAdd().First((CollectionAddEvent<AdCapStoreItem> x) => x.Value.Id == "event_mission_refresh").Subscribe(delegate(CollectionAddEvent<AdCapStoreItem> evt)
			{
				this.RefreshStoreItem.Value = evt.Value;
			}).AddTo(this.disposables);
		}
		else
		{
			this.RefreshStoreItem.Value = adCapStoreItem;
		}
		this.gameController.OnLoadNewPlanetPre += this.OnPreStateChange;
		this.gameController.State.Subscribe(new Action<GameState>(this.OnGameStatechanged)).AddTo(this.disposables);
	}

	// Token: 0x060002E0 RID: 736 RVA: 0x0000FC68 File Offset: 0x0000DE68
	public void Dispose()
	{
		if (this.gameController != null)
		{
			this.gameController.OnLoadNewPlanetPre -= this.OnPreStateChange;
		}
		this.disposables.Dispose();
		this.stateDisposables.Dispose();
	}

	// Token: 0x060002E1 RID: 737 RVA: 0x0000FCA0 File Offset: 0x0000DEA0
	public bool RefreshTasksForGold()
	{
		if (this.RefreshStoreItem.Value != null && this.storeService.AttemptPurchase(this.RefreshStoreItem.Value, this.RefreshStoreItem.Value.Cost.Currency))
		{
			this.ReplaceCurrentMissions();
			return true;
		}
		return false;
	}

	// Token: 0x060002E2 RID: 738 RVA: 0x0000FCF0 File Offset: 0x0000DEF0
	public bool ClearMission(UserEventMission mission)
	{
		if (mission.State.Value == EventMissionState.EXPIRED)
		{
			this.analyticService.SendMissionEvent("MissionCleared", mission, "");
			this.ReplaceMission(mission);
			return true;
		}
		return false;
	}

	// Token: 0x060002E3 RID: 739 RVA: 0x0000FD20 File Offset: 0x0000DF20
	public bool ClaimMissionReward(UserEventMission mission)
	{
		if (mission.State.Value != EventMissionState.COMPLETE)
		{
			return false;
		}
		this.CurentScore.Value += mission.RewardAmount;
		if (mission.Type != EventMissionType.FIRST_AUTO_COMPLETE)
		{
			ReactiveProperty<int> numberOfClaimedTasks = this.NumberOfClaimedTasks;
			int value = numberOfClaimedTasks.Value;
			numberOfClaimedTasks.Value = value + 1;
		}
		mission.IsClaimed.Value = true;
		MessageBroker.Default.Publish<EventMissionPointsEarnedEvent>(new EventMissionPointsEarnedEvent
		{
			Amount = (double)mission.RewardAmount,
			Total = (double)this.CurentScore.Value
		});
		if (mission.Type == EventMissionType.EQUIP_ITEM)
		{
			this.EquipItemMissionsCompleted.Add(mission.Venture);
		}
		this.analyticService.SendMissionEvent("MissionClaimed", mission, string.Concat(new object[]
		{
			mission.RewardAmount,
			":",
			this.CurentScore.Value.ToString(),
			":",
			this.currentState.CashOnHand.Value
		}));
		this.ReplaceMission(mission);
		return true;
	}

	// Token: 0x060002E4 RID: 740 RVA: 0x0000FE3C File Offset: 0x0000E03C
	public string GetMissionDescription(UserEventMission mission)
	{
		IEventMissionMonitor eventMissionMonitor = null;
		if (this.missionMonitorsByType.TryGetValue(mission.Type, out eventMissionMonitor))
		{
			return eventMissionMonitor.GetMissionDescription(mission);
		}
		Debug.LogError("[EventMissionService](GetMissionDescription) Unhandled Type:" + mission.Type);
		return "";
	}

	// Token: 0x060002E5 RID: 741 RVA: 0x0000FE88 File Offset: 0x0000E088
	private void ReplaceMission(UserEventMission mission)
	{
		List<EventMissionType> list = new List<EventMissionType>();
		for (int i = 0; i < this.currentMissions.Count; i++)
		{
			if (this.currentMissions[i] != mission)
			{
				list.AddRange(this.GetTypesToFilterOut(this.currentMissions[i].Type));
			}
			else
			{
				list.Add(this.currentMissions[i].Type);
			}
		}
		List<string> venturesToFilterOut = this.currentMissions.Select(delegate(UserEventMission x)
		{
			if (x != mission && (x.Type == EventMissionType.VENTURE_EARN || x.Type == EventMissionType.VENTURE_HAVE))
			{
				return x.Venture;
			}
			return null;
		}).ToList<string>();
		int index = this.currentMissions.IndexOf(mission);
		this.RemoveCurrentMission(mission);
		bool canBeGolden = this.currentMissions.Count((UserEventMission x) => x.IsTimed) < this.currentStateConstants.MaxSuperTasks;
		UserEventMission userEventMission = this.GeneratePlanetMission(venturesToFilterOut, list, canBeGolden);
		this.currentMissions.Insert(index, userEventMission);
		MessageBroker.Default.Publish<EventMissionChangedEvent>(new EventMissionChangedEvent
		{
			oldMission = mission,
			newMission = userEventMission
		});
		mission.Dispose();
	}

	// Token: 0x060002E6 RID: 742 RVA: 0x0000FFCB File Offset: 0x0000E1CB
	private void RemoveCurrentMission(UserEventMission mission)
	{
		if (this.MissionTimersById.ContainsKey(mission.ID))
		{
			this.MissionTimersById.Remove(mission.ID);
		}
		this.currentMissions.Remove(mission);
	}

	// Token: 0x060002E7 RID: 743 RVA: 0x00010000 File Offset: 0x0000E200
	private IObservable<double> MonitorEventMission(UserEventMission mission)
	{
		IEventMissionMonitor eventMissionMonitor = null;
		if (this.missionMonitorsByType.TryGetValue(mission.Type, out eventMissionMonitor))
		{
			return from v in eventMissionMonitor.MonitorEventMission(mission).CombineLatest(mission.State, (double x, EventMissionState s) => new Tuple<double, EventMissionState>(x, s)).StartWith(new Tuple<double, EventMissionState>(mission.CurrentCount.Value, mission.State.Value)).TakeUntil((Tuple<double, EventMissionState> v) => v.Item1 >= mission.TargetAmount || v.Item2 == EventMissionState.EXPIRED)
			select v.Item1;
		}
		Debug.LogError("[EventMissionService](MonitorEventMission) Unhandled Type:" + mission.Type);
		return null;
	}

	// Token: 0x060002E8 RID: 744 RVA: 0x000100F4 File Offset: 0x0000E2F4
	private void OnPreStateChange()
	{
		this.stateDisposables.Clear();
		foreach (UserEventMission userEventMission in this.currentMissions)
		{
			userEventMission.Dispose();
		}
		this.currentMissions.Clear();
		this.CurentScore.Value = 0;
		this.NumberOfClaimedTasks.Value = 0;
		this.NumberOfOfferedTasks.Value = 0;
		this.PreviousCashPerSecondAtLastReset = 0.0;
		this.gameController.OnPreSavePlanetData -= this.AppendSaveData;
		this.gameController.OnSoftResetPre -= this.onPreReset;
		this.gameController.OnHardResetPre -= this.onPreReset;
		this.difficultyLevels = null;
		this.dayBonuses = null;
		this.currentStateConstants = null;
		this.taskWeights = null;
	}

	// Token: 0x060002E9 RID: 745 RVA: 0x000101E8 File Offset: 0x0000E3E8
	private void ReplaceCurrentMissions()
	{
		List<UserEventMission> list = new List<UserEventMission>();
		for (int i = 0; i < this.currentMissions.Count; i++)
		{
			list.Add(this.currentMissions[i]);
		}
		List<EventMissionType> list2 = new List<EventMissionType>();
		List<string> list3 = new List<string>();
		while (this.currentMissions.Count > 0)
		{
			UserEventMission userEventMission = this.currentMissions[0];
			if (userEventMission.Type == EventMissionType.VENTURE_EARN || userEventMission.Type == EventMissionType.VENTURE_HAVE)
			{
				list3.Add(userEventMission.Venture);
			}
			else
			{
				list2.Add(userEventMission.Type);
			}
			this.RemoveCurrentMission(userEventMission);
			userEventMission.Dispose();
		}
		List<UserEventMission> list4 = this.GeneratePlanetMissions(false, list2, list3);
		for (int j = 0; j < list4.Count; j++)
		{
			this.currentMissions.Add(list4[j]);
		}
		MessageBroker.Default.Publish<EventMissionsRefreshedEvent>(new EventMissionsRefreshedEvent
		{
			newMissions = this.currentMissions.ToList<UserEventMission>(),
			oldMissions = list.ToList<UserEventMission>()
		});
	}

	// Token: 0x060002EA RID: 746 RVA: 0x000102F0 File Offset: 0x0000E4F0
	private void OnGameStatechanged(GameState gameState)
	{
		this.currentState = gameState;
		bool flag = this.currentState.progressionType == PlanetProgressionType.Missions;
		if (gameState.IsEventPlanet && flag)
		{
			Dictionary<EventMissionType, IEventMissionMonitor> dictionary = new Dictionary<EventMissionType, IEventMissionMonitor>();
			dictionary.Add(EventMissionType.ACTIVATE_AD_MULTIPLIER, new ActivateAdMultiplierMissionMonitor(this.profitBoostAdService.AdProfitBoostTimer));
			dictionary.Add(EventMissionType.ANGEL_UPGRADES_HAVE, new AngelsUpgradesHaveMissionMonitor((from x in this.upgradeService.Upgrades
			where x.currency == Upgrade.Currency.AngelInvestors
			select x.IsPurchased).ToList<ReactiveProperty<bool>>()));
			dictionary.Add(EventMissionType.ANGELS_EARN, new AngelsEarnMissionMonitor(this.angelInvestorService.AngelsOnHand));
			dictionary.Add(EventMissionType.ANGELS_HAVE, new AngelsHaveMissionMonitor(this.angelInvestorService.AngelsOnHand));
			dictionary.Add(EventMissionType.CASH_EARN, new CashEarnMissionMonitor(this.currentState.CashOnHand));
			dictionary.Add(EventMissionType.CASH_HAVE, new CashHaveMissionMonitor(this.currentState.CashOnHand));
			dictionary.Add(EventMissionType.CASH_SPEND, new CashSpendMissionMonitor(this.currentState.CashOnHand));
			dictionary.Add(EventMissionType.CASH_UPGRADES_HAVE, new CashUpgradesHaveMissionMonitor((from x in this.upgradeService.Upgrades
			where x.currency == Upgrade.Currency.InGameCash
			select x.IsPurchased).ToList<ReactiveProperty<bool>>()));
			dictionary.Add(EventMissionType.COMPLETE_MISSIONS, new CompleteMissionsMissionMonitor(this.NumberOfClaimedTasks));
			dictionary.Add(EventMissionType.EQUIP_ITEM, new EquipItemMissionMonitor(this.gameController.GlobalPlayerData.inventory));
			dictionary.Add(EventMissionType.VENTURE_EARN, new VentureEarnMissionMonitor(this.currentState.VentureModels));
			dictionary.Add(EventMissionType.VENTURE_HAVE, new VentureHaveMissionMonitor(this.currentState.VentureModels));
			dictionary.Add(EventMissionType.WATCH_ANY_AD, new WatchAnyAdMissionMonitor(MessageBroker.Default.Receive<AdWatchedEvent>()));
			dictionary.Add(EventMissionType.FIRST_AUTO_COMPLETE, new FirstAutoCompleteMissionMonitor());
			this.missionMonitorsByType = dictionary;
			this.difficultyLevels = this.currentState.gameStateSerialized.missionDifficulties.ToList<EventMissionDifficulty>();
			this.dayBonuses = this.currentState.gameStateSerialized.missionDayBonuses.ToList<EventMissionDayBonus>();
			this.currentStateConstants = this.currentState.gameStateSerialized.missionConstants[0];
			this.taskWeights = this.currentState.gameStateSerialized.missionTaskTypeWeights.ToList<EventMissionTaskTypeWeights>();
			this.LoadEventMissions(this.currentState.planetName);
			this.gameController.OnPreSavePlanetData += this.AppendSaveData;
			this.gameController.OnSoftResetPre += this.onPreReset;
			this.gameController.OnHardResetPre += this.onPreReset;
		}
	}

	// Token: 0x060002EB RID: 747 RVA: 0x000105C4 File Offset: 0x0000E7C4
	private void LoadEventMissions(string eventId)
	{
		string name = string.Format("EventMission_{0}", eventId);
		string text = this.gameController.GlobalPlayerData.Get(name, "");
		if (!string.IsNullOrEmpty(text))
		{
			EventMissionsSaveData eventMissionsSaveData = JsonUtility.FromJson<EventMissionsSaveData>(text);
			List<EventMissionSaveData> saveDatas = eventMissionsSaveData.SaveDatas;
			this.CurentScore.Value = eventMissionsSaveData.CurrentPoints;
			this.NumberOfClaimedTasks.Value = eventMissionsSaveData.NumberOfClaimedTasks;
			this.NumberOfOfferedTasks.Value = eventMissionsSaveData.NumberOfOfferedTasks;
			this.EquipItemMissionsCompleted = eventMissionsSaveData.EquipItemMissionsCompleted;
			this.PreviousCashPerSecondAtLastReset = eventMissionsSaveData.PreviousCashPerSecondAtLastReset;
			List<TimedEventMissionSaveData> timedEventMissionSaveDatas = eventMissionsSaveData.TimedEventMissionSaveDatas;
			for (int i = 0; i < timedEventMissionSaveDatas.Count; i++)
			{
				TimedEventMissionSaveData timedEventMissionSaveData = timedEventMissionSaveDatas[i];
				double totalSeconds = DateTime.Parse(timedEventMissionSaveData.EndDate).Subtract(this.dateTimeService.UtcNow).TotalSeconds;
				this.MissionTimersById[timedEventMissionSaveData.MissionID] = new ReactiveProperty<double>(totalSeconds);
			}
			for (int j = 0; j < saveDatas.Count; j++)
			{
				UserEventMission item = this.CreateAndMonitorUserMission(saveDatas[j]);
				this.currentMissions.Add(item);
			}
		}
		else
		{
			List<UserEventMission> list = this.GeneratePlanetMissions(true, null, null);
			for (int k = 0; k < list.Count; k++)
			{
				this.currentMissions.Add(list[k]);
			}
		}
		this.timerService.GetTimer(TimerService.TimerGroups.Global).Subscribe(new Action<TimeSpan>(this.Update)).AddTo(this.stateDisposables);
	}

	// Token: 0x060002EC RID: 748 RVA: 0x00010758 File Offset: 0x0000E958
	private List<UserEventMission> GeneratePlanetMissions(bool isFirstGeneration, List<EventMissionType> pTypesToFilterOut = null, List<string> pVenturesToFilterOut = null)
	{
		List<UserEventMission> list = new List<UserEventMission>();
		int i = 0;
		if (isFirstGeneration)
		{
			i++;
			UserEventMission item = this.GenerateFirstAutoCompleteMission();
			list.Add(item);
		}
		List<EventMissionType> list2 = new List<EventMissionType>();
		if (pTypesToFilterOut != null)
		{
			list2.AddRange(pTypesToFilterOut);
		}
		List<string> list3 = new List<string>();
		if (pVenturesToFilterOut != null)
		{
			list3.AddRange(pVenturesToFilterOut);
		}
		while (i < 4)
		{
			UserEventMission userEventMission = this.GeneratePlanetMission(list3, list2, list.Count((UserEventMission x) => x.IsTimed) < this.currentStateConstants.MaxSuperTasks);
			list2.AddRange(this.GetTypesToFilterOut(userEventMission.Type));
			if (userEventMission.Type == EventMissionType.VENTURE_EARN || userEventMission.Type == EventMissionType.VENTURE_HAVE)
			{
				list3.Add(userEventMission.Venture);
			}
			list.Add(userEventMission);
			i++;
		}
		return list;
	}

	// Token: 0x060002ED RID: 749 RVA: 0x0001082C File Offset: 0x0000EA2C
	private UserEventMission GenerateFirstAutoCompleteMission()
	{
		ReactiveProperty<int> numberOfOfferedTasks = this.NumberOfOfferedTasks;
		int value = numberOfOfferedTasks.Value;
		numberOfOfferedTasks.Value = value + 1;
		return this.CreateAndMonitorUserMission(new EventMissionSaveData
		{
			ID = this.NumberOfOfferedTasks.Value,
			Type = EventMissionType.FIRST_AUTO_COMPLETE,
			Venture = "",
			CurrentCount = 1.0,
			TargetAmount = 1.0,
			IsClaimed = false,
			RewardAmount = this.currentStateConstants.FirstFreeTaskRewardPoints
		});
	}

	// Token: 0x060002EE RID: 750 RVA: 0x000108B8 File Offset: 0x0000EAB8
	private UserEventMission GeneratePlanetMission(List<string> venturesToFilterOut, List<EventMissionType> missionTypesToFilterOut, bool canBeGolden)
	{
		float difficulty_coef = this.SelectMissionDifficulty(this.NumberOfClaimedTasks.Value).DifficultyCoefficient;
		bool flag = false;
		if (canBeGolden && this.NumberOfOfferedTasks.Value >= 4)
		{
			flag = (Random.RandomRange(0, 100) < this.currentStateConstants.SuperTaskPercentThreshold);
			if (flag)
			{
				difficulty_coef = (float)this.currentStateConstants.SuperTaskDifficultyCoef;
			}
		}
		double num = this.CalcuateCashToSpend(difficulty_coef);
		List<string> list = (from x in this.currentState.VentureModels
		select x.Id).ToList<string>();
		for (int i = 0; i < venturesToFilterOut.Count; i++)
		{
			list.Remove(venturesToFilterOut[i]);
		}
		for (int j = list.Count - 1; j >= 0; j--)
		{
			if (this.CalculateMaxCanAffordVenture(difficulty_coef, list[j]) < 1.0)
			{
				list.RemoveAt(j);
			}
		}
		double num2 = Math.Log10(this.gameController.game.TotalCashEarned + 1.0);
		List<WeightedPair<EventMissionType>> list2 = new List<WeightedPair<EventMissionType>>();
		if (list.Count > 0)
		{
			if (num2 >= (double)this.currentStateConstants.FirstMomentExponentThreshold && !missionTypesToFilterOut.Contains(EventMissionType.VENTURE_EARN))
			{
				list2.Add(new WeightedPair<EventMissionType>
				{
					Option = EventMissionType.VENTURE_EARN,
					Weight = this.SelectTaskTypeWeight(EventMissionType.VENTURE_EARN)
				});
			}
			if (!missionTypesToFilterOut.Contains(EventMissionType.VENTURE_HAVE))
			{
				list2.Add(new WeightedPair<EventMissionType>
				{
					Option = EventMissionType.VENTURE_HAVE,
					Weight = this.SelectTaskTypeWeight(EventMissionType.VENTURE_HAVE)
				});
			}
		}
		else
		{
			Debug.LogError("Unable to afford ventures, this is an issue");
		}
		double num3 = this.CalculateAngelHave(difficulty_coef);
		double num4 = this.angelInvestorService.AngelsOnHand.Value * 2.0;
		if (num3 >= 50.0 && num3 >= num4)
		{
			if (!missionTypesToFilterOut.Contains(EventMissionType.ANGELS_HAVE))
			{
				list2.Add(new WeightedPair<EventMissionType>
				{
					Option = EventMissionType.ANGELS_HAVE,
					Weight = this.SelectTaskTypeWeight(EventMissionType.ANGELS_HAVE)
				});
			}
			if (!missionTypesToFilterOut.Contains(EventMissionType.ANGELS_EARN))
			{
				list2.Add(new WeightedPair<EventMissionType>
				{
					Option = EventMissionType.ANGELS_EARN,
					Weight = this.SelectTaskTypeWeight(EventMissionType.ANGELS_EARN)
				});
			}
			if (!missionTypesToFilterOut.Contains(EventMissionType.ANGEL_UPGRADES_HAVE) && num2 <= (double)this.currentStateConstants.ContentCompleteExponent)
			{
				int num5 = this.upgradeService.Upgrades.Count((Upgrade x) => x.currency == Upgrade.Currency.AngelInvestors);
				int num6 = this.upgradeService.Upgrades.Count((Upgrade x) => x.currency == Upgrade.Currency.AngelInvestors && x.IsPurchased.Value);
				double num7 = this.CalculateAngelUpgradeHave(difficulty_coef);
				if (num7 > (double)num6 && num7 != (double)num5)
				{
					list2.Add(new WeightedPair<EventMissionType>
					{
						Option = EventMissionType.ANGEL_UPGRADES_HAVE,
						Weight = this.SelectTaskTypeWeight(EventMissionType.ANGEL_UPGRADES_HAVE)
					});
				}
			}
		}
		if (!missionTypesToFilterOut.Contains(EventMissionType.CASH_HAVE))
		{
			list2.Add(new WeightedPair<EventMissionType>
			{
				Option = EventMissionType.CASH_HAVE,
				Weight = this.SelectTaskTypeWeight(EventMissionType.CASH_HAVE)
			});
		}
		if (!missionTypesToFilterOut.Contains(EventMissionType.CASH_EARN))
		{
			list2.Add(new WeightedPair<EventMissionType>
			{
				Option = EventMissionType.CASH_EARN,
				Weight = this.SelectTaskTypeWeight(EventMissionType.CASH_EARN)
			});
		}
		if (!missionTypesToFilterOut.Contains(EventMissionType.CASH_SPEND))
		{
			list2.Add(new WeightedPair<EventMissionType>
			{
				Option = EventMissionType.CASH_SPEND,
				Weight = this.SelectTaskTypeWeight(EventMissionType.CASH_SPEND)
			});
		}
		if (!missionTypesToFilterOut.Contains(EventMissionType.CASH_UPGRADES_HAVE) && num2 <= (double)this.currentStateConstants.ContentCompleteExponent)
		{
			List<Upgrade> allAffordableCashUpgrades = this.GetAllAffordableCashUpgrades(difficulty_coef);
			int num8 = this.upgradeService.Upgrades.Count((Upgrade x) => x.currency == Upgrade.Currency.InGameCash);
			double num9 = allAffordableCashUpgrades.Sum((Upgrade x) => x.cost);
			if (allAffordableCashUpgrades.Count > 0 && num9 > 0.0 && allAffordableCashUpgrades.Count != num8 && num9 / num > (double)this.currentStateConstants.UpgradeSpendPercentThreshold)
			{
				list2.Add(new WeightedPair<EventMissionType>
				{
					Option = EventMissionType.CASH_UPGRADES_HAVE,
					Weight = this.SelectTaskTypeWeight(EventMissionType.CASH_UPGRADES_HAVE)
				});
			}
		}
		List<PlanetMilestone> planetMilestones = this.currentState.PlanetMilestones;
		List<string> list3 = new List<string>();
		for (int k = 0; k < planetMilestones.Count; k++)
		{
			PlanetMilestoneRewardData planetMilestoneRewardData = planetMilestones[k].Rewards[0];
			if (planetMilestoneRewardData.RewardType == ERewardType.Item)
			{
				Item itemById = this.gameController.GlobalPlayerData.inventory.GetItemById(planetMilestoneRewardData.Id);
				if (!this.EquipItemMissionsCompleted.Contains(itemById.ItemId) && itemById.ItemType == ItemType.Badge && itemById.Owned.Value > 0 && !string.IsNullOrEmpty(itemById.BonusCustomData))
				{
					string[] array = itemById.BonusCustomData.Split(new char[]
					{
						':'
					});
					if (array.Length >= 1 && (array[0] == this.currentState.PlanetData.PlanetName || array[0] == "AllEvents"))
					{
						list3.Add(planetMilestoneRewardData.Id);
					}
				}
			}
		}
		if (list3.Count > 0 && !missionTypesToFilterOut.Contains(EventMissionType.EQUIP_ITEM))
		{
			list2.Add(new WeightedPair<EventMissionType>
			{
				Option = EventMissionType.EQUIP_ITEM,
				Weight = this.SelectTaskTypeWeight(EventMissionType.EQUIP_ITEM)
			});
		}
		if (list2.Count == 0)
		{
			Debug.LogError(string.Concat(new object[]
			{
				"Unable to select a mission type when generating missions cashToSpend(",
				num,
				") venturePool(",
				list.ToCommaSeparateList(),
				")"
			}));
			this.analyticService.SendFailedToGenerateMissionTask(this.gameController.game.planetName, num.ToString(), this.NumberOfClaimedTasks.Value);
			list2.Add(new WeightedPair<EventMissionType>
			{
				Option = EventMissionType.VENTURE_EARN,
				Weight = 50
			});
			list2.Add(new WeightedPair<EventMissionType>
			{
				Option = EventMissionType.VENTURE_HAVE,
				Weight = 50
			});
			if (list.Count == 0)
			{
				list = (from x in this.currentState.VentureModels
				select x.Id).ToList<string>();
			}
		}
		int combinedWeights = 0;
		list2.ForEach(delegate(WeightedPair<EventMissionType> x)
		{
			combinedWeights += x.Weight;
		});
		EventMissionType eventMissionType = EventMissionType.NONE;
		int num10 = Random.Range(0, combinedWeights);
		for (int l = 0; l < list2.Count; l++)
		{
			num10 -= list2[l].Weight;
			if (num10 <= 0)
			{
				eventMissionType = list2[l].Option;
				break;
			}
		}
		string venture = "unknown";
		if (eventMissionType == EventMissionType.VENTURE_EARN || eventMissionType == EventMissionType.VENTURE_HAVE)
		{
			venture = list[Random.Range(0, list.Count)];
		}
		else if (eventMissionType == EventMissionType.EQUIP_ITEM)
		{
			venture = list3[Random.RandomRange(0, list3.Count)];
			flag = false;
		}
		else if (eventMissionType == EventMissionType.ACTIVATE_AD_MULTIPLIER || eventMissionType == EventMissionType.WATCH_ANY_AD)
		{
			flag = false;
		}
		ReactiveProperty<int> numberOfOfferedTasks = this.NumberOfOfferedTasks;
		int value = numberOfOfferedTasks.Value;
		numberOfOfferedTasks.Value = value + 1;
		EventMissionSaveData eventMissionSaveData = new EventMissionSaveData();
		eventMissionSaveData.ID = this.NumberOfOfferedTasks.Value;
		eventMissionSaveData.Type = eventMissionType;
		eventMissionSaveData.Venture = venture;
		eventMissionSaveData.CurrentCount = this.CalculateInitialValue(eventMissionType, venture);
		eventMissionSaveData.TargetAmount = this.CalculateTargetAmount(eventMissionType, venture, difficulty_coef);
		eventMissionSaveData.IsClaimed = false;
		if (flag)
		{
			EventModel eventModel = this.eventService.ActiveEvents.FirstOrDefault((EventModel x) => x.Id == this.currentState.planetName);
			float num11 = this.SelectDayBonusPointsMultiplier(this.dateTimeService.UtcNow, eventModel.StartDate);
			int rewardAmount = (int)(Math.Round((double)((float)this.currentStateConstants.SuperTaskRewardPoints * num11 / 100f)) * 100.0);
			eventMissionSaveData.RewardAmount = rewardAmount;
		}
		else
		{
			eventMissionSaveData.RewardAmount = this.CalculateRewardAmount(eventMissionType, difficulty_coef);
		}
		if (double.IsInfinity(eventMissionSaveData.TargetAmount))
		{
			Debug.LogError("saveData.TargetAmount is infinity");
		}
		if (flag)
		{
			this.MissionTimersById.Add(eventMissionSaveData.ID, new ReactiveProperty<double>(this.currentStateConstants.SuperTaskDuration));
		}
		UserEventMission userEventMission = this.CreateAndMonitorUserMission(eventMissionSaveData);
		this.analyticService.SendMissionEvent("MissionOffered", userEventMission, userEventMission.TargetAmount.ToString());
		return userEventMission;
	}

	// Token: 0x060002EF RID: 751 RVA: 0x00011190 File Offset: 0x0000F390
	private void SaveEventMissions(string eventId)
	{
		EventMissionsSaveData eventMissionsSaveData = new EventMissionsSaveData();
		eventMissionsSaveData.CurrentPoints = this.CurentScore.Value;
		eventMissionsSaveData.NumberOfClaimedTasks = this.NumberOfClaimedTasks.Value;
		eventMissionsSaveData.NumberOfOfferedTasks = this.NumberOfOfferedTasks.Value;
		eventMissionsSaveData.EquipItemMissionsCompleted = this.EquipItemMissionsCompleted.ToList<string>();
		ReactiveCollection<UserEventMission> currentMissions = this.currentMissions;
		for (int i = 0; i < currentMissions.Count; i++)
		{
			UserEventMission mission = currentMissions[i];
			EventMissionSaveData item = this.ConvertToSaveData(mission);
			eventMissionsSaveData.SaveDatas.Add(item);
		}
		foreach (KeyValuePair<int, ReactiveProperty<double>> keyValuePair in this.MissionTimersById)
		{
			DateTime dateTime = this.dateTimeService.UtcNow.Add(TimeSpan.FromSeconds(keyValuePair.Value.Value));
			TimedEventMissionSaveData timedEventMissionSaveData = new TimedEventMissionSaveData();
			timedEventMissionSaveData.MissionID = keyValuePair.Key;
			timedEventMissionSaveData.EndDate = dateTime.ToString();
			eventMissionsSaveData.TimedEventMissionSaveDatas.Add(timedEventMissionSaveData);
		}
		string value = JsonUtility.ToJson(eventMissionsSaveData);
		this.gameController.GlobalPlayerData.Set(string.Format("EventMission_{0}", eventId), value);
		this.gameController.GlobalPlayerData.Save();
	}

	// Token: 0x060002F0 RID: 752 RVA: 0x000112EC File Offset: 0x0000F4EC
	private void AppendSaveData()
	{
		this.SaveEventMissions(this.currentState.planetName);
	}

	// Token: 0x060002F1 RID: 753 RVA: 0x00011300 File Offset: 0x0000F500
	private void onPreReset()
	{
		double num = this.TotalCashPerSecond();
		double previousCashPerSecondAtLastReset = this.PreviousCashPerSecondAtLastReset;
		if (num > previousCashPerSecondAtLastReset)
		{
			this.PreviousCashPerSecondAtLastReset = num;
		}
	}

	// Token: 0x060002F2 RID: 754 RVA: 0x00011328 File Offset: 0x0000F528
	private EventMissionSaveData ConvertToSaveData(UserEventMission mission)
	{
		return new EventMissionSaveData
		{
			ID = mission.ID,
			Type = mission.Type,
			Venture = mission.Venture,
			CurrentCount = mission.CurrentCount.Value,
			IsClaimed = mission.IsClaimed.Value,
			RewardAmount = mission.RewardAmount,
			TargetAmount = mission.TargetAmount
		};
	}

	// Token: 0x060002F3 RID: 755 RVA: 0x00011398 File Offset: 0x0000F598
	private List<EventMissionType> GetTypesToFilterOut(EventMissionType type)
	{
		IEventMissionMonitor eventMissionMonitor = null;
		if (this.missionMonitorsByType.TryGetValue(type, out eventMissionMonitor))
		{
			return eventMissionMonitor.GetTypesToFilterOut();
		}
		Debug.LogError("[EventMissionService](GetTypesToFilterOut) Unhandled Type:" + type);
		return new List<EventMissionType>
		{
			EventMissionType.NONE
		};
	}

	// Token: 0x060002F4 RID: 756 RVA: 0x000113E0 File Offset: 0x0000F5E0
	private EventMissionDifficulty SelectMissionDifficulty(int completedMissionCount)
	{
		for (int i = 0; i < this.difficultyLevels.Count; i++)
		{
			EventMissionDifficulty eventMissionDifficulty = this.difficultyLevels[i];
			if ((double)completedMissionCount >= eventMissionDifficulty.TaskCompletedStartThreshold && (double)completedMissionCount <= eventMissionDifficulty.TaskCompletedEndThreshold)
			{
				return eventMissionDifficulty;
			}
		}
		Debug.LogError("Couldnt find difficulty level for offered mission count=" + completedMissionCount);
		return this.difficultyLevels[this.difficultyLevels.Count - 1];
	}

	// Token: 0x060002F5 RID: 757 RVA: 0x00011454 File Offset: 0x0000F654
	public float SelectDayBonusPointsMultiplier(DateTime utcNow, DateTime startDate)
	{
		double totalDays = utcNow.Subtract(startDate).TotalDays;
		float result = 1f;
		int num = 0;
		while (num < this.dayBonuses.Count && (double)this.dayBonuses[num].Day <= totalDays)
		{
			result = this.dayBonuses[num].Multiplier;
			num++;
		}
		return result;
	}

	// Token: 0x060002F6 RID: 758 RVA: 0x000114B8 File Offset: 0x0000F6B8
	private int SelectTaskTypeWeight(EventMissionType type)
	{
		EventMissionTaskTypeWeights eventMissionTaskTypeWeights = this.taskWeights.FirstOrDefault((EventMissionTaskTypeWeights x) => x.Type == type);
		if (eventMissionTaskTypeWeights != null)
		{
			return eventMissionTaskTypeWeights.Weight;
		}
		return 0;
	}

	// Token: 0x060002F7 RID: 759 RVA: 0x000114F8 File Offset: 0x0000F6F8
	private double CalculateInitialValue(EventMissionType type, string venture)
	{
		if (type == EventMissionType.VENTURE_HAVE)
		{
			return this.currentState.VentureModels.FirstOrDefault((VentureModel x) => x.Id == venture).TotalOwned.Value;
		}
		if (type == EventMissionType.CASH_HAVE)
		{
			return this.currentState.CashOnHand.Value;
		}
		if (type == EventMissionType.ANGELS_HAVE)
		{
			return this.angelInvestorService.AngelsOnHand.Value;
		}
		return 0.0;
	}

	// Token: 0x060002F8 RID: 760 RVA: 0x00011570 File Offset: 0x0000F770
	private double CalculateTargetAmount(EventMissionType type, string venture, float difficulty_coef)
	{
		this.angelInvestorService.CalculateAngelInvestors(this.gameController.game.TotalCashEarned);
		double num = this.angelInvestorService.CalculateAngelInvestors(this.gameController.game.TotalPreviousCash.Value);
		double value = this.currentState.CashOnHand.Value;
		this.angelInvestorService.GetAdWatchAngelBonus();
		double num2 = 0.0;
		switch (type)
		{
		case EventMissionType.VENTURE_HAVE:
			num2 = this.CalculateMaxVentureHave(difficulty_coef, venture);
			break;
		case EventMissionType.VENTURE_EARN:
			num2 = this.CalculateMaxVentureEarn(difficulty_coef, venture);
			break;
		case EventMissionType.CASH_HAVE:
			num2 = this.CalculateCashHave(difficulty_coef);
			break;
		case EventMissionType.CASH_EARN:
			num2 = this.CalculateCashEarn(difficulty_coef);
			break;
		case EventMissionType.CASH_SPEND:
			num2 = this.CalculateCashSpend(difficulty_coef);
			break;
		case EventMissionType.CASH_UPGRADES_HAVE:
			num2 = this.CalculateCashUpgradesHave(difficulty_coef);
			break;
		case EventMissionType.ANGELS_HAVE:
			num2 = this.CalculateAngelHave(difficulty_coef);
			break;
		case EventMissionType.ANGELS_EARN:
			num2 = this.CalculateAngelEarn(difficulty_coef);
			break;
		case EventMissionType.ANGEL_UPGRADES_HAVE:
			num2 = this.CalculateAngelUpgradeHave(difficulty_coef);
			break;
		case EventMissionType.ACTIVATE_AD_MULTIPLIER:
		case EventMissionType.WATCH_ANY_AD:
			num2 = 1.0;
			break;
		case EventMissionType.EQUIP_ITEM:
			num2 = 1.0;
			break;
		}
		double num3 = num2;
		double num4 = Math.Floor(Math.Log10(num3) + 1.0);
		double num5 = num4 % 3.0;
		num4 -= 3.0 + num5;
		double num6 = Math.Pow(10.0, num4);
		return Math.Round(num3 / num6) * num6;
	}

	// Token: 0x060002F9 RID: 761 RVA: 0x000116E0 File Offset: 0x0000F8E0
	private int CalculateRewardAmount(EventMissionType type, float difficulty_coef)
	{
		EventModel eventModel = this.eventService.ActiveEvents.FirstOrDefault((EventModel x) => x.Id == this.currentState.planetName);
		float num = this.SelectDayBonusPointsMultiplier(this.dateTimeService.UtcNow, eventModel.StartDate);
		switch (type)
		{
		case EventMissionType.VENTURE_HAVE:
		case EventMissionType.VENTURE_EARN:
		case EventMissionType.CASH_HAVE:
		case EventMissionType.CASH_EARN:
		case EventMissionType.CASH_SPEND:
		case EventMissionType.CASH_UPGRADES_HAVE:
		case EventMissionType.ANGELS_HAVE:
		case EventMissionType.ANGELS_EARN:
		case EventMissionType.ANGEL_UPGRADES_HAVE:
			return (int)(Math.Round((double)(Math.Max(1f, difficulty_coef) * this.currentStateConstants.ScoreCoef * num * (1f + Random.RandomRange(-0.1f, 0.1f)) / 5f)) * 5.0);
		case EventMissionType.COMPLETE_MISSIONS:
			return (int)(Math.Round((double)((float)this.currentStateConstants.SuperTaskRewardPoints * num / 100f)) * 100.0);
		case EventMissionType.ACTIVATE_AD_MULTIPLIER:
		case EventMissionType.WATCH_ANY_AD:
		case EventMissionType.EQUIP_ITEM:
			return (int)(Math.Round((double)((float)this.currentStateConstants.SimpleTaskRewardPoints * num / 5f)) * 5.0);
		default:
			return 0;
		}
	}

	// Token: 0x060002FA RID: 762 RVA: 0x000117F8 File Offset: 0x0000F9F8
	private double CalcuateCashToSpend(float difficulty_coef)
	{
		double num = this.gameController.game.TotalCashEarned;
		float num2 = 0f;
		double angelAccumulationRate = this.currentState.angelAccumulationRate;
		double num3 = Math.Pow(10.0, (double)this.currentStateConstants.FirstMomentExponentThreshold);
		if (Math.Log10(num + 1.0) >= (double)this.currentStateConstants.ContentCompleteExponent)
		{
			num = Math.Pow(10.0, (double)this.currentStateConstants.ContentCompleteExponent);
		}
		else if (num < num3)
		{
			float num4 = (float)(Math.Log10(num + 1.0) / num3);
			num2 = this.currentStateConstants.FirstGenerationDifficultyCoef + this.currentStateConstants.DifficultyConversionMP * num4;
			num = num3;
		}
		double num5 = Math.Log10(num + 1.0);
		double num6 = num5 / Math.Log10(num3);
		double num7 = (double)difficulty_coef / num6;
		double y = num5 + num7 + (double)this.currentStateConstants.DifficultyCoversionBase + (double)num2;
		double num8 = Math.Pow(10.0, y);
		double num9 = this.TotalCashPerSecond();
		double num10 = Math.Pow((double)this.currentStateConstants.TaskTimerModifier, (double)(difficulty_coef / 2f + this.currentStateConstants.CashPerSecMultiplier)) * 60.0 * num9 / (double)this.currentStateConstants.CashGenerateAdjuster;
		if (num10 > num8)
		{
			num8 = num10;
		}
		if (!double.IsInfinity(num8))
		{
			return num8;
		}
		return GameState.MAX_CASH_DOUBLE;
	}

	// Token: 0x060002FB RID: 763 RVA: 0x00011960 File Offset: 0x0000FB60
	private double CalculateCashToSpendWithCashOnHand(double cashToSpend, float cashFactor)
	{
		double value = this.gameController.game.CashOnHand.Value;
		double result = cashToSpend * (double)cashFactor + value;
		if (value >= cashToSpend * (double)cashFactor)
		{
			result = cashToSpend * (double)cashFactor * (double)this.currentStateConstants.CashHaveAdjuster;
		}
		return result;
	}

	// Token: 0x060002FC RID: 764 RVA: 0x000119A4 File Offset: 0x0000FBA4
	private double CalculateCashHave(float difficulty_coef)
	{
		double cashToSpend = this.CalcuateCashToSpend(difficulty_coef);
		double num = this.CalculateCashToSpendWithCashOnHand(cashToSpend, 1f / this.currentStateConstants.CashHaveAdjuster);
		if (num >= GameState.MAX_CASH_DOUBLE)
		{
			return GameState.MAX_CASH_DOUBLE;
		}
		return num;
	}

	// Token: 0x060002FD RID: 765 RVA: 0x000119E4 File Offset: 0x0000FBE4
	private double CalculateCashEarn(float difficulty_coef)
	{
		double cashToSpend = this.CalcuateCashToSpend(difficulty_coef);
		double num = this.CalculateCashToSpendWithCashOnHand(cashToSpend, this.currentStateConstants.CashGenerateAdjuster);
		if (num >= GameState.MAX_CASH_DOUBLE)
		{
			return GameState.MAX_CASH_DOUBLE;
		}
		return num;
	}

	// Token: 0x060002FE RID: 766 RVA: 0x00011A1C File Offset: 0x0000FC1C
	private double CalculateCashSpend(float difficulty_coef)
	{
		double value = this.gameController.game.CashOnHand.Value;
		double num = this.CalcuateCashToSpend(difficulty_coef) * (double)this.currentStateConstants.CashGenerateAdjuster + value;
		if (num >= GameState.MAX_CASH_DOUBLE)
		{
			return GameState.MAX_CASH_DOUBLE;
		}
		return num;
	}

	// Token: 0x060002FF RID: 767 RVA: 0x00011A68 File Offset: 0x0000FC68
	private double CalculateCashUpgradesHave(float difficulty_coef)
	{
		this.CalcuateCashToSpend(difficulty_coef);
		List<Upgrade> allAffordableCashUpgrades = this.GetAllAffordableCashUpgrades(difficulty_coef);
		allAffordableCashUpgrades.Sum((Upgrade x) => x.cost);
		int num = allAffordableCashUpgrades.Count;
		num = this.upgradeService.Upgrades.Count((Upgrade x) => x.currency == Upgrade.Currency.InGameCash && x.IsPurchased.Value) + num;
		return (double)num;
	}

	// Token: 0x06000300 RID: 768 RVA: 0x00011AE4 File Offset: 0x0000FCE4
	private List<Upgrade> GetAllAffordableCashUpgrades(float difficulty_coef)
	{
		List<Upgrade> list = new List<Upgrade>();
		double num = this.CalcuateCashToSpend(difficulty_coef) * (double)this.currentStateConstants.CashHaveAdjuster;
		List<Upgrade> list2 = (from x in this.upgradeService.Upgrades
		where x.currency == Upgrade.Currency.InGameCash && !x.IsPurchased.Value
		select x).ToList<Upgrade>();
		double num2 = 0.0;
		int num3 = 0;
		while (num3 < list2.Count && num2 + list2[num3].cost <= num)
		{
			num2 += list2[num3].cost;
			list.Add(list2[num3]);
			num3++;
		}
		return list;
	}

	// Token: 0x06000301 RID: 769 RVA: 0x00011B94 File Offset: 0x0000FD94
	private double CalculateAngelHave(float difficulty_coef)
	{
		double totalCashEarned = this.gameController.game.TotalCashEarned;
		double num = this.CalcuateCashToSpend(difficulty_coef) * (double)this.currentStateConstants.CashGenerateAdjuster;
		double num2 = this.angelInvestorService.CalculateAngelInvestors(totalCashEarned + num);
		double value = this.angelInvestorService.AngelsOnHand.Value;
		return (num2 - value) * 1.2 + value;
	}

	// Token: 0x06000302 RID: 770 RVA: 0x00011BF4 File Offset: 0x0000FDF4
	private double CalculateAngelEarn(float difficulty_coef)
	{
		double num = this.CalculateAngelHave(difficulty_coef);
		double value = this.angelInvestorService.AngelsOnHand.Value;
		return num - value;
	}

	// Token: 0x06000303 RID: 771 RVA: 0x00011C1C File Offset: 0x0000FE1C
	private double CalculateAngelUpgradeHave(float difficulty_coef)
	{
		double num = this.CalculateAngelHave(difficulty_coef) * (double)this.currentStateConstants.AngelSpentThreshold;
		this.upgradeService.Upgrades.Count((Upgrade x) => x.currency == Upgrade.Currency.AngelInvestors && x.IsPurchased.Value);
		List<Upgrade> list = (from x in this.upgradeService.Upgrades
		where x.currency == Upgrade.Currency.AngelInvestors
		select x).ToList<Upgrade>();
		double num2 = 0.0;
		int num3 = 0;
		while (num3 < list.Count && num2 + list[num3].cost <= num)
		{
			num2 += list[num3].cost;
			num3++;
		}
		return (double)num3;
	}

	// Token: 0x06000304 RID: 772 RVA: 0x00011CE0 File Offset: 0x0000FEE0
	private double CalculateMaxCanAffordVenture(float difficulty_coef, string venture)
	{
		double cashToSpend = this.CalcuateCashToSpend(difficulty_coef);
		double cashAmount = this.CalculateCashToSpendWithCashOnHand(cashToSpend, 1f);
		return this.currentState.VentureModels.First((VentureModel x) => x.Id == venture).CalculateMaxCanAfford(cashAmount);
	}

	// Token: 0x06000305 RID: 773 RVA: 0x00011D34 File Offset: 0x0000FF34
	private double CalculateMaxVentureHave(float difficulty_coef, string venture)
	{
		double num = this.CalculateMaxCanAffordVenture(difficulty_coef, venture);
		double value = this.currentState.VentureModels.First((VentureModel x) => x.Id == venture).TotalOwned.Value;
		if (num <= 0.0)
		{
			Debug.LogError(string.Concat(new object[]
			{
				"Error with creating mission venture, {difficulty_coef:",
				difficulty_coef,
				", venture:",
				venture,
				", maxAfford:",
				num,
				",current=",
				value,
				"}"
			}));
			return value + 1.0;
		}
		return value + num;
	}

	// Token: 0x06000306 RID: 774 RVA: 0x00011DFC File Offset: 0x0000FFFC
	private double CalculateMaxVentureEarn(float difficulty_coef, string venture)
	{
		VentureModel ventureModel = this.gameController.game.VentureModels.First((VentureModel x) => x.Id == venture);
		double value = this.angelInvestorService.AngelsOnHand.Value;
		double num = this.CalculateAngelHave(difficulty_coef) - value;
		double value2 = ventureModel.TotalOwned.Value;
		double num2 = this.CalculateMaxCanAffordVenture(difficulty_coef, venture) + value2;
		double num3;
		if (num >= value && num >= 50.0)
		{
			num3 = Math.Ceiling(Math.Pow(num2, (double)this.currentStateConstants.PrestigeVentureRatio) + value2);
		}
		else
		{
			num3 = num2;
		}
		if (num3 <= 0.0)
		{
			return 1.0;
		}
		return num3;
	}

	// Token: 0x06000307 RID: 775 RVA: 0x00011EC4 File Offset: 0x000100C4
	private UserEventMission CreateAndMonitorUserMission(EventMissionSaveData saveData)
	{
		ReactiveProperty<double> timeRemaining = null;
		this.MissionTimersById.TryGetValue(saveData.ID, out timeRemaining);
		UserEventMission userMission = new UserEventMission(saveData, timeRemaining);
		if (userMission.State.Value == EventMissionState.ACTIVE)
		{
			userMission.State.TakeUntil((EventMissionState x) => x == EventMissionState.COMPLETE || x == EventMissionState.EXPIRED).Subscribe(delegate(EventMissionState x)
			{
				if (x == EventMissionState.COMPLETE)
				{
					this.analyticService.SendMissionEvent("MissionComplete", userMission, "");
					return;
				}
				if (x == EventMissionState.EXPIRED)
				{
					this.analyticService.SendMissionEvent("MissionExpired", userMission, userMission.CurrentCount.Value + "/" + userMission.TargetAmount);
				}
			}).AddTo(userMission.disposables);
		}
		IObservable<double> observable = this.MonitorEventMission(userMission);
		if (observable != null)
		{
			observable.Subscribe(delegate(double x)
			{
				userMission.CurrentCount.Value = Math.Min(x, userMission.TargetAmount);
			}).AddTo(userMission.disposables);
		}
		return userMission;
	}

	// Token: 0x06000308 RID: 776 RVA: 0x00011FA0 File Offset: 0x000101A0
	public double TotalCashPerSecond()
	{
		double num = this.currentState.TotalCashPerSecond();
		double previousCashPerSecondAtLastReset = this.PreviousCashPerSecondAtLastReset;
		if (num <= previousCashPerSecondAtLastReset)
		{
			return previousCashPerSecondAtLastReset;
		}
		return num;
	}

	// Token: 0x06000309 RID: 777 RVA: 0x00011FC8 File Offset: 0x000101C8
	private void Update(TimeSpan deltaSpan)
	{
		double totalSeconds = deltaSpan.TotalSeconds;
		using (Dictionary<int, ReactiveProperty<double>>.Enumerator enumerator = this.MissionTimersById.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, ReactiveProperty<double>> pair = enumerator.Current;
				if (this.currentMissions.FirstOrDefault((UserEventMission x) => x.ID == pair.Key).State.Value == EventMissionState.ACTIVE)
				{
					pair.Value.Value -= totalSeconds;
				}
			}
		}
	}

	// Token: 0x04000271 RID: 625
	public const string EVENT_MISSION_DATA_KEY = "EventMission_{0}";

	// Token: 0x04000272 RID: 626
	public const string STORE_ITEM_MISSIONS_REFRESH = "event_mission_refresh";

	// Token: 0x04000273 RID: 627
	private const int MAX_TASKS = 4;

	// Token: 0x04000278 RID: 632
	private readonly ReactiveProperty<int> NumberOfClaimedTasks = new ReactiveProperty<int>(0);

	// Token: 0x04000279 RID: 633
	private readonly ReactiveProperty<int> NumberOfOfferedTasks = new ReactiveProperty<int>(0);

	// Token: 0x0400027A RID: 634
	private List<string> EquipItemMissionsCompleted = new List<string>();

	// Token: 0x0400027B RID: 635
	private double PreviousCashPerSecondAtLastReset;

	// Token: 0x0400027C RID: 636
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x0400027D RID: 637
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x0400027E RID: 638
	private GameState currentState;

	// Token: 0x0400027F RID: 639
	private EventMissionContants currentStateConstants;

	// Token: 0x04000280 RID: 640
	private List<EventMissionDifficulty> difficultyLevels;

	// Token: 0x04000281 RID: 641
	private List<EventMissionDayBonus> dayBonuses;

	// Token: 0x04000282 RID: 642
	private List<EventMissionTaskTypeWeights> taskWeights;

	// Token: 0x04000283 RID: 643
	private IGameController gameController;

	// Token: 0x04000284 RID: 644
	private IDateTimeService dateTimeService;

	// Token: 0x04000285 RID: 645
	private IAngelInvestorService angelInvestorService;

	// Token: 0x04000286 RID: 646
	private IStoreService storeService;

	// Token: 0x04000287 RID: 647
	private IEventService eventService;

	// Token: 0x04000288 RID: 648
	private TimerService timerService;

	// Token: 0x04000289 RID: 649
	private AdWatchService adService;

	// Token: 0x0400028A RID: 650
	private UpgradeService upgradeService;

	// Token: 0x0400028B RID: 651
	private IAnalyticService analyticService;

	// Token: 0x0400028C RID: 652
	private ProfitBoostAdService profitBoostAdService;

	// Token: 0x0400028D RID: 653
	private Dictionary<EventMissionType, IEventMissionMonitor> missionMonitorsByType;
}
