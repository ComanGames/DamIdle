using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Platforms.Logger;
using UniRx;
using UnityEngine.Purchasing;

namespace Platforms.Store
{
	// Token: 0x020006DD RID: 1757
	public class SteamStore : IPlatformStoreExtension, IPlatform, IDisposable
	{
		// Token: 0x1400029A RID: 666
		// (add) Token: 0x06002461 RID: 9313 RVA: 0x0009D80C File Offset: 0x0009BA0C
		// (remove) Token: 0x06002462 RID: 9314 RVA: 0x0009D844 File Offset: 0x0009BA44
		public event Action<PurchaseEvent> PurchaseSuccess;

		// Token: 0x1400029B RID: 667
		// (add) Token: 0x06002463 RID: 9315 RVA: 0x0009D87C File Offset: 0x0009BA7C
		// (remove) Token: 0x06002464 RID: 9316 RVA: 0x0009D8B4 File Offset: 0x0009BAB4
		public event Action<PurchaseEvent> PurchaseFail;

		// Token: 0x06002465 RID: 9317 RVA: 0x0009D8E9 File Offset: 0x0009BAE9
		public int EnabledForPlatform(PlatformType type)
		{
			if (type != PlatformType.Steam)
			{
				return -1;
			}
			return 10;
		}

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06002466 RID: 9318 RVA: 0x0002C8BE File Offset: 0x0002AABE
		public CurrencyType CurrencyType
		{
			get
			{
				return CurrencyType.Real;
			}
		}

		// Token: 0x06002467 RID: 9319 RVA: 0x0009D8F3 File Offset: 0x0009BAF3
		public IObservable<Unit> ProcessCatalog(List<CatalogStoreItem> catalog)
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				CultureInfo provider = CultureInfo.CreateSpecificCulture("en-US");
				foreach (CatalogStoreItem catalogStoreItem in from i in catalog
				where i.StoreItem.VirtualCurrencyPrices.ContainsKey("RM")
				select i)
				{
					decimal num = decimal.Divide(catalogStoreItem.StoreItem.VirtualCurrencyPrices["RM"], 100m);
					string priceString = num.ToString("C", provider);
					string displayName = catalogStoreItem.CatalogItem.DisplayName;
					string description = catalogStoreItem.CatalogItem.Description;
					string currencyCode = "USD";
					decimal localizedPrice = num;
					catalogStoreItem.Metadata = new ProductMetadata(priceString, displayName, description, currencyCode, localizedPrice);
				}
				observer.OnNext(Unit.Default);
				observer.OnCompleted();
				return Disposable.Empty;
			});
		}

		// Token: 0x06002468 RID: 9320 RVA: 0x0009D911 File Offset: 0x0009BB11
		public IObservable<Unit> OnLogin(IPlatformStore platformStore, LoginResult result)
		{
			this.logger = Logger.GetLogger(this);
			this.playFab = platformStore.PlayFab;
			return Observable.ReturnUnit();
		}

		// Token: 0x06002469 RID: 9321 RVA: 0x00002718 File Offset: 0x00000918
		public void Dispose()
		{
		}

		// Token: 0x0600246A RID: 9322 RVA: 0x0009D930 File Offset: 0x0009BB30
		public void Purchase(CatalogStoreItem item, string currencyCode)
		{
			Action<PurchaseItemResult> onSuccess = delegate(PurchaseItemResult result)
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
			this.playFab.StartPurchase(item.ItemId, item.CatalogItem.CatalogVersion, item.StoreId, delegate(StartPurchaseResult res)
			{
				this.OnStartPurchase(res, onSuccess, onError);
			}, onError);
		}

		// Token: 0x0600246B RID: 9323 RVA: 0x0009D9C0 File Offset: 0x0009BBC0
		private void OnStartPurchase(StartPurchaseResult order, Action<PurchaseItemResult> onSuccess, Action<PlayFabError> onError)
		{
			Action<ConfirmPurchaseResult> <>9__2;
			this.steam.MicroTransactionMonitor(delegate(string orderId)
			{
				this.logger.Debug("Steam returned successful response for order [{0}]", new object[]
				{
					orderId
				});
				PlayFabWrapper playFabWrapper = this.playFab;
				Action<ConfirmPurchaseResult> onResult;
				if ((onResult = <>9__2) == null)
				{
					onResult = (<>9__2 = delegate(ConfirmPurchaseResult res)
					{
						this.OnConfirmPurchase(res, onSuccess);
					});
				}
				playFabWrapper.ConfirmPurchase(orderId, onResult, onError);
			}, delegate(EResult error)
			{
				this.logger.Error("Steam returned error for order [{0}]: [{1}]", new object[]
				{
					order.OrderId,
					error
				});
				onError(new PlayFabError
				{
					Error = PlayFabErrorCode.NotAuthorized
				});
			});
			this.logger.Debug("Paying for order [{0}]", new object[]
			{
				order.OrderId
			});
			this.playFab.PayForPurchase(order.OrderId, "Steam", "RM", new Action<PayForPurchaseResult>(this.OnPayForPurchase), onError, null);
		}

		// Token: 0x0600246C RID: 9324 RVA: 0x0009DA69 File Offset: 0x0009BC69
		private void OnPayForPurchase(PayForPurchaseResult res)
		{
			this.logger.Debug("Waiting for Steam to confirm order [{0}]", new object[]
			{
				res.OrderId
			});
		}

		// Token: 0x0600246D RID: 9325 RVA: 0x0009DA8A File Offset: 0x0009BC8A
		private void OnConfirmPurchase(ConfirmPurchaseResult order, Action<PurchaseItemResult> onSuccess)
		{
			this.logger.Info("PlayFab confirmed purchase for order [{0}]", new object[]
			{
				order.OrderId
			});
			onSuccess(new PurchaseItemResult
			{
				Items = order.Items
			});
		}

		// Token: 0x04002515 RID: 9493
		private Logger.Logger logger;

		// Token: 0x04002516 RID: 9494
		private PlayFabWrapper playFab;

		// Token: 0x04002517 RID: 9495
		private SteamworksWrapper steam = SteamworksWrapper.Instance;
	}
}
