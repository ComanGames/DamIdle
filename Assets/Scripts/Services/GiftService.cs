using System;
using System.Collections.Generic;
using System.Linq;
using AdCap;
using Platforms.Logger;
using UniRx;

// Token: 0x0200009F RID: 159
public class GiftService : IDisposable
{
	// Token: 0x06000457 RID: 1111 RVA: 0x00017788 File Offset: 0x00015988
	public void Init(IGameController gameController, DataService dataService, IUserDataService userDataService, ITriggerService triggerService, PlayerData playerData, IGrantRewardService grantRewardService)
	{
		this.logger = Logger.GetLogger(this);
		this.logger.Info("Initializing....");
		this.dataService = dataService;
		this.triggerService = triggerService;
		this.playerData = playerData;
		this.grantRewardService = grantRewardService;
		this.LoadClaimedGifts();
		IEnumerable<GiftService.GiftData> source = from x in this.dataService.ExternalData.GiftConfig
		where !this.ClaimedGiftIds.Contains(x.Id)
		select x;
		source = from x in source
		where x.Platforms.Contains(gameController.PlatformId)
		select x;
		source = from x in source
		where string.IsNullOrEmpty(x.ABTestGroup) || userDataService.IsTestGroupMember(x.ABTestGroup)
		select x;
		this.giftDataMap = source.ToDictionary((GiftService.GiftData x) => x.Id, (GiftService.GiftData y) => y);
		this.MonitorGiftTriggers();
		this.logger.Info("Initialized");
	}

	// Token: 0x06000458 RID: 1112 RVA: 0x00017899 File Offset: 0x00015A99
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000459 RID: 1113 RVA: 0x000178A8 File Offset: 0x00015AA8
	public bool ClaimGift(GiftService.GiftData gift)
	{
		bool result = false;
		if (!this.ClaimedGiftIds.Contains(gift.Id))
		{
			List<RewardData> list = this.grantRewardService.GrantRewards(gift.Rewards, "Gift", gift.Id, false);
			if (list != null && list.Count == gift.Rewards.Count)
			{
				this.ClaimedGiftIds.Add(gift.Id);
				this.AvailableGifts.Remove(gift);
				this.SaveClaimedIds();
				result = true;
			}
			else
			{
				this.logger.Error("Gift [" + gift.Id + "] failed granting the rewards");
			}
		}
		else
		{
			this.logger.Error("Gift [" + gift.Id + "] has already been claimed");
		}
		return result;
	}

	// Token: 0x0600045A RID: 1114 RVA: 0x0001796C File Offset: 0x00015B6C
	private void MonitorGiftTriggers()
	{
		foreach (KeyValuePair<string, GiftService.GiftData> keyValuePair in this.giftDataMap)
		{
			GiftService.GiftData gift = keyValuePair.Value;
			this.triggerService.MonitorTriggers(gift.TriggerDatas, false).First((bool x) => x).Subscribe(delegate(bool _)
			{
				this.OnGiftAvailable(gift);
			}).AddTo(this.disposables);
		}
	}

	// Token: 0x0600045B RID: 1115 RVA: 0x00017A30 File Offset: 0x00015C30
	private void OnGiftAvailable(GiftService.GiftData giftData)
	{
		this.AvailableGifts.Add(giftData);
	}

	// Token: 0x0600045C RID: 1116 RVA: 0x00017A40 File Offset: 0x00015C40
	private void LoadClaimedGifts()
	{
		string text = this.playerData.Get("CLAIMED_GIFTS", "");
		if (!string.IsNullOrEmpty(text))
		{
			foreach (string item in text.Split(new char[]
			{
				','
			}))
			{
				this.ClaimedGiftIds.Add(item);
			}
		}
	}

	// Token: 0x0600045D RID: 1117 RVA: 0x00017A9C File Offset: 0x00015C9C
	private void SaveClaimedIds()
	{
		string value = string.Join(",", this.ClaimedGiftIds.ToArray<string>());
		this.playerData.Set("CLAIMED_GIFTS", value);
	}

	// Token: 0x040003D7 RID: 983
	public ReactiveCollection<GiftService.GiftData> AvailableGifts = new ReactiveCollection<GiftService.GiftData>();

	// Token: 0x040003D8 RID: 984
	public readonly ReactiveCollection<string> ClaimedGiftIds = new ReactiveCollection<string>();

	// Token: 0x040003D9 RID: 985
	private Dictionary<string, GiftService.GiftData> giftDataMap = new Dictionary<string, GiftService.GiftData>();

	// Token: 0x040003DA RID: 986
	private DataService dataService;

	// Token: 0x040003DB RID: 987
	private ITriggerService triggerService;

	// Token: 0x040003DC RID: 988
	private PlayerData playerData;

	// Token: 0x040003DD RID: 989
	private IGrantRewardService grantRewardService;

	// Token: 0x040003DE RID: 990
	private const string CLAIMED_GIFTS_KEY = "CLAIMED_GIFTS";

	// Token: 0x040003DF RID: 991
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040003E0 RID: 992
	private Logger logger;

	// Token: 0x02000788 RID: 1928
	[Serializable]
	public class GiftData
	{
		// Token: 0x04002800 RID: 10240
		public string Id;

		// Token: 0x04002801 RID: 10241
		public string Title;

		// Token: 0x04002802 RID: 10242
		public string Description;

		// Token: 0x04002803 RID: 10243
		public string ABTestGroup;

		// Token: 0x04002804 RID: 10244
		public string Platforms;

		// Token: 0x04002805 RID: 10245
		public string PlanetToAwardOn;

		// Token: 0x04002806 RID: 10246
		public List<TriggerData> TriggerDatas = new List<TriggerData>();

		// Token: 0x04002807 RID: 10247
		public List<RewardData> Rewards = new List<RewardData>();
	}
}
