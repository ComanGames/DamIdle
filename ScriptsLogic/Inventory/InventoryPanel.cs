using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000022 RID: 34
public class InventoryPanel : PanelBaseClass
{
	// Token: 0x06000088 RID: 136 RVA: 0x00003A78 File Offset: 0x00001C78
	private void Awake()
	{
		this.viewingDropdown.value = 0;
		this.sortingDropdown.value = 0;
		this.planetDropdown.value = 0;
		this.emptyInventoryCanvasGroup.alpha = 0f;
		this.emptyInventoryCanvasGroup.blocksRaycasts = false;
		this.viewingDropdown.onValueChanged.AddListener(new UnityAction<int>(this.ViewingValueChanged));
		this.planetDropdown.onValueChanged.AddListener(new UnityAction<int>(this.PlanetValueChanged));
		this.sortingDropdown.onValueChanged.AddListener(new UnityAction<int>(this.SortingValueChanged));
		this.InventorySortingStruct.ItemType = ItemType.None;
		this.InventorySortingStruct.SortingMethod = SortingMethod.Rarity;
		this.InventorySortingStruct.PlanetFilter = "All";
		this.inventory = PlayerData.GetPlayerData("Global").inventory;
		this.navService = GameController.Instance.NavigationService;
		MessageBroker.Default.Receive<Item>().Subscribe(delegate(Item item)
		{
			this.CreateItem(item);
			this.SortItems();
		}).AddTo(base.gameObject);
		this.backButton.gameObject.SetActive(false);
		this.backButton.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnBackButtonPressed();
		}).AddTo(base.gameObject);
		this.swapBadgeUI.SetCanvasGroupState(false);
		this.goToStoreButton.OnClickAsObservable().Subscribe(new Action<Unit>(this.GoToStorePanel)).AddTo(base.gameObject);
		this.resetFilterButton.gameObject.SetActive(true);
		this.resetFilterButton.OnClickAsObservable().Subscribe(new Action<Unit>(this.ResetFilters)).AddTo(base.gameObject);
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
		this.SetupViewForAllTypes();
	}

	// Token: 0x06000089 RID: 137 RVA: 0x00003C60 File Offset: 0x00001E60
	private IEnumerator Start()
	{
		List<Item> allItems = (from x in this.inventory.AllEquippableOwnedItmes
		orderby x.RarityRank descending
		select x).ToList<Item>();
		int num;
		for (int i = 0; i < allItems.Count; i = num)
		{
			if (i % 10 == 0)
			{
				yield return null;
			}
			this.CreateItem(allItems[i]);
			num = i + 1;
		}
		yield break;
	}

	// Token: 0x0600008A RID: 138 RVA: 0x00003C6F File Offset: 0x00001E6F
	private void GoToStorePanel(Unit u)
	{
		MessageBroker.Default.Publish<StoreServiceNavController.OpenStoreCommand>(new StoreServiceNavController.OpenStoreCommand("Items", "Career", "NoItemsStoreButton", null, false));
		this.ParentModalController.CloseModal(u);
	}

	// Token: 0x0600008B RID: 139 RVA: 0x00003CA0 File Offset: 0x00001EA0
	private void ResetFilters(Unit u)
	{
		this.InventorySortingStruct.ItemType = this.filterType;
		this.InventorySortingStruct.SortingMethod = SortingMethod.Rarity;
		this.InventorySortingStruct.PlanetFilter = "All";
		this.viewingDropdown.value = (int)(this.filterType + 1);
		this.ViewingValueChanged((int)(this.filterType + 1));
		this.sortingDropdown.value = 0;
		this.planetDropdown.value = 0;
		this.SortItems();
	}

	// Token: 0x0600008C RID: 140 RVA: 0x00003D19 File Offset: 0x00001F19
	private void OnDestroy()
	{
		this.viewingDropdown.onValueChanged.RemoveAllListeners();
		this.sortingDropdown.onValueChanged.RemoveAllListeners();
		this.planetDropdown.onValueChanged.RemoveAllListeners();
	}

	// Token: 0x0600008D RID: 141 RVA: 0x00003D4C File Offset: 0x00001F4C
	private void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		if (orientation.IsPortrait)
		{
			this.inventoryListGrid.cellSize = new Vector2(210f, 210f);
			this.inventoryListGrid.spacing = new Vector2(20f, 20f);
			return;
		}
		this.inventoryListGrid.cellSize = new Vector2(200f, 200f);
		this.inventoryListGrid.spacing = new Vector2(10f, 10f);
	}

	// Token: 0x0600008E RID: 142 RVA: 0x00003DCC File Offset: 0x00001FCC
	private void ViewingValueChanged(int newValue)
	{
		ItemType itemType = this.InventorySortingStruct.ItemType;
		if (-1 != newValue)
		{
			if (newValue == 5)
			{
				itemType = ItemType.Trophy;
			}
			else
			{
				itemType = (ItemType)(newValue - 1);
			}
		}
		if (itemType != this.InventorySortingStruct.ItemType)
		{
			this.InventorySortingStruct.ItemType = itemType;
			this.SortItems();
		}
	}

	// Token: 0x0600008F RID: 143 RVA: 0x00003E18 File Offset: 0x00002018
	private void PlanetValueChanged(int newValue)
	{
		string text = this.planetDropdown.options[newValue].text;
		if (text != this.InventorySortingStruct.PlanetFilter)
		{
			this.InventorySortingStruct.PlanetFilter = text;
			this.SortItems();
		}
	}

	// Token: 0x06000090 RID: 144 RVA: 0x00003E61 File Offset: 0x00002061
	private void SortingValueChanged(int newValue)
	{
		this.InventorySortingStruct.SortingMethod = (SortingMethod)newValue;
		this.SortItems();
	}

	// Token: 0x06000091 RID: 145 RVA: 0x00003E78 File Offset: 0x00002078
	private void SortItems()
	{
		List<ItemIconView> list = new List<ItemIconView>();
		List<Item> equippedItems = this.inventory.GetAllEquippedItems();
		List<ItemIconView> list2 = this.inventoryItemsCreated;
		string a = this.InventorySortingStruct.PlanetFilter.ToLower();
		List<ItemIconView> source;
		if (!(a == "all"))
		{
			if (!(a == "earth") && !(a == "moon") && !(a == "mars"))
			{
				if (!(a == "current adventure"))
				{
					if (!(a == "event"))
					{
						Debug.LogError("Unhandled inventory filter type = " + this.InventorySortingStruct.PlanetFilter);
						source = new List<ItemIconView>();
					}
					else
					{
						source = (from x in list2
						where x.Item.IsEquipable() && (string.IsNullOrEmpty(x.Item.BonusCustomData) || x.Item.BonusCustomData == "AllEvents" || (x.Item.BonusCustomData != "AllPlanets" && GameState.IsEvent(x.Item.BonusCustomData.Split(new char[]
						{
							':'
						})[0])))
						select x).ToList<ItemIconView>();
					}
				}
				else
				{
					string allString = "AllPlanets";
					string currenttheme = GameController.Instance.game.planetTheme.Value;
					if (GameController.Instance.game.IsEventPlanet)
					{
						allString = "AllEvents";
					}
					source = (from x in list2
					where x.Item.IsEquipable() && (string.IsNullOrEmpty(x.Item.BonusCustomData) || x.Item.BonusCustomData == allString || x.Item.BonusCustomData.ToLower().StartsWith(currenttheme.ToLower()))
					select x).ToList<ItemIconView>();
				}
			}
			else
			{
				source = (from x in list2
				where x.Item.IsEquipable() && (string.IsNullOrEmpty(x.Item.BonusCustomData) || x.Item.BonusCustomData == "AllPlanets" || x.Item.BonusCustomData.ToLower().StartsWith(this.InventorySortingStruct.PlanetFilter.ToLower()))
				select x).ToList<ItemIconView>();
			}
		}
		else
		{
			source = list2;
		}
		source = (from x in source
		where x.Item.ItemType == this.InventorySortingStruct.ItemType || this.InventorySortingStruct.ItemType == ItemType.None
		select x).ToList<ItemIconView>();
		switch (this.InventorySortingStruct.SortingMethod)
		{
		case SortingMethod.Rarity:
			list = (from x in source
			orderby x.Item.RarityRank descending, x.Item.Owned.Value descending, equippedItems.Contains(x.Item) descending
			select x).ToList<ItemIconView>();
			break;
		case SortingMethod.Level:
			list = (from x in source
			orderby x.Item.Owned.Value descending, x.Item.RarityRank descending, equippedItems.Contains(x.Item) descending
			select x).ToList<ItemIconView>();
			break;
		case SortingMethod.Equipped:
			list = (from x in source
			orderby equippedItems.Contains(x.Item) descending, x.Item.RarityRank descending, x.Item.Owned.Value descending
			select x).ToList<ItemIconView>();
			break;
		}
		for (int i = 0; i < list2.Count; i++)
		{
			ItemIconView itemIconView = list2[i];
			int num = list.IndexOf(itemIconView);
			if (num > -1)
			{
				itemIconView.gameObject.SetActive(true);
				itemIconView.transform.SetSiblingIndex(num);
			}
			else
			{
				itemIconView.gameObject.SetActive(false);
			}
		}
		if (list.Count == 0)
		{
			this.emptyInventoryCanvasGroup.alpha = 1f;
			this.emptyInventoryCanvasGroup.blocksRaycasts = true;
			return;
		}
		this.emptyInventoryCanvasGroup.alpha = 0f;
		this.emptyInventoryCanvasGroup.blocksRaycasts = false;
	}

	// Token: 0x06000092 RID: 146 RVA: 0x000041F4 File Offset: 0x000023F4
	private void CreateItem(Item item)
	{
		if (item.ItemType == ItemType.SuitSet)
		{
			return;
		}
		ItemIconView itemIconView = Object.Instantiate<ItemIconView>(this.itemIconViewPrefab, this.itemIconViewsParent, false);
		itemIconView.Setup(item, new Action<Item>(this.OnItemIconClicked), new Action<Item>(this.OnItemEquippedPressed), true, false, false, item.NewItem, item.IsEquipped, false, item.ItemType != ItemType.Trophy, false);
		this.inventoryItemsCreated.Add(itemIconView);
	}

	// Token: 0x06000093 RID: 147 RVA: 0x00004266 File Offset: 0x00002466
	private void OnBackButtonPressed()
	{
		this.ParentModalController.ShowPanel("Swag");
	}

	// Token: 0x06000094 RID: 148 RVA: 0x00004278 File Offset: 0x00002478
	private void OnItemIconClicked(Item selectedItem)
	{
		this.navService.CreateModal<ItemDetailModal>(NavModals.ITEM_DETAIL, false).ShowItem(selectedItem, !selectedItem.IsEquipped, delegate()
		{
			this.OnItemEquippedPressed(selectedItem);
		}, 1, false);
	}

	// Token: 0x06000095 RID: 149 RVA: 0x000042D4 File Offset: 0x000024D4
	private void OnItemEquippedPressed(Item item)
	{
		if (item.ItemType == ItemType.Badge && this.inventory.AreAllBadgeSlotsFull() && this.slotIndex == -1)
		{
			this.swapBadgeUI.SetCanvasGroupState(true);
			this.swapBadgeUI.Setup(item, this.inventory, new Action<Item>(this.OnSwapBadgePressed));
			return;
		}
		this.inventory.EquipItem(item, this.slotIndex, false);
		this.ParentModalController.ShowPanel("Swag");
	}

	// Token: 0x06000096 RID: 150 RVA: 0x00004350 File Offset: 0x00002550
	private void OnSwapBadgePressed(Item badge)
	{
		int slotIndexForItem = this.inventory.GetSlotIndexForItem(badge);
		this.inventory.EquipItem(this.swapBadgeUI.Item, slotIndexForItem, false);
		this.ParentModalController.ShowPanel("Swag");
		this.swapBadgeUI.SetCanvasGroupState(false);
	}

	// Token: 0x06000097 RID: 151 RVA: 0x000043A0 File Offset: 0x000025A0
	public void SetupViewForAllTypes()
	{
		foreach (ItemIconView itemIconView in this.inventoryItemsCreated)
		{
			itemIconView.SetExtraInfo(true, false, false, itemIconView.Item.NewItem, itemIconView.Item.IsEquipped);
			itemIconView.Item.NewItem = false;
		}
		this.slotIndex = -1;
		this.inventory.HasNewItem.Value = false;
		this.filterType = ItemType.None;
		this.backButton.gameObject.SetActive(false);
		this.resetFilterButton.gameObject.SetActive(true);
		this.viewingDropdown.gameObject.SetActive(true);
		Vector2 sizeDelta = this.sortingRectTransform.sizeDelta;
		sizeDelta.x = 300f;
		this.sortingRectTransform.sizeDelta = sizeDelta;
	}

	// Token: 0x06000098 RID: 152 RVA: 0x00004490 File Offset: 0x00002690
	public void SetupViewForType(ItemType itemType, int newSlotIndex, bool levelup, bool equip)
	{
		this.slotIndex = newSlotIndex;
		this.filterType = itemType;
		foreach (ItemIconView itemIconView in this.inventoryItemsCreated)
		{
			itemIconView.SetExtraInfo(levelup, equip, false, itemIconView.Item.NewItem, itemIconView.Item.IsEquipped);
		}
		this.viewingDropdown.value = (int)(itemType + 1);
		this.ViewingValueChanged((int)(itemType + 1));
		this.backButton.gameObject.SetActive(true);
		this.resetFilterButton.gameObject.SetActive(true);
		this.viewingDropdown.gameObject.SetActive(false);
		Vector2 sizeDelta = this.sortingRectTransform.sizeDelta;
		sizeDelta.x = 430f;
		this.sortingRectTransform.sizeDelta = sizeDelta;
	}

	// Token: 0x0400007B RID: 123
	[SerializeField]
	private ItemIconView itemIconViewPrefab;

	// Token: 0x0400007C RID: 124
	[SerializeField]
	private Transform itemIconViewsParent;

	// Token: 0x0400007D RID: 125
	[SerializeField]
	private Dropdown viewingDropdown;

	// Token: 0x0400007E RID: 126
	[SerializeField]
	private RectTransform sortingRectTransform;

	// Token: 0x0400007F RID: 127
	[SerializeField]
	private Dropdown sortingDropdown;

	// Token: 0x04000080 RID: 128
	[SerializeField]
	private Dropdown planetDropdown;

	// Token: 0x04000081 RID: 129
	[SerializeField]
	private Button backButton;

	// Token: 0x04000082 RID: 130
	[SerializeField]
	private SwapBadgeUI swapBadgeUI;

	// Token: 0x04000083 RID: 131
	[SerializeField]
	private CanvasGroup emptyInventoryCanvasGroup;

	// Token: 0x04000084 RID: 132
	[SerializeField]
	private Button goToStoreButton;

	// Token: 0x04000085 RID: 133
	[SerializeField]
	private Button resetFilterButton;

	// Token: 0x04000086 RID: 134
	[SerializeField]
	private GridLayoutGroup inventoryListGrid;

	// Token: 0x04000087 RID: 135
	private InventorySortingStruct InventorySortingStruct;

	// Token: 0x04000088 RID: 136
	private IInventoryService inventory;

	// Token: 0x04000089 RID: 137
	private readonly List<ItemIconView> inventoryItemsCreated = new List<ItemIconView>();

	// Token: 0x0400008A RID: 138
	private int slotIndex;

	// Token: 0x0400008B RID: 139
	private ItemType filterType;

	// Token: 0x0400008C RID: 140
	private NavigationService navService;
}
