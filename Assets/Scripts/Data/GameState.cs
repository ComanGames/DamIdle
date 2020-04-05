using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UniRx;
using UnityEngine;

// Token: 0x02000153 RID: 339
public class GameState
{
	// Token: 0x170000CA RID: 202
	// (get) Token: 0x06000A88 RID: 2696 RVA: 0x0002E94E File Offset: 0x0002CB4E
	// (set) Token: 0x06000A89 RID: 2697 RVA: 0x0002E956 File Offset: 0x0002CB56
	public bool IsDisposed { get; private set; }

	// Token: 0x170000CB RID: 203
	// (get) Token: 0x06000A8A RID: 2698 RVA: 0x0002E95F File Offset: 0x0002CB5F
	// (set) Token: 0x06000A8B RID: 2699 RVA: 0x0002E967 File Offset: 0x0002CB67
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public FloatReactiveProperty MultiplierBonus { get; private set; }

	// Token: 0x170000CC RID: 204
	// (get) Token: 0x06000A8C RID: 2700 RVA: 0x0002E970 File Offset: 0x0002CB70
	// (set) Token: 0x06000A8D RID: 2701 RVA: 0x0002E978 File Offset: 0x0002CB78
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public ReadOnlyReactiveProperty<float> TotalMultiplier { get; set; }

	// Token: 0x170000CD RID: 205
	// (get) Token: 0x06000A8E RID: 2702 RVA: 0x0002E981 File Offset: 0x0002CB81
	// (set) Token: 0x06000A8F RID: 2703 RVA: 0x0002E989 File Offset: 0x0002CB89
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public ReadOnlyReactiveProperty<float> SubscriptionMultiplier { get; set; }

	// Token: 0x170000CE RID: 206
	// (get) Token: 0x06000A90 RID: 2704 RVA: 0x0002E992 File Offset: 0x0002CB92
	// (set) Token: 0x06000A91 RID: 2705 RVA: 0x0002E99A File Offset: 0x0002CB9A
	public FloatReactiveProperty CoolDownBonus { get; private set; }

	// Token: 0x170000CF RID: 207
	// (get) Token: 0x06000A92 RID: 2706 RVA: 0x0002E9A3 File Offset: 0x0002CBA3
	// (set) Token: 0x06000A93 RID: 2707 RVA: 0x0002E9BE File Offset: 0x0002CBBE
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public float adRewardMultiplier
	{
		get
		{
			if (this._adRewardMultiplier <= 0f)
			{
				return 1f;
			}
			return this._adRewardMultiplier;
		}
		set
		{
			this._adRewardMultiplier = value;
		}
	}

	// Token: 0x170000D0 RID: 208
	// (get) Token: 0x06000A94 RID: 2708 RVA: 0x0002E9C7 File Offset: 0x0002CBC7
	// (set) Token: 0x06000A95 RID: 2709 RVA: 0x0002E9E2 File Offset: 0x0002CBE2
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public float facebookMultiplier
	{
		get
		{
			if (this._facebookMultiplier <= 0f)
			{
				return 1f;
			}
			return this._facebookMultiplier;
		}
		set
		{
			this._facebookMultiplier = value;
		}
	}

	// Token: 0x170000D1 RID: 209
	// (get) Token: 0x06000A96 RID: 2710 RVA: 0x0002E9EB File Offset: 0x0002CBEB
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public float purchasedMultiplier
	{
		get
		{
			return this._purchasedMultiplier;
		}
	}

	// Token: 0x170000D2 RID: 210
	// (get) Token: 0x06000A97 RID: 2711 RVA: 0x0002E9F3 File Offset: 0x0002CBF3
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public double TotalCashEarned
	{
		get
		{
			if (this.SessionCash.Value + this.TotalPreviousCash.Value <= GameState.MAX_CASH_DOUBLE)
			{
				return this.SessionCash.Value + this.TotalPreviousCash.Value;
			}
			return GameState.MAX_CASH_DOUBLE;
		}
	}

	// Token: 0x170000D3 RID: 211
	// (get) Token: 0x06000A98 RID: 2712 RVA: 0x0002EA30 File Offset: 0x0002CC30
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public bool IsEventPlanet
	{
		get
		{
			return GameState.IsEvent(this.planetName);
		}
	}

	// Token: 0x06000A99 RID: 2713 RVA: 0x0002EA3D File Offset: 0x0002CC3D
	public static bool IsEvent(string planetName)
	{
		return !new string[]
		{
			"Earth",
			"Moon",
			"Mars"
		}.Contains(planetName);
	}

	// Token: 0x14000060 RID: 96
	// (add) Token: 0x06000A9A RID: 2714 RVA: 0x0002EA68 File Offset: 0x0002CC68
	// (remove) Token: 0x06000A9B RID: 2715 RVA: 0x0002EAA0 File Offset: 0x0002CCA0
    public event Action<VentureModel> OnVentureBoosted;

	// Token: 0x06000A9C RID: 2716 RVA: 0x0002EAD8 File Offset: 0x0002CCD8
	public GameState()
	{
		this.MultiplierBonus = new FloatReactiveProperty();
		this.CoolDownBonus = new FloatReactiveProperty(0f);
		this.SubscriptionMultiplier = GameController.Instance.SubscriptionService.IsActive.CombineLatest(this.planetTheme, delegate(bool subscription, string theme)
		{
			if (!subscription || !(theme == "Earth"))
			{
				return 0f;
			}
			return 9f;
		}).ToReadOnlyReactiveProperty<float>();
		this.TotalMultiplier = this.PurchasedMultiplier.CombineLatest(this.MultiplierBonus, this.SubscriptionMultiplier, (float purchaseMult, float multiplier, float sub) => purchaseMult + multiplier + sub).Select(delegate(float v)
		{
			if (v != 0f)
			{
				return v;
			}
			return 1f;
		}).ToReadOnlyReactiveProperty<float>();
		this.fluxCapitalorMultiplier = (from v in this.fluxCapitalorQuantity
		select (float)v * this.fluxCapitalorEffectiveness).ToReadOnlyReactiveProperty<float>();
		this.IsDisposed = false;
	}

	// Token: 0x06000A9D RID: 2717 RVA: 0x0002EE08 File Offset: 0x0002D008
	public void Dispose()
	{
		foreach (VentureModel ventureModel in this.VentureModels)
		{
			ventureModel.Dispose();
		}
		this.VentureModels.Clear();
		this._Disposables.Dispose();
		this.IsDisposed = true;
	}

	// Token: 0x06000A9E RID: 2718 RVA: 0x0002EE70 File Offset: 0x0002D070
	public void Finished(double profit)
	{
		double num = this.CashOnHand.Value + profit;
		this.CashOnHand.Value = ((num > GameState.MAX_CASH_DOUBLE) ? GameState.MAX_CASH_DOUBLE : num);
		double num2 = this.SessionCash.Value + profit;
		this.SessionCash.Value = ((num2 > GameState.MAX_CASH_DOUBLE) ? GameState.MAX_CASH_DOUBLE : num2);
	}

	// Token: 0x06000A9F RID: 2719 RVA: 0x0002EED0 File Offset: 0x0002D0D0
	public double AddCash(double addAmount, bool ignoreMultiplier = false)
	{
		if (!ignoreMultiplier)
		{
			addAmount *= (double)this.TotalMultiplier.Value;
		}
		double num = this.CashOnHand.Value + addAmount;
		this.CashOnHand.Value = ((num > GameState.MAX_CASH_DOUBLE) ? GameState.MAX_CASH_DOUBLE : num);
		double num2 = this.SessionCash.Value + addAmount;
		this.SessionCash.Value = ((num2 > GameState.MAX_CASH_DOUBLE) ? GameState.MAX_CASH_DOUBLE : num2);
		return addAmount;
	}

	// Token: 0x06000AA0 RID: 2720 RVA: 0x0002EF44 File Offset: 0x0002D144
	public double TotalCashPerSecond()
	{
		return (from venture in this.VentureModels
		where venture.IsManaged.Value
		select venture).Sum((VentureModel venture) => venture.CashPerSec.Value);
	}

	// Token: 0x06000AA1 RID: 2721 RVA: 0x0002EFA0 File Offset: 0x0002D1A0
	public void Purchase(VentureModel venture)
	{
		double value = venture.CostForNext.Value;
		this.CashOnHand.Value -= value;
		venture.NumOwned_Base.Value += venture.CanAfford.Value;
		this.OnVenturePurchased.OnNext(venture);
	}

	// Token: 0x06000AA2 RID: 2722 RVA: 0x0002EFF8 File Offset: 0x0002D1F8
	public void BoostVenture(VentureModel model)
	{
		if (PlayerData.GetPlayerData("Global").Spend("MegaTickets", 1.0))
		{
			model.IsBoosted.Value = true;
			GameController.Instance.AnalyticService.SendVentureBoostedAnalytic(model);
			MessageBroker.Default.Publish<MegaTicketCountChanged>(new MegaTicketCountChanged());
			GameController.Instance.GildingService.ShowingGild.Value = false;
		}
	}

	// Token: 0x06000AA3 RID: 2723 RVA: 0x0002F064 File Offset: 0x0002D264
	public void IncreaseGildLevel(VentureModel model, int levelAmount = 1)
	{
		if (this.IsEventPlanet)
		{
			IntReactiveProperty gildLevel = model.gildLevel;
			int value = gildLevel.Value;
			gildLevel.Value = value + 1;
			this.OnVentureBoosted(model);
			return;
		}
		Debug.LogError("Attempting to increase the Gild level for " + model.Id + " gild levels are only for event guilding");
	}

	// Token: 0x06000AA4 RID: 2724 RVA: 0x0002F0B5 File Offset: 0x0002D2B5
	public void SetMultiplier(float multiplier)
	{
		multiplier = Math.Max(0f, multiplier);
		this.PurchasedMultiplier.Value = multiplier;
	}

	// Token: 0x06000AA5 RID: 2725 RVA: 0x0002F0D0 File Offset: 0x0002D2D0
	public void Reset(bool isAdWatch = false)
	{
		this.ResetGameStateVentures();
		using (List<Venture>.Enumerator enumerator = this.ventures.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Venture newVenture = enumerator.Current;
				this.VentureModels.FirstOrDefault((VentureModel v) => v.Id == newVenture.id).Reset(newVenture.baseAmount, newVenture.costPer, newVenture.profitPer, (float)newVenture.cooldownTime);
			}
		}
		GameController.Instance.UpgradeService.Reset();
		foreach (Unlock unlock in GameController.Instance.UnlockService.Unlocks)
		{
			if (!unlock.permanent)
			{
				unlock.Earned.Value = false;
				unlock.Claimed.Value = false;
			}
		}
		this.holidayVentureGifts.Clear();
		GameController.Instance.UnlockService.SetAllNextUnlocks();
		this.CashOnHand.Value = 0.0;
		GameController.Instance.AngelService.Reset(isAdWatch);
		double value = (this.TotalPreviousCash.Value + this.SessionCash.Value > GameState.MAX_CASH_DOUBLE) ? GameState.MAX_CASH_DOUBLE : (this.TotalPreviousCash.Value + this.SessionCash.Value);
		this.TotalPreviousCash.Value = value;
		this.SessionCash.Value = 0.0;
	}

	// Token: 0x06000AA6 RID: 2726 RVA: 0x0002F288 File Offset: 0x0002D488
	public void HardReset()
	{
		this.ResetGameStateVentures();
		using (List<Venture>.Enumerator enumerator = this.ventures.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Venture newVenture = enumerator.Current;
				this.VentureModels.FirstOrDefault((VentureModel v) => v.Id == newVenture.id).Reset(newVenture.baseAmount, newVenture.costPer, newVenture.profitPer, (float)newVenture.cooldownTime);
			}
		}
		GameController.Instance.UpgradeService.Reset();
		foreach (Unlock unlock in GameController.Instance.UnlockService.Unlocks)
		{
			if (!unlock.permanent)
			{
				unlock.Earned.Value = false;
				unlock.Claimed.Value = false;
				unlock.EverClaimed.Value = false;
			}
		}
		this.holidayVentureGifts.Clear();
		GameController.Instance.UnlockService.SetAllNextUnlocks();
		GameController.Instance.AngelService.HardReset();
		this.CashOnHand.Value = 0.0;
		this.TotalPreviousCash.Value = 0.0;
		this.SessionCash.Value = 0.0;
	}

	// Token: 0x06000AA7 RID: 2727 RVA: 0x0002F414 File Offset: 0x0002D614
	public static GameState Create(string planetName, GameState_Serialized serialized, PlanetData planetData)
	{
		GameState gameState = new GameState
		{
			planetName = planetName,
			PlanetData = planetData,
			gameStateSerialized = serialized
		};
		gameState.ImportDataFromAsset(serialized);
		gameState.planetPlayerData = (PlayerData.GetPlayerData(gameState.planetName) ?? new PlayerData(gameState.planetName));
		if (!gameState.planetPlayerData.Has("PlanetLoadCount"))
		{
			gameState.planetPlayerData.Set("PlanetLoadCount", "1");
		}
		else
		{
			gameState.planetPlayerData.Add("PlanetLoadCount", 1.0);
		}
		gameState.OnPlayerDataLoaded();
		return gameState;
	}

	// Token: 0x06000AA8 RID: 2728 RVA: 0x0002F4B0 File Offset: 0x0002D6B0
	private void ValidateEventDictionary()
	{
		foreach (Unlock unlock2 in from unlock in this.Unlocks
		where this.Unlocks.FindAll((Unlock u) => u.name == unlock.name).Count > 1
		select unlock)
		{
			Debug.LogErrorFormat("There are two or more unlocks with the name [{0}], this will cause issues with saving and must be corrected.", new object[]
			{
				unlock2.name
			});
		}
	}

	// Token: 0x06000AA9 RID: 2729 RVA: 0x0002F520 File Offset: 0x0002D720
	public void InitVentureModels(IObservable<float> updateStream)
	{
		List<UnfoldingData> unfoldingData = GameController.Instance.UnfoldingService.UnfoldingData;
		using (List<Venture>.Enumerator enumerator = this.ventures.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Venture venture = enumerator.Current;
				UnfoldingData unfoldingData2 = unfoldingData.FirstOrDefault((UnfoldingData x) => x.Target == venture.id && x.Type == UnfoldingData.EUnfoldingType.ShowVenture);
				UnfoldingData unfoldingData3 = unfoldingData.FirstOrDefault((UnfoldingData x) => x.Target == venture.id && x.Type == UnfoldingData.EUnfoldingType.UnlockVenture);
				List<TriggerData> visableTriggers = (unfoldingData2 == null) ? new List<TriggerData>() : unfoldingData2.TriggerDatas;
				List<TriggerData> unlockTriggers = (unfoldingData3 == null) ? new List<TriggerData>() : unfoldingData3.TriggerDatas;
				VentureModel ventureModel = new VentureModel(venture, GameController.Instance.AngelService.TotalAngelBonus, this.TotalMultiplier, GameController.Instance.GildingService.AllVenturesBoosted, this.planetPlayerData.GetObservable("Platinum Upgrade", 0.0), visableTriggers, unlockTriggers);
				if (updateStream != null)
				{
					ventureModel.RegisterUpdateCall(updateStream, this.CashOnHand, new Func<double, double>(this.CalculateElapsed));
				}
				this.VentureModels.Add(ventureModel);
			}
		}
	}

	// Token: 0x06000AAA RID: 2730 RVA: 0x0002F654 File Offset: 0x0002D854
	public long GetFacebookLeaderboardScore()
	{
		long num = (long)Math.Log10(this.TotalPreviousCash.Value) + 1L;
		double num2;
		for (num2 = this.TotalPreviousCash.Value; num2 > 1000000.0; num2 /= 10.0)
		{
		}
		return num * 1000000L + (long)Convert.ToInt32(num2);
	}

	// Token: 0x06000AAB RID: 2731 RVA: 0x0002F6AC File Offset: 0x0002D8AC
	public int GetCashLeaderboardScore()
	{
		return GameState.GetDoubleAsInt(this.TotalPreviousCash.Value + this.CashOnHand.Value);
	}

	// Token: 0x06000AAC RID: 2732 RVA: 0x0002F6CA File Offset: 0x0002D8CA
	public int GetAngelsLeaderboardScore()
	{
		return GameState.GetDoubleAsInt(GameController.Instance.AngelService.AngelsOnHand.Value);
	}

	// Token: 0x06000AAD RID: 2733 RVA: 0x0002F6E8 File Offset: 0x0002D8E8
	public static int GetDoubleAsInt(double val)
	{
		int num = (int)Math.Log10(val);
		double num2;
		for (num2 = val; num2 > 100000.0; num2 /= 10.0)
		{
		}
		return num * 100000 + Convert.ToInt32(num2);
	}

	// Token: 0x06000AAE RID: 2734 RVA: 0x0002F728 File Offset: 0x0002D928
	public static double GetValueFromLeaderboardScore(int score)
	{
		string text = score.ToString();
		string text2 = text.Substring(text.Length - Math.Min(5, text.Length));
		int num = int.Parse(text2);
		if (num < 10000)
		{
			return (double)num;
		}
		int num2 = (score - num) / 100000;
		double num3 = Convert.ToDouble(text2.Insert(1, "."));
		string s = string.Format("{0}E{1}", num3, num2);
		double result;
		try
		{
			result = double.Parse(s);
		}
		catch (OverflowException)
		{
			result = double.MaxValue;
		}
		return result;
	}

	// Token: 0x06000AAF RID: 2735 RVA: 0x00002718 File Offset: 0x00000918
	public virtual void WireAchievements()
	{
	}

	// Token: 0x06000AB0 RID: 2736 RVA: 0x0002F7CC File Offset: 0x0002D9CC
	public virtual void ResetGameStateVentures()
	{
		this.ventures = new List<Venture>();
		this.ventures = this.gameStateSerialized.planetVentures;
	}

	// Token: 0x06000AB1 RID: 2737 RVA: 0x0002F7EA File Offset: 0x0002D9EA
	protected void AchievementEarned(HHAchievement achievement)
	{
		if (achievement.Achieved)
		{
			return;
		}
		Debug.LogFormat("[ Achievement ] [{0}] earned", new object[]
		{
			achievement.Id
		});
		achievement.Achieved = true;
		this.OnAchievementEarned.OnNext(achievement);
	}

	// Token: 0x06000AB2 RID: 2738 RVA: 0x0002F821 File Offset: 0x0002DA21
	public void OnPlayerDataLoaded()
	{
		this.fluxCapitalorQuantity.Value = this.planetPlayerData.GetInt("Flux Capacitor", 0);
		this.PlatinumUpgradesMultiplier = this.planetPlayerData.GetObservable("Platinum Upgrade", 0.0);
	}

	// Token: 0x06000AB3 RID: 2739 RVA: 0x0002F85E File Offset: 0x0002DA5E
	public double CalculateElapsed(double elapsed)
	{
		return elapsed + elapsed * (double)this.fluxCapitalorMultiplier.Value;
	}

	// Token: 0x06000AB4 RID: 2740 RVA: 0x0002F870 File Offset: 0x0002DA70
	public void LoadGameSaveData(GameStateSaveData data)
	{
		if (data == null)
		{
			this.ProfitAdExpiry = GameController.Instance.DateTimeService.UtcNow;
			return;
		}
		using (List<UnlockSaveData>.Enumerator enumerator = data.permanentUnlocks.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				UnlockSaveData permanentUnlock = enumerator.Current;
				Unlock unlock = this.Unlocks.Find((Unlock u) => u.name == permanentUnlock.id);
				if (unlock != null)
				{
					unlock.Earned.Value = true;
					unlock.Claimed.Value = true;
					unlock.EverClaimed.Value = true;
					this.AchievedUnlocks.Add(unlock.name);
				}
			}
		}
		this.MilestonesSaveDatas = data.planetMilestones;
		this.MilestoneScore = data.planetMilestoneScore;
		using (IEnumerator<VentureModel> enumerator2 = this.VentureModels.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				VentureModel venture = enumerator2.Current;
				VentureSaveData ventureSaveData = data.ventures.Find((VentureSaveData v) => v.id == venture.Id);
				if (ventureSaveData == null)
				{
					Debug.LogWarningFormat("Unable to restore data for venture [{0}], did the id change?", new object[]
					{
						venture.Id
					});
				}
				else
				{
					venture.LoadSavedData(ventureSaveData);
				}
			}
		}
		if (!GameController.Instance.UnlockService.AutoClaimFirstTimeUnlocks)
		{
			using (List<LastClaimedUnlockSaveData>.Enumerator enumerator3 = data.claimedUnlockThresholds.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					LastClaimedUnlockSaveData threshold = enumerator3.Current;
					if (threshold.targetId == "all")
					{
						using (List<EveryVentureUnlock>.Enumerator enumerator4 = this.Unlocks.OfType<EveryVentureUnlock>().ToList<EveryVentureUnlock>().GetEnumerator())
						{
							while (enumerator4.MoveNext())
							{
								EveryVentureUnlock everyVentureUnlock = enumerator4.Current;
								if (threshold.amount >= everyVentureUnlock.amountToEarn)
								{
									everyVentureUnlock.EverClaimed.Value = true;
								}
							}
							continue;
						}
					}
					foreach (SingleVentureUnlock singleVentureUnlock in (from x in this.Unlocks.OfType<SingleVentureUnlock>()
					where x.ventureName == threshold.targetId
					select x).ToList<SingleVentureUnlock>())
					{
						if (threshold.amount >= singleVentureUnlock.amountToEarn)
						{
							singleVentureUnlock.EverClaimed.Value = true;
						}
					}
				}
			}
		}
		using (List<Upgrade>.Enumerator enumerator6 = this.Upgrades.GetEnumerator())
		{
			while (enumerator6.MoveNext())
			{
				Upgrade upgrade = enumerator6.Current;
				upgrade.LoadSaveData(data.upgrades.Find((UpgradeSaveData u) => u.id == upgrade.id));
				if (upgrade.IsPurchased.Value)
				{
					upgrade.Apply(this);
					upgrade.IsPurchaseable.Value = false;
				}
			}
		}
		using (List<Upgrade>.Enumerator enumerator6 = this.Managers.GetEnumerator())
		{
			while (enumerator6.MoveNext())
			{
				Upgrade manager = enumerator6.Current;
				manager.LoadSaveData(data.managers.Find((UpgradeSaveData m) => m.id == manager.id));
				if (manager.IsPurchased.Value)
				{
					manager.Apply(this);
					manager.IsPurchaseable.Value = false;
				}
			}
		}
		this.timestamp = data.timestamp;
		GameController.Instance.SetMute(data.isMuted);
		this.planetName = data.planetName;
		if (data.cashOnHand < 0.0)
		{
			double cashOnHand = data.cashOnHand;
			data.cashOnHand *= -1.0;
			GameController.Instance.AnalyticService.SendTaskCompleteEvent("CashBelowZeroFix", string.Concat(cashOnHand), string.Concat(data.cashOnHand));
		}
		if (data.sessionCash < 0.0)
		{
			data.sessionCash *= -1.0;
			double sessionCash = data.sessionCash;
			GameController.Instance.AnalyticService.SendTaskCompleteEvent("SessionCashBelowZeroFix", string.Concat(sessionCash), string.Concat(data.sessionCash));
		}
		if (data.totalPreviousCash < 0.0)
		{
			data.totalPreviousCash *= -1.0;
			double num = data.totalPreviousCash;
			GameController.Instance.AnalyticService.SendTaskCompleteEvent("TotalPreviousCashBelowZeroFix", string.Concat(num), string.Concat(data.totalPreviousCash));
		}
		if (data.cashOnHand > GameState.MAX_CASH_DOUBLE)
		{
			data.cashOnHand = GameState.MAX_CASH_DOUBLE;
		}
		if (data.sessionCash > GameState.MAX_CASH_DOUBLE)
		{
			data.sessionCash = GameState.MAX_CASH_DOUBLE;
		}
		if (data.totalPreviousCash > GameState.MAX_CASH_DOUBLE)
		{
			data.totalPreviousCash = GameState.MAX_CASH_DOUBLE;
		}
		this.CashOnHand.Value = data.cashOnHand;
		this.SessionCash.Value = data.sessionCash;
		this.TotalPreviousCash.Value = data.totalPreviousCash;
		this.angelInvestors = data.angelInvestors;
		this.angelInvestorsSpent = data.angelInvestorsSpent;
		this.neverRate = data.neverRate;
		this.managerRateOnce = data.managerRateOnce;
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		if (string.IsNullOrEmpty(data.ProfitAdExpiry) || data.ProfitAdExpiry == dateTime.ToString())
		{
			DateTime utcNow = GameController.Instance.DateTimeService.UtcNow;
			data.ProfitAdExpiry = utcNow.ToString();
			if (utcNow > GameController.Instance.DateTimeService.UtcNow)
			{
				data.videosAvailible = 5;
				data.videosCurrentDay = GameController.Instance.DateTimeService.UtcNow.DayOfYear;
			}
		}
		this.AvailableAds = data.videosAvailible;
		this.VideosCurrentDay = data.videosCurrentDay;
		this.ProfitAdExpiry = DateTime.Parse(data.ProfitAdExpiry);
		this.HighestMultiplierPurchased.Value = Mathf.Max(4, data.highestMultiplierPurchased);
		this.hasProfitMartiansBeenRun.Value = data.hasProfitMartiansBeenRun;
		this.hasUserClaimedFreeGold = data.hasUserClaimedFreeGold;
		this.LoadGender();
	}

	// Token: 0x06000AB5 RID: 2741 RVA: 0x0002FFB0 File Offset: 0x0002E1B0
	private void LoadGender()
	{
		string value = PlayerData.GetPlayerData("Global").Get("gender", "");
		if (string.IsNullOrEmpty(value))
		{
			value = "male";
		}
	}


	// Token: 0x06000AB7 RID: 2743 RVA: 0x00030070 File Offset: 0x0002E270
	public GameStateSaveData BuildGameStateSaveData()
	{
		List<VentureSaveData> list = (from venture in this.VentureModels
		select venture.Save()).ToList<VentureSaveData>();
		List<UpgradeSaveData> upgrades = (from upgrade in GameController.Instance.UpgradeService.Upgrades
		select upgrade.Save()).ToList<UpgradeSaveData>();
		List<UpgradeSaveData> managers = (from manager in GameController.Instance.UpgradeService.Managers
		select manager.Save()).ToList<UpgradeSaveData>();
		List<UnlockSaveData> permanentUnlocks = (from x in (from x in (from x in GameController.Instance.UnlockService.Unlocks
		where x.permanent
		select x).ToList<Unlock>()
		where x.Earned.Value
		select x).ToList<Unlock>()
		select x.Save()).ToList<UnlockSaveData>();
		List<PlanetMilestoneSaveData> milestones = (from milestone 
                in GameController.Instance.PlanetMilestoneService.GetUserMilestonesForCurrentPlanet()
		select milestone.Save()).ToList<PlanetMilestoneSaveData>();
		List<LastClaimedUnlockSaveData> list2 = new List<LastClaimedUnlockSaveData>();
		List<SingleVentureUnlock> source = (from x in GameController.Instance.UnlockService.Unlocks.OfType<SingleVentureUnlock>()
		where x.EverClaimed.Value
		select x).ToList<SingleVentureUnlock>();
		for (int i = 0; i < this.VentureModels.Count; i++)
		{
			VentureModel venture = this.VentureModels[i];
			List<SingleVentureUnlock> list3 = (from x in source
			where x.ventureName == venture.Name
			orderby x.amountToEarn descending
			select x).ToList<SingleVentureUnlock>();
			if (list3.Count > 0)
			{
				SingleVentureUnlock singleVentureUnlock = list3[0];
				list2.Add(new LastClaimedUnlockSaveData
				{
					targetId = venture.Name,
					amount = singleVentureUnlock.amountToEarn
				});
			}
		}
		List<EveryVentureUnlock> list4 = (from x in GameController.Instance.UnlockService.Unlocks.OfType<EveryVentureUnlock>()
		where x.EverClaimed.Value
		orderby x.amountToEarn descending
		select x).ToList<EveryVentureUnlock>();
		if (list4.Count > 0)
		{
			list2.Add(new LastClaimedUnlockSaveData
			{
				targetId = "all",
				amount = list4[0].amountToEarn
			});
		}
		return new GameStateSaveData(this, GameController.Instance.AngelService, list, upgrades, managers, permanentUnlocks, milestones, list2)
		{
			planetMilestoneScore = GameController.Instance.PlanetMilestoneService.CurrentScore.Value
		};
	}

	// Token: 0x06000AB8 RID: 2744 RVA: 0x000303AC File Offset: 0x0002E5AC
	public void SetMultiplierBonus(GameState.MultiplierBonusType key, float value)
	{
		if (this._MultiplierBonusMap.ContainsKey(key))
		{
			this._MultiplierBonusMap[key] = value;
		}
		else
		{
			this._MultiplierBonusMap.Add(key, value);
		}
		this.MultiplierBonus.Value = this._MultiplierBonusMap.Sum((KeyValuePair<GameState.MultiplierBonusType, float> i) => i.Value);
	}

	// Token: 0x06000AB9 RID: 2745 RVA: 0x00030418 File Offset: 0x0002E618
	public void RemoveMultiplierBonus(GameState.MultiplierBonusType key)
	{
		if (this._MultiplierBonusMap.Remove(key))
		{
			this.MultiplierBonus.Value = this._MultiplierBonusMap.Sum((KeyValuePair<GameState.MultiplierBonusType, float> i) => i.Value);
		}
	}

	// Token: 0x06000ABA RID: 2746 RVA: 0x00030468 File Offset: 0x0002E668
	public float GetMultiplierBonus(GameState.MultiplierBonusType key)
	{
		float result;
		this._MultiplierBonusMap.TryGetValue(key, out result);
		return result;
	}

	// Token: 0x06000ABB RID: 2747 RVA: 0x00030485 File Offset: 0x0002E685
	public int GetModifiedGoldAmount(int initialAmount)
	{
		return Mathf.CeilToInt((float)initialAmount * (1f + this.goldBonus.Value));
	}

	// Token: 0x06000ABC RID: 2748 RVA: 0x000304A0 File Offset: 0x0002E6A0
	public int GetModifiedMegaBucksAmount(int initialAmount)
	{
		return Mathf.CeilToInt((float)initialAmount * (1f + this.megabucksBonus.Value));
	}

	// Token: 0x06000ABD RID: 2749 RVA: 0x000304BC File Offset: 0x0002E6BC
	protected void ImportDataFromAsset(GameState_Serialized gameStateInfo)
	{
		this.planetName = gameStateInfo.planetName;
		this.planetTheme.Value = gameStateInfo.planetTheme;
		this.planetTitle = gameStateInfo.planetTitle;
		this.currencyName = gameStateInfo.currencyName;
		this.megaBucksExchangeBaseCost = gameStateInfo.megaBucksExchangeBaseCost;
		this.megaBucksExchangeRatePercent = gameStateInfo.megaBucksExchangeRatePercent;
		this.angelAccumulationRate = gameStateInfo.angelAccumulationRate;
		this.progressionType = gameStateInfo.progressionType;
		this.dockPortraitProfitBooster = gameStateInfo.dockPortraitProfitBooster;
		this.ventures = gameStateInfo.planetVentures;
		this.Managers.Clear();
		foreach (OrderLogic orderLogic in gameStateInfo.managerOrder)
		{
			switch (orderLogic.list)
			{
			case 0:
				this.Managers.Add(gameStateInfo.runManagers[orderLogic.index]);
				break;
			case 1:
				this.Managers.Add(gameStateInfo.accountManagers[orderLogic.index]);
				break;
			case 2:
				this.Managers.Add(gameStateInfo.featureManagers[orderLogic.index]);
				break;
			}
		}
		foreach (Upgrade upgrade in this.Managers)
		{
			upgrade.IsPurchased.Value = false;
			upgrade.IsPurchaseable.Value = false;
		}
		this.Unlocks.Clear();
		int num = 0;
		foreach (OrderLogic orderLogic2 in gameStateInfo.unlockOrder)
		{
			switch (orderLogic2.list)
			{
			case 0:
				this.Unlocks.Add(gameStateInfo.singleUnlocks[orderLogic2.index]);
				break;
			case 1:
				this.Unlocks.Add(gameStateInfo.everyUnlocks[orderLogic2.index]);
				break;
			case 2:
				this.Unlocks.Add(gameStateInfo.eventUnlocks[orderLogic2.index]);
				break;
			}
			RewardSerialInformation rewardSerialInformation = gameStateInfo.rewardSerialInformation[num];
			string type = rewardSerialInformation.type;
			uint num2 = (uint)(type).GetHashCode();
			if (num2 <= 1609428040U)
			{
				if (num2 <= 163507718U)
				{
					if (num2 != 63423903U)
					{
						if (num2 != 163507718U)
						{
							goto IL_613;
						}
						if (!(type == "UnlockRewardVentureProfitPer"))
						{
							goto IL_613;
						}
						this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardVentureProfitPer(rewardSerialInformation.affectedVenture, double.Parse(rewardSerialInformation.bonus));
					}
					else
					{
						if (!(type == "UnlockRewardTimewarpExpress"))
						{
							goto IL_613;
						}
						this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardTimeWarpExpress(int.Parse(rewardSerialInformation.bonus));
					}
				}
				else if (num2 != 1350674840U)
				{
					if (num2 != 1573848462U)
					{
						if (num2 != 1609428040U)
						{
							goto IL_613;
						}
						if (!(type == "UnlockRewardEveryVentureCooldownTime"))
						{
							goto IL_613;
						}
						this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardEveryVentureCooldownTime(float.Parse(rewardSerialInformation.bonus));
					}
					else
					{
						if (!(type == "UnlockRewardTimewarpDaily"))
						{
							goto IL_613;
						}
						this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardTimewarpDaily(int.Parse(rewardSerialInformation.bonus));
					}
				}
				else
				{
					if (!(type == "UnlockRewardGold"))
					{
						goto IL_613;
					}
					this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardGold(int.Parse(rewardSerialInformation.bonus));
				}
			}
			else if (num2 <= 2701424353U)
			{
				if (num2 != 1838497017U)
				{
					if (num2 != 2344991440U)
					{
						if (num2 != 2701424353U)
						{
							goto IL_613;
						}
						if (!(type == "UnlockRewardVentureCooldownTime"))
						{
							goto IL_613;
						}
						this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardVentureCooldownTime(rewardSerialInformation.affectedVenture, float.Parse(rewardSerialInformation.bonus));
					}
					else
					{
						if (!(type == "UnlockRewardTimeWarpHourly"))
						{
							goto IL_613;
						}
						this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardTimewarpHourly(int.Parse(rewardSerialInformation.bonus), false);
					}
				}
				else
				{
					if (!(type == "UnlockRewardEveryVentureProfitPer"))
					{
						goto IL_613;
					}
					this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardEveryVentureProfitPer(double.Parse(rewardSerialInformation.bonus));
				}
			}
			else if (num2 != 2855315681U)
			{
				if (num2 != 3730089504U)
				{
					if (num2 != 4246317712U)
					{
						goto IL_613;
					}
					if (!(type == "UnlockRewardItemizationItem"))
					{
						goto IL_613;
					}
					this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardItemizationItem(rewardSerialInformation.affectedVenture, int.Parse(rewardSerialInformation.bonus));
				}
				else
				{
					if (!(type == "UnlockRewardMegaBucks"))
					{
						goto IL_613;
					}
					this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardMegaBucks(int.Parse(rewardSerialInformation.bonus), rewardSerialInformation.affectedVenture);
				}
			}
			else
			{
				if (!(type == "UnlockRewardBadge"))
				{
					goto IL_613;
				}
				this.Unlocks[this.Unlocks.Count - 1].Reward = new UnlockRewardBadge(rewardSerialInformation.bonus);
			}
			IL_636:
			num++;
			continue;
			IL_613:
			Debug.LogErrorFormat("[GameState](ImportDataFromAsset) Unhandled reward type [{0}] in GameState_Serialized [{1}]", new object[]
			{
				rewardSerialInformation.type,
				gameStateInfo.name
			});
			goto IL_636;
		}
		foreach (Unlock unlock in this.Unlocks)
		{
			unlock.Earned.Value = false;
			unlock.EverClaimed.Value = false;
			unlock.Claimed.Value = false;
		}
		this.Upgrades.Clear();
		foreach (OrderLogic orderLogic3 in gameStateInfo.upgradeOrder)
		{
			switch (orderLogic3.list)
			{
			case 0:
				this.Upgrades.Add(gameStateInfo.ventureUpgrades[orderLogic3.index]);
				break;
			case 1:
				this.Upgrades.Add(gameStateInfo.aiUpgrades[orderLogic3.index]);
				break;
			case 2:
				this.Upgrades.Add(gameStateInfo.buyVentureUpgrades[orderLogic3.index]);
				break;
			case 3:
				this.Upgrades.Add(gameStateInfo.everythingUpgrades[orderLogic3.index]);
				break;
			}
		}
		foreach (Upgrade upgrade2 in this.Upgrades)
		{
			upgrade2.IsPurchased.Value = false;
			upgrade2.IsPurchaseable.Value = false;
		}
		this.PlanetMilestones.Clear();
		foreach (PlanetMilestone item in gameStateInfo.planetMilestones)
		{
			this.PlanetMilestones.Add(item);
		}
		Debug.Log("Finished Loading Data");
	}

	// Token: 0x040008A9 RID: 2217
	public static double MAX_CASH_DOUBLE = 1E+307;

	// Token: 0x040008AA RID: 2218
	public string planetName = "";

	// Token: 0x040008AB RID: 2219
	public StringReactiveProperty planetTheme = new StringReactiveProperty();

	// Token: 0x040008AC RID: 2220
	public string planetTitle = "";

	// Token: 0x040008AD RID: 2221
	public string currencyName = "dollars";

	// Token: 0x040008AE RID: 2222
	public double megaBucksExchangeBaseCost = 1E+33;

	// Token: 0x040008AF RID: 2223
	public double megaBucksExchangeRatePercent = 100.0;

	// Token: 0x040008B0 RID: 2224
	public double angelAccumulationRate;

	// Token: 0x040008B1 RID: 2225
	public PlanetProgressionType progressionType;

	// Token: 0x040008B2 RID: 2226
	public bool dockPortraitProfitBooster;

	// Token: 0x040008B3 RID: 2227
	public PlayerData planetPlayerData;

	// Token: 0x040008B4 RID: 2228
	public PlanetData PlanetData;

	// Token: 0x040008B5 RID: 2229
	public GameState_Serialized gameStateSerialized;

	// Token: 0x040008B7 RID: 2231
	[Obsolete("now CashOnHand")]
	[JsonInclude]
	public double cash;

	// Token: 0x040008B8 RID: 2232
	[Obsolete("Now SessionCash")]
	[JsonInclude]
	public double totalCash;

	// Token: 0x040008B9 RID: 2233
	[Obsolete("Now TotalPreviousCash")]
	[JsonInclude]
	public double totalPreviousCash;

	// Token: 0x040008BA RID: 2234
	[Obsolete("Now AngelsOnHand")]
	public double angelInvestors;

	// Token: 0x040008BB RID: 2235
	[Obsolete("Now AngelsSpent")]
	public double angelInvestorsSpent;

	// Token: 0x040008BC RID: 2236
	[Obsolete("Now ventureModels")]
	[JsonInclude]
	public List<Venture> ventures = new List<Venture>();

	// Token: 0x040008BD RID: 2237
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public ClampedReactiveDouble CashOnHand = new ClampedReactiveDouble(0.0);

	// Token: 0x040008BE RID: 2238
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public ClampedReactiveDouble SessionCash = new ClampedReactiveDouble(0.0);

	// Token: 0x040008BF RID: 2239
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public ClampedReactiveDouble TotalPreviousCash = new ClampedReactiveDouble(0.0);

	// Token: 0x040008C0 RID: 2240
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public FloatReactiveProperty PurchasedMultiplier = new FloatReactiveProperty(0f);

	// Token: 0x040008C1 RID: 2241
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public IObservable<double> PlatinumUpgradesMultiplier;

	// Token: 0x040008C2 RID: 2242
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	private Dictionary<GameState.MultiplierBonusType, float> _MultiplierBonusMap = new Dictionary<GameState.MultiplierBonusType, float>();

	// Token: 0x040008C6 RID: 2246
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public IntReactiveProperty HighestMultiplierPurchased = new IntReactiveProperty(4);

	// Token: 0x040008C8 RID: 2248
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public ReactiveProperty<float> goldBonus = new ReactiveProperty<float>();

	// Token: 0x040008C9 RID: 2249
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public ReactiveProperty<float> megabucksBonus = new ReactiveProperty<float>();

	// Token: 0x040008CA RID: 2250
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public Subject<VentureModel> OnVenturePurchased = new Subject<VentureModel>();

	// Token: 0x040008CB RID: 2251
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public Subject<HHAchievement> OnAchievementEarned = new Subject<HHAchievement>();

	// Token: 0x040008CC RID: 2252
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public BoolReactiveProperty hasProfitMartiansBeenRun = new BoolReactiveProperty(false);

	// Token: 0x040008CD RID: 2253
	public double timestamp;

	// Token: 0x040008CE RID: 2254
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public ReactiveCollection<VentureModel> VentureModels = new ReactiveCollection<VentureModel>();

	// Token: 0x040008CF RID: 2255
	public List<string> AchievedUnlocks = new List<string>();

	// Token: 0x040008D0 RID: 2256
	public int neverRate;

	// Token: 0x040008D1 RID: 2257
	public int managerRateOnce;

	// Token: 0x040008D2 RID: 2258
	public int highestMultiplierPurchase;

	// Token: 0x040008D3 RID: 2259
	public Dictionary<string, int> eventItems = new Dictionary<string, int>();

	// Token: 0x040008D4 RID: 2260
	public Dictionary<string, int> eventItemMax = new Dictionary<string, int>();

	// Token: 0x040008D5 RID: 2261
	public Dictionary<string, int> unopendGifts = new Dictionary<string, int>();

	// Token: 0x040008D6 RID: 2262
	public Dictionary<string, int> unclickedItems = new Dictionary<string, int>();

	// Token: 0x040008D7 RID: 2263
	public Dictionary<string, int> eventGifts = new Dictionary<string, int>();

	// Token: 0x040008D8 RID: 2264
	public Dictionary<string, int> eventGiftsItemAmounts = new Dictionary<string, int>();

	// Token: 0x040008D9 RID: 2265
	public Dictionary<string, int> holidayVentureGifts = new Dictionary<string, int>();

	// Token: 0x040008DA RID: 2266
	public Dictionary<string, BadgeInfo> eventBadges = new Dictionary<string, BadgeInfo>();

	// Token: 0x040008DB RID: 2267
	public bool hasUserClaimedFreeGold;

	// Token: 0x040008DC RID: 2268
	public List<Unlock> Unlocks = new List<Unlock>();

	// Token: 0x040008DD RID: 2269
	public List<Upgrade> Upgrades = new List<Upgrade>();

	// Token: 0x040008DE RID: 2270
	public List<Upgrade> Managers = new List<Upgrade>();

	// Token: 0x040008DF RID: 2271
	public List<PlanetMilestone> PlanetMilestones = new List<PlanetMilestone>();

	// Token: 0x040008E0 RID: 2272
	public List<PlanetMilestoneSaveData> MilestonesSaveDatas = new List<PlanetMilestoneSaveData>();

	// Token: 0x040008E1 RID: 2273
	public double MilestoneScore;

	// Token: 0x040008E2 RID: 2274
	public DateTime ProfitAdExpiry = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	// Token: 0x040008E3 RID: 2275
	public float AdMultiplierBonus = 2f;

	// Token: 0x040008E4 RID: 2276
	public int AvailableAds = 6;

	// Token: 0x040008E5 RID: 2277
	public int VideosCurrentDay = -1;

	// Token: 0x040008E6 RID: 2278
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public bool kongregateDataTransferred;

	// Token: 0x040008E7 RID: 2279
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	private float _adRewardMultiplier = 1f;

	// Token: 0x040008E8 RID: 2280
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	private float _facebookMultiplier = 2f;

	// Token: 0x040008E9 RID: 2281
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	private float _purchasedMultiplier = 1f;

	// Token: 0x040008EA RID: 2282
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public readonly float fluxCapitalorEffectiveness = 1.21f;

	// Token: 0x040008EB RID: 2283
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public IntReactiveProperty fluxCapitalorQuantity = new IntReactiveProperty(0);

	// Token: 0x040008EC RID: 2284
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public ReadOnlyReactiveProperty<float> fluxCapitalorMultiplier;

	// Token: 0x040008EE RID: 2286
	protected CompositeDisposable _Disposables = new CompositeDisposable();

	// Token: 0x02000847 RID: 2119
	public enum MultiplierBonusType
	{
		// Token: 0x04002A57 RID: 10839
		NA,
		// Token: 0x04002A58 RID: 10840
		Ad,
		// Token: 0x04002A59 RID: 10841
		Facebook,
		// Token: 0x04002A5A RID: 10842
		ProfitBoost,
		// Token: 0x04002A5B RID: 10843
		Itemization_suit,
		// Token: 0x04002A5C RID: 10844
		PlanetProfit,
		// Token: 0x04002A5D RID: 10845
		Itemization_Badge,
		// Token: 0x04002A5E RID: 10846
		VIPSubscription,
		// Token: 0x04002A5F RID: 10847
		New_Itemization_Hat = 100,
		// Token: 0x04002A60 RID: 10848
		New_Itemization_Body,
		// Token: 0x04002A61 RID: 10849
		New_Itemization_Legs,
		// Token: 0x04002A62 RID: 10850
		New_Itemization_Badge_One,
		// Token: 0x04002A63 RID: 10851
		New_Itemization_Badge_Two,
		// Token: 0x04002A64 RID: 10852
		New_Itemization_Badge_Three
	}
}
