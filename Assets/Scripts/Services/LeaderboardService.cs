using System;
using System.Collections.Generic;
using System.Linq;
using Platforms.Logger;
using UniRx;
using UnityEngine;

// Token: 0x020001D2 RID: 466
public class LeaderboardService : ILeaderboardService, IDisposable
{
	// Token: 0x06000DBC RID: 3516 RVA: 0x0003D78A File Offset: 0x0003B98A
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000DBD RID: 3517 RVA: 0x0003D798 File Offset: 0x0003B998
	public void Init(IGameController gameController, ILeaderboardPlatform leaderboardPlatform)
	{
		this.gameController = gameController;
		this.leaderboardPlatform = leaderboardPlatform;
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		string text = gameController.GlobalPlayerData.Get(LeaderboardService.EVENT_HISTORY_SAVE_KEY, "");
		new LeaderboardRankContainer();
		try
		{
			if (!string.IsNullOrEmpty(text))
			{
				foreach (LeaderboardRankData leaderboardRankData in JsonUtility.FromJson<LeaderboardRankContainer>(text).historicEventData)
				{
					this.logger.Debug(leaderboardRankData.ToString());
					leaderboardRankData.eventId = leaderboardRankData.eventId.Replace("_steam", "");
					this.HistoricEventData.Add(leaderboardRankData.eventId, leaderboardRankData);
				}
			}
		}
		catch
		{
			this.logger.Error("Failed to deserialize Historic Event Data");
		}
		(from x in gameController.State
		where x.IsEventPlanet
		select x).Take(1).Subscribe(delegate(GameState _)
		{
			this.GenerateLeaderboardToken();
		}).AddTo(this.disposables);
	}

	// Token: 0x06000DBE RID: 3518 RVA: 0x0003D8D4 File Offset: 0x0003BAD4
	public int GetLeaderBoardToken()
	{
		return this.leaderboardToken;
	}

	// Token: 0x06000DBF RID: 3519 RVA: 0x0003D8DC File Offset: 0x0003BADC
	public IObservable<LeaderboardRankData> GetPlayerRank(string leaderboardId, LeaderboardType leaderboardType, bool allowFromCache = true)
	{
		bool allowFromCache2 = allowFromCache;
		return Observable.Create<LeaderboardRankData>(delegate(IObserver<LeaderboardRankData> observer)
		{
			if (allowFromCache && this.HistoricEventData.ContainsKey(leaderboardId))
			{
				this.logger.Debug("Get Player Rank() FROM CACHE\n{0}", new object[]
				{
					this.HistoricEventData[leaderboardId].ToString()
				});
				observer.OnNext(this.HistoricEventData[leaderboardId]);
				observer.OnCompleted();
			}
			else
			{
				this.GetLeaderboardAroundPlayer(leaderboardId, leaderboardType, 100).Subscribe(delegate(List<LeaderboardItem> players)
				{
					if (this.HistoricEventData.ContainsKey(leaderboardId))
					{
						this.logger.Debug("Get Player Rank() FROM Server\n{0}", new object[]
						{
							this.HistoricEventData[leaderboardId].ToString()
						});
						observer.OnNext(this.HistoricEventData[leaderboardId]);
						observer.OnCompleted();
						return;
					}
					observer.OnError(new Exception("Unable to find rank for leaderboard " + leaderboardId));
				}, new Action<Exception>(observer.OnError));
			}
			return Disposable.Empty;
		});
	}

	// Token: 0x06000DC0 RID: 3520 RVA: 0x0003D916 File Offset: 0x0003BB16
	public IObservable<List<LeaderboardItem>> GetLeaderboardAroundPlayer(string leaderboardId, LeaderboardType leaderboardType, int maxCount = 100)
	{
		return Observable.Create<List<LeaderboardItem>>(delegate(IObserver<List<LeaderboardItem>> observer)
		{
			Action<List<LeaderboardItem>> callback = delegate(List<LeaderboardItem> players)
			{
				for (int i = 0; i < players.Count; i++)
				{
					if (players[i].me)
					{
						this.UpdateHistoricLeaderboardData(leaderboardId, players[i].position, players[i].val);
						break;
					}
				}
				observer.OnNext(players);
				observer.OnCompleted();
			};
			this.leaderboardPlatform.GetLeaderboardAroundPlayer(this.ConvertToPlatformLeaderboardId(leaderboardId), leaderboardType, callback, delegate(string error)
			{
				observer.OnError(new Exception(error));
			}, maxCount);
			return Disposable.Empty;
		});
	}

	// Token: 0x06000DC1 RID: 3521 RVA: 0x0003D949 File Offset: 0x0003BB49
	public IObservable<List<LeaderboardItem>> GetLeaderboardTop100(string leaderboardId, LeaderboardType leaderboardType)
	{
		return Observable.Create<List<LeaderboardItem>>(delegate(IObserver<List<LeaderboardItem>> observer)
		{
			this.leaderboardPlatform.GetLeaderboardTop100(this.ConvertToPlatformLeaderboardId(leaderboardId), leaderboardType, delegate(List<LeaderboardItem> players)
			{
				observer.OnNext(players);
				observer.OnCompleted();
			}, delegate(string error)
			{
				observer.OnError(new Exception(error));
			});
			return Disposable.Empty;
		});
	}

	// Token: 0x06000DC2 RID: 3522 RVA: 0x0003D978 File Offset: 0x0003BB78
	public IObservable<Tuple<string, int>> PostLeaderboardValue(string leaderboardId, LeaderboardType leaderboardType, int leaderboardSize, int value)
	{
		string requestedLeaderboardId = leaderboardId;
		return Observable.Create<Tuple<string, int>>(delegate(IObserver<Tuple<string, int>> observer)
		{
			this.leaderboardPlatform.PostLeaderboardValue(this.ConvertToPlatformLeaderboardId(leaderboardId), leaderboardType, leaderboardSize, value, delegate
			{
				observer.OnNext(new Tuple<string, int>(requestedLeaderboardId, value));
				observer.OnCompleted();
			}, delegate(string error)
			{
				observer.OnError(new Exception(error));
			});
			return Disposable.Empty;
		});
	}

	// Token: 0x06000DC3 RID: 3523 RVA: 0x0003D9CA File Offset: 0x0003BBCA
	private void GenerateLeaderboardToken()
	{
		this.leaderboardPlatform.GetLeaderboardToken(new Action<int>(this.OnLeaderboardTokenCreated), new Action<string>(this.OnLeaderboardUpdateError));
	}

	// Token: 0x06000DC4 RID: 3524 RVA: 0x0003D9EF File Offset: 0x0003BBEF
	private void OnLeaderboardTokenCreated(int token)
	{
		this.leaderboardToken = token;
	}

	// Token: 0x06000DC5 RID: 3525 RVA: 0x0003D9F8 File Offset: 0x0003BBF8
	private void OnLeaderboardUpdateError(string error)
	{
		Debug.Log("[LeaderboardService] " + error);
	}

	// Token: 0x06000DC6 RID: 3526 RVA: 0x0003DA0C File Offset: 0x0003BC0C
	private void UpdateHistoricLeaderboardData(string leaderboardId, int newRank, int newScore)
	{
		if (this.HistoricEventData.ContainsKey(leaderboardId))
		{
			this.HistoricEventData[leaderboardId].leaderboardRank = (double)newRank;
			this.HistoricEventData[leaderboardId].score = (double)newScore;
		}
		else
		{
			this.HistoricEventData.Add(leaderboardId, new LeaderboardRankData(leaderboardId, (double)newRank)
			{
				score = (double)newScore
			});
		}
		this.SaveHistoricLeaderboardData();
	}

	// Token: 0x06000DC7 RID: 3527 RVA: 0x0003DA74 File Offset: 0x0003BC74
	private void SaveHistoricLeaderboardData()
	{
		string value = JsonUtility.ToJson(new LeaderboardRankContainer
		{
			historicEventData = this.HistoricEventData.Values.ToList<LeaderboardRankData>()
		});
		this.gameController.GlobalPlayerData.Set(LeaderboardService.EVENT_HISTORY_SAVE_KEY, value);
	}

	// Token: 0x06000DC8 RID: 3528 RVA: 0x0003DAB9 File Offset: 0x0003BCB9
	private string ConvertToPlatformLeaderboardId(string leaderboardId)
	{
		if (!leaderboardId.EndsWith("_steam"))
		{
			leaderboardId = string.Format("{0}_steam", leaderboardId);
		}
		return leaderboardId;
	}

	// Token: 0x04000BCF RID: 3023
	public readonly ReactiveDictionary<string, LeaderboardRankData> HistoricEventData = new ReactiveDictionary<string, LeaderboardRankData>();

	// Token: 0x04000BD0 RID: 3024
	public static readonly string EVENT_HISTORY_SAVE_KEY = "eventHistory";

	// Token: 0x04000BD1 RID: 3025
	private Platforms.Logger.Logger logger;

	// Token: 0x04000BD2 RID: 3026
	private int leaderboardToken;

	// Token: 0x04000BD3 RID: 3027
	private IGameController gameController;

	// Token: 0x04000BD4 RID: 3028
	private ILeaderboardPlatform leaderboardPlatform;

	// Token: 0x04000BD5 RID: 3029
	private CompositeDisposable disposables = new CompositeDisposable();
}
