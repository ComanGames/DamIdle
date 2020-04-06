using System;

// Token: 0x02000173 RID: 371
public class UnlockRewardItemizationItem : IUnlockReward
{
	// Token: 0x1700010F RID: 271
	// (get) Token: 0x06000BCA RID: 3018 RVA: 0x00035A97 File Offset: 0x00033C97
	// (set) Token: 0x06000BCB RID: 3019 RVA: 0x00035A9F File Offset: 0x00033C9F
	public string id { get; private set; }

	// Token: 0x17000110 RID: 272
	// (get) Token: 0x06000BCC RID: 3020 RVA: 0x00035AA8 File Offset: 0x00033CA8
	// (set) Token: 0x06000BCD RID: 3021 RVA: 0x00035AB0 File Offset: 0x00033CB0
	public string name { get; private set; }

	// Token: 0x17000111 RID: 273
	// (get) Token: 0x06000BCE RID: 3022 RVA: 0x00035AB9 File Offset: 0x00033CB9
	// (set) Token: 0x06000BCF RID: 3023 RVA: 0x00035AC1 File Offset: 0x00033CC1
	public int qty { get; private set; }

	// Token: 0x17000112 RID: 274
	// (get) Token: 0x06000BD0 RID: 3024 RVA: 0x00035ACA File Offset: 0x00033CCA
	// (set) Token: 0x06000BD1 RID: 3025 RVA: 0x00035AD2 File Offset: 0x00033CD2
	public RewardData rewardData { get; private set; }

	// Token: 0x06000BD2 RID: 3026 RVA: 0x00035ADC File Offset: 0x00033CDC
	public UnlockRewardItemizationItem(string id, int count)
	{
		this.id = id;
		this.qty = count;
		Item itemById = GameController.Instance.GlobalPlayerData.inventory.GetItemById(id);
		this.itemName = itemById.ItemName;
		this.rewardData = new RewardData(itemById.ItemId, ERewardType.Item, this.qty);
	}

	// Token: 0x06000BD3 RID: 3027 RVA: 0x00035B37 File Offset: 0x00033D37
	public string Description(GameState state)
	{
		return string.Format("Earn item: {0}", this.itemName);
	}

	// Token: 0x06000BD4 RID: 3028 RVA: 0x00035B49 File Offset: 0x00033D49
	public string ShortDescription(GameState state)
	{
		return string.Format("{0}\nItem", this.itemName);
	}

	// Token: 0x06000BD5 RID: 3029 RVA: 0x00035B5C File Offset: 0x00033D5C
	public void Apply(GameState state)
	{
		GameController.Instance.GrantRewardService.GrantReward(this.rewardData, string.Concat(new object[]
		{
			"Item_Reward_",
			this.rewardData.Id,
			"_x",
			this.rewardData.Qty
		}), GameController.Instance.planetName, false);
	}

	// Token: 0x04000A27 RID: 2599
	private string itemName;
}
