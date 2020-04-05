using System;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000B3 RID: 179
public class MenuPanelBase : MonoBehaviour
{
	// Token: 0x060004CC RID: 1228 RVA: 0x00019470 File Offset: 0x00017670
	private void Awake()
	{
		this.btn_Career.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowCareer)).AddTo(base.gameObject);
		this.btn_Unlocks.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowUnlocks)).AddTo(base.gameObject);
		this.btn_Upgrades.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowUpgrades)).AddTo(base.gameObject);
		this.btn_Managers.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowManagers)).AddTo(base.gameObject);
		this.btn_Investors.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowInvestors)).AddTo(base.gameObject);
		this.btn_Adventures.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowAdventures)).AddTo(base.gameObject);
		this.btn_Store.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowStore)).AddTo(base.gameObject);
		this.btn_Connect.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowConnect)).AddTo(base.gameObject);
		this.btn_Offerwall.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnClickShowOfferwall)).AddTo(base.gameObject);
		GameController.Instance.IsInitialized.First((bool x) => x).Subscribe(new Action<bool>(this.OnGameInitialized)).AddTo(base.gameObject);
	}

	// Token: 0x060004CD RID: 1229 RVA: 0x00019639 File Offset: 0x00017839
	protected virtual void ShowCareer(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Open_Career", "Menu", "");
		this.navService.CreateModal<CareerModalController>(NavModals.CAREER, false);
	}

	// Token: 0x060004CE RID: 1230 RVA: 0x0001966B File Offset: 0x0001786B
	protected virtual void ShowUnlocks(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Open_Unlocks", "Menu", "");
		this.navService.CreateModal<UnlocksModalController>(NavModals.UNLOCKS, false);
		FTUE_Manager.ShowFTUE("Unlocks", null);
	}

	// Token: 0x060004CF RID: 1231 RVA: 0x000196AC File Offset: 0x000178AC
	protected virtual void ShowUpgrades(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Open_Upgrades", "Menu", "");
		if (KongregateAPI.GetAPI() != null)
		{
			KongregateAPI.GetAPI().Analytics.AddEvent("delta.menu.open", null);
		}
		this.navService.CreateModal<UpgradeModalController>(NavModals.UPGRADES, false);
	}

	// Token: 0x060004D0 RID: 1232 RVA: 0x00019705 File Offset: 0x00017905
	protected virtual void ShowManagers(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Open_Managers", "Menu", "");
		this.navService.CreateModal<ManagerModalController>(NavModals.MANAGERS, false);
	}

	// Token: 0x060004D1 RID: 1233 RVA: 0x00019737 File Offset: 0x00017937
	protected virtual void ShowInvestors(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Open_Investors", "Menu", "");
		this.navService.CreateModal<InvestorsModal>(NavModals.INVESTORS, false).WireData();
	}

	// Token: 0x060004D2 RID: 1234 RVA: 0x0001976D File Offset: 0x0001796D
	protected virtual void ShowAdventures(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Open_AdVentures", "Menu", "");
		this.navService.CreateModal<AdventuresModalController>(NavModals.ADVENTURES_PANEL, false);
		FTUE_Manager.ShowFTUE("MissionControl", null);
	}

	// Token: 0x060004D3 RID: 1235 RVA: 0x000197AB File Offset: 0x000179AB
	protected virtual void ShowStore(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Open_Store", "Menu", "");
		MessageBroker.Default.Publish<StoreServiceNavController.OpenStoreCommand>(new StoreServiceNavController.OpenStoreCommand(StoreModalController.PANEL_SERVER_CONTROLED_DEFAULT, "Store", "StoreButton", null, false));
	}

	// Token: 0x060004D4 RID: 1236 RVA: 0x000197EB File Offset: 0x000179EB
	protected virtual void ShowConnect(Unit u)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Click", "ConnectButton", base.gameObject.name);
		this.navService.CreateModal<InfoModalController>(NavModals.CONNECT, false);
	}

	// Token: 0x060004D5 RID: 1237 RVA: 0x00002718 File Offset: 0x00000918
	protected virtual void OnClickShowOfferwall(Unit u)
	{
	}

	// Token: 0x060004D6 RID: 1238 RVA: 0x00019823 File Offset: 0x00017A23
	private void OnGameInitialized(bool isInitialized)
	{
		this.navService = GameController.Instance.NavigationService;
	}

	// Token: 0x04000449 RID: 1097
	[SerializeField]
	protected Button btn_Career;

	// Token: 0x0400044A RID: 1098
	[SerializeField]
	protected Button btn_Unlocks;

	// Token: 0x0400044B RID: 1099
	[SerializeField]
	protected Button btn_Upgrades;

	// Token: 0x0400044C RID: 1100
	[SerializeField]
	protected Button btn_Managers;

	// Token: 0x0400044D RID: 1101
	[SerializeField]
	protected Button btn_Investors;

	// Token: 0x0400044E RID: 1102
	[SerializeField]
	protected Button btn_Adventures;

	// Token: 0x0400044F RID: 1103
	[SerializeField]
	protected Button btn_Store;

	// Token: 0x04000450 RID: 1104
	[SerializeField]
	protected Button btn_Connect;

	// Token: 0x04000451 RID: 1105
	[SerializeField]
	protected Button btn_Offerwall;

	// Token: 0x04000452 RID: 1106
	private NavigationService navService;
}
