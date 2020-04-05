using System;
using System.Collections;
using System.Linq;
using System.Threading;
using HHTools.Navigation;
using UniRx;
using UnityEngine;

// Token: 0x02000081 RID: 129
public class EventNavController : MonoBehaviour
{
	// Token: 0x060003B8 RID: 952 RVA: 0x00014518 File Offset: 0x00012718
	private void Awake()
	{
		this.eventService = GameController.Instance.EventService;
		MessageBroker.Default.Receive<LimitedTimeEventEnded>().Subscribe(new Action<LimitedTimeEventEnded>(this.OnLimitedTimeEventEnded)).AddTo(base.gameObject);
		MessageBroker.Default.Receive<WelcomeBackSequenceCompleted>().Subscribe(delegate(WelcomeBackSequenceCompleted x)
		{
			if (GameController.Instance.game.IsEventPlanet && !GameController.Instance.GlobalPlayerData.GetBool(GameController.Instance.game.planetName + "_hasSeenPlanetIntro"))
			{
				GameController.Instance.NavigationService.CreateModal<EventIntroModal>(NavModals.EVENT_INTRO_MODAL, false);
			}
		}).AddTo(base.gameObject);
		(from x in this.eventService.EventUnlocked.CombineLatest(MessageBroker.Default.Receive<WelcomeBackSequenceCompleted>(), (bool x, WelcomeBackSequenceCompleted y) => x)
		where x
		select x).Take(1).Subscribe(new Action<bool>(this.CheckPromo)).AddTo(base.gameObject);
		this.eventService.PendingEventRewards.ObserveCountChanged(true).Subscribe(new Action<int>(this.OnRewardCountChanged)).AddTo(base.gameObject);
	}

	// Token: 0x060003B9 RID: 953 RVA: 0x00014644 File Offset: 0x00012844
	private void OnDestroy()
	{
		this.disposables.Dispose();
	}

	// Token: 0x060003BA RID: 954 RVA: 0x00014654 File Offset: 0x00012854
	private void CheckPromo(bool isUnlocked)
	{
		(from eventcount in this.eventService.ActiveEvents.ObserveCountChanged(true)
		where eventcount > 0
		select eventcount).Subscribe(delegate(int _)
		{
			EventModel eventModel = this.eventService.ActiveEvents.FirstOrDefault((EventModel x) => x.ShouldShowEventPromo.Value);
			if (eventModel != null)
			{
				this.OnEventPromoPush(eventModel);
			}
		}).AddTo(base.gameObject);
	}

	// Token: 0x060003BB RID: 955 RVA: 0x000146B3 File Offset: 0x000128B3
	private void OnLimitedTimeEventEnded(LimitedTimeEventEnded evt)
	{
		if (GameController.Instance.game.planetName == evt.eventId)
		{
			GameController.Instance.LoadPlanetScene("Earth", false);
		}
	}

	// Token: 0x060003BC RID: 956 RVA: 0x000146E4 File Offset: 0x000128E4
	private void OnRewardCountChanged(int count)
	{
		if (count > 0)
		{
			this.disposables.Clear();
			PendingEventRewards pendingRewards = GameController.Instance.EventService.PendingEventRewards[0];
			if (GameController.Instance.IsLoadingPlanet)
			{
				MessageBroker.Default.Receive<WelcomeBackSequenceCompleted>().Take(1).Subscribe(delegate(WelcomeBackSequenceCompleted _)
				{
					GameController.Instance.NavigationService.CreateModal<LeaderboardRewardModal>(NavModals.LEADERBOARD_REWARD, false).WireData(pendingRewards.eventId, pendingRewards.rewardTier);
				}).AddTo(this.disposables);
				return;
			}
			GameController.Instance.NavigationService.CreateModal<LeaderboardRewardModal>(NavModals.LEADERBOARD_REWARD, false).WireData(pendingRewards.eventId, pendingRewards.rewardTier);
		}
	}

	// Token: 0x060003BD RID: 957 RVA: 0x00014790 File Offset: 0x00012990
	private void OnEventPromoPush(EventModel eventModel)
	{
		if (eventModel != null)
		{
			this.PrepareToShowEventPromo(eventModel).ToObservable(false).StartAsCoroutine(default(CancellationToken));
		}
	}

	// Token: 0x060003BE RID: 958 RVA: 0x000147BC File Offset: 0x000129BC
	private IEnumerator PrepareToShowEventPromo(EventModel eventData)
	{
		while (GameController.Instance.IsLoadingPlanet)
		{
			yield return null;
		}
		if (GameController.Instance.planetName != eventData.Id)
		{
			GameController.Instance.NavigationService.CreateModal<EventPromoModal>(NavModals.EVENT_PROMO, false).WireData(eventData);
		}
		yield break;
	}

	// Token: 0x0400033F RID: 831
	private IEventService eventService;

	// Token: 0x04000340 RID: 832
	private CompositeDisposable disposables = new CompositeDisposable();
}
