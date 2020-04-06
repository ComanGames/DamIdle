using System;

namespace AdCap.Redemption
{
	// Token: 0x020006FD RID: 1789
	public struct RedemptionEvent
	{
		// Token: 0x040025FF RID: 9727
		public RedemptionEventType Type;

		// Token: 0x04002600 RID: 9728
		public bool Success;

		// Token: 0x04002601 RID: 9729
		public RedemptionItem Item;

		// Token: 0x04002602 RID: 9730
		public string ErrorMessage;
	}
}
