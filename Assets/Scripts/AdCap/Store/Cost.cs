using System;

namespace AdCap.Store
{
	// Token: 0x020006F1 RID: 1777
	[Serializable]
	public sealed class Cost
	{
		// Token: 0x060024EF RID: 9455 RVA: 0x00002EDA File Offset: 0x000010DA
		public Cost()
		{
		}

		// Token: 0x060024F0 RID: 9456 RVA: 0x000A064D File Offset: 0x0009E84D
		public Cost(double price, Currency currency) : this(price, currency, 0.0)
		{
		}

		// Token: 0x060024F1 RID: 9457 RVA: 0x000A0660 File Offset: 0x0009E860
		public Cost(double price, Currency currency, double discount)
		{
			this.Price = price;
			this.Currency = currency;
			this.Discount = discount;
		}

		// Token: 0x060024F2 RID: 9458 RVA: 0x000A0680 File Offset: 0x0009E880
		public string GetCurrentCost()
		{
			switch (this.Currency)
			{
			case Currency.Gold:
			case Currency.MegaBuck:
				return (this.IsSaleActive ? this.Discount : this.Price).ToString();
			case Currency.Cash:
				return this.LocalizedPriceString;
			}
			return string.Empty;
		}

		// Token: 0x060024F3 RID: 9459 RVA: 0x000A06DF File Offset: 0x0009E8DF
		public double GetCurrentCostAsDouble()
		{
			if (!this.IsSaleActive)
			{
				return this.Price;
			}
			return this.Discount;
		}

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x060024F4 RID: 9460 RVA: 0x000A06F6 File Offset: 0x0009E8F6
		public bool IsSaleActive
		{
			get
			{
				return this.Discount > 0.0 && this.Discount < this.Price;
			}
		}

		// Token: 0x040025AC RID: 9644
		public double Price;

		// Token: 0x040025AD RID: 9645
		public Currency Currency;

		// Token: 0x040025AE RID: 9646
		public double Discount;

		// Token: 0x040025AF RID: 9647
		public string LocalizedPriceString;
	}
}
