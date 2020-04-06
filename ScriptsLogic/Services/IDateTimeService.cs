using System;
using Platforms;
using UniRx;

// Token: 0x020000A5 RID: 165
public interface IDateTimeService : IDisposable
{
	// Token: 0x1700006D RID: 109
	// (get) Token: 0x0600047E RID: 1150
	bool IsStarted { get; }

	// Token: 0x1700006E RID: 110
	// (get) Token: 0x0600047F RID: 1151
	DateTime UtcNow { get; }

	// Token: 0x1700006F RID: 111
	// (get) Token: 0x06000480 RID: 1152
	PlatformAccount PlatformAccount { get; }

	// Token: 0x06000481 RID: 1153
	void Init(PlatformAccount pAccount);

	// Token: 0x06000482 RID: 1154
	// Token: 0x06000483 RID: 1155
	void SetNow(DateTime now);

	// Token: 0x06000484 RID: 1156
	void Start(Action onComplete, Action onError);
}
