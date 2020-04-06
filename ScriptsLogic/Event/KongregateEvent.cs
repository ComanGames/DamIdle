using System;

// Token: 0x020001ED RID: 493
public class KongregateEvent
{
	// Token: 0x04000C22 RID: 3106
	public KongregateEvent.EventType Event;

	// Token: 0x04000C23 RID: 3107
	public string Data;

	// Token: 0x020008B3 RID: 2227
	public enum EventType
	{
		// Token: 0x04002BB2 RID: 11186
		SetUserItems,
		// Token: 0x04002BB3 RID: 11187
		LogMessage,
		// Token: 0x04002BB4 RID: 11188
		PurchaseSucceeded,
		// Token: 0x04002BB5 RID: 11189
		PurchaseFailed,
		// Token: 0x04002BB6 RID: 11190
		KongregateUserSignedIn,
		// Token: 0x04002BB7 RID: 11191
		KongregateAPILoaded
	}
}
