using System;

namespace AdCap.Store
{
	// Token: 0x020006F8 RID: 1784
	[Serializable]
	public struct ItemPurchaseSaveData
	{
		// Token: 0x040025C4 RID: 9668
		public string ItemId;

		// Token: 0x040025C5 RID: 9669
		public int TotalPurchaseCount;

		// Token: 0x040025C6 RID: 9670
		public int CurrentPurchaseCount;

		// Token: 0x040025C7 RID: 9671
		public string ExpiryDate;

		// Token: 0x040025C8 RID: 9672
		public bool HasExpired;

		// Token: 0x040025C9 RID: 9673
		public bool IsLastChance;
	}
}
