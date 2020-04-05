using System;
using UnityEngine;

// Token: 0x02000066 RID: 102
[Serializable]
public class EventMissionDifficulty
{
	// Token: 0x1700004A RID: 74
	// (get) Token: 0x0600034D RID: 845 RVA: 0x00013400 File Offset: 0x00011600
	public float DifficultyCoefficient
	{
		get
		{
			return Mathf.Floor(Random.Range(this.DifficultyCoefficientMin, this.DifficultyCoefficientMax));
		}
	}

	// Token: 0x040002E5 RID: 741
	public double TaskCompletedStartThreshold;

	// Token: 0x040002E6 RID: 742
	public double TaskCompletedEndThreshold;

	// Token: 0x040002E7 RID: 743
	public float DifficultyCoefficientMin;

	// Token: 0x040002E8 RID: 744
	public float DifficultyCoefficientMax;
}
