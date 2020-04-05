using System;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using Platforms;
using Platforms.Logger;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000256 RID: 598
public class StatsUI : PanelBaseClass
{
	// Token: 0x060010A9 RID: 4265 RVA: 0x0004C074 File Offset: 0x0004A274
	private void Awake()
	{
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		this.UpdatePlatformObjects();
		this.achievmentButtons.ForEach(delegate(Button btn)
		{
			btn.OnClickAsObservable().Subscribe(delegate(Unit _)
			{
				this.ShowAchievements();
			}).AddTo(base.gameObject);
		});
		this.leaderboardButtons.ForEach(delegate(Button btn)
		{
			btn.OnClickAsObservable().Subscribe(delegate(Unit _)
			{
				this.ShowLeaderboards();
			}).AddTo(base.gameObject);
		});
	}

	// Token: 0x060010AA RID: 4266 RVA: 0x0004C0C1 File Offset: 0x0004A2C1
	private void Start()
	{
		GameController.Instance.State.Subscribe(new Action<GameState>(this.Init)).AddTo(this);
	}

	// Token: 0x060010AB RID: 4267 RVA: 0x0004C0E8 File Offset: 0x0004A2E8
	private void Init(GameState state)
	{
		OrientationController.Instance.OrientationStream.DelayFrame(1, FrameCountType.Update).Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
		foreach (string text in this.planetNameStrings)
		{
			if (text != state.planetName)
			{
				string n = text;
				GameStateSaveLoad.Load(text, delegate(GameStateSaveData saveData)
				{
					this.otherPlanetsSaveData.Add(n, saveData);
				});
			}
		}
		foreach (string text2 in this.orderedStatsTitles)
		{
			StatsFieldUI statsFieldUI = Object.Instantiate<StatsFieldUI>(this.statsFieldUIPrefab);
			statsFieldUI.transform.SetParent(this.statsFieldUIParent, false);
			statsFieldUI.gameObject.SetActive(true);
			statsFieldUI.titleText.text = text2;
			this.orderedStatsFieldUis.Add(statsFieldUI);
		}
		this.hardResetStatObject.SetActive(false);
		this.planetName = state.planetName;
		this.chosenPlanetName = state.planetName;
		this.isCurrentPlanetChosen = true;
		this.SetupResetButton(state);
		this.UpdateLabels();
		Observable.Interval(TimeSpan.FromMilliseconds(500.0)).Subscribe(delegate(long _)
		{
			this.UpdateLabels();
		}).AddTo(this);
		this.choosePlanetDropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnPlanetChoiceChanged));
		GameController.Instance.UnlockService.OnAllUnlocksDone.Subscribe(new Action<bool>(this.OnEventCompletedCheck)).AddTo(base.gameObject);
		this.goToMissionControlButton.OnClickAsObservable().Subscribe(delegate(Unit u)
		{
			this.ParentModalController.CloseModal(u);
			GameController.Instance.NavigationService.CreateModal<AdventuresModalController>(NavModals.ADVENTURES_PANEL, false).ShowMissionControl();
		}).AddTo(base.gameObject);
		string a = this.chosenPlanetName;
		if (!(a == "Earth"))
		{
			if (!(a == "Moon"))
			{
				if (!(a == "Mars"))
				{
					if (a == "Event")
					{
						this.choosePlanetDropdown.value = 3;
					}
				}
				else
				{
					this.choosePlanetDropdown.value = 2;
				}
			}
			else
			{
				this.choosePlanetDropdown.value = 1;
			}
		}
		else
		{
			this.choosePlanetDropdown.value = 0;
		}
		this.Setup(GameController.Instance.EventService.PastEvents.ToList<EventModel>());
		MessageBroker.Default.Receive<EventParticipatedEvent>().Subscribe(new Action<EventParticipatedEvent>(this.OnEventParticipated)).AddTo(base.gameObject);
		if (!state.IsEventPlanet)
		{
			this.hardResetStatButton.OnClickAsObservable().Subscribe(new Action<Unit>(StatsUI.ShowHardResetPanel)).AddTo(base.gameObject);
		}
	}

	// Token: 0x060010AC RID: 4268 RVA: 0x0004C390 File Offset: 0x0004A590
	private void UpdatePlatformObjects()
	{
		PlatformType platformType = Helper.GetPlatformType();
		foreach (KeyValuePair<PlatformType, GameObjectListObject> keyValuePair in this.platformObjectsList)
		{
			foreach (GameObject gameObject in keyValuePair.Value.GameObjectList)
			{
				gameObject.SetActive(keyValuePair.Key == platformType);
			}
		}
	}

	// Token: 0x060010AD RID: 4269 RVA: 0x0004C434 File Offset: 0x0004A634
	private void ShowAchievements()
	{
		this.SendNavAction("Open Platform Achievments");
		if (Social.Active.localUser.authenticated)
		{
			Social.Active.ShowAchievementsUI();
			return;
		}
		Social.localUser.Authenticate(delegate(bool success, string error)
		{
			if (success)
			{
				Social.Active.ShowAchievementsUI();
				return;
			}
			this.logger.Warning(error);
		});
	}

	// Token: 0x060010AE RID: 4270 RVA: 0x0004C473 File Offset: 0x0004A673
	private void ShowLeaderboards()
	{
		this.SendNavAction("Open Platform Leaderboards");
		if (Social.Active.localUser.authenticated)
		{
			Social.Active.ShowLeaderboardUI();
			return;
		}
		Social.localUser.Authenticate(delegate(bool success, string error)
		{
			if (success)
			{
				Social.Active.ShowLeaderboardUI();
				return;
			}
			this.logger.Warning(error);
		});
	}

	// Token: 0x060010AF RID: 4271 RVA: 0x0004C4B2 File Offset: 0x0004A6B2
	private void SendNavAction(string action)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics(action, "StatsUI", GameController.Instance.planetName);
	}

	// Token: 0x060010B0 RID: 4272 RVA: 0x0004C4D3 File Offset: 0x0004A6D3
	private static void ShowHardResetPanel(Unit u)
	{
		GameController.Instance.NavigationService.CreateModal<HardResetModal>(NavModals.HARD_RESET, false).Init();
	}

	// Token: 0x060010B1 RID: 4273 RVA: 0x0004C4F0 File Offset: 0x0004A6F0
	private void Setup(List<EventModel> eventModels)
	{
		if (eventModels == null)
		{
			return;
		}
		PlayerData playerData = PlayerData.GetPlayerData("Global");
		foreach (EventModel eventModel in eventModels)
		{
			this.eventsParticipated++;
			double num = playerData.GetDouble(string.Format("{0}_{1}", eventModel.Id, "gold"), 0.0);
			num += playerData.GetDouble(string.Format("{0}_{1}", eventModel.Id, "megaBucks"), 0.0);
			num += playerData.GetDouble(string.Format("{0}_{1}", eventModel.Id, "badges"), 0.0);
			num += playerData.GetDouble(string.Format("{0}_{1}", eventModel.Id, "itemization"), 0.0);
			num += playerData.GetDouble(string.Format("{0}_{1}", eventModel.Id, "timewarp"), 0.0);
			num += playerData.GetDouble(string.Format("{0}_{1}", eventModel.Id, "timewarpExpress"), 0.0);
			num += playerData.GetDouble(string.Format("{0}_{1}", eventModel.Id, "milestones"), 0.0);
			if ((double)eventModel.UnlockCount >= num)
			{
				this.eventsCompleted++;
			}
		}
	}

	// Token: 0x060010B2 RID: 4274 RVA: 0x0004C690 File Offset: 0x0004A890
	private void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		Vector2 vector = new Vector2(20f, 20f);
		int num = this.gridLayoutGroup.padding.left + this.gridLayoutGroup.padding.right;
		Rect rect = ((RectTransform)this.gridLayoutGroup.transform).rect;
		float x = orientation.IsPortrait ? (rect.width - (float)num) : (rect.width * 0.5f - vector.x * 0.5f - (float)num * 0.5f);
		this.gridLayoutGroup.cellSize = new Vector2(x, this.gridLayoutGroup.cellSize.y);
		this.gridLayoutGroup.spacing = vector;
	}

	// Token: 0x060010B3 RID: 4275 RVA: 0x0004C74C File Offset: 0x0004A94C
	private void SetupResetButton(GameState state)
	{
		if (!state.IsEventPlanet)
		{
			bool value = GameController.Instance.UnlockService.OnAllUnlocksDone.Value;
			bool @bool = GameController.Instance.game.planetPlayerData.GetBool(this.planetName + "HardResetReward");
			bool bool2 = GameController.Instance.GlobalPlayerData.GetBool(string.Format("{0}HasSeenPostcard", this.planetName));
			if (value || bool2)
			{
				this.hardResetStatObject.SetActive(true);
				if (state.planetName == "Moon")
				{
					this.hardResetTitleText.text = "Reset The " + state.planetName;
				}
				else
				{
					this.hardResetTitleText.text = "Reset " + state.planetName;
				}
				if (!@bool)
				{
					this.hardResetSubText.text = "and Earn Gold!";
					return;
				}
				this.hardResetSubText.text = "Just for Fun!";
			}
		}
	}

	// Token: 0x060010B4 RID: 4276 RVA: 0x0004C83C File Offset: 0x0004AA3C
	private void OnPlanetChoiceChanged(int value)
	{
		switch (value)
		{
		case 0:
			this.chosenPlanetName = "Earth";
			break;
		case 1:
			this.chosenPlanetName = "Moon";
			break;
		case 2:
			this.chosenPlanetName = "Mars";
			break;
		default:
			this.chosenPlanetName = "Event";
			break;
		}
		this.isCurrentPlanetChosen = (this.planetName == this.chosenPlanetName);
		this.SetupStatFieldUIs();
		this.UpdateLabels();
	}

	// Token: 0x060010B5 RID: 4277 RVA: 0x0004C8B4 File Offset: 0x0004AAB4
	private void SetupStatFieldUIs()
	{
		if (this.chosenPlanetName == "Event")
		{
			int num = this.orderedEventPlanetTitles.Length;
			for (int i = 0; i < this.orderedStatsFieldUis.Count; i++)
			{
				if (i < num)
				{
					this.orderedStatsFieldUis[i].titleText.text = this.orderedEventPlanetTitles[i];
				}
				this.orderedStatsFieldUis[i].gameObject.SetActive(i < num);
			}
			return;
		}
		for (int j = 0; j < this.orderedStatsFieldUis.Count; j++)
		{
			this.orderedStatsFieldUis[j].titleText.text = this.orderedStatsTitles[j];
			this.orderedStatsFieldUis[j].gameObject.SetActive(true);
		}
	}

	// Token: 0x060010B6 RID: 4278 RVA: 0x0004C97C File Offset: 0x0004AB7C
	public void UpdateLabels()
	{
		if (this.chosenPlanetName == "Event")
		{
			this.notUnlockedYetCanvasGroup.alpha = 0f;
			this.statsCanvasGroup.alpha = 1f;
			this.orderedStatsFieldUis[0].infoText.text = this.eventsParticipated.ToString();
			this.orderedStatsFieldUis[1].infoText.text = this.eventsCompleted.ToString();
			return;
		}
		if (this.isCurrentPlanetChosen)
		{
			this.notUnlockedYetCanvasGroup.alpha = 0f;
			this.statsCanvasGroup.alpha = 1f;
			GameState game = GameController.Instance.game;
			int count = GameController.Instance.UnlockService.Unlocks.FindAll((Unlock a) => a.Earned.Value).Count;
			this.orderedStatsFieldUis[0].infoText.text = NumberFormat.Convert(game.CashOnHand.Value, 1000000.0, true, 3);
			this.orderedStatsFieldUis[1].infoText.text = NumberFormat.Convert(game.SessionCash.Value, 1000000.0, true, 3);
			this.orderedStatsFieldUis[2].infoText.text = NumberFormat.Convert(game.TotalPreviousCash.Value + game.SessionCash.Value, 1000000.0, true, 3);
			game.planetPlayerData.GetObservable("ResetCount", 0.0).Subscribe(delegate(double x)
			{
				this.orderedStatsFieldUis[3].infoText.text = string.Concat(x);
			}).AddTo(base.gameObject);
			this.orderedStatsFieldUis[4].infoText.text = NumberFormat.ConvertNormal(GameController.Instance.AngelService.AngelsOnHand.Value, 999999.0, 3);
			this.orderedStatsFieldUis[5].infoText.text = string.Format(" {0} Angels", NumberFormat.ConvertNormal(GameController.Instance.AngelService.AngelsSpent.Value, 999999.0, 3));
			this.orderedStatsFieldUis[6].infoText.text = NumberFormat.Convert((double)count, 999999.0, false, 3);
			return;
		}
		GameStateSaveData gameStateSaveData = this.otherPlanetsSaveData[this.chosenPlanetName];
		if (gameStateSaveData != null)
		{
			this.notUnlockedYetCanvasGroup.alpha = 0f;
			this.statsCanvasGroup.alpha = 1f;
			int count2 = gameStateSaveData.permanentUnlocks.Count;
			this.orderedStatsFieldUis[0].infoText.text = NumberFormat.Convert(gameStateSaveData.cashOnHand, 1000000.0, true, 3);
			this.orderedStatsFieldUis[1].infoText.text = NumberFormat.Convert(gameStateSaveData.sessionCash, 1000000.0, true, 3);
			this.orderedStatsFieldUis[2].infoText.text = NumberFormat.Convert(gameStateSaveData.totalPreviousCash + gameStateSaveData.sessionCash, 1000000.0, true, 3);
			this.orderedStatsFieldUis[3].infoText.text = gameStateSaveData.angelResetCount.ToString();
			this.orderedStatsFieldUis[4].infoText.text = NumberFormat.ConvertNormal(gameStateSaveData.angelInvestors, 999999.0, 3);
			this.orderedStatsFieldUis[5].infoText.text = string.Format(" {0} Angels", NumberFormat.ConvertNormal(gameStateSaveData.angelInvestorsSpent, 999999.0, 3));
			this.orderedStatsFieldUis[6].infoText.text = NumberFormat.Convert((double)count2, 999999.0, false, 3);
			return;
		}
		this.notUnlockedYetCanvasGroup.alpha = 1f;
		this.statsCanvasGroup.alpha = 0f;
		string text = this.chosenPlanetName;
		if (text == "Moon")
		{
			text = "The Moon";
		}
		this.notUnlockedTitleText.text = text;
	}

	// Token: 0x060010B7 RID: 4279 RVA: 0x0004CDAC File Offset: 0x0004AFAC
	private void OnEventCompletedCheck(bool complete)
	{
		if (complete && GameController.Instance.game.IsEventPlanet)
		{
			string name = string.Format("{0}_hasBeenCompleted", GameController.Instance.game.planetName);
			if (!GameController.Instance.GlobalPlayerData.GetBool(name))
			{
				this.eventsCompleted++;
				this.orderedStatsFieldUis[1].infoText.text = this.eventsCompleted.ToString();
				GameController.Instance.GlobalPlayerData.SetBool(name, true);
			}
		}
	}

	// Token: 0x060010B8 RID: 4280 RVA: 0x0004CE39 File Offset: 0x0004B039
	private void OnEventParticipated(EventParticipatedEvent evt)
	{
		this.eventsParticipated++;
		this.orderedStatsFieldUis[0].infoText.text = this.eventsParticipated.ToString();
	}

	// Token: 0x04000E41 RID: 3649
	private readonly string[] orderedStatsTitles = new string[]
	{
		"Cash on Hand",
		"Session Earnings",
		"Lifetime Earnings",
		"Total Resets",
		"Lifetime Angels Earned",
		"Lifetime Angels Sacrificed",
		"Total Unlocks"
	};

	// Token: 0x04000E42 RID: 3650
	private readonly string[] orderedEventPlanetTitles = new string[]
	{
		"Events Played",
		"Events Completed"
	};

	// Token: 0x04000E43 RID: 3651
	private readonly string[] planetNameStrings = new string[]
	{
		"Earth",
		"Moon",
		"Mars",
		"Event"
	};

	// Token: 0x04000E44 RID: 3652
	[SerializeField]
	private StatsFieldUI statsFieldUIPrefab;

	// Token: 0x04000E45 RID: 3653
	[SerializeField]
	private Transform statsFieldUIParent;

	// Token: 0x04000E46 RID: 3654
	[SerializeField]
	private Dropdown choosePlanetDropdown;

	// Token: 0x04000E47 RID: 3655
	[SerializeField]
	private CanvasGroup notUnlockedYetCanvasGroup;

	// Token: 0x04000E48 RID: 3656
	[SerializeField]
	private Text notUnlockedTitleText;

	// Token: 0x04000E49 RID: 3657
	[SerializeField]
	private Button goToMissionControlButton;

	// Token: 0x04000E4A RID: 3658
	[SerializeField]
	private CanvasGroup statsCanvasGroup;

	// Token: 0x04000E4B RID: 3659
	[SerializeField]
	private GridLayoutGroup gridLayoutGroup;

	// Token: 0x04000E4C RID: 3660
	[SerializeField]
	private Button hardResetStatButton;

	// Token: 0x04000E4D RID: 3661
	[SerializeField]
	private GameObject hardResetStatObject;

	// Token: 0x04000E4E RID: 3662
	[SerializeField]
	private Text hardResetTitleText;

	// Token: 0x04000E4F RID: 3663
	[SerializeField]
	private Text hardResetSubText;

	// Token: 0x04000E50 RID: 3664
	[SerializeField]
	private List<Button> achievmentButtons;

	// Token: 0x04000E51 RID: 3665
	[SerializeField]
	private List<Button> leaderboardButtons;

	// Token: 0x04000E52 RID: 3666
	[SerializeField]
	private PlatformGameObjectList platformObjectsList;

	// Token: 0x04000E53 RID: 3667
	private List<StatsFieldUI> orderedStatsFieldUis = new List<StatsFieldUI>();

	// Token: 0x04000E54 RID: 3668
	private Dictionary<string, GameStateSaveData> otherPlanetsSaveData = new Dictionary<string, GameStateSaveData>();

	// Token: 0x04000E55 RID: 3669
	private int eventsParticipated;

	// Token: 0x04000E56 RID: 3670
	private int eventsCompleted;

	// Token: 0x04000E57 RID: 3671
	private string planetName;

	// Token: 0x04000E58 RID: 3672
	private string chosenPlanetName;

	// Token: 0x04000E59 RID: 3673
	private bool isCurrentPlanetChosen;

	// Token: 0x04000E5A RID: 3674
	private Platforms.Logger.Logger logger;
}
