using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000237 RID: 567
public static class AdCapHardResetRewards
{
	// Token: 0x0600101C RID: 4124 RVA: 0x00049A58 File Offset: 0x00047C58
	public static void LoadHardResetData(List<AdCapHardResetReward> hardResetPlanetRewards)
	{
		if (AdCapHardResetRewards.initialized)
		{
			return;
		}
		AdCapHardResetRewards.initialized = true;
		foreach (AdCapHardResetReward adCapHardResetReward in hardResetPlanetRewards)
		{
			AdCapHardResetRewards.HardResetPlanetRewards.Add(adCapHardResetReward.PlanetName, adCapHardResetReward.GoldReward);
		}
	}

	// Token: 0x0600101D RID: 4125 RVA: 0x00049AC4 File Offset: 0x00047CC4
	public static int GetRewardByKey(string key)
	{
		int result;
		if (AdCapHardResetRewards.HardResetPlanetRewards.TryGetValue(key, out result))
		{
			return result;
		}
		Debug.LogError("(" + key + ")");
		return 0;
	}

	// Token: 0x04000DB6 RID: 3510
	public static Dictionary<string, int> HardResetPlanetRewards = new Dictionary<string, int>();

	// Token: 0x04000DB7 RID: 3511
	private static bool initialized;
}
