using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Token: 0x02000243 RID: 579
public class MainUIController : MonoBehaviour
{
	// Token: 0x17000161 RID: 353
	// (get) Token: 0x06001045 RID: 4165 RVA: 0x0004A285 File Offset: 0x00048485
	// (set) Token: 0x06001046 RID: 4166 RVA: 0x0004A2A3 File Offset: 0x000484A3
	public static MainUIController instance
	{
		get
		{
			if (MainUIController._instance == null)
			{
				MainUIController._instance = Object.FindObjectOfType<MainUIController>();
			}
			return MainUIController._instance;
		}
		private set
		{
			MainUIController._instance = value;
		}
	}

	// Token: 0x17000162 RID: 354
	// (get) Token: 0x06001047 RID: 4167 RVA: 0x0004A2AB File Offset: 0x000484AB
	private GameController gameController
	{
		get
		{
			return GameController.Instance;
		}
	}

	// Token: 0x06001048 RID: 4168 RVA: 0x0004A2B4 File Offset: 0x000484B4
	private void Start()
	{
		MainUIController.instance = this;
		GameController.Instance.State.Subscribe(delegate(GameState state)
		{
			GameObject gameObject = GameObject.Find("Splash");
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
			GameObject gameObject2 = GameObject.Find("Launching");
			if (gameObject2 != null)
			{
				Object.Destroy(gameObject2);
			}
		}).AddTo(base.gameObject);
		GameController.Instance.State.Take(1).Subscribe(new Action<GameState>(this.OnGameStateReady)).AddTo(base.gameObject);
	}

	// Token: 0x06001049 RID: 4169 RVA: 0x0004A32E File Offset: 0x0004852E
	private void Awake()
	{
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
	}

	// Token: 0x0600104A RID: 4170 RVA: 0x0004A358 File Offset: 0x00048558
	private void OnGameStateReady(GameState state)
	{
		this.InitialSetup();
		Observable.FromEvent<VentureModel>(delegate(Action<VentureModel> h)
		{
			state.OnVentureBoosted += h;
		}, delegate(Action<VentureModel> h)
		{
			state.OnVentureBoosted -= h;
		}).Subscribe(delegate(VentureModel __)
		{
			this.IsBoostedState.Value = false;
		}).AddTo(this);
		if (state.PlanetMilestones.Count > 0)
		{
			this.milestoneProgressBar.gameObject.SetActive(true);
			this.milestoneProgressBar.gameObject.SetActive(true);
			this.milestoneProgressBar.WireData();
			this.btn_planetMilestoneRoot.OnClickAsObservable().Subscribe(delegate(Unit _)
			{
				GameController.Instance.AnalyticService.SendNavActionAnalytics("OpenMissionModal", "RootProgressBar", GameController.Instance.planetName);
				GameController.Instance.NavigationService.CreateModal<EventMissionsModal>(NavModals.EVENT_MISSIONS_MODAL, false);
			}).AddTo(base.gameObject);
		}
		else
		{
			this.milestoneProgressBar.gameObject.SetActive(false);
		}
		GameController.Instance.PlanetThemeService.VentureIcons.Subscribe(delegate(IconDataScriptableObject icons)
		{
			this.ventureIcons = icons;
		}).AddTo(base.gameObject);
		if (GameController.Instance.UnlockService.Unlocks.All((Unlock u) => u.EverClaimed.Value))
		{
			this.OnOrientationChanged(OrientationController.Instance.CurrentOrientation);
		}
		else
		{
			GameController.Instance.UnlockService.OnUnlockAchieved.DelayFrame(1, FrameCountType.Update).Subscribe(delegate(Unlock _)
			{
				if (GameController.Instance.UnlockService.Unlocks.All((Unlock u) => u.EverClaimed.Value))
				{
					this.OnOrientationChanged(OrientationController.Instance.CurrentOrientation);
				}
			}).AddTo(base.gameObject);
		}
		this.unlockMissionsView.ShouldBeShown.Subscribe(delegate(bool _)
		{
			this.OnOrientationChanged(OrientationController.Instance.CurrentOrientation);
		}).AddTo(base.gameObject);
		this.OnOrientationChanged(OrientationController.Instance.CurrentOrientation);
	}

	// Token: 0x0600104B RID: 4171 RVA: 0x0004A520 File Offset: 0x00048720
	private void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		bool value = GameController.Instance.PlanetMilestoneService.DoesCurrentPlanetHaveMilestones.Value;
		bool flag = false;
		if (GameController.Instance.game != null)
		{
			if ((from x in GameController.Instance.game.Unlocks
			select x.EverClaimed.Value).Count((bool x) => !x) > 0)
			{
				flag = GameController.Instance.UnlockService.ShowsUnlockMissions;
			}
			else
			{
				flag = (GameController.Instance.UnlockService.ShowsUnlockMissions && this.unlockMissionsView.ShouldBeShown.Value);
			}
			this.ShowUnlocksMissionBar.Value = flag;
		}
		if (orientation.IsPortrait)
		{
			this.milestoneProgressBar.GetComponent<RectTransform>().offsetMin = new Vector2(-10f, (float)(flag ? -345 : -215));
			this.milestoneProgressBar.GetComponent<RectTransform>().offsetMax = new Vector2(0f, (float)(flag ? -315 : -185));
			if (!flag)
			{
				this.unlockMissionsView.gameObject.SetActive(false);
			}
			else
			{
				this.unlockMissionsView.gameObject.SetActive(true);
			}
			this.unlockMissionsView.GetComponent<RectTransform>().offsetMin = new Vector2(-10f, -315f);
			this.unlockMissionsView.GetComponent<RectTransform>().offsetMax = new Vector2(0f, -185f);
			return;
		}
		this.milestoneProgressBar.GetComponent<RectTransform>().offsetMin = new Vector2(215f, -155f);
		this.milestoneProgressBar.GetComponent<RectTransform>().offsetMax = new Vector2(0f, -125f);
	}

	// Token: 0x0600104C RID: 4172 RVA: 0x00002718 File Offset: 0x00000918
	private void ShowRatePanel(Unit u)
	{
	}

	// Token: 0x0600104D RID: 4173 RVA: 0x0004A6FB File Offset: 0x000488FB
	private void InitialSetup()
	{
		this.SetupPlanetImages();
		this.RescaleUI();
		base.StartCoroutine(this.HideModals());
	}

	// Token: 0x0600104E RID: 4174 RVA: 0x0004A716 File Offset: 0x00048916
	private void SetupPlanetImages()
	{
		GameController.Instance.PlanetThemeService.BackgroundImage.Subscribe(delegate(Sprite bg)
		{
			foreach (Image image in this.backgroundImages)
			{
				image.sprite = bg;
			}
		}).AddTo(base.gameObject);
	}

	// Token: 0x0600104F RID: 4175 RVA: 0x0004A744 File Offset: 0x00048944
	private void RescaleUI()
	{
		CanvasScaler[] array = Object.FindObjectsOfType<CanvasScaler>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
	}

	// Token: 0x06001050 RID: 4176 RVA: 0x0004A76E File Offset: 0x0004896E
	private IEnumerator HideModals()
	{
		yield return null;
		this.currentModal.Clear();
		yield break;
	}

	// Token: 0x06001051 RID: 4177 RVA: 0x0004A77D File Offset: 0x0004897D
	public void ToggleBoostUI()
	{
		this.IsBoostedState.Value = !this.IsBoostedState.Value;
	}

	// Token: 0x06001052 RID: 4178 RVA: 0x0004A798 File Offset: 0x00048998
	public Sprite GetUnlockSprite(Unlock unlock)
	{
		string text;
		if (unlock is SingleVentureUnlock)
		{
			SingleVentureUnlock vba = (SingleVentureUnlock)unlock;
			text = this.gameController.game.VentureModels.FirstOrDefault((VentureModel v) => v.Name == vba.ventureName).ImageName;
		}
		else
		{
			if (unlock is EventUnlock)
			{
				return ((EventUnlock)unlock).theSprite;
			}
			text = "Mogul";
		}
		if (!this.ventureIcons.iconMap.ContainsKey(text.ToLower()))
		{
			Debug.LogError("[Error] Function GetUnlockSprite cannot find " + text);
			return null;
		}
		return this.ventureIcons.iconMap[text.ToLower()];
	}

	// Token: 0x06001053 RID: 4179 RVA: 0x0004A844 File Offset: 0x00048A44
	public void ShowKongregateWindow()
	{
		if (Application.isEditor)
		{
			Debug.LogWarning("Cannot show Kongregate Window in Editor!");
			return;
		}
		Debug.Log("MainUIController:ShowKongregateWindow");
	}

	// Token: 0x06001054 RID: 4180 RVA: 0x00002718 File Offset: 0x00000918
	public void OnClickKiziTutorial()
	{
	}

	// Token: 0x06001055 RID: 4181 RVA: 0x0004A8A3 File Offset: 0x00048AA3
	public void HideLoadingPanel()
	{
		if (null != this.loadingModal)
		{
			this.loadingModal.CloseModal(Unit.Default);
			this.loadingModal = null;
		}
	}

	// Token: 0x06001056 RID: 4182 RVA: 0x0004A8CA File Offset: 0x00048ACA
	public void ShowAdLoadingModal(string message)
	{
		this.loadingModal = this.gameController.NavigationService.CreateModal<LoadingModal>(NavModals.LOADING, false);
		this.loadingModal.WireData(message);
	}

	// Token: 0x06001057 RID: 4183 RVA: 0x0004A8F4 File Offset: 0x00048AF4
	public void OnClickShowModal(RectTransform modal)
	{
		if (this.currentModal.First != null && this.currentModal.First.Value == modal)
		{
			return;
		}
		if (this.currentModal.Contains(modal))
		{
			this.currentModal.Remove(modal);
		}
		this.currentModal.AddFirst(modal);
		modal.Show();
	}

	// Token: 0x06001058 RID: 4184 RVA: 0x0004A958 File Offset: 0x00048B58
	public void ShowAskFacebook()
	{
		string title = "Would you like to connect through Facebook?";
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData(title, base.gameObject, "OnConfirmFacebook", PopupModal.PopupOptions.OK_Cancel, "Yes", "No", true);
	}

	// Token: 0x06001059 RID: 4185 RVA: 0x0004A99D File Offset: 0x00048B9D
	public void OnClickForceCrash()
	{
		base.StartCoroutine(this.CorutineNullCrash());
	}

	// Token: 0x0600105A RID: 4186 RVA: 0x0004A9AC File Offset: 0x00048BAC
	private IEnumerator CorutineNullCrash()
	{
		((string)null).ToLower();
		yield break;
	}

	// Token: 0x0600105B RID: 4187 RVA: 0x0004A9B4 File Offset: 0x00048BB4
	public void OnGoGetUpdate()
	{
		RuntimePlatform platform = Application.platform;
		if (platform != RuntimePlatform.IPhonePlayer)
		{
			if (platform != RuntimePlatform.Android)
			{
				Application.OpenURL("http://hyperhippo.ca/games/adventure-capitalist/");
			}
			else
			{
				Application.OpenURL(string.Format("market://details?id={0}", Application.identifier));
			}
		}
		else
		{
			Application.OpenURL("https://itunes.apple.com/app/id927006017");
		}
		Application.Quit();
	}

	// Token: 0x0600105C RID: 4188 RVA: 0x00007EBA File Offset: 0x000060BA
	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	// Token: 0x0600105D RID: 4189 RVA: 0x0004AA03 File Offset: 0x00048C03
	public VentureView GetVentureViewFromModel(VentureModel model)
	{
		return this.venturesPanel.VentureModelViewMap[model];
	}

	// Token: 0x04000DE2 RID: 3554
	private static MainUIController _instance;

	// Token: 0x04000DE3 RID: 3555
	[Header("Planet Visuals")]
	[SerializeField]
	private List<Image> backgroundImages;

	// Token: 0x04000DE4 RID: 3556
	[SerializeField]
	private VenturesPanel venturesPanel;

	// Token: 0x04000DE5 RID: 3557
	[SerializeField]
	private PlanetMilestonesRootProgressBar milestoneProgressBar;

	// Token: 0x04000DE6 RID: 3558
	[SerializeField]
	private RootUIUnlockMissionsView unlockMissionsView;

	// Token: 0x04000DE7 RID: 3559
	[SerializeField]
	private Button btn_planetMilestoneRoot;

	// Token: 0x04000DE8 RID: 3560
	[SerializeField]
	private IconDataScriptableObject ventureIcons;

	// Token: 0x04000DE9 RID: 3561
	public ReactiveProperty<bool> ShowUnlocksMissionBar = new ReactiveProperty<bool>();

	// Token: 0x04000DEA RID: 3562
	private LoadingModal loadingModal;

	// Token: 0x04000DEB RID: 3563
	public LinkedList<RectTransform> currentModal = new LinkedList<RectTransform>();

	// Token: 0x04000DEC RID: 3564
	public WelcomeBack welcomeBack;

	// Token: 0x04000DED RID: 3565
	[HideInInspector]
	public BoolReactiveProperty IsBoostedState = new BoolReactiveProperty(false);
}
