using System;
using System.Collections.Generic;

namespace AdCap.Store
{
	// Token: 0x020006F2 RID: 1778
	public struct RewardsGrantedEvent
	{
		// Token: 0x060024F5 RID: 9461 RVA: 0x000A0719 File Offset: 0x0009E919
		public RewardsGrantedEvent(List<RewardData> rewards, string source, string context)
		{
			this.Rewards = rewards;
			this.Source = source;
			this.Context = context;
		}

		// Token: 0x060024F6 RID: 9462 RVA: 0x000A0730 File Offset: 0x0009E930
		public RewardsGrantedEvent(RewardData reward, string source, string context)
		{
			this.Rewards = new List<RewardData>
			{
				reward
			};
			this.Source = source;
			this.Context = context;
		}

		// Token: 0x040025B0 RID: 9648
		public List<RewardData> Rewards;

		// Token: 0x040025B1 RID: 9649
		public string Source;

		// Token: 0x040025B2 RID: 9650
		public string Context;
	}
}
