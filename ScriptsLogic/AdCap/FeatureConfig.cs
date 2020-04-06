using System;
using System.Collections.Generic;

namespace AdCap
{
	// Token: 0x020006E7 RID: 1767
	public static class FeatureConfig
	{
		// Token: 0x06002497 RID: 9367 RVA: 0x0009E390 File Offset: 0x0009C590
		public static void SetFlag(string flag, bool enabled)
		{
			FeatureConfig.s_flags[flag] = enabled;
		}

		// Token: 0x06002498 RID: 9368 RVA: 0x0009E3A0 File Offset: 0x0009C5A0
		public static bool IsFlagSet(string flag)
		{
			bool flag2;
			return FeatureConfig.s_flags.TryGetValue(flag, out flag2) && flag2;
		}

		// Token: 0x06002499 RID: 9369 RVA: 0x0009E3BC File Offset: 0x0009C5BC
		public static void Initialize(Dictionary<string, bool> featureConfig)
		{
			foreach (KeyValuePair<string, bool> keyValuePair in featureConfig)
			{
				FeatureConfig.s_flags[keyValuePair.Key] = keyValuePair.Value;
			}
		}

		// Token: 0x04002559 RID: 9561
		private static Dictionary<string, bool> s_flags = new Dictionary<string, bool>();
	}
}
