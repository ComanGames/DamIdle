using System;
using AdCap.Store;
using UniRx;

// Token: 0x020000D0 RID: 208
public class PlanetUnlockService : IDisposable
{
	// Token: 0x06000593 RID: 1427 RVA: 0x0001D25F File Offset: 0x0001B45F
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000594 RID: 1428 RVA: 0x0001D26C File Offset: 0x0001B46C
	public void Init()
	{
		this.inventory = GameController.Instance.GlobalPlayerData.inventory;
		(from evt in MessageBroker.Default.Receive<StorePurchaseEvent>()
		where evt.PurchaseState == EStorePurchaseState.Success
		select evt).Subscribe(new Action<StorePurchaseEvent>(this.OnStoreItemPurchased)).AddTo(this.disposables);
	}

	// Token: 0x06000595 RID: 1429 RVA: 0x0001D2DC File Offset: 0x0001B4DC
	private void OnStoreItemPurchased(StorePurchaseEvent e)
	{
		for (int i = 0; i < e.Item.Rewards.Count; i++)
		{
			if (e.Item.Rewards[i].RewardType == ERewardType.Item)
			{
				Item itemById = this.inventory.GetItemById(e.Item.Rewards[i].Id);
				if (itemById != null && itemById.Product == Product.PlanetUnlock)
				{
					this.OnPlanetUnlockPurchase(e.Item.Rewards[i].Id);
				}
			}
		}
	}

	// Token: 0x06000596 RID: 1430 RVA: 0x0001D368 File Offset: 0x0001B568
	private void OnPlanetUnlockPurchase(string id)
	{
		this.inventory.AddItem(id, 1, true, true);
		MessageBroker.Default.Publish<PlanetPurchasedEvent>(new PlanetPurchasedEvent(GameController.Instance.planetName));
	}

	// Token: 0x040004FE RID: 1278
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040004FF RID: 1279
	private IInventoryService inventory;
}
