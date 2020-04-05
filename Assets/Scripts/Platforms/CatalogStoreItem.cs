using System;
using PlayFab.ClientModels;
using UniRx;
using UnityEngine.Purchasing;

namespace Platforms
{
	// Token: 0x020006C0 RID: 1728
	public class CatalogStoreItem
	{
		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06002315 RID: 8981 RVA: 0x00099620 File Offset: 0x00097820
		// (set) Token: 0x06002316 RID: 8982 RVA: 0x00099628 File Offset: 0x00097828
		public string ItemId { get; set; }

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x06002317 RID: 8983 RVA: 0x00099631 File Offset: 0x00097831
		// (set) Token: 0x06002318 RID: 8984 RVA: 0x00099639 File Offset: 0x00097839
		public string StoreId { get; set; }

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x06002319 RID: 8985 RVA: 0x00099642 File Offset: 0x00097842
		// (set) Token: 0x0600231A RID: 8986 RVA: 0x0009964A File Offset: 0x0009784A
		public bool IsPermanent { get; set; }

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x0600231B RID: 8987 RVA: 0x00099653 File Offset: 0x00097853
		// (set) Token: 0x0600231C RID: 8988 RVA: 0x0009965B File Offset: 0x0009785B
		public CatalogItem CatalogItem { get; set; }

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x0600231D RID: 8989 RVA: 0x00099664 File Offset: 0x00097864
		// (set) Token: 0x0600231E RID: 8990 RVA: 0x0009966C File Offset: 0x0009786C
		public StoreItem StoreItem { get; set; }

		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x0600231F RID: 8991 RVA: 0x00099675 File Offset: 0x00097875
		// (set) Token: 0x06002320 RID: 8992 RVA: 0x0009967D File Offset: 0x0009787D
		public DateTime ExpiryDate { get; set; }

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06002321 RID: 8993 RVA: 0x00099686 File Offset: 0x00097886
		// (set) Token: 0x06002322 RID: 8994 RVA: 0x0009968E File Offset: 0x0009788E
		public ProductMetadata Metadata { get; set; }

		// Token: 0x06002323 RID: 8995 RVA: 0x00099697 File Offset: 0x00097897
		public override string ToString()
		{
			return string.Format("CatalogStoreItem ItemId=[{0}]", this.ItemId);
		}

		// Token: 0x0400248B RID: 9355
		public ReactiveProperty<bool> IsActive = new ReactiveProperty<bool>(false);

		// Token: 0x0400248C RID: 9356
		public ReactiveProperty<bool> Purchased = new ReactiveProperty<bool>(false);

		// Token: 0x0400248D RID: 9357
		public ReactiveProperty<double> TimeRemaining = new ReactiveProperty<double>();
	}
}
