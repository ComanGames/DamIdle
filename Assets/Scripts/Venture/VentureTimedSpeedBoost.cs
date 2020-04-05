using System;

// Token: 0x02000162 RID: 354
[Serializable]
public class VentureTimedSpeedBoost
{
	// Token: 0x06000B53 RID: 2899 RVA: 0x000337DE File Offset: 0x000319DE
	public VentureTimedSpeedBoost(string id, float multiplier, float multiplyerTimeInSeconds, DateTime timeApplied)
	{
		this.id = id;
		this.multiplier = multiplier;
		this.multiplyerTimeInSeconds = multiplyerTimeInSeconds;
		this.timeApplied = timeApplied;
		this.expireTime = this.timeApplied.AddSeconds((double)multiplyerTimeInSeconds);
	}

	// Token: 0x04000997 RID: 2455
	public string id;

	// Token: 0x04000998 RID: 2456
	public float multiplier;

	// Token: 0x04000999 RID: 2457
	public float multiplyerTimeInSeconds;

	// Token: 0x0400099A RID: 2458
	public DateTime timeApplied;

	// Token: 0x0400099B RID: 2459
	public DateTime expireTime;
}
