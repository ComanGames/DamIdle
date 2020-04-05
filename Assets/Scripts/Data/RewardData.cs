using System;

// Token: 0x020000DA RID: 218
[Serializable]
public class RewardData
{
	// Token: 0x060005F7 RID: 1527 RVA: 0x00002EDA File Offset: 0x000010DA
	public RewardData()
	{
	}

	// Token: 0x060005F8 RID: 1528 RVA: 0x0001FC66 File Offset: 0x0001DE66
	public RewardData(string id, ERewardType type, int qty)
	{
		this.Id = id;
		this.RewardType = type;
		this.Qty = qty;
	}

	// Token: 0x04000552 RID: 1362
	public string Id;

	// Token: 0x04000553 RID: 1363
	public ERewardType RewardType;

	// Token: 0x04000554 RID: 1364
	public int Qty;
}
