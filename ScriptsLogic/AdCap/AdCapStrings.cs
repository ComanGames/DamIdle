using System;
using System.Collections.Generic;

// Token: 0x02000122 RID: 290
public static class AdCapStrings
{
	// Token: 0x060007A7 RID: 1959 RVA: 0x00028AD0 File Offset: 0x00026CD0
	public static void LoadStringData(List<AdCapString> stringDataList)
	{
		if (AdCapStrings.initialized)
		{
			return;
		}
		AdCapStrings.initialized = true;
		foreach (AdCapString adCapString in stringDataList)
		{
			AdCapStrings.StringMap.Add(adCapString.Key, adCapString.Value);
		}
	}

	// Token: 0x060007A8 RID: 1960 RVA: 0x00028B3C File Offset: 0x00026D3C
	public static string GetStringByKey(string key)
	{
		string result;
		if (AdCapStrings.StringMap.TryGetValue(key, out result))
		{
			return result;
		}
		return "(" + key + ")";
	}

	// Token: 0x0400072D RID: 1837
	public static Dictionary<string, string> StringMap = new Dictionary<string, string>();

	// Token: 0x0400072E RID: 1838
	private static bool initialized = false;
}
