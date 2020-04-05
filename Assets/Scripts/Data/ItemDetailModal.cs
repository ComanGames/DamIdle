using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AdCap.Store;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x02000033 RID: 51
public class ItemDetailModal : AnimatedModal, IBeginDragHandler, IEventSystemHandler, IEndDragHandler
{
	// Token: 0x060000DB RID: 219 RVA: 0x00005D50 File Offset: 0x00003F50
	protected override void Awake()
	{
		this.inventory = PlayerData.GetPlayerData("Global").inventory;
		this.nextItemButton.OnClickAsObservable().Subscribe(delegate(Unit __)
		{
			this.GoToNextItem();
		}).AddTo(base.gameObject);
		this.previousItemButton.OnClickAsObservable().Subscribe(delegate(Unit __)
		{
			this.GoToPreviousItem();
		}).AddTo(base.gameObject);
		this.closeButton.OnClickAsObservable().Subscribe(new Action<Unit>(this.CloseModal)).AddTo(base.gameObject);
		this.navigationItemsCanvasGroup.alpha = 0f;
		base.Awake();
	}

	// Token: 0x060000DC RID: 220 RVA: 0x00005E04 File Offset: 0x00004004
	public void ShowItem(Item item, bool showButtons, Action onEquipCallback, int qty = 1, bool showBuy = false)
	{
		this.OnEquipedCallback = onEquipCallback;
		this.createdItemDetails.ForEach(delegate(ItemDetailView x)
		{
			Object.Destroy(x.gameObject);
		});
		this.createdItemDetails.Clear();
		this.currentElement = 0;
		this.purchaseDetails.gameObject.SetActive(false);
		if (item.ItemType == ItemType.SuitSet)
		{
			List<Item> suitItemsForId = this.inventory.GetSuitItemsForId(item.ItemId);
			this.bottomItemDetails.SetActive(true);
			this.ShowItems(suitItemsForId, qty, null, false, null);
		}
		else
		{
			if (item.ItemType == ItemType.Currency)
			{
				item.Owned.Value = qty;
			}
			this.CreateItemDetailView(item, 0, showButtons);
		}
		this.Wait1().ToObservable(false).StartAsCoroutine(default(CancellationToken));
	}

	// Token: 0x060000DD RID: 221 RVA: 0x00005ED4 File Offset: 0x000040D4
	public void ShowItem(AdCapStoreItem storeItem)
	{
		if (storeItem != null)
		{
			this.isStoreItem = true;
			if (storeItem.Rewards.Count > 1)
			{
				for (int i = 0; i < storeItem.Rewards.Count; i++)
				{
					RewardData rewardData = storeItem.Rewards[i];
					Item itemById = this.inventory.GetItemById(rewardData.Id);
					if (itemById.ItemType != ItemType.SuitSet)
					{
						this.CreateItemDetailView(itemById, rewardData.Qty, false);
					}
					else
					{
						List<Item> suitItemsForId = GameController.Instance.GlobalPlayerData.inventory.GetSuitItemsForId(itemById.ItemId);
						this.ShowItems(suitItemsForId, rewardData.Qty, null, false, null);
					}
				}
			}
			else
			{
				Item itemById2 = this.inventory.GetItemById(storeItem.Rewards[0].Id);
				if (itemById2.ItemType == ItemType.SuitSet)
				{
					List<Item> suitItemsForId2 = GameController.Instance.GlobalPlayerData.inventory.GetSuitItemsForId(itemById2.ItemId);
					this.ShowItems(suitItemsForId2, storeItem.Rewards[0].Qty, null, false, null);
				}
				else
				{
					this.CreateItemDetailView(itemById2, storeItem.Rewards[0].Qty, false);
				}
			}
			this.purchaseDetails.gameObject.SetActive(true);
			this.txt_ItemName.text = storeItem.DisplayName;
			Action<Unit> <>9__2;
			foreach (KeyValuePair<string, GameObject> keyValuePair in this.purchaseButtons)
			{
				if (keyValuePair.Key == storeItem.Cost.Currency.ToString())
				{
					keyValuePair.Value.SetActive(true);
					string currentCost = storeItem.Cost.GetCurrentCost();
					keyValuePair.Value.GetComponentInChildren<Text>().text = currentCost;
					Button componentInChildren = keyValuePair.Value.GetComponentInChildren<Button>();
					IObservable<Unit> source = componentInChildren.OnClickAsObservable();
					Action<Unit> onNext;
					if ((onNext = <>9__2) == null)
					{
						onNext = (<>9__2 = delegate(Unit _)
						{
							this.OnItemPurchaseClicked(storeItem);
						});
					}
					source.Subscribe(onNext).AddTo(base.gameObject);
					if (currentCost == "Loading")
					{
						componentInChildren.interactable = false;
					}
				}
				else
				{
					keyValuePair.Value.SetActive(false);
				}
			}
			this.purchaseDetails.gameObject.SetActive(true);
			this.bottomItemDetails.SetActive(false);
			this.SendStoreAnalytics(storeItem.Id, "info");
		}
		else
		{
			Debug.LogErrorFormat("null store item in [ItemDetialView]", Array.Empty<object>());
		}
		this.Wait1().ToObservable(false).StartAsCoroutine(default(CancellationToken));
		(from x in GameController.Instance.StoreService.CurrentCatalog.ObserveRemove()
		where storeItem.Id == x.Value.Id
		select x).Subscribe(delegate(CollectionRemoveEvent<AdCapStoreItem> _)
		{
			this.CloseModal(Unit.Default);
		}).AddTo(base.gameObject);
	}

	// Token: 0x060000DE RID: 222 RVA: 0x00006210 File Offset: 0x00004410
	public void SendStoreAnalytics(string bundleId, string action)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics(bundleId, GameController.Instance.game.planetName + "_ItemDetail", action);
	}

	// Token: 0x060000DF RID: 223 RVA: 0x0000623C File Offset: 0x0000443C
	private void OnItemPurchaseClicked(AdCapStoreItem storeItem)
	{
		this.SendStoreAnalytics(storeItem.Id, "buy");
		(from evt in MessageBroker.Default.Receive<StorePurchaseEvent>()
		where evt.PurchaseState == EStorePurchaseState.Success
		select evt).Subscribe(delegate(StorePurchaseEvent _)
		{
			this.CloseModal(Unit.Default);
		}).AddTo(base.gameObject);
		GameController.Instance.StoreService.AttemptPurchase(storeItem, storeItem.Cost.Currency);
	}

	// Token: 0x060000E0 RID: 224 RVA: 0x000062C4 File Offset: 0x000044C4
	private void ShowItems(List<Item> itemList, int addedLevels, Item selectedItem, bool showButtons, Action onEquipCallback)
	{
		this.currentElement = ((selectedItem == null) ? 0 : itemList.IndexOf(selectedItem));
		for (int i = 0; i < itemList.Count; i++)
		{
			this.CreateItemDetailView(itemList[i], addedLevels, showButtons);
		}
	}

	// Token: 0x060000E1 RID: 225 RVA: 0x00006305 File Offset: 0x00004505
	public void OnBeginDrag(PointerEventData data)
	{
		this.TweenOutNavigationItems();
		this.itemDetailViewParent.DOKill(false);
	}

	// Token: 0x060000E2 RID: 226 RVA: 0x0000631A File Offset: 0x0000451A
	public void OnEndDrag(PointerEventData eventData)
	{
		if (Mathf.Abs(this.scrollArea.velocity.x) > this.minSnapVelocityThreshold)
		{
			base.StartCoroutine(this.WaitForVelocityToReduce());
			return;
		}
		this.SnapToClosest();
	}

	// Token: 0x060000E3 RID: 227 RVA: 0x00006350 File Offset: 0x00004550
	protected override void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		if (orientation.IsPortrait)
		{
			this.horizontalLayoutGroup.spacing = 25f;
			this.closeButton.transform.localPosition = new Vector3(313f, 290f, 0f);
		}
		else
		{
			this.horizontalLayoutGroup.spacing = 250f;
			this.closeButton.transform.localPosition = new Vector3(313f, 255f, 0f);
		}
		this.TweenItemDetailsToIndex(this.currentElement, true);
	}

	// Token: 0x060000E4 RID: 228 RVA: 0x000063DC File Offset: 0x000045DC
	private void OnEquipPressed()
	{
		this.OnEquipedCallback();
		this.CloseModal(Unit.Default);
	}

	// Token: 0x060000E5 RID: 229 RVA: 0x000063F4 File Offset: 0x000045F4
	private void OnUnequipPressed(ItemDetailView itemDetailView)
	{
		this.inventory.UnequipItem(itemDetailView.Item);
		itemDetailView.SetEquippedState(false);
		this.CloseModal(Unit.Default);
	}

	// Token: 0x060000E6 RID: 230 RVA: 0x00006419 File Offset: 0x00004619
	private void GoToNextItem()
	{
		this.TweenOutNavigationItems();
		if (this.currentElement < this.createdItemDetails.Count - 1)
		{
			this.TweenItemDetailsToIndex(this.currentElement + 1, false);
		}
	}

	// Token: 0x060000E7 RID: 231 RVA: 0x00006445 File Offset: 0x00004645
	private void GoToPreviousItem()
	{
		this.TweenOutNavigationItems();
		if (this.currentElement > 0)
		{
			this.TweenItemDetailsToIndex(this.currentElement - 1, false);
		}
	}

	// Token: 0x060000E8 RID: 232 RVA: 0x00006468 File Offset: 0x00004668
	private void SnapToClosest()
	{
		float width = ((RectTransform)this.createdItemDetails[0].transform).rect.width;
		float num = (float)(OrientationController.Instance.CurrentOrientation.IsPortrait ? 25 : 250);
		int num2 = (int)Math.Floor((double)((this.itemDetailViewParent.anchoredPosition.x + width / 2f - num / 2f) / (width + num))) * -1;
		num2--;
		if (num2 < 0)
		{
			num2 = 0;
		}
		else if (num2 > this.createdItemDetails.Count - 1)
		{
			num2 = this.createdItemDetails.Count - 1;
		}
		this.SnapToIndex(num2);
	}

	// Token: 0x060000E9 RID: 233 RVA: 0x00006514 File Offset: 0x00004714
	private IEnumerator WaitForVelocityToReduce()
	{
		while (Mathf.Abs(this.scrollArea.velocity.x) > this.minSnapVelocityThreshold)
		{
			yield return null;
		}
		this.SnapToClosest();
		yield break;
	}

	// Token: 0x060000EA RID: 234 RVA: 0x00006523 File Offset: 0x00004723
	private void SnapToIndex(int index)
	{
		this.scrollArea.StopMovement();
		this.TweenItemDetailsToIndex(index, false);
	}

	// Token: 0x060000EB RID: 235 RVA: 0x00006538 File Offset: 0x00004738
	private void TweenOutNavigationItems()
	{
		this.nextItemButton.interactable = false;
		this.previousItemButton.interactable = false;
		this.closeButton.interactable = false;
		this.navigationItemsCanvasGroup.DOFade(0f, 0.1f);
	}

	// Token: 0x060000EC RID: 236 RVA: 0x00006574 File Offset: 0x00004774
	private void TweenInNaviagiontItems()
	{
		this.navigationItemsCanvasGroup.DOFade(1f, 0.1f).onComplete = new TweenCallback(this.OnTweenInFinished);
	}

	// Token: 0x060000ED RID: 237 RVA: 0x0000659C File Offset: 0x0000479C
	private void TweenItemDetailsToIndex(int targetIndex, bool jumpto = false)
	{
		this.itemDetailViewParent.DOKill(false);
		if (this.createdItemDetails.Count == 0)
		{
			return;
		}
		float width = ((RectTransform)this.createdItemDetails[0].transform).rect.width;
		float num = (float)(OrientationController.Instance.CurrentOrientation.IsPortrait ? 25 : 250);
		float num2 = (float)targetIndex * width + num * (float)targetIndex + width / 2f;
		num2 += (float)this.itemDetailViewParent.GetComponent<HorizontalLayoutGroup>().padding.left;
		if (jumpto)
		{
			Vector3 v = this.itemDetailViewParent.anchoredPosition;
			v.x = -num2;
			this.itemDetailViewParent.anchoredPosition = v;
			this.OnMoveTweenFinished();
		}
		else
		{
			this.itemDetailViewParent.DOAnchorPosX(-num2, 0.15f, false).onComplete = new TweenCallback(this.OnMoveTweenFinished);
		}
		this.currentElement = targetIndex;
	}

	// Token: 0x060000EE RID: 238 RVA: 0x00006694 File Offset: 0x00004894
	private void OnMoveTweenFinished()
	{
		string itemSetId = this.createdItemDetails[this.currentElement].Item.ItemSetId;
		if (!string.IsNullOrEmpty(itemSetId))
		{
			Item itemById = this.inventory.GetItemById(itemSetId);
			int itemCountInSet = this.inventory.GetItemCountInSet(itemById.ItemId);
			int equippedItemsInSet = this.inventory.GetEquippedItemsInSet(itemById.ItemId);
			string text = (itemCountInSet == equippedItemsInSet) ? "#8fb858" : "#E25252ff";
			this.piecesEquippedText.text = string.Format("<color={0}>{1}/{2}</color> {3} pieces equipped.", new object[]
			{
				text,
				equippedItemsInSet,
				itemCountInSet,
				itemById.ItemName
			});
			this.itemSetText.text = ((itemCountInSet == equippedItemsInSet) ? string.Format("<color={0}> {1}</color>", "#8fb858", itemById.GetFilledItemDescription(0, "")) : itemById.GetFilledItemDescription(0, ""));
			this.bottomItemDetails.SetActive(!this.isStoreItem);
		}
		else
		{
			this.bottomItemDetails.SetActive(false);
		}
		this.previousItemButton.gameObject.SetActive(this.currentElement > 0);
		this.nextItemButton.gameObject.SetActive(this.currentElement < this.createdItemDetails.Count - 1);
		this.TweenInNaviagiontItems();
	}

	// Token: 0x060000EF RID: 239 RVA: 0x000067E4 File Offset: 0x000049E4
	private void OnTweenInFinished()
	{
		this.nextItemButton.interactable = true;
		this.previousItemButton.interactable = true;
		this.closeButton.interactable = true;
	}

	// Token: 0x060000F0 RID: 240 RVA: 0x0000680A File Offset: 0x00004A0A
	private IEnumerator Wait1()
	{
		yield return null;
		this.TweenItemDetailsToIndex(this.currentElement, true);
		yield break;
	}

	// Token: 0x060000F1 RID: 241 RVA: 0x0000681C File Offset: 0x00004A1C
	private void CreateItemDetailView(Item item, int addedLevels, bool showButtons = true)
	{
		ItemDetailView itemDetailView = Object.Instantiate<ItemDetailView>(this.itemDetailViewPrefab, this.itemDetailViewParent, false);
		itemDetailView.Setup(item, addedLevels, new Action(this.OnEquipPressed), delegate
		{
			this.OnUnequipPressed(itemDetailView);
		}, showButtons);
		this.createdItemDetails.Add(itemDetailView);
	}

	// Token: 0x04000108 RID: 264
	private const float MOVE_TWEEN_ANIMATION_TIME = 0.15f;

	// Token: 0x04000109 RID: 265
	private const int LANDSCAPE_SPACING = 250;

	// Token: 0x0400010A RID: 266
	private const int PORTRAIT_SPACING = 25;

	// Token: 0x0400010B RID: 267
	[SerializeField]
	private ItemDetailView itemDetailViewPrefab;

	// Token: 0x0400010C RID: 268
	[SerializeField]
	private RectTransform itemDetailViewParent;

	// Token: 0x0400010D RID: 269
	[SerializeField]
	private HorizontalLayoutGroup horizontalLayoutGroup;

	// Token: 0x0400010E RID: 270
	[SerializeField]
	private Button nextItemButton;

	// Token: 0x0400010F RID: 271
	[SerializeField]
	private Button previousItemButton;

	// Token: 0x04000110 RID: 272
	[SerializeField]
	private Button closeButton;

	// Token: 0x04000111 RID: 273
	[SerializeField]
	private GameObject bottomItemDetails;

	// Token: 0x04000112 RID: 274
	[SerializeField]
	private Text piecesEquippedText;

	// Token: 0x04000113 RID: 275
	[SerializeField]
	private Text itemSetText;

	// Token: 0x04000114 RID: 276
	[SerializeField]
	private CanvasGroup navigationItemsCanvasGroup;

	// Token: 0x04000115 RID: 277
	[SerializeField]
	private ScrollRect scrollArea;

	// Token: 0x04000116 RID: 278
	[SerializeField]
	private float minSnapVelocityThreshold = 1f;

	// Token: 0x04000117 RID: 279
	[SerializeField]
	private GameObject purchaseDetails;

	// Token: 0x04000118 RID: 280
	[SerializeField]
	private Text txt_ItemName;

	// Token: 0x04000119 RID: 281
	[SerializeField]
	private StringGameObjectDictionary purchaseButtons;

	// Token: 0x0400011A RID: 282
	private Action OnEquipedCallback;

	// Token: 0x0400011B RID: 283
	private bool isStoreItem;

	// Token: 0x0400011C RID: 284
	private IInventoryService inventory;

	// Token: 0x0400011D RID: 285
	private readonly List<ItemDetailView> createdItemDetails = new List<ItemDetailView>();

	// Token: 0x0400011E RID: 286
	private int currentElement;
}
