using System;
using System.Collections.Generic;
using System.Linq;
using Platforms.Ad;
using Platforms.Logger;
using UniRx;

namespace Platforms
{
	// Token: 0x020006C9 RID: 1737
	public class PlatformAd : IPlatformAd, IDisposable
	{
		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x0600236E RID: 9070 RVA: 0x00099FAC File Offset: 0x000981AC
		// (set) Token: 0x0600236F RID: 9071 RVA: 0x00099FB4 File Offset: 0x000981B4
		public ReplaySubject<int> OfferWallCreditsReceived { get; private set; }

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x06002370 RID: 9072 RVA: 0x00099FBD File Offset: 0x000981BD
		// (set) Token: 0x06002371 RID: 9073 RVA: 0x00099FC5 File Offset: 0x000981C5
		public Dictionary<AdType, ReactiveProperty<bool>> AdReadyMap { get; private set; }

		// Token: 0x06002372 RID: 9074 RVA: 0x00099FD0 File Offset: 0x000981D0
		public PlatformAd()
		{
			this.OfferWallCreditsReceived = new ReplaySubject<int>();
			this.AdReadyMap = new Dictionary<AdType, ReactiveProperty<bool>>();
			foreach (AdType key in Enum.GetValues(typeof(AdType)).Cast<AdType>())
			{
				this.AdReadyMap.Add(key, new ReactiveProperty<bool>(true));
				this.adTypeDisposables[key] = new CompositeDisposable();
			}
		}

		// Token: 0x06002373 RID: 9075 RVA: 0x0009A084 File Offset: 0x00098284
		public void Init()
		{
			this.logger = Logger.GetLogger(this);
			this.logger.Info("Initilizing");
			this.GetExtensions();
			this.logger.Info("Initilized");
		}

		// Token: 0x06002374 RID: 9076 RVA: 0x0009A0B8 File Offset: 0x000982B8
		public void Dispose()
		{
			this.logger.Info("Disposing");
			using (Dictionary<AdType, List<IPlatformAdExtension>>.ValueCollection.Enumerator enumerator = this.platformAdMap.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					enumerator.Current.ForEach(delegate(IPlatformAdExtension x)
					{
						x.Dispose();
					});
				}
			}
			foreach (CompositeDisposable compositeDisposable in this.adTypeDisposables.Values)
			{
				compositeDisposable.Dispose();
			}
			this.disposables.Dispose();
		}

		// Token: 0x06002375 RID: 9077 RVA: 0x0009A18C File Offset: 0x0009838C
		public void SetAdExtension(AdType type, IPlatformAdExtension extension, bool initializeExtension)
		{
			if (this.platformAdMap.ContainsKey(type))
			{
				this.platformAdMap[type].ForEach(delegate(IPlatformAdExtension x)
				{
					x.Dispose();
				});
				this.platformAdMap[type].Clear();
			}
			if (this.adTypeDisposables.ContainsKey(type))
			{
				this.adTypeDisposables[type].Dispose();
			}
			this.platformAdMap.Add(type, new List<IPlatformAdExtension>
			{
				initializeExtension ? extension.InitPlatform(this) : extension
			});
			extension.AdReady[type].Subscribe(delegate(bool _)
			{
				this.UpdateAdReady(type);
			}).AddTo(this.adTypeDisposables[type]);
		}

		// Token: 0x06002376 RID: 9078 RVA: 0x0009A298 File Offset: 0x00098498
		public IObservable<ShowResult> ShowAd(AdType type, string placementId = "")
		{
			List<IPlatformAdExtension> source;
			if (!this.platformAdMap.TryGetValue(type, out source))
			{
				return Observable.Return<ShowResult>(ShowResult.NoExtensionFound);
			}
			IPlatformAdExtension platformAdExtension = source.FirstOrDefault((IPlatformAdExtension x) => x.AdReady[type].Value);
			if (platformAdExtension == null)
			{
				return Observable.Return<ShowResult>(ShowResult.NoAdsReady);
			}
			return platformAdExtension.ShowAd(type, placementId);
		}

		// Token: 0x06002377 RID: 9079 RVA: 0x0009A2F8 File Offset: 0x000984F8
		private void GetExtensions()
		{
			Helper.GetAvailableInstances<IPlatformAdExtension>(Helper.GetPlatformType()).ForEach(delegate(Helper.PlatformContainer<IPlatformAdExtension> container)
			{
				IPlatformAdExtension platform = container.Platform;
				if (platform.EnabledForPlatform(Helper.GetPlatformType()) != -1)
				{
					foreach (AdType adType in platform.AdTypePriority.Keys)
					{
						if (this.platformAdMap.ContainsKey(adType))
						{
							this.platformAdMap[adType].Add(platform);
						}
						else
						{
							this.platformAdMap.Add(adType, new List<IPlatformAdExtension>
							{
								platform
							});
						}
						AdType _adType = adType;
						platform.AdReady[adType].Subscribe(delegate(bool _)
						{
							this.UpdateAdReady(_adType);
						}).AddTo(this.adTypeDisposables[_adType]);
					}
					platform.InitPlatform(this);
				}
			});
			this.SortExtensionsByPriority();
		}

		// Token: 0x06002378 RID: 9080 RVA: 0x0009A31C File Offset: 0x0009851C
		private void UpdateAdReady(AdType type)
		{
			this.platformAdMap[type].ForEach(delegate(IPlatformAdExtension x)
			{
				this.logger.Info("{0} ads ready = {1}", new object[]
				{
					x.ToString(),
					x.AdReady[type].ToString()
				});
			});
			this.AdReadyMap[type].Value = this.platformAdMap[type].Any((IPlatformAdExtension x) => x.AdReady[type].Value);
		}

		// Token: 0x06002379 RID: 9081 RVA: 0x0009A398 File Offset: 0x00098598
		private void SortExtensionsByPriority()
		{
			Dictionary<AdType, List<IPlatformAdExtension>> dictionary = new Dictionary<AdType, List<IPlatformAdExtension>>();
			using (Dictionary<AdType, List<IPlatformAdExtension>>.Enumerator enumerator = this.platformAdMap.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<AdType, List<IPlatformAdExtension>> keyValuePair = enumerator.Current;
					List<IPlatformAdExtension> value = keyValuePair.Value;
					dictionary[keyValuePair.Key] = (from x in value
					orderby x.AdTypePriority[keyValuePair.Key] descending
					select x).ToList<IPlatformAdExtension>();
				}
			}
			this.platformAdMap = dictionary;
		}

		// Token: 0x040024AF RID: 9391
		private Dictionary<AdType, List<IPlatformAdExtension>> platformAdMap = new Dictionary<AdType, List<IPlatformAdExtension>>();

		// Token: 0x040024B0 RID: 9392
		protected Logger logger;

		// Token: 0x040024B1 RID: 9393
		protected CompositeDisposable disposables = new CompositeDisposable();

		// Token: 0x040024B2 RID: 9394
		protected Dictionary<AdType, CompositeDisposable> adTypeDisposables = new Dictionary<AdType, CompositeDisposable>();
	}
}
