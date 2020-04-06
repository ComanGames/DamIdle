using System;
using System.Collections.Generic;
using System.Linq;
using AdCap;
using UniRx;
using UnityEngine;

// Token: 0x0200010F RID: 271
public class UnlockService : IDisposable
{
	// Token: 0x14000001 RID: 1
	// (add) Token: 0x06000713 RID: 1811 RVA: 0x00025AA4 File Offset: 0x00023CA4
	// (remove) Token: 0x06000714 RID: 1812 RVA: 0x00025ADC File Offset: 0x00023CDC
	public event Action RateGameUiRequested = delegate()
	{
	};

	// Token: 0x06000715 RID: 1813 RVA: 0x00025B14 File Offset: 0x00023D14
	~UnlockService()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000716 RID: 1814 RVA: 0x00025B48 File Offset: 0x00023D48
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000717 RID: 1815 RVA: 0x00025B58 File Offset: 0x00023D58
	public void Init()
	{
		GameController.Instance.State.Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
		GameController.Instance.IsInitialized.First(x => x).Subscribe(new Action<bool>(this.OnGameControllerInitiliazed)).AddTo(this.disposables);
	}

	// Token: 0x06000718 RID: 1816 RVA: 0x00025BD8 File Offset: 0x00023DD8
	private void OnGameControllerInitiliazed(bool isInit)
	{
		(from x in GameController.Instance.UpgradeService.OnUpgradePurchased
		where x != null
		select x).Subscribe(delegate(Upgrade _)
		{
			this.CheckUnlocks();
		}).AddTo(this.disposables);
	}

	// Token: 0x06000719 RID: 1817 RVA: 0x00025C38 File Offset: 0x00023E38
	private void OnStateChanged(GameState state)
	{
		this.stateDisposables.Clear();
		this.Unlocks.Clear();
		this.Unlocks.AddRange(state.Unlocks);
		this.AchievedUnlocks.Clear();
		this.AchievedUnlocks.AddRange(state.AchievedUnlocks);
		this.state = state;
		this.OnAllUnlocksDone.Value = false;
		this.RestoreAchievedUnlocks();
		this.CheckUnlocks();
		this.SetAllNextUnlocks();
		foreach (VentureModel ventureModel2 in this.state.VentureModels)
		{
			VentureModel ventureModel = ventureModel2;
			(from unlockTarget in this.OnUnlockTargetSet
			where unlockTarget.ventureName == ventureModel.Name
			select unlockTarget).Subscribe(delegate(UnlockTargetInfo unlockTarget)
			{
				ventureModel.LastAchievementTarget.Value = unlockTarget.previousTarget;
				ventureModel.AchievementTarget.Value = unlockTarget.nextTarget;
			}).AddTo(this.stateDisposables);
			IEnumerable<SingleVentureUnlock> source = from x in GameController.Instance.UnlockService.Unlocks
			where x is SingleVentureUnlock && (x as SingleVentureUnlock).ventureName == ventureModel.Name
			select x as SingleVentureUnlock;
			List<BoolReactiveProperty> sources = (from x in source
			select x.Claimed).ToList<BoolReactiveProperty>();
			List<BoolReactiveProperty> sources2 = (from x in source
			select x.Earned).ToList<BoolReactiveProperty>();
			if (!this.AutoClaimFirstTimeUnlocks)
			{
				(from x in sources.CombineLatest<bool>()
				select x.Count(y => y)).CombineLatest(from x in sources2.CombineLatest<bool>()
				select x.Count(y => y), (claimedCount, earnedCount) => earnedCount - claimedCount).Subscribe(delegate(int unlocksToClaim)
				{
					ventureModel.UnlocksToClaim.Value = unlocksToClaim;
				}).AddTo(this.stateDisposables);
			}
		}
		(from x in state.OnVenturePurchased
		where x != null
		select x).Subscribe(delegate(VentureModel _)
		{
			this.CheckUnlocks();
		}).AddTo(this.stateDisposables);
		if (state.IsEventPlanet)
		{
			this.OnUnlockAchieved.Subscribe(new Action<Unlock>(this.OnEventUnlockAchieved)).AddTo(this.stateDisposables);
		}
	}

	// Token: 0x0600071A RID: 1818 RVA: 0x00025EEC File Offset: 0x000240EC
	private void ValidateUnlocks()
	{
		foreach (Unlock unlock2 in from unlock in this.Unlocks
		where this.Unlocks.FindAll(u => u.name == unlock.name).Count > 1
		select unlock)
		{
			Debug.LogErrorFormat("There are two or more unlocks with the name [{0}], this will cause issues with saving and must be corrected.", new object[]
			{
				unlock2.name
			});
		}
	}

	// Token: 0x0600071B RID: 1819 RVA: 0x00025F5C File Offset: 0x0002415C
	private void RestoreAchievedUnlocks()
	{
		IEnumerable<string> collection = from u in this.Unlocks
		where !u.Earned.Value && u.Check(this.state)
		select u.name;
		this.AchievedUnlocks.AddRange(collection);
	}

	// Token: 0x0600071C RID: 1820 RVA: 0x00025FB4 File Offset: 0x000241B4
	private void CheckUnlocks()
	{
		foreach (Unlock unlock in from u in this.Unlocks
		where !u.Earned.Value && u.Check(this.state)
		select u)
		{
			unlock.Earned.Value = true;
			if ((this.AutoClaimFirstTimeUnlocks || unlock.EverClaimed.Value || (!this.AutoClaimFirstTimeUnlocks && !this.ShowsUnlockMissions && unlock is EveryVentureUnlock)) && !unlock.Claimed.Value)
			{
				this.ClaimUnlock(unlock);
			}
			this.TriggerUnlockEvent(unlock);
		}
	}

	// Token: 0x0600071D RID: 1821 RVA: 0x00026060 File Offset: 0x00024260
	public void TriggerUnlockEvent(Unlock unlock)
	{
		if (unlock is SingleVentureUnlock)
		{
			SingleVentureUnlock singleVentureUnlock = (SingleVentureUnlock)this.Unlocks.FirstOrDefault(a => a is SingleVentureUnlock && ((SingleVentureUnlock)a).ventureName == ((SingleVentureUnlock)unlock).ventureName && !a.Earned.Value);
			if (singleVentureUnlock != null)
			{
				UnlockTargetInfo value = new UnlockTargetInfo(singleVentureUnlock.ventureName, unlock.amountToEarn, singleVentureUnlock.amountToEarn);
				this.OnUnlockTargetSet.OnNext(value);
			}
		}
	}

	// Token: 0x0600071E RID: 1822 RVA: 0x000260D0 File Offset: 0x000242D0
	public void ClaimUnlock(Unlock unlock)
	{
		bool value = unlock.EverClaimed.Value;
		unlock.Apply(GameController.Instance.game);
		bool flag = this.FirstTimeUnlocked(unlock);
		flag = ((!this.AutoClaimFirstTimeUnlocks) ? (!value) : flag);
		if (flag && this.state.neverRate != 1)
		{
			SingleVentureUnlock singleVentureUnlock = unlock as SingleVentureUnlock;
			if (singleVentureUnlock != null)
			{
				if (singleVentureUnlock.ventureName == "Newspaper Delivery" && singleVentureUnlock.amountToEarn == 100)
				{
					this.RateGameUiRequested();
				}
				else if (singleVentureUnlock.ventureName == "Lemonade Stand" && singleVentureUnlock.amountToEarn == 500)
				{
					this.RateGameUiRequested();
				}
				else if (singleVentureUnlock.ventureName == "Newspaper Delivery" && singleVentureUnlock.amountToEarn == 500)
				{
					this.RateGameUiRequested();
				}
			}
			this.AchievedUnlocks.Add(unlock.name);
		}
		this.OnUnlockAchieved.OnNext(unlock);
		if (flag)
		{
			this.OnUnlockAchievedFirstTime.OnNext(unlock);
		}
		this.UpdateNextUnlocks();
		if (this.Unlocks.All(u => u.Claimed.Value))
		{
			this.OnAllUnlocksDone.Value = true;
		}
	}

	// Token: 0x0600071F RID: 1823 RVA: 0x0002621C File Offset: 0x0002441C
	public void SetAllNextUnlocks()
	{
		using (IEnumerator<VentureModel> enumerator = this.state.VentureModels.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				VentureModel model = enumerator.Current;
				SingleVentureUnlock singleVentureUnlock = (SingleVentureUnlock)this.Unlocks.FirstOrDefault(a => a is SingleVentureUnlock && ((SingleVentureUnlock)a).ventureName == model.Name && !a.Earned.Value);
				if (singleVentureUnlock != null)
				{
					SingleVentureUnlock singleVentureUnlock2 = (SingleVentureUnlock)this.Unlocks.LastOrDefault(a => a is SingleVentureUnlock && ((SingleVentureUnlock)a).ventureName == model.Name && a.Earned.Value);
					UnlockTargetInfo value = new UnlockTargetInfo(singleVentureUnlock.ventureName, (singleVentureUnlock2 == null) ? 0 : singleVentureUnlock2.amountToEarn, singleVentureUnlock.amountToEarn);
					this.OnUnlockTargetSet.OnNext(value);
				}
			}
		}
		this.UpdateNextUnlocks();
	}

	// Token: 0x06000720 RID: 1824 RVA: 0x000262E4 File Offset: 0x000244E4
	private bool FirstTimeUnlocked(Unlock unlock)
	{
		return unlock != null && !this.AchievedUnlocks.Contains(unlock.name);
	}

	// Token: 0x06000721 RID: 1825 RVA: 0x00026300 File Offset: 0x00024500
	private void UpdateNextUnlocks()
	{
		this.ReactiveNextUnlocks.Clear();
		List<SingleVentureUnlock> source = (from u in this.Unlocks
		where !u.Earned.Value
		select u as SingleVentureUnlock).ToList<SingleVentureUnlock>();
		List<EveryVentureUnlock> source2 = (from u in this.Unlocks
		where !u.Earned.Value
		select u as EveryVentureUnlock).ToList<EveryVentureUnlock>();
		using (IEnumerator<VentureModel> enumerator = this.state.VentureModels.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				VentureModel venture = enumerator.Current;
				Unlock unlock3 = source.FirstOrDefault(unlock => unlock != null && unlock.ventureName == venture.Name);
				if (unlock3 != null)
				{
					this.ReactiveNextUnlocks.Add(unlock3);
				}
			}
		}
		Unlock unlock2 = source2.FirstOrDefault(unlock => unlock != null);
		if (unlock2 != null)
		{
			this.ReactiveNextUnlocks.Add(unlock2);
		}
	}

	// Token: 0x06000722 RID: 1826 RVA: 0x00026468 File Offset: 0x00024668
	private void OnEventUnlockAchieved(Unlock unlock)
	{
		string planetName = this.state.planetName;
		PlayerData playerData = PlayerData.GetPlayerData("Global");
		if (unlock.Reward is UnlockRewardGold)
		{
			playerData.Add(planetName + "_gold", 1.0);
			return;
		}
		if (unlock.Reward is UnlockRewardMegaBucks)
		{
			playerData.Add(planetName + "_megaBucks", 1.0);
			return;
		}
		if (unlock.Reward is UnlockRewardBadge)
		{
			playerData.Add(planetName + "_badges", 1.0);
			return;
		}
		if (unlock.Reward is UnlockRewardItemizationItem)
		{
			playerData.Add(planetName + "_itemization", 1.0);
			return;
		}
		if (unlock.Reward is UnlockRewardTimewarpDaily)
		{
			playerData.Add(planetName + "_timewarp", 1.0);
			return;
		}
		if (unlock.Reward is UnlockRewardTimeWarpExpress)
		{
			playerData.Add(planetName + "_timewarpExpress", 1.0);
		}
	}

	// Token: 0x17000081 RID: 129
	// (get) Token: 0x06000723 RID: 1827 RVA: 0x00026581 File Offset: 0x00024781
	public bool CanClaimUnlocksFromRoot
	{
		get
		{
			return UnlockService.PortraitEnabledPlatform && FeatureConfig.IsFlagSet("CanClaimUnlocksFromRoot");
		}
	}

	// Token: 0x17000082 RID: 130
	// (get) Token: 0x06000724 RID: 1828 RVA: 0x00026596 File Offset: 0x00024796
	public bool ShowsProgressOnVentureView
	{
		get
		{
			return !UnlockService.PortraitEnabledPlatform || FeatureConfig.IsFlagSet("ShowsProgressOnVentureView");
		}
	}

	// Token: 0x17000083 RID: 131
	// (get) Token: 0x06000725 RID: 1829 RVA: 0x000265AB File Offset: 0x000247AB
	public bool ShowsUnlockOutOfOnVentureView
	{
		get
		{
			return UnlockService.PortraitEnabledPlatform && FeatureConfig.IsFlagSet("ShowsUnlockOutOfOnVentureView");
		}
	}

	// Token: 0x17000084 RID: 132
	// (get) Token: 0x06000726 RID: 1830 RVA: 0x000265C0 File Offset: 0x000247C0
	public bool ShowsUnlockMissions
	{
		get
		{
			return UnlockService.PortraitEnabledPlatform && FeatureConfig.IsFlagSet("ShowsUnlockMissions");
		}
	}

	// Token: 0x17000085 RID: 133
	// (get) Token: 0x06000727 RID: 1831 RVA: 0x000265D5 File Offset: 0x000247D5
	public bool ShowsNewToast
	{
		get
		{
			return UnlockService.PortraitEnabledPlatform && FeatureConfig.IsFlagSet("ShowsNewToast");
		}
	}

	// Token: 0x17000086 RID: 134
	// (get) Token: 0x06000728 RID: 1832 RVA: 0x000265EA File Offset: 0x000247EA
	public bool AutoClaimFirstTimeUnlocks
	{
		get
		{
			return !UnlockService.PortraitEnabledPlatform || FeatureConfig.IsFlagSet("AutoClaimFirstTimeUnlocks");
		}
	}

	// Token: 0x17000087 RID: 135
	// (get) Token: 0x06000729 RID: 1833 RVA: 0x000265FF File Offset: 0x000247FF
	private static bool PortraitEnabledPlatform
	{
		get
		{
			return Application.isEditor || Application.isMobilePlatform;
		}
	}

	// Token: 0x040006AE RID: 1710
	public const string EVERY_VENTURE_MAP_ID = "EveryVenture";

	// Token: 0x040006AF RID: 1711
	public readonly List<Unlock> Unlocks = new List<Unlock>();

	// Token: 0x040006B0 RID: 1712
	public List<string> AchievedUnlocks = new List<string>();

	// Token: 0x040006B1 RID: 1713
	public ReplaySubject<UnlockTargetInfo> OnUnlockTargetSet = new ReplaySubject<UnlockTargetInfo>();

	// Token: 0x040006B2 RID: 1714
	public Subject<Unlock> OnUnlockAchieved = new Subject<Unlock>();

	// Token: 0x040006B3 RID: 1715
	public Subject<Unlock> OnUnlockAchievedFirstTime = new Subject<Unlock>();

	// Token: 0x040006B4 RID: 1716
	public ReactiveProperty<bool> OnAllUnlocksDone = new ReactiveProperty<bool>();

	// Token: 0x040006B6 RID: 1718
	public ReactiveCollection<Unlock> ReactiveNextUnlocks = new ReactiveCollection<Unlock>();

	// Token: 0x040006B7 RID: 1719
	private GameState state;

	// Token: 0x040006B8 RID: 1720
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040006B9 RID: 1721
	private CompositeDisposable stateDisposables = new CompositeDisposable();
}
