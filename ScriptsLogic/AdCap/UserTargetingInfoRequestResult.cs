using System;
using System.Collections.Generic;
using PlayFab.ClientModels;

namespace AdCap
{
	// Token: 0x020006E9 RID: 1769
	public class UserTargetingInfoRequestResult
	{
		// Token: 0x04002567 RID: 9575
		public string ErrorMessage;

		// Token: 0x04002568 RID: 9576
		public GetSegmentResult[] Segments;

		// Token: 0x04002569 RID: 9577
		public Dictionary<string, string> TestGroups;
	}
}
