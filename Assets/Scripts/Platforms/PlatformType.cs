using System;

namespace Platforms
{
	// Token: 0x020006C1 RID: 1729
	[Flags]
	public enum PlatformType
	{
		// Token: 0x0400248F RID: 9359
		NA = 0,
		// Token: 0x04002490 RID: 9360
		Android = 1,
		// Token: 0x04002491 RID: 9361
		Ios = 2,
		// Token: 0x04002492 RID: 9362
		Steam = 4,
		// Token: 0x04002493 RID: 9363
		Facebook = 8,
		// Token: 0x04002494 RID: 9364
		FacebookGameroom = 16,
		// Token: 0x04002495 RID: 9365
		Kongregate = 32,
		// Token: 0x04002496 RID: 9366
		XboxOne = 64,
		// Token: 0x04002497 RID: 9367
		PlayStation4 = 128
	}
}
