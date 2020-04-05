using System;

namespace Platforms.Logger
{
	// Token: 0x020006D9 RID: 1753
	public struct LogEntry
	{
		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x06002446 RID: 9286 RVA: 0x0009D552 File Offset: 0x0009B752
		// (set) Token: 0x06002447 RID: 9287 RVA: 0x0009D55A File Offset: 0x0009B75A
		public string LoggerName { get; private set; }

		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x06002448 RID: 9288 RVA: 0x0009D563 File Offset: 0x0009B763
		// (set) Token: 0x06002449 RID: 9289 RVA: 0x0009D56B File Offset: 0x0009B76B
		public LogLevel LogLevel { get; private set; }

		// Token: 0x170002FA RID: 762
		// (get) Token: 0x0600244A RID: 9290 RVA: 0x0009D574 File Offset: 0x0009B774
		// (set) Token: 0x0600244B RID: 9291 RVA: 0x0009D57C File Offset: 0x0009B77C
		public string Message { get; private set; }

		// Token: 0x170002FB RID: 763
		// (get) Token: 0x0600244C RID: 9292 RVA: 0x0009D585 File Offset: 0x0009B785
		// (set) Token: 0x0600244D RID: 9293 RVA: 0x0009D58D File Offset: 0x0009B78D
		public DateTime Timestamp { get; private set; }

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x0600244E RID: 9294 RVA: 0x0009D596 File Offset: 0x0009B796
		// (set) Token: 0x0600244F RID: 9295 RVA: 0x0009D59E File Offset: 0x0009B79E
		public Exception Exception { get; private set; }

		// Token: 0x06002450 RID: 9296 RVA: 0x0009D5A7 File Offset: 0x0009B7A7
		public LogEntry(string loggerName, LogLevel logLevel, DateTime timestamp, string message, Exception exception = null)
		{
			this = default(LogEntry);
			this.LoggerName = loggerName;
			this.LogLevel = logLevel;
			this.Timestamp = timestamp;
			this.Message = message;
			this.Exception = exception;
		}
	}
}
