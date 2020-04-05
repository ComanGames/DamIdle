using System;

namespace AdCap.Store
{
	// Token: 0x020006F3 RID: 1779
	public struct StorePurchaseEvent
	{
		// Token: 0x040025B3 RID: 9651
		public AdCapStoreItem Item;

		// Token: 0x040025B4 RID: 9652
		public EStorePurchaseState PurchaseState;

		// Token: 0x040025B5 RID: 9653
		public Currency PurchaseCurrency;

		// Token: 0x040025B6 RID: 9654
		public string Error;
	}
}
