using System;
using System.Collections.Generic;
using PlayFab.ClientModels;

namespace Platforms
{
	// Token: 0x020006C6 RID: 1734
	public interface IPlatformStore
	{
		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x06002331 RID: 9009
		IPlatformStoreExtension RealMoney { get; }

		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x06002332 RID: 9010
		IPlatformStoreExtension VirtualCurrency { get; }

		// Token: 0x170002CA RID: 714
		// (get) Token: 0x06002333 RID: 9011
		PlayFabWrapper PlayFab { get; }

		// Token: 0x170002CB RID: 715
		// (get) Token: 0x06002334 RID: 9012
		PlatformAccount Account { get; }

		// Token: 0x170002CC RID: 716
		// (get) Token: 0x06002335 RID: 9013
		List<CatalogItem> Catalog { get; }

		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06002336 RID: 9014
		GetStoreItemsResult[] Stores { get; }

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06002337 RID: 9015
		List<CatalogStoreItem> CatalogStore { get; }

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06002338 RID: 9016
		Dictionary<string, CatalogStoreItem> StoreMap { get; }

		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06002339 RID: 9017
		Dictionary<string, CatalogItem> CatalogMap { get; }

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x0600233A RID: 9018
		IObservable<List<CatalogStoreItem>> Ready { get; }

		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x0600233B RID: 9019
		IObservable<PurchaseEvent> OnPurchaseStart { get; }

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x0600233C RID: 9020
		IObservable<PurchaseEvent> OnPurchaseSuccess { get; }

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x0600233D RID: 9021
		IObservable<PurchaseEvent> OnPurchaseFail { get; }

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x0600233E RID: 9022
		IObservable<PurchaseEvent> OnPurchaseSuccessHistory { get; }

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x0600233F RID: 9023
		IObservable<Exception> OnInitializationError { get; }

		// Token: 0x06002340 RID: 9024
		IPlatformStore Init(PlayFabWrapper playFab, PlatformAccount account);

		// Token: 0x06002341 RID: 9025
		void Dispose();

		// Token: 0x06002342 RID: 9026
		void Purchase(string itemId, string currencyCode, Action<PurchaseEvent> onSuccess, Action<PurchaseEvent> onError);

		// Token: 0x06002343 RID: 9027
		void Purchase(CatalogStoreItem item, string currencyCode, Action<PurchaseEvent> onSuccess, Action<PurchaseEvent> onError);

		// Token: 0x06002344 RID: 9028
		void RefreshStore(Action onComplete);
	}
}
