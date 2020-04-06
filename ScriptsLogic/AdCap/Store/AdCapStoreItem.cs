using System;
using System.Collections.Generic;
using UniRx;

namespace AdCap.Store
{
	// Token: 0x020006F0 RID: 1776
	public class AdCapStoreItem
	{
		// Token: 0x1700030D RID: 781
		// (get) Token: 0x060024EA RID: 9450 RVA: 0x000A05B9 File Offset: 0x0009E7B9
		public string AppleId
		{
			get
			{
				return "com.kongregate.mobile.adventurecapitalist." + this.Id;
			}
		}

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x060024EB RID: 9451 RVA: 0x000A05CB File Offset: 0x0009E7CB
		public string AndroidId
		{
			get
			{
				return "com.kongregate.mobile.adventurecapitalist.google." + this.Id;
			}
		}

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x060024EC RID: 9452 RVA: 0x000A05DD File Offset: 0x0009E7DD
		public string SteamId
		{
			get
			{
				return "steam." + this.Id;
			}
		}

		// Token: 0x17000310 RID: 784
		// (get) Token: 0x060024ED RID: 9453 RVA: 0x000A05EF File Offset: 0x0009E7EF
		public string PlatformId
		{
			get
			{
				return this.SteamId;
			}
		}

		// Token: 0x04002592 RID: 9618
		public const string APPLE_ID = "com.kongregate.mobile.adventurecapitalist.";

		// Token: 0x04002593 RID: 9619
		public const string ANDROID_ID = "com.kongregate.mobile.adventurecapitalist.google.";

		// Token: 0x04002594 RID: 9620
		public const string STEAM_ID = "steam.";

		// Token: 0x04002595 RID: 9621
		public const string LOCALIZED_PRICE_LOADING_ERROR_STRING = "Loading";

		// Token: 0x04002596 RID: 9622
		public string Id;

		// Token: 0x04002597 RID: 9623
		public string DisplayName;

		// Token: 0x04002598 RID: 9624
		public string ABTestGroup;

		// Token: 0x04002599 RID: 9625
		public bool Active;

		// Token: 0x0400259A RID: 9626
		public int DisplayPriority;

		// Token: 0x0400259B RID: 9627
		public string Platforms;

		// Token: 0x0400259C RID: 9628
		public string Description;

		// Token: 0x0400259D RID: 9629
		public string IconName;

		// Token: 0x0400259E RID: 9630
		public DateTime StartDateUtc = DateTime.MinValue;

		// Token: 0x0400259F RID: 9631
		public DateTime EndDateUtc = DateTime.MinValue;

		// Token: 0x040025A0 RID: 9632
		public double Duration;

		// Token: 0x040025A1 RID: 9633
		public int PurchaseCount;

		// Token: 0x040025A2 RID: 9634
		public ReactiveProperty<double> TimeRemaining = new DoubleReactiveProperty();

		// Token: 0x040025A3 RID: 9635
		public DateTime ExpiryDate;

		// Token: 0x040025A4 RID: 9636
		public string StoreId;

		// Token: 0x040025A5 RID: 9637
		public EStoreItemType StoreItemType;

		// Token: 0x040025A6 RID: 9638
		public Cost Cost;


		// Token: 0x040025A8 RID: 9640
		public List<TriggerData> TriggerDatas = new List<TriggerData>();

		// Token: 0x040025A9 RID: 9641
		public List<TriggerData> ExpiryTriggerDatas = new List<TriggerData>();

		// Token: 0x040025AA RID: 9642
		public List<RewardData> Rewards = new List<RewardData>();

		// Token: 0x040025AB RID: 9643
		public string BannerText;
	}
}
