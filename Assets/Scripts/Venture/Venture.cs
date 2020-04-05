using System;
using LitJson;

// Token: 0x0200015D RID: 349
[Serializable]
public sealed class Venture
{
	// Token: 0x170000FB RID: 251
	// (get) Token: 0x06000B06 RID: 2822 RVA: 0x000316C5 File Offset: 0x0002F8C5
	public string imageName
	{
		get
		{
			return this.id;
		}
	}

	// Token: 0x170000FC RID: 252
	// (get) Token: 0x06000B07 RID: 2823 RVA: 0x000316CD File Offset: 0x0002F8CD
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public double Progress
	{
		get
		{
			return this.currentTime / this.cooldownTime;
		}
	}

	// Token: 0x170000FD RID: 253
	// (get) Token: 0x06000B08 RID: 2824 RVA: 0x000316DC File Offset: 0x0002F8DC
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public double CostPer
	{
		get
		{
			return this.costPer - this.costPer * this.accountantEffect;
		}
	}

	// Token: 0x06000B09 RID: 2825 RVA: 0x000316F4 File Offset: 0x0002F8F4
	public void Copy(Venture other)
	{
		this.baseAmount = other.baseAmount;
		this.profitPer = other.profitPer;
		this.costPer = other.costPer;
		this.cooldownTime = other.cooldownTime;
		this.currentTime = other.currentTime;
		this.expenseRate = other.expenseRate;
		this.accountantEffect = other.accountantEffect;
	}

	// Token: 0x04000942 RID: 2370
	public string id;

	// Token: 0x04000943 RID: 2371
	public string name;

	// Token: 0x04000944 RID: 2372
	public string plural;

	// Token: 0x04000945 RID: 2373
	public string bonusName;

	// Token: 0x04000946 RID: 2374
	[JsonAlias("amount", false)]
	public int baseAmount;

	// Token: 0x04000947 RID: 2375
	public double profitPer;

	// Token: 0x04000948 RID: 2376
	public double costPer;

	// Token: 0x04000949 RID: 2377
	public double cooldownTime;

	// Token: 0x0400094A RID: 2378
	public double currentTime;

	// Token: 0x0400094B RID: 2379
	public double expenseRate;

	// Token: 0x0400094C RID: 2380
	public double accountantEffect;
}
