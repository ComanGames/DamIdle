using System;

// Token: 0x02000121 RID: 289
public static class AdCapExternalDataStorage
{
	// Token: 0x1700008E RID: 142
	// (get) Token: 0x060007A4 RID: 1956 RVA: 0x00028AB6 File Offset: 0x00026CB6
	// (set) Token: 0x060007A5 RID: 1957 RVA: 0x00028ABD File Offset: 0x00026CBD
	public static InventoryJsonDataObject Data { get; private set; }

	// Token: 0x060007A6 RID: 1958 RVA: 0x00028AC5 File Offset: 0x00026CC5
	public static void LoadData(InventoryJsonDataObject data)
	{
		AdCapExternalDataStorage.Data = data;
	}
}
