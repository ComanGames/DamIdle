using System;
using System.Linq;
using AdCap.Store;
using LitJson;
using UniRx;
using UnityEngine;

// Token: 0x0200002E RID: 46
public class Item : IDisposable
{
	// Token: 0x060000C6 RID: 198 RVA: 0x00005794 File Offset: 0x00003994
	public Item()
	{
		this.disposable = this.Owned.Pairwise<int>().Subscribe(delegate(Pair<int> pair)
		{
			this.PreviousOwned = pair.Previous;
		});
		this.Owned.Subscribe(delegate(int _)
		{
			this.CalculateLeveledBonus();
		});
	}

	// Token: 0x060000C7 RID: 199 RVA: 0x00005804 File Offset: 0x00003A04
	~Item()
	{
		this.Dispose();
	}

	// Token: 0x060000C8 RID: 200 RVA: 0x00005830 File Offset: 0x00003A30
	public void Dispose()
	{
		this.disposable.Dispose();
	}

	// Token: 0x060000C9 RID: 201 RVA: 0x0000583D File Offset: 0x00003A3D
	public float GetLeveledBonus(int addedLevels = 0)
	{
		return (this.BonusAmount + this.BonusGrowthRate * (float)(this.Owned.Value + addedLevels - 1)) * this.SuitSetBonusToItem;
	}

	// Token: 0x060000CA RID: 202 RVA: 0x00005864 File Offset: 0x00003A64
	public void CalculateLeveledBonus()
	{
		this.CurrentLeveledBonus.Value = (this.BonusAmount + this.BonusGrowthRate * (float)(this.Owned.Value - 1)) * this.SuitSetBonusToItem;
	}

	// Token: 0x060000CB RID: 203 RVA: 0x00005894 File Offset: 0x00003A94
	public string GetFilledItemDescription(int addedLevels = 0, string color = "")
	{
		if (string.IsNullOrEmpty(this.Description))
		{
			Debug.LogWarning("Item [" + this.ItemId + "] is missing a description");
			return "";
		}
		string text = this.Description.Replace("/n", "\n");
		if (this.ItemType == ItemType.SuitSet)
		{
			if (!this.IsPercentage)
			{
				return string.Format(text, this.GetLeveledBonusAsMultiplierString(addedLevels));
			}
			return string.Format(text, this.GetLeveledBonusAsPercentageString(addedLevels));
		}
		else
		{
			if (this.ItemType == ItemType.Trophy)
			{
				return text;
			}
			switch (this.ItemBonusType)
			{
			case ItemBonusType.VentureProfitBoost:
			case ItemBonusType.VentureSpeedBoost:
			case ItemBonusType.AngelEffectivenessPercent:
			case ItemBonusType.AngelClaim:
			case ItemBonusType.BoosterRecharge:
			case ItemBonusType.BoosterBonus:
			case ItemBonusType.ProfitAdBonus:
			case ItemBonusType.GoldBonus:
			case ItemBonusType.MegaBucks:
				if (!this.IsPercentage)
				{
					return string.Format(text, this.GetLeveledBonusAsMultiplierString(addedLevels));
				}
				return string.Format(text, this.GetLeveledBonusAsPercentageString(addedLevels));
			case ItemBonusType.InstantProfit:
			{
				double bonusData;
				if (double.TryParse(this.BonusCustomData, out bonusData))
				{
					return this.GetInstantProfitString(text, bonusData);
				}
				break;
			}
			case ItemBonusType.VentureRebate:
				return string.Format(text, this.GetRebateValueString(addedLevels));
			}
			return text;
		}
	}

	// Token: 0x060000CC RID: 204 RVA: 0x000059AA File Offset: 0x00003BAA
	private string GetLeveledBonusAsPercentageString(int addedLevels)
	{
		return this.GetLeveledBonus(addedLevels) * 100f + "%";
	}

	// Token: 0x060000CD RID: 205 RVA: 0x000059C8 File Offset: 0x00003BC8
	private string GetLeveledBonusAsMultiplierString(int addedLevels)
	{
		return this.GetLeveledBonus(addedLevels) + "x";
	}

	// Token: 0x060000CE RID: 206 RVA: 0x000059E0 File Offset: 0x00003BE0
	private string GetInstantProfitString(string preparedDescription, double bonusData)
	{
		double elapsed = bonusData * 60.0 * 60.0;
		double bonusTimeTotal = GameController.Instance.game.CalculateElapsed(elapsed);
		double num = GameController.Instance.game.VentureModels.Sum(v => v.ProfitSurgeAmount(bonusTimeTotal, false));
		if (num == 0.0)
		{
			return this.StoreDescription.Replace("/n", "\n");
		}
		if (!preparedDescription.Contains("{1}"))
		{
			return string.Format(preparedDescription, "$" + NumberFormat.Convert(num, 1000000.0, true, 3));
		}
		return string.Format(preparedDescription, "$" + NumberFormat.Convert(num, 1000000.0, true, 3), this.BonusCustomData);
	}

	// Token: 0x060000CF RID: 207 RVA: 0x00005AB8 File Offset: 0x00003CB8
	private string GetVentureTargetName()
	{
		if (string.IsNullOrEmpty(this.BonusCustomData))
		{
			return "Everything";
		}
		string[] splitString = this.BonusCustomData.Split(new char[]
		{
			':'
		});
		int num = splitString.Length;
		if (num != 1)
		{
			if (num == 2)
			{
				VentureModel ventureModel = GameController.Instance.game.VentureModels.FirstOrDefault(x => x.Id == splitString[1]);
				if (ventureModel != null)
				{
					return ventureModel.Name;
				}
			}
			return "";
		}
		return this.BonusCustomData;
	}

	// Token: 0x060000D0 RID: 208 RVA: 0x00005B44 File Offset: 0x00003D44
	private string GetRebateValueString(int addedLevels)
	{
		string text = Math.Round((double)((1f - 1f / (1f + this.GetLeveledBonus(addedLevels))) * 100f), 4).ToString();
		if (text.IndexOf('.') == -1)
		{
			return text;
		}
		int num = text.LastIndexOf('9');
		if (num == -1)
		{
			return text;
		}
		int length = Mathf.Min(num + 2, text.Length);
		text = text.Substring(0, length);
		if (!this.IsPercentage)
		{
			return "x" + text;
		}
		return text + "%";
	}

	// Token: 0x060000D1 RID: 209 RVA: 0x00005BD3 File Offset: 0x00003DD3
	public string GetNameString()
	{
		if (this.ItemType != ItemType.Currency)
		{
			return this.ItemName;
		}
		return string.Format("{0} Gold", this.Owned.Value);
	}

	// Token: 0x060000D2 RID: 210 RVA: 0x00005BFF File Offset: 0x00003DFF
	public bool IsEquipable()
	{
		return this.ItemType == ItemType.Badge || this.ItemType == ItemType.Head || this.ItemType == ItemType.Pants || this.ItemType == ItemType.Shirt;
	}

	// Token: 0x060000D3 RID: 211 RVA: 0x00005C26 File Offset: 0x00003E26
	public string GetPathToImage(Gender gender)
	{
		if (gender == Gender.male)
		{
			return "CapitalistMale/" + this.ImageName;
		}
		return "CapitalistFemale/" + this.ImageName;
	}

	// Token: 0x060000D4 RID: 212 RVA: 0x00005C4C File Offset: 0x00003E4C
	public string GetPathToIcon()
	{
		if (this.ItemType == ItemType.Currency)
		{
			return this.GetPathToGoldIcon();
		}
		return "ItemIcons/" + this.IconName;
	}

	// Token: 0x060000D5 RID: 213 RVA: 0x00005C70 File Offset: 0x00003E70
	private string GetPathToGoldIcon()
	{
		string arg = "1";
		if (this.Owned.Value >= 1300)
		{
			arg = "5";
		}
		else if (this.Owned.Value >= 625)
		{
			arg = "4";
		}
		else if (this.Owned.Value >= 240)
		{
			arg = "3";
		}
		else if (this.Owned.Value >= 115)
		{
			arg = "2";
		}
		return string.Format("ItemIcons/{0}{1}", this.IconName, arg);
	}

	// Token: 0x060000D6 RID: 214 RVA: 0x00005CF7 File Offset: 0x00003EF7
	public Sprite GetBGIcon()
	{
		return Resources.Load<Sprite>(string.Format("ItemBackgrounds/bg-container-item{0}", this.IsEquipable() ? "" : "-empty"));
	}

	// Token: 0x060000D7 RID: 215 RVA: 0x00005D1C File Offset: 0x00003F1C
	public string GetRarityName()
	{
		return AdCapStrings.GetStringByKey("Rarity" + this.RarityRank);
	}

	// Token: 0x040000CA RID: 202
	public string ItemId;

	// Token: 0x040000CB RID: 203
	public string ItemName;

	// Token: 0x040000CC RID: 204
	public string ItemSetId;

	// Token: 0x040000CD RID: 205
	public string Platforms;

	// Token: 0x040000CE RID: 206
	public string ABTestGroup;

	// Token: 0x040000CF RID: 207
	public ItemType ItemType;

	// Token: 0x040000D0 RID: 208
	public int RarityValue;

	// Token: 0x040000D1 RID: 209
	public int RarityRank;

	// Token: 0x040000D2 RID: 210
	public Product Product;

	// Token: 0x040000D3 RID: 211
	public int MaxLevel;

	// Token: 0x040000D4 RID: 212
	public ItemBonusType ItemBonusType;

	// Token: 0x040000D5 RID: 213
	public ItemBonusTarget ItemBonusTarget;

	// Token: 0x040000D6 RID: 214
	public float BonusAmount;

	// Token: 0x040000D7 RID: 215
	public float BonusGrowthRate;

	// Token: 0x040000D8 RID: 216
	public string BonusCustomData;

	// Token: 0x040000D9 RID: 217
	public int Duration;

	// Token: 0x040000DA RID: 218
	public string IconName;

	// Token: 0x040000DB RID: 219
	public string ImageName;

	// Token: 0x040000DC RID: 220
	public string Description;

	// Token: 0x040000DD RID: 221
	public string StoreDescription;

	// Token: 0x040000DE RID: 222
	public bool IsPercentage;

	// Token: 0x040000DF RID: 223
	public bool IsEquipped;

	// Token: 0x040000E0 RID: 224
	public bool NewItem;

	// Token: 0x040000E1 RID: 225
	public bool IsNudeEquipment;

	// Token: 0x040000E2 RID: 226
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public float SuitSetBonusToItem = 1f;

	// Token: 0x040000E3 RID: 227
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public int PreviousOwned;

	// Token: 0x040000E4 RID: 228
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public readonly ReactiveProperty<int> Owned = new ReactiveProperty<int>();

	// Token: 0x040000E5 RID: 229
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public readonly ReactiveProperty<float> CurrentLeveledBonus = new ReactiveProperty<float>();

	// Token: 0x040000E6 RID: 230
	private IDisposable disposable;
}
