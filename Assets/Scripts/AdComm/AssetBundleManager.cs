using System;
using AssetBundles;
using UniRx;
using UnityEngine;


namespace AdComm
{
	// Token: 0x02000655 RID: 1621
	public class AssetBundleManager : IAssetBundleManager
	{
		// Token: 0x06001F5D RID: 8029 RVA: 0x0008B3A1 File Offset: 0x000895A1
		public AssetBundleManager(bool useAppendedHash = true)
		{
			this.abm = new AssetBundles.AssetBundleManager();
			this.abm.AppendHashToBundleNames(useAppendedHash);
		}

		// Token: 0x06001F5E RID: 8030 RVA: 0x0008B3C1 File Offset: 0x000895C1
		public void Initialize(string baseUri, string manifestVersion, Action<bool> onComplete)
		{
			this.ConfigureAbm(baseUri, manifestVersion);
			this.abm.Initialize(onComplete);
		}

		// Token: 0x06001F5F RID: 8031 RVA: 0x0008B3D8 File Offset: 0x000895D8
		public IObservable<bool> InitializeAsync(string baseUri, string manifestVersion)
		{
			this.ConfigureAbm(baseUri, manifestVersion);
			AssetBundleManifestAsync v = this.abm.InitializeAsync();
			return Observable.Create<bool>(delegate(IObserver<bool> observer)
			{
				v.ToObservable(false).Subscribe(delegate(Unit b)
				{
					if (v.Success)
					{
						observer.OnNext(v.Success);
						observer.OnCompleted();
						return;
					}
					observer.OnError(new Exception("Error occured loading " + baseUri + " : " + manifestVersion));
				});
				return Disposable.Empty;
			});
		}

		// Token: 0x06001F60 RID: 8032 RVA: 0x0008B42D File Offset: 0x0008962D
		private void ConfigureAbm(string baseUri, string manifestVersion)
		{
			if (!this.abm.Initialized)
			{
				this.abm.SetBundleConfig(baseUri, manifestVersion);
			}
		}

		// Token: 0x06001F61 RID: 8033 RVA: 0x0008B44C File Offset: 0x0008964C
		public void GetBundle(string bundleName, Action<IAssetBundle> onComplete)
		{
			this.abm.GetBundle(bundleName, delegate(AssetBundle bundle)
			{
				onComplete(new HHAssetBundle(bundle));
			});
		}

		// Token: 0x06001F62 RID: 8034 RVA: 0x0008B480 File Offset: 0x00089680
		public IObservable<IAssetBundle> GetBundleAsync(string bundleName)
		{
			AssetBundleAsync v = this.abm.GetBundleAsync(bundleName);
			return Observable.Create<IAssetBundle>(delegate(IObserver<IAssetBundle> observer)
			{
				v.ToObservable(false).Subscribe(delegate(Unit b)
				{
					if (!v.Failed)
					{
						observer.OnNext(new HHAssetBundle(v.AssetBundle));
						observer.OnCompleted();
						return;
					}
					observer.OnError(new Exception("Failed to load asset bundle " + bundleName));
				});
				return Disposable.Empty;
			});
		}

		// Token: 0x06001F63 RID: 8035 RVA: 0x0008B4C2 File Offset: 0x000896C2
		public void UnloadBundle(IAssetBundle bundle, bool unloadAllLoadedObjects)
		{
			this.abm.UnloadBundle(((HHAssetBundle)bundle).GetBundle(), unloadAllLoadedObjects);
		}

		// Token: 0x06001F64 RID: 8036 RVA: 0x0008B4DB File Offset: 0x000896DB
		public void UnloadAssetBundle(string assetbundleName, bool unloadAllLoadedObjects, bool force)
		{
			this.abm.UnloadBundle(assetbundleName, unloadAllLoadedObjects, force);
		}

		// Token: 0x040022B9 RID: 8889
		 private AssetBundles.AssetBundleManager abm;

		// Token: 0x040022BA RID: 8890
		private bool initialized;
	}
}
