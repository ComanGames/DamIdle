using System;
using UnityEngine;

namespace Platforms.Logger
{
	// Token: 0x020006DB RID: 1755
	public class UnityLogPublisher : ILogPublisher
	{
		// Token: 0x06002453 RID: 9299 RVA: 0x0009D5D5 File Offset: 0x0009B7D5
		public void SetTplus(bool enabled)
		{
			this.tplus = enabled;
			this.startTime = DateTime.UtcNow;
		}

		// Token: 0x06002454 RID: 9300 RVA: 0x0009D5EC File Offset: 0x0009B7EC
		public void Publish(LogEntry log)
		{
			string message;
			if (this.tplus)
			{
				message = string.Format("[{0:F2}] [{1}]: {2}", (DateTime.UtcNow - this.startTime).TotalSeconds, log.LoggerName, log.Message);
			}
			else
			{
				message = string.Format("[{0}]: {1}", log.LoggerName, log.Message);
			}
			LogLevel logLevel = log.LogLevel;
			if (logLevel == LogLevel.Warning)
			{
				Debug.LogWarning(message);
				return;
			}
			if (logLevel == LogLevel.Error)
			{
				Debug.LogError(message);
				return;
			}
			Debug.Log(message);
		}

		// Token: 0x0400250E RID: 9486
		private bool tplus;

		// Token: 0x0400250F RID: 9487
		private DateTime startTime;
	}
}
