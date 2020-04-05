using System;
using UniRx;

namespace AdCap.Store
{
	// Token: 0x020006F6 RID: 1782
	public class ItemPurchaseData
	{
		// Token: 0x060024F8 RID: 9464 RVA: 0x000A0768 File Offset: 0x0009E968
		public ItemPurchaseData(string id)
		{
			this.ItemId = id;
		}

		// Token: 0x060024F9 RID: 9465 RVA: 0x000A0790 File Offset: 0x0009E990
		public ItemPurchaseSaveData ToSaveData()
		{
			return new ItemPurchaseSaveData
			{
				ItemId = this.ItemId,
				TotalPurchaseCount = this.TotalPurchaseCount,
				CurrentPurchaseCount = this.CurrentPurchaseCount.Value,
				ExpiryDate = this.ExpiryDate.ToString(),
				IsLastChance = this.IsLastChance
			};
		}

		// Token: 0x060024FA RID: 9466 RVA: 0x000A07F4 File Offset: 0x0009E9F4
		public ItemPurchaseData(ItemPurchaseSaveData saveData)
		{
			this.ItemId = saveData.ItemId;
			this.TotalPurchaseCount = saveData.TotalPurchaseCount;
			this.CurrentPurchaseCount.Value = saveData.CurrentPurchaseCount;
			this.ExpiryDate = DateTime.Parse(saveData.ExpiryDate);
			this.HasExpired.Value = saveData.HasExpired;
			this.IsLastChance = saveData.IsLastChance;
		}

		// Token: 0x040025BD RID: 9661
		public string ItemId;

		// Token: 0x040025BE RID: 9662
		public int TotalPurchaseCount;

		// Token: 0x040025BF RID: 9663
		public readonly ReactiveProperty<int> CurrentPurchaseCount = new ReactiveProperty<int>(0);

		// Token: 0x040025C0 RID: 9664
		public DateTime ExpiryDate;

		// Token: 0x040025C1 RID: 9665
		public readonly ReactiveProperty<bool> HasExpired = new ReactiveProperty<bool>();

		// Token: 0x040025C2 RID: 9666
		public bool IsLastChance;
	}
}
