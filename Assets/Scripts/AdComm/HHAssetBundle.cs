using System;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdComm
{
	// Token: 0x02000657 RID: 1623
	public class HHAssetBundle : IAssetBundle
	{
		// Token: 0x06001F69 RID: 8041 RVA: 0x0008B4EB File Offset: 0x000896EB
		public HHAssetBundle(AssetBundle bundle)
		{
			this.bundle = bundle;
		}

		// Token: 0x06001F6A RID: 8042 RVA: 0x0008B4FA File Offset: 0x000896FA
		public T LoadAsset<T>(string assetName) where T : Object
		{
			return this.bundle.LoadAsset<T>(assetName);
		}

		// Token: 0x06001F6B RID: 8043 RVA: 0x0008B508 File Offset: 0x00089708
		public T[] LoadAllAssets<T>() where T : Object
		{
			return this.bundle.LoadAllAssets<T>();
		}

		// Token: 0x06001F6C RID: 8044 RVA: 0x0008B515 File Offset: 0x00089715
		public IObservable<T> LoadAssetAsync<T>(string assetName) where T : Object
		{
			return from v in this.bundle.LoadAssetAsync<T>(assetName).AsAsyncOperationObservable(null)
			select (T)((object)v.asset);
		}

		// Token: 0x06001F6D RID: 8045 RVA: 0x0008B54D File Offset: 0x0008974D
		public IObservable<T[]> LoadAllAssetsAsync<T>(string assetName) where T : Object
		{
			return from v in this.bundle.LoadAllAssetsAsync<T>().AsAsyncOperationObservable(null)
			select (T[])v.allAssets;
		}

		// Token: 0x06001F6E RID: 8046 RVA: 0x0008B584 File Offset: 0x00089784
		public AssetBundle GetBundle()
		{
			return this.bundle;
		}

		// Token: 0x06001F6F RID: 8047 RVA: 0x0008B58C File Offset: 0x0008978C
		public void UnloadAssets(bool unloadAllLoadedObjects)
		{
			if (this.bundle != null)
			{
				this.bundle.Unload(unloadAllLoadedObjects);
			}
		}

		// Token: 0x040022BB RID: 8891
		private AssetBundle bundle;
	}
}
