using System;
using System.Collections.Generic;
using PlayFab.ClientModels;
using UniRx;

namespace Platforms
{
	// Token: 0x020006C7 RID: 1735
	public interface IPlatformStoreExtension : IPlatform, IDisposable
	{
		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06002345 RID: 9029
		CurrencyType CurrencyType { get; }

		// Token: 0x06002346 RID: 9030
		IObservable<Unit> OnLogin(IPlatformStore platformStore, LoginResult result);

		// Token: 0x06002347 RID: 9031
		IObservable<Unit> ProcessCatalog(List<CatalogStoreItem> catalog);

		// Token: 0x06002348 RID: 9032
		void Purchase(CatalogStoreItem item, string currencyCode);

		// Token: 0x14000295 RID: 661
		// (add) Token: 0x06002349 RID: 9033
		// (remove) Token: 0x0600234A RID: 9034
		event Action<PurchaseEvent> PurchaseSuccess;

		// Token: 0x14000296 RID: 662
		// (add) Token: 0x0600234B RID: 9035
		// (remove) Token: 0x0600234C RID: 9036
		event Action<PurchaseEvent> PurchaseFail;
	}
}
