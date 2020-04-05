using System;

// Token: 0x0200014C RID: 332
public struct OfflineEarnings
{
	// Token: 0x060009E5 RID: 2533 RVA: 0x0002CA95 File Offset: 0x0002AC95
	public OfflineEarnings(double elapsed, double cashEarned)
	{
		this.Elapsed = elapsed;
		this.CashEarned = cashEarned;
	}

	// Token: 0x04000853 RID: 2131
	public double Elapsed;

	// Token: 0x04000854 RID: 2132
	public double CashEarned;
}
