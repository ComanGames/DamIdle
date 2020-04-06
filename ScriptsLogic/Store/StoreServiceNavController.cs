using System;
using HHTools.Navigation;
using UniRx;
using UnityEngine;

// Token: 0x020000FA RID: 250
public class StoreServiceNavController : MonoBehaviour
{
	// Token: 0x0600068A RID: 1674 RVA: 0x00023035 File Offset: 0x00021235
	public void Awake()
	{
		MessageBroker.Default.Receive<StoreServiceNavController.OpenStoreCommand>().Subscribe(delegate(StoreServiceNavController.OpenStoreCommand request)
		{
			this.OnClickShowStore(request.panelToShow, request.nav_element_name_analytic, request.analyticSource, request.CanStackOverride);
		}).AddTo(base.gameObject);
	}

	// Token: 0x0600068B RID: 1675 RVA: 0x00023060 File Offset: 0x00021260
	public void OnClickShowStore(string panelToShow, string nav_element_name_analytic, string analyticSource, bool canStackOverride)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics(nav_element_name_analytic, GameController.Instance.game.planetName + "_" + analyticSource, "Bundle");
		MessageBroker.Default.Publish<TimeWarpValueUpdateEvent>(new TimeWarpValueUpdateEvent());
		StoreModalController.Parameters modalParams = new StoreModalController.Parameters
		{
			initialPanelToOpen = panelToShow
		};
		GameController.Instance.NavigationService.CreateModal<StoreModalController, StoreModalController.Parameters>(NavModals.STORE_PARAMETERS, modalParams, canStackOverride);
	}

	// Token: 0x020007E7 RID: 2023
	public struct OpenStoreCommand
	{
		// Token: 0x06002893 RID: 10387 RVA: 0x000A7D6C File Offset: 0x000A5F6C
		public OpenStoreCommand(string panelToShow, string nav_element_name_analytic, string analyticSource, string focusItem = null, bool canStackOverride = false)
		{
			this.panelToShow = panelToShow;
			this.nav_element_name_analytic = nav_element_name_analytic;
			this.analyticSource = analyticSource;
			this.FocusItem = focusItem;
			this.CanStackOverride = canStackOverride;
		}

		// Token: 0x040028FB RID: 10491
		public string panelToShow;

		// Token: 0x040028FC RID: 10492
		public string nav_element_name_analytic;

		// Token: 0x040028FD RID: 10493
		public string analyticSource;

		// Token: 0x040028FE RID: 10494
		public string FocusItem;

		// Token: 0x040028FF RID: 10495
		public bool CanStackOverride;
	}
}
