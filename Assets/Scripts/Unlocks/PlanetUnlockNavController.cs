using System;
using System.Linq;
using AdCap.Store;
using HHTools.Navigation;
using UniRx;
using UnityEngine;

// Token: 0x020000CF RID: 207
public class PlanetUnlockNavController : MonoBehaviour
{
	// Token: 0x0600058D RID: 1421 RVA: 0x0001CF70 File Offset: 0x0001B170
	private void Awake()
	{
		GameController.Instance.State.First((GameState x) => x != null).Subscribe(new Action<GameState>(this.OnInitilized)).AddTo(base.gameObject);
	}

	// Token: 0x0600058E RID: 1422 RVA: 0x0001CFC8 File Offset: 0x0001B1C8
	private void OnInitilized(GameState state)
	{
		GameController.Instance.PlanetDataList.ForEach(new Action<PlanetData>(this.WireUpListner));
	}

	// Token: 0x0600058F RID: 1423 RVA: 0x0001CFE8 File Offset: 0x0001B1E8
	private void WireUpListner(PlanetData data)
	{
		if (!GameController.Instance.GlobalPlayerData.GetBool(data.PlanetName + "hasSeenUnlockModal"))
		{
			Item itemById = GameController.Instance.GlobalPlayerData.inventory.GetItemById(data.PlanetName.ToLower() + "_unlock");
			if (itemById != null && itemById.Owned.Value == 0)
			{
				AdCapStoreItem adCapStoreItem = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault((AdCapStoreItem x) => x.Id == data.PurchaseId);
				if (adCapStoreItem != null)
				{
					this.OnPlanetUnlockItemAdded(adCapStoreItem, data);
					return;
				}
				(from x in GameController.Instance.StoreService.CurrentCatalog.ObserveAdd().First((CollectionAddEvent<AdCapStoreItem> x) => x.Value.Id == data.PurchaseId)
				select x.Value).Subscribe(delegate(AdCapStoreItem x)
				{
					this.OnPlanetUnlockItemAdded(x, data);
				}).AddTo(base.gameObject);
			}
		}
	}

	// Token: 0x06000590 RID: 1424 RVA: 0x0001D110 File Offset: 0x0001B310
	private void OnPlanetUnlockItemAdded(AdCapStoreItem storeItem, PlanetData data)
	{
		switch (storeItem.Cost.Currency)
		{
		case Currency.NA:
		case Currency.Gold:
		case Currency.Cash:
		case Currency.Kreds:
		case Currency.Inventory:
			break;
		case Currency.MegaBuck:
			GameController.Instance.GlobalPlayerData.GetObservable("MegaBucksBalance", 0.0).First((double x) => x >= storeItem.Cost.Price).Subscribe(delegate(double _)
			{
				this.OnCanAffordPlanet(data);
			}).AddTo(base.gameObject);
			return;
		case Currency.InGameCash:
			GameController.Instance.game.CashOnHand.First((double x) => x >= storeItem.Cost.Price).Subscribe(delegate(double _)
			{
				this.OnCanAffordPlanet(data);
			}).AddTo(base.gameObject);
			break;
		default:
			return;
		}
	}

	// Token: 0x06000591 RID: 1425 RVA: 0x0001D1F8 File Offset: 0x0001B3F8
	private void OnCanAffordPlanet(PlanetData data)
	{
		GameController.Instance.GlobalPlayerData.SetBool(data.PlanetName + "hasSeenUnlockModal", true);
		GameController.Instance.NavigationService.CreateModal<PlanetUnlockModal>(NavModals.PLANET_UNLOCK, false).WireData(data);
		GameController.Instance.AnalyticService.SendTaskCompleteEvent("PlanetUnlocked", data.PlanetName, "");
	}
}
