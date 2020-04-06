using System;

// Token: 0x02000020 RID: 32
public struct InventoryEquipMessage
{
	// Token: 0x06000085 RID: 133 RVA: 0x000039A4 File Offset: 0x00001BA4
	public InventoryEquipMessage(Item item, int slotIndex, bool equipped)
	{
		this.item = item;
		this.slotIndex = slotIndex;
		this.equipped = equipped;
	}

	// Token: 0x04000075 RID: 117
	public Item item;

	// Token: 0x04000076 RID: 118
	public int slotIndex;

	// Token: 0x04000077 RID: 119
	public bool equipped;
}
