using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using UniRx;
using UnityEngine;
using Utils;

// Token: 0x020001F5 RID: 501
[Serializable]
public class ProfitBooster
{
	// Token: 0x17000139 RID: 313
	// (get) Token: 0x06000E98 RID: 3736 RVA: 0x0004157F File Offset: 0x0003F77F
	// (set) Token: 0x06000E99 RID: 3737 RVA: 0x00041587 File Offset: 0x0003F787
	public ReadOnlyReactiveProperty<float> TotalBoost { get; set; }

	// Token: 0x06000E9A RID: 3738 RVA: 0x00041590 File Offset: 0x0003F790
	public ProfitBooster(GameState game, ProfitBoostSetup setup, IObservable<Unit> onDeployClick, IObservable<Unit> onFightBackClick, ProfitBoosterType profitBoosterType)
	{
		this.game = game;
		this.setup = setup;
		this.profitBoosterType = profitBoosterType;
		this.boosterStateTapCounter = 0;
		this.TotalBoost = this.boostEffect.CombineLatest(this.ProfitBoosterBoost, (int bbe, float pbb) => (float)bbe + pbb).CombineLatest(this.boostAffectorEffect, (float tb, int bae) => tb + (float)bae).CombineLatest(this.itemEffectBonus, (float tb, float ieb) => tb * (1f + ieb)).ToReadOnlyReactiveProperty<float>();
		(from _ in this.TotalBoost
		where this.CurrentState.Value == ProfitBoosterState.Active && this.setup.style == ProfitBoostSetup.BoostStyle.all
		select _).Subscribe(delegate(float b)
		{
			this.game.SetMultiplierBonus(GameState.MultiplierBonusType.ProfitBoost, this.TotalBoost.Value);
		}).AddTo(this._Disposables);
		this.equippedItemEffectItemBonuses.Add(new ReactiveProperty<float>(0f));
		this.equippedItemCooldownItemBonuses.Add(new ReactiveProperty<float>(0f));
		onDeployClick.Subscribe(delegate(Unit _)
		{
			this.DeploySetup();
			this.Deploy();
		}).AddTo(this._Disposables);
		onFightBackClick.Subscribe(delegate(Unit _)
		{
			this.OnFightBack();
		}).AddTo(this._Disposables);
		(from x in MessageBroker.Default.Receive<StorePurchaseEvent>()
		where x.PurchaseState == EStorePurchaseState.Success
		select x).Subscribe(new Action<StorePurchaseEvent>(this.OnStoreItemPurchased)).AddTo(this._Disposables);
		Observable.Interval(TimeSpan.FromMilliseconds(500.0)).Subscribe(delegate(long _)
		{
			this.Save();
		}).AddTo(this._Disposables);
		this.CurrentState.Subscribe(delegate(ProfitBoosterState _)
		{
			this.CalculateDuration();
		}).AddTo(this._Disposables);
		Observable.EveryApplicationPause().Subscribe(new Action<bool>(this.OnApplicationPause)).AddTo(this._Disposables);
		Observable.OnceApplicationQuit().Subscribe(delegate(Unit _)
		{
			this.OnApplicationQuit();
		}).AddTo(this._Disposables);
		MessageBroker.Default.Receive<InventoryEquipMessage>().Subscribe(new Action<InventoryEquipMessage>(this.OnItemEquipStateChanged)).AddTo(this._Disposables);
		foreach (Item item in GameController.Instance.GlobalPlayerData.inventory.GetAllEquippedItems())
		{
			this.AdjustItemBonuses(item, true);
		}
		this.DeploySetup();
		this.Load();
		GameController.Instance.TimerService.GetTimer(TimerService.TimerGroups.State).Subscribe(delegate(TimeSpan time)
		{
			this.UpdateTime(time.TotalSeconds);
		}).AddTo(this._Disposables);
		this.ResetBoosterAnalytic();
		this.SendBoosterAnalytic("PlanetLoaded");
	}

	// Token: 0x06000E9B RID: 3739 RVA: 0x00041954 File Offset: 0x0003FB54
	private void OnStoreItemPurchased(StorePurchaseEvent e)
	{
		IInventoryService inventory = GameController.Instance.GlobalPlayerData.inventory;
		for (int i = 0; i < e.Item.Rewards.Count; i++)
		{
			if (e.Item.Rewards[i].RewardType == ERewardType.Item)
			{
				Item itemById = inventory.GetItemById(e.Item.Rewards[i].Id);
				if (itemById != null && (itemById.Product == Product.ProfitMartiansBoost || itemById.Product == Product.ProfitBoosterBoost))
				{
					this.ProfitBoosterBoost.Value += (float)e.Item.Rewards[i].Qty;
					this.CalculateDuration();
					this.Save();
				}
			}
		}
	}

	// Token: 0x06000E9C RID: 3740 RVA: 0x00041A14 File Offset: 0x0003FC14
	private void UpdateTime(double timePassed)
	{
		this.CurrentTime.Value += timePassed;
		switch (this.CurrentState.Value)
		{
		case ProfitBoosterState.Ready:
			this.TimeLeftString.Value = "READY";
			return;
		case ProfitBoosterState.Active:
			if (this.CurrentTime.Value >= this.CurrentDuration.Value)
			{
				this.ChangeState(ProfitBoosterState.Recharging);
				this.CurrentTime.Value = 0.0;
				this.RemoveBonus();
				return;
			}
			this.SetTimeString();
			this.TimeLeftPercent.Value = (float)(1.0 - this.CurrentTime.Value / this.CurrentDuration.Value);
			return;
		case ProfitBoosterState.Recharging:
			if (this.CurrentTime.Value >= this.CurrentDuration.Value)
			{
				this.ChangeState(ProfitBoosterState.Ready);
				return;
			}
			this.SetTimeString();
			this.TimeLeftPercent.Value = (float)(this.CurrentTime.Value / this.CurrentDuration.Value);
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	// Token: 0x06000E9D RID: 3741 RVA: 0x00041B28 File Offset: 0x0003FD28
	private void ChangeState(ProfitBoosterState newBoosterState)
	{
		ProfitBoosterState value = this.CurrentState.Value;
		this.CurrentState.Value = newBoosterState;
		this.PrepareBoosterAnalytic(value, newBoosterState);
	}

	// Token: 0x06000E9E RID: 3742 RVA: 0x00041B58 File Offset: 0x0003FD58
	private void SetTimeString()
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(this.CurrentDuration.Value - this.CurrentTime.Value);
		this.TimeLeftString.Value = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
	}

	// Token: 0x06000E9F RID: 3743 RVA: 0x00041BBB File Offset: 0x0003FDBB
	private void OnItemEquipStateChanged(InventoryEquipMessage inventoryEquipMessage)
	{
		this.AdjustItemBonuses(inventoryEquipMessage.item, inventoryEquipMessage.equipped);
	}

	// Token: 0x06000EA0 RID: 3744 RVA: 0x00041BD0 File Offset: 0x0003FDD0
	public void AdjustItemBonuses(Item item, bool equipped)
	{
		if (item.ItemBonusTarget != ItemBonusTarget.Booster)
		{
			return;
		}
		if (!string.IsNullOrEmpty(item.BonusCustomData))
		{
			string[] array = item.BonusCustomData.Split(new char[]
			{
				':'
			});
			if (array.Length == 1)
			{
				GameState gameState = GameController.Instance.game;
				if (gameState.IsEventPlanet)
				{
					if (array[0] != gameState.PlanetData.PlanetName && array[0] != "AllEvents")
					{
						return;
					}
				}
				else if (array[0] != gameState.PlanetData.PlanetName && array[0] != "AllPlanets")
				{
					return;
				}
			}
		}
		ItemBonusType itemBonusType = item.ItemBonusType;
		if (itemBonusType != ItemBonusType.BoosterRecharge)
		{
			if (itemBonusType == ItemBonusType.BoosterBonus)
			{
				this.itemEffectDisposable.Dispose();
				if (equipped)
				{
					this.equippedItemEffectItemBonuses.Add(item.CurrentLeveledBonus);
				}
				else
				{
					this.equippedItemEffectItemBonuses.Remove(item.CurrentLeveledBonus);
				}
				this.itemEffectDisposable = (from v in this.equippedItemEffectItemBonuses
				select v).CombineLatest<float>().Subscribe(delegate(IList<float> v)
				{
					this.itemEffectBonus.Value = v.Sum();
				});
				return;
			}
		}
		else
		{
			this.itemCooldownDisposable.Dispose();
			if (equipped)
			{
				this.equippedItemCooldownItemBonuses.Add(item.CurrentLeveledBonus);
			}
			else
			{
				this.equippedItemCooldownItemBonuses.Remove(item.CurrentLeveledBonus);
			}
			this.itemCooldownDisposable = (from v in this.equippedItemCooldownItemBonuses
			select v).CombineLatest<float>().Subscribe(delegate(IList<float> v)
			{
				this.itemCooldownBonus.Value = v.Sum();
				this.CalculateDuration();
			});
		}
	}

	// Token: 0x06000EA1 RID: 3745 RVA: 0x00041D78 File Offset: 0x0003FF78
	private void OnFightBack()
	{
		if (this.CurrentState.Value != ProfitBoosterState.Ready)
		{
			this.boosterStateTapCounter++;
		}
		if (this.CurrentState.Value != ProfitBoosterState.Active)
		{
			this.CurrentTime.Value += (double)this.setup.fightBackEffect;
			return;
		}
		if (this.CurrentTime.Value > (double)this.setup.fightBackEffect)
		{
			this.CurrentTime.Value = this.CurrentTime.Value - (double)this.setup.fightBackEffect;
			return;
		}
		this.CurrentTime.Value = 0.0;
	}

	// Token: 0x06000EA2 RID: 3746 RVA: 0x00041E20 File Offset: 0x00040020
	private void CalculateDuration()
	{
		switch (this.CurrentState.Value)
		{
		case ProfitBoosterState.Ready:
			this.TimeLeftPercent.Value = 1f;
			return;
		case ProfitBoosterState.Active:
			this.CurrentDuration.Value = this.baseActiveDuration;
			return;
		case ProfitBoosterState.Recharging:
		{
			double num = 1.0;
			if (this.CurrentDuration.Value > 0.0)
			{
				num = this.CurrentTime.Value / this.CurrentDuration.Value;
			}
			this.CurrentDuration.Value = this.baseRechargeDuration / (double)Mathf.Pow(2f, this.ProfitBoosterBoost.Value);
			this.CurrentDuration.Value /= (double)(1f + this.itemCooldownBonus.Value);
			this.CurrentTime.Value = num * this.CurrentDuration.Value;
			return;
		}
		default:
			return;
		}
	}

	// Token: 0x06000EA3 RID: 3747 RVA: 0x00041F0C File Offset: 0x0004010C
	private void DeploySetup()
	{
		this.boostEffect.Value = Random.Range(this.setup.minBoostEffect, this.setup.maxBoostEffect);
		this.baseActiveDuration = (double)Random.Range(this.setup.minEffectTime, this.setup.maxEffectTime);
		this.baseRechargeDuration = (double)Random.Range(this.setup.minRechargeTime, this.setup.maxRechargeTime);
		this.ventureDiscount = Mathf.Round(Random.Range(this.setup.minDiscount, this.setup.maxDiscount));
		if (this.setup.style == ProfitBoostSetup.BoostStyle.random)
		{
			List<VentureModel> list = (from m in this.game.VentureModels
			where m.TotalOwned.Value > 0.0
			select m).ToList<VentureModel>();
			this.affectedVentureId = list[Random.Range(0, list.Count)].Id;
		}
		this.CalculateDuration();
	}

	// Token: 0x06000EA4 RID: 3748 RVA: 0x00042010 File Offset: 0x00040210
	private void Deploy()
	{
		this.CurrentTime.Value = 1.0;
		this.CurrentDuration.Value = this.baseRechargeDuration;
		this.CurrentState.Value = ProfitBoosterState.Recharging;
		this.Save();
		this.CurrentTime.Value = 0.0;
		if (this.profitBoosterType == ProfitBoosterType.ProfitMartians && !GameController.Instance.GlobalPlayerData.GetBool("hasProfitMartianBeenRun"))
		{
			GameController.Instance.game.hasProfitMartiansBeenRun.Value = true;
		}
		this.ChangeState(ProfitBoosterState.Active);
		ProfitBoostSetup.BoostStyle style = this.setup.style;
		if (style != ProfitBoostSetup.BoostStyle.all)
		{
			if (style != ProfitBoostSetup.BoostStyle.random)
			{
				return;
			}
		}
		else
		{
			this.game.SetMultiplierBonus(GameState.MultiplierBonusType.ProfitBoost, this.TotalBoost.Value);
			using (IEnumerator<VentureModel> enumerator = this.game.VentureModels.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VentureModel ventureModel = enumerator.Current;
					ventureModel.CostPer.Value *= (double)(this.ventureDiscount / 100f);
					float num = this.ventureDiscount;
					ventureModel.IsProfitBoosted.Value = true;
				}
				return;
			}
		}
		VentureModel ventureModel2 = this.game.VentureModels.FirstOrDefault((VentureModel v) => v.Id == this.affectedVentureId);
		ventureModel2.ProfitPer.Value *= (double)this.TotalBoost.Value;
		ventureModel2.CostPer.Value *= (double)(this.ventureDiscount / 100f);
		float num2 = this.ventureDiscount;
		ventureModel2.IsProfitBoosted.Value = true;
	}

	// Token: 0x06000EA5 RID: 3749 RVA: 0x000421A8 File Offset: 0x000403A8
	private void RemoveBonus()
	{
		if (this.setup.style == ProfitBoostSetup.BoostStyle.all)
		{
			this.game.RemoveMultiplierBonus(GameState.MultiplierBonusType.ProfitBoost);
			using (IEnumerator<VentureModel> enumerator = this.game.VentureModels.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VentureModel ventureModel = enumerator.Current;
					ventureModel.CostPer.Value /= (double)(this.ventureDiscount / 100f);
					ventureModel.IsProfitBoosted.Value = false;
				}
				return;
			}
		}
		VentureModel ventureModel2 = this.game.VentureModels.FirstOrDefault((VentureModel v) => v.Id == this.affectedVentureId);
		ventureModel2.ProfitPer.Value /= (double)this.TotalBoost.Value;
		ventureModel2.CostPer.Value /= (double)(this.ventureDiscount / 100f);
		ventureModel2.IsProfitBoosted.Value = false;
	}

	// Token: 0x06000EA6 RID: 3750 RVA: 0x0004229C File Offset: 0x0004049C
	private void Save()
	{
		if (this.CurrentState.Value != ProfitBoosterState.Active)
		{
			this.game.planetPlayerData.Set(this.PROFIT_MARTIANS_BOOSTS_KEY, this.ProfitBoosterBoost.Value.ToString());
			this.game.planetPlayerData.Set("profitBoostRechargeTime", this.CurrentTime.Value.ToString());
			this.game.planetPlayerData.Set("profitBoostCurrentDuration", this.CurrentDuration.Value.ToString());
			this.game.planetPlayerData.Set("pbstate" + this.game.planetName, this.CurrentState.Value.ToString());
		}
	}

	// Token: 0x06000EA7 RID: 3751 RVA: 0x00042378 File Offset: 0x00040578
	private void Load()
	{
		this.ProfitBoosterBoost.Value += (float)this.game.planetPlayerData.GetDouble(this.PROFIT_MARTIANS_BOOSTS_KEY, 0.0);
		this.CurrentTime.Value = this.game.planetPlayerData.GetDouble("profitBoostRechargeTime", 0.0);
		this.CurrentDuration.Value = (double)((float)this.game.planetPlayerData.GetDouble("profitBoostCurrentDuration", 0.0));
		this.CurrentState.Value = (ProfitBoosterState)Enum.Parse(typeof(ProfitBoosterState), this.game.planetPlayerData.Get("pbstate" + this.game.planetName, "Ready"));
	}

	// Token: 0x06000EA8 RID: 3752 RVA: 0x00042454 File Offset: 0x00040654
	public void Dispose()
	{
		this.SendBoosterAnalytic("LeavePlanet");
		this._Disposables.Dispose();
	}

	// Token: 0x06000EA9 RID: 3753 RVA: 0x0004246C File Offset: 0x0004066C
	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			this.SendBoosterAnalytic("Pause");
			if (this.CurrentState.Value == ProfitBoosterState.Active)
			{
				this.CurrentDuration.Value = this.baseRechargeDuration;
				this.CurrentTime.Value = 1.0;
				this.Save();
			}
			if (this.CurrentState.Value == ProfitBoosterState.Active)
			{
				this.ChangeState(ProfitBoosterState.Recharging);
				this.RemoveBonus();
				this.Save();
			}
		}
	}

	// Token: 0x06000EAA RID: 3754 RVA: 0x000424E4 File Offset: 0x000406E4
	private void OnApplicationQuit()
	{
		this.SendBoosterAnalytic("Quit");
		if (this.CurrentState.Value == ProfitBoosterState.Active)
		{
			this.CurrentDuration.Value = this.baseRechargeDuration;
			this.CurrentTime.Value = 1.0;
		}
		if (this.CurrentState.Value == ProfitBoosterState.Active)
		{
			this.ChangeState(ProfitBoosterState.Recharging);
			this.RemoveBonus();
			this.Save();
		}
	}

	// Token: 0x06000EAB RID: 3755 RVA: 0x00042550 File Offset: 0x00040750
	private void SetupNotification()
	{
		if (this.CurrentState.Value != ProfitBoosterState.Recharging)
		{
			return;
		}
		DateTime scheduleDate = DateTime.Now.AddSeconds(this.baseRechargeDuration - this.CurrentTime.Value);
		Notifications.Schedule(this.setup.notificationTitle, this.setup.notificationBody, scheduleDate, 8);
	}

	// Token: 0x06000EAC RID: 3756 RVA: 0x000425AC File Offset: 0x000407AC
	private void PrepareBoosterAnalytic(ProfitBoosterState prevState, ProfitBoosterState currentState)
	{
		if (prevState == ProfitBoosterState.Ready && currentState == ProfitBoosterState.Active)
		{
			this.ResetBoosterAnalytic();
			this.SendBoosterAnalytic("Activated");
		}
		if (prevState == ProfitBoosterState.Recharging && currentState == ProfitBoosterState.Ready)
		{
			this.SendBoosterAnalytic("RechargeComplete");
			this.ResetBoosterAnalytic();
		}
		if (prevState == ProfitBoosterState.Active && currentState == ProfitBoosterState.Recharging)
		{
			this.SendBoosterAnalytic("BoostComplete");
			this.ResetBoosterAnalytic();
		}
	}

	// Token: 0x06000EAD RID: 3757 RVA: 0x00042604 File Offset: 0x00040804
	private void SendBoosterAnalytic(string status)
	{
		DateTime.UtcNow - this.boostCounterStartTime;
		float num = (float)this.boosterStateTapCounter * this.setup.fightBackEffect;
		if (this.boosterStateTapCounter > 0)
		{
			string taskDescription = string.Format("TapSecsTimeGain:{0} State:{1}", num, this.CurrentState.Value);
			GameController.Instance.AnalyticService.SendTaskCompleteEvent("ProfitBoosterState", status, taskDescription);
		}
	}

	// Token: 0x06000EAE RID: 3758 RVA: 0x00042676 File Offset: 0x00040876
	private void ResetBoosterAnalytic()
	{
		this.boostCounterStartTime = DateTime.UtcNow;
		this.boosterStateTapCounter = 0;
	}

	// Token: 0x04000C42 RID: 3138
	private const string PROFIT_BOOST_CURRENT_TIME_PREFS = "profitBoostRechargeTime";

	// Token: 0x04000C43 RID: 3139
	private const string PROFIT_BOOST_CURRENT_DURATION_PREFS = "profitBoostCurrentDuration";

	// Token: 0x04000C44 RID: 3140
	private const string PROFIT_BOOST_STATE_PREFS = "pbstate";

	// Token: 0x04000C45 RID: 3141
	private string PROFIT_MARTIANS_BOOSTS_KEY = "ProfitMartiansBoosts";

	// Token: 0x04000C46 RID: 3142
	private GameState game;

	// Token: 0x04000C47 RID: 3143
	private ProfitBoostSetup setup;

	// Token: 0x04000C48 RID: 3144
	private float ventureDiscount;

	// Token: 0x04000C49 RID: 3145
	private string affectedVentureId;

	// Token: 0x04000C4A RID: 3146
	private double baseRechargeDuration;

	// Token: 0x04000C4B RID: 3147
	private double baseActiveDuration;

	// Token: 0x04000C4C RID: 3148
	private ProfitBoosterType profitBoosterType;

	// Token: 0x04000C4D RID: 3149
	private ReactiveProperty<int> boostEffect = new ReactiveProperty<int>(0);

	// Token: 0x04000C4E RID: 3150
	private ReactiveProperty<int> boostAffectorEffect = new ReactiveProperty<int>(0);

	// Token: 0x04000C4F RID: 3151
	private ReactiveProperty<float> itemEffectBonus = new ReactiveProperty<float>();

	// Token: 0x04000C50 RID: 3152
	private List<ReactiveProperty<float>> equippedItemEffectItemBonuses = new List<ReactiveProperty<float>>();

	// Token: 0x04000C51 RID: 3153
	private IDisposable itemEffectDisposable = Disposable.Empty;

	// Token: 0x04000C52 RID: 3154
	private ReactiveProperty<float> itemCooldownBonus = new ReactiveProperty<float>();

	// Token: 0x04000C53 RID: 3155
	private List<ReactiveProperty<float>> equippedItemCooldownItemBonuses = new List<ReactiveProperty<float>>();

	// Token: 0x04000C54 RID: 3156
	private IDisposable itemCooldownDisposable = Disposable.Empty;

	// Token: 0x04000C55 RID: 3157
	public FloatReactiveProperty ProfitBoosterBoost = new FloatReactiveProperty(0f);

	// Token: 0x04000C57 RID: 3159
	public ReactiveProperty<string> TimeLeftString = new ReactiveProperty<string>("READY!");

	// Token: 0x04000C58 RID: 3160
	public ReactiveProperty<float> TimeLeftPercent = new ReactiveProperty<float>(1f);

	// Token: 0x04000C59 RID: 3161
	public ReactiveProperty<ProfitBoosterState> CurrentState = new ReactiveProperty<ProfitBoosterState>(ProfitBoosterState.Ready);

	// Token: 0x04000C5A RID: 3162
	private ReactiveProperty<double> CurrentDuration = new ReactiveProperty<double>();

	// Token: 0x04000C5B RID: 3163
	private ReactiveProperty<double> CurrentTime = new ReactiveProperty<double>();

	// Token: 0x04000C5C RID: 3164
	private CompositeDisposable _Disposables = new CompositeDisposable();

	// Token: 0x04000C5D RID: 3165
	private int boosterStateTapCounter;

	// Token: 0x04000C5E RID: 3166
	private DateTime boostCounterStartTime;
}
