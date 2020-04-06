using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UniRx;

namespace Platforms.Store
{
	// Token: 0x020006DC RID: 1756
	public class PlayFabVirtualCurrency : IPlatformStoreExtension, IPlatform, IDisposable
	{
		// Token: 0x14000298 RID: 664
		// (add) Token: 0x06002456 RID: 9302 RVA: 0x0009D678 File Offset: 0x0009B878
		// (remove) Token: 0x06002457 RID: 9303 RVA: 0x0009D6B0 File Offset: 0x0009B8B0
		public event Action<PurchaseEvent> PurchaseSuccess;

		// Token: 0x14000299 RID: 665
		// (add) Token: 0x06002458 RID: 9304 RVA: 0x0009D6E8 File Offset: 0x0009B8E8
		// (remove) Token: 0x06002459 RID: 9305 RVA: 0x0009D720 File Offset: 0x0009B920
		public event Action<PurchaseEvent> PurchaseFail;

		// Token: 0x0600245A RID: 9306 RVA: 0x0000F40F File Offset: 0x0000D60F
		public int EnabledForPlatform(PlatformType platformType)
		{
			return 0;
		}

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x0600245B RID: 9307 RVA: 0x0009D755 File Offset: 0x0009B955
		public CurrencyType CurrencyType
		{
			get
			{
				return CurrencyType.Virtual;
			}
		}

		// Token: 0x0600245C RID: 9308 RVA: 0x0009D758 File Offset: 0x0009B958
		public IObservable<Unit> OnLogin(IPlatformStore platformStore, LoginResult result)
		{
			this.playFab = platformStore.PlayFab;
			return Observable.ReturnUnit();
		}

		// Token: 0x0600245D RID: 9309 RVA: 0x00002718 File Offset: 0x00000918
		public void Dispose()
		{
		}

		// Token: 0x0600245E RID: 9310 RVA: 0x0009D76B File Offset: 0x0009B96B
		public IObservable<Unit> ProcessCatalog(List<CatalogStoreItem> catalog)
		{
			return Observable.ReturnUnit();
		}

		// Token: 0x0600245F RID: 9311 RVA: 0x0009D774 File Offset: 0x0009B974
		public void Purchase(CatalogStoreItem item, string currencyCode)
		{
			Action<PurchaseItemResult> onResult = delegate(PurchaseItemResult result)
			{
				if (this.PurchaseSuccess != null)
				{
					this.PurchaseSuccess(new PurchaseEvent(item, currencyCode, null, result));
				}
			};
			Action<PlayFabError> onError = delegate(PlayFabError error)
			{
				if (this.PurchaseFail != null)
				{
					this.PurchaseFail(new PurchaseEvent(item, currencyCode, error));
				}
			};
			this.playFab.PurchaseItem(item.ItemId, item.CatalogItem.CatalogVersion, item.StoreId, currencyCode, (int)item.StoreItem.VirtualCurrencyPrices[currencyCode], onResult, onError);
		}

		// Token: 0x04002512 RID: 9490
		private PlayFabWrapper playFab;
	}
}
