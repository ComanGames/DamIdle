using System;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using UniRx;
using UnityEngine;

// Token: 0x020000D9 RID: 217
public class GrantRewardService : IGrantRewardService, IDisposable
{
	// Token: 0x060005E3 RID: 1507 RVA: 0x00002718 File Offset: 0x00000918
	public void Dispose()
	{
	}

	// Token: 0x060005E4 RID: 1508 RVA: 0x0001F3BA File Offset: 0x0001D5BA
	public void Init(IGameController gameController, IInventoryService inventory, IAngelInvestorService angelService, SubscriptionService subscriptionService, GildingService gildingService)
	{
		this.gameController = gameController;
		this.inventory = inventory;
		this.angelService = angelService;
		this.subscriptionService = subscriptionService;
		this.gildingService = gildingService;
	}

	// Token: 0x060005E5 RID: 1509 RVA: 0x0001F3E4 File Offset: 0x0001D5E4
	public List<RewardData> GrantRewards(List<RewardData> rewards, string source, string context, bool isFromPurchase)
	{
		List<RewardData> list = (from x in rewards
		select this.GrantRewardInternal(x, isFromPurchase)).ToList<RewardData>();
		if (list.Count == rewards.Count)
		{
			MessageBroker.Default.Publish<RewardsGrantedEvent>(new RewardsGrantedEvent(rewards, source, context));
			return list;
		}
		Debug.LogError(string.Format("Error Granting Rewards with source {0} and context {1}", source, context));
		return null;
	}

	// Token: 0x060005E6 RID: 1510 RVA: 0x0001F454 File Offset: 0x0001D654
	public List<RewardData> GrantRewardsWithoutCelebration(List<RewardData> rewards, bool isFromPurchase)
	{
		List<RewardData> list = (from x in rewards
		select this.GrantRewardInternal(x, isFromPurchase)).ToList<RewardData>();
		if (list.Count == rewards.Count)
		{
			return list;
		}
		Debug.LogError(string.Format("Error Granting Rewards ", Array.Empty<object>()));
		return null;
	}

	// Token: 0x060005E7 RID: 1511 RVA: 0x0001F4B4 File Offset: 0x0001D6B4
	public RewardData GrantReward(RewardData rewardData, string source, string context, bool isFromPurchase)
	{
		RewardData rewardData2 = this.GrantRewardInternal(rewardData, isFromPurchase);
		if (rewardData2 != null)
		{
			MessageBroker.Default.Publish<RewardsGrantedEvent>(new RewardsGrantedEvent(rewardData2, source, context));
			return rewardData;
		}
		Debug.LogError(string.Format("Error Granting Reward {0} with source {1} and context {2}", rewardData.Id, source, context));
		return null;
	}

	// Token: 0x060005E8 RID: 1512 RVA: 0x0001F4FA File Offset: 0x0001D6FA
	public RewardData GrantRewardWithoutCelebration(RewardData rewardData, bool isFromPurchase)
	{
		if (this.GrantRewardInternal(rewardData, isFromPurchase) != null)
		{
			return rewardData;
		}
		Debug.LogError(string.Format("Error Granting Reward {0} ", rewardData.Id));
		return null;
	}

	// Token: 0x060005E9 RID: 1513 RVA: 0x0001F520 File Offset: 0x0001D720
	private RewardData GrantRewardInternal(RewardData rewardData, bool isFromPurchase)
	{
		switch (rewardData.RewardType)
		{
		case ERewardType.Gold:
			return this.GrantGoldReward(rewardData, isFromPurchase);
		case ERewardType.Item:
			return this.GrantItemReward(rewardData);
		case ERewardType.InvestmentQty:
			return this.GrantInvestmentReward(rewardData);
		case ERewardType.AngelsOnHand:
			return this.GrantAngelsOnHand(rewardData);
		case ERewardType.InstantItem:
			return this.GrantInstantItemReward(rewardData);
		default:
			Debug.LogError(string.Concat(new object[]
			{
				"[GrantRewardService] Unhandled RewardType (",
				rewardData.RewardType,
				") on reward with id (",
				rewardData.Id,
				")"
			}));
			return null;
		}
	}

	// Token: 0x060005EA RID: 1514 RVA: 0x0001F5BC File Offset: 0x0001D7BC
	private RewardData GrantGoldReward(RewardData rewardData, bool isFromPurchase)
	{
		if (rewardData.Qty > 0)
		{
			int modifiedGoldAmount = this.gameController.game.GetModifiedGoldAmount(rewardData.Qty);
			this.AddCurrency(modifiedGoldAmount);
			if (isFromPurchase)
			{
				this.gameController.GlobalPlayerData.Add(GameController.BOUGHT_GOLD_PREF_KEY, (double)modifiedGoldAmount);
			}
			else
			{
				this.gameController.GlobalPlayerData.Add(GameController.EARNED_GOLD_PREF_KEY, (double)modifiedGoldAmount);
			}
			return new RewardData
			{
				Id = rewardData.Id,
				Qty = modifiedGoldAmount,
				RewardType = rewardData.RewardType
			};
		}
		Debug.LogError(string.Format("Tried granting gold reward with qty less than 0. qty = {0}", rewardData.Qty));
		return null;
	}

	// Token: 0x060005EB RID: 1515 RVA: 0x0001F668 File Offset: 0x0001D868
	private RewardData GrantInvestmentReward(RewardData rewardData)
	{
		int index;
		VentureModel ventureModel = int.TryParse(rewardData.Id, out index) ? this.gameController.game.VentureModels[index] : this.gameController.game.VentureModels.FirstOrDefault((VentureModel x) => x.Id == rewardData.Id);
		if (ventureModel != null)
		{
			ventureModel.NumOwned_Upgrades.Value += (double)rewardData.Qty;
			return rewardData;
		}
		Debug.LogError(string.Format("Could not find venture [{0}] when trying to grant an investment", rewardData.Id));
		return null;
	}

	// Token: 0x060005EC RID: 1516 RVA: 0x0001F713 File Offset: 0x0001D913
	private RewardData GrantAngelsOnHand(RewardData rewardData)
	{
		this.angelService.AddAngelsOnHand((double)rewardData.Qty);
		return rewardData;
	}

	// Token: 0x060005ED RID: 1517 RVA: 0x0001F728 File Offset: 0x0001D928
	private RewardData GrantItemReward(RewardData rewardData)
	{
		Item itemById = this.inventory.GetItemById(rewardData.Id);
		if (itemById != null)
		{
			Product product = itemById.Product;
			if (product <= Product.Megabucks)
			{
				if (product == Product.Gold)
				{
					int modifiedGoldAmount = this.gameController.game.GetModifiedGoldAmount(rewardData.Qty);
					this.AddCurrency(modifiedGoldAmount);
					this.gameController.GlobalPlayerData.Add(GameController.EARNED_GOLD_PREF_KEY, (double)modifiedGoldAmount);
					return rewardData;
				}
				if (product == Product.Megabucks)
				{
					int modifiedMegaBucksAmount = this.gameController.game.GetModifiedMegaBucksAmount(rewardData.Qty);
					this.AwardMegaBucks((double)modifiedMegaBucksAmount);
					return new RewardData
					{
						Id = rewardData.Id,
						RewardType = rewardData.RewardType,
						Qty = modifiedMegaBucksAmount
					};
				}
			}
			else
			{
				if (product == Product.MegaTicket)
				{
					this.AddTickets(rewardData.Qty);
					return rewardData;
				}
				if (product == Product.MonthlySubscription)
				{
					this.subscriptionService.OnSubscriptionPurchased();
					return rewardData;
				}
			}
			this.inventory.AddItem(rewardData.Id, rewardData.Qty, true, true);
			return rewardData;
		}
		Debug.LogError(string.Format("Could not grant item rewrard for item [{0}]. Item not found", rewardData.Id));
		return null;
	}

	// Token: 0x060005EE RID: 1518 RVA: 0x0001F844 File Offset: 0x0001DA44
	private RewardData GrantInstantItemReward(RewardData rewardData)
	{
		Item itemById = this.inventory.GetItemById(rewardData.Id);
		if (itemById != null)
		{
			Product product = itemById.Product;
			if (product <= Product.Unlock)
			{
				switch (product)
				{
				case Product.TimeWarp:
				{
					int num;
					if (int.TryParse(itemById.BonusCustomData, out num))
					{
						this.gameController.TimeWarpService.ApplyTimeWarp((double)(num * 3600), false);
						goto IL_261;
					}
					goto IL_261;
				}
				case Product.Multiplier:
					this.ApplyMultiplier(rewardData.Qty / 3);
					goto IL_261;
				case Product.ProfitMartiansBoost:
					break;
				case Product.FluxCapitalor:
					this.ApplyFluxCapitalor(rewardData.Qty);
					goto IL_261;
				case Product.AdWatchBoost:
					this.OnAdWatchBoostPurchaseCompleted();
					goto IL_261;
				case Product.AngelClaim:
					this.gameController.AngelService.OnAngelClaimPurchaseCompleted();
					goto IL_261;
				default:
					if (product == Product.Angels)
					{
						double num2 = GameController.Instance.AngelService.CalculateCashFromAngels((double)rewardData.Qty, 1.0);
						this.angelService.AngelsOnHand.Value += GameController.Instance.AngelService.CalculateAngelInvestors(num2);
						this.gameController.game.TotalPreviousCash.Value += num2;
						goto IL_261;
					}
					if (product == Product.Unlock)
					{
						Unlock unlock = this.gameController.UnlockService.Unlocks.Find((Unlock a) => a.name == rewardData.Id);
						GameController.Instance.UnlockService.ClaimUnlock(unlock);
						goto IL_261;
					}
					break;
				}
			}
			else if (product <= Product.TimeWarpExpress)
			{
				if (product != Product.PlatinumUpgrade)
				{
					if (product == Product.TimeWarpExpress)
					{
						this.ApplyTimeWarpExpress();
						goto IL_261;
					}
				}
				else
				{
					if (this.gameController.game.IsEventPlanet)
					{
						this.gildingService.OnVentureGildingPurchased();
						goto IL_261;
					}
					this.gildingService.OnPlatniumGildingPurchased();
					goto IL_261;
				}
			}
			else
			{
				if (product == Product.VentureGilding)
				{
					this.gildingService.OnVentureGildingPurchased();
					goto IL_261;
				}
				if (product == Product.AngelFix)
				{
					double value = this.gameController.game.TotalPreviousCash.Value;
					double value2 = this.angelService.CalculateAngelInvestors(value);
					this.angelService.AngelsOnHand.Value = value2;
					goto IL_261;
				}
			}
			throw new Exception(string.Format("Instant Reward Product:[{0}] is and invalid InstantReward", itemById.Product));
			IL_261:
			return rewardData;
		}
		Debug.LogError(string.Format("Could not grant item rewrard for item [{0}]. Item not found", rewardData.Id));
		return null;
	}

	// Token: 0x060005EF RID: 1519 RVA: 0x0001FAD4 File Offset: 0x0001DCD4
	private void AddCurrency(int amt)
	{
		this.gameController.GlobalPlayerData.Add("Gold", (double)amt);
	}

	// Token: 0x060005F0 RID: 1520 RVA: 0x0001FAEE File Offset: 0x0001DCEE
	private void AwardMegaBucks(double amount)
	{
		this.gameController.GlobalPlayerData.Add("MegaBucksBalance", amount);
	}

	// Token: 0x060005F1 RID: 1521 RVA: 0x0001FB07 File Offset: 0x0001DD07
	private void AddTickets(int amount)
	{
		this.gameController.GlobalPlayerData.Add("MegaTickets", (double)amount);
		MessageBroker.Default.Publish<MegaTicketCountChanged>(new MegaTicketCountChanged());
	}

	// Token: 0x060005F2 RID: 1522 RVA: 0x0001FB30 File Offset: 0x0001DD30
	private void ApplyMultiplier(int multiplierPurchased)
	{
		this.gameController.game.planetPlayerData.Add("Multipliers", (double)multiplierPurchased);
		this.gameController.game.HighestMultiplierPurchased.Value = Math.Max(this.gameController.game.HighestMultiplierPurchased.Value, multiplierPurchased);
		int num = Math.Max(0, this.gameController.game.planetPlayerData.GetInt("Multipliers", 0) * 3);
		this.gameController.game.SetMultiplier((float)num);
	}

	// Token: 0x060005F3 RID: 1523 RVA: 0x0001FBC0 File Offset: 0x0001DDC0
	private void ApplyFluxCapitalor(int qty)
	{
		this.gameController.game.planetPlayerData.Add("Flux Capacitor", (double)qty);
		this.gameController.game.OnPlayerDataLoaded();
	}

	// Token: 0x060005F4 RID: 1524 RVA: 0x0001FBEF File Offset: 0x0001DDEF
	private void ApplyTimeWarpExpress()
	{
		this.gameController.TimeWarpService.UseTimeWarpExpress();
	}

	// Token: 0x060005F5 RID: 1525 RVA: 0x0001FC04 File Offset: 0x0001DE04
	private void OnAdWatchBoostPurchaseCompleted()
	{
		if (!this.gameController.GlobalPlayerData.Has("ad_watch_boost"))
		{
			this.gameController.GlobalPlayerData.Add("ad_watch_boost", 2.0);
		}
		this.gameController.GlobalPlayerData.Add("ad_watch_boost", 2.0);
	}

	// Token: 0x0400054D RID: 1357
	private IInventoryService inventory;

	// Token: 0x0400054E RID: 1358
	private IAngelInvestorService angelService;

	// Token: 0x0400054F RID: 1359
	private SubscriptionService subscriptionService;

	// Token: 0x04000550 RID: 1360
	private GildingService gildingService;

	// Token: 0x04000551 RID: 1361
	private IGameController gameController;
}
