using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

// Token: 0x0200000C RID: 12
public class ScheduledOfferService
{
	// Token: 0x17000005 RID: 5
	// (get) Token: 0x0600002D RID: 45 RVA: 0x00002A39 File Offset: 0x00000C39
	// (set) Token: 0x0600002E RID: 46 RVA: 0x00002A41 File Offset: 0x00000C41
	public ReactiveCollection<ScheduledOfferModel> ActiveOffers { get; private set; }

	// Token: 0x17000006 RID: 6
	// (get) Token: 0x0600002F RID: 47 RVA: 0x00002A4A File Offset: 0x00000C4A
	// (set) Token: 0x06000030 RID: 48 RVA: 0x00002A52 File Offset: 0x00000C52
	public ReactiveCollection<ScheduledOfferModel> FutureOffers { get; private set; }

	// Token: 0x06000031 RID: 49 RVA: 0x00002A5B File Offset: 0x00000C5B
	public ScheduledOfferService()
	{
		this.ActiveOffers = new ReactiveCollection<ScheduledOfferModel>();
		this.FutureOffers = new ReactiveCollection<ScheduledOfferModel>();
	}

	// Token: 0x06000032 RID: 50 RVA: 0x00002A90 File Offset: 0x00000C90
	public void Init(List<ScheduledOffer> offerDatas, IDateTimeService dateTimeService, IUserDataService userDataService)
	{
		this.dateTimeService = dateTimeService;
		this.userDataService = userDataService;
		for (int i = 0; i < offerDatas.Count; i++)
		{
			ScheduledOffer scheduledOffer = offerDatas[i];
			ScheduledOfferModel scheduledOfferModel = new ScheduledOfferModel(scheduledOffer);
			this.allOffers.Add(scheduledOfferModel);
			if ((string.IsNullOrEmpty(scheduledOffer.ABTestGroup) || this.userDataService.ListAbTestGroups().Contains(scheduledOffer.ABTestGroup)) && scheduledOffer.EndDate.CompareTo(this.dateTimeService.UtcNow) > 0)
			{
				if (scheduledOffer.StartDate.CompareTo(this.dateTimeService.UtcNow) > 0)
				{
					this.AddToFutureOffers(scheduledOfferModel);
				}
				else
				{
					this.AddToActiveOffers(scheduledOfferModel);
				}
			}
		}
		this.MonitorOfferTimers();
	}

	// Token: 0x06000033 RID: 51 RVA: 0x00002B4C File Offset: 0x00000D4C
	public IObservable<ScheduledOfferModel> MonitorForActiveOfferOfType(ScheduledOfferType type)
	{
		ScheduledOfferModel value = this.ActiveOffers.FirstOrDefault((ScheduledOfferModel x) => x.Type == type);
		Func<ScheduledOfferModel, bool> <>9__2;
		return this.ActiveOffers.ObserveCountChanged(false).Select(delegate(int _)
		{
			IEnumerable<ScheduledOfferModel> activeOffers = this.ActiveOffers;
			Func<ScheduledOfferModel, bool> predicate;
			if ((predicate = <>9__2) == null)
			{
				predicate = (<>9__2 = ((ScheduledOfferModel x) => x.Type == type && x.State.Value == ScheduledOfferState.ACTIVE));
			}
			List<ScheduledOfferModel> list = (from x in activeOffers.Where(predicate)
			orderby x.StartDate
			select x).ToList<ScheduledOfferModel>();
			if (list.Count <= 0)
			{
				return null;
			}
			return list[0];
		}).StartWith(value);
	}

	// Token: 0x06000034 RID: 52 RVA: 0x00002BA8 File Offset: 0x00000DA8
	private void MonitorOfferTimers()
	{
		Observable.Interval(TimeSpan.FromSeconds(1.0)).StartWith(0L).Subscribe(delegate(long _)
		{
			DateTime utcNow = this.dateTimeService.UtcNow;
			for (int i = this.ActiveOffers.Count - 1; i >= 0; i--)
			{
				this.ActiveOffers[i].TimeRemaining.Value = (this.ActiveOffers[i].EndDate - utcNow).TotalSeconds;
				if (this.ActiveOffers[i].TimeRemaining.Value <= 0.0)
				{
					this.ActiveOffers[i].State.Value = ScheduledOfferState.PAST;
					this.ActiveOffers.RemoveAt(i);
				}
			}
			for (int j = this.FutureOffers.Count - 1; j >= 0; j--)
			{
				this.FutureOffers[j].TimeRemaining.Value = (this.FutureOffers[j].StartDate - utcNow).TotalSeconds;
				if (this.FutureOffers[j].TimeRemaining.Value <= 0.0)
				{
					this.AddToActiveOffers(this.FutureOffers[j]);
					this.FutureOffers.RemoveAt(j);
				}
			}
		}, delegate(Exception x)
		{
			Debug.LogError(x.Message);
		}).AddTo(this.disposables);
	}

	// Token: 0x06000035 RID: 53 RVA: 0x00002C0C File Offset: 0x00000E0C
	private void AddToActiveOffers(ScheduledOfferModel model)
	{
		DateTime utcNow = this.dateTimeService.UtcNow;
		model.TimeRemaining.Value = (model.EndDate - utcNow).TotalSeconds;
		model.State.Value = ScheduledOfferState.ACTIVE;
		this.ActiveOffers.Add(model);
	}

	// Token: 0x06000036 RID: 54 RVA: 0x00002C5C File Offset: 0x00000E5C
	private void AddToFutureOffers(ScheduledOfferModel model)
	{
		DateTime utcNow = this.dateTimeService.UtcNow;
		model.TimeRemaining.Value = (model.StartDate - utcNow).TotalSeconds;
		model.State.Value = ScheduledOfferState.FUTURE;
		this.FutureOffers.Add(model);
	}

	// Token: 0x0400002B RID: 43
	private List<ScheduledOfferModel> allOffers = new List<ScheduledOfferModel>();

	// Token: 0x0400002C RID: 44
	private IDateTimeService dateTimeService;

	// Token: 0x0400002D RID: 45
	private IUserDataService userDataService;

	// Token: 0x0400002E RID: 46
	private CompositeDisposable disposables = new CompositeDisposable();
}
