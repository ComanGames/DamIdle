using System;

namespace AdComm
{
	// Token: 0x02000654 RID: 1620
	public interface IAssetBundleManager
	{
		// Token: 0x06001F58 RID: 8024
		void Initialize(string baseUri, string manifestVersion, Action<bool> onComplete);

		// Token: 0x06001F59 RID: 8025
		IObservable<bool> InitializeAsync(string baseUri, string manifestVersion);

		// Token: 0x06001F5A RID: 8026
		void GetBundle(string bundleName, Action<IAssetBundle> onComplete);

		// Token: 0x06001F5B RID: 8027
		IObservable<IAssetBundle> GetBundleAsync(string bundleName);

		// Token: 0x06001F5C RID: 8028
		void UnloadAssetBundle(string assetbundleName, bool unloadAllLoadedObjects, bool force);
	}
}
