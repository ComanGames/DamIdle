using System;

namespace Platforms.Logger
{
	// Token: 0x020006DA RID: 1754
	public interface ILogPublisher
	{
		// Token: 0x06002451 RID: 9297
		void Publish(LogEntry log);

		// Token: 0x06002452 RID: 9298
		void SetTplus(bool tplus);
	}
}
