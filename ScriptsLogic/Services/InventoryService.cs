using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

// Token: 0x02000023 RID: 35
public class InventoryService : IInventoryService, IDisposable
{
	// Token: 0x1700000E RID: 14
	// (get) Token: 0x0600009C RID: 156 RVA: 0x000045A4 File Offset: 0x000027A4
	public List<Item> AllEquippableOwnedItmes
	{
		get
		{
			return (from x in this.allItems
			select x.Value into x
			where (x.IsEquipable() || x.ItemType == ItemType.Trophy) && x.Owned.Value > 0 && !x.IsNudeEquipment
			select x).ToList<Item>();
		}
	}

	// Token: 0x1700000F RID: 15
	// (get) Token: 0x0600009D RID: 157 RVA: 0x00004604 File Offset: 0x00002804
	public ReactiveProperty<bool> HasNewItem
	{
		get
		{
			return this.hasNewItem;
		}
	}

	// Token: 0x17000010 RID: 16
	// (get) Token: 0x0600009E RID: 158 RVA: 0x0000460C File Offset: 0x0000280C
	public Dictionary<ItemType, ReactiveProperty<int>> ItemTypeQuantityMap { get; }

	// Token: 0x0600009F RID: 159 RVA: 0x00004614 File Offset: 0x00002814
	public InventoryService()
	{
		this.ItemTypeQuantityMap = new Dictionary<ItemType, ReactiveProperty<int>>
		{
			{
				ItemType.Badge,
				new ReactiveProperty<int>(0)
			},
			{
				ItemType.Currency,
				new ReactiveProperty<int>(0)
			},
			{
				ItemType.Head,
				new ReactiveProperty<int>(0)
			},
			{
				ItemType.Pants,
				new ReactiveProperty<int>(0)
			},
			{
				ItemType.Shirt,
				new ReactiveProperty<int>(0)
			},
			{
				ItemType.SuitSet,
				new ReactiveProperty<int>(0)
			},
			{
				ItemType.Trophy,
				new ReactiveProperty<int>(0)
			},
			{
				ItemType.None,
				new ReactiveProperty<int>(0)
			}
		};
	}

	// Token: 0x060000A0 RID: 160 RVA: 0x000046C8 File Offset: 0x000028C8
	~InventoryService()
	{
		this.Dispose();
	}

	// Token: 0x060000A1 RID: 161 RVA: 0x00002718 File Offset: 0x00000918
	public void Dispose()
	{
	}

	// Token: 0x060000A2 RID: 162 RVA: 0x000046F4 File Offset: 0x000028F4
	private void CheckForCompleteSuit(bool isFromLoad = false)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (ItemType key in this.equipItemSlots.Keys)
		{
			for (int i = 0; i < this.equipItemSlots[key].Length; i++)
			{
				if (this.equipItemSlots[key][i] != null && this.equipItemSlots[key][i] != null && !string.IsNullOrEmpty(this.equipItemSlots[key][i].ItemSetId))
				{
					if (dictionary.ContainsKey(this.equipItemSlots[key][i].ItemSetId))
					{
						Dictionary<string, int> dictionary2 = dictionary;
						string itemSetId = this.equipItemSlots[key][i].ItemSetId;
						int num = dictionary2[itemSetId];
						dictionary2[itemSetId] = num + 1;
					}
					else
					{
						dictionary.Add(this.equipItemSlots[key][i].ItemSetId, 1);
					}
				}
			}
		}
		foreach (string text in dictionary.Keys)
		{
			if (this.itemSets[text].Count == dictionary[text])
			{
				if (this.equipItemSlots[ItemType.SuitSet][0] != null)
				{
					if (this.equipItemSlots[ItemType.SuitSet][0] == this.allItems[text])
					{
						return;
					}
					this.SuitSetEquipStateChanged(text, false, isFromLoad);
					this.equipItemSlots[ItemType.SuitSet][0] = null;
				}
				this.equipItemSlots[ItemType.SuitSet][0] = this.allItems[text];
				this.SuitSetEquipStateChanged(text, true, isFromLoad);
				return;
			}
		}
		if (this.equipItemSlots[ItemType.SuitSet][0] != null)
		{
			this.SuitSetEquipStateChanged(this.equipItemSlots[ItemType.SuitSet][0].ItemId, false, isFromLoad);
			this.equipItemSlots[ItemType.SuitSet][0] = null;
		}
	}

	// Token: 0x060000A3 RID: 163 RVA: 0x0000491C File Offset: 0x00002B1C
	private void SuitSetEquipStateChanged(string suitSetId, bool equipped, bool isFromLoad = false)
	{
		if (this.allItems[suitSetId].ItemBonusType == ItemBonusType.SuitPiecesEffectBoost)
		{
			this.ApplySuitSetBonusToItemsInSet(this.allItems[suitSetId], equipped);
			return;
		}
		if (!isFromLoad)
		{
			MessageBroker.Default.Publish<InventoryEquipMessage>(new InventoryEquipMessage(this.allItems[suitSetId], 0, equipped));
		}
	}

	// Token: 0x060000A4 RID: 164 RVA: 0x00004974 File Offset: 0x00002B74
	private void ApplySuitSetBonusToItemsInSet(Item suit, bool equipped)
	{
		foreach (Item item in this.itemSets[suit.ItemId])
		{
			item.SuitSetBonusToItem += (float)(equipped ? 1 : -1) * (suit.GetLeveledBonus(0) - 1f);
			item.CalculateLeveledBonus();
		}
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x000049F4 File Offset: 0x00002BF4
	private void UnequipBadgeIfEquipped(Item item)
	{
		if (item == null)
		{
			return;
		}
		for (int i = 0; i < this.equipItemSlots[ItemType.Badge].Length; i++)
		{
			if (this.equipItemSlots[ItemType.Badge][i] == item)
			{
				this.UnequipItemSlot(ItemType.Badge, i);
				return;
			}
		}
	}

	// Token: 0x060000A6 RID: 166 RVA: 0x00004A38 File Offset: 0x00002C38
	private void UnequipItemSlot(ItemType itemType, int itemSlotIndex = 0)
	{
		if (this.equipItemSlots[itemType][itemSlotIndex] == null)
		{
			return;
		}
		Item item = this.equipItemSlots[itemType][itemSlotIndex];
		this.equipItemSlots[itemType][itemSlotIndex].IsEquipped = false;
		this.equipItemSlots[itemType][itemSlotIndex] = null;
		this.CheckForCompleteSuit(false);
		MessageBroker.Default.Publish<InventoryEquipMessage>(new InventoryEquipMessage(item, itemSlotIndex, false));
	}

	// Token: 0x060000A7 RID: 167 RVA: 0x00004AA4 File Offset: 0x00002CA4
	public void EquipItem(Item itemToEquip, int itemSlotIndex, bool isLoadGameEquip = false)
	{
		Item item = null;
		if (itemToEquip.ItemType == ItemType.Badge)
		{
			if (itemSlotIndex == -1)
			{
				for (int i = 0; i < this.equipItemSlots[ItemType.Badge].Length; i++)
				{
					if (this.equipItemSlots[ItemType.Badge][i] == null)
					{
						itemSlotIndex = i;
						break;
					}
				}
			}
			item = this.equipItemSlots[itemToEquip.ItemType][itemSlotIndex];
		}
		else
		{
			itemSlotIndex = 0;
		}
		this.UnequipItemSlot(itemToEquip.ItemType, itemSlotIndex);
		if (!isLoadGameEquip)
		{
			GameController.Instance.AnalyticService.SendNavActionAnalytics(itemToEquip.ItemName, "Inventory", itemToEquip.Owned.Value.ToString());
		}
		itemToEquip.IsEquipped = true;
		this.equipItemSlots[itemToEquip.ItemType][itemSlotIndex] = itemToEquip;
		this.UnequipBadgeIfEquipped(item);
		this.CheckForCompleteSuit(isLoadGameEquip);
		if (!isLoadGameEquip)
		{
			MessageBroker.Default.Publish<InventoryEquipMessage>(new InventoryEquipMessage(itemToEquip, itemSlotIndex, true));
		}
	}

	// Token: 0x060000A8 RID: 168 RVA: 0x00004B84 File Offset: 0x00002D84
	public void UnequipItem(Item item)
	{
		for (int i = 0; i < this.equipItemSlots[item.ItemType].Length; i++)
		{
			if (this.equipItemSlots[item.ItemType][i] == item)
			{
				this.UnequipItemSlot(item.ItemType, i);
				break;
			}
		}
		switch (item.ItemType)
		{
		case ItemType.Head:
			this.equipItemSlots[ItemType.Head][0] = this.allItems[InventoryService.NUDE_ITEM_HEAD];
			MessageBroker.Default.Publish<InventoryEquipMessage>(new InventoryEquipMessage(this.allItems[InventoryService.NUDE_ITEM_HEAD], 0, true));
			this.CheckForCompleteSuit(false);
			return;
		case ItemType.Shirt:
			this.equipItemSlots[ItemType.Shirt][0] = this.allItems[InventoryService.NUDE_ITEM_BODY];
			MessageBroker.Default.Publish<InventoryEquipMessage>(new InventoryEquipMessage(this.allItems[InventoryService.NUDE_ITEM_BODY], 0, true));
			this.CheckForCompleteSuit(false);
			return;
		case ItemType.Pants:
			this.equipItemSlots[ItemType.Pants][0] = this.allItems[InventoryService.NUDE_ITEM_LEGS];
			MessageBroker.Default.Publish<InventoryEquipMessage>(new InventoryEquipMessage(this.allItems[InventoryService.NUDE_ITEM_LEGS], 0, true));
			this.CheckForCompleteSuit(false);
			return;
		default:
			return;
		}
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x00004CC4 File Offset: 0x00002EC4
	public void LoadItemSlots(List<ItemSlot> data)
	{
		foreach (ItemSlot itemSlot in data)
		{
			ItemType key = (ItemType)Enum.Parse(typeof(ItemType), itemSlot.SlotName);
			this.equipItemSlots.Add(key, new Item[itemSlot.SlotCount]);
		}
	}

	// Token: 0x060000AA RID: 170 RVA: 0x00004D40 File Offset: 0x00002F40
	public void LoadInventoryData(List<Item> data)
	{
		string value = "Steam";
		if (this.Initialized)
		{
			Debug.LogError("Attempting to load the inventory a second time. Escaping out.");
			return;
		}
		this.playerData = PlayerData.GetPlayerData("Global");
		List<string> list = new List<string>();
		foreach (Item item in data)
		{
			if (item.ItemType == ItemType.SuitSet)
			{
				item.Owned.Value = 1;
			}
			if (item.ItemId == InventoryService.NUDE_ITEM_HEAD || item.ItemId == InventoryService.NUDE_ITEM_BODY || item.ItemId == InventoryService.NUDE_ITEM_LEGS)
			{
				item.IsNudeEquipment = true;
			}
			if (true & (string.IsNullOrEmpty(item.Platforms) || item.Platforms.Contains(value)) & (string.IsNullOrEmpty(item.ABTestGroup) || GameController.Instance.UserDataService.IsTestGroupMember(item.ABTestGroup)))
			{
				if (!this.allItems.ContainsKey(item.ItemId))
				{
					this.allItems.Add(item.ItemId, item);
				}
				else
				{
					Debug.LogErrorFormat("Inventory already contains item: {0} Group: {1}", new object[]
					{
						item.ItemId,
						item.ABTestGroup
					});
				}
			}
			else if (!list.Contains(item.ItemId))
			{
				list.Add(item.ItemId);
			}
			item.NewItem = false;
			if (!string.IsNullOrEmpty(item.ItemSetId))
			{
				if (!this.itemSets.ContainsKey(item.ItemSetId))
				{
					this.itemSets.Add(item.ItemSetId, new List<Item>());
				}
				this.itemSets[item.ItemSetId].Add(item);
			}
		}
		using (List<string>.Enumerator enumerator2 = list.GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				string itemId = enumerator2.Current;
				if (!this.allItems.Any(x => x.Key == itemId))
				{
					Debug.LogErrorFormat("MISSING ITEM: " + itemId, Array.Empty<object>());
				}
			}
		}
		this.Initialized = true;
	}

	// Token: 0x060000AB RID: 171 RVA: 0x00004FA8 File Offset: 0x000031A8
	public void AddItem(string id, int qty, bool sendKongAnalytics = true, bool newItem = true)
	{
		if (this.itemSets.ContainsKey(id))
		{
			using (List<Item>.Enumerator enumerator = this.itemSets[id].GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item = enumerator.Current;
					this.AddItem(item.ItemId, qty, sendKongAnalytics, true);
				}
				return;
			}
		}
		if (!this.allItems.ContainsKey(id))
		{
			Debug.LogErrorFormat("[INVENTORY] Item {0} not found", new object[]
			{
				id
			});
			return;
		}
		Item item2 = this.allItems[id];
		item2.Owned.Value += qty;
		if (item2.Owned.Value == qty)
		{
			item2.NewItem = newItem;
			this.ItemTypeQuantityMap[item2.ItemType].Value++;
			if (item2.IsEquipable() || item2.ItemType == ItemType.Trophy)
			{
				this.HasNewItem.Value = newItem;
				MessageBroker.Default.Publish<Item>(this.allItems[id]);
			}
		}
		if (sendKongAnalytics && this.allItems[id].IsEquipable() && KongregateGameObject.Instance != null)
		{
			Item item3 = this.allItems[id];
			GameController.Instance.AnalyticService.SendSwagAcquired(item3.ItemName, item3.Owned.Value - qty, qty, item3.RarityRank);
		}
		this.playerData.Set(id, this.allItems[id].Owned.Value.ToString());
	}

	// Token: 0x060000AC RID: 172 RVA: 0x0000514C File Offset: 0x0000334C
	public bool ConsumeItem(string id, int qty = 1)
	{
		if (qty < 0 || this.allItems[id].Owned.Value < qty)
		{
			return false;
		}
		this.allItems[id].Owned.Value -= qty;
		this.playerData.Set(id, this.allItems[id].Owned.Value.ToString());
		return true;
	}

	// Token: 0x060000AD RID: 173 RVA: 0x000051C2 File Offset: 0x000033C2
	public void SetItemQuantity(string id, int qty)
	{
		this.allItems[id].Owned.Value = qty;
	}

	// Token: 0x060000AE RID: 174 RVA: 0x000051DB File Offset: 0x000033DB
	public bool AreAllBadgeSlotsFull()
	{
		return this.equipItemSlots[ItemType.Badge].All(item => item != null);
	}

	// Token: 0x060000AF RID: 175 RVA: 0x0000520D File Offset: 0x0000340D
	public IObservable<int> GetOwned(string id)
	{
		return this.allItems[id].Owned;
	}

	// Token: 0x060000B0 RID: 176 RVA: 0x00005220 File Offset: 0x00003420
	public List<Item> GetAllEquippedItems()
	{
		return (from item in this.equipItemSlots.SelectMany(x => x.Value)
		where item != null
		select item).ToList<Item>();
	}

	// Token: 0x060000B1 RID: 177 RVA: 0x00005280 File Offset: 0x00003480
	public string GetAllEquippedItemsStringForAnalytics()
	{
		string text = "";
		List<Item> allEquippedItems = this.GetAllEquippedItems();
		if (allEquippedItems != null && allEquippedItems.Count > 0)
		{
			for (int i = 0; i < allEquippedItems.Count; i++)
			{
				if (i != 0)
				{
					text += ",";
				}
				text = string.Concat(new object[]
				{
					text,
					allEquippedItems[i].ItemId,
					":",
					allEquippedItems[i].Owned.Value
				});
			}
		}
		return text;
	}

	// Token: 0x060000B2 RID: 178 RVA: 0x00005307 File Offset: 0x00003507
	public Item GetEquippedItemForSlot(ItemType itemType, int itemIndexSlot = 0)
	{
		if (this.equipItemSlots.ContainsKey(itemType))
		{
			return this.equipItemSlots[itemType][itemIndexSlot];
		}
		return null;
	}

	// Token: 0x060000B3 RID: 179 RVA: 0x00005328 File Offset: 0x00003528
	public List<Item> GetEquippedItemForType(ItemType itemType)
	{
		List<Item> list = new List<Item>();
		foreach (Item item in this.equipItemSlots[itemType])
		{
			if (item != null)
			{
				list.Add(item);
			}
		}
		return list;
	}

	// Token: 0x060000B4 RID: 180 RVA: 0x00005365 File Offset: 0x00003565
	public Item GetItemById(string id)
	{
		if (string.IsNullOrEmpty(id) || !this.allItems.ContainsKey(id))
		{
			Debug.LogWarningFormat("GetItemById() Key [{0}] not present", new object[]
			{
				id
			});
			return null;
		}
		return this.allItems[id];
	}

	// Token: 0x060000B5 RID: 181 RVA: 0x000053A0 File Offset: 0x000035A0
	public List<Item> GetItemsByIds(List<string> ids)
	{
		List<Item> items = new List<Item>();
		ids.ForEach(delegate(string x)
		{
			items.Add(this.GetItemById(x));
		});
		return items;
	}

	// Token: 0x060000B6 RID: 182 RVA: 0x000053E0 File Offset: 0x000035E0
	public List<Item> GetAllPlayerOwnedItemsOfType(ItemType itemType)
	{
		List<Item> list = new List<Item>();
		foreach (string key in this.allItems.Keys)
		{
			if (this.allItems[key].Owned.Value > 0 && this.allItems[key].ItemType == itemType)
			{
				list.Add(this.allItems[key]);
			}
		}
		return list;
	}

	// Token: 0x060000B7 RID: 183 RVA: 0x00005478 File Offset: 0x00003678
	public List<Item> GetAllItemsOfType(ItemType itemType)
	{
		List<Item> list = new List<Item>();
		foreach (string key in this.allItems.Keys)
		{
			if (this.allItems[key].ItemType == itemType)
			{
				list.Add(this.allItems[key]);
			}
		}
		return list;
	}

	// Token: 0x060000B8 RID: 184 RVA: 0x000054F8 File Offset: 0x000036F8
	public List<Item> GetSuitItemsForId(string suitId)
	{
		if (this.itemSets.ContainsKey(suitId))
		{
			return this.itemSets[suitId];
		}
		return null;
	}

	// Token: 0x060000B9 RID: 185 RVA: 0x00005516 File Offset: 0x00003716
	public int GetItemCountInSet(string suitSetId)
	{
		return this.itemSets[suitSetId].Count;
	}

	// Token: 0x060000BA RID: 186 RVA: 0x0000552C File Offset: 0x0000372C
	public int GetEquippedItemsInSet(string suitSetId)
	{
		int num = 0;
		List<Item> allEquippedItems = this.GetAllEquippedItems();
		for (int i = 0; i < allEquippedItems.Count; i++)
		{
			if (allEquippedItems[i].ItemSetId == suitSetId)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x060000BB RID: 187 RVA: 0x0000556C File Offset: 0x0000376C
	public List<string> GetAllItemIds()
	{
		return this.allItems.Keys.ToList<string>();
	}

	// Token: 0x060000BC RID: 188 RVA: 0x00005580 File Offset: 0x00003780
	public int GetSlotIndexForItem(Item item)
	{
		foreach (KeyValuePair<ItemType, Item[]> keyValuePair in this.equipItemSlots)
		{
			for (int i = 0; i < keyValuePair.Value.Length; i++)
			{
				if (keyValuePair.Value[i] == item)
				{
					return i;
				}
			}
		}
		return -1;
	}

	// Token: 0x060000BD RID: 189 RVA: 0x000055F4 File Offset: 0x000037F4
	public Color GetColourForRarity(int rarity)
	{
		return AdCapColours.ColourMap[(ColourNames)rarity].Color;
	}

	// Token: 0x0400008D RID: 141
	public static string NUDE_ITEM_HEAD = "nude_universal_basic_head";

	// Token: 0x0400008E RID: 142
	public static string NUDE_ITEM_BODY = "nude_universal_basic_shirt";

	// Token: 0x0400008F RID: 143
	public static string NUDE_ITEM_LEGS = "nude_universal_basic_pants";

	// Token: 0x04000090 RID: 144
	public static Dictionary<ItemType, string[]> INVENTORY_SAVE_PREF_IDS = new Dictionary<ItemType, string[]>
	{
		{
			ItemType.Head,
			new string[]
			{
				"equip_head_slot"
			}
		},
		{
			ItemType.Shirt,
			new string[]
			{
				"equip_body_slot"
			}
		},
		{
			ItemType.Pants,
			new string[]
			{
				"equip_legs_slot"
			}
		},
		{
			ItemType.SuitSet,
			new string[]
			{
				"equip_suit_set_slot"
			}
		},
		{
			ItemType.Badge,
			new string[]
			{
				"equip_badge_one_slot",
				"equip_badge_two_slot",
				"equip_badge_three_slot"
			}
		}
	};

	// Token: 0x04000091 RID: 145
	private readonly Dictionary<string, Item> allItems = new Dictionary<string, Item>();

	// Token: 0x04000092 RID: 146
	private readonly Dictionary<string, List<Item>> itemSets = new Dictionary<string, List<Item>>();

	// Token: 0x04000093 RID: 147
	private readonly Dictionary<ItemType, Item[]> equipItemSlots = new Dictionary<ItemType, Item[]>();

	// Token: 0x04000094 RID: 148
	private PlayerData playerData;

	// Token: 0x04000096 RID: 150
	private readonly ReactiveProperty<bool> hasNewItem = new ReactiveProperty<bool>();

	// Token: 0x04000097 RID: 151
	public bool Initialized;
}
