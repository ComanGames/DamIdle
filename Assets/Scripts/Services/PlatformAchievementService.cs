using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Platforms;
using Services;
using Services.Steam;
using UniRx;
using UnityEngine;

// Token: 0x02000201 RID: 513
public class PlatformAchievementService
{
	// Token: 0x06000EE9 RID: 3817 RVA: 0x000437B8 File Offset: 0x000419B8
	public void Init(SteamService steamService)
	{
		MessageBroker.Default.Receive<AchievementUnlockedEvent>().Subscribe(new Action<AchievementUnlockedEvent>(this.OnAchievementUnlocked)).AddTo(this.disposables);
		(from x in GameController.Instance.State
		where x != null
		select x).Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
		GameController.Instance.OnLoadNewPlanetPre += this.OnPlanetChangePre;
		this.upgradeService = GameController.Instance.UpgradeService;
		this.LoadEarnedAchievements();
		if (Helper.GetPlatformType() == PlatformType.Steam && !Application.isEditor)
		{
			this.achievementPlatform = new SteamAchievementPlatform(steamService);
		}
		else
		{
			this.achievementPlatform = new MockAchievementPlatform();
		}
		this.achievementPlatform.Init(delegate(bool success)
		{
			this.platformReady = success;
			if (!this.platformReady)
			{
				Debug.LogError("[PlatformAchievementService] FAILED TO INITIALIZED CURRENT PLATFORM: " + this.achievementPlatform.ToString());
				return;
			}
			this.TryStartAchievementCheck();
		});
	}

	// Token: 0x06000EEA RID: 3818 RVA: 0x000438A4 File Offset: 0x00041AA4
	~PlatformAchievementService()
	{
		this.Dispose();
	}

	// Token: 0x06000EEB RID: 3819 RVA: 0x000438D0 File Offset: 0x00041AD0
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000EEC RID: 3820 RVA: 0x000438DD File Offset: 0x00041ADD
	public void OnPlanetChangePre()
	{
		this.stateDisposables.Clear();
		this.enabled = false;
	}

	// Token: 0x06000EED RID: 3821 RVA: 0x000438F1 File Offset: 0x00041AF1
	private void TryStartAchievementCheck()
	{
		if (this.platformReady && this.achievementObject != null)
		{
			this.enabled = true;
			this.AchievementCheck();
		}
	}

	// Token: 0x06000EEE RID: 3822 RVA: 0x00043918 File Offset: 0x00041B18
	public void OnStateChanged(GameState state)
	{
		this.stateDisposables.Clear();
		this.enabled = false;
		if (state == null)
		{
			this.upgradeService = null;
			this.state = null;
			this.achievementObject = null;
			return;
		}
		this.state = state;
		this.achievementObject = GameController.Instance.game.PlanetData.PlatformAchievements;
		this.TryStartAchievementCheck();
	}

	// Token: 0x06000EEF RID: 3823 RVA: 0x00043977 File Offset: 0x00041B77
	private void AchievementCheck()
	{
		Observable.FromCoroutine(new Func<IEnumerator>(this.AchievementCheckEnumerator), false).Subscribe<Unit>().AddTo(this.stateDisposables);
	}

	// Token: 0x06000EF0 RID: 3824 RVA: 0x0004399C File Offset: 0x00041B9C
	private void OnAchievementUnlocked(AchievementUnlockedEvent evt)
	{
		PlatformAchievement acht = this.achievementObject.achievements.FirstOrDefault((PlatformAchievement a) => a.name == evt.Id);
		if (acht != null)
		{
			this.achievementPlatform.AwardAchievement(acht, delegate(bool success)
			{
				if (success)
				{
					this.earnedAchievements.Add(this.achievementPlatform.GetAchievementID(acht));
					this.SaveEarnedAchievements();
				}
			});
		}
	}

	// Token: 0x06000EF1 RID: 3825 RVA: 0x00043A04 File Offset: 0x00041C04
	private IEnumerator AchievementCheckEnumerator()
	{
		while (this.enabled)
		{
			PlatformAchievement[] achievements = this.achievementObject.achievements;
			for (int i = 0; i < achievements.Length; i++)
			{
				PlatformAchievement achievement = achievements[i];
				if (this.achievementPlatform.IsValidForPlatform(achievement) && !this.earnedAchievements.Contains(this.achievementPlatform.GetAchievementID(achievement)))
				{
					bool flag = false;
					switch (achievement.type)
					{
					case PlatformAchievementService.AchievementType.OFFSHORE_ACCOUNTS:
						flag = (this.state.CashOnHand.Value >= 1E+42);
						break;
					case PlatformAchievementService.AchievementType.LEMON_STANDS_100:
					{
						VentureModel ventureModel = this.state.VentureModels.FirstOrDefault((VentureModel v) => v.Id == "lemon");
						if (ventureModel != null)
						{
							flag = (ventureModel.TotalOwned.Value >= 100.0);
						}
						break;
					}
					case PlatformAchievementService.AchievementType.OIL_RIG_1:
					{
						VentureModel ventureModel2 = this.state.VentureModels.FirstOrDefault((VentureModel v) => v.Id == "oil");
						if (ventureModel2 != null)
						{
							flag = (ventureModel2.TotalOwned.Value >= 1.0);
						}
						break;
					}
					case PlatformAchievementService.AchievementType.EVERYTHING_100:
						flag = this.state.VentureModels.All((VentureModel v) => v.TotalOwned.Value >= 100.0);
						break;
					case PlatformAchievementService.AchievementType.EVERYTHING_1000:
						flag = this.state.VentureModels.All((VentureModel v) => v.TotalOwned.Value >= 1000.0);
						break;
					case PlatformAchievementService.AchievementType.EVERYTHING_3000:
						flag = this.state.VentureModels.All((VentureModel v) => v.TotalOwned.Value >= 3000.0);
						break;
					case PlatformAchievementService.AchievementType.ALL_ACHIEVEMENTS:
						flag = (GameController.Instance.UnlockService.Unlocks.Count((Unlock u) => u.Earned.Value) >= 626);
						break;
					case PlatformAchievementService.AchievementType.BUY_EARTH:
					{
						Upgrade upgrade = this.upgradeService.Upgrades.FirstOrDefault((Upgrade u) => u.id == "global_8");
						if (upgrade != null)
						{
							flag = upgrade.IsPurchased.Value;
						}
						break;
					}
					case PlatformAchievementService.AchievementType.ANGEL_100K:
						flag = (GameController.Instance.AngelService.AngelsOnHand.Value >= 100000.0);
						break;
					case PlatformAchievementService.AchievementType.LETS_LEARN_BIG_NUMBERS:
						flag = (this.state.CashOnHand.Value >= 1E+33);
						break;
					case PlatformAchievementService.AchievementType.GOOGALAIRE:
						flag = (this.state.CashOnHand.Value >= 1E+100);
						break;
					case PlatformAchievementService.AchievementType.META_REFERENCE:
					{
						Upgrade upgrade2 = this.upgradeService.Upgrades.FirstOrDefault((Upgrade u) => u.id == "global_57");
						if (upgrade2 != null)
						{
							flag = upgrade2.IsPurchased.Value;
						}
						break;
					}
					case PlatformAchievementService.AchievementType.ACCURATE_DESCRIPTION:
					{
						Upgrade upgrade3 = this.upgradeService.Upgrades.FirstOrDefault((Upgrade u) => u.id == "angel_everything_c");
						if (upgrade3 != null)
						{
							flag = upgrade3.IsPurchased.Value;
						}
						break;
					}
					case PlatformAchievementService.AchievementType.LIFES_MANAGER:
					{
						if (this.upgradeService.Managers.Count((Upgrade m) => m.IsPurchased.Value) < 32)
						{
							goto IL_578;
						}
						if ((from v in this.state.VentureModels
						where v.Id != "lemon"
						select v).Sum((VentureModel v) => v.TotalOwned.Value) != 0.0)
						{
							goto IL_578;
						}
						bool flag2 = true;
						IL_57C:
						flag = flag2;
						break;
						IL_578:
						flag2 = false;
						goto IL_57C;
					}
					case PlatformAchievementService.AchievementType.TRIUMPH:
					{
						Unlock unlock = GameController.Instance.UnlockService.Unlocks.Find((Unlock u) => u.name == "Achievement");
						if (unlock != null)
						{
							flag = unlock.Earned.Value;
						}
						break;
					}
					case PlatformAchievementService.AchievementType.OMINOUS:
						flag = this.state.VentureModels.All((VentureModel v) => v.TotalOwned.Value >= 666.0);
						break;
					case PlatformAchievementService.AchievementType.CAPITALISM_CLASSIC:
						flag = (GameController.Instance.UnlockService.Unlocks.Count((Unlock u) => u.Earned.Value) > 500);
						break;
					case PlatformAchievementService.AchievementType.DELEGATION:
						flag = (this.upgradeService.Managers.Count((Upgrade m) => m.IsPurchased.Value) >= 20);
						break;
					case PlatformAchievementService.AchievementType.ONE_SMALL_STEP:
						flag = this.state.VentureModels.All((VentureModel v) => v.TotalOwned.Value >= 1.0);
						break;
					case PlatformAchievementService.AchievementType.HERE_WE_GO_AGAIN:
					{
						Upgrade upgrade4 = this.upgradeService.Upgrades.FirstOrDefault((Upgrade u) => u.id == "angel_sac_1");
						if (upgrade4 != null)
						{
							flag = upgrade4.IsPurchased.Value;
						}
						break;
					}
					case PlatformAchievementService.AchievementType.OVER_9000:
						flag = (this.state.VentureModels.FirstOrDefault((VentureModel v) => v.Id == "shoes").TotalOwned.Value >= 9000.0);
						break;
					case PlatformAchievementService.AchievementType.MOON_WALK:
					{
						if (this.upgradeService.Managers.Count((Upgrade m) => m.IsPurchased.Value) < 20)
						{
							goto IL_76D;
						}
						if ((from v in this.state.VentureModels
						where v.Id != "shoes"
						select v).Sum((VentureModel v) => v.TotalOwned.Value) != 0.0)
						{
							goto IL_76D;
						}
						bool flag3 = true;
						IL_771:
						flag = flag3;
						break;
						IL_76D:
						flag3 = false;
						goto IL_771;
					}
					case PlatformAchievementService.AchievementType.LUCKY_DUCKY:
						flag = this.state.VentureModels.All((VentureModel v) => v.TotalOwned.Value >= 777.0);
						break;
					case PlatformAchievementService.AchievementType.THAT_ACHIEVEMENTS_NAME:
						flag = GameController.Instance.UnlockService.Unlocks.Find((Unlock u) => u.name == "Special Relativity").Earned.Value;
						break;
					case PlatformAchievementService.AchievementType.MOONUMENTAL_ACHIEVEMENT:
						flag = this.state.VentureModels.All((VentureModel v) => v.TotalOwned.Value >= 1111.0);
						break;
					case PlatformAchievementService.AchievementType.DIVINE_INTERVENTION:
						flag = (GameController.Instance.AngelService.AngelsOnHand.Value > 0.0);
						break;
					case PlatformAchievementService.AchievementType.ONE_MORE:
						flag = (this.state.VentureModels.FirstOrDefault((VentureModel v) => v.Id == "laser").TotalOwned.Value >= 1112.0);
						break;
					case PlatformAchievementService.AchievementType.MOOGAL:
						flag = (this.state.CashOnHand.Value >= 1E+101);
						break;
					case PlatformAchievementService.AchievementType.TWO_DECILLION_WINGS:
						flag = (GameController.Instance.AngelService.AngelsOnHand.Value > 2E+33);
						break;
					case PlatformAchievementService.AchievementType.RELEASE_THE_HOUNDS:
						flag = GameController.Instance.game.hasProfitMartiansBeenRun.Value;
						break;
					case PlatformAchievementService.AchievementType.MARS_ATTACKS:
						flag = GameController.Instance.UnlockService.Unlocks.Find((Unlock u) => u.name == "Show Them The Meaning Of Haste").Earned.Value;
						break;
					}
					if (flag)
					{
						this.achievementPlatform.AwardAchievement(achievement, delegate(bool success)
						{
							if (success)
							{
								this.earnedAchievements.Add(this.achievementPlatform.GetAchievementID(achievement));
								this.SaveEarnedAchievements();
							}
						});
					}
				}
			}
			yield return new WaitForSeconds(10f);
		}
		yield break;
	}

	// Token: 0x06000EF2 RID: 3826 RVA: 0x00043A14 File Offset: 0x00041C14
	private void LoadEarnedAchievements()
	{
		if (PlayerPrefs.HasKey("earned_achievements"))
		{
			string @string = PlayerPrefs.GetString("earned_achievements");
			if (!string.IsNullOrEmpty(@string))
			{
				string[] collection = @string.Split(new char[]
				{
					'|'
				});
				this.earnedAchievements.AddRange(collection);
			}
		}
	}

	// Token: 0x06000EF3 RID: 3827 RVA: 0x00043A60 File Offset: 0x00041C60
	private void SaveEarnedAchievements()
	{
		if (this.earnedAchievements.Count > 0)
		{
			string text = this.earnedAchievements[0];
			for (int i = 1; i < this.earnedAchievements.Count; i++)
			{
				text = text + "|" + this.earnedAchievements[i];
			}
			PlayerPrefs.SetString("earned_achievements", text);
		}
	}

	// Token: 0x04000CD2 RID: 3282
	private const string EARNED_ACHIEVEMENTS_PLAYER_PREFS_KEY = "earned_achievements";

	// Token: 0x04000CD3 RID: 3283
	private const char EARNED_ACHIEVEMENTS_DELIMITER = '|';

	// Token: 0x04000CD4 RID: 3284
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000CD5 RID: 3285
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x04000CD6 RID: 3286
	private List<string> earnedAchievements = new List<string>();

	// Token: 0x04000CD7 RID: 3287
	private UpgradeService upgradeService;

	// Token: 0x04000CD8 RID: 3288
	private GameState state;

	// Token: 0x04000CD9 RID: 3289
	private PlatformAchievementsScriptableObject achievementObject;

	// Token: 0x04000CDA RID: 3290
	private IAchievementPlatform achievementPlatform;

	// Token: 0x04000CDB RID: 3291
	private bool enabled;

	// Token: 0x04000CDC RID: 3292
	private bool platformReady;

	// Token: 0x020008C1 RID: 2241
	public enum AchievementType
	{
		// Token: 0x04002BE8 RID: 11240
		DECILLION,
		// Token: 0x04002BE9 RID: 11241
		OFFSHORE_ACCOUNTS,
		// Token: 0x04002BEA RID: 11242
		NOVEMDECILLION,
		// Token: 0x04002BEB RID: 11243
		QUATTUORVIGINTILLION,
		// Token: 0x04002BEC RID: 11244
		DUOTRIGINTILLION,
		// Token: 0x04002BED RID: 11245
		LEMON_STANDS_100,
		// Token: 0x04002BEE RID: 11246
		OIL_RIG_1,
		// Token: 0x04002BEF RID: 11247
		EVERYTHING_100,
		// Token: 0x04002BF0 RID: 11248
		EVERYTHING_1000,
		// Token: 0x04002BF1 RID: 11249
		EVERYTHING_3000,
		// Token: 0x04002BF2 RID: 11250
		ALL_ACHIEVEMENTS,
		// Token: 0x04002BF3 RID: 11251
		BUY_EARTH,
		// Token: 0x04002BF4 RID: 11252
		ANGEL_100K,
		// Token: 0x04002BF5 RID: 11253
		LETS_LEARN_BIG_NUMBERS,
		// Token: 0x04002BF6 RID: 11254
		GOOGALAIRE,
		// Token: 0x04002BF7 RID: 11255
		META_REFERENCE,
		// Token: 0x04002BF8 RID: 11256
		ACCURATE_DESCRIPTION,
		// Token: 0x04002BF9 RID: 11257
		LIFES_MANAGER,
		// Token: 0x04002BFA RID: 11258
		TRIUMPH,
		// Token: 0x04002BFB RID: 11259
		OMINOUS,
		// Token: 0x04002BFC RID: 11260
		CAPITALISM_CLASSIC,
		// Token: 0x04002BFD RID: 11261
		DELEGATION,
		// Token: 0x04002BFE RID: 11262
		ONE_SMALL_STEP,
		// Token: 0x04002BFF RID: 11263
		HERE_WE_GO_AGAIN,
		// Token: 0x04002C00 RID: 11264
		OVER_9000,
		// Token: 0x04002C01 RID: 11265
		MOON_WALK,
		// Token: 0x04002C02 RID: 11266
		LUCKY_DUCKY,
		// Token: 0x04002C03 RID: 11267
		THAT_ACHIEVEMENTS_NAME,
		// Token: 0x04002C04 RID: 11268
		MOONUMENTAL_ACHIEVEMENT,
		// Token: 0x04002C05 RID: 11269
		DIVINE_INTERVENTION,
		// Token: 0x04002C06 RID: 11270
		ONE_MORE,
		// Token: 0x04002C07 RID: 11271
		MOOGAL,
		// Token: 0x04002C08 RID: 11272
		TWO_DECILLION_WINGS,
		// Token: 0x04002C09 RID: 11273
		GALLERY_TOUR,
		// Token: 0x04002C0A RID: 11274
		RELEASE_THE_HOUNDS,
		// Token: 0x04002C0B RID: 11275
		MARS_ATTACKS
	}
}
