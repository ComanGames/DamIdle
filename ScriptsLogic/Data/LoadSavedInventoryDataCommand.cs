using System;
using System.Linq;

// Token: 0x0200001E RID: 30
public class LoadSavedInventoryDataCommand
{
	// Token: 0x06000067 RID: 103 RVA: 0x00003762 File Offset: 0x00001962
	public LoadSavedInventoryDataCommand()
	{
		this.globalPlayerData = PlayerData.GetPlayerData("Global");
		this.inventory = this.globalPlayerData.inventory;
	}

	// Token: 0x06000068 RID: 104 RVA: 0x0000378C File Offset: 0x0000198C
	public void Execute()
	{
		foreach (string text in this.inventory.GetAllItemIds())
		{
			if (this.globalPlayerData.Has(text))
			{
				this.inventory.SetItemQuantity(text, this.globalPlayerData.GetInt(text, 0));
			}
		}
		this.LoadItem(ItemType.Head, LoadSavedInventoryDataCommand.DEFAULT_EQUIPPED_ITEM_HEAD, 0);
		this.LoadItem(ItemType.Shirt, LoadSavedInventoryDataCommand.DEFAULT_EQUIPPED_ITEM_BODY, 0);
		this.LoadItem(ItemType.Pants, LoadSavedInventoryDataCommand.DEFAULT_EQUIPPED_ITEM_LEGS, 0);
		this.LoadItem(ItemType.Badge, null, 0);
		this.LoadItem(ItemType.Badge, null, 1);
		this.LoadItem(ItemType.Badge, null, 2);
		foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)).Cast<ItemType>())
		{
			this.inventory.ItemTypeQuantityMap[itemType].Value = this.inventory.GetAllItemsOfType(itemType).Count(x => x.Owned.Value > 0);
		}
	}

	// Token: 0x06000069 RID: 105 RVA: 0x000038D0 File Offset: 0x00001AD0
	private void LoadItem(ItemType itemType, string defaultId, int slotIndex)
	{
		string name = InventoryService.INVENTORY_SAVE_PREF_IDS[itemType][slotIndex];
		string text = this.globalPlayerData.Get(name, "");
		if (string.IsNullOrEmpty(text) || this.inventory.GetItemById(text).Owned.Value == 0)
		{
			if (string.IsNullOrEmpty(defaultId))
			{
				return;
			}
			text = defaultId;
		}
		if (this.inventory.GetItemById(text).IsEquipped)
		{
			GameController.Instance.AnalyticService.SendTaskCompleteEvent("DuplicateBadgeFix", "slot" + slotIndex, text);
			if (string.IsNullOrEmpty(defaultId))
			{
				return;
			}
			text = defaultId;
		}
		this.inventory.EquipItem(this.inventory.GetItemById(text), slotIndex, true);
	}

	// Token: 0x04000070 RID: 112
	public static string DEFAULT_EQUIPPED_ITEM_HEAD = "default_earth_basic_head";

	// Token: 0x04000071 RID: 113
	public static string DEFAULT_EQUIPPED_ITEM_BODY = "default_earth_basic_shirt";

	// Token: 0x04000072 RID: 114
	public static string DEFAULT_EQUIPPED_ITEM_LEGS = "default_earth_basic_pants";

	// Token: 0x04000073 RID: 115
	private readonly PlayerData globalPlayerData;

	// Token: 0x04000074 RID: 116
	private readonly IInventoryService inventory;
}
