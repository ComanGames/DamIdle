using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class VentureModel
{
    public IObservable<double> FinishedRunning
	{
		get
		{
			return Observable.FromEvent<double>(delegate(Action<double> h)
			{
				this._FinishedRunning = (Action<double>)Delegate.Combine(this._FinishedRunning, h);
			}, delegate(Action<double> h)
			{
				this._FinishedRunning = (Action<double>)Delegate.Remove(this._FinishedRunning, h);
			});
		}
	}

    public string Id { get; private set; }

    public string Name { get; private set; }

    public string Plural { get; private set; }

    public string BonusName { get; private set; }

    public string ImageName { get; private set; }

    public double ExpenseRate { get; set; }

    public ReadOnlyReactiveProperty<float> EffectiveCoolDownTime { get; private set; }

    public ReadOnlyReactiveProperty<double> ProfitOnNext { get; private set; }

    public ReadOnlyReactiveProperty<double> CashPerSec { get; set; }

    private event Func<double, double> RecalculateDeltaTime;



    public VentureModel(string id, string name, int numOwned, string imageName, double costPer, double profitPer, float coolDownTime, double expenseRate, string plural, string bonusName, IObservable<double> angelBonus, IObservable<float> totalProfitMultiplier, BoolReactiveProperty isPlatinumBoosted, IObservable<double> platinumBoostedVentureBonus, List<TriggerData> visableTriggers, List<TriggerData> unlockTriggers)
	{
		this.Id = id;
		this.Name = name;
		this.NumOwned_Base.Value = (double)numOwned;
		this.ImageName = imageName;
		this.ProfitPer.Value = profitPer;
		this.CoolDownTime.Value = coolDownTime;
		this.ProgressTimer.Value = 0f;
		this.ExpenseRate = expenseRate;
		this.Plural = plural;
		this.BonusName = bonusName;
		this.CostPer.Value = costPer;
		this.initialCostPer = costPer;
		this.NumOwned_Base.CombineLatest(this.NumOwned_Upgrades, (b, u) => b + u).Subscribe(delegate(double x) { this.TotalOwned.Value = x; });
		this.AccountantEffect.Subscribe(delegate(double v)
		{
			this.OnVentureCostChange();
		}).AddTo(this._Disposables);
		this.EffectiveCostReduction.Subscribe(delegate(double v)
		{
			this.OnVentureCostChange();
		}).AddTo(this._Disposables);
		this.equippedMultiplierItemBonuses.Add(new ReactiveProperty<float>(0f));
		this.equippedSpeedItemBonuses.Add(new ReactiveProperty<float>(0f));
		this.equippedCostReductionItemBonuses.Add(new ReactiveProperty<float>(0f));
		this.visableTriggers = visableTriggers;
		this.unlockTriggers = unlockTriggers;
		IObservable<double> left = this.ProfitPer.CombineLatest(this.TotalOwned, (p, o) => p * o).CombineLatest(this.totalVentureBoostBonus, (p, t) => p * t).CombineLatest(this.gildLevel, (p, g) => p *  this.CalculateTotalGildBonus( this.gildLevel.Value));
		this.EffectiveCoolDownTime = this.CoolDownTime.CombineLatest(this.speedItemBonus, (cooldown, ventureBoost) => cooldown / (1f + ventureBoost)).ToReadOnlyReactiveProperty<float>();
		this.CashPerSec = left.CombineLatest(angelBonus, (bProfit, aBonus) => bProfit * aBonus).CombineLatest(totalProfitMultiplier, this.ventureMultiplierBonus, (bProfit, tMultiplier, vmBonus) => bProfit * (double)(tMultiplier + vmBonus)).CombineLatest(this.EffectiveCoolDownTime, (pValue, cdTime) => pValue / (double)cdTime).ToReadOnlyReactiveProperty<double>();
		this.ProfitOnNext = left.CombineLatest(angelBonus, (p, a) => p * a).CombineLatest(totalProfitMultiplier, this.ventureMultiplierBonus, (m, t, v) => m * (double)(t + v)).ToReadOnlyReactiveProperty<double>();
		this.IsBoosted.CombineLatest(isPlatinumBoosted, new Func<bool, bool, Tuple<bool, bool>>(Tuple.Create<bool, bool>)).Subscribe(delegate(Tuple<bool, bool> t)
		{
			if (t.Item1)
			{
				IObservable<double> source =  this.goldBoostedVentureBonusValue;
				Action<double> onNext;
                onNext =  delegate(double gv)
                {
                     this.totalVentureBoostBonus.Value = gv;
                };

				source.Subscribe(onNext).AddTo( this._Disposables);
				if (t.Item2)
				{
					IObservable<double> platinumBoostedVentureBonus2 = platinumBoostedVentureBonus;
					Action<double> onNext2;
						onNext2 = delegate(double pv)
						{
							 this.totalVentureBoostBonus.Value = pv;
						};
					platinumBoostedVentureBonus2.Subscribe(onNext2).AddTo( this._Disposables);
				}
			}
		}).AddTo(this._Disposables);
		(from u in GameController.Instance.UnlockService.OnUnlockAchieved
		where u is SingleVentureUnlock || u is EveryVentureUnlock
		select u into unlock
		select unlock.Reward into r
		where r is UnlockRewardVentureCooldownTime || r is UnlockRewardVentureProfitPer || r is UnlockRewardEveryVentureCooldownTime || r is UnlockRewardEveryVentureProfitPer
		select r).ThrottleFrame(1, FrameCountType.Update).Subscribe(new Action<IUnlockReward>(this.OnRewardEarned)).AddTo(this._Disposables);
		MessageBroker.Default.Receive<MicroManagerService.MicroManagerBoostValues>().Subscribe(new Action<MicroManagerService.MicroManagerBoostValues>(this.OnMicromanagerBoosted)).AddTo(this._Disposables);
		MessageBroker.Default.Receive<InventoryEquipMessage>().Subscribe(new Action<InventoryEquipMessage>(this.OnItemEquipStateChanged)).AddTo(this._Disposables);
		this.MonitorUnlockState();
	}

    // Token: 0x06000B28 RID: 2856 RVA: 0x00032730 File Offset: 0x00030930

    public void ClaimAllUnclaimedUnlocks()
	{
		(from x in GameController.Instance.UnlockService.Unlocks
		where x is SingleVentureUnlock && (x as SingleVentureUnlock).ventureName == this.Name
		select x as SingleVentureUnlock into x
		where x.Earned.Value && !x.Claimed.Value
		select x).ToList<SingleVentureUnlock>().ForEach(delegate(SingleVentureUnlock unlock)
		{
			GameController.Instance.UnlockService.ClaimUnlock(unlock);
		});
	}

    // Token: 0x06000B29 RID: 2857 RVA: 0x000327D0 File Offset: 0x000309D0

    private void MonitorUnlockState()
	{
		this.unlockStateDisposables.Clear();
		switch (this.UnlockState.Value)
		{
		case VentureModel.EUnlockState.Hidden:
			this.CheckVisibleTrigger();
			return;
		case VentureModel.EUnlockState.Locked:
			this.CheckUnlockTrigger();
			return;
		case VentureModel.EUnlockState.Unlocked:
			this.CheckForPurchased();
			return;
		default:
			return;
		}
	}

    // Token: 0x06000B2A RID: 2858 RVA: 0x0003281C File Offset: 0x00030A1C

    private void CheckVisibleTrigger()
	{
		if (this.visableTriggers.TrueForAll(x => GameController.Instance.TriggerService.CheckTrigger(x)))
		{
			this.UnlockState.Value = VentureModel.EUnlockState.Locked;
			this.CheckUnlockTrigger();
			return;
		}
		GameController.Instance.TriggerService.MonitorTriggers(this.visableTriggers, true).First(x => x).Subscribe(delegate(bool _)
		{
			this.UnlockState.Value = VentureModel.EUnlockState.Locked;
			this.CheckUnlockTrigger();
		}).AddTo(this.unlockStateDisposables);
	}

    // Token: 0x06000B2B RID: 2859 RVA: 0x000328C0 File Offset: 0x00030AC0

    private void CheckUnlockTrigger()
	{
		if (this.unlockTriggers.All(x => GameController.Instance.TriggerService.CheckTrigger(x)))
		{
			this.UnlockState.Value = VentureModel.EUnlockState.Unlocked;
			this.CheckForPurchased();
			return;
		}
		GameController.Instance.TriggerService.MonitorTriggers(this.unlockTriggers, true).First(x => x).Subscribe(delegate(bool _)
		{
			this.UnlockState.Value = VentureModel.EUnlockState.Unlocked;
			this.CheckForPurchased();
		}).AddTo(this.unlockStateDisposables);
	}

    // Token: 0x06000B2C RID: 2860 RVA: 0x00032964 File Offset: 0x00030B64

    private void CheckForPurchased()
	{
		this.TotalOwned.First(x => x >= 1.0).Subscribe(delegate(double _)
		{
			this.UnlockState.Value = VentureModel.EUnlockState.Purchased;
		}).AddTo(this.unlockStateDisposables);
	}

    // Token: 0x06000B2D RID: 2861 RVA: 0x000329B8 File Offset: 0x00030BB8

    public void ApplyAllEquipmentBonuses()
	{
		foreach (Item item in PlayerData.GetPlayerData("Global").inventory.GetAllEquippedItems())
		{
			if (item.ItemBonusTarget == ItemBonusTarget.Venture)
			{
				this.AdjustItemBonuses(item, true);
			}
		}
	}

    // Token: 0x06000B2E RID: 2862 RVA: 0x00032A24 File Offset: 0x00030C24

    public double CalculateGildBonus(int gildLevel)
	{
		double result = 1.0;
		if (gildLevel >= 0)
		{
			int num = Mathf.FloorToInt((float)(gildLevel / this.gildLevelsRequiredToUpgrade));
			if (num < this.gildLevelBonusMap.Count)
			{
				result = this.gildLevelBonusMap[num];
			}
			else
			{
				result = this.gildLevelBonusMap[this.gildLevelBonusMap.Count - 1];
			}
		}
		return result;
	}

    // Token: 0x06000B2F RID: 2863 RVA: 0x00032A88 File Offset: 0x00030C88

    private void OnRewardEarned(IUnlockReward reward)
	{
		string value = "normal";
		if (reward is UnlockRewardVentureCooldownTime)
		{
			UnlockRewardVentureCooldownTime unlockRewardVentureCooldownTime = (UnlockRewardVentureCooldownTime)reward;
			if (unlockRewardVentureCooldownTime.affectedVenture == this.Id)
			{
				value = ((unlockRewardVentureCooldownTime.timeBonus > 1f) ? "bad" : "good");
			}
		}
		else if (reward is UnlockRewardEveryVentureCooldownTime)
		{
			value = ((((UnlockRewardEveryVentureCooldownTime)reward).timeBonus < 0f) ? "bad" : "good");
		}
		else if (reward is UnlockRewardVentureProfitPer)
		{
			UnlockRewardVentureProfitPer unlockRewardVentureProfitPer = (UnlockRewardVentureProfitPer)reward;
			if (unlockRewardVentureProfitPer.affectedVenture == this.Id)
			{
				value = ((unlockRewardVentureProfitPer.profitBonus < 1.0) ? "bad" : "good");
			}
		}
		else if (reward is UnlockRewardEveryVentureProfitPer)
		{
			value = ((((UnlockRewardEveryVentureProfitPer)reward).profitBonus < 1.0) ? "bad" : "good");
		}
		this.BonusEffect.Value = value;
	}

    // Token: 0x06000B30 RID: 2864 RVA: 0x00032B84 File Offset: 0x00030D84

    public double CalculateTotalGildBonus(int gildLevel)
	{
		double num = 1.0;
		if (gildLevel >= 0)
		{
			int num2 = Mathf.FloorToInt((float)(gildLevel / this.gildLevelsRequiredToUpgrade));
			for (int i = 1; i <= num2; i++)
			{
				if (i < this.gildLevelBonusMap.Count)
				{
					num *= this.gildLevelBonusMap[i];
				}
				else
				{
					num *= this.gildLevelBonusMap[this.gildLevelBonusMap.Count - 1];
				}
			}
		}
		return num;
	}

    // Token: 0x06000B31 RID: 2865 RVA: 0x00032BF8 File Offset: 0x00030DF8

    ~VentureModel()
	{
		this.Dispose();
	}

    // Token: 0x06000B32 RID: 2866 RVA: 0x00032C24 File Offset: 0x00030E24

    public void Dispose()
	{
		this._Disposables.Dispose();
		this.unlockStateDisposables.Dispose();
	}

    // Token: 0x06000B33 RID: 2867 RVA: 0x00032C3C File Offset: 0x00030E3C

    public VentureModel RegisterUpdateCall(IObservable<float> updateStream, ClampedReactiveDouble cashOnHand, Func<double, double> recalculateDeltaTime)
	{
		this.RecalculateDeltaTime = recalculateDeltaTime;
		updateStream.Subscribe(delegate(float time)
		{
			this.Update(time);
			double numOwned = this.NumOwned_Base.Value - (double)((this.Id == "lemon") ? 1 : 0);
			double num = (double)((int)this.CalculateAffordableAmount(this.CostPer.Value, this.ExpenseRate, numOwned, cashOnHand.Value));
			double value;
			if (GameController.Instance.BuyMultiplierService.BuyCount.Value > 0)
			{
				value = this.CalculateCost(this.CostPer.Value, this.ExpenseRate, numOwned, (double)GameController.Instance.BuyMultiplierService.BuyCount.Value);
				num = (double)GameController.Instance.BuyMultiplierService.BuyCount.Value;
			}
			else if (GameController.Instance.BuyMultiplierService.BuyCount.Value == -1)
			{
				if (num >= 1.0)
				{
					value = this.CalculateCost(this.CostPer.Value, this.ExpenseRate, numOwned, num);
				}
				else
				{
					value = this.CalculateCost(this.CostPer.Value, this.ExpenseRate, numOwned, 1.0);
					num = 1.0;
				}
			}
			else
			{
				int value2 = this.AchievementTarget.Value;
				if ((double)value2 <= this.TotalOwned.Value)
				{
					if (num >= 1.0)
					{
						num = 1.0;
						value = this.CalculateCost(this.CostPer.Value, this.ExpenseRate, numOwned, num);
					}
					else
					{
						value = this.CalculateCost(this.CostPer.Value, this.ExpenseRate, numOwned, 1.0);
						num = 1.0;
					}
				}
				else
				{
					double num2 = (double)value2 - this.TotalOwned.Value;
					value = this.CalculateCost(this.CostPer.Value, this.ExpenseRate, numOwned, num2);
					num = num2;
				}
			}
			this.CostForNext.Value = value;
			this.CanAfford.Value = num;
		}).AddTo(this._Disposables);
		return this;
	}

    // Token: 0x06000B34 RID: 2868 RVA: 0x00032C84 File Offset: 0x00030E84

    public double CalculateMaxCanAfford(double cashAmount)
	{
		double numOwned = this.NumOwned_Base.Value - (double)((this.Id == "lemon") ? 1 : 0);
		return this.CalculateAffordableAmount(this.CostPer.Value, this.ExpenseRate, numOwned, cashAmount);
	}

    // Token: 0x06000B35 RID: 2869 RVA: 0x00032CCE File Offset: 0x00030ECE

    private void OnItemEquipStateChanged(InventoryEquipMessage inventoryEquipMessage)
	{
		this.AdjustItemBonuses(inventoryEquipMessage.item, inventoryEquipMessage.equipped);
	}

    // Token: 0x06000B36 RID: 2870 RVA: 0x00032CE4 File Offset: 0x00030EE4

    public void AdjustItemBonuses(Item item, bool equipped)
	{
		if (item.ItemBonusTarget != ItemBonusTarget.Venture)
		{
			return;
		}
		if (!string.IsNullOrEmpty(item.BonusCustomData))
		{
			string[] array = item.BonusCustomData.Split(new char[]
			{
				':'
			});
			if (array.Length == 2 && array[1] != this.Id)
			{
				return;
			}
			if (array.Length == 1)
			{
				GameState game = GameController.Instance.game;
				if (game.IsEventPlanet)
				{
					if (array[0] != game.PlanetData.PlanetName && array[0] != "AllEvents")
					{
						return;
					}
				}
				else if (array[0] != game.PlanetData.PlanetName && array[0] != "AllPlanets")
				{
					return;
				}
			}
		}
		ItemBonusType itemBonusType = item.ItemBonusType;
		if (itemBonusType == ItemBonusType.VentureProfitBoost)
		{
			this.ventureMultiplierDisposable.Dispose();
			if (equipped)
			{
				this.equippedMultiplierItemBonuses.Add(item.CurrentLeveledBonus);
			}
			else
			{
				this.equippedMultiplierItemBonuses.Remove(item.CurrentLeveledBonus);
			}
			this.ventureMultiplierDisposable = (from v in this.equippedMultiplierItemBonuses
			select v).CombineLatest<float>().Subscribe(delegate(IList<float> v)
			{
				this.ventureMultiplierBonus.Value = v.Sum();
			});
			return;
		}
		if (itemBonusType == ItemBonusType.VentureSpeedBoost)
		{
			this.ventureSpeedDisposable.Dispose();
			if (equipped)
			{
				this.equippedSpeedItemBonuses.Add(item.CurrentLeveledBonus);
			}
			else
			{
				this.equippedSpeedItemBonuses.Remove(item.CurrentLeveledBonus);
			}
			this.ventureSpeedDisposable = (from v in this.equippedSpeedItemBonuses
			select v).CombineLatest<float>().Subscribe(delegate(IList<float> v)
			{
				this.speedItemBonus.Value = v.Sum();
			});
			return;
		}
		if (itemBonusType != ItemBonusType.VentureRebate)
		{
			return;
		}
		this.costReductionDisposable.Dispose();
		if (equipped)
		{
			this.equippedCostReductionItemBonuses.Add(item.CurrentLeveledBonus);
		}
		else
		{
			this.equippedCostReductionItemBonuses.Remove(item.CurrentLeveledBonus);
		}
		this.costReductionDisposable = (from v in this.equippedCostReductionItemBonuses
		select v).CombineLatest<float>().Subscribe(delegate(IList<float> v)
		{
			this.EffectiveCostReduction.Value = (double)(1f + v.Sum());
		});
	}

    // Token: 0x06000B37 RID: 2871 RVA: 0x00032F22 File Offset: 0x00031122

    public double CalculateCost(double initialCost, double expenseRate, double numOwned, double numberToBuy)
	{
		return initialCost * Math.Pow(expenseRate, numOwned) * (1.0 - Math.Pow(expenseRate, numberToBuy)) / (1.0 - expenseRate);
	}

    // Token: 0x06000B38 RID: 2872 RVA: 0x00032F4C File Offset: 0x0003114C

    public double CalculateCostFromZeroForUnlock(double numberToAchieve)
	{
		return this.CalculateCost(this.initialCostPer, this.ExpenseRate, 0.0, numberToAchieve);
	}

    // Token: 0x06000B39 RID: 2873 RVA: 0x00032F6A File Offset: 0x0003116A

    private double CalculateAffordableAmount(double initialCost, double expenseRate, double numOwned, double cashOnHand)
	{
		return Math.Floor(Math.Log(1.0 - cashOnHand * (1.0 - expenseRate) / (initialCost * Math.Pow(expenseRate, numOwned))) / Math.Log(expenseRate));
	}

    // Token: 0x06000B3A RID: 2874 RVA: 0x00032FA0 File Offset: 0x000311A0

    private void OnVentureCostChange()
	{
		double num = 1.0 - Math.Round(1.0 - 1.0 / this.EffectiveCostReduction.Value, 4);
		this.CostPer.Value = this.initialCostPer * this.AccountantEffect.Value * num;
		this.NumOwned_Base.SetValueAndForceNotify(this.NumOwned_Base.Value);
	}

    // Token: 0x06000B3B RID: 2875 RVA: 0x00033014 File Offset: 0x00031214

    public void Reset(int numOwned, double costPer, double profitPer, float coolDownTime)
	{
		this.IsManaged.Value = false;
		this.IsRunning.Value = false;
		this.CoolDownTime.Value = coolDownTime;
		this.ProgressTimer.Value = 0f;
		this.AccountantEffect.Value = 1.0;
		this.NumOwned_Base.Value = (double)numOwned;
		this.NumOwned_Upgrades.Value = 0.0;
		this.CostPer.Value = costPer;
		this.initialCostPer = costPer;
		this.ProfitPer.Value = profitPer;
		this.ShowCPS.Value = false;
		this.UnlockState.Value = ((this.UnlockState.Value == VentureModel.EUnlockState.Purchased) ? ((numOwned > 0) ? VentureModel.EUnlockState.Purchased : VentureModel.EUnlockState.Unlocked) : this.UnlockState.Value);
		this.MonitorUnlockState();
		this.OnVentureCostChange();
	}

    // Token: 0x06000B3C RID: 2876 RVA: 0x000330F0 File Offset: 0x000312F0

    public void Run()
	{
		this.IsRunning.Value = true;
	}

    // Token: 0x06000B3D RID: 2877 RVA: 0x00033100 File Offset: 0x00031300

    private void Update(float time)
	{
		if (this.lastTimeUpdateTime == 0f)
		{
			this.lastTimeUpdateTime = time;
		}
		float num = time - this.lastTimeUpdateTime;
		this.lastTimeUpdateTime = time;
		num = (float)this.RecalculateDeltaTime((double)num) * ((this.TimedBonusMultiplier.Value <= 0f) ? 1f : this.TimedBonusMultiplier.Value);
		List<VentureTimedSpeedBoost> list = new List<VentureTimedSpeedBoost>();
		DateTime utcNow = GameController.Instance.DateTimeService.UtcNow;
		foreach (VentureTimedSpeedBoost ventureTimedSpeedBoost in this.TimedSpeedBoosts)
		{
			if (utcNow >= ventureTimedSpeedBoost.expireTime)
			{
				list.Add(ventureTimedSpeedBoost);
				this.CoolDownTime.Value *= ventureTimedSpeedBoost.multiplier;
			}
		}
		foreach (VentureTimedSpeedBoost item in list)
		{
			this.TimedSpeedBoosts.Remove(item);
		}
		list.Clear();
		if (!this.IsRunning.Value)
		{
			return;
		}
		this.ProgressTimer.Value += num;
		if (this.ProgressTimer.Value < this.EffectiveCoolDownTime.Value)
		{
			return;
		}
		double value = this.ProfitOnNext.Value;
		double num2 = value;
		if (this.IsManaged.Value)
		{
			float num3 = Mathf.Floor(this.ProgressTimer.Value / this.EffectiveCoolDownTime.Value) - 1f;
			float value2 = this.ProgressTimer.Value % this.EffectiveCoolDownTime.Value;
			num2 += value * (double)num3;
			this.ProgressTimer.Value = value2;
		}
		else
		{
			this.ProgressTimer.Value = 0f;
			this.IsRunning.Value = false;
		}
		this._FinishedRunning(num2);
		if (this.IsManaged.Value)
		{
			this.Run();
		}
	}

    // Token: 0x06000B3E RID: 2878 RVA: 0x00033320 File Offset: 0x00031520

    public double ProfitSurgeAmount(double time, bool offline = false)
	{
		if (!this.IsRunning.Value)
		{
			return 0.0;
		}
		double num = 0.0;
		if (offline)
		{
			bool flag = (double)this.ProgressTimer.Value + time >= (double)this.EffectiveCoolDownTime.Value;
			if (!this.IsManaged.Value)
			{
				this.IsRunning.Value = !flag;
				this.ProgressTimer.Value = (flag ? 0f : (this.ProgressTimer.Value + (float)time));
				if (flag)
				{
					num += 1.0;
				}
			}
			else
			{
				double num2 = this.TimesFinished((double)this.ProgressTimer.Value + time);
				this.ProgressTimer.Value = (flag ? (this.ProgressTimer.Value + (float)time - this.EffectiveCoolDownTime.Value * (float)num2) : (this.ProgressTimer.Value + (float)time));
				num += num2;
			}
		}
		if (!this.IsManaged.Value && this.IsRunning.Value && time > (double)this.EffectiveCoolDownTime.Value)
		{
			time = (double)this.EffectiveCoolDownTime.Value;
		}
		if (!offline)
		{
			num += this.TimesFinished(time);
		}
		if (!this.IsManaged.Value && num > 1.0)
		{
			num = 1.0;
		}
		return num * this.ProfitOnNext.Value;
	}

    // Token: 0x06000B3F RID: 2879 RVA: 0x0003348C File Offset: 0x0003168C

    private double TimesFinished(double time)
	{
		return Math.Truncate(time / (double)this.EffectiveCoolDownTime.Value);
	}

    // Token: 0x06000B40 RID: 2880 RVA: 0x000334A1 File Offset: 0x000316A1

    private void OnMicromanagerBoosted(MicroManagerService.MicroManagerBoostValues values)
	{
		this.SetTimedMultiplier(values.managerBonusDuration, values.effectiveManagerBonusMultiplier);
	}

    // Token: 0x06000B41 RID: 2881 RVA: 0x000334B8 File Offset: 0x000316B8

    public void SetTimedMultiplier(float bonusDuration, float bonusMultiplier)
	{
		if (this.timerDisposable != null)
		{
			this.timerDisposable.Dispose();
		}
		this.TimedBonusMultiplier.Value = bonusMultiplier;
		this.timerDisposable = Observable.Timer(TimeSpan.FromSeconds((double)bonusDuration)).Subscribe(delegate(long _)
		{
			this.TimedBonusMultiplier.Value = 0f;
		}).AddTo(this._Disposables);
	}

    // Token: 0x06000B42 RID: 2882 RVA: 0x00033512 File Offset: 0x00031712

    public void AddTimedSpeedBonus(string id, float bonusSpeedPercentage, float bonusTimeInSeconds)
	{
		this.TimedSpeedBoosts.Add(new VentureTimedSpeedBoost(id, bonusSpeedPercentage, bonusTimeInSeconds, GameController.Instance.DateTimeService.UtcNow));
		this.CoolDownTime.Value /= bonusSpeedPercentage;
	}

    // Token: 0x06000B43 RID: 2883 RVA: 0x0003354C File Offset: 0x0003174C

    public void RemoveTimedSpeedBonus(string id)
	{
		VentureTimedSpeedBoost ventureTimedSpeedBoost = this.TimedSpeedBoosts.Find(b => b.id == id);
		if (ventureTimedSpeedBoost != null)
		{
			this.TimedSpeedBoosts.Remove(ventureTimedSpeedBoost);
		}
	}

    // Token: 0x06000B44 RID: 2884 RVA: 0x00033590 File Offset: 0x00031790

    public void AddTimeToTimedSpeedBonus(string id, float additionalTimeInSeconds)
	{
		VentureTimedSpeedBoost ventureTimedSpeedBoost = this.TimedSpeedBoosts.Find(b => b.id == id);
		if (ventureTimedSpeedBoost != null)
		{
			ventureTimedSpeedBoost.expireTime = ventureTimedSpeedBoost.expireTime.AddSeconds((double)additionalTimeInSeconds);
			ventureTimedSpeedBoost.multiplyerTimeInSeconds += additionalTimeInSeconds;
		}
	}

    // Token: 0x06000B45 RID: 2885 RVA: 0x000335E8 File Offset: 0x000317E8

    public void LoadSavedData(VentureSaveData data)
	{
		this.NumOwned_Base.Value = data.numOwned;
		this.ProgressTimer.Value = data.timeRun;
		this.IsBoosted.Value = data.isBoosted;
		this.gildLevel.Value = data.gildLevel;
		this.UnlockState.Value = data.unlockState;
		this.MonitorUnlockState();
		if (this.ProgressTimer.Value > 0f)
		{
			this.Run();
		}
	}

    // Token: 0x06000B46 RID: 2886 RVA: 0x00033668 File Offset: 0x00031868

    public VentureSaveData Save()
	{
		return new VentureSaveData(this.Id, this.NumOwned_Base.Value, this.ProgressTimer.Value, this.IsBoosted.Value, this.gildLevel.Value, this.UnlockState.Value);
	}

    // Token: 0x04000958 RID: 2392

    public ReactiveProperty<VentureModel.EUnlockState> UnlockState = new ReactiveProperty<VentureModel.EUnlockState>(VentureModel.EUnlockState.Hidden);

    // Token: 0x04000959 RID: 2393

    public string UnlockTargetAmount;

    // Token: 0x0400095A RID: 2394

    public Sprite UnlockTargetSprite;

    // Token: 0x0400095B RID: 2395

    private Action<double> _FinishedRunning;

    private IObservable<double> goldBoostedVentureBonusValue = new ReactiveProperty<double>(7.77);

    // Token: 0x04000962 RID: 2402

    private ReactiveProperty<double> totalVentureBoostBonus = new ReactiveProperty<double>(1.0);

    // Token: 0x04000964 RID: 2404

    public DoubleReactiveProperty AccountantEffect = new DoubleReactiveProperty(1.0);

    // Token: 0x04000965 RID: 2405

    public BoolReactiveProperty ShowCPS = new BoolReactiveProperty();

    // Token: 0x04000966 RID: 2406

    public BoolReactiveProperty IsRunning = new BoolReactiveProperty(false);

    // Token: 0x04000967 RID: 2407

    public BoolReactiveProperty IsManaged = new BoolReactiveProperty(false);

    // Token: 0x04000968 RID: 2408

    public DoubleReactiveProperty NumOwned_Base = new DoubleReactiveProperty(0.0);

    // Token: 0x04000969 RID: 2409

    public DoubleReactiveProperty NumOwned_Upgrades = new DoubleReactiveProperty(0.0);

    // Token: 0x0400096A RID: 2410

    public readonly ReactiveProperty<double> TotalOwned = new ReactiveProperty<double>();

    // Token: 0x0400096B RID: 2411

    public DoubleReactiveProperty CostPer = new DoubleReactiveProperty();

    // Token: 0x0400096C RID: 2412

    public DoubleReactiveProperty ProfitPer = new DoubleReactiveProperty();

    // Token: 0x0400096D RID: 2413

    public FloatReactiveProperty CoolDownTime = new FloatReactiveProperty(0f);

    // Token: 0x0400096E RID: 2414

    public FloatReactiveProperty ProgressTimer = new FloatReactiveProperty();

    // Token: 0x0400096F RID: 2415

    public readonly ReactiveProperty<int> AchievementTarget = new ReactiveProperty<int>(0);

    // Token: 0x04000970 RID: 2416

    public IntReactiveProperty LastAchievementTarget = new IntReactiveProperty(0);

    // Token: 0x04000971 RID: 2417

    public readonly ReactiveProperty<string> BonusEffect = new ReactiveProperty<string>();

    // Token: 0x04000972 RID: 2418

    public BoolReactiveProperty IsBoosted = new BoolReactiveProperty(false);

    // Token: 0x04000973 RID: 2419

    public BoolReactiveProperty IsProfitBoosted = new BoolReactiveProperty(false);

    // Token: 0x04000974 RID: 2420

    public DoubleReactiveProperty CostForNext = new DoubleReactiveProperty(0.0);

    // Token: 0x04000975 RID: 2421

    public DoubleReactiveProperty CanAfford = new DoubleReactiveProperty(0.0);

    // Token: 0x04000976 RID: 2422

    public FloatReactiveProperty TimedBonusMultiplier = new FloatReactiveProperty(0f);

    // Token: 0x04000977 RID: 2423

    private List<VentureTimedSpeedBoost> TimedSpeedBoosts = new List<VentureTimedSpeedBoost>();

    // Token: 0x04000978 RID: 2424

    public IntReactiveProperty gildLevel = new IntReactiveProperty(0);

    // Token: 0x04000979 RID: 2425

    public List<double> gildLevelBonusMap = new List<double>(new double[]
	{
		1.0,
		7.77,
		17.77,
		777.77,
		777.77,
		777.77,
		1000.0,
		2000.0
	});

    // Token: 0x0400097A RID: 2426

    public int gildLevelsRequiredToUpgrade = 1;

    // Token: 0x0400097C RID: 2428

    public ReactiveProperty<double> EffectiveCostReduction = new ReactiveProperty<double>(1.0);

    // Token: 0x0400097F RID: 2431

    public readonly ReactiveProperty<float> AchievementProgressBarVal = new ReactiveProperty<float>();

    // Token: 0x04000980 RID: 2432

    public readonly ReactiveProperty<int> UnlocksToClaim = new ReactiveProperty<int>(0);

    // Token: 0x04000981 RID: 2433

    private float lastTimeUpdateTime;

    // Token: 0x04000983 RID: 2435

    private IDisposable timerDisposable;

    // Token: 0x04000984 RID: 2436

    private CompositeDisposable _Disposables = new CompositeDisposable();

    // Token: 0x04000985 RID: 2437

    private List<ReactiveProperty<float>> equippedMultiplierItemBonuses = new List<ReactiveProperty<float>>();

    // Token: 0x04000986 RID: 2438

    private ReactiveProperty<float> ventureMultiplierBonus = new ReactiveProperty<float>(0f);

    // Token: 0x04000987 RID: 2439

    private IDisposable ventureMultiplierDisposable = Disposable.Empty;

    // Token: 0x04000988 RID: 2440

    private List<ReactiveProperty<float>> equippedSpeedItemBonuses = new List<ReactiveProperty<float>>();

    // Token: 0x04000989 RID: 2441

    private ReactiveProperty<float> speedItemBonus = new ReactiveProperty<float>();

    // Token: 0x0400098A RID: 2442

    private IDisposable ventureSpeedDisposable = Disposable.Empty;

    // Token: 0x0400098B RID: 2443

    private List<ReactiveProperty<float>> equippedCostReductionItemBonuses = new List<ReactiveProperty<float>>();

    // Token: 0x0400098C RID: 2444

    private IDisposable costReductionDisposable = Disposable.Empty;

    // Token: 0x0400098D RID: 2445

    private double initialCostPer;

    // Token: 0x0400098E RID: 2446

    private List<TriggerData> visableTriggers = new List<TriggerData>();

    // Token: 0x0400098F RID: 2447

    private List<TriggerData> unlockTriggers = new List<TriggerData>();

    // Token: 0x04000990 RID: 2448

    private CompositeDisposable unlockStateDisposables = new CompositeDisposable();


    public VentureModel(Venture venture, IObservable<double> angelBonus, IObservable<float> totalProfitMultiplier, BoolReactiveProperty isPlatinumBoosted, IObservable<double> platinumBoostedVentureBonus, List<TriggerData> visableTriggers, List<TriggerData> unlockTriggers) : this(venture.id, venture.name, venture.baseAmount, venture.imageName, venture.costPer, venture.profitPer, (float)venture.cooldownTime, venture.expenseRate, venture.plural, venture.bonusName, angelBonus, totalProfitMultiplier, isPlatinumBoosted, platinumBoostedVentureBonus, visableTriggers, unlockTriggers)
    {
    }

    // Token: 0x02000857 RID: 2135
    public enum EUnlockState
	{
		// Token: 0x04002A9D RID: 10909
		Hidden,
		// Token: 0x04002A9E RID: 10910
		Locked,
		// Token: 0x04002A9F RID: 10911
		Unlocked,
		// Token: 0x04002AA0 RID: 10912
		Purchased
	}
}
