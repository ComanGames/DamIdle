using System;

// Token: 0x02000155 RID: 341
public class UnlockTargetInfo
{
	// Token: 0x06000AC3 RID: 2755 RVA: 0x00031073 File Offset: 0x0002F273
	public UnlockTargetInfo(string ventureName, int previousTarget, int nextTarget)
	{
		this.ventureName = ventureName;
		this.previousTarget = previousTarget;
		this.nextTarget = nextTarget;
	}

	// Token: 0x04000917 RID: 2327
	public string ventureName;

	// Token: 0x04000918 RID: 2328
	public int previousTarget;

	// Token: 0x04000919 RID: 2329
	public int nextTarget;
}
