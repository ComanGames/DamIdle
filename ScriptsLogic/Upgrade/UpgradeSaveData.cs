using System;

// Token: 0x02000114 RID: 276
[Serializable]
public class UpgradeSaveData
{
	// Token: 0x06000744 RID: 1860 RVA: 0x00002EDA File Offset: 0x000010DA
	public UpgradeSaveData()
	{
	}

	// Token: 0x06000745 RID: 1861 RVA: 0x00026938 File Offset: 0x00024B38
	public UpgradeSaveData(string id, bool purchased)
	{
		this.id = id;
		this.purchased = purchased;
	}

	// Token: 0x040006C7 RID: 1735
	public string id;

	// Token: 0x040006C8 RID: 1736
	public bool purchased;
}
