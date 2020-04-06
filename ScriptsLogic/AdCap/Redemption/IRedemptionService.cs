using System;
using UniRx;

namespace AdCap.Redemption
{
	// Token: 0x020006FB RID: 1787
	public interface IRedemptionService
	{
		// Token: 0x17000312 RID: 786
		// (get) Token: 0x060024FB RID: 9467
		Subject<RedemptionEvent> RedemptionEventCallback { get; }

		// Token: 0x060024FC RID: 9468
		void SubmitCode(string code, Action<string> onSuccess, Action<string> onFail);
	}
}
