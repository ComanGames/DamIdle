using System;
using Object = UnityEngine.Object;

namespace AdComm
{
	// Token: 0x02000656 RID: 1622
	public interface IAssetBundle
	{
		// Token: 0x06001F65 RID: 8037
		T LoadAsset<T>(string assetName) where T : Object;

		// Token: 0x06001F66 RID: 8038
		T[] LoadAllAssets<T>() where T : Object;

		// Token: 0x06001F67 RID: 8039
		IObservable<T> LoadAssetAsync<T>(string assetName) where T : Object;

		// Token: 0x06001F68 RID: 8040
		void UnloadAssets(bool unloadAll);
	}
}
