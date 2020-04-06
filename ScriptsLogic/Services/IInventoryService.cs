using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

// Token: 0x0200001F RID: 31
public interface IInventoryService : IDisposable
{
	// Token: 0x1700000B RID: 11
	// (get) Token: 0x0600006B RID: 107
	List<Item> AllEquippableOwnedItmes { get; }

	// Token: 0x1700000C RID: 12
	// (get) Token: 0x0600006C RID: 108
	ReactiveProperty<bool> HasNewItem { get; }

	// Token: 0x1700000D RID: 13
	// (get) Token: 0x0600006D RID: 109
	Dictionary<ItemType, ReactiveProperty<int>> ItemTypeQuantityMap { get; }

	// Token: 0x0600006E RID: 110
	void LoadInventoryData(List<Item> data);

	// Token: 0x0600006F RID: 111
	void LoadItemSlots(List<ItemSlot> data);

	// Token: 0x06000070 RID: 112
	void AddItem(string id, int qty, bool sendKongAnalytics = true, bool newItem = true);

	// Token: 0x06000071 RID: 113
	Item GetItemById(string id);

	// Token: 0x06000072 RID: 114
	Color GetColourForRarity(int rarity);

	// Token: 0x06000073 RID: 115
	int GetSlotIndexForItem(Item item);

	// Token: 0x06000074 RID: 116
	List<string> GetAllItemIds();

	// Token: 0x06000075 RID: 117
	int GetEquippedItemsInSet(string suitSetId);

	// Token: 0x06000076 RID: 118
	int GetItemCountInSet(string suitSetId);

	// Token: 0x06000077 RID: 119
	List<Item> GetSuitItemsForId(string suitId);

	// Token: 0x06000078 RID: 120
	List<Item> GetAllItemsOfType(ItemType itemType);

	// Token: 0x06000079 RID: 121
	List<Item> GetAllPlayerOwnedItemsOfType(ItemType itemType);

	// Token: 0x0600007A RID: 122
	List<Item> GetItemsByIds(List<string> ids);

	// Token: 0x0600007B RID: 123
	List<Item> GetEquippedItemForType(ItemType itemType);

	// Token: 0x0600007C RID: 124
	Item GetEquippedItemForSlot(ItemType itemType, int itemIndexSlot = 0);

	// Token: 0x0600007D RID: 125
	string GetAllEquippedItemsStringForAnalytics();

	// Token: 0x0600007E RID: 126
	List<Item> GetAllEquippedItems();

	// Token: 0x0600007F RID: 127
	IObservable<int> GetOwned(string id);

	// Token: 0x06000080 RID: 128
	bool AreAllBadgeSlotsFull();

	// Token: 0x06000081 RID: 129
	void SetItemQuantity(string id, int qty);

	// Token: 0x06000082 RID: 130
	bool ConsumeItem(string id, int qty = 1);

	// Token: 0x06000083 RID: 131
	void UnequipItem(Item item);

	// Token: 0x06000084 RID: 132
	void EquipItem(Item itemToEquip, int itemSlotIndex, bool isLoadGameEquip = false);
}
