using System;
using PlayFab;
using PlayFab.ClientModels;

namespace Platforms
{
	// Token: 0x020006CF RID: 1743
	public struct PurchaseEvent
	{
		// Token: 0x060023F3 RID: 9203 RVA: 0x0009BB22 File Offset: 0x00099D22
		public PurchaseEvent(CatalogStoreItem item, string currencyCode)
		{
			this = new PurchaseEvent(item, currencyCode, null, null, null);
		}

		// Token: 0x060023F4 RID: 9204 RVA: 0x0009BB2F File Offset: 0x00099D2F
		public PurchaseEvent(CatalogStoreItem item, string currencyCode, object transactionDetails)
		{
			this = new PurchaseEvent(item, currencyCode, transactionDetails, null, null);
		}

		// Token: 0x060023F5 RID: 9205 RVA: 0x0009BB3C File Offset: 0x00099D3C
		public PurchaseEvent(CatalogStoreItem item, string currencyCode, object transactionDetails, PurchaseItemResult purchaseItemResult)
		{
			this = new PurchaseEvent(item, currencyCode, transactionDetails, purchaseItemResult, null);
		}

		// Token: 0x060023F6 RID: 9206 RVA: 0x0009BB4A File Offset: 0x00099D4A
		public PurchaseEvent(CatalogStoreItem item, string currencyCode, PlayFabError error)
		{
			this = new PurchaseEvent(item, currencyCode, null, null, error);
		}

		// Token: 0x060023F7 RID: 9207 RVA: 0x0009BB57 File Offset: 0x00099D57
		public PurchaseEvent(CatalogStoreItem item, string currencyCode, object transactionDetails, PlayFabError error)
		{
			this = new PurchaseEvent(item, currencyCode, transactionDetails, null, error);
		}

		// Token: 0x060023F8 RID: 9208 RVA: 0x0009BB65 File Offset: 0x00099D65
		public PurchaseEvent(CatalogStoreItem item, string currencyCode, object transactionDetails, PurchaseItemResult purchaseItemResult, PlayFabError error)
		{
			this.Item = item;
			this.CurrencyCode = currencyCode;
			this.TransactionDetails = transactionDetails;
			this.PurchaseItemResult = purchaseItemResult;
			this.Error = error;
		}

		// Token: 0x040024D3 RID: 9427
		public CatalogStoreItem Item;

		// Token: 0x040024D4 RID: 9428
		public string CurrencyCode;

		// Token: 0x040024D5 RID: 9429
		public object TransactionDetails;

		// Token: 0x040024D6 RID: 9430
		public PurchaseItemResult PurchaseItemResult;

		// Token: 0x040024D7 RID: 9431
		public PlayFabError Error;
	}
}
