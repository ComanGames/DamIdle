using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Redemption;
using AdCap.Store;
using Facebook.Unity;
using Kongregate;
using Platforms;
using Platforms.Logger;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EventsModels;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;

// Token: 0x02000047 RID: 71
public class KongAnalyticService : IAnalyticService, IDisposable
{
	// Token: 0x1700001C RID: 28
	// (get) Token: 0x060001B2 RID: 434 RVA: 0x00009D10 File Offset: 0x00007F10
	// (set) Token: 0x060001B3 RID: 435 RVA: 0x00002718 File Offset: 0x00000918
	public bool SendAnalyticsToPlayfab
	{
		get
		{
			return this.sendAnalyticsToPlayfab;
		}
		set
		{
		}
	}

	// Token: 0x060001B4 RID: 436 RVA: 0x00009D18 File Offset: 0x00007F18
	public void Init(IGameController gameController)
	{
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		this.gameController = gameController;
	}

	// Token: 0x060001B5 RID: 437 RVA: 0x00009D2D File Offset: 0x00007F2D
	public void Dispose()
	{
		this.disposables.Dispose();
		this.stateDisposables.Dispose();
	}

	// Token: 0x060001B6 RID: 438 RVA: 0x00009D48 File Offset: 0x00007F48
	public void SendKongEconomyTransaction(KongEconomyTransactionData kongEconomyTransactionData)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"type",
				kongEconomyTransactionData.economyTransactionType
			},
			{
				"resources_summary",
				kongEconomyTransactionData.resourcesSummary
			},
			{
				"context_of_offer",
				this.gameController.game.planetName
			},
			{
				"hard_currency_change",
				kongEconomyTransactionData.goldChangeAmount
			},
			{
				"soft_currency_5_change",
				kongEconomyTransactionData.megabucksChangeAmount
			},
			{
				"soft_currency_6_change",
				kongEconomyTransactionData.megaTicketsChangeAmount
			}
		};
		this.SendAnalyticEvent("economy_transactions", evt, false, "");
	}

	// Token: 0x060001B7 RID: 439 RVA: 0x00009DEC File Offset: 0x00007FEC
	public void OnVentureUnlockedOnFirstPurchase(VentureModel model)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"task_id",
				"Venture_Unlock"
			},
			{
				"task_type",
				this.gameController.game.planetName
			},
			{
				"task_description",
				model.Id + ":" + this.gameController.AngelService.AngelResetCount.Value
			}
		};
		this.SendAnalyticEvent("task_completes", evt, false, "");
	}

	// Token: 0x060001B8 RID: 440 RVA: 0x00009E74 File Offset: 0x00008074
	public void OnOfferwallGoldRewarded(int goldAmountAwarded)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"reward",
				goldAmountAwarded
			},
			{
				"ad_type",
				"offerwall"
			}
		};
		this.SendAnalyticEvent("ad_end", evt, false, "");
		this.SendKongEconomyTransaction(new KongEconomyTransactionData("offerwall", "Offer Completed", this.gameController.game.planetName, goldAmountAwarded, 0, 0));
	}

	// Token: 0x060001B9 RID: 441 RVA: 0x00009EE4 File Offset: 0x000080E4
	public void OnAppOnBoardWatched()
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"ad_type",
				"apponboard"
			}
		};
		this.SendAnalyticEvent("ad_end", evt, false, "");
	}

	// Token: 0x060001BA RID: 442 RVA: 0x00009F1C File Offset: 0x0000811C
	public void SendAdStartEvent(string origin, string adType, string contextOfOffer)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"is_optional",
				true
			},
			{
				"ad_type",
				adType
			},
			{
				"origin",
				origin
			},
			{
				"context_of_offer",
				contextOfOffer
			}
		};
		this.SendAnalyticEvent("ad_start", evt, false, "");
	}

	// Token: 0x060001BB RID: 443 RVA: 0x00009F78 File Offset: 0x00008178
	public void SendAdFinished(string origin, string adType, string contextOfOffer, string reward = "")
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"reward",
				reward
			},
			{
				"ad_type",
				adType
			},
			{
				"origin",
				origin
			},
			{
				"context_of_offer",
				contextOfOffer
			}
		};
		this.SendAnalyticEvent("ad_end", evt, false, "");
	}

	// Token: 0x060001BC RID: 444 RVA: 0x00009FCE File Offset: 0x000081CE
	public void SendEarnedCurrencyEvent()
	{
		if (!this.gameController.GlobalPlayerData.Has(KongAnalyticService.ANALYTIC_EARNED_GOLD_KEY))
		{
			this.SendAdjustAnalyticEvent(KongregateGameObject.ADJUST_KONG_FIRST_HC_KEY, null);
			this.gameController.GlobalPlayerData.Set(KongAnalyticService.ANALYTIC_EARNED_GOLD_KEY, "true");
		}
	}

	// Token: 0x060001BD RID: 445 RVA: 0x0000A010 File Offset: 0x00008210
	public void SendTaskCompleteEvent(string taskId = "", string taskType = "", string taskDescription = "")
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"task_id",
				taskId
			},
			{
				"task_type",
				taskType
			},
			{
				"task_description",
				taskDescription
			}
		};
		this.SendAnalyticEvent("task_completes", evt, false, "");
	}

	// Token: 0x060001BE RID: 446 RVA: 0x0000A05C File Offset: 0x0000825C
	public void SendSwagAcquired(string swagName, int previousLevel, int addedLevels, int rarity)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"type",
				"Item_Upgraded"
			},
			{
				"resources_summary",
				swagName
			},
			{
				"context_of_offer",
				this.gameController.game.planetName
			},
			{
				"level_before",
				previousLevel
			},
			{
				"added_levels",
				addedLevels
			},
			{
				"current_level",
				previousLevel + addedLevels
			},
			{
				"rarity",
				rarity
			}
		};
		this.SendAnalyticEvent("economy_transactions", evt, false, "");
	}

	// Token: 0x060001BF RID: 447 RVA: 0x0000A100 File Offset: 0x00008300
	public void SendAwardMegaBucksKongAnalytic(double amount, string unlockName)
	{
		string planetName = this.gameController.game.planetName;
		string collection = "economy_transactions";
		string value = "MegaBuck_Buff";
		string value2 = planetName;
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"type",
				value
			},
			{
				"resources_summary",
				unlockName
			},
			{
				"context_of_offer",
				value2
			},
			{
				"soft_currency_5_change",
				amount
			}
		};
		this.SendAnalyticEvent(collection, evt, true, unlockName);
	}

	// Token: 0x060001C0 RID: 448 RVA: 0x0000A174 File Offset: 0x00008374
	public void SendNavActionAnalytics(string navElementName, string origin, string result)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"nav_element_name",
				navElementName
			},
			{
				"origin",
				origin
			},
			{
				"result",
				result
			}
		};
		this.SendAnalyticEvent("nav_actions", evt, false, "");
	}

	// Token: 0x060001C1 RID: 449 RVA: 0x0000A1C0 File Offset: 0x000083C0
	public void SendVentureBoostedAnalytic(VentureModel model)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"type",
				"MegaTicket_Buff"
			},
			{
				"resources_summary",
				model.Name
			},
			{
				"context_of_offer",
				this.gameController.game.planetName
			},
			{
				"soft_currency_6_change",
				-1
			}
		};
		this.SendAnalyticEvent("economy_transactions", evt, true, model.Name);
	}

	// Token: 0x060001C2 RID: 450 RVA: 0x0000A234 File Offset: 0x00008434
	public void SendUpgradeAnalytic(Upgrade upgrade)
	{
		if (!(upgrade is ManagerUpgrade) && !upgrade.isKongPlateau)
		{
			return;
		}
		string text = "";
		switch (upgrade.currency)
		{
		case Upgrade.Currency.InGameCash:
			text += "Cash_";
			break;
		case Upgrade.Currency.AngelInvestors:
			text += "Angel_";
			break;
		case Upgrade.Currency.Megabucks:
			text += "Megabucks_";
			break;
		}
		text += ((upgrade is ManagerUpgrade) ? "Manager" : "Upgrade");
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"type",
				text
			},
			{
				"resources_summary",
				upgrade.name
			},
			{
				"context_of_offer",
				this.gameController.game.planetName
			},
			{
				"hard_currency_change",
				0
			}
		};
		this.SendAnalyticEvent("economy_transactions", evt, true, upgrade.name);
	}

	// Token: 0x060001C3 RID: 451 RVA: 0x0000A318 File Offset: 0x00008518
	public void SendFTUEAnalyticEvent(string stepDescription, int step)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"tutorial_type",
				"tutorial"
			},
			{
				"description",
				stepDescription
			},
			{
				"step_number",
				step
			},
			{
				"is_final",
				false
			}
		};
		this.SendAnalyticEvent("tutorial_step_ends", evt, true, "tutorial_step_" + step);
	}

	// Token: 0x060001C4 RID: 452 RVA: 0x0000A388 File Offset: 0x00008588
	public void SendTutorialEvent(string stepId, int stepNumber, bool isCompleted)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"tutorial_type",
				stepId
			},
			{
				"step_number",
				stepNumber
			},
			{
				"tutorial_completed ",
				isCompleted
			}
		};
		this.SendAnalyticEvent("tutorial_step_ends", evt, false, "");
	}

	// Token: 0x060001C5 RID: 453 RVA: 0x0000A3DC File Offset: 0x000085DC
	public void SendAdsUnavailableEvent(int availableAds)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"reward",
				"AdsAvailable:" + availableAds
			},
			{
				"ad_type",
				"VideoUnavailable"
			}
		};
		this.SendAnalyticEvent("ad_end", evt, false, "");
	}

	// Token: 0x060001C6 RID: 454 RVA: 0x0000A42C File Offset: 0x0000862C
	public void SendAdsAvailableChangedEvent(bool isAvialable)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"client_event_name",
				"AdAvailablityChanged"
			},
			{
				"bool1",
				isAvialable
			}
		};
		this.SendAnalyticEvent(KongAnalyticService.DEV_ANALYTIC_TABLE_ID, evt, false, "");
	}

	// Token: 0x060001C7 RID: 455 RVA: 0x0000A474 File Offset: 0x00008674
	public void SendPlatniumUpgradeFixedEvent(string planetName, int venturesReset)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"client_event_name",
				"PlanetPlatniumGildingFix"
			},
			{
				"message",
				planetName
			},
			{
				"number1",
				venturesReset
			}
		};
		this.SendAnalyticEvent(KongAnalyticService.DEV_ANALYTIC_TABLE_ID, evt, false, "");
	}

	// Token: 0x060001C8 RID: 456 RVA: 0x0000A4C8 File Offset: 0x000086C8
	public void SendShortLeaderboardError(int errorNumber, string errorMessage)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"client_event_name",
				"ShortLeaderboardError"
			},
			{
				"number1",
				errorNumber
			},
			{
				"message",
				errorMessage
			}
		};
		this.SendAnalyticEvent(KongAnalyticService.DEV_ANALYTIC_TABLE_ID, evt, false, "");
	}

	// Token: 0x060001C9 RID: 457 RVA: 0x0000A51C File Offset: 0x0000871C
	public void SendMissionEvent(string analyticId, UserEventMission mission = null, string description = "")
	{
		string collection = "task_completes";
		string value = GameController.Instance.game.planetName + ((mission == null) ? "" : string.Concat(new object[]
		{
			":",
			mission.ID,
			":",
			mission.Type.ToString()
		}));
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"task_id",
				analyticId
			},
			{
				"task_type",
				value
			},
			{
				"task_description",
				description
			}
		};
		this.SendAnalyticEvent(collection, evt, false, "");
	}

	// Token: 0x060001CA RID: 458 RVA: 0x0000A5CC File Offset: 0x000087CC
	public void SendFailedToGenerateMissionTask(string eventId, string cashToSpend, int taskCompletes)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"client_event_name",
				"FailedToGenerateMissionTask"
			},
			{
				"message",
				eventId + ":" + cashToSpend
			},
			{
				"number1",
				taskCompletes
			}
		};
		this.SendAnalyticEvent(KongAnalyticService.DEV_ANALYTIC_TABLE_ID, evt, false, "");
	}

	// Token: 0x060001CB RID: 459 RVA: 0x0000A62C File Offset: 0x0000882C
	public void SendCampaignEvent(string ddna_event_id)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"ddna_event_id",
				ddna_event_id
			}
		};
		this.SendAnalyticEvent("campaign_events", evt, false, "");
	}

	// Token: 0x060001CC RID: 460 RVA: 0x0000A660 File Offset: 0x00008860
	public void SendMilestoneEvent(string analyticId, UserPlanetMilestone milestone = null, string description = "")
	{
		string planetName = GameController.Instance.game.planetName;
		string collection = "task_completes";
		string value = planetName + ((milestone == null) ? "" : (":" + milestone.MilestoneId));
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"task_id",
				analyticId
			},
			{
				"task_type",
				value
			},
			{
				"task_description",
				description
			}
		};
		this.SendAnalyticEvent(collection, evt, false, "");
	}

	// Token: 0x060001CD RID: 461 RVA: 0x0000A6DC File Offset: 0x000088DC
	private Dictionary<string, object> GetCommonPropertiesDictionary()
	{
		PlayerData playerData = PlayerData.GetPlayerData("Global");
		return new Dictionary<string, object>
		{
			{
				"hard_currency_balance",
				playerData.GetInt("Gold", 0)
			},
			{
				"tutorial_completed",
				FTUE_Manager.GetSeenFTUE("FirstFTUE")
			},
			{
				"soft_currency_5_balance",
				playerData.GetInt("MegaBucksBalance", 0)
			},
			{
				"soft_currency_6_balance",
				playerData.GetInt("MegaTickets", 0)
			},
			{
				"device_orientation",
				OrientationController.Instance.CurrentOrientation.IsPortrait ? "Portrait" : "Landscape"
			},
			{
				"playfab_id",
				(Helper.GetPlatformAccount().LastLoginResult == null) ? "" : Helper.GetPlatformAccount().LastLoginResult.PlayFabId
			},
			{
				"subscriber",
				this.gameController.SubscriptionService.IsActive.Value ? "true" : "false"
			},
			{
				"hard_currency_earned",
				playerData.GetInt(GameController.EARNED_GOLD_PREF_KEY, 0)
			},
			{
				"hard_currency_bought",
				playerData.GetInt(GameController.BOUGHT_GOLD_PREF_KEY, 0)
			},
			{
				"player_ab_test_groups",
				this.gameController.UserDataService.ListAbTestGroups()
			},
			{
				"soft_currency_4_balance",
				GameState.GetDoubleAsInt(this.gameController.AngelService.AngelsOnHand.Value)
			},
			{
				"planet_name",
				(this.gameController.game != null) ? this.gameController.game.planetName : "none"
			}
		};
	}

	// Token: 0x060001CE RID: 462 RVA: 0x00002718 File Offset: 0x00000918
	public void InitializeAnalyticsHooks()
	{
	}

	// Token: 0x060001CF RID: 463 RVA: 0x0000A898 File Offset: 0x00008A98
	private void InitializeGameEventsAnalyticsHooks()
	{
		if (!this.gameController.GlobalPlayerData.Has(KongAnalyticService.FIRST_RESET_ANALYTIC_KEY))
		{
			Observable.FromEvent(delegate(Action h)
			{
				this.gameController.OnSoftResetPost += h;
			}, delegate(Action h)
			{
				this.gameController.OnSoftResetPost -= h;
			}).Subscribe(delegate(Unit _)
			{
				this.SendAdjustAnalyticEvent(KongregateGameObject.ADJUST_KONG_PRESTIGE_KEY, null);
				this.gameController.GlobalPlayerData.Set(KongAnalyticService.FIRST_RESET_ANALYTIC_KEY, "true");
			}).AddTo(this.disposables);
		}
		(from u in this.gameController.UnlockService.OnUnlockAchievedFirstTime
		where u.permanent
		select u).Subscribe(new Action<Unlock>(this.OnPerminantUnlockReward)).AddTo(this.disposables);
		this.gameController.UnlockService.OnUnlockAchievedFirstTime.Subscribe(new Action<Unlock>(this.SendUnlockAnalytic)).AddTo(this.disposables);
		Observable.FromEvent(delegate(Action h)
		{
			this.gameController.OnSoftResetPre += h;
		}, delegate(Action h)
		{
			this.gameController.OnSoftResetPre -= h;
		}).Subscribe(delegate(Unit _)
		{
			int count = this.gameController.UnlockService.Unlocks.FindAll(a => a.Earned.Value).Count;
			string planetName = this.gameController.game.planetName;
			Dictionary<string, object> evt = new Dictionary<string, object>
			{
				{
					"type",
					planetName.ToLower()
				},
				{
					"num_achievements",
					count
				}
			};
			this.SendAnalyticEvent("play_prestige", evt, false, "");
		}).AddTo(this.disposables);
		MessageBroker.Default.Receive<RewardsGrantedEvent>().Subscribe(new Action<RewardsGrantedEvent>(this.OnRewardsGranted)).AddTo(this.disposables);
		MessageBroker.Default.Receive<TimeWarpEvent>().Subscribe(delegate(TimeWarpEvent evt)
		{
			this.SendKongEconomyTransaction(new KongEconomyTransactionData(evt.IsExpress ? "Time Warp Express" : "Time Warp", "Profit: " + NumberFormat.Convert(evt.AmountEarned, 1000000.0, true, 3), this.gameController.game.planetName.ToLower(), 0, 0, 0));
		}).AddTo(this.disposables);
		if (this.gameController.GlobalPlayerData.GetInt(GameController.SESSION_COUNT_KEY, 0) < 10)
		{
			(from x in this.gameController.GlobalPlayerData.GetObservable(GameController.SESSION_COUNT_KEY, 0.0)
			where x == 10.0
			select x).Subscribe(delegate(double _)
			{
				this.SendAdjustAnalyticEvent(KongregateGameObject.ADJUST_KONG_10TH_SESSION_KEY, null);
			}).AddTo(this.disposables);
		}
		if (this.gameController.GlobalPlayerData.GetInt(GameController.LOGIN_CURRENT_SEQUENTIAL_DAYS_KEY, 0) < 7)
		{
			(from x in this.gameController.GlobalPlayerData.GetObservable(GameController.LOGIN_CURRENT_SEQUENTIAL_DAYS_KEY, 0.0)
			where x == 7.0
			select x).Subscribe(delegate(double _)
			{
				this.SendAdjustAnalyticEvent(KongregateGameObject.ADJUST_KONG_7_CONSECUTIVE_DAYS_PLAYED_KEY, null);
			}).AddTo(this.disposables);
		}
		this.gameController.State.Subscribe(new Action<GameState>(this.OnGameStateChanged)).AddTo(this.disposables);
		if (!this.gameController.GlobalPlayerData.GetBool(EventService.EVENT_ACCESSIBLE_PREFS_STRING))
		{
			(from x in this.gameController.EventService.EventUnlocked
			where x
			select x).Take(1).Subscribe(new Action<bool>(this.OnEventUnlocked)).AddTo(this.disposables);
		}
		(from x in this.gameController.RedemptionService.RedemptionEventCallback
		where x.Success && x.Type == RedemptionEventType.RedemptionCompleted
		select x).Subscribe(new Action<RedemptionEvent>(this.OnRedmeptionCompleted)).AddTo(this.disposables);
		foreach (ItemPurchaseData purchaseData in this.gameController.StoreService.PurchaseData)
		{
			this.WatchPurchaseData(purchaseData);
		}
		(from x in this.gameController.StoreService.PurchaseData.ObserveAdd()
		select x.Value).Subscribe(new Action<ItemPurchaseData>(this.WatchPurchaseData)).AddTo(this.disposables);
		foreach (AdCapStoreItem adCapStoreItem in this.gameController.StoreService.NewTimedItems)
		{
			this.OnNewTimedItemAddedToStore(adCapStoreItem.Id);
		}
		(from x in this.gameController.StoreService.NewTimedItems.ObserveAdd()
		select x.Value.Id).Subscribe(new Action<string>(this.OnNewTimedItemAddedToStore)).AddTo(this.disposables);
	}

	// Token: 0x060001D0 RID: 464 RVA: 0x0000AD18 File Offset: 0x00008F18
	private void OnNewTimedItemAddedToStore(string id)
	{
		this.SendNavActionAnalytics("Offered", "TimedOffer", id);
	}

	// Token: 0x060001D1 RID: 465 RVA: 0x0000AD2C File Offset: 0x00008F2C
	private void WatchPurchaseData(ItemPurchaseData purchaseData)
	{
		if (!purchaseData.HasExpired.Value)
		{
			(from x in purchaseData.HasExpired
			where x
			select x).Subscribe(delegate(bool _)
			{
				this.SendTaskCompleteEvent("Expired", "TimedOffer", purchaseData.ItemId);
			}).AddTo(this.disposables);
		}
	}

	// Token: 0x060001D2 RID: 466 RVA: 0x0000ADAC File Offset: 0x00008FAC
	private void OnRedmeptionCompleted(RedemptionEvent e)
	{
		AdCap.Store.Product product = e.Item.Product;
		if (product <= AdCap.Store.Product.PlatinumUpgrade)
		{
			if (product == AdCap.Store.Product.FluxCapitalor)
			{
				this.SendKongEconomyTransaction(new KongEconomyTransactionData("code_redeemed", "Flux_Capitalor_x" + e.Item.Qty, GameController.Instance.game.planetName, 0, 0, 0));
				return;
			}
			switch (product)
			{
			case AdCap.Store.Product.Gold:
				this.SendKongEconomyTransaction(new KongEconomyTransactionData("code_redeemed", "Gold", GameController.Instance.game.planetName, (int)e.Item.Qty, 0, 0));
				return;
			case AdCap.Store.Product.Angels:
			case AdCap.Store.Product.Unlock:
			case AdCap.Store.Product.Hack:
			case AdCap.Store.Product.PlatinumUpgrade:
				return;
			case AdCap.Store.Product.Badge:
				this.SendKongEconomyTransaction(new KongEconomyTransactionData("code_redeemed", string.Concat(new object[]
				{
					"badge_",
					e.Item.id,
					"_x",
					e.Item.Qty
				}), GameController.Instance.game.planetName, 0, 0, 0));
				return;
			case AdCap.Store.Product.Megabucks:
				this.SendKongEconomyTransaction(new KongEconomyTransactionData("code_redeemed", "Megabucks", GameController.Instance.game.planetName, 0, (int)e.Item.Qty, 0));
				return;
			}
		}
		else
		{
			if (product == AdCap.Store.Product.InventoryItem)
			{
				this.SendKongEconomyTransaction(new KongEconomyTransactionData("code_redeemed", string.Concat(new object[]
				{
					"Item_",
					e.Item.id,
					"_x",
					e.Item.Qty
				}), GameController.Instance.game.planetName, 0, 0, 0));
				return;
			}
			if (product == AdCap.Store.Product.AngelFix)
			{
				return;
			}
		}
		Debug.LogError("[CodeRedemptionService] Unsupported product type " + e.Item.Product);
	}

	// Token: 0x060001D3 RID: 467 RVA: 0x0000AFA4 File Offset: 0x000091A4
	private void OnAngelsClaimed(AngelsClaimedEvent evt)
	{
		int angelsLeaderboardScore = this.gameController.game.GetAngelsLeaderboardScore();
		this.SendTaskCompleteEvent("prestige_logging", this.gameController.game.planetName + "_prestige", angelsLeaderboardScore.ToString());
		string taskDescription = string.Format("[{0}] {1}", angelsLeaderboardScore, NumberFormat.Convert(GameState.GetValueFromLeaderboardScore(angelsLeaderboardScore), 999999.0, false, 3));
		this.SendTaskCompleteEvent("EventPlanet_Angel_Reset", this.gameController.game.planetName, taskDescription);
	}

	// Token: 0x060001D4 RID: 468 RVA: 0x0000B031 File Offset: 0x00009231
	private void OnEventUnlocked(bool unlocked)
	{
		this.SendTaskCompleteEvent("EventsUnlocked", "oil-5", "");
	}

	// Token: 0x060001D5 RID: 469 RVA: 0x0000B048 File Offset: 0x00009248
	private void OnGameStateChanged(GameState state)
	{
		this.stateDisposables.Clear();
		foreach (VentureModel model2 in state.VentureModels)
		{
			VentureModel model = model2;
			(from pair in model.NumOwned_Base.Pairwise<double>()
			where pair.Previous == 0.0 && pair.Current > 0.0
			select pair).Subscribe(delegate(Pair<double> x)
			{
				this.OnVentureUnlockedOnFirstPurchase(model);
			}).AddTo(this.stateDisposables);
		}
		state.VentureModels.ObserveAdd().Subscribe(delegate(CollectionAddEvent<VentureModel> addEvt)
		{
			VentureModel model = addEvt.Value;
			(from pair in model.NumOwned_Base.Pairwise<double>()
			where pair.Previous == 0.0 && pair.Current > 0.0
			select pair).Subscribe(delegate(Pair<double> x)
			{
				this.OnVentureUnlockedOnFirstPurchase(model);
			}).AddTo(this.stateDisposables);
		}).AddTo(this.stateDisposables);
		state.planetPlayerData.GetObservable("MegaBucksCreated", 0.0).Pairwise((x, y) => y - x).Subscribe(delegate(double x)
		{
			this.SendKongEconomyTransaction(new KongEconomyTransactionData("ExchangeCurrency", "Megabucks", state.planetName, 0, (int)x, 0));
		}).AddTo(this.stateDisposables);
		if (state.IsEventPlanet)
		{
			MessageBroker.Default.Receive<AngelsClaimedEvent>().Subscribe(new Action<AngelsClaimedEvent>(this.OnAngelsClaimed)).AddTo(this.stateDisposables);
		}
	}

	// Token: 0x060001D6 RID: 470 RVA: 0x0000B1D8 File Offset: 0x000093D8
	private void InitializePurchaseAnalyticsHooks()
	{
		Debug.Log("[KONG]Initializing hooks for purchase analytics");
		Helper.GetPlatformStore().OnPurchaseStart.Subscribe(delegate(PurchaseEvent e)
		{
			AdCapStoreItem adCapStoreItem = this.gameController.StoreService.CurrentCatalog.FirstOrDefault(x => x.PlatformId == e.Item.ItemId);
			if (adCapStoreItem != null)
			{
				Dictionary<string, object> gameFields = new Dictionary<string, object>
				{
					{
						"type",
						adCapStoreItem.StoreItemType.ToString()
					}
				};
				this.StartPurchase(e.Item.ItemId, 1, gameFields);
			}
		}).AddTo(this.disposables);
		Helper.GetPlatformStore().OnPurchaseFail.Subscribe(delegate(PurchaseEvent e)
		{
			this.FinishPurchase("FAIL", e.Error.ToString(), this.GetPurchaseFields(0.0, 0));
		}).AddTo(this.disposables);
		Helper.GetPlatformStore().OnPurchaseSuccess.Subscribe(delegate(PurchaseEvent evt)
		{
			CatalogItemBundleInfo bundle = evt.Item.CatalogItem.Bundle;
			int num = (int)((bundle.BundledVirtualCurrencies != null && bundle.BundledVirtualCurrencies.ContainsKey("GB")) ? bundle.BundledVirtualCurrencies["GB"] : 0U);
			this.GetPurchaseFields((double)num, 0);
		}).AddTo(this.disposables);
		(from e in MessageBroker.Default.Receive<StorePurchaseEvent>()
		where e.PurchaseState == EStorePurchaseState.Success && e.Item.Cost.Price > 0.0 && (e.PurchaseCurrency == AdCap.Store.Currency.Gold || e.PurchaseCurrency == AdCap.Store.Currency.MegaBuck || e.PurchaseCurrency == AdCap.Store.Currency.InGameCash || e.PurchaseCurrency == AdCap.Store.Currency.MegaTicket)
		select e).Subscribe(new Action<StorePurchaseEvent>(this.OnEventMobilePurchaseComplete)).AddTo(this.disposables);
	}

	// Token: 0x060001D7 RID: 471 RVA: 0x0000B2AF File Offset: 0x000094AF
	private void InitializeAdAnalyticsHooks()
	{
		Helper.GetPlatformAd().OfferWallCreditsReceived.Subscribe(new Action<int>(this.OnOfferwallCredited)).AddTo(this.disposables);
	}

	// Token: 0x060001D8 RID: 472 RVA: 0x0000B2D8 File Offset: 0x000094D8
	private void OnOfferwallCredited(int numberOfCredits)
	{
		this.OnOfferwallGoldRewarded(numberOfCredits);
		this.SendAdFinished("Offerwal", "offerwall", this.gameController.game.planetName, numberOfCredits.ToString());
		if (!this.gameController.GlobalPlayerData.Has(KongAnalyticService.ANALYTICS_OFFERWALL_FIRST_EARNED))
		{
			this.SendAdjustAnalyticEvent(KongregateGameObject.ADJUST_KONG_FIRST_OW_COMPLETION_KEY, null);
			this.gameController.GlobalPlayerData.Set(KongAnalyticService.ANALYTICS_OFFERWALL_FIRST_EARNED, "true");
		}
	}

	// Token: 0x060001D9 RID: 473 RVA: 0x0000B351 File Offset: 0x00009551
	public void SendAdjustAnalyticEvent(string key, Dictionary<string, object> data = null)
	{
		Debug.Log("(SendAdjustAnalyticEvent) " + key);
	}

	// Token: 0x060001DA RID: 474 RVA: 0x0000B364 File Offset: 0x00009564
	private string GetAdjustToken(string adjustMapKey)
	{
		object obj;
		KongregateAPI.Settings.AdjustEventTokenMap.TryGetValue(adjustMapKey, out obj);
		if (obj != null)
		{
			return obj.ToString();
		}
		return "";
	}

	// Token: 0x060001DB RID: 475 RVA: 0x0000B390 File Offset: 0x00009590
	private Dictionary<string, object> GetPurchaseFields(double softCurrencyChange, int hardCurrencyChange)
	{
		return new Dictionary<string, object>
		{
			{
				"hard_currency_change",
				hardCurrencyChange
			},
			{
				"soft_currency_change",
				softCurrencyChange
			},
			{
				"type",
				"Coin Pack"
			},
			{
				"Money Now",
				this.gameController.game.CashOnHand.Value
			},
			{
				"Money This Game",
				this.gameController.game.SessionCash.Value
			},
			{
				"Money All Time",
				this.gameController.game.TotalPreviousCash.Value
			}
		};
	}

	// Token: 0x060001DC RID: 476 RVA: 0x0000B444 File Offset: 0x00009644
	private void OnEventMobilePurchaseComplete(StorePurchaseEvent purchaseEvent)
	{
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"type",
				purchaseEvent.PurchaseCurrency + "_Buff"
			},
			{
				"resources_summary",
				purchaseEvent.Item.Id
			},
			{
				"context_of_offer",
				this.gameController.game.planetName
			},
			{
				"hard_currency_change",
				(purchaseEvent.PurchaseCurrency == AdCap.Store.Currency.Gold) ? (-purchaseEvent.Item.Cost.Price) : 0.0
			},
			{
				"soft_currency_5_change",
				(purchaseEvent.PurchaseCurrency == AdCap.Store.Currency.MegaBuck) ? (-purchaseEvent.Item.Cost.Price) : 0.0
			},
			{
				"soft_currency_6_change",
				0
			}
		};
		this.SendAnalyticEvent("economy_transactions", evt, false, "");
	}

	// Token: 0x060001DD RID: 477 RVA: 0x0000B538 File Offset: 0x00009738
	private void OnPerminantUnlockReward(Unlock perminantUnlockReward)
	{
		int num = 0;
		int num2 = 0;
		string value;
		if (perminantUnlockReward.Reward is UnlockRewardTimewarpHourly)
		{
			value = "Time_Warp_Reward_1_Hour";
		}
		else
		{
			if (!(perminantUnlockReward.Reward is UnlockRewardTimewarpDaily))
			{
				return;
			}
			value = "Time_Warp_Reward_1_Day";
		}
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"type",
				value
			},
			{
				"resources_summary",
				perminantUnlockReward.name
			},
			{
				"context_of_offer",
				this.gameController.game.planetName
			},
			{
				"hard_currency_change",
				num
			},
			{
				"soft_currency_5_change",
				num2
			},
			{
				"soft_currency_6_change",
				0
			},
			{
				"current_equipped_items",
				this.gameController.GlobalPlayerData.inventory.GetAllEquippedItemsStringForAnalytics()
			}
		};
		this.SendAnalyticEvent("economy_transactions", evt, false, "");
	}

	// Token: 0x060001DE RID: 478 RVA: 0x0000B620 File Offset: 0x00009820
	private void SendUnlockAnalytic(Unlock unlock)
	{
		if (!(unlock is EveryVentureUnlock))
		{
			return;
		}
		Dictionary<string, object> evt = new Dictionary<string, object>
		{
			{
				"task_id",
				unlock.name
			},
			{
				"task_type",
				"achievement_" + this.gameController.game.planetName
			},
			{
				"task_description",
				string.Format("earn {0} of every investment", unlock.amountToEarn)
			}
		};
		this.SendAnalyticEvent("task_completes", evt, true, unlock.name);
	}

	// Token: 0x060001DF RID: 479 RVA: 0x0000B6A8 File Offset: 0x000098A8
	private void OnRewardsGranted(RewardsGrantedEvent evt)
	{
		evt.Rewards.ForEach(delegate(RewardData reward)
		{
			int goldChangeAmount = 0;
			int megabucksChangeAmount = 0;
			int megaTicketsChangeAmount = 0;
			string resourcesSummary;
			switch (reward.RewardType)
			{
			case ERewardType.None:
				resourcesSummary = "None";
				break;
			case ERewardType.Gold:
				goldChangeAmount = reward.Qty;
				resourcesSummary = "Gold";
				this.SendEarnedCurrencyEvent();
				break;
			case ERewardType.Item:
				this.SendAnalyticForItemReward(reward, evt.Source, evt.Context);
				return;
			case ERewardType.InvestmentQty:
				resourcesSummary = string.Format("Investment:{0}", reward.Id);
				break;
			case ERewardType.AngelsOnHand:
				resourcesSummary = "AngelsOnHand";
				break;
			case ERewardType.InstantItem:
				this.SendAnalyticForInstantItemReward(reward, evt.Source, evt.Context);
				return;
			default:
				Debug.LogError("[KongAnalytics](OnRewardsGranted) Unhandled RewardType (" + reward.RewardType + ")");
				return;
			}
			this.SendKongEconomyTransaction(new KongEconomyTransactionData(evt.Source, resourcesSummary, evt.Context, goldChangeAmount, megabucksChangeAmount, megaTicketsChangeAmount));
		});
	}

	// Token: 0x060001E0 RID: 480 RVA: 0x0000B6E8 File Offset: 0x000098E8
	private void SendAnalyticForItemReward(RewardData reward, string source, string context)
	{
		int goldChangeAmount = 0;
		int megabucksChangeAmount = 0;
		int megaTicketsChangeAmount = 0;
		string resourcesSummary = "";
		Item itemById = this.gameController.GlobalPlayerData.inventory.GetItemById(reward.Id);
		if (itemById != null)
		{
			AdCap.Store.Product product = itemById.Product;
			if (product <= AdCap.Store.Product.Megabucks)
			{
				if (product == AdCap.Store.Product.Gold)
				{
					goldChangeAmount = reward.Qty;
					resourcesSummary = "Gold";
					this.SendEarnedCurrencyEvent();
					goto IL_DA;
				}
				if (product == AdCap.Store.Product.Megabucks)
				{
					megabucksChangeAmount = reward.Qty;
					resourcesSummary = "Megabucks";
					goto IL_DA;
				}
			}
			else
			{
				if (product == AdCap.Store.Product.MegaTicket)
				{
					megaTicketsChangeAmount = reward.Qty;
					resourcesSummary = "MegaTicket";
					goto IL_DA;
				}
				if (product == AdCap.Store.Product.MonthlySubscription)
				{
					resourcesSummary = "Monthy Subscription";
					goto IL_DA;
				}
			}
			if (itemById.ItemType == ItemType.Badge || itemById.ItemType == ItemType.Trophy)
			{
				resourcesSummary = "badge_" + reward.Id;
			}
			else
			{
				resourcesSummary = itemById.ItemId + "x" + reward.Qty;
			}
		}
		IL_DA:
		this.SendKongEconomyTransaction(new KongEconomyTransactionData(source, resourcesSummary, context, goldChangeAmount, megabucksChangeAmount, megaTicketsChangeAmount));
	}

	// Token: 0x060001E1 RID: 481 RVA: 0x0000B7E0 File Offset: 0x000099E0
	private void SendAnalyticForInstantItemReward(RewardData reward, string source, string context)
	{
		int goldChangeAmount = 0;
		int megabucksChangeAmount = 0;
		int megaTicketsChangeAmount = 0;
		Item itemById = this.gameController.GlobalPlayerData.inventory.GetItemById(reward.Id);
		AdCap.Store.Product product = itemById.Product;
		string resourcesSummary;
		if (product <= AdCap.Store.Product.PlatinumUpgrade)
		{
			switch (product)
			{
			case AdCap.Store.Product.TimeWarp:
				resourcesSummary = "TimeWarp";
				goto IL_CB;
			case AdCap.Store.Product.Multiplier:
				resourcesSummary = "Multiplier";
				goto IL_CB;
			case AdCap.Store.Product.ProfitMartiansBoost:
				break;
			case AdCap.Store.Product.FluxCapitalor:
				resourcesSummary = "FluxCapitalor";
				goto IL_CB;
			case AdCap.Store.Product.AdWatchBoost:
				resourcesSummary = "AdWatchBoost";
				goto IL_CB;
			case AdCap.Store.Product.AngelClaim:
				resourcesSummary = "AngelClaim";
				goto IL_CB;
			default:
				if (product == AdCap.Store.Product.PlatinumUpgrade)
				{
					resourcesSummary = "Platnium Gilding";
					goto IL_CB;
				}
				break;
			}
		}
		else
		{
			if (product == AdCap.Store.Product.TimeWarpExpress)
			{
				resourcesSummary = "TimeWarpExpress";
				goto IL_CB;
			}
			if (product == AdCap.Store.Product.VentureGilding)
			{
				resourcesSummary = "Venture Gilding";
				goto IL_CB;
			}
		}
		Debug.LogError(string.Format("Instant Reward Product:[{0}] is and invalid InstantReward", itemById.Product));
		return;
		IL_CB:
		this.SendKongEconomyTransaction(new KongEconomyTransactionData(source, resourcesSummary, context, goldChangeAmount, megabucksChangeAmount, megaTicketsChangeAmount));
	}

	// Token: 0x060001E2 RID: 482 RVA: 0x00002718 File Offset: 0x00000918
	private void SendAnalyticEvent(string collection, Dictionary<string, object> evt, bool sendOnce = false, string sendOnceKey = "")
	{
	}

	// Token: 0x060001E3 RID: 483 RVA: 0x0000B8CC File Offset: 0x00009ACC
	private void AddToPlayfabQueue(string collection, Dictionary<string, object> evt)
	{
		if (this.SendAnalyticsToPlayfab)
		{
			try
			{
				Dictionary<string, object> commonPropertiesDictionary = this.GetCommonPropertiesDictionary();
				EventContents eventContents = new EventContents();
				eventContents.Name = collection;
				foreach (KeyValuePair<string, object> keyValuePair in evt)
				{
					if (!commonPropertiesDictionary.ContainsKey(keyValuePair.Key))
					{
						commonPropertiesDictionary.Add(keyValuePair.Key, keyValuePair.Value);
					}
				}
				string payloadJSON = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer, "").SerializeObject(commonPropertiesDictionary);
				eventContents.EventNamespace = "com.playfab.events.adventurecapitalist";
				eventContents.PayloadJSON = payloadJSON;
				this.pendingPlayfabAnalytics.Add(eventContents);
				if (this.pendingPlayfabAnalytics.Count >= 5)
				{
					this.SendPlayfabAnalyticsBatch();
				}
			}
			catch (Exception)
			{
			}
		}
	}

	// Token: 0x060001E4 RID: 484 RVA: 0x0000B9AC File Offset: 0x00009BAC
	private void SendPlayfabAnalyticsBatch()
	{
		if (this.pendingPlayfabAnalytics.Count > 0)
		{
			WriteEventsRequest writeEventsRequest = new WriteEventsRequest();
			writeEventsRequest.Events = this.pendingPlayfabAnalytics;
			this.pendingPlayfabAnalytics = new List<EventContents>();
			this.playFab.WriteTelemetryEvents(writeEventsRequest, delegate(WriteEventsResponse success)
			{
				Debug.Log("Success");
			}, delegate(PlayFabError error)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Send analytics Error ",
					error.Error,
					":",
					error.ErrorMessage
				}));
			}, null, null);
		}
	}

	// Token: 0x060001E5 RID: 485 RVA: 0x0000BA30 File Offset: 0x00009C30
	private void StartPurchase(string productID, int quantity, Dictionary<string, object> gameFields)
	{
		if (this.kongAPI != null)
		{
			this.kongAPI.StartPurchase(productID, 1, gameFields);
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.AddAllKVPFrom(gameFields);
		this.addIAPEvent("iap_attempts", productID, dictionary, null, null, null);
	}

	// Token: 0x060001E6 RID: 486 RVA: 0x0000BA70 File Offset: 0x00009C70
	private void FinishPurchase(string resultCodeString, string responseInfo, Dictionary<string, object> gameFields)
	{
		this.FinishPurchase(resultCodeString, responseInfo, gameFields, null);
	}

	// Token: 0x060001E7 RID: 487 RVA: 0x0000BA7C File Offset: 0x00009C7C
	private void FinishPurchase(string resultCodeString, string responseInfo, Dictionary<string, object> gameFields, string dataSignture)
	{
		if (this.kongAPI != null)
		{
			this.kongAPI.FinishPurchase(resultCodeString, responseInfo, gameFields, dataSignture);
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.AddAllKVPFrom(gameFields);
		UnityAnalytics.IabResultType iabResultType = (UnityAnalytics.IabResultType)Enum.Parse(typeof(UnityAnalytics.IabResultType), resultCodeString);
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		Dictionary<string, object> json = null;
		string text = null;
		if (UnityAnalytics.IabResultType.SUCCESS.Equals(iabResultType) || UnityAnalytics.IabResultType.RECEIPT_FAIL.Equals(iabResultType))
		{
			json = KongAnalyticService.ParseJSONObject(responseInfo);
			string value = KongAnalyticService.optStringWarn(json, "orderId", "", "unable to parse orderId from responseInfo in finishPurchase()");
			dictionary2["receipt_id"] = value;
		}
		else
		{
			dictionary2["receipt_id"] = null;
		}
		if (UnityAnalytics.IabResultType.SUCCESS.Equals(iabResultType))
		{
			string productID = KongAnalyticService.optStringWarn(json, "productId", "", "unable to parse productId from responseInfo in finishPurchase()");
			if (!string.IsNullOrEmpty(text))
			{
				dictionary2["success_reason"] = text;
			}
			this.iapTransaction(productID, dictionary2, dictionary, responseInfo, dataSignture);
			return;
		}
		if (UnityAnalytics.IabResultType.RECEIPT_FAIL.Equals(iabResultType))
		{
			this.iapFails(text, dictionary2, dictionary);
			return;
		}
		if (UnityAnalytics.IabResultType.FAIL.Equals(iabResultType))
		{
			this.iapFails(responseInfo, dictionary2, dictionary);
			return;
		}
		Debug.LogWarning("invalid result code passed to finishPurchase: " + iabResultType);
	}

	// Token: 0x060001E8 RID: 488 RVA: 0x0000BBE8 File Offset: 0x00009DE8
	protected void iapTransaction(string productID, Dictionary<string, object> iapFields, Dictionary<string, object> gameFields, string data, string dataSignature)
	{
		if (iapFields != null || (gameFields != null && gameFields.Count > 0))
		{
			Debug.Log("iapTransaction: " + productID);
			if (iapFields != null)
			{
				iapFields["receipt_data"] = data;
				iapFields["receipt_signature"] = dataSignature;
			}
			this.addIAPEvent("iap_transactions", productID, iapFields, gameFields, data, dataSignature);
		}
	}

	// Token: 0x060001E9 RID: 489 RVA: 0x0000BC44 File Offset: 0x00009E44
	protected void iapFails(string failReason, Dictionary<string, object> iapFields, Dictionary<string, object> gameFields)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>(iapFields);
		dictionary["fail_reason"] = failReason;
		if (gameFields != null)
		{
			foreach (string key in gameFields.Keys)
			{
				dictionary[key] = gameFields[key];
			}
		}
		Debug.LogWarning("IAP FLOW STEP: completed: iap_fails : " + failReason);
		this.AddToPlayfabQueue("iap_fails", dictionary);
	}

	// Token: 0x060001EA RID: 490 RVA: 0x0000BCD0 File Offset: 0x00009ED0
	protected void addIAPEvent(string collection, string productId, Dictionary<string, object> gameFields, Dictionary<string, object> iapFields, string receipt, string receiptSignature)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		AdCapStoreItem adCapStoreItem = GameController.Instance.StoreService.CurrentCatalog.FirstOrDefault(x => x.Id == productId);
		ProductMetadata productMetadata = (adCapStoreItem != null) ? adCapStoreItem.RMCostMetaData : null;
		if (productMetadata != null)
		{
			dictionary.Add("local_currency_cost", (productMetadata != null) ? productMetadata.localizedPriceString : null);
			dictionary.Add("local_currency_type", (productMetadata != null) ? productMetadata.isoCurrencyCode : null);
		}
		double num = KongDeltaUtils.parseItemPrice(productId);
		dictionary["usd_cost"] = num;
		dictionary["product_id"] = productId;
		if (gameFields != null)
		{
			foreach (string key in gameFields.Keys)
			{
				dictionary[key] = gameFields[key];
			}
		}
		if (iapFields != null)
		{
			foreach (string key2 in iapFields.Keys)
			{
				dictionary[key2] = iapFields[key2];
			}
		}
		this.AddToPlayfabQueue(collection, dictionary);
	}

	// Token: 0x060001EB RID: 491 RVA: 0x0000BE2C File Offset: 0x0000A02C
	protected static string optStringWarn(Dictionary<string, object> json, string key, string defaultValue, string warningMessage)
	{
		if (json == null || json.Keys.Count == 0)
		{
			Debug.LogWarning("key " + key + " not found in null or empty json object: " + warningMessage);
			return defaultValue;
		}
		object obj;
		json.TryGetValue(key, out obj);
		if (obj == null)
		{
			Debug.LogWarning("key " + key + " not found in json object: " + warningMessage);
			return defaultValue;
		}
		return obj.ToString();
	}

	// Token: 0x060001EC RID: 492 RVA: 0x0000BE8C File Offset: 0x0000A08C
	protected static Dictionary<string, object> ParseJSONObject(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return new Dictionary<string, object>();
		}
		return Json.Deserialize(json) as Dictionary<string, object>;
	}

	// Token: 0x040001D4 RID: 468
	public static readonly string ANALYTIC_EARNED_GOLD_KEY = "UserEarnedGold";

	// Token: 0x040001D5 RID: 469
	public static readonly string FIRST_RESET_ANALYTIC_KEY = "FirstTimePRestigeAnalytic";

	// Token: 0x040001D6 RID: 470
	public static readonly string ANALYTICS_OFFERWALL_FIRST_EARNED = "OfferwallCreditEarned";

	// Token: 0x040001D7 RID: 471
	public static readonly string DEV_ANALYTIC_TABLE_ID = "application_events";

	// Token: 0x040001D8 RID: 472
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040001D9 RID: 473
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x040001DA RID: 474
	private IAnalytics kongAPI;

	// Token: 0x040001DB RID: 475
	private IGameController gameController;

	// Token: 0x040001DC RID: 476
	private PlayFabWrapper playFab;

	// Token: 0x040001DD RID: 477
	private Platforms.Logger.Logger logger;

	// Token: 0x040001DE RID: 478
	private bool sendAnalyticsToPlayfab;

	// Token: 0x040001DF RID: 479
	private IDisposable playfabAnalyticsWindoTimerDisposable;

	// Token: 0x040001E0 RID: 480
	private List<EventContents> pendingPlayfabAnalytics = new List<EventContents>();
}
