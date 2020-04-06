using System;
using UnityEngine;

namespace HyperHippo
{
	// Token: 0x020002D4 RID: 724
	public class Util
	{
		// Token: 0x060013BC RID: 5052 RVA: 0x0005A804 File Offset: 0x00058A04
		public static Transform FindChildInChildren(Transform parent, string name)
		{
			if (!parent || string.IsNullOrEmpty(name))
			{
				return null;
			}
			foreach (Transform transform in parent.GetComponentsInChildren<Transform>())
			{
				if (transform.name == name)
				{
					return transform;
				}
			}
			return null;
		}

		// Token: 0x060013BD RID: 5053 RVA: 0x0005A84D File Offset: 0x00058A4D
		public static double Clamp(double value, double min, double max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}
			return value;
		}

		// Token: 0x060013BE RID: 5054 RVA: 0x0005A85C File Offset: 0x00058A5C
		public static double UnixTime()
		{
			return Util.UnixTime(DateTime.UtcNow);
		}

		// Token: 0x060013BF RID: 5055 RVA: 0x0005A868 File Offset: 0x00058A68
		public static double UnixTime(DateTime date)
		{
			DateTime d = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			return (date - d).TotalSeconds;
		}
	}
}
