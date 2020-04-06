using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

// Token: 0x02000118 RID: 280
public class UpgradeService : IDisposable
{
	// Token: 0x06000764 RID: 1892 RVA: 0x000272E4 File Offset: 0x000254E4
	~UpgradeService()
	{
		this.Dispose();
	}

	// Token: 0x06000765 RID: 1893 RVA: 0x00027310 File Offset: 0x00025510
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000766 RID: 1894 RVA: 0x00027320 File Offset: 0x00025520
	public void Init()
	{
		GameController.Instance.OnLoadNewPlanetPre += this.OnPreStateChanged;
		GameController.Instance.State.Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
		this.NewUpgradeAvailableMap.Add(Upgrade.Currency.InGameCash, new ReactiveProperty<bool>(false));
		this.NewUpgradeAvailableMap.Add(Upgrade.Currency.AngelInvestors, new ReactiveProperty<bool>(false));
		this.NewManagerAvailableMap.Add(Upgrade.Currency.InGameCash, new ReactiveProperty<bool>(false));
		this.NewManagerAvailableMap.Add(Upgrade.Currency.AngelInvestors, new ReactiveProperty<bool>(false));
	}

	// Token: 0x06000767 RID: 1895 RVA: 0x000273B2 File Offset: 0x000255B2
	private void OnPreStateChanged()
	{
		this.QuikBuyUnlocked.Value = false;
		this.NextUpgrade.Value = null;
	}

	// Token: 0x06000768 RID: 1896 RVA: 0x000273CC File Offset: 0x000255CC
	private void OnStateChanged(GameState state)
	{
		this.stateDisposables.Clear();
		this.Upgrades.Clear();
		this.Managers.Clear();
		this.state = state;
		this.uiController = MainUIController.instance;
		this.aiService = GameController.Instance.AngelService;
		this.playerData = PlayerData.GetPlayerData("Global");
		this.Upgrades.AddRange(state.Upgrades);
		this.Managers.AddRange(state.Managers);
		this.Upgrades.Sort(new Comparison<Upgrade>(UpgradeService.SortUpgradesByOrderAscending));
		(from x in this.Upgrades
		where x.IsPurchased.Value
		select x.id).ToList<string>().ForEach(new Action<string>(this.PurchasedUpgradeCollection.Add));
		(from x in this.Managers
		where x.IsPurchased.Value
		select x.id).ToList<string>().ForEach(new Action<string>(this.PurchasedUpgradeCollection.Add));
		this.SetupAllUpgradesMap();
		this.MonitorPurchasbleUpgrades();
		this.SetNextUpgrade();
		this.MonitorUpgradeCosts();
		this.IsInitialized.Value = true;
	}

	// Token: 0x06000769 RID: 1897 RVA: 0x00027560 File Offset: 0x00025760
	public void Reset()
	{
		this.NewUpgradeAvailableMap[Upgrade.Currency.InGameCash].Value = false;
		this.NewUpgradeAvailableMap[Upgrade.Currency.AngelInvestors].Value = false;
		this.NewManagerAvailableMap[Upgrade.Currency.InGameCash].Value = false;
		this.NewManagerAvailableMap[Upgrade.Currency.AngelInvestors].Value = false;
		(from up in this.Upgrades
		where up.currency != Upgrade.Currency.Megabucks
		select up).ToList<Upgrade>().ForEach(delegate(Upgrade x)
		{
			x.Reset();
		});
		this.Managers.ForEach(delegate(Upgrade x)
		{
			x.Reset();
		});
		(from up in this.Upgrades
		where up.IsPurchased.Value && up.currency == Upgrade.Currency.Megabucks
		select up).ToList<Upgrade>().ForEach(delegate(Upgrade x)
		{
			x.Apply(this.state);
		});
		this.OnAngelsOnHandChanged(this.aiService.AngelsOnHand.Value);
		this.QuikBuyUnlocked.Value = false;
	}

	// Token: 0x0600076A RID: 1898 RVA: 0x00027694 File Offset: 0x00025894
	public void PurchaseNextUpgrade()
	{
		if (this.NextUpgrade.Value == null)
		{
			Debug.LogError("Trying to purchase next upgrade but there isn't one");
			return;
		}
		this.Purchase(this.NextUpgrade.Value).Take(1).Subscribe<bool>();
	}

	// Token: 0x0600076B RID: 1899 RVA: 0x000276CB File Offset: 0x000258CB
	public void PurchaseAllAvailableUpgradesOfCurrency(Upgrade.Currency currency)
	{
		this.GetQuickPurchaseableUpgrades(currency).ForEach(delegate(Upgrade x)
		{
			this.Purchase(x).Subscribe<bool>();
		});
	}

	// Token: 0x0600076C RID: 1900 RVA: 0x000276E8 File Offset: 0x000258E8
	public List<Upgrade> GetQuickPurchaseableUpgrades(Upgrade.Currency currency)
	{
		List<Upgrade> list = this.PurchasableUpgradesMap[currency].ToList<Upgrade>();
		double num = 0.0;
		if (currency == Upgrade.Currency.AngelInvestors)
		{
			num = this.aiService.AngelsOnHand.Value * 0.01;
		}
		else if (currency == Upgrade.Currency.InGameCash)
		{
			num = this.state.CashOnHand.Value;
		}
		List<Upgrade> list2 = new List<Upgrade>();
		double num2 = 0.0;
		for (int i = 0; i < list.Count; i++)
		{
			Upgrade upgrade = list[i];
			if (num2 + upgrade.cost > num)
			{
				break;
			}
			list2.Add(upgrade);
			num2 += upgrade.cost;
		}
		return list2;
	}

	// Token: 0x0600076D RID: 1901 RVA: 0x00027796 File Offset: 0x00025996
	public IObservable<bool> Purchase(Upgrade upgrade)
	{
		return Observable.Create<bool>(delegate(IObserver<bool> observer)
		{
			bool flag = false;
			if (!upgrade.IsPurchaseable.Value)
			{
				observer.OnError(new Exception("Error! Upgrade is already purchased"));
			}
			else
			{
				Upgrade.Currency currency = upgrade.currency;
				if (currency != Upgrade.Currency.InGameCash)
				{
					if (currency != Upgrade.Currency.Megabucks)
					{
						if (this.aiService.AngelsOnHand.Value < upgrade.cost)
						{
							observer.OnError(new Exception("Insufficient Angels to purchase Upgrade"));
						}
						else
						{
							this.aiService.SpendAngelInvestors(upgrade.cost);
							flag = true;
						}
					}
					else if (!this.playerData.Spend("MegaBucksBalance", upgrade.cost))
					{
						observer.OnError(new Exception("Not enough megabucks for upgrade purchase"));
					}
					else
					{
						GameController.Instance.AnalyticService.SendTaskCompleteEvent("Megabucks_Upgrade", this.state.planetName, "Upgrade Purchased: " + upgrade.id);
						flag = true;
					}
				}
				else if (this.state.CashOnHand.Value < upgrade.cost)
				{
					observer.OnError(new Exception("Not enough cash for upgrade purchase"));
				}
				else
				{
					this.state.CashOnHand.Value -= upgrade.cost;
					flag = true;
				}
				if (flag)
				{
					this.CompletePurchase(upgrade);
					observer.OnNext(true);
					observer.OnCompleted();
				}
			}
			return Disposable.Empty;
		});
	}

	// Token: 0x0600076E RID: 1902 RVA: 0x000277BC File Offset: 0x000259BC
	public void CompletePurchase(Upgrade upgrade)
	{
		if (this.state.IsEventPlanet)
		{
			this.SendEventAnalytic(upgrade.id, this.state.planetName);
		}
		upgrade.Apply(this.state);
		upgrade.IsPurchaseable.Value = false;
		if (!this.PurchasedUpgradeCollection.Contains(upgrade.id))
		{
			this.PurchasedUpgradeCollection.Add(upgrade.id);
		}
		this.SetNextUpgrade();
		this.UpgradePurchased(upgrade);
	}

	// Token: 0x0600076F RID: 1903 RVA: 0x00027838 File Offset: 0x00025A38
	public void SetNextUpgrade()
	{
		Upgrade value = this.Upgrades.FirstOrDefault(up => !up.IsPurchased.Value && up.currency == Upgrade.Currency.InGameCash);
		this.NextUpgrade.Value = value;
	}

	// Token: 0x06000770 RID: 1904 RVA: 0x0002787C File Offset: 0x00025A7C
	private void MonitorUpgradeCosts()
	{
		this.state.CashOnHand.Sample(TimeSpan.FromMilliseconds(250.0)).Subscribe(new Action<double>(this.OnCashOnHandChanged)).AddTo(this.stateDisposables);
		this.aiService.AngelsOnHand.Sample(TimeSpan.FromMilliseconds(250.0)).Subscribe(new Action<double>(this.OnAngelsOnHandChanged)).AddTo(this.stateDisposables);
		this.playerData.GetObservable("MegaBucksBalance", 0.0).Sample(TimeSpan.FromMilliseconds(250.0)).Subscribe(new Action<double>(this.OnMegaBucksChanged)).AddTo(this.stateDisposables);
	}

	// Token: 0x06000771 RID: 1905 RVA: 0x00027948 File Offset: 0x00025B48
	private void OnCashOnHandChanged(double cashOnHand)
	{
		this.allUpgradesMap[Upgrade.Currency.InGameCash].ForEach(delegate(Upgrade u)
		{
			if (u.IsPurchaseable.Value)
			{
				u.IsPurchaseable.Value = (!u.IsPurchased.Value && cashOnHand >= u.cost);
				return;
			}
			u.IsPurchaseable.Value = (!u.IsPurchased.Value && cashOnHand >= u.cost);
			if (u is ManagerUpgrade)
			{
				this.NewManagerAvailableMap[Upgrade.Currency.InGameCash].Value = (this.NewManagerAvailableMap[Upgrade.Currency.InGameCash].Value || u.IsPurchaseable.Value);
				return;
			}
			this.NewUpgradeAvailableMap[Upgrade.Currency.InGameCash].Value = (this.NewUpgradeAvailableMap[Upgrade.Currency.InGameCash].Value || u.IsPurchaseable.Value);
		});
		if (this.NewUpgradeAvailableMap[Upgrade.Currency.InGameCash].Value)
		{
			if (!this.Upgrades.Any(cashUpgrade => cashUpgrade.IsPurchaseable.Value && cashUpgrade.currency == Upgrade.Currency.InGameCash))
			{
				this.NewUpgradeAvailableMap[Upgrade.Currency.InGameCash].Value = false;
			}
		}
		if (this.NewManagerAvailableMap[Upgrade.Currency.InGameCash].Value)
		{
			if (!this.Managers.Any(cashUpgrade => cashUpgrade.IsPurchaseable.Value && cashUpgrade.currency == Upgrade.Currency.InGameCash))
			{
				this.NewManagerAvailableMap[Upgrade.Currency.InGameCash].Value = false;
			}
		}
	}

	// Token: 0x06000772 RID: 1906 RVA: 0x00027A28 File Offset: 0x00025C28
	private void OnAngelsOnHandChanged(double angelsOnHand)
	{
		this.allUpgradesMap[Upgrade.Currency.AngelInvestors].ForEach(delegate(Upgrade u)
		{
			if (u.IsPurchaseable.Value)
			{
				u.IsPurchaseable.Value = (!u.IsPurchased.Value && angelsOnHand >= u.cost);
				return;
			}
			u.IsPurchaseable.Value = (!u.IsPurchased.Value && angelsOnHand >= u.cost);
			if (u is ManagerUpgrade)
			{
				this.NewManagerAvailableMap[Upgrade.Currency.AngelInvestors].Value = (this.NewManagerAvailableMap[Upgrade.Currency.AngelInvestors].Value || u.IsPurchaseable.Value);
				return;
			}
			this.NewUpgradeAvailableMap[Upgrade.Currency.AngelInvestors].Value = (this.NewUpgradeAvailableMap[Upgrade.Currency.AngelInvestors].Value || u.IsPurchaseable.Value);
		});
		if (this.NewUpgradeAvailableMap[Upgrade.Currency.AngelInvestors].Value)
		{
			if (!this.Upgrades.Any(cashUpgrade => cashUpgrade.IsPurchaseable.Value && cashUpgrade.currency == Upgrade.Currency.AngelInvestors))
			{
				this.NewUpgradeAvailableMap[Upgrade.Currency.AngelInvestors].Value = false;
			}
		}
		if (this.NewManagerAvailableMap[Upgrade.Currency.AngelInvestors].Value)
		{
			if (!this.Managers.Any(cashUpgrade => cashUpgrade.IsPurchaseable.Value && cashUpgrade.currency == Upgrade.Currency.AngelInvestors))
			{
				this.NewManagerAvailableMap[Upgrade.Currency.AngelInvestors].Value = false;
			}
		}
	}

	// Token: 0x06000773 RID: 1907 RVA: 0x00027B08 File Offset: 0x00025D08
	private void OnMegaBucksChanged(double megaBucks)
	{
		this.allUpgradesMap[Upgrade.Currency.Megabucks].ForEach(delegate(Upgrade u)
		{
			u.IsPurchaseable.Value = (!u.IsPurchased.Value && megaBucks >= u.cost);
		});
	}

	// Token: 0x06000774 RID: 1908 RVA: 0x00027B40 File Offset: 0x00025D40
	private void SendEventAnalytic(string upgradeId, string eventId)
	{
		if (!this.playerData.GetBool("FTUpgrade_" + eventId))
		{
			GameController.Instance.AnalyticService.SendTaskCompleteEvent("eventUpgrade", eventId, "event_upgrade_" + upgradeId);
			this.playerData.SetBool("FTUpgrade_" + eventId, true);
		}
	}

	// Token: 0x06000775 RID: 1909 RVA: 0x00027B9C File Offset: 0x00025D9C
	private void UpgradePurchased(Upgrade upgradePurchased)
	{
		int num = this.Upgrades.Count(u => u.IsPurchased.Value);
		int @int = this.state.planetPlayerData.GetInt("Planet_Upgrade_Count", 0);
		if (num > @int)
		{
			this.state.planetPlayerData.Set("Planet_Upgrade_Count", num.ToString());
			GameController.Instance.AnalyticService.SendTaskCompleteEvent("Planet_Upgrade_Count", GameController.Instance.game.planetName, num.ToString());
		}
		this.OnUpgradePurchased.OnNext(upgradePurchased);
		GameController.Instance.AnalyticService.SendUpgradeAnalytic(upgradePurchased);
	}

	// Token: 0x06000776 RID: 1910 RVA: 0x00027C52 File Offset: 0x00025E52
	public static int SortUpgradesByOrderAscending(Upgrade a, Upgrade b)
	{
		if (a == null || b == null)
		{
			return 0;
		}
		if (a.order <= b.order)
		{
			return -1;
		}
		return 1;
	}

	// Token: 0x06000777 RID: 1911 RVA: 0x00027C70 File Offset: 0x00025E70
	private void MonitorPurchasbleUpgrades()
	{
		this.PurchasableUpgradesMap[Upgrade.Currency.InGameCash] = new ReactiveCollection<Upgrade>();
		this.PurchasableUpgradesMap[Upgrade.Currency.AngelInvestors] = new ReactiveCollection<Upgrade>();
		this.PurchasableUpgradesMap[Upgrade.Currency.Megabucks] = new ReactiveCollection<Upgrade>();
		for (int i = 0; i < this.Upgrades.Count; i++)
		{
			Upgrade upgrade = this.Upgrades[i];
			upgrade.IsPurchaseable.Subscribe(delegate(bool x)
			{
				if (x)
				{
					this.PurchasableUpgradesMap[upgrade.currency].Add(upgrade);
					return;
				}
				this.PurchasableUpgradesMap[upgrade.currency].Remove(upgrade);
			}).AddTo(this.stateDisposables);
		}
	}

	// Token: 0x06000778 RID: 1912 RVA: 0x00027D10 File Offset: 0x00025F10
	private void SetupAllUpgradesMap()
	{
		List<Upgrade> first = (from u in this.Upgrades
		where u.currency == Upgrade.Currency.Megabucks
		select u).ToList<Upgrade>();
		this.allUpgradesMap[Upgrade.Currency.Megabucks] = first.Concat(from u in this.Managers
		where u.currency == Upgrade.Currency.Megabucks
		select u).ToList<Upgrade>();
		List<Upgrade> first2 = (from u in this.Upgrades
		where u.currency == Upgrade.Currency.InGameCash
		select u).ToList<Upgrade>();
		this.allUpgradesMap[Upgrade.Currency.InGameCash] = first2.Concat(from u in this.Managers
		where u.currency == Upgrade.Currency.InGameCash
		select u).ToList<Upgrade>();
		List<Upgrade> first3 = (from u in this.Upgrades
		where u.currency == Upgrade.Currency.AngelInvestors
		select u).ToList<Upgrade>();
		this.allUpgradesMap[Upgrade.Currency.AngelInvestors] = first3.Concat(from u in this.Managers
		where u.currency == Upgrade.Currency.AngelInvestors
		select u).ToList<Upgrade>();
	}

	// Token: 0x040006E7 RID: 1767
	public readonly ReactiveProperty<Upgrade> NextUpgrade = new ReactiveProperty<Upgrade>();

	// Token: 0x040006E8 RID: 1768
	public readonly ReactiveProperty<bool> QuikBuyUnlocked = new ReactiveProperty<bool>(false);

	// Token: 0x040006E9 RID: 1769
	public readonly ReactiveProperty<bool> IsInitialized = new ReactiveProperty<bool>(false);

	// Token: 0x040006EA RID: 1770
	public readonly List<Upgrade> Managers = new List<Upgrade>();

	// Token: 0x040006EB RID: 1771
	public readonly List<Upgrade> Upgrades = new List<Upgrade>();

	// Token: 0x040006EC RID: 1772
	public Subject<Upgrade> OnUpgradePurchased = new Subject<Upgrade>();

	// Token: 0x040006ED RID: 1773
	public Dictionary<Upgrade.Currency, ReactiveCollection<Upgrade>> PurchasableUpgradesMap = new Dictionary<Upgrade.Currency, ReactiveCollection<Upgrade>>();

	// Token: 0x040006EE RID: 1774
	public Dictionary<Upgrade.Currency, ReactiveProperty<bool>> NewUpgradeAvailableMap = new Dictionary<Upgrade.Currency, ReactiveProperty<bool>>();

	// Token: 0x040006EF RID: 1775
	public Dictionary<Upgrade.Currency, ReactiveProperty<bool>> NewManagerAvailableMap = new Dictionary<Upgrade.Currency, ReactiveProperty<bool>>();

	// Token: 0x040006F0 RID: 1776
	public ReactiveCollection<string> PurchasedUpgradeCollection = new ReactiveCollection<string>();

	// Token: 0x040006F1 RID: 1777
	private Dictionary<Upgrade.Currency, List<Upgrade>> allUpgradesMap = new Dictionary<Upgrade.Currency, List<Upgrade>>();

	// Token: 0x040006F2 RID: 1778
	private GameState state;

	// Token: 0x040006F3 RID: 1779
	private PlayerData playerData;

	// Token: 0x040006F4 RID: 1780
	private MainUIController uiController;

	// Token: 0x040006F5 RID: 1781
	private IAngelInvestorService aiService;

	// Token: 0x040006F6 RID: 1782
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040006F7 RID: 1783
	private CompositeDisposable stateDisposables = new CompositeDisposable();
}
