using System;

// Token: 0x02000161 RID: 353
[Serializable]
public class VentureSaveData
{
	// Token: 0x06000B51 RID: 2897 RVA: 0x00033793 File Offset: 0x00031993
	public VentureSaveData()
	{
	}

	// Token: 0x06000B52 RID: 2898 RVA: 0x000337A2 File Offset: 0x000319A2
	public VentureSaveData(string id, double numOwned, float timeRun, bool isBoosted, int gildLevel = 0, VentureModel.EUnlockState unlockState = VentureModel.EUnlockState.Unlocked)
	{
		this.id = id;
		this.numOwned = numOwned;
		this.timeRun = timeRun;
		this.isBoosted = isBoosted;
		this.gildLevel = gildLevel;
		this.unlockState = unlockState;
	}

	// Token: 0x04000991 RID: 2449
	public string id;

	// Token: 0x04000992 RID: 2450
	public double numOwned;

	// Token: 0x04000993 RID: 2451
	public float timeRun;

	// Token: 0x04000994 RID: 2452
	public bool isBoosted;

	// Token: 0x04000995 RID: 2453
	public int gildLevel;

	// Token: 0x04000996 RID: 2454
	public VentureModel.EUnlockState unlockState = VentureModel.EUnlockState.Unlocked;
}
