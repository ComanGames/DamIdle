using System;
using System.Collections.Generic;
using Platforms.Ad;
using UniRx;

namespace Platforms
{
	// Token: 0x020006CA RID: 1738
	public interface IPlatformAd : IDisposable
	{
		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x0600237B RID: 9083
		ReplaySubject<int> OfferWallCreditsReceived { get; }

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x0600237C RID: 9084
		Dictionary<AdType, ReactiveProperty<bool>> AdReadyMap { get; }

		// Token: 0x0600237D RID: 9085
		IObservable<ShowResult> ShowAd(AdType type, string placementId = "");
	}
}
