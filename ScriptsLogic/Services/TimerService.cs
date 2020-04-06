using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Utils
{
	// Token: 0x020006BC RID: 1724
	public class TimerService : IDisposable
	{
		// Token: 0x06002304 RID: 8964 RVA: 0x0009930D File Offset: 0x0009750D
		public TimerService() : this(from _ in Observable.EveryUpdate()
		select Convert.ToInt64(Time.deltaTime * 1E+07f))
		{
		}

		// Token: 0x06002305 RID: 8965 RVA: 0x00099340 File Offset: 0x00097540
		public TimerService(IObservable<long> timeGenerator)
		{
			this.timeGenerator = timeGenerator;
			Array values = Enum.GetValues(typeof(TimerService.TimerGroups));
			this.pendingDeltaTime = new long[values.Length];
			this.timersList = new Subject<TimeSpan>[values.Length];
			int num = 0;
			foreach (object obj in values)
			{
				this.timersList[num] = new Subject<TimeSpan>();
				this.timerMap.Add((TimerService.TimerGroups)obj, this.timersList[num]);
				num++;
			}
		}

		// Token: 0x06002306 RID: 8966 RVA: 0x0009940C File Offset: 0x0009760C
		public IObservable<TimeSpan> GetTimer(TimerService.TimerGroups timerGroups)
		{
			return this.timerMap[timerGroups];
		}

		// Token: 0x06002307 RID: 8967 RVA: 0x0009941A File Offset: 0x0009761A
		public void StartClock()
		{
			this.disposable = this.timeGenerator.Subscribe(new Action<long>(this.OnUpdate));
		}

		// Token: 0x06002308 RID: 8968 RVA: 0x00099439 File Offset: 0x00097639
		public void StopClock()
		{
			this.disposable.Dispose();
		}

		// Token: 0x06002309 RID: 8969 RVA: 0x00099446 File Offset: 0x00097646
		public void AddTimeOverTime(TimerService.TimerGroups timerGroups, TimeSpan timeToAdd, double duration)
		{
			Observable.FromCoroutine(() => this.InterpolateTime(timerGroups, timeToAdd, duration), false).Subscribe<Unit>();
		}

		// Token: 0x0600230A RID: 8970 RVA: 0x00099480 File Offset: 0x00097680
		public void AddTimeToAll(TimeSpan timeToAdd, bool immediateUpdate = false)
		{
			this.AddTimeToAll(timeToAdd.Ticks, immediateUpdate);
		}

		// Token: 0x0600230B RID: 8971 RVA: 0x00099490 File Offset: 0x00097690
		public void AddTimeToAll(long timeToAdd, bool immediateUpdate = false)
		{
			Debug.Log(string.Format("AddTimeToAll time to add = {0}, immediate update = {1}", timeToAdd, immediateUpdate));
			for (int i = 0; i < this.pendingDeltaTime.Length; i++)
			{
				this.pendingDeltaTime[i] += timeToAdd;
			}
			if (immediateUpdate)
			{
				this.OnUpdate(0L);
			}
		}

		// Token: 0x0600230C RID: 8972 RVA: 0x000994E7 File Offset: 0x000976E7
		public void AddTime(TimerService.TimerGroups timerGroups, TimeSpan timeToAdd, bool immediateUpdate = false)
		{
			this.AddTime(timerGroups, timeToAdd.Ticks, immediateUpdate);
		}

		// Token: 0x0600230D RID: 8973 RVA: 0x000994F8 File Offset: 0x000976F8
		public void AddTime(TimerService.TimerGroups timerGroups, long ticks, bool immediateUpdate = false)
		{
			this.pendingDeltaTime[(int)timerGroups] += ticks;
			if (immediateUpdate)
			{
				this.OnUpdate(0L);
			}
		}

		// Token: 0x0600230E RID: 8974 RVA: 0x00099516 File Offset: 0x00097716
		private IEnumerator InterpolateTime(TimerService.TimerGroups timerGroups, TimeSpan timeToAdd, double duration)
		{
			long ticks = (long)(timeToAdd.TotalSeconds / duration) * 10000000L;
			while (duration > 0.0)
			{
				duration -= (double)Time.deltaTime;
				this.AddTime(timerGroups, (long)((float)ticks * Time.deltaTime), true);
				yield return null;
			}
			yield break;
		}

		// Token: 0x0600230F RID: 8975 RVA: 0x0009953C File Offset: 0x0009773C
		private void OnUpdate(long deltaTime)
		{
			for (int i = 0; i < this.timersList.Length; i++)
			{
				this.timersList[i].OnNext(new TimeSpan(deltaTime + this.pendingDeltaTime[i]));
				this.pendingDeltaTime[i] = 0L;
			}
		}

		// Token: 0x06002310 RID: 8976 RVA: 0x00099582 File Offset: 0x00097782
		public TimerService.Scheduler GetScheduler(TimerService.TimerGroups timerGroups)
		{
			return new TimerService.Scheduler(this.timerMap[timerGroups]);
		}

		// Token: 0x06002311 RID: 8977 RVA: 0x00099595 File Offset: 0x00097795
		public void Dispose()
		{
			this.StopClock();
		}

		// Token: 0x04002477 RID: 9335
		private Subject<TimeSpan>[] timersList;

		// Token: 0x04002478 RID: 9336
		private Dictionary<TimerService.TimerGroups, Subject<TimeSpan>> timerMap = new Dictionary<TimerService.TimerGroups, Subject<TimeSpan>>();

		// Token: 0x04002479 RID: 9337
		private long[] pendingDeltaTime;

		// Token: 0x0400247A RID: 9338
		private IDisposable disposable = Disposable.Empty;

		// Token: 0x0400247B RID: 9339
		private IObservable<long> timeGenerator;

		// Token: 0x02000A0B RID: 2571
		public enum TimerGroups
		{
			// Token: 0x04002FDB RID: 12251
			State,
			// Token: 0x04002FDC RID: 12252
			Global
		}

		// Token: 0x02000A0C RID: 2572
		public class Scheduler : IScheduler, ISchedulerPeriodic
		{
			// Token: 0x060030C4 RID: 12484 RVA: 0x000B8219 File Offset: 0x000B6419
			public Scheduler(IObservable<TimeSpan> timerStream)
			{
				this.disposable = timerStream.Subscribe(new Action<TimeSpan>(this.OnNext));
			}

			// Token: 0x170003D8 RID: 984
			// (get) Token: 0x060030C5 RID: 12485 RVA: 0x000B8239 File Offset: 0x000B6439
			public DateTimeOffset Now
			{
				get
				{
					return UniRx.Scheduler.Now;
				}
			}

			// Token: 0x060030C6 RID: 12486 RVA: 0x000B8240 File Offset: 0x000B6440
			public IDisposable Schedule(Action action)
			{
				action();
				return this.disposable;
			}

			// Token: 0x060030C7 RID: 12487 RVA: 0x000B8250 File Offset: 0x000B6450
			public IDisposable Schedule(TimeSpan dueTime, Action action)
			{
				this.started = true;
				this.baseTime = (this.remainingTime = UniRx.Scheduler.Normalize(dueTime).TotalSeconds);
				this.action = action;
				return this.disposable;
			}

			// Token: 0x060030C8 RID: 12488 RVA: 0x000B8290 File Offset: 0x000B6490
			private void OnNext(TimeSpan elapsedTime)
			{
				if (!this.started)
				{
					return;
				}
				this.remainingTime -= elapsedTime.TotalSeconds;
				if (this.remainingTime > 0.0)
				{
					return;
				}
				this.action();
				if (this.periodic)
				{
					this.remainingTime = this.baseTime + this.remainingTime;
				}
			}

			// Token: 0x060030C9 RID: 12489 RVA: 0x000B82F2 File Offset: 0x000B64F2
			public IDisposable SchedulePeriodic(TimeSpan period, Action action)
			{
				this.periodic = true;
				return this.Schedule(period, action);
			}

			// Token: 0x04002FDD RID: 12253
			private bool started;

			// Token: 0x04002FDE RID: 12254
			private double baseTime;

			// Token: 0x04002FDF RID: 12255
			private double remainingTime;

			// Token: 0x04002FE0 RID: 12256
			private Action action;

			// Token: 0x04002FE1 RID: 12257
			private bool periodic;

			// Token: 0x04002FE2 RID: 12258
			private IDisposable disposable;
		}
	}
}
