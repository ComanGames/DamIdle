using System;
using LitJson;
using UniRx;

// Token: 0x02000113 RID: 275
public abstract class Upgrade
{
	// Token: 0x1700008B RID: 139
	// (get) Token: 0x0600073D RID: 1853
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public abstract string Effect { get; }

	// Token: 0x0600073E RID: 1854
	public abstract void Apply(GameState state);

	// Token: 0x0600073F RID: 1855
	public abstract void Copy(object other);

	// Token: 0x06000740 RID: 1856 RVA: 0x000268C6 File Offset: 0x00024AC6
	public void Reset()
	{
		this.IsPurchased.Value = false;
		this.IsPurchaseable.Value = false;
	}

	// Token: 0x06000741 RID: 1857 RVA: 0x000268E0 File Offset: 0x00024AE0
	public void LoadSaveData(UpgradeSaveData data)
	{
		if (data != null)
		{
			this.IsPurchased.Value = data.purchased;
		}
	}

	// Token: 0x06000742 RID: 1858 RVA: 0x000268F6 File Offset: 0x00024AF6
	public UpgradeSaveData Save()
	{
		return new UpgradeSaveData(this.id, this.IsPurchased.Value);
	}

	// Token: 0x040006BE RID: 1726
	public string id;

	// Token: 0x040006BF RID: 1727
	public string name;

	// Token: 0x040006C0 RID: 1728
	public double cost;

	// Token: 0x040006C1 RID: 1729
	public readonly ReactiveProperty<bool> IsPurchased = new ReactiveProperty<bool>(false);

	// Token: 0x040006C2 RID: 1730
	public string imageName;

	// Token: 0x040006C3 RID: 1731
	public Upgrade.Currency currency;

	// Token: 0x040006C4 RID: 1732
	public float order = -1f;

	// Token: 0x040006C5 RID: 1733
	public bool isKongPlateau;

	// Token: 0x040006C6 RID: 1734
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public readonly ReactiveProperty<bool> IsPurchaseable = new ReactiveProperty<bool>();

	// Token: 0x02000810 RID: 2064
	public enum Currency
	{
		// Token: 0x04002985 RID: 10629
		InGameCash,
		// Token: 0x04002986 RID: 10630
		AngelInvestors,
		// Token: 0x04002987 RID: 10631
		Megabucks
	}
}
