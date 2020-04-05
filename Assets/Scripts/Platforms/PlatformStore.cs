using System;
using System.Collections.Generic;
using System.Linq;
using Platforms.Logger;
using PlayFab;
using PlayFab.ClientModels;
using UniRx;

namespace Platforms
{
	// Token: 0x020006CC RID: 1740
	public class PlatformStore : IPlatformStore
	{
		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x0600237E RID: 9086 RVA: 0x0009A52C File Offset: 0x0009872C
		// (set) Token: 0x0600237F RID: 9087 RVA: 0x0009A534 File Offset: 0x00098734
		public IPlatformStoreExtension RealMoney { get; private set; }

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x06002380 RID: 9088 RVA: 0x0009A53D File Offset: 0x0009873D
		// (set) Token: 0x06002381 RID: 9089 RVA: 0x0009A545 File Offset: 0x00098745
		public IPlatformStoreExtension VirtualCurrency { get; private set; }

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x06002382 RID: 9090 RVA: 0x0009A54E File Offset: 0x0009874E
		// (set) Token: 0x06002383 RID: 9091 RVA: 0x0009A556 File Offset: 0x00098756
		public PlayFabWrapper PlayFab { get; private set; }

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06002384 RID: 9092 RVA: 0x0009A55F File Offset: 0x0009875F
		// (set) Token: 0x06002385 RID: 9093 RVA: 0x0009A567 File Offset: 0x00098767
		public PlatformAccount Account { get; private set; }

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06002386 RID: 9094 RVA: 0x0009A570 File Offset: 0x00098770
		// (set) Token: 0x06002387 RID: 9095 RVA: 0x0009A578 File Offset: 0x00098778
		public List<CatalogItem> Catalog { get; protected set; }

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06002388 RID: 9096 RVA: 0x0009A581 File Offset: 0x00098781
		// (set) Token: 0x06002389 RID: 9097 RVA: 0x0009A589 File Offset: 0x00098789
		public GetStoreItemsResult[] Stores { get; protected set; }

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x0600238A RID: 9098 RVA: 0x0009A592 File Offset: 0x00098792
		// (set) Token: 0x0600238B RID: 9099 RVA: 0x0009A59A File Offset: 0x0009879A
		public List<CatalogStoreItem> CatalogStore { get; private set; }

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x0600238C RID: 9100 RVA: 0x0009A5A3 File Offset: 0x000987A3
		// (set) Token: 0x0600238D RID: 9101 RVA: 0x0009A5AB File Offset: 0x000987AB
		public Dictionary<string, CatalogStoreItem> StoreMap { get; private set; }

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x0600238E RID: 9102 RVA: 0x0009A5B4 File Offset: 0x000987B4
		// (set) Token: 0x0600238F RID: 9103 RVA: 0x0009A5BC File Offset: 0x000987BC
		public Dictionary<string, CatalogItem> CatalogMap { get; private set; }

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x06002390 RID: 9104 RVA: 0x0009A5C8 File Offset: 0x000987C8
		public IObservable<List<CatalogStoreItem>> Ready
		{
			get
			{
				return from _ in (from v in this.ready
				where v
				select v).Take(1)
				select this.CatalogStore;
			}
		}

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06002391 RID: 9105 RVA: 0x0009A616 File Offset: 0x00098816
		public IObservable<PurchaseEvent> OnPurchaseStart
		{
			get
			{
				return this.onPurchaseStart;
			}
		}

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x06002392 RID: 9106 RVA: 0x0009A61E File Offset: 0x0009881E
		public IObservable<PurchaseEvent> OnPurchaseSuccess
		{
			get
			{
				return this.onPurchaseSuccess;
			}
		}

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06002393 RID: 9107 RVA: 0x0009A626 File Offset: 0x00098826
		public IObservable<PurchaseEvent> OnPurchaseFail
		{
			get
			{
				return this.onPurchaseFail;
			}
		}

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06002394 RID: 9108 RVA: 0x0009A62E File Offset: 0x0009882E
		public IObservable<PurchaseEvent> OnPurchaseSuccessHistory
		{
			get
			{
				return this.onPurchaseSuccessHistory;
			}
		}

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06002395 RID: 9109 RVA: 0x0009A636 File Offset: 0x00098836
		public IObservable<Exception> OnInitializationError
		{
			get
			{
				return this.onInitializationError;
			}
		}

		// Token: 0x06002396 RID: 9110 RVA: 0x0009A640 File Offset: 0x00098840
		public PlatformStore()
		{
			this.CatalogStore = new List<CatalogStoreItem>();
			this.StoreMap = new Dictionary<string, CatalogStoreItem>();
			this.CatalogMap = new Dictionary<string, CatalogItem>();
		}

		// Token: 0x06002397 RID: 9111 RVA: 0x0009A6C4 File Offset: 0x000988C4
		public virtual IPlatformStore Init(PlayFabWrapper playFab, PlatformAccount account)
		{
			this.logger = Logger.GetLogger(this);
			this.logger.Info("Initializing [{0}]", new object[]
			{
				base.GetType().Name
			});
			this.PlayFab = playFab;
			this.Account = account;
			this.onPurchaseSuccess.Subscribe(new Action<PurchaseEvent>(this.onPurchaseSuccessHistory.OnNext));
			this.onInitializationError.Subscribe(delegate(Exception err)
			{
				this.logger.Error("Error initializing PlatformStore: [{0}]", new object[]
				{
					err.Message
				});
			});
			account.OnLogin.Subscribe(new Action<LoginResult>(this.OnLogin)).AddTo(this.disposables);
			this.GetExtensions();
			this.logger.Info("Initialized");
			return this;
		}

		// Token: 0x06002398 RID: 9112 RVA: 0x0009A77F File Offset: 0x0009897F
		public void Dispose()
		{
			this.disposables.Dispose();
		}

		// Token: 0x06002399 RID: 9113 RVA: 0x0009A78C File Offset: 0x0009898C
		public virtual void Purchase(string itemId, string currencyCode, Action<PurchaseEvent> onSuccess, Action<PurchaseEvent> onError)
		{
			if (!this.StoreMap.ContainsKey(itemId))
			{
				if (onError != null)
				{
					onError(new PurchaseEvent(null, currencyCode, new PlayFabError
					{
						Error = PlayFabErrorCode.ItemNotFound,
						ErrorMessage = string.Format("Unable to find item [{0}]", itemId)
					}));
				}
				return;
			}
			this.Purchase(this.StoreMap[itemId], currencyCode, onSuccess, onError);
		}

		// Token: 0x0600239A RID: 9114 RVA: 0x0009A7F4 File Offset: 0x000989F4
		public virtual void Purchase(CatalogStoreItem item, string currencyCode, Action<PurchaseEvent> onSuccess, Action<PurchaseEvent> onError)
		{
			this.logger.Debug("Attempting to purchase CatalogItem [{0}] with currency [{1}] in store [{2}]", new object[]
			{
				item.ItemId,
				currencyCode,
				item.StoreId
			});
			(from evt in this.OnPurchaseSuccess
			where evt.Item == item
			select evt).Amb(from evt in this.onPurchaseFail
			where evt.Item == item
			select evt).Take(1).Subscribe(delegate(PurchaseEvent evt)
			{
				if (evt.Error == null)
				{
					if (onSuccess != null)
					{
						onSuccess(evt);
					}
					item.Purchased.Value = true;
					return;
				}
				if (onError != null)
				{
					onError(evt);
				}
			});
			this.onPurchaseStart.OnNext(new PurchaseEvent(item, currencyCode));
			((currencyCode == "RM") ? this.RealMoney : this.VirtualCurrency).Purchase(item, currencyCode);
		}

		// Token: 0x0600239B RID: 9115 RVA: 0x0009A8DC File Offset: 0x00098ADC
		public void RefreshStore(Action onComplete)
		{
			if (this.Catalog == null)
			{
				this.logger.Error("Unable to refresh store, catalog not initialized.");
				return;
			}
			this.GetStoreStream().Subscribe(delegate(Unit _)
			{
				this.ProcessCatalogAndStore();
				onComplete();
			});
		}

		// Token: 0x0600239C RID: 9116 RVA: 0x0009A930 File Offset: 0x00098B30
		private void OnLogin(LoginResult result)
		{
			this.RealMoney.OnLogin(this, result).Concat(new IObservable<Unit>[]
			{
				this.VirtualCurrency.OnLogin(this, result),
				this.GetCatalogAndStoreStream(),
				Observable.WhenAll(new IObservable<Unit>[]
				{
					this.RealMoney.ProcessCatalog(this.CatalogStore).DoOnCompleted(delegate
					{
						this.AvailableCurrencies |= CurrencyType.Real;
					}).CatchIgnore(new Action<Exception>(this.onInitializationError.OnNext)),
					this.VirtualCurrency.ProcessCatalog(this.CatalogStore).DoOnCompleted(delegate
					{
						this.AvailableCurrencies |= CurrencyType.Virtual;
					})
				})
			}).Last<Unit>().Subscribe(delegate(Unit _)
			{
				this.ready.OnNext(true);
			}, new Action<Exception>(this.onInitializationError.OnNext));
		}

		// Token: 0x0600239D RID: 9117 RVA: 0x0009AA06 File Offset: 0x00098C06
		private IObservable<Unit> GetCatalogAndStoreStream()
		{
			this.logger.Debug("GetCatalogAndStoreStream...");
			if (this.Catalog != null)
			{
				return Observable.ReturnUnit();
			}
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				Observable.WhenAll(new IObservable<Unit>[]
				{
					this.GetCatalogStream(),
					this.GetStoreStream()
				}).Subscribe(delegate(Unit _)
				{
					this.logger.Debug("Catalog and store received, processing...");
					this.ProcessCatalogAndStore();
					observer.OnNext(Unit.Default);
					observer.OnCompleted();
				});
				return Disposable.Empty;
			});
		}

		// Token: 0x0600239E RID: 9118 RVA: 0x0009AA37 File Offset: 0x00098C37
		private IObservable<Unit> GetCatalogStream()
		{
			if (this.Catalog != null)
			{
				return Observable.ReturnUnit();
			}
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				if (this.Account.TitleDataConfig == null || this.Account.TitleDataConfig.StoreConfig == null || string.IsNullOrEmpty(this.Account.TitleDataConfig.StoreConfig.CatalogVersion))
				{
					observer.OnError(new NullReferenceException("Unable to find store configuration in title data, please verify you have your store config set up correctly in PlayFab."));
					return Disposable.Empty;
				}
				this.logger.Debug("Getting catalog [{0}]...", new object[]
				{
					this.Account.TitleDataConfig.StoreConfig.CatalogVersion
				});
				this.PlayFab.GetCatalogItems(this.Account.TitleDataConfig.StoreConfig.CatalogVersion, delegate(GetCatalogItemsResult result)
				{
					this.logger.Trace("Received catalog [{0}].", new object[]
					{
						this.Account.TitleDataConfig.StoreConfig.CatalogVersion
					});
					this.Catalog = result.Catalog;
					this.CatalogMap = this.Catalog.ToDictionary((CatalogItem v) => v.ItemId, (CatalogItem v) => v);
					observer.OnCompleted();
				}, delegate(PlayFabError err)
				{
					observer.OnError(err.ToException());
				});
				return Disposable.Empty;
			});
		}

		// Token: 0x0600239F RID: 9119 RVA: 0x0009AA58 File Offset: 0x00098C58
		private IObservable<Unit> GetStoreStream()
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> parentObserver)
			{
				if (this.Account.TitleDataConfig == null || this.Account.TitleDataConfig.StoreConfig == null || this.Account.TitleDataConfig.StoreConfig.Store == null)
				{
					parentObserver.OnError(new NullReferenceException("Unable to find store configuration in title data, please verify you have your store config set up correctly in PlayFab."));
					return Disposable.Empty;
				}
				PlatformAccount.StoreDataConfigModel storeConfig = this.Account.TitleDataConfig.StoreConfig;
				string[] array = storeConfig.Store[Helper.GetPlatformType().ToString()];
				IObservable<GetStoreItemsResult>[] array2 = new IObservable<GetStoreItemsResult>[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					string storeId = array[i];
					array2[i] = Observable.Create<GetStoreItemsResult>(delegate(IObserver<GetStoreItemsResult> childObserver)
					{
						this.logger.Info("Getting store [{0}] for catalog [{1}]...", new object[]
						{
							storeId,
							storeConfig.CatalogVersion
						});
						this.PlayFab.GetStoreItems(storeId, storeConfig.CatalogVersion, delegate(GetStoreItemsResult res)
						{
							this.logger.Trace("Received store [{0}] for catalog [{1}].", new object[]
							{
								storeId,
								storeConfig.CatalogVersion
							});
							childObserver.OnNext(res);
							childObserver.OnCompleted();
						}, delegate(PlayFabError err)
						{
							childObserver.OnError(err.ToException());
						});
						return Disposable.Empty;
					});
				}
				Observable.WhenAll<GetStoreItemsResult>(array2).Subscribe(delegate(GetStoreItemsResult[] stores)
				{
					this.Stores = stores;
					parentObserver.OnNext(Unit.Default);
					parentObserver.OnCompleted();
				});
				return Disposable.Empty;
			});
		}

		// Token: 0x060023A0 RID: 9120 RVA: 0x0009AA6C File Offset: 0x00098C6C
		private void ProcessCatalogAndStore()
		{
			this.logger.Debug("GetCatalogAndStoreStream...");
			this.CatalogStore.Clear();
			this.CatalogStore.AddRange(this.CreateCatalogStoreItem(this.Catalog, this.Stores));
			this.StoreMap.Clear();
			for (int i = 0; i < this.CatalogStore.Count; i++)
			{
				this.StoreMap.Add(this.CatalogStore[i].ItemId, this.CatalogStore[i]);
			}
		}

		// Token: 0x060023A1 RID: 9121 RVA: 0x0009AAFC File Offset: 0x00098CFC
		private List<CatalogStoreItem> CreateCatalogStoreItem(List<CatalogItem> catalog, GetStoreItemsResult[] stores)
		{
			return (from store in stores
			from si in store.Store
			let ci = catalog.Find((CatalogItem v) => v.ItemId == si.ItemId)
			select new CatalogStoreItem
			{
				ItemId = ci.ItemId,
				StoreId = store.StoreId,
				CatalogItem = ci,
				StoreItem = si
			}).ToList<CatalogStoreItem>();
		}

		// Token: 0x060023A2 RID: 9122 RVA: 0x0009AB94 File Offset: 0x00098D94
		private void GetExtensions()
		{
			foreach (Helper.PlatformContainer<IPlatformStoreExtension> platformContainer in Helper.GetAvailableInstances<IPlatformStoreExtension>(Helper.GetPlatformType()))
			{
				if (platformContainer.Platform.CurrencyType == CurrencyType.Real)
				{
					if (this.RealMoney == null)
					{
						this.RealMoney = platformContainer.Platform;
					}
				}
				else if (this.VirtualCurrency == null)
				{
					this.VirtualCurrency = platformContainer.Platform;
				}
			}
			this.RealMoney.PurchaseSuccess += this.OnExtensionPurchaseSuccess;
			this.VirtualCurrency.PurchaseSuccess += this.OnExtensionPurchaseSuccess;
			this.RealMoney.PurchaseFail += this.OnExtensionPurchaseFail;
			this.VirtualCurrency.PurchaseFail += this.OnExtensionPurchaseFail;
		}

		// Token: 0x060023A3 RID: 9123 RVA: 0x0009AC78 File Offset: 0x00098E78
		private void OnExtensionPurchaseSuccess(PurchaseEvent evt)
		{
			this.onPurchaseSuccess.OnNext(evt);
		}

		// Token: 0x060023A4 RID: 9124 RVA: 0x0009AC86 File Offset: 0x00098E86
		private void OnExtensionPurchaseFail(PurchaseEvent evt)
		{
			this.onPurchaseFail.OnNext(evt);
		}

		// Token: 0x040024BA RID: 9402
		public CurrencyType AvailableCurrencies;

		// Token: 0x040024C2 RID: 9410
		private Logger logger;

		// Token: 0x040024C3 RID: 9411
		private CompositeDisposable disposables = new CompositeDisposable();

		// Token: 0x040024C4 RID: 9412
		private ReplaySubject<bool> ready = new ReplaySubject<bool>();

		// Token: 0x040024C5 RID: 9413
		private Subject<PurchaseEvent> onPurchaseStart = new Subject<PurchaseEvent>();

		// Token: 0x040024C6 RID: 9414
		private Subject<PurchaseEvent> onPurchaseSuccess = new Subject<PurchaseEvent>();

		// Token: 0x040024C7 RID: 9415
		private Subject<PurchaseEvent> onPurchaseFail = new Subject<PurchaseEvent>();

		// Token: 0x040024C8 RID: 9416
		private ReplaySubject<PurchaseEvent> onPurchaseSuccessHistory = new ReplaySubject<PurchaseEvent>();

		// Token: 0x040024C9 RID: 9417
		private ReplaySubject<Exception> onInitializationError = new ReplaySubject<Exception>(1);
	}
}
