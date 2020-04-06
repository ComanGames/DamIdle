using System;
using Platforms;
using UniRx;
using Utils;

namespace AdCap.Store
{
	// Token: 0x020006ED RID: 1773
	public interface IStoreService : IDisposable
	{
		// Token: 0x17000303 RID: 771
		// (get) Token: 0x060024B5 RID: 9397
		ReactiveCollection<AdCapStoreItem> CurrentCatalog { get; }

		// Token: 0x17000304 RID: 772
		// (get) Token: 0x060024B6 RID: 9398
		ReactiveCollection<ItemPurchaseData> PurchaseData { get; }

		// Token: 0x17000305 RID: 773
		// (get) Token: 0x060024B7 RID: 9399
		ReactiveProperty<int> TotalRMPurchaseCount { get; }

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x060024B8 RID: 9400
		ReactiveCollection<AdCapStoreItem> ActiveLimitedTimeItems { get; }

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x060024B9 RID: 9401
		ReactiveCollection<AdCapStoreItem> NewTimedItems { get; }

		// Token: 0x060024BA RID: 9402
		void Init(IGameController gameController, IUserDataService userDataService, IGrantRewardService grantRewardService, IDateTimeService dateTimeService, UpgradeService upgradeService, IAngelInvestorService angelInvestorService, TimerService timerService, ITriggerService triggerService, PlayerData globalPlayerData, IInventoryService inventoryService, IPlatformStore platformStore);

		// Token: 0x060024BB RID: 9403
		bool AttemptPurchase(string id, Currency currency);

		// Token: 0x060024BC RID: 9404
		bool AttemptPurchase(AdCapStoreItem item, Currency currency);

		// Token: 0x060024BD RID: 9405
		void SavePurchaseData();

		// Token: 0x060024BE RID: 9406
		void LoadPurchaseData();
	}
}
