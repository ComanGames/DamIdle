using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using HHTools.Navigation;
using Platforms;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000037 RID: 55
public sealed class SwagPanel : PanelBaseClass
{
	// Token: 0x0600010D RID: 269 RVA: 0x000071EC File Offset: 0x000053EC
	private void Awake()
	{
		this.Reset();
		if (this.angelPortrait)
		{
			this.angelPortrait.enabled = false;
		}
		if (this.angelLandscape)
		{
			this.angelLandscape.enabled = false;
		}
		GameController.Instance.IsInitialized.First(x => x).Subscribe(delegate(bool _)
		{
			this.Setup();
		}).AddTo(base.gameObject);
		this.btn_Mute.ForEach(delegate(Button x)
		{
			x.OnClickAsObservable().Subscribe(new Action<Unit>(this.SetMute)).AddTo(base.gameObject);
		});
		this.btn_Share.ForEach(delegate(Button x)
		{
			x.gameObject.SetActive(false);
		});
		this.btn_Kong.ForEach(delegate(Button x)
		{
			x.gameObject.SetActive(false);
		});
	}

	// Token: 0x0600010E RID: 270 RVA: 0x000072E8 File Offset: 0x000054E8
	private void Setup()
	{
		this.img_Share.ForEach(delegate(Image x)
		{
			x.sprite = this.androidShareIcon;
		});
		GameController.Instance.UnlockService.OnUnlockAchieved.Subscribe(delegate(Unlock _)
		{
			this.AddClothes();
		}).AddTo(base.gameObject);
		GameController.Instance.State.Subscribe(delegate(GameState state)
		{
			GameController.Instance.PlanetThemeService.Swag.Subscribe(delegate(SwagPrefabSetup swagSetup)
			{
				this.SetupSwagBackground(swagSetup);
				this.OnItemsLoaded();
			}).AddTo(base.gameObject);
		}).AddTo(base.gameObject);
		Observable.FromEvent(delegate(Action h)
		{
			GameController.Instance.OnSoftResetPost += h;
		}, delegate(Action h)
		{
			GameController.Instance.OnSoftResetPost -= h;
		}).Subscribe(delegate(Unit _)
		{
			this.HandleOnSoftResetPost();
		}).AddTo(base.gameObject);
		MessageBroker.Default.Receive<AngelsClaimedEvent>().Subscribe(new Action<AngelsClaimedEvent>(this.HandleOnAngelInvestmentClaim)).AddTo(base.gameObject);
		MessageBroker.Default.Receive<InventoryEquipMessage>().Subscribe(new Action<InventoryEquipMessage>(this.OnItemEquipStateChanged)).AddTo(base.gameObject);
		this.hatEquipButton.onClick.AddListener(delegate()
		{
			this.OnItemSlotClicked(ItemType.Head, 0);
		});
		this.bodyEquipButton.onClick.AddListener(delegate()
		{
			this.OnItemSlotClicked(ItemType.Shirt, 0);
		});
		this.legsEquipButton.onClick.AddListener(delegate()
		{
			this.OnItemSlotClicked(ItemType.Pants, 0);
		});
		this.badgeOneEquipButton.onClick.AddListener(delegate()
		{
			this.OnItemSlotClicked(ItemType.Badge, 0);
		});
		this.badgeTwoEquipButton.onClick.AddListener(delegate()
		{
			this.OnItemSlotClicked(ItemType.Badge, 1);
		});
		this.badgeThreeEquipButton.onClick.AddListener(delegate()
		{
			this.OnItemSlotClicked(ItemType.Badge, 2);
		});
		this.inventory = PlayerData.GetPlayerData("Global").inventory;
		this.SetInitialEquipForSlot(ItemType.Head, 0);
		this.SetInitialEquipForSlot(ItemType.Shirt, 0);
		this.SetInitialEquipForSlot(ItemType.Pants, 0);
		this.SetInitialEquipForSlot(ItemType.Badge, 0);
		this.SetInitialEquipForSlot(ItemType.Badge, 1);
		this.SetInitialEquipForSlot(ItemType.Badge, 2);
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
		GameController.Instance.IsMuted.Subscribe(new Action<bool>(this.UpdateMuted)).AddTo(base.gameObject);
	}

	// Token: 0x0600010F RID: 271 RVA: 0x00007548 File Offset: 0x00005748
	private void UpdateMuted(bool isMuted)
	{
		this.btn_Mute.ForEach(delegate(Button x)
		{
			((Image)x.targetGraphic).sprite = (isMuted ? this.sprt_muted : this.sprt_unmuted);
		});
	}

	// Token: 0x06000110 RID: 272 RVA: 0x00007580 File Offset: 0x00005780
	private void ShowKong(Unit u)
	{
		if (Application.isEditor)
		{
			Debug.LogWarning("Cannot show Kongregate Window in Editor!");
			return;
		}
		Debug.Log("MainUIController:ShowKongregateWindow");
	}

	// Token: 0x06000111 RID: 273 RVA: 0x000075DF File Offset: 0x000057DF
	private void SetMute(Unit u)
	{
		GameController.Instance.SetMute(!GameController.Instance.IsMuted.Value);
	}

	// Token: 0x06000112 RID: 274 RVA: 0x00007600 File Offset: 0x00005800
	private void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		Transform parent;
		if (orientation.IsPortrait)
		{
			this.ContentLandscape.alpha = 0f;
			this.ContentLandscape.blocksRaycasts = false;
			this.ContentPortrait.alpha = 1f;
			this.ContentPortrait.blocksRaycasts = true;
			parent = this.tform_SuitSlotsPortrait;
		}
		else
		{
			this.ContentLandscape.alpha = 1f;
			this.ContentLandscape.blocksRaycasts = true;
			this.ContentPortrait.alpha = 0f;
			this.ContentPortrait.blocksRaycasts = false;
			parent = this.tform_SuitSlotsLandscape;
		}
		this.hatEquipButton.transform.SetParent(parent);
		this.bodyEquipButton.transform.SetParent(parent);
		this.legsEquipButton.transform.SetParent(parent);
		this.badgeOneEquipButton.transform.SetParent(parent);
		this.badgeTwoEquipButton.transform.SetParent(parent);
		this.badgeThreeEquipButton.transform.SetParent(parent);
	}

	// Token: 0x06000113 RID: 275 RVA: 0x000076FC File Offset: 0x000058FC
	private void SetInitialEquipForSlot(ItemType itemType, int slotIndex)
	{
		InventoryEquipMessage inventoryEquipMessage = new InventoryEquipMessage
		{
			item = this.inventory.GetEquippedItemForSlot(itemType, slotIndex),
			slotIndex = slotIndex
		};
		inventoryEquipMessage.equipped = (inventoryEquipMessage.item != null);
		this.OnItemEquipStateChanged(inventoryEquipMessage);
	}

	// Token: 0x06000114 RID: 276 RVA: 0x00007748 File Offset: 0x00005948
	private void OnItemSlotClicked(ItemType slotType, int slotIndex)
	{
		Item equippedItemForSlot = this.inventory.GetEquippedItemForSlot(slotType, slotIndex);
		if (equippedItemForSlot == null || equippedItemForSlot.IsNudeEquipment)
		{
			this.ShowPlayerInventory(slotType, slotIndex);
			return;
		}
		GameController.Instance.NavigationService.CreateModal<ItemDetailModal>(NavModals.ITEM_DETAIL, false).ShowItem(equippedItemForSlot, true, delegate()
		{
			this.ShowPlayerInventory(slotType, slotIndex);
		}, 1, false);
	}

	// Token: 0x06000115 RID: 277 RVA: 0x000077D0 File Offset: 0x000059D0
	private void ShowPlayerInventory(ItemType slotType, int slotIndex)
	{
		this.ParentModalController.ShowPanel("Inventory");
		((CareerModalController)this.ParentModalController).ShowPlayerInventory(slotType, slotIndex);
	}

	// Token: 0x06000116 RID: 278 RVA: 0x000077F4 File Offset: 0x000059F4
	private void OnItemEquipStateChanged(InventoryEquipMessage inventoryEquipMessage)
	{
		if (inventoryEquipMessage.item == null)
		{
			return;
		}
		Transform slotButtonTransform = this.GetSlotButtonTransform(inventoryEquipMessage.item.ItemType, inventoryEquipMessage.slotIndex);
		if (slotButtonTransform == null)
		{
			return;
		}
		if (!inventoryEquipMessage.item.IsNudeEquipment)
		{
			if (inventoryEquipMessage.equipped)
			{
				ItemIconView itemIconView = Object.Instantiate<ItemIconView>(this.itemIconViewPrefab);
				itemIconView.Setup(inventoryEquipMessage.item, null, null, true, false, false, false, false, false, true, false);
				itemIconView.transform.SetParent(slotButtonTransform, false);
				return;
			}
			Object.Destroy(slotButtonTransform.GetChild(slotButtonTransform.childCount - 1).gameObject);
		}
	}

	// Token: 0x06000117 RID: 279 RVA: 0x00007888 File Offset: 0x00005A88
	private Transform GetSlotButtonTransform(ItemType itemType, int slotIndex)
	{
		switch (itemType)
		{
		case ItemType.Head:
			return this.hatEquipButton.transform;
		case ItemType.Shirt:
			return this.bodyEquipButton.transform;
		case ItemType.Pants:
			return this.legsEquipButton.transform;
		case ItemType.Badge:
			switch (slotIndex)
			{
			case 0:
				return this.badgeOneEquipButton.transform;
			case 1:
				return this.badgeTwoEquipButton.transform;
			case 2:
				return this.badgeThreeEquipButton.transform;
			}
			break;
		}
		return null;
	}

	// Token: 0x06000118 RID: 280 RVA: 0x0000790C File Offset: 0x00005B0C
	private void SetupSwagBackground(SwagPrefabSetup prefabSetup)
	{
		SwagItemsViewSetup component = Object.Instantiate<GameObject>(prefabSetup.prefabPortrait, this.swagViewParentPortrait, false).GetComponent<SwagItemsViewSetup>();
		this.angelPortrait = component.angel;
		this.levelsPortrait = component.levels;
		component = Object.Instantiate<GameObject>(prefabSetup.prefabLandscape, this.swagViewParentLandscape, false).GetComponent<SwagItemsViewSetup>();
		this.angelLandscape = component.angel;
		this.levelsLandscape = component.levels;
	}

	// Token: 0x06000119 RID: 281 RVA: 0x00007979 File Offset: 0x00005B79
	private void HandleOnSoftResetPost()
	{
		this.Reset();
		this.AddClothes();
		if (this.angelPortrait)
		{
			this.angelPortrait.enabled = true;
		}
		if (this.angelLandscape)
		{
			this.angelLandscape.enabled = true;
		}
	}

	// Token: 0x0600011A RID: 282 RVA: 0x000079B9 File Offset: 0x00005BB9
	private void HandleOnAngelInvestmentClaim(AngelsClaimedEvent evt)
	{
		if (evt.AngelAmount > 0.0)
		{
			this.angelPortrait.enabled = true;
			this.angelLandscape.enabled = true;
		}
	}

	// Token: 0x0600011B RID: 283 RVA: 0x000079E4 File Offset: 0x00005BE4
	private void OnShowModal()
	{
		if (NotificationController.instance != null)
		{
			NotificationController.instance.swagDotNotification.View();
		}
	}

	// Token: 0x0600011C RID: 284 RVA: 0x00007A04 File Offset: 0x00005C04
	private void GetSwagAchievementNumbers()
	{
		List<Unlock> unlocks = GameController.Instance.UnlockService.Unlocks;
		int i;
		int j;
		for (i = 0; i < this.levelsPortrait.Count; i = j + 1)
		{
			this.levelsPortrait[i].achievementActivator = unlocks.FindIndex(a => a.name == this.levelsPortrait[i].achievementName);
			this.levelsLandscape[i].achievementActivator = unlocks.FindIndex(a => a.name == this.levelsLandscape[i].achievementName);
			j = i;
		}
	}

	// Token: 0x0600011D RID: 285 RVA: 0x00007AAC File Offset: 0x00005CAC
	private void AddClothes()
	{
		for (int i = 0; i < this.levelsPortrait.Count; i++)
		{
			SwagLevel swagLevel = this.levelsPortrait[i];
			if (swagLevel.achievementActivator == -1)
			{
				Debug.LogWarningFormat("Swag item [{0}] could not find unlock [{1}]", new object[]
				{
					swagLevel.name,
					swagLevel.achievementName
				});
			}
			else if (GameController.Instance.UnlockService.Unlocks[swagLevel.achievementActivator].Earned.Value)
			{
				swagLevel.ToggleImages(true);
				this.levelsLandscape[i].ToggleImages(true);
			}
		}
	}

	// Token: 0x0600011E RID: 286 RVA: 0x00007B4C File Offset: 0x00005D4C
	public void Reset()
	{
		for (int i = 0; i < this.levelsPortrait.Count; i++)
		{
			this.levelsPortrait[i].ToggleImages(false);
			this.levelsLandscape[i].ToggleImages(false);
		}
	}

	// Token: 0x0600011F RID: 287 RVA: 0x00007B94 File Offset: 0x00005D94
	public void OnItemsLoaded()
	{
		this.GetSwagAchievementNumbers();
		if (GameController.Instance.AngelService.AngelsOnHand.Value > 0.0)
		{
			if (this.angelPortrait)
			{
				this.angelPortrait.enabled = true;
			}
			if (this.angelLandscape)
			{
				this.angelLandscape.enabled = true;
			}
		}
		this.AddClothes();
		if (GameController.Instance.game.planetPlayerData.GetDouble("Flux Capacitor", 0.0) > 0.0)
		{
			this.ShowTimeCarIfUnlocked();
			return;
		}
		this.storeDisposable = (from x in MessageBroker.Default.Receive<StorePurchaseEvent>()
		where x.PurchaseState == EStorePurchaseState.Success
		select x).Subscribe(new Action<StorePurchaseEvent>(this.OnAdCapPurchaseEvent)).AddTo(base.gameObject);
	}

	// Token: 0x06000120 RID: 288 RVA: 0x00007C84 File Offset: 0x00005E84
	private void OnAdCapPurchaseEvent(StorePurchaseEvent evt)
	{
		for (int i = 0; i < evt.Item.Rewards.Count; i++)
		{
			if (this.inventory.GetItemById(evt.Item.Rewards[i].Id).Product == Product.FluxCapitalor)
			{
				this.ShowTimeCarIfUnlocked();
				this.storeDisposable.Dispose();
				return;
			}
		}
	}

	// Token: 0x06000121 RID: 289 RVA: 0x00007CE8 File Offset: 0x00005EE8
	private void ShowTimeCarIfUnlocked()
	{
		bool enabled = GameController.Instance.game.planetPlayerData.GetDouble("Flux Capacitor", 0.0) > 0.0;
		SwagLevel swagLevel = this.levelsPortrait.FirstOrDefault(l => l.name == "Time Car");
		if (swagLevel != null)
		{
			swagLevel.images[0].enabled = enabled;
		}
		SwagLevel swagLevel2 = this.levelsLandscape.FirstOrDefault(l => l.name == "Time Car");
		if (swagLevel2 != null)
		{
			swagLevel2.images[0].enabled = enabled;
		}
	}

	// Token: 0x06000122 RID: 290 RVA: 0x00007DA4 File Offset: 0x00005FA4
	public bool DoesAchievementHaveSwag(Unlock achievement)
	{
		return achievement != null && this.levelsPortrait.Any(level => level.achievementName == achievement.name);
	}

	// Token: 0x06000123 RID: 291 RVA: 0x00007DE0 File Offset: 0x00005FE0
	public void ShowLeaderboard()
	{
		PlatformType platformType = Helper.GetPlatformType();
		if (platformType - PlatformType.Android <= 1)
		{
			Social.ShowLeaderboardUI();
			return;
		}
		Debug.LogWarningFormat("Unable to show leaderboards for platform [{0}]", new object[]
		{
			Helper.GetPlatformType()
		});
	}

	// Token: 0x06000124 RID: 292 RVA: 0x00007E1C File Offset: 0x0000601C
	public void ShowAchievements()
	{
		PlatformType platformType = Helper.GetPlatformType();
		if (platformType - PlatformType.Android <= 1)
		{
			Social.ShowAchievementsUI();
			return;
		}
		Debug.LogWarningFormat("Unable to show achievements for platform [{0}]", new object[]
		{
			Helper.GetPlatformType()
		});
	}

	// Token: 0x06000125 RID: 293 RVA: 0x00007E58 File Offset: 0x00006058
	public void LogoutGooglePlay()
	{
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Google Play Logout", "You have been logged out of Google Play Games.", delegate()
		{
		}, PopupModal.PopupOptions.OK, "Alright", "No", true, null, "");
	}

	// Token: 0x06000126 RID: 294 RVA: 0x00007EBA File Offset: 0x000060BA
	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	// Token: 0x04000145 RID: 325
	private IInventoryService inventory;

	// Token: 0x04000146 RID: 326
	private Image angelPortrait;

	// Token: 0x04000147 RID: 327
	private Image angelLandscape;

	// Token: 0x04000148 RID: 328
	[SerializeField]
	private List<Button> btn_Kong;

	// Token: 0x04000149 RID: 329
	[SerializeField]
	private List<Button> btn_Mute;

	// Token: 0x0400014A RID: 330
	[SerializeField]
	private Sprite sprt_unmuted;

	// Token: 0x0400014B RID: 331
	[SerializeField]
	private Sprite sprt_muted;

	// Token: 0x0400014C RID: 332
	[SerializeField]
	private Transform swagViewParentPortrait;

	// Token: 0x0400014D RID: 333
	[SerializeField]
	private Transform swagViewParentLandscape;

	// Token: 0x0400014E RID: 334
	[SerializeField]
	private List<Button> btn_Share;

	// Token: 0x0400014F RID: 335
	[SerializeField]
	private List<Image> img_Share;

	// Token: 0x04000150 RID: 336
	[SerializeField]
	private Sprite twitterShareIcon;

	// Token: 0x04000151 RID: 337
	[SerializeField]
	private Sprite androidShareIcon;

	// Token: 0x04000152 RID: 338
	[SerializeField]
	private Button hatEquipButton;

	// Token: 0x04000153 RID: 339
	[SerializeField]
	private Button bodyEquipButton;

	// Token: 0x04000154 RID: 340
	[SerializeField]
	private Button legsEquipButton;

	// Token: 0x04000155 RID: 341
	[SerializeField]
	private Button badgeOneEquipButton;

	// Token: 0x04000156 RID: 342
	[SerializeField]
	private Button badgeTwoEquipButton;

	// Token: 0x04000157 RID: 343
	[SerializeField]
	private Button badgeThreeEquipButton;

	// Token: 0x04000158 RID: 344
	[SerializeField]
	private ItemIconView itemIconViewPrefab;

	// Token: 0x04000159 RID: 345
	[SerializeField]
	private Transform tform_SuitSlotsPortrait;

	// Token: 0x0400015A RID: 346
	[SerializeField]
	private Transform tform_SuitSlotsLandscape;

	// Token: 0x0400015B RID: 347
	[SerializeField]
	private CanvasGroup ContentPortrait;

	// Token: 0x0400015C RID: 348
	[SerializeField]
	private CanvasGroup ContentLandscape;

	// Token: 0x0400015D RID: 349
	[HideInInspector]
	public List<SwagLevel> levelsPortrait;

	// Token: 0x0400015E RID: 350
	[HideInInspector]
	public List<SwagLevel> levelsLandscape;

	// Token: 0x0400015F RID: 351
	private IDisposable storeDisposable = Disposable.Empty;
}
