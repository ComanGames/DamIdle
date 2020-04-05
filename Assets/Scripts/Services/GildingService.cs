using System;
using System.Linq;
using AdCap.Store;
using HHTools.Navigation;
using UniRx;
using UnityEngine;

// Token: 0x020001B3 RID: 435
public class GildingService : IDisposable
{
	// Token: 0x06000CF6 RID: 3318 RVA: 0x0003A18F File Offset: 0x0003838F
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000CF7 RID: 3319 RVA: 0x0003A19C File Offset: 0x0003839C
	public void Init()
	{
		(from x in GameController.Instance.State
		where x != null
		select x).Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
		GameController.Instance.OnLoadNewPlanetPre += this.OnPrePlanetLoad;
	}

	// Token: 0x06000CF8 RID: 3320 RVA: 0x0003A20A File Offset: 0x0003840A
	private void OnPrePlanetLoad()
	{
		this.ShowingGild.Value = false;
		this.AllVenturesBoosted.Value = false;
		this.selectedVenture = null;
	}

	// Token: 0x06000CF9 RID: 3321 RVA: 0x0003A22C File Offset: 0x0003842C
	public void OnStateChanged(GameState state)
	{
		this.stateDisposables.Clear();
		this.state = state;
		if (!state.IsEventPlanet)
		{
			int num = 0;
			for (int i = 0; i < state.VentureModels.Count; i++)
			{
				VentureModel ventureModel = state.VentureModels[i];
				if (ventureModel.gildLevel.Value > 0)
				{
					ventureModel.gildLevel.Value = 0;
					num++;
				}
			}
			if (num > 0)
			{
				GameController.Instance.AnalyticService.SendPlatniumUpgradeFixedEvent(state.planetName, num);
			}
		}
		this.ShowingGild.SetValueAndForceNotify(state.IsEventPlanet);
		this.GildLevelLimit = (state.IsEventPlanet ? 2 : 1);
		foreach (VentureModel venture in state.VentureModels)
		{
			this.OnVentureAdded(venture);
		}
		(from x in state.VentureModels.ObserveAdd()
		select x.Value).Subscribe(new Action<VentureModel>(this.OnVentureAdded)).AddTo(this.stateDisposables);
		state.PlatinumUpgradesMultiplier.Subscribe(new Action<double>(this.GetPlatinumGildBoostConfig)).AddTo(this.stateDisposables);
	}

	// Token: 0x06000CFA RID: 3322 RVA: 0x0003A384 File Offset: 0x00038584
	public void PurchaseGilding(VentureModel ventureModel)
	{
		string currentGildingItemID = (ventureModel.gildLevel.Value > 0) ? "venture_gilding_platinum" : "venture_gilding";
		AdCapStoreItem item = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault((AdCapStoreItem x) => x.Id == currentGildingItemID);
		GameController.Instance.NavigationService.CreateModal<GildingPurchaseModal>(NavModals.GILDING_PURCHASE, false).WireData(ventureModel, item);
		this.selectedVenture = ventureModel;
	}

	// Token: 0x06000CFB RID: 3323 RVA: 0x0003A3FB File Offset: 0x000385FB
	public void OnVentureGildingPurchased()
	{
		this.state.IncreaseGildLevel(this.selectedVenture, 1);
		this.ShowingGild.SetValueAndForceNotify(this.state.IsEventPlanet);
	}

	// Token: 0x06000CFC RID: 3324 RVA: 0x0003A425 File Offset: 0x00038625
	public void OnPlatniumGildingPurchased()
	{
		this.state.planetPlayerData.Set("Platinum Upgrade", this.GildBoostConfig.Value.nextBoostValue.ToString());
	}

	// Token: 0x06000CFD RID: 3325 RVA: 0x0003A452 File Offset: 0x00038652
	private void OnVentureAdded(VentureModel venture)
	{
		venture.IsBoosted.Subscribe(new Action<bool>(this.OnVentureBoostChanged)).AddTo(this.stateDisposables);
	}

	// Token: 0x06000CFE RID: 3326 RVA: 0x0003A477 File Offset: 0x00038677
	private void OnVentureBoostChanged(bool isBoosted)
	{
		this.AllVenturesBoosted.Value = GameController.Instance.game.VentureModels.All((VentureModel x) => x.IsBoosted.Value);
	}

	// Token: 0x06000CFF RID: 3327 RVA: 0x0003A4B8 File Offset: 0x000386B8
	private void GetPlatinumGildBoostConfig(double currentBoostValue)
	{
		string storeItemId = "";
		double nextBoostValue = 0.0;
		bool flag = false;
		if (Math.Abs(currentBoostValue - 17.77) < 1E-05 || currentBoostValue == 0.0)
		{
			nextBoostValue = 77.77;
			storeItemId = "platinum_upgrade_60";
		}
		else if (Math.Abs(currentBoostValue - 77.77) < 1E-05)
		{
			nextBoostValue = 777.77;
			storeItemId = "platinum_upgrade_700";
		}
		else if (Math.Abs(currentBoostValue - 777.77) < 1E-05)
		{
			nextBoostValue = 7777.77;
			storeItemId = "platinum_upgrade_7000";
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			this.GildBoostConfig.Value = new GildingService.GildBoostConfiguration("", currentBoostValue, currentBoostValue, true);
			return;
		}
		if (string.IsNullOrEmpty(storeItemId))
		{
			Debug.LogError("Unknown Gilding storeitemid " + storeItemId);
			return;
		}
		AdCapStoreItem adCapStoreItem = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault((AdCapStoreItem x) => x.Id == storeItemId);
		if (adCapStoreItem == null)
		{
			GameController.Instance.StoreService.CurrentCatalog.ObserveAdd().First((CollectionAddEvent<AdCapStoreItem> x) => x.Value.Id == storeItemId).Subscribe(delegate(CollectionAddEvent<AdCapStoreItem> _)
			{
				this.GetPlatinumGildBoostConfig(currentBoostValue);
			}).AddTo(this.stateDisposables);
			return;
		}
		this.GildBoostConfig.Value = new GildingService.GildBoostConfiguration(adCapStoreItem.Id, currentBoostValue, nextBoostValue, flag);
	}

	// Token: 0x04000B03 RID: 2819
	public GameObject buyButtonPrefab;

	// Token: 0x04000B04 RID: 2820
	public int GildLevelLimit = 3;

	// Token: 0x04000B05 RID: 2821
	public ReactiveProperty<GildingService.GildBoostConfiguration> GildBoostConfig = new ReactiveProperty<GildingService.GildBoostConfiguration>();

	// Token: 0x04000B06 RID: 2822
	public readonly ReactiveProperty<bool> ShowingGild = new ReactiveProperty<bool>(false);

	// Token: 0x04000B07 RID: 2823
	public readonly BoolReactiveProperty AllVenturesBoosted = new BoolReactiveProperty(false);

	// Token: 0x04000B08 RID: 2824
	private const string GOLD_GILD_ITEM_ID = "venture_gilding";

	// Token: 0x04000B09 RID: 2825
	private const string PLATINUM_GILD_ITEM_ID = "venture_gilding_platinum";

	// Token: 0x04000B0A RID: 2826
	private VentureModel selectedVenture;

	// Token: 0x04000B0B RID: 2827
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000B0C RID: 2828
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x04000B0D RID: 2829
	private GameState state;

	// Token: 0x02000883 RID: 2179
	public class GildBoostConfiguration
	{
		// Token: 0x06002B4D RID: 11085 RVA: 0x000ADA51 File Offset: 0x000ABC51
		public GildBoostConfiguration(string itemID, double currentBoostValue, double nextBoostValue, bool allUpgradesComplete = false)
		{
			this.storeItemID = itemID;
			this.currentBoostValue = currentBoostValue;
			this.nextBoostValue = nextBoostValue;
			this.allUpgradesComplete = allUpgradesComplete;
		}

		// Token: 0x04002B38 RID: 11064
		public string storeItemID;

		// Token: 0x04002B39 RID: 11065
		public double currentBoostValue;

		// Token: 0x04002B3A RID: 11066
		public double nextBoostValue;

		// Token: 0x04002B3B RID: 11067
		public bool allUpgradesComplete;
	}

	// Token: 0x02000884 RID: 2180
	public struct RequestEnableGildingMode
	{
	}
}
