using System;
using System.Collections.Generic;
using Extensions.Collections;

namespace Platforms.Logger
{
	// Token: 0x020006D8 RID: 1752
	public class Logger
	{
		// Token: 0x14000297 RID: 663
		// (add) Token: 0x0600242D RID: 9261 RVA: 0x0009CFD8 File Offset: 0x0009B1D8
		// (remove) Token: 0x0600242E RID: 9262 RVA: 0x0009D00C File Offset: 0x0009B20C
		public static event Action<LogEntry> PublishEvent;

		// Token: 0x0600242F RID: 9263 RVA: 0x0009D03F File Offset: 0x0009B23F
		public static Logger GetLogger(object callingObject)
		{
			return Logger.GetLoggerInternal((callingObject as string) ?? callingObject.GetType().FullName);
		}

		// Token: 0x06002430 RID: 9264 RVA: 0x0009D05C File Offset: 0x0009B25C
		private static Logger GetLoggerInternal(string loggerName)
		{
			Logger logger;
			if (Logger.loggers.TryGetValue(loggerName, out logger))
			{
				return logger;
			}
			Logger.LoadConfig();
			if (Logger.publishers == null)
			{
				Logger.publishers = Logger.GetPublishers();
				for (int i = 0; i < Logger.publishers.Length; i++)
				{
					ILogPublisher logPublisher = Logger.publishers[i];
					logPublisher.SetTplus(Logger.tplus);
					Logger.PublishEvent += logPublisher.Publish;
				}
			}
			logger = new Logger(loggerName, Logger.PublishEvent);
			logger.SetLevel(Logger.defaultLevels.TryGetValue(loggerName, Logger.defaultLevels["*"]));
			Logger.loggers.Add(loggerName, logger);
			return logger;
		}

		// Token: 0x06002431 RID: 9265 RVA: 0x0009D100 File Offset: 0x0009B300
		private static void LoadConfig()
		{
			if (Logger.defaultLevels != null)
			{
				return;
			}
			Logger.defaultLevels = new Dictionary<string, LogLevel>();
			Dictionary<string, string> dictionary = SimpleConfigFile.LoadConfig("logger-config", true);
			dictionary.Remove("_Tplus");
			foreach (KeyValuePair<string, string> keyValuePair in dictionary)
			{
				Logger.defaultLevels.Add(keyValuePair.Key, (LogLevel)Enum.Parse(typeof(LogLevel), keyValuePair.Value));
			}
		}

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x06002432 RID: 9266 RVA: 0x0009D19C File Offset: 0x0009B39C
		// (set) Token: 0x06002433 RID: 9267 RVA: 0x0009D1A4 File Offset: 0x0009B3A4
		public string Name { get; private set; }

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x06002434 RID: 9268 RVA: 0x0009D1AD File Offset: 0x0009B3AD
		// (set) Token: 0x06002435 RID: 9269 RVA: 0x0009D1B5 File Offset: 0x0009B3B5
		public LogLevel CurrentLevel { get; protected set; }

		// Token: 0x06002436 RID: 9270 RVA: 0x0009D1BE File Offset: 0x0009B3BE
		public Logger(string loggerName, Action<LogEntry> publisher)
		{
			this.Name = loggerName;
			this.publish = publisher;
		}

		// Token: 0x06002437 RID: 9271 RVA: 0x0009D1D4 File Offset: 0x0009B3D4
		public void SetLevel(LogLevel level)
		{
			this.CurrentLevel = level;
		}

		// Token: 0x06002438 RID: 9272 RVA: 0x0009D1E0 File Offset: 0x0009B3E0
		public virtual void Trace(string message, params object[] args)
		{
			if (this.CurrentLevel <= LogLevel.Trace)
			{
				Action<LogEntry> action = this.publish;
				string message2 = string.Format(message, args);
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Trace, now, message2, null));
			}
		}

		// Token: 0x06002439 RID: 9273 RVA: 0x0009D220 File Offset: 0x0009B420
		public virtual void Debug(string message)
		{
			if (this.CurrentLevel <= LogLevel.Debug)
			{
				Action<LogEntry> action = this.publish;
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Debug, now, message, null));
			}
		}

		// Token: 0x0600243A RID: 9274 RVA: 0x0009D258 File Offset: 0x0009B458
		public virtual void Debug(string message, params object[] args)
		{
			if (this.CurrentLevel <= LogLevel.Debug)
			{
				Action<LogEntry> action = this.publish;
				string message2 = string.Format(message, args);
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Debug, now, message2, null));
			}
		}

		// Token: 0x0600243B RID: 9275 RVA: 0x0009D298 File Offset: 0x0009B498
		public virtual void Info(string message)
		{
			if (this.CurrentLevel <= LogLevel.Info)
			{
				Action<LogEntry> action = this.publish;
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Info, now, message, null));
			}
		}

		// Token: 0x0600243C RID: 9276 RVA: 0x0009D2D0 File Offset: 0x0009B4D0
		public virtual void Info(string message, params object[] args)
		{
			if (this.CurrentLevel <= LogLevel.Info)
			{
				Action<LogEntry> action = this.publish;
				string message2 = string.Format(message, args);
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Info, now, message2, null));
			}
		}

		// Token: 0x0600243D RID: 9277 RVA: 0x0009D310 File Offset: 0x0009B510
		public virtual void Warning(string message)
		{
			if (this.CurrentLevel <= LogLevel.Warning)
			{
				Action<LogEntry> action = this.publish;
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Warning, now, message, null));
			}
		}

		// Token: 0x0600243E RID: 9278 RVA: 0x0009D348 File Offset: 0x0009B548
		public virtual void Warning(string message, params object[] args)
		{
			if (this.CurrentLevel <= LogLevel.Warning)
			{
				Action<LogEntry> action = this.publish;
				string message2 = string.Format(message, args);
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Warning, now, message2, null));
			}
		}

		// Token: 0x0600243F RID: 9279 RVA: 0x0009D388 File Offset: 0x0009B588
		public virtual void Error(string message)
		{
			if (this.CurrentLevel <= LogLevel.Error)
			{
				Action<LogEntry> action = this.publish;
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Error, now, message, null));
			}
		}

		// Token: 0x06002440 RID: 9280 RVA: 0x0009D3C0 File Offset: 0x0009B5C0
		public virtual void Error(string message, params object[] args)
		{
			if (this.CurrentLevel <= LogLevel.Error)
			{
				Action<LogEntry> action = this.publish;
				string message2 = string.Format(message, args);
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Error, now, message2, null));
			}
		}

		// Token: 0x06002441 RID: 9281 RVA: 0x0009D400 File Offset: 0x0009B600
		public virtual void Exception(string message, Exception exception)
		{
			if (this.CurrentLevel <= LogLevel.Error)
			{
				Action<LogEntry> action = this.publish;
				string message2 = message + "\n" + ((exception != null) ? exception.ToString() : string.Empty);
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Error, now, message2, exception));
			}
		}

		// Token: 0x06002442 RID: 9282 RVA: 0x0009D454 File Offset: 0x0009B654
		public virtual void Exception(Exception exception)
		{
			if (this.CurrentLevel <= LogLevel.Error)
			{
				Action<LogEntry> action = this.publish;
				string message = (exception != null) ? exception.ToString() : string.Empty;
				DateTime now = DateTime.Now;
				action(new LogEntry(this.Name, LogLevel.Error, now, message, exception));
			}
		}

		// Token: 0x06002443 RID: 9283 RVA: 0x0009D49D File Offset: 0x0009B69D
		public virtual void Raw(LogEntry logEntry)
		{
			this.publish(logEntry);
		}

		// Token: 0x06002444 RID: 9284 RVA: 0x0009D4AC File Offset: 0x0009B6AC
		private static ILogPublisher[] GetPublishers()
		{
			List<Type> assignableTypes = Helper.GetAssignableTypes<ILogPublisher>();
			List<ILogPublisher> list = new List<ILogPublisher>();
			foreach (Type type in assignableTypes)
			{
				if (!(type == typeof(UnityLogPublisher)))
				{
					list.Add((ILogPublisher)Activator.CreateInstance(type));
				}
			}
			if (list.Count == 0)
			{
				return new ILogPublisher[]
				{
					new UnityLogPublisher()
				};
			}
			return list.ToArray();
		}

		// Token: 0x04002501 RID: 9473
		private static ILogPublisher[] publishers;

		// Token: 0x04002502 RID: 9474
		private static Dictionary<string, Logger> loggers = new Dictionary<string, Logger>();

		// Token: 0x04002503 RID: 9475
		private static Dictionary<string, LogLevel> defaultLevels;

		// Token: 0x04002504 RID: 9476
		private static bool tplus = false;

		// Token: 0x04002508 RID: 9480
		protected Action<LogEntry> publish;
	}
}
