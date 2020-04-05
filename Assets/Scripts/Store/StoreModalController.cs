using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000F9 RID: 249
public class StoreModalController : ModalController, IModalWithParams<StoreModalController.Parameters>
{
	// Token: 0x0600067A RID: 1658 RVA: 0x00022CCC File Offset: 0x00020ECC
	public override void Init(NavigationService navService)
	{
		base.Init(navService);
		this.loadingSpinner.SetActive(true);
		GameController.Instance.game.planetPlayerData.GetObservable("Multipliers", 0.0).CombineLatest(GameController.Instance.game.SubscriptionMultiplier, (double mult, float subscription) => mult * 3.0 + (double)subscription).Subscribe(delegate(double x)
		{
			this.txt_MultiplierValue.text = "x" + x;
		}).AddTo(base.gameObject);
		GameController.Instance.StoreService.CurrentCatalog.ObserveCountChanged(true).First((int x) => x > 0).Subscribe(delegate(int _)
		{
			this.loadingSpinner.SetActive(false);
		}).AddTo(base.gameObject);
		this.ShowPanel(this.parameters.initialPanelToOpen);
		MessageBroker.Default.Receive<StorePurchaseEvent>().Subscribe(new Action<StorePurchaseEvent>(this.OnStorePurchaseEvent)).AddTo(base.gameObject);
	}

	// Token: 0x0600067B RID: 1659 RVA: 0x00022DEC File Offset: 0x00020FEC
	public void OnDestroy()
	{
		IStoreService storeService = GameController.Instance.StoreService;
	}

	// Token: 0x0600067C RID: 1660 RVA: 0x00022DF9 File Offset: 0x00020FF9
	public void SetParams(StoreModalController.Parameters p)
	{
		this.parameters = p;
	}

	// Token: 0x0600067D RID: 1661 RVA: 0x00022E04 File Offset: 0x00021004
	public void ShowItemDetails(string itemId, int qty)
	{
		Item itemById = GameController.Instance.GlobalPlayerData.inventory.GetItemById(itemId);
		this.NavService.CreateModal<ItemDetailModal>(NavModals.ITEM_DETAIL, false).ShowItem(itemById, false, null, qty, true);
		this.SendStoreAnalytics("bundleid", "info");
	}

	// Token: 0x0600067E RID: 1662 RVA: 0x00022E52 File Offset: 0x00021052
	public void ShowStorePanel(string panelId, string scrollToTarget)
	{
		this.ShowStorePanel(panelId, scrollToTarget, StoreModalController.ShowAction.ScrollToTarget);
	}

	// Token: 0x0600067F RID: 1663 RVA: 0x00022E5D File Offset: 0x0002105D
	public void ShowStorePanel(string panelId, string actionId, StoreModalController.ShowAction action)
	{
		if (action == StoreModalController.ShowAction.ScrollToTop)
		{
			this.panelMap[panelId].GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
			return;
		}
		if (action != StoreModalController.ShowAction.ScrollToTarget)
		{
			return;
		}
		this.panelMap[panelId].GetComponent<StoreView>().ScrollTo(actionId);
	}

	// Token: 0x06000680 RID: 1664 RVA: 0x00022E9C File Offset: 0x0002109C
	public void SendStoreAnalytics(string bundleId, string action)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics(bundleId, GameController.Instance.game.planetName + "_" + this.toggleMap.FirstOrDefault((KeyValuePair<string, Toggle> x) => x.Value.isOn).Key, action);
	}

	// Token: 0x06000681 RID: 1665 RVA: 0x00022F08 File Offset: 0x00021108
	private void OnStorePurchaseEvent(StorePurchaseEvent evt)
	{
		switch (evt.PurchaseState)
		{
		case EStorePurchaseState.Started:
			this.EnablePurchaseBlocker();
			return;
		case EStorePurchaseState.Success:
			this.OnPurchaseCompleted(true, "");
			return;
		case EStorePurchaseState.Fail:
			this.OnPurchaseCompleted(false, evt.Error);
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	// Token: 0x06000682 RID: 1666 RVA: 0x00022F58 File Offset: 0x00021158
	private void OnPurchaseCompleted(bool success, string message)
	{
		this.DisablePurchaseBlocker();
		if (!success && message != "UserCancelled" && message != "Insufficient Funds")
		{
			GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Oh no!", message, null, PopupModal.PopupOptions.OK, "Ok", "", true, null, "");
		}
	}

	// Token: 0x06000683 RID: 1667 RVA: 0x00022FBB File Offset: 0x000211BB
	private void EnablePurchaseBlocker()
	{
		this.purchaseBlocker.SetActive(true);
		this.AllowBackButtonToClose = false;
	}

	// Token: 0x06000684 RID: 1668 RVA: 0x00022FD0 File Offset: 0x000211D0
	public void DisablePurchaseBlocker()
	{
		this.purchaseBlocker.SetActive(false);
		this.AllowBackButtonToClose = true;
	}

	// Token: 0x06000685 RID: 1669 RVA: 0x00022FE5 File Offset: 0x000211E5
	public override void CloseModal(Unit u)
	{
		this.SendStoreAnalytics("none", "exit");
		base.CloseModal(u);
	}

	// Token: 0x040005FC RID: 1532
	public static string PANEL_SERVER_CONTROLED_DEFAULT = "Default";

	// Token: 0x040005FD RID: 1533
	public const string PANEL_GOLD = "Gold";

	// Token: 0x040005FE RID: 1534
	public const string PANEL_BOOSTS = "Boost";

	// Token: 0x040005FF RID: 1535
	public const string PANEL_ITEMS = "Items";

	// Token: 0x04000600 RID: 1536
	private StoreModalController.Parameters parameters;

	// Token: 0x04000601 RID: 1537
	[SerializeField]
	private Text txt_MultiplierValue;

	// Token: 0x04000602 RID: 1538
	[SerializeField]
	private GameObject purchaseBlocker;

	// Token: 0x04000603 RID: 1539
	[SerializeField]
	private GameObject loadingSpinner;

	// Token: 0x020007E4 RID: 2020
	public enum ShowAction
	{
		// Token: 0x040028F3 RID: 10483
		None,
		// Token: 0x040028F4 RID: 10484
		ScrollToTop,
		// Token: 0x040028F5 RID: 10485
		ScrollToTarget
	}

	// Token: 0x020007E5 RID: 2021
	public class Parameters
	{
		// Token: 0x040028F6 RID: 10486
		public string initialPanelToOpen;
	}
}
