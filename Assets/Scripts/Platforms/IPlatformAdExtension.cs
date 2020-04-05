using System;
using System.Collections.Generic;
using Platforms.Ad;
using UniRx;

namespace Platforms
{
	// Token: 0x020006C5 RID: 1733
	public interface IPlatformAdExtension : IPlatform, IDisposable
	{
		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x0600232D RID: 9005
		Dictionary<AdType, int> AdTypePriority { get; }

		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x0600232E RID: 9006
		Dictionary<AdType, ReactiveProperty<bool>> AdReady { get; }

		// Token: 0x0600232F RID: 9007
		IPlatformAdExtension InitPlatform(PlatformAd platformAd);

		// Token: 0x06002330 RID: 9008
		IObservable<ShowResult> ShowAd(AdType type, string placementId);
	}
}
