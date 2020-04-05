using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

// Token: 0x020000C5 RID: 197
public class PlanetMilestoneService : IDisposable
{
	// Token: 0x0600055F RID: 1375 RVA: 0x0001BF24 File Offset: 0x0001A124
	public void Init(IGameController gameController, IEventService eventService, IAnalyticService analyticService, IGrantRewardService grantRewardService)
	{
		this.gameController = gameController;
		this.eventService = eventService;
		this.analyticService = analyticService;
		this.grantRewardService = grantRewardService;
		this.LoadUnclaimedMilestones();
		this.gameController.OnLoadNewPlanetPre += this.OnPreStateChange;
		this.gameController.State.Subscribe(new Action<GameState>(this.OnGameStatechanged)).AddTo(this.disposables);
		this.eventService.PastEvents.ObserveCountChanged(false).Subscribe(delegate(int x)
		{
			this.ReevaluateUnclaimedMilestonesFromPastEvents();
		}).AddTo(this.disposables);
		this.ReevaluateUnclaimedMilestonesFromPastEvents();
	}

	// Token: 0x06000560 RID: 1376 RVA: 0x0001BFC7 File Offset: 0x0001A1C7
	public void Dispose()
	{
		this.gameController.OnLoadNewPlanetPre -= this.OnPreStateChange;
		this.disposables.Dispose();
		this.stateDisposables.Dispose();
	}

	// Token: 0x06000561 RID: 1377 RVA: 0x0001BFF6 File Offset: 0x0001A1F6
	public List<UserPlanetMilestone> GetUserMilestonesForCurrentPlanet()
	{
		return this.UserPlanetMilestones;
	}

	// Token: 0x06000562 RID: 1378 RVA: 0x0001BFFE File Offset: 0x0001A1FE
	public IObservable<List<PlanetMilestoneRewardData>> ClaimMilestoneRewardsForCurrentPlanet(string milestoneId)
	{
		Func<PlanetMilestone, bool> <>9__1;
		Func<UserPlanetMilestone, bool> <>9__2;
		Func<UnclaimedMilestone, bool> <>9__4;
		return Observable.Create<List<PlanetMilestoneRewardData>>(delegate(IObserver<List<PlanetMilestoneRewardData>> observer)
		{
			IEnumerable<PlanetMilestone> planetMilestones = this.currentState.PlanetMilestones;
			Func<PlanetMilestone, bool> predicate;
			if ((predicate = <>9__1) == null)
			{
				predicate = (<>9__1 = ((PlanetMilestone x) => x.Id == milestoneId));
			}
			PlanetMilestone planetMilestone = planetMilestones.FirstOrDefault(predicate);
			IEnumerable<UserPlanetMilestone> userPlanetMilestones = this.UserPlanetMilestones;
			Func<UserPlanetMilestone, bool> predicate2;
			if ((predicate2 = <>9__2) == null)
			{
				predicate2 = (<>9__2 = ((UserPlanetMilestone x) => x.MilestoneId == milestoneId));
			}
			UserPlanetMilestone userPlanetMilestone = userPlanetMilestones.FirstOrDefault(predicate2);
			if (planetMilestone == null || userPlanetMilestone == null)
			{
				observer.OnError(new Exception("Invalid " + ((planetMilestone == null) ? "Milestone Id" : "User Milestone Id ") + milestoneId));
			}
			else if (userPlanetMilestone.IsClaimed.Value)
			{
				observer.OnError(new Exception("Milestone Rewards for " + milestoneId + " are already claimed"));
			}
			else if (userPlanetMilestone.CurrentCount.Value < planetMilestone.TargetAmount)
			{
				observer.OnError(new Exception("Milestone Rewards for " + milestoneId + " cant be claimed yet"));
			}
			else
			{
				if (this.unclaimedMilestones.ContainsKey(this.currentState.planetName))
				{
					IEnumerable<UnclaimedMilestone> source = this.unclaimedMilestones[this.currentState.planetName];
					Func<UnclaimedMilestone, bool> predicate3;
					if ((predicate3 = <>9__4) == null)
					{
						predicate3 = (<>9__4 = ((UnclaimedMilestone x) => x.MilestoneId == milestoneId));
					}
					UnclaimedMilestone unclaimedMilestone = source.FirstOrDefault(predicate3);
					if (unclaimedMilestone != null)
					{
						this.unclaimedMilestones[this.currentState.planetName].Remove(unclaimedMilestone);
					}
				}
				List<PlanetMilestoneRewardData> list = planetMilestone.Rewards.ToList<PlanetMilestoneRewardData>();
				List<RewardData> converted = new List<RewardData>();
				list.ForEach(delegate(PlanetMilestoneRewardData x)
				{
					converted.Add(x);
				});
				this.grantRewardService.GrantRewards(converted, this.currentState.planetName + ":Milestone", milestoneId, false);
				userPlanetMilestone.IsClaimed.Value = true;
				this.analyticService.SendMilestoneEvent("MilestoneClaimed", userPlanetMilestone, "");
				this.ReevaluateCurrentMilestone();
				observer.OnNext(list);
				observer.OnCompleted();
			}
			return Disposable.Empty;
		});
	}

	// Token: 0x06000563 RID: 1379 RVA: 0x0001C024 File Offset: 0x0001A224
	public bool HasUnclaimedMilestonesFromEvent(string eventId)
	{
		return (from x in this.UnclaimedMilestonesFromPastEvents
		where x.planetId == eventId
		select x).ToList<UnclaimedMilestone>().Count > 0;
	}

	// Token: 0x06000564 RID: 1380 RVA: 0x0001C062 File Offset: 0x0001A262
	public IObservable<List<RewardData>> ClaimedMilestoneRewardsForPreviousEvent(string eventId)
	{
		Func<UnclaimedMilestone, bool> <>9__1;
		return Observable.Create<List<RewardData>>(delegate(IObserver<List<RewardData>> observer)
		{
			IEnumerable<UnclaimedMilestone> unclaimedMilestonesFromPastEvents = this.UnclaimedMilestonesFromPastEvents;
			Func<UnclaimedMilestone, bool> predicate;
			if ((predicate = <>9__1) == null)
			{
				predicate = (<>9__1 = ((UnclaimedMilestone x) => x.planetId == eventId));
			}
			List<UnclaimedMilestone> list = unclaimedMilestonesFromPastEvents.Where(predicate).ToList<UnclaimedMilestone>();
			if (list.Count > 0)
			{
				List<RewardData> list2 = new List<RewardData>();
				for (int i = 0; i < list.Count; i++)
				{
					UnclaimedMilestone unclaimedMilestone = list[i];
					RewardData reward = unclaimedMilestone.reward;
					list2.Add(reward);
					this.grantRewardService.GrantReward(reward, unclaimedMilestone.planetId + ":Milestone", unclaimedMilestone.MilestoneId, false);
					this.UnclaimedMilestonesFromPastEvents.Remove(unclaimedMilestone);
				}
				list2 = this.ConsolidateRewardData(list2);
				observer.OnNext(list2);
				observer.OnCompleted();
			}
			else
			{
				observer.OnError(new Exception("There are past milestones to be claimed for event " + eventId));
			}
			return Disposable.Empty;
		});
	}

	// Token: 0x06000565 RID: 1381 RVA: 0x0001C088 File Offset: 0x0001A288
	public List<RewardData> ConsolidateRewardData(List<RewardData> rewards)
	{
		Dictionary<string, RewardData> dictionary = new Dictionary<string, RewardData>();
		for (int i = 0; i < rewards.Count; i++)
		{
			RewardData rewardData = rewards[i];
			if (!dictionary.ContainsKey(rewardData.Id))
			{
				dictionary.Add(rewardData.Id, new RewardData
				{
					Id = rewardData.Id,
					Qty = rewardData.Qty,
					RewardType = rewardData.RewardType
				});
			}
			else
			{
				dictionary[rewardData.Id].Qty += rewardData.Qty;
			}
		}
		return dictionary.Values.ToList<RewardData>();
	}

	// Token: 0x06000566 RID: 1382 RVA: 0x0001C124 File Offset: 0x0001A324
	private static List<Dictionary<string, string>> Base64ToDictionary(object rawData)
	{
		byte[] bytes;
		try
		{
			bytes = Convert.FromBase64String((string)rawData);
		}
		catch (Exception)
		{
			Debug.LogErrorFormat("[ Hippo ] Unable to decode base64 string for ", Array.Empty<object>());
			throw;
		}
		List<Dictionary<string, string>> result;
		try
		{
			result = CSVReader.Read(bytes);
		}
		catch (Exception)
		{
			Debug.LogErrorFormat("[ Hippo ] Unable to process csv string for ", Array.Empty<object>());
			throw;
		}
		return result;
	}

	// Token: 0x06000567 RID: 1383 RVA: 0x0001C18C File Offset: 0x0001A38C
	private void OnPreStateChange()
	{
		this.CurrentMilestone.Value = null;
		this.DoesCurrentPlanetHaveMilestones.Value = false;
		this.stateDisposables.Clear();
		this.CurrentScore.Value = 0.0;
	}

	// Token: 0x06000568 RID: 1384 RVA: 0x0001C1C8 File Offset: 0x0001A3C8
	private void OnGameStatechanged(GameState gameState)
	{
		this.UserPlanetMilestones.Clear();
		this.currentState = gameState;
		double progressStart = 0.0;
		for (int i = 0; i < this.currentState.PlanetMilestones.Count; i++)
		{
			PlanetMilestone milestone = this.currentState.PlanetMilestones[i];
			PlanetMilestoneSaveData planetMilestoneSaveData = gameState.MilestonesSaveDatas.FirstOrDefault((PlanetMilestoneSaveData x) => x.MilestoneId == milestone.Id);
			if (planetMilestoneSaveData == null)
			{
				double currentCount = 0.0;
				planetMilestoneSaveData = new PlanetMilestoneSaveData
				{
					MilestoneId = this.currentState.PlanetMilestones[i].Id,
					CurrentCount = currentCount,
					IsClaimed = false
				};
			}
			UserPlanetMilestone item = new UserPlanetMilestone(milestone, planetMilestoneSaveData, progressStart, i);
			this.UserPlanetMilestones.Add(item);
			progressStart = milestone.TargetAmount;
		}
		this.CurrentScore.Value = gameState.MilestoneScore;
		this.ReevaluateCurrentMilestone();
		this.DoesCurrentPlanetHaveMilestones.Value = (this.currentState != null && this.currentState.PlanetMilestones.Count > 0);
		if (this.DoesCurrentPlanetHaveMilestones.Value)
		{
			if (this.currentState.progressionType == PlanetProgressionType.Angels)
			{
				MessageBroker.Default.Receive<AngelsClaimedEvent>().Subscribe(new Action<AngelsClaimedEvent>(this.OnAngelsClaimed)).AddTo(this.stateDisposables);
				return;
			}
			if (this.currentState.progressionType == PlanetProgressionType.Missions)
			{
				MessageBroker.Default.Receive<EventMissionPointsEarnedEvent>().Subscribe(new Action<EventMissionPointsEarnedEvent>(this.OnPointsClaimed)).AddTo(this.stateDisposables);
				return;
			}
			Debug.LogError("[PlanetMilestoneService] Planet " + this.currentState.planetName + " progressionType is not handled");
		}
	}

	// Token: 0x06000569 RID: 1385 RVA: 0x0001C38C File Offset: 0x0001A58C
	private void ReevaluateCurrentMilestone()
	{
		UserPlanetMilestone userPlanetMilestone = null;
		for (int i = 0; i < this.UserPlanetMilestones.Count; i++)
		{
			UserPlanetMilestone userPlanetMilestone2 = this.UserPlanetMilestones[i];
			if (userPlanetMilestone2.State.Value == UserPlanetMilestone.PlanetMilestoneState.ACTIVE || userPlanetMilestone2.State.Value == UserPlanetMilestone.PlanetMilestoneState.COMPLETE)
			{
				userPlanetMilestone = userPlanetMilestone2;
				break;
			}
		}
		float num = 0f;
		for (int j = 0; j < this.UserPlanetMilestones.Count; j++)
		{
			UserPlanetMilestone userPlanetMilestone3 = this.UserPlanetMilestones[j];
			float num2 = (float)(userPlanetMilestone3.CurrentCount.Value / userPlanetMilestone3.TargetAmount);
			num += num2;
			if (userPlanetMilestone3.State.Value == UserPlanetMilestone.PlanetMilestoneState.ACTIVE)
			{
				break;
			}
		}
		num /= (float)this.UserPlanetMilestones.Count;
		this.CurrentNormalizedProgress.Value = num;
		if (this.CurrentMilestone.Value != userPlanetMilestone)
		{
			this.CurrentMilestone.SetValueAndForceNotify(userPlanetMilestone);
		}
	}

	// Token: 0x0600056A RID: 1386 RVA: 0x0001C470 File Offset: 0x0001A670
	private void ReevaluateUnclaimedMilestonesFromPastEvents()
	{
		using (Dictionary<string, List<UnclaimedMilestone>>.Enumerator enumerator = this.unclaimedMilestones.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, List<UnclaimedMilestone>> unclaimedpair = enumerator.Current;
				EventModel eventModel = this.eventService.PastEvents.FirstOrDefault((EventModel x) => x.Id == unclaimedpair.Key);
				if (eventModel != null)
				{
					List<UnclaimedMilestone> value = unclaimedpair.Value;
					for (int i = 0; i < value.Count; i++)
					{
						if (!this.UnclaimedMilestonesFromPastEvents.Contains(value[i]))
						{
							this.UnclaimedMilestonesFromPastEvents.Add(value[i]);
						}
					}
				}
			}
		}
	}

	// Token: 0x0600056B RID: 1387 RVA: 0x0001C534 File Offset: 0x0001A734
	private void OnAngelsClaimed(AngelsClaimedEvent evt)
	{
		List<UserPlanetMilestone> list = new List<UserPlanetMilestone>();
		this.CurrentScore.Value += evt.AngelAmount;
		for (int i = 0; i < this.UserPlanetMilestones.Count; i++)
		{
			UserPlanetMilestone userPlanetMilestone = this.UserPlanetMilestones[i];
			double num = Math.Min(this.CurrentScore.Value, userPlanetMilestone.TargetAmount);
			if (num != userPlanetMilestone.CurrentCount.Value)
			{
				list.Add(userPlanetMilestone);
				if (num >= userPlanetMilestone.TargetAmount)
				{
					this.gameController.GlobalPlayerData.Add(string.Format("{0}_{1}", this.currentState.planetName, "milestones"), 1.0);
					if (!this.unclaimedMilestones.ContainsKey(this.currentState.planetName))
					{
						this.unclaimedMilestones.Add(this.currentState.planetName, new List<UnclaimedMilestone>());
					}
					UnclaimedMilestone item = new UnclaimedMilestone
					{
						planetId = this.currentState.planetName,
						MilestoneId = userPlanetMilestone.MilestoneId,
						reward = userPlanetMilestone.Reward
					};
					this.unclaimedMilestones[this.currentState.planetName].Add(item);
					this.SaveUnclaimedMilestones();
				}
				userPlanetMilestone.CurrentCount.Value = num;
			}
		}
		this.ReevaluateCurrentMilestone();
	}

	// Token: 0x0600056C RID: 1388 RVA: 0x0001C690 File Offset: 0x0001A890
	private void OnPointsClaimed(EventMissionPointsEarnedEvent evt)
	{
		List<UserPlanetMilestone> list = new List<UserPlanetMilestone>();
		this.CurrentScore.Value = evt.Total;
		for (int i = 0; i < this.UserPlanetMilestones.Count; i++)
		{
			UserPlanetMilestone userPlanetMilestone = this.UserPlanetMilestones[i];
			double num = Math.Min(this.CurrentScore.Value, userPlanetMilestone.TargetAmount);
			if (num != userPlanetMilestone.CurrentCount.Value)
			{
				list.Add(userPlanetMilestone);
				if (num >= userPlanetMilestone.TargetAmount)
				{
					this.gameController.GlobalPlayerData.Add(string.Format("{0}_{1}", this.currentState.planetName, "milestones"), 1.0);
					if (!this.unclaimedMilestones.ContainsKey(this.currentState.planetName))
					{
						this.unclaimedMilestones.Add(this.currentState.planetName, new List<UnclaimedMilestone>());
					}
					UnclaimedMilestone item = new UnclaimedMilestone
					{
						planetId = this.currentState.planetName,
						MilestoneId = userPlanetMilestone.MilestoneId,
						reward = userPlanetMilestone.Reward
					};
					this.unclaimedMilestones[this.currentState.planetName].Add(item);
					this.SaveUnclaimedMilestones();
					this.analyticService.SendMilestoneEvent("MilestoneComplete", userPlanetMilestone, "");
				}
				userPlanetMilestone.CurrentCount.Value = num;
			}
		}
		this.ReevaluateCurrentMilestone();
	}

	// Token: 0x0600056D RID: 1389 RVA: 0x0001C7FC File Offset: 0x0001A9FC
	private void LoadUnclaimedMilestones()
	{
		string text = this.gameController.GlobalPlayerData.Get("UNCLAIMED_MILESTONES_KEY", "");
		if (!string.IsNullOrEmpty(text))
		{
			foreach (UnclaimedMilestone unclaimedMilestone in JsonUtility.FromJson<UnclaimedMilestonesSaveData>(text).UnclaimedMilestones)
			{
				if (!this.unclaimedMilestones.ContainsKey(unclaimedMilestone.planetId))
				{
					this.unclaimedMilestones.Add(unclaimedMilestone.planetId, new List<UnclaimedMilestone>());
				}
				this.unclaimedMilestones[unclaimedMilestone.planetId].Add(unclaimedMilestone);
			}
		}
	}

	// Token: 0x0600056E RID: 1390 RVA: 0x0001C8B0 File Offset: 0x0001AAB0
	private void SaveUnclaimedMilestones()
	{
		UnclaimedMilestonesSaveData unclaimedMilestonesSaveData = new UnclaimedMilestonesSaveData();
		foreach (KeyValuePair<string, List<UnclaimedMilestone>> keyValuePair in this.unclaimedMilestones)
		{
			foreach (UnclaimedMilestone item in keyValuePair.Value)
			{
				unclaimedMilestonesSaveData.UnclaimedMilestones.Add(item);
			}
		}
		string value = JsonUtility.ToJson(unclaimedMilestonesSaveData);
		this.gameController.GlobalPlayerData.Set("UNCLAIMED_MILESTONES_KEY", value);
	}

	// Token: 0x040004C9 RID: 1225
	public const string UNCLAIMED_MILESTONES_KEY = "UNCLAIMED_MILESTONES_KEY";

	// Token: 0x040004CA RID: 1226
	public readonly ReactiveProperty<bool> DoesCurrentPlanetHaveMilestones = new ReactiveProperty<bool>(false);

	// Token: 0x040004CB RID: 1227
	public readonly ReactiveProperty<UserPlanetMilestone> CurrentMilestone = new ReactiveProperty<UserPlanetMilestone>();

	// Token: 0x040004CC RID: 1228
	public readonly ReactiveProperty<float> CurrentNormalizedProgress = new ReactiveProperty<float>(0f);

	// Token: 0x040004CD RID: 1229
	public readonly ReactiveProperty<double> CurrentScore = new ReactiveProperty<double>(0.0);

	// Token: 0x040004CE RID: 1230
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040004CF RID: 1231
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x040004D0 RID: 1232
	private GameState currentState;

	// Token: 0x040004D1 RID: 1233
	private List<UserPlanetMilestone> UserPlanetMilestones = new List<UserPlanetMilestone>();

	// Token: 0x040004D2 RID: 1234
	private Dictionary<string, List<UnclaimedMilestone>> unclaimedMilestones = new Dictionary<string, List<UnclaimedMilestone>>();

	// Token: 0x040004D3 RID: 1235
	private List<UnclaimedMilestone> UnclaimedMilestonesFromPastEvents = new List<UnclaimedMilestone>();

	// Token: 0x040004D4 RID: 1236
	private IGameController gameController;

	// Token: 0x040004D5 RID: 1237
	private IEventService eventService;

	// Token: 0x040004D6 RID: 1238
	private IAnalyticService analyticService;

	// Token: 0x040004D7 RID: 1239
	private IGrantRewardService grantRewardService;
}
