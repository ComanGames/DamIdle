using System;
using System.Collections.Generic;
using AdCap.Store;
using UserTargeting.Segments;
using UserTargeting.TestGroups;

// Token: 0x02000027 RID: 39
public class InventoryJsonDataObject
{
	// Token: 0x040000A4 RID: 164
	public List<ItemSlot> ItemSlotInfo = new List<ItemSlot>();

	// Token: 0x040000A5 RID: 165
	public List<LegacySaveDataConversion> LegacySaveDataConversion = new List<LegacySaveDataConversion>();

	// Token: 0x040000A6 RID: 166
	public List<LegacyInventorySaveDataConvertion> LegacyInventorySaveDataConvertion = new List<LegacyInventorySaveDataConvertion>();

	// Token: 0x040000A7 RID: 167
	public List<Item> Items = new List<Item>();

	// Token: 0x040000A8 RID: 168
	public List<AdCapStoreItem> StoreItems = new List<AdCapStoreItem>();

	// Token: 0x040000A9 RID: 169
	public List<ColourData> AdCapColourInfo = new List<ColourData>();

	// Token: 0x040000AA RID: 170
	public List<AdCapString> AdCapStrings = new List<AdCapString>();

	// Token: 0x040000AB RID: 171
	public List<AdCapHardResetReward> AdCapHardResetRewards = new List<AdCapHardResetReward>();

	// Token: 0x040000AC RID: 172
	public List<QuickButtonController.QuickButtonConfig> QuickButtonConfigs = new List<QuickButtonController.QuickButtonConfig>();

	// Token: 0x040000AD RID: 173
	public List<AppOnboardConfig> AppOnboardConfig;

	// Token: 0x040000AE RID: 174
	public List<GeneralConfig> GeneralConfig = new List<GeneralConfig>();

	// Token: 0x040000AF RID: 175
	public List<OfferwallConfig> OfferwallConfig = new List<OfferwallConfig>();

	// Token: 0x040000B0 RID: 176
	public List<GiftService.GiftData> GiftConfig = new List<GiftService.GiftData>();

	// Token: 0x040000B1 RID: 177
	public List<TutorialStep> TutorialSteps = new List<TutorialStep>();

	// Token: 0x040000B2 RID: 178
	public List<TutorialBlock> TutorialConfig = new List<TutorialBlock>();

	// Token: 0x040000B3 RID: 179
	public List<UnfoldingData> UnfoldingConfig = new List<UnfoldingData>();

	// Token: 0x040000B4 RID: 180
	public List<FreeloaderConfig> FreeloaderConfig = new List<FreeloaderConfig>();

	// Token: 0x040000B5 RID: 181
	public List<ScheduledOffer> ScheduledOffers = new List<ScheduledOffer>();

	// Token: 0x040000B6 RID: 182
	public List<FirstTimeBuyerGroup> FirstTimeBuyerGroups = new List<FirstTimeBuyerGroup>();

	// Token: 0x040000B7 RID: 183
	public SegmentData[] Segments;

	// Token: 0x040000B8 RID: 184
	public TestGroupData[] TestGroups;

	// Token: 0x040000B9 RID: 185
	public Dictionary<string, bool> FeatureConfig;
}
