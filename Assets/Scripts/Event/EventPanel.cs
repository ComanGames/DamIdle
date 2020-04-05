using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000167 RID: 359
public class EventPanel : PanelBaseClass
{
	// Token: 0x06000B7A RID: 2938 RVA: 0x00034368 File Offset: 0x00032568
	public void Init(IEventService eventService)
	{
		this.eventService = eventService;
		bool flag = LoadScene.MeetsVersionRequirement(GameController.Instance.EventDataService.CatalogEntry.MinClientVersion);
		if (GameController.Instance.EventDataService.WasEventScheduleDownloadedSuccessfully && flag)
		{
			this.ErrorPanel.gameObject.SetActive(false);
			foreach (EventModel eventModel in this.eventService.ActiveEvents)
			{
				this.SetupEventPanel(eventModel, this.CurrentEventPanel.transform);
			}
			this.eventService.ActiveEvents.ObserveAdd().Subscribe(delegate(CollectionAddEvent<EventModel> x)
			{
				this.OnEventAdded(x.Value, this.CurrentEventPanel.transform);
			}).AddTo(base.gameObject);
			(from x in this.eventService.ActiveEvents.ObserveRemove()
			select x.Value).Subscribe(delegate(EventModel x)
			{
				this.OnEventRemoved(x);
				this.UpdateFutureEvents();
			}).AddTo(base.gameObject);
			this.UpdateFutureEvents();
			foreach (EventModel eventModel2 in (from evnt in this.eventService.PastEvents
			where PlayerPrefs.HasKey("GameStateData_" + evnt.Id)
			select evnt).ToList<EventModel>())
			{
				this.SetupEventPanel(eventModel2, this.PastEventsPanel.transform);
			}
			(from x in this.eventService.PastEvents.ObserveAdd()
			where PlayerPrefs.HasKey("GameStateData_" + x.Value.Id)
			select x).Subscribe(delegate(CollectionAddEvent<EventModel> x)
			{
				this.OnEventAdded(x.Value, this.PastEventsPanel.transform);
			}).AddTo(base.gameObject);
			this.UpdatePanels();
			return;
		}
		this.UpdatePanels();
		this.ErrorPanel.gameObject.SetActive(true);
		this.ErrorTxt.text = "Oops! To access Events, you will need the latest version of AdVenture Capitalist installed and a stable internet connection.";
		this.btn_gotoStore.gameObject.SetActive(false);
	}

	// Token: 0x06000B7B RID: 2939 RVA: 0x000345A0 File Offset: 0x000327A0
	private void UpdateFutureEvents()
	{
		bool showUpcomingEventWhenEventActive = AdCapExternalDataStorage.Data.GeneralConfig[0].ShowUpcomingEventWhenEventActive;
		if (this.eventService.ActiveEvents.Count == 0 || showUpcomingEventWhenEventActive)
		{
			List<EventModel> future = (from x in this.eventService.FutureEvents
			orderby x.StartDate
			select x).ToList<EventModel>();
			if (future.Count > 0)
			{
				this.SetupEventPanel(future[0], this.FutureEventsPanel.transform);
				(from x in this.eventService.FutureEvents.ObserveRemove()
				where x.Value == future[0]
				select x.Value).Subscribe(new Action<EventModel>(this.OnEventRemoved)).AddTo(base.gameObject);
				this.UpdatePanels();
			}
		}
	}

	// Token: 0x06000B7C RID: 2940 RVA: 0x000346B4 File Offset: 0x000328B4
	private void UpdatePanels()
	{
		this.FutureEventsPanel.gameObject.SetActive(this.FutureEventsPanel.childCount > 1);
		this.CurrentEventPanel.gameObject.SetActive(this.CurrentEventPanel.childCount > 1);
		this.PastEventsPanel.gameObject.SetActive(this.PastEventsPanel.childCount > 1);
	}

	// Token: 0x06000B7D RID: 2941 RVA: 0x0003471C File Offset: 0x0003291C
	private void OnEventRemoved(EventModel eventModel)
	{
		if (this.eventViews.ContainsKey(eventModel.Id))
		{
			Object.DestroyImmediate(this.eventViews[eventModel.Id].gameObject);
			this.eventViews.Remove(eventModel.Id);
		}
		this.UpdatePanels();
	}

	// Token: 0x06000B7E RID: 2942 RVA: 0x0003476F File Offset: 0x0003296F
	private void OnEventAdded(EventModel eventModel, Transform parent)
	{
		this.SetupEventPanel(eventModel, parent);
		this.UpdatePanels();
	}

	// Token: 0x06000B7F RID: 2943 RVA: 0x00034780 File Offset: 0x00032980
	private void SetupEventPanel(EventModel eventModel, Transform parent)
	{
		PlayerData playerData = PlayerData.GetPlayerData("Global");
		GameObject gameObject = Object.Instantiate<GameObject>(this.PanelItemPrefab);
		EventPanelItemUI component = gameObject.GetComponent<EventPanelItemUI>();
		IObservable<double> observable = playerData.GetObservable(string.Format("{0}_{1}", eventModel.Id, "gold"), 0.0);
		IObservable<double> observable2 = playerData.GetObservable(string.Format("{0}_{1}", eventModel.Id, "megaBucks"), 0.0);
		IObservable<double> observable3 = playerData.GetObservable(string.Format("{0}_{1}", eventModel.Id, "badges"), 0.0);
		IObservable<double> observable4 = playerData.GetObservable(string.Format("{0}_{1}", eventModel.Id, "itemization"), 0.0);
		IObservable<double> observable5 = playerData.GetObservable(string.Format("{0}_{1}", eventModel.Id, "timewarp"), 0.0);
		IObservable<double> observable6 = playerData.GetObservable(string.Format("{0}_{1}", eventModel.Id, "timewarpExpress"), 0.0);
		IObservable<double> observable7 = playerData.GetObservable(string.Format("{0}_{1}", eventModel.Id, "milestones"), 0.0);
		ReadOnlyReactiveProperty<double> unlocksUnlocked = observable.CombineLatest(observable2, (double g, double m) => g + m).CombineLatest(observable3, (double t, double b) => t + b).CombineLatest(observable4, (double t2, double i) => t2 + i).CombineLatest(observable5, (double t3, double w) => t3 + w).CombineLatest(observable6, (double t4, double e) => t4 + e).CombineLatest(observable7, (double t5, double e) => t5 + e).ToReadOnlyReactiveProperty<double>();
		component.Init(eventModel, delegate
		{
			this.OnClickLaunchEvent(eventModel.Id);
		}, unlocksUnlocked);
		gameObject.transform.SetParent(parent);
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		if (!this.eventViews.ContainsKey(eventModel.Id))
		{
			this.eventViews.Add(eventModel.Id, component);
			return;
		}
		Debug.LogError("WTF " + eventModel.Id);
	}

	// Token: 0x06000B80 RID: 2944 RVA: 0x00034A68 File Offset: 0x00032C68
	private void OnClickLaunchEvent(string id)
	{
		string result = string.Format("{0}:{1}", GameController.Instance.game.planetName, id);
		GameController.Instance.AnalyticService.SendNavActionAnalytics("EventLaunchClicked", "Event_Panel_Button", result);
		GameController.Instance.LoadPlanetScene(id, false);
	}

	// Token: 0x040009EA RID: 2538
	public Transform FutureEventsPanel;

	// Token: 0x040009EB RID: 2539
	public Transform CurrentEventPanel;

	// Token: 0x040009EC RID: 2540
	public Transform PastEventsPanel;

	// Token: 0x040009ED RID: 2541
	[SerializeField]
	private Transform ErrorPanel;

	// Token: 0x040009EE RID: 2542
	[SerializeField]
	private Text ErrorTxt;

	// Token: 0x040009EF RID: 2543
	[SerializeField]
	private Button btn_gotoStore;

	// Token: 0x040009F0 RID: 2544
	public GameObject PanelItemPrefab;

	// Token: 0x040009F1 RID: 2545
	private IEventService eventService;

	// Token: 0x040009F2 RID: 2546
	private Dictionary<string, EventPanelItemUI> eventViews = new Dictionary<string, EventPanelItemUI>();
}
