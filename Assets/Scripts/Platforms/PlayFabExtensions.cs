using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

namespace Platforms
{
	// Token: 0x020006CD RID: 1741
	public static class PlayFabExtensions
	{
		// Token: 0x060023AD RID: 9133 RVA: 0x0009AF3C File Offset: 0x0009913C
		public static Dictionary<string, object> CustomDataTable(this CatalogItem item)
		{
			Dictionary<string, object> dictionary;
			if (!PlayFabExtensions.customDataTableCache.TryGetValue(item.ItemId, out dictionary))
			{
				dictionary = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer, "").DeserializeObject<Dictionary<string, object>>(item.CustomData);
				PlayFabExtensions.customDataTableCache.Add(item.ItemId, dictionary);
			}
			return dictionary;
		}

		// Token: 0x060023AE RID: 9134 RVA: 0x0009AF86 File Offset: 0x00099186
		public static PlayFabWrapper.Exception ToException(this PlayFabError err)
		{
			return new PlayFabWrapper.Exception(err);
		}

		// Token: 0x040024CA RID: 9418
		private static Dictionary<string, Dictionary<string, object>> customDataTableCache = new Dictionary<string, Dictionary<string, object>>();
	}
}
