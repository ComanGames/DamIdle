using System;

namespace AdCap.Store
{
	// Token: 0x020006F5 RID: 1781
	public class InsufficientFundsEvent
	{
		// Token: 0x060024F7 RID: 9463 RVA: 0x000A0752 File Offset: 0x0009E952
		public InsufficientFundsEvent(Currency currency, int outstandingCurrencyAmount)
		{
			this.Currency = currency;
			this.OutstandingCurrencyAmount = outstandingCurrencyAmount;
		}

		// Token: 0x040025BB RID: 9659
		public Currency Currency;

		// Token: 0x040025BC RID: 9660
		public int OutstandingCurrencyAmount;
	}
}
