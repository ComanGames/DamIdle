using System;
using AdCap.Store;

// Token: 0x020000F6 RID: 246
public struct InsufficientGoldEvent
{
	// Token: 0x0600066B RID: 1643 RVA: 0x00022929 File Offset: 0x00020B29
	public InsufficientGoldEvent(Currency currency, int outstandingGoldAmount)
	{
		this.Currency = currency;
		this.OutstandingGoldAmount = outstandingGoldAmount;
	}

	// Token: 0x040005E9 RID: 1513
	public Currency Currency;

	// Token: 0x040005EA RID: 1514
	public int OutstandingGoldAmount;
}
