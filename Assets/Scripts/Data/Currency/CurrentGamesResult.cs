using System;
using System.Collections.Generic;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels
{
	// Token: 0x0200050F RID: 1295
	[Serializable]
	public class CurrentGamesResult : PlayFabResultCommon
	{
		// Token: 0x04001CFF RID: 7423
		public int GameCount;

		// Token: 0x04001D00 RID: 7424
		public List<GameInfo> Games;

		// Token: 0x04001D01 RID: 7425
		public int PlayerCount;
	}
}
