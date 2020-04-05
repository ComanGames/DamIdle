using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000168 RID: 360
public class EventPanelItemUI : MonoBehaviour
{
	// Token: 0x06000B85 RID: 2949 RVA: 0x00034B0C File Offset: 0x00032D0C
	public void Init(EventModel eventModel, Action onClickLaunch, IObservable<double> unlocksUnlocked = null)
	{
		this.eventStateDisposables.Clear();
		this.currentEventModel = eventModel;
		this.title.text = this.currentEventModel.Name;
		this.bg.color = this.currentEventModel.TintColor;
		GameController.Instance.IconService.LoadPromoImage(eventModel.PlanetTheme).Subscribe(delegate(Sprite sprite)
		{
			this.bg.sprite = sprite;
			this.bg.color = Color.white;
		}).AddTo(this.eventStateDisposables);
		this.tintableItems.ForEach(delegate(Image x)
		{
			x.color = eventModel.TintColor;
		});
		this.rank.text = "0";
		this.currentEventModel.TimeRemaining.Subscribe(new Action<double>(this.OnEventTimerChanged)).AddTo(this.eventStateDisposables);
		this.currentEventModel.State.Subscribe(new Action<EventState>(this.OnEventStateChanged)).AddTo(base.gameObject);
		if (unlocksUnlocked != null)
		{
			int totalUnlocks = eventModel.UnlockCount;
			unlocksUnlocked.Subscribe(delegate(double unlocked)
			{
				this.unlocks.text = string.Format("{0}/{1}", unlocked, totalUnlocks);
			}).AddTo(this.eventStateDisposables);
		}
		else
		{
			this.go_Info.SetActive(false);
		}
		this.launchButton.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			if (this.currentEventModel.State.Value == EventState.current)
			{
				onClickLaunch();
			}
		}).AddTo(this.eventStateDisposables);
		this.eventInfo.Setup(eventModel);
		this.mission_unlock_icon.gameObject.SetActive(eventModel.ProgressionType == PlanetProgressionType.Missions);
		this.angel_unlock_icon.gameObject.SetActive(eventModel.ProgressionType == PlanetProgressionType.Angels);
	}

	// Token: 0x06000B86 RID: 2950 RVA: 0x00034CEC File Offset: 0x00032EEC
	private void OnEventStateChanged(EventState currentState)
	{
		this.timer.transform.parent.gameObject.SetActive(currentState == EventState.current);
		if (currentState == EventState.future)
		{
			DateTime utcNow = GameController.Instance.DateTimeService.UtcNow;
			this.startBanner.SetActive(true);
		}
		else
		{
			this.startBanner.SetActive(false);
		}
		if (currentState == EventState.current)
		{
			if (GameController.Instance.game.planetName != this.currentEventModel.Id)
			{
				this.launchButton.gameObject.SetActive(true);
			}
			else
			{
				this.launchButton.gameObject.SetActive(false);
			}
		}
		else
		{
			this.launchButton.gameObject.SetActive(false);
		}
		string id = this.currentEventModel.Id;
		if (this.currentEventModel.HasLeaderboard && this.currentEventModel.State.Value != EventState.future)
		{
			GameController.Instance.LeaderboardService.GetPlayerRank(id, this.currentEventModel.LeaderboardType, true).Subscribe(delegate(LeaderboardRankData position)
			{
				this.rank.text = position.leaderboardRank.ToString();
				this.rank.transform.parent.gameObject.SetActive(true);
			}, delegate(Exception error)
			{
				this.rank.text = "0";
				this.rank.transform.parent.gameObject.SetActive(true);
			}).AddTo(this.eventStateDisposables);
			return;
		}
		this.rank.transform.parent.gameObject.SetActive(false);
	}

	// Token: 0x06000B87 RID: 2951 RVA: 0x00034E2C File Offset: 0x0003302C
	private void OnDestroy()
	{
		this.eventStateDisposables.Dispose();
	}

	// Token: 0x06000B88 RID: 2952 RVA: 0x00034E3C File Offset: 0x0003303C
	private void OnEventTimerChanged(double time)
	{
		EventState value = this.currentEventModel.State.Value;
		if (value != EventState.future)
		{
			if (value == EventState.current)
			{
				if (time > 0.0)
				{
					this.timer.text = time.ToCountdown("{0}");
					return;
				}
				this.timer.text = "0s";
				this.launchButton.gameObject.SetActive(false);
				return;
			}
		}
		else
		{
			if (time > 0.0)
			{
				this.txt_EventStart.text = "Starts in: " + time.ToCountdownWithTextShort(false, false, false, false);
				return;
			}
			this.txt_EventStart.text = "00:00:00:00";
			this.launchButton.gameObject.SetActive(false);
		}
	}

	// Token: 0x040009F3 RID: 2547
	[SerializeField]
	private Text title;

	// Token: 0x040009F4 RID: 2548
	[SerializeField]
	private Text timer;

	// Token: 0x040009F5 RID: 2549
	[SerializeField]
	private Text unlocks;

	// Token: 0x040009F6 RID: 2550
	[SerializeField]
	private Text rank;

	// Token: 0x040009F7 RID: 2551
	[SerializeField]
	private Image bg;

	// Token: 0x040009F8 RID: 2552
	[SerializeField]
	private GameObject startBanner;

	// Token: 0x040009F9 RID: 2553
	[SerializeField]
	private Text txt_EventStart;

	// Token: 0x040009FA RID: 2554
	[SerializeField]
	private Button launchButton;

	// Token: 0x040009FB RID: 2555
	[SerializeField]
	private EventInfoPopupUI eventInfo;

	// Token: 0x040009FC RID: 2556
	[SerializeField]
	private GameObject angel_unlock_icon;

	// Token: 0x040009FD RID: 2557
	[SerializeField]
	private GameObject mission_unlock_icon;

	// Token: 0x040009FE RID: 2558
	[SerializeField]
	private GameObject go_Info;

	// Token: 0x040009FF RID: 2559
	[SerializeField]
	private List<Image> tintableItems = new List<Image>();

	// Token: 0x04000A00 RID: 2560
	private CompositeDisposable eventStateDisposables = new CompositeDisposable();

	// Token: 0x04000A01 RID: 2561
	private EventModel currentEventModel;
}
