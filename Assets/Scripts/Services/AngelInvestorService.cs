using System;
using System.Collections.Generic;
using System.Linq;
using Platforms;
using UniRx;
using UnityEngine;

// Token: 0x02000048 RID: 72
public class AngelInvestorService : IAngelInvestorService, IDisposable
{
	// Token: 0x1700001D RID: 29
	// (get) Token: 0x060001FD RID: 509 RVA: 0x0000C1AA File Offset: 0x0000A3AA
	// (set) Token: 0x060001FE RID: 510 RVA: 0x0000C1B2 File Offset: 0x0000A3B2
	public double angelResetThreshold { get; private set; }

	// Token: 0x1700001E RID: 30
	// (get) Token: 0x060001FF RID: 511 RVA: 0x0000C1BB File Offset: 0x0000A3BB
	// (set) Token: 0x06000200 RID: 512 RVA: 0x0000C1C3 File Offset: 0x0000A3C3
	public double eventAngelResetThreshold { get; private set; }

	// Token: 0x1700001F RID: 31
	// (get) Token: 0x06000201 RID: 513 RVA: 0x0000C1CC File Offset: 0x0000A3CC
	// (set) Token: 0x06000202 RID: 514 RVA: 0x0000C1D4 File Offset: 0x0000A3D4
	public ReactiveProperty<double> AngelsOnHand { get; private set; }

	// Token: 0x17000020 RID: 32
	// (get) Token: 0x06000203 RID: 515 RVA: 0x0000C1DD File Offset: 0x0000A3DD
	// (set) Token: 0x06000204 RID: 516 RVA: 0x0000C1E5 File Offset: 0x0000A3E5
	public ReactiveProperty<double> AngelsSpent { get; private set; }

	// Token: 0x17000021 RID: 33
	// (get) Token: 0x06000205 RID: 517 RVA: 0x0000C1EE File Offset: 0x0000A3EE
	// (set) Token: 0x06000206 RID: 518 RVA: 0x0000C1F6 File Offset: 0x0000A3F6
	public ReactiveProperty<int> AngelResetCount { get; private set; }

	// Token: 0x17000022 RID: 34
	// (get) Token: 0x06000207 RID: 519 RVA: 0x0000C1FF File Offset: 0x0000A3FF
	// (set) Token: 0x06000208 RID: 520 RVA: 0x0000C207 File Offset: 0x0000A407
	public ReactiveProperty<double> RewardAngelsAtInterval { get; private set; }

	// Token: 0x17000023 RID: 35
	// (get) Token: 0x06000209 RID: 521 RVA: 0x0000C210 File Offset: 0x0000A410
	// (set) Token: 0x0600020A RID: 522 RVA: 0x0000C218 File Offset: 0x0000A418
	public ReactiveProperty<double> AngelInvestorEffectiveness { get; private set; }

	// Token: 0x17000024 RID: 36
	// (get) Token: 0x0600020B RID: 523 RVA: 0x0000C221 File Offset: 0x0000A421
	// (set) Token: 0x0600020C RID: 524 RVA: 0x0000C229 File Offset: 0x0000A429
	public ReactiveProperty<double> AngelInvestorEffectivenessBonus { get; private set; }

	// Token: 0x17000025 RID: 37
	// (get) Token: 0x0600020D RID: 525 RVA: 0x0000C232 File Offset: 0x0000A432
	// (set) Token: 0x0600020E RID: 526 RVA: 0x0000C23A File Offset: 0x0000A43A
	public ReadOnlyReactiveProperty<double> TotalAngelBonus { get; private set; }

	// Token: 0x17000026 RID: 38
	// (get) Token: 0x0600020F RID: 527 RVA: 0x0000C243 File Offset: 0x0000A443
	// (set) Token: 0x06000210 RID: 528 RVA: 0x0000C24B File Offset: 0x0000A44B
	public ReactiveProperty<bool> AngelsDoubledNotification { get; private set; }

	// Token: 0x17000027 RID: 39
	// (get) Token: 0x06000211 RID: 529 RVA: 0x0000C254 File Offset: 0x0000A454
	// (set) Token: 0x06000212 RID: 530 RVA: 0x0000C25C File Offset: 0x0000A45C
	public ReactiveProperty<bool> IsAngelThresholdReached { get; private set; }

	// Token: 0x17000028 RID: 40
	// (get) Token: 0x06000213 RID: 531 RVA: 0x0000C265 File Offset: 0x0000A465
	// (set) Token: 0x06000214 RID: 532 RVA: 0x0000C26D File Offset: 0x0000A46D
	public ReactiveProperty<bool> FirstTimeAngelReset { get; private set; }

	// Token: 0x17000029 RID: 41
	// (get) Token: 0x06000215 RID: 533 RVA: 0x0000C276 File Offset: 0x0000A476
	// (set) Token: 0x06000216 RID: 534 RVA: 0x0000C27E File Offset: 0x0000A47E
	public ReactiveProperty<bool> IsFirstEventAngelResetComplete { get; private set; }

	// Token: 0x06000217 RID: 535 RVA: 0x0000C288 File Offset: 0x0000A488
	public AngelInvestorService()
	{
		this.AngelsOnHand = new ReactiveProperty<double>();
		this.AngelsSpent = new ReactiveProperty<double>(0.0);
		this.AngelResetCount = new ReactiveProperty<int>(0);
		this.RewardAngelsAtInterval = new ReactiveProperty<double>(0.0);
		this.AngelInvestorEffectivenessBonus = new ReactiveProperty<double>();
		this.AngelInvestorEffectiveness = new ReactiveProperty<double>(AngelInvestorService.ANGEL_INVESTOR_BASE_EFFECTIVENESS);
		this.TotalAngelBonus = this.AngelInvestorEffectiveness.CombineLatest(this.AngelInvestorEffectivenessBonus, (double angelEffectiveness, double angelEffectivenessBonus) => angelEffectiveness + angelEffectivenessBonus).CombineLatest(this.AngelsOnHand, (double newlyCalculatedBonus, double angelsOnHand) => 1.0 + angelsOnHand * newlyCalculatedBonus).ToReadOnlyReactiveProperty<double>();
		this.AngelsDoubledNotification = new ReactiveProperty<bool>(false);
		this.IsAngelThresholdReached = new ReactiveProperty<bool>(false);
		this.FirstTimeAngelReset = new ReactiveProperty<bool>(false);
		this.IsFirstEventAngelResetComplete = new ReactiveProperty<bool>(false);
	}

	// Token: 0x06000218 RID: 536 RVA: 0x0000C418 File Offset: 0x0000A618
	public void Init(IGameController gameController)
	{
		this.gameController = gameController;
		this.IsFirstEventAngelResetComplete.Value = this.gameController.GlobalPlayerData.Has("EventAngelResetComplete");
		(from x in this.gameController.State
		where x != null
		select x).Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
		(from message in MessageBroker.Default.Receive<InventoryEquipMessage>()
		where message.item.ItemBonusTarget == ItemBonusTarget.Angel
		select message).Subscribe(new Action<InventoryEquipMessage>(this.OnItemEquipStateChanged)).AddTo(this.disposables);
		KongregateGameObject.isLoggedIntoKong.Subscribe(new Action<bool>(this.HandleKongLoginStatus)).AddTo(this.disposables);
		this.gameController.OnLoadNewPlanetPre += this.OnLoadPlanetPre;
	}

	// Token: 0x06000219 RID: 537 RVA: 0x0000C51C File Offset: 0x0000A71C
	public void Dispose()
	{
		Debug.Log("[AngelInvestorService] Dispose()");
		this.disposables.Dispose();
		this.stateDisposables.Dispose();
	}

	// Token: 0x0600021A RID: 538 RVA: 0x0000C540 File Offset: 0x0000A740
	public void SpendAngelInvestors(double amount)
	{
		this.AngelsOnHand.Value -= amount;
		this.AngelsSpent.Value += amount;
		string angelString = NumberFormat.ConvertNormal(this.AngelsOnHand.Value, 1000000.0, 3);
		MessageBroker.Default.Publish<AngelsSpentEvent>(new AngelsSpentEvent
		{
			AngelAmount = amount,
			AngelString = angelString
		});
	}

	// Token: 0x0600021B RID: 539 RVA: 0x0000C5B4 File Offset: 0x0000A7B4
	public double CalculateAngelInvestors(double cash)
	{
		if (double.IsInfinity(cash) || cash >= GameState.MAX_CASH_DOUBLE)
		{
			cash = GameState.MAX_CASH_DOUBLE;
		}
		double num = Math.Floor(Math.Sqrt(9.0 * (cash / Math.Pow(10.0, 11.0))) / 2.0) * this.angelAccumulationRate - this.AngelsSpent.Value;
		if (num <= 0.0)
		{
			return 0.0;
		}
		return num;
	}

	// Token: 0x0600021C RID: 540 RVA: 0x0000C63C File Offset: 0x0000A83C
	public double CalculateCashFromAngels(double angels, double passedAngelAccumulationRate = 1.0)
	{
		return Math.Ceiling(Math.Pow(2.0 * (angels / passedAngelAccumulationRate), 2.0) * Math.Pow(10.0, 11.0) / 9.0);
	}

	// Token: 0x0600021D RID: 541 RVA: 0x0000C68C File Offset: 0x0000A88C
	public double GetAdWatchAngelBonus()
	{
		double num = this.CalculateAngelInvestors(this.gameController.game.TotalCashEarned);
		double num2 = this.CalculateAngelInvestors(this.gameController.game.TotalPreviousCash.Value);
		double num3 = (num - num2) * AngelInvestorService.ANGEL_INVESTOR_AD_MULTIPLIER;
		if (num3 > 0.0)
		{
			return num3;
		}
		return 0.0;
	}

	// Token: 0x0600021E RID: 542 RVA: 0x0000C6EC File Offset: 0x0000A8EC
	public void Reset(bool isAdWatch = false)
	{
		Debug.LogFormat("[AngelInvestorService] Reset() isAdWatch = {0}", new object[]
		{
			isAdWatch
		});
		double num = this.CalculateAngelInvestors(this.gameController.game.TotalCashEarned);
		double num2 = this.CalculateAngelInvestors(this.gameController.game.TotalPreviousCash.Value);
		double num3 = num - num2;
		if (num3 < 0.0)
		{
			num3 = 0.0;
		}
		if (isAdWatch)
		{
			num3 += num3 * AngelInvestorService.ANGEL_INVESTOR_AD_MULTIPLIER;
		}
		if (this.angelClaimBonus > 0f)
		{
			num3 *= (double)(1f + this.angelClaimBonus);
		}
		this.RewardAngelsAtInterval.SetValueAndForceNotify(0.0);
		this.AdjustSessionCashForAngelBonus(num3);
		this.AngelsOnHand.Value += num3;
		this.AngelInvestorEffectiveness.Value = AngelInvestorService.ANGEL_INVESTOR_BASE_EFFECTIVENESS;
		string angelString = NumberFormat.ConvertNormal(num3, 1000000.0, 3);
		MessageBroker.Default.Publish<AngelsClaimedEvent>(new AngelsClaimedEvent
		{
			AngelAmount = num3,
			AngelString = angelString,
			IsAngelClaimItem = isAdWatch
		});
	}

	// Token: 0x0600021F RID: 543 RVA: 0x0000C804 File Offset: 0x0000AA04
	public void HardReset()
	{
		this.AngelsOnHand.Value = 0.0;
		this.AngelsSpent.Value = 0.0;
		this.AngelInvestorEffectiveness.Value = AngelInvestorService.ANGEL_INVESTOR_BASE_EFFECTIVENESS;
		this.angelAccumulationRate = this.state.angelAccumulationRate;
	}

	// Token: 0x06000220 RID: 544 RVA: 0x0000C85A File Offset: 0x0000AA5A
	public void AddAngelsOnHand(double amount)
	{
		this.state.TotalPreviousCash.Value = this.CalculateCashFromAngels(amount, 1.0);
		this.AngelsOnHand.Value += amount;
	}

	// Token: 0x06000221 RID: 545 RVA: 0x0000C890 File Offset: 0x0000AA90
	public double GetRewardAngelCount()
	{
		double num = this.CalculateAngelInvestors(this.gameController.game.TotalCashEarned);
		double num2 = this.CalculateAngelInvestors(this.gameController.game.TotalPreviousCash.Value);
		return num - num2;
	}

	// Token: 0x06000222 RID: 546 RVA: 0x0000C8D1 File Offset: 0x0000AAD1
	public bool ShouldPromptOnAngelPurchase(double cost)
	{
		return cost > this.AngelsOnHand.Value * 0.01;
	}

	// Token: 0x06000223 RID: 547 RVA: 0x0000C8EC File Offset: 0x0000AAEC
	private void OnStateChanged(GameState state)
	{
		this.stateDisposables.Clear();
		this.state = state;
		this.RewardAngelsAtInterval.Value = this.GetRewardAngelCount();
		this.angelAccumulationRate = state.angelAccumulationRate;
		if (this.gameController.DataService.ExternalData.GeneralConfig[0].AngelServiceConfig != null)
		{
			this.angelResetThresholdPercentage = this.gameController.DataService.ExternalData.GeneralConfig[0].AngelServiceConfig.ResetThresholdPercentage;
			this.firstEventAngelResetThresholdPercentage = this.gameController.DataService.ExternalData.GeneralConfig[0].AngelServiceConfig.FirstEventResetAngelThresholdPercentage;
			this.secondsBetweenAngelThresholdNotifications = this.gameController.DataService.ExternalData.GeneralConfig[0].AngelServiceConfig.SecondsBetweenAngelThresholdNotifications;
		}
		this.lastAngelReminderTime = DateTime.MinValue;
		DateTime.TryParse(this.gameController.game.planetPlayerData.Get("nextAngelReminderTimeSaveKey", ""), out this.lastAngelReminderTime);
		this.eventAngelResetThreshold = Math.Round(this.angelNotificationValue + this.angelNotificationValue * (double)this.firstEventAngelResetThresholdPercentage);
		this.IsAngelThresholdReached.Value = false;
		this.LoadAngelResetThreshold();
		Observable.Interval(TimeSpan.FromSeconds(1.0)).Subscribe(delegate(long x)
		{
			this.RewardAngelsAtInterval.Value = this.GetRewardAngelCount();
		}).AddTo(this.stateDisposables);
		Observable.Interval(TimeSpan.FromSeconds(1.0)).Subscribe(delegate(long x)
		{
			double num2 = this.CalculateAngelInvestors(this.state.TotalCashEarned);
			double num3 = this.CalculateAngelInvestors(this.state.TotalPreviousCash.Value);
			double num4 = num2 - num3;
			if (!this.FirstTimeAngelReset.Value && num3 == 0.0 && num4 >= this.angelNotificationValue)
			{
				this.FirstTimeAngelReset.Value = true;
			}
			if (num4 > this.angelResetThreshold && !this.IsAngelThresholdReached.Value && this.lastAngelReminderTime.AddSeconds((double)this.secondsBetweenAngelThresholdNotifications) < DateTime.UtcNow)
			{
				this.IsAngelThresholdReached.Value = true;
				this.SetAngelResetThreshold(false);
				this.UpdateAngelReminderTime();
			}
			if (num4 >= 50.0 && num4 >= num3 && num3 > 0.0)
			{
				this.AngelsDoubledNotification.Value = true;
				return;
			}
			this.AngelsDoubledNotification.Value = false;
		}).AddTo(this.stateDisposables);
		this.AngelsSpent.Value = state.angelInvestorsSpent;
		this.AngelsOnHand.Value = state.angelInvestors;
		this.AngelResetCount.Value = state.planetPlayerData.GetInt("ResetCount", 0);
		this.AngelsDoubledNotification.Value = false;
		this.FirstTimeAngelReset.Value = false;
		this.CalculateKongAngelInvestorBonus(KongregateGameObject.isLoggedIntoKong.Value);
		this.gameController.OnSoftResetPost -= this.HandleOnSoftResetPost;
		this.gameController.OnSoftResetPost += this.HandleOnSoftResetPost;
		foreach (Item item in this.gameController.GlobalPlayerData.inventory.GetAllEquippedItems())
		{
			if (item.ItemBonusTarget == ItemBonusTarget.Angel)
			{
				this.ApplyItemEquipStateChanged(item, true);
			}
		}
		if (this.AngelsOnHand.Value < 0.0)
		{
			double value = this.AngelsOnHand.Value;
			double num = this.CalculateAngelInvestors(state.TotalPreviousCash.Value);
			this.AngelsOnHand.Value = num;
			this.gameController.AnalyticService.SendTaskCompleteEvent("AngelsBelowZeroFix", string.Concat(value), string.Concat(num));
		}
	}

	// Token: 0x06000224 RID: 548 RVA: 0x0000CBF4 File Offset: 0x0000ADF4
	private void OnLoadPlanetPre()
	{
		this.angelInvestorEffectivenessBonusMap.Clear();
		this.equippedAngelInvestorItemBonuses.Clear();
		this.equippedAngelInvestorItemBonuses.Add(new ReactiveProperty<float>(0f));
		this.equippedAngelClaimItemBonuses.Clear();
		this.equippedAngelClaimItemBonuses = new List<ReactiveProperty<float>>(0);
		this.AngelInvestorEffectivenessBonus.Value = 0.0;
		this.AngelInvestorEffectiveness.Value = AngelInvestorService.ANGEL_INVESTOR_BASE_EFFECTIVENESS;
		this.RewardAngelsAtInterval.SetValueAndForceNotify(0.0);
		this.angelInvestorDisposable.Dispose();
		this.angelClaimDisposable.Dispose();
	}

	// Token: 0x06000225 RID: 549 RVA: 0x0000CC94 File Offset: 0x0000AE94
	private void HandleOnSoftResetPost()
	{
		this.FirstTimeAngelReset.Value = false;
		this.IsAngelThresholdReached.Value = false;
		this.SetAngelResetThreshold(true);
		this.state.planetPlayerData.Add("ResetCount", 1.0);
		this.AngelResetCount.Value = this.state.planetPlayerData.GetInt("ResetCount", 0);
		if (this.gameController.game.IsEventPlanet)
		{
			this.gameController.GlobalPlayerData.SetBool("EventAngelResetComplete", true);
			this.IsFirstEventAngelResetComplete.Value = true;
		}
	}

	// Token: 0x06000226 RID: 550 RVA: 0x0000CD34 File Offset: 0x0000AF34
	private void OnItemEquipStateChanged(InventoryEquipMessage inventoryEquipMessage)
	{
		this.ApplyItemEquipStateChanged(inventoryEquipMessage.item, inventoryEquipMessage.equipped);
	}

	// Token: 0x06000227 RID: 551 RVA: 0x0000CD48 File Offset: 0x0000AF48
	private void ApplyItemEquipStateChanged(Item item, bool equipped)
	{
		if (!string.IsNullOrEmpty(item.BonusCustomData))
		{
			string[] array = item.BonusCustomData.Split(new char[]
			{
				':'
			});
			if (array.Length == 1)
			{
				if (this.state.IsEventPlanet)
				{
					if (array[0] != this.state.PlanetData.PlanetName && array[0] != "AllEvents")
					{
						return;
					}
				}
				else if (array[0] != this.state.PlanetData.PlanetName && array[0] != "AllPlanets")
				{
					return;
				}
			}
		}
		ItemBonusType itemBonusType = item.ItemBonusType;
		if (itemBonusType == ItemBonusType.AngelEffectivenessPercent)
		{
			this.angelInvestorDisposable.Dispose();
			if (equipped)
			{
				this.equippedAngelInvestorItemBonuses.Add(item.CurrentLeveledBonus);
			}
			else
			{
				this.equippedAngelInvestorItemBonuses.Remove(item.CurrentLeveledBonus);
			}
			this.angelInvestorDisposable = (from v in this.equippedAngelInvestorItemBonuses
			select v).CombineLatest<float>().Subscribe(delegate(IList<float> v)
			{
				this.AddAngelInvestorBonus("AIItemBonus", (double)v.Sum());
			});
			return;
		}
		if (itemBonusType != ItemBonusType.AngelClaim)
		{
			return;
		}
		this.angelClaimDisposable.Dispose();
		if (equipped)
		{
			this.equippedAngelClaimItemBonuses.Add(item.CurrentLeveledBonus);
		}
		else
		{
			this.equippedAngelClaimItemBonuses.Remove(item.CurrentLeveledBonus);
		}
		this.angelClaimDisposable = (from v in this.equippedAngelClaimItemBonuses
		select v).CombineLatest<float>().Subscribe(delegate(IList<float> v)
		{
			this.angelClaimBonus = v.Sum();
		});
	}

	// Token: 0x06000228 RID: 552 RVA: 0x0000CEE4 File Offset: 0x0000B0E4
	private void AdjustSessionCashForAngelBonus(double newRewardAngels)
	{
		double num = newRewardAngels + this.AngelsOnHand.Value + this.AngelsSpent.Value;
		if (num > 0.0 && this.angelAccumulationRate > 0.0)
		{
			double num2 = this.CalculateCashFromAngels(num, this.angelAccumulationRate);
			if (num2 > this.state.TotalPreviousCash.Value)
			{
				this.state.SessionCash.Value = num2 - this.state.TotalPreviousCash.Value;
			}
		}
	}

	// Token: 0x06000229 RID: 553 RVA: 0x0000CF6C File Offset: 0x0000B16C
	public void AddAngelInvestorBonus(string key, double value)
	{
		if (this.angelInvestorEffectivenessBonusMap.ContainsKey(key))
		{
			this.angelInvestorEffectivenessBonusMap[key] = value;
		}
		else
		{
			this.angelInvestorEffectivenessBonusMap.Add(key, value);
		}
		this.AngelInvestorEffectivenessBonus.Value = this.angelInvestorEffectivenessBonusMap.Sum((KeyValuePair<string, double> i) => i.Value);
	}

	// Token: 0x0600022A RID: 554 RVA: 0x0000CFD8 File Offset: 0x0000B1D8
	public void RemoveAngelInvestorBonus(string key)
	{
		if (this.angelInvestorEffectivenessBonusMap.Remove(key))
		{
			this.AngelInvestorEffectivenessBonus.Value = this.angelInvestorEffectivenessBonusMap.Sum((KeyValuePair<string, double> i) => i.Value);
		}
	}

	// Token: 0x0600022B RID: 555 RVA: 0x0000D028 File Offset: 0x0000B228
	public void OnAngelClaimPurchaseCompleted()
	{
		double num = this.CalculateAngelInvestors(this.state.TotalCashEarned);
		double num2 = this.CalculateAngelInvestors(this.gameController.game.TotalPreviousCash.Value);
		double num3 = num - num2;
		this.AngelsOnHand.Value = num;
		this.state.TotalPreviousCash.Value = this.state.TotalCashEarned;
		this.state.SessionCash.Value = 0.0;
		string angelString = NumberFormat.ConvertNormal(num3, 1000000.0, 3);
		MessageBroker.Default.Publish<AngelsClaimedEvent>(new AngelsClaimedEvent
		{
			AngelAmount = num3,
			AngelString = angelString,
			IsAngelClaimItem = true
		});
		this.FirstTimeAngelReset.Value = false;
	}

	// Token: 0x0600022C RID: 556 RVA: 0x0000D0F0 File Offset: 0x0000B2F0
	private void HandleKongLoginStatus(bool isLoggedIn)
	{
		Debug.Log("[Kong][AngelInvestorService] HandleKongLoginStatus=" + isLoggedIn.ToString());
		this.CalculateKongAngelInvestorBonus(isLoggedIn);
	}

	// Token: 0x0600022D RID: 557 RVA: 0x0000D110 File Offset: 0x0000B310
	private void CalculateKongAngelInvestorBonus(bool authenticated)
	{
		if (Helper.GetPlatformType() == PlatformType.Android || Helper.GetPlatformType() == PlatformType.Ios)
		{
			if (this.state == null)
			{
				return;
			}
			if (this.state.IsEventPlanet)
			{
				this.RemoveAngelInvestorBonus("kong_login_angels");
				return;
			}
			if (authenticated)
			{
				this.AddAngelInvestorBonus("kong_login_angels", 0.05);
				return;
			}
			this.RemoveAngelInvestorBonus("kong_login_angels");
		}
	}

	// Token: 0x0600022E RID: 558 RVA: 0x0000D174 File Offset: 0x0000B374
	private void LoadAngelResetThreshold()
	{
		double @double = this.gameController.game.planetPlayerData.GetDouble("angelResetThreshold", 0.0);
		if (@double == 0.0)
		{
			this.SetAngelResetThreshold(false);
			return;
		}
		this.angelResetThreshold = @double;
		if (this.GetRewardAngelCount() > this.angelResetThreshold)
		{
			this.IsAngelThresholdReached.Value = true;
		}
	}

	// Token: 0x0600022F RID: 559 RVA: 0x0000D1DC File Offset: 0x0000B3DC
	private void SetAngelResetThreshold(bool includeCurrentAngels = false)
	{
		this.IsAngelThresholdReached.Value = false;
		double num = this.CalculateAngelInvestors(this.gameController.game.TotalPreviousCash.Value);
		double num2 = includeCurrentAngels ? num : this.GetRewardAngelCount();
		double num3 = (num2 < this.angelNotificationValue) ? this.angelNotificationValue : num2;
		this.angelResetThreshold = num3 + (double)this.angelResetThresholdPercentage * num3;
		this.gameController.game.planetPlayerData.Set("angelResetThreshold", this.angelResetThreshold.ToString());
	}

	// Token: 0x06000230 RID: 560 RVA: 0x0000D26B File Offset: 0x0000B46B
	private void UpdateAngelReminderTime()
	{
		this.lastAngelReminderTime = DateTime.UtcNow;
		this.gameController.game.planetPlayerData.Set("nextAngelReminderTimeSaveKey", this.lastAngelReminderTime.ToString());
	}

	// Token: 0x040001EE RID: 494
	private const string ANGEL_RESET_THRESHOLD_SAVE_KEY = "angelResetThreshold";

	// Token: 0x040001EF RID: 495
	private const string ANGEL_RESET_TIME_SAVE_KEY = "nextAngelReminderTimeSaveKey";

	// Token: 0x040001F0 RID: 496
	private const string EVENT_ANGEL_RESET_COMPLETE_KEY = "EventAngelResetComplete";

	// Token: 0x040001F1 RID: 497
	private DateTime lastAngelReminderTime;

	// Token: 0x040001F2 RID: 498
	private float secondsBetweenAngelThresholdNotifications = 28800f;

	// Token: 0x040001F3 RID: 499
	private const float INITIAL_ANGEL_RESET_TARGET_BASE = 50f;

	// Token: 0x040001F4 RID: 500
	private float angelResetThresholdPercentage = 0.25f;

	// Token: 0x040001F5 RID: 501
	private float firstEventAngelResetThresholdPercentage = 0.1f;

	// Token: 0x040001F6 RID: 502
	private static double ANGEL_INVESTOR_BASE_EFFECTIVENESS = 0.02;

	// Token: 0x040001F7 RID: 503
	private static double ANGEL_INVESTOR_AD_MULTIPLIER = 0.20000000298023224;

	// Token: 0x040001F8 RID: 504
	private const string KONG_ANGEL_INVESTOR_BONUS_KEY = "kong_login_angels";

	// Token: 0x040001F9 RID: 505
	private const double KONG_ANGEL_INVESTOR_BONUS = 0.05;

	// Token: 0x040001FA RID: 506
	private double angelAccumulationRate = 1.0;

	// Token: 0x040001FB RID: 507
	private float angelClaimBonus;

	// Token: 0x040001FC RID: 508
	private double angelNotificationValue = 50.0;

	// Token: 0x040001FD RID: 509
	private Dictionary<string, double> angelInvestorEffectivenessBonusMap = new Dictionary<string, double>();

	// Token: 0x040001FE RID: 510
	private List<ReactiveProperty<float>> equippedAngelInvestorItemBonuses = new List<ReactiveProperty<float>>();

	// Token: 0x040001FF RID: 511
	private IDisposable angelInvestorDisposable = Disposable.Empty;

	// Token: 0x04000200 RID: 512
	private List<ReactiveProperty<float>> equippedAngelClaimItemBonuses = new List<ReactiveProperty<float>>();

	// Token: 0x04000201 RID: 513
	private IDisposable angelClaimDisposable = Disposable.Empty;

	// Token: 0x04000202 RID: 514
	private IGameController gameController;

	// Token: 0x04000203 RID: 515
	private GameState state;

	// Token: 0x04000204 RID: 516
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000205 RID: 517
	private CompositeDisposable stateDisposables = new CompositeDisposable();
}
