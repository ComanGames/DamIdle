using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AdCap.Ads;
using HHTools.MiniJSON;
using Platforms;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Token: 0x0200009A RID: 154
public class FTUE_Manager : MonoBehaviour
{
	// Token: 0x17000069 RID: 105
	// (get) Token: 0x0600041E RID: 1054 RVA: 0x00015BC6 File Offset: 0x00013DC6
	// (set) Token: 0x0600041F RID: 1055 RVA: 0x00015BCD File Offset: 0x00013DCD
	public static FTUE_Manager Instance { get; internal set; }

	// Token: 0x06000420 RID: 1056 RVA: 0x00015BD8 File Offset: 0x00013DD8
	public void Init()
	{
		if (FTUE_Manager.Instance != null)
		{
			Object.Destroy(this);
			return;
		}
		FTUE_Manager.Instance = this;
		(from x in GameController.Instance.State
		where x != null
		select x).DelayFrame(1, FrameCountType.Update).Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
	}

	// Token: 0x06000421 RID: 1057 RVA: 0x00015C51 File Offset: 0x00013E51
	private void OnDestroy()
	{
		this.inputCheckDisposable.Dispose();
		this.ftueDisposable.Dispose();
		this.gameStateDisposable.Dispose();
		this.disposables.Dispose();
	}

	// Token: 0x06000422 RID: 1058 RVA: 0x00015C80 File Offset: 0x00013E80
	private void OnStateChanged(GameState state)
	{
		this.gameStateDisposable.Clear();
		this.ftueDisposable.Clear();
		this.angelInvestorService = GameController.Instance.AngelService;
		this.ShowEventAngelResetFTUE(state);
		(from b in this.angelInvestorService.FirstTimeAngelReset
		where b
		select b).Subscribe(delegate(bool b)
		{
			if (state.planetName == "Earth")
			{
				FTUE_Manager.ShowFTUE("Investors>50", null);
			}
		}).AddTo(this.gameStateDisposable);
		(from b in this.angelInvestorService.IsAngelThresholdReached
		where b
		select b).Subscribe(delegate(bool b)
		{
			if (GameController.Instance.game.IsEventPlanet)
			{
				this.ShowSignificantAngelResetReminderFTUE();
			}
		}).AddTo(this);
		(from b in this.angelInvestorService.AngelsDoubledNotification
		where b
		select b).First<bool>().Subscribe(delegate(bool b)
		{
			FTUE_Manager.ShowFTUE("Investorsx2", null);
		}).AddTo(this);
		this.initialParent = base.transform.parent;
		string text = GameController.Instance.GlobalPlayerData.Get("FTUE", "");
		if (!string.IsNullOrEmpty(text))
		{
			(Json.Deserialize(text) as List<object>).ForEach(delegate(object ftue)
			{
				this.CompletedFTUEsteps.Add(ftue as string);
			});
		}
		if (state.planetName == "Earth")
		{
			if (this.debug || !this.CompletedFTUEsteps.Contains("FirstFTUE") || this.GetFTUE("FirstFTUE").debug)
			{
				base.StartCoroutine(this.FirstFTUE());
			}
			if (this.debug || !this.CompletedFTUEsteps.Contains("Managers>5") || this.GetFTUE("Managers>5").debug)
			{
				base.StartCoroutine(this.MonitorManagersFTUE());
			}
		}
		if (Helper.GetPlatformType() == PlatformType.Android || Helper.GetPlatformType() == PlatformType.Ios)
		{
			state.OnVenturePurchased.Subscribe(new Action<VentureModel>(this.OnVenturePurchase)).AddTo(this);
		}
		if (!this.CompletedFTUEsteps.Contains("Ads"))
		{
			(from e in MessageBroker.Default.Receive<AdWatchedEvent>()
			where e.AdWatchEventType == EAdWatchEventType.Completed
			select e).Take(1).Subscribe(delegate(AdWatchedEvent e)
			{
				this.CompletedFTUEsteps.Add("Ads");
			}).AddTo(this);
		}
		GameController.Instance.UpgradeService.OnUpgradePurchased.Subscribe(new Action<Upgrade>(this.OnUpgradePurchase)).AddTo(this);
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(this);
		this.initialized = true;
		if (this.queuedFTUE != null)
		{
			FTUE_Manager.ShowFTUE(this.queuedFTUE, this.queuedCallback);
			this.queuedFTUE = null;
			this.queuedCallback = null;
		}
		if ((Helper.GetPlatformType() == PlatformType.Android || Helper.GetPlatformType() == PlatformType.Ios) && FTUE_Manager.Instance && !GameController.Instance.ProfitBoostAdService.VideoMultiplierActive && GameController.Instance.ProfitBoostAdService.AvailableProfitAds.Value > 0 && state.planetName == "Moon")
		{
			Observable.FromCoroutine(new Func<IEnumerator>(this.AdMultiplierReminder), false).Subscribe<Unit>().AddTo(base.gameObject);
		}
		if (state.planetName == "Mars")
		{
			if (this.CompletedFTUEsteps.Contains("FirstEarnedSuitPiece"))
			{
				GameController.Instance.NavigationService.CurrentLocation.First(x => x == "Root").Subscribe(delegate(string __)
				{
					FTUE_Manager.ShowFTUE("ProfitMartians", null);
				}).AddTo(this.gameStateDisposable);
				return;
			}
			MessageBroker.Default.Receive<FTUEFinished>().First(x => x.FTUEName == "FirstEarnedSuitPiece").Subscribe(delegate(FTUEFinished _)
			{
				GameController.Instance.NavigationService.CurrentLocation.First(x => x == "Root").Subscribe(delegate(string __)
				{
					FTUE_Manager.ShowFTUE("ProfitMartians", null);
				}).AddTo(this.gameStateDisposable);
			}).AddTo(this.gameStateDisposable);
		}
	}

	// Token: 0x06000423 RID: 1059 RVA: 0x00016100 File Offset: 0x00014300
	private void ShowEventAngelResetFTUE(GameState state)
	{
		if (state.IsEventPlanet && !this.CompletedFTUEsteps.Contains("EventAngelResetTutorial_A"))
		{
			this.angelInvestorService.IsFirstEventAngelResetComplete.Subscribe(delegate(bool isFirstResetComplete)
			{
				if (!isFirstResetComplete)
				{
					this.angelInvestorService.RewardAngelsAtInterval.Subscribe(delegate(double angels)
					{
						if (angels > this.angelInvestorService.eventAngelResetThreshold && !this.CompletedFTUEsteps.Contains("EventAngelResetTutorial_A"))
						{
							FTUE_Manager.ShowFTUE("EventAngelResetTutorial_A", null);
						}
					}).AddTo(this);
				}
			}).AddTo(this.ftueDisposable);
		}
	}

	// Token: 0x06000424 RID: 1060 RVA: 0x00016150 File Offset: 0x00014350
	private void ShowSignificantAngelResetReminderFTUE()
	{
		double num = this.angelInvestorService.CalculateAngelInvestors(GameController.Instance.game.TotalPreviousCash.Value);
		string arg = NumberFormat.Convert(this.angelInvestorService.GetRewardAngelCount() / num * 100.0, 1E+15, false, 0);
		if (this.CompletedFTUEsteps.Contains("AngelResetThreshold"))
		{
			this.CompletedFTUEsteps.Remove("AngelResetThreshold");
		}
		FTUE_Manager.FTUE_Element ftue_Element = this.ftueElements.FirstOrDefault(x => x.ftueName == "AngelResetThreshold");
		if (ftue_Element != null && ftue_Element.ftueContents.Count > 0)
		{
			if (num > 0.0)
			{
				string format = "Claim your <color=orange>Investors</color> now and start over with a <color=orange>{0}%</color> profit boost! It's financially foolproof!";
				ftue_Element.ftueContents[0].text = string.Format(format, arg);
			}
			else
			{
				ftue_Element.ftueContents[0].text = "Claim your <color=orange>Investors</color> now and start over with a <color=orange>HUGE</color> profit boost! It's fiscally fantastic!";
			}
		}
		FTUE_Manager.ShowFTUE("AngelResetThreshold", null);
	}

	// Token: 0x06000425 RID: 1061 RVA: 0x00016254 File Offset: 0x00014454
	private void CheckFtueStatus()
	{
		if (this.curElement == null)
		{
			return;
		}
		FTUE_Manager.FTUE_Content ftue_Content = this.curElement.ftueContents[this.curContentIndex];
		if (!ftue_Content.nextFtueOnClick && !ftue_Content.dismissOnClick)
		{
			return;
		}
		if ((Input.touchCount <= 0 || Input.touches[0].phase != TouchPhase.Began) && !Input.GetMouseButtonDown(0) && !Input.GetButtonDown("Submit"))
		{
			return;
		}
		if (!this.curElement.ftueContents[this.curContentIndex].nextFtueOnClick)
		{
			if (this.curElement.ftueContents[this.curContentIndex].dismissOnClick)
			{
				this.HideFTUE();
			}
			return;
		}
		this.curContentIndex++;
		if (this.curContentIndex >= this.curElement.ftueContents.Count)
		{
			this.CompleteFTUE();
			return;
		}
		this.UpdateFTUEContent();
	}

	// Token: 0x06000426 RID: 1062 RVA: 0x00016338 File Offset: 0x00014538
	public static bool ShowFTUE(string elementName, Action callback = null)
	{
		if (string.IsNullOrEmpty(elementName))
		{
			return false;
		}
		if (!FTUE_Manager.Instance)
		{
			return false;
		}
		if (FTUE_Manager.Instance.initialized)
		{
			return FTUE_Manager.Instance.ShowFTUE(FTUE_Manager.Instance.GetFTUE(elementName), callback);
		}
		FTUE_Manager.Instance.queuedFTUE = elementName;
		FTUE_Manager.Instance.queuedCallback = callback;
		return false;
	}

	// Token: 0x06000427 RID: 1063 RVA: 0x00016398 File Offset: 0x00014598
	private bool ShowFTUE(FTUE_Manager.FTUE_Element element, Action callback = null)
	{
		if (element == null)
		{
			return false;
		}
		if (this.CompletedFTUEsteps.Contains(element.ftueName) && !this.debug && !element.debug)
		{
			return false;
		}
		Debug.Log("[FtueManager] Showing Ftue " + element.ftueName);
		if (this.curElement != null)
		{
			this.CompleteFTUE();
		}
		this.curElement = element;
		this.curElementIndex = (this.ftueElements.IndexOf(element) + 1) * 10;
		this.curContentIndex = 0;
		this.UpdateFTUEContent();
		this.OnOrientationChanged(OrientationController.Instance.CurrentOrientation);
		this.OnCompleteCallback = callback;
		this.inputCheckDisposable.Dispose();
		this.inputCheckDisposable = Observable.EveryUpdate().Skip(1).Subscribe(delegate(long _)
		{
			this.CheckFtueStatus();
		}).AddTo(this);
		return true;
	}

	// Token: 0x06000428 RID: 1064 RVA: 0x00016468 File Offset: 0x00014668
	private FTUE_Manager.FTUE_Element GetFTUE(string ftueName)
	{
		FTUE_Manager.FTUE_Element ftue_Element = this.ftueElements.Find(e => e.ftueName == ftueName);
		if (ftue_Element != null)
		{
			return ftue_Element;
		}
		Debug.LogWarningFormat("Could not find FTUE element [{0}]", new object[]
		{
			ftueName
		});
		return null;
	}

	// Token: 0x06000429 RID: 1065 RVA: 0x000164B9 File Offset: 0x000146B9
	public static bool GetSeenFTUE(string ftueName)
	{
		return FTUE_Manager.Instance == null || FTUE_Manager.Instance.CompletedFTUEsteps.Contains(ftueName);
	}

	// Token: 0x0600042A RID: 1066 RVA: 0x000164DA File Offset: 0x000146DA
	public static void SetSeenFTUE(string ftueName)
	{
		if (FTUE_Manager.Instance != null && !FTUE_Manager.Instance.CompletedFTUEsteps.Contains(ftueName))
		{
			FTUE_Manager.Instance.CompletedFTUEsteps.Add(ftueName);
		}
	}

	// Token: 0x0600042B RID: 1067 RVA: 0x0001650B File Offset: 0x0001470B
	public static void RemoveSeenFTUE(string ftueName)
	{
		if (FTUE_Manager.Instance != null && FTUE_Manager.Instance.CompletedFTUEsteps.Contains(ftueName))
		{
			FTUE_Manager.Instance.CompletedFTUEsteps.Remove(ftueName);
		}
	}

	// Token: 0x0600042C RID: 1068 RVA: 0x00016540 File Offset: 0x00014740
	private void UpdateFTUEContent()
	{
		this.ftueDisposable.Clear();
		if (this.curElement == null || this.curContentIndex == -1 || this.curContentIndex > this.curElement.ftueContents.Count)
		{
			return;
		}
		FTUE_Manager.FTUE_Content ftue_Content = this.curElement.ftueContents[this.curContentIndex];
		GameObject gameObject = ftue_Content.speechBubbleTop ? this.speechBubbleTop : this.speechBubbleBottom;
		if (gameObject)
		{
			this.speechBubbleTop.SetActive(gameObject == this.speechBubbleTop);
			this.speechBubbleBottom.SetActive(gameObject == this.speechBubbleBottom);
			Text componentInChildren = gameObject.GetComponentInChildren<Text>();
			if (componentInChildren)
			{
				componentInChildren.text = ftue_Content.text;
				componentInChildren.fontSize = ftue_Content.fontSize;
			}
		}
		for (int i = 0; i < this.ftueGuys.Length; i++)
		{
			if (!this.ftueGuys[0].transform.parent.gameObject.activeInHierarchy)
			{
				this.ftueGuys[0].transform.parent.gameObject.SetActive(true);
			}
			this.ftueGuys[i].SetActive(i == ftue_Content.guyIndex);
			this.ftueGuys[i].transform.Find("Helmet").gameObject.SetActive(ftue_Content.showHelmet);
		}
		if (ftue_Content.eatClicks && !this.eatingClicks)
		{
			this.graphicRayCasters.Clear();
			foreach (GraphicRaycaster graphicRaycaster in Object.FindObjectsOfType<GraphicRaycaster>())
			{
				if (null != graphicRaycaster && graphicRaycaster.enabled)
				{
					this.graphicRayCasters.Add(graphicRaycaster);
				}
			}
			foreach (GraphicRaycaster graphicRaycaster2 in this.graphicRayCasters)
			{
				if (graphicRaycaster2 != null)
				{
					graphicRaycaster2.enabled = false;
				}
			}
			this.eatingClicks = true;
		}
		this.leftArrow.SetActive(false);
		this.rightArrow.SetActive(false);
		if (OrientationController.Instance.CurrentOrientation.IsPortrait)
		{
			if (ftue_Content.isMenu && !this.hasClickedMenu)
			{
				GameObject gameObject2 = GameObject.Find("/Panel Canvases/MenuPanelController/MenuPanelPortrait/ClosedCanvasGroup/Content/Btn_Menu");
				if (gameObject2)
				{
					this.leftArrow.SetActive(true);
					this.leftArrow.transform.SetParent(gameObject2.transform);
					this.leftArrow.GetComponent<RectTransform>().localScale = Vector3.one;
					this.leftArrow.transform.position = gameObject2.transform.position;
				}
				GameController.Instance.NavigationService.CurrentLocation.First(x => x == "Menu").Subscribe(delegate(string _)
				{
					this.hasClickedMenu = true;
					this.UpdateFTUEContent();
				}).AddTo(this.ftueDisposable);
				if (this.background)
				{
					this.background.SetActive(true);
				}
				return;
			}
			if (this.leftArrow)
			{
				if (!string.IsNullOrEmpty(ftue_Content.portraitLeftArrowFocus))
				{
					GameObject gameObject3 = GameObject.Find(ftue_Content.portraitLeftArrowFocus);
					if (gameObject3)
					{
						this.leftArrow.SetActive(true);
						this.leftArrow.transform.SetParent(gameObject3.transform);
						this.leftArrow.GetComponent<RectTransform>().localScale = Vector3.one;
						this.leftArrow.transform.position = gameObject3.transform.position;
					}
				}
				else
				{
					this.leftArrow.SetActive(false);
				}
			}
			if (this.rightArrow)
			{
				if (!string.IsNullOrEmpty(ftue_Content.portraitRightArrowFocus))
				{
					GameObject gameObject4 = GameObject.Find(ftue_Content.portraitRightArrowFocus);
					if (gameObject4)
					{
						this.rightArrow.SetActive(true);
						this.rightArrow.transform.SetParent(gameObject4.transform);
						this.rightArrow.GetComponent<RectTransform>().localScale = FTUE_Manager.RIGHT_ARROW_SCALE;
						this.rightArrow.transform.position = gameObject4.transform.position;
					}
				}
				else
				{
					this.rightArrow.SetActive(false);
				}
			}
		}
		else
		{
			if (this.leftArrow)
			{
				if (!string.IsNullOrEmpty(ftue_Content.leftArrowFocus))
				{
					GameObject gameObject5 = GameObject.Find(ftue_Content.leftArrowFocus);
					if (gameObject5)
					{
						this.leftArrow.SetActive(true);
						this.leftArrow.transform.SetParent(gameObject5.transform);
						this.leftArrow.GetComponent<RectTransform>().localScale = Vector3.one;
						this.leftArrow.transform.position = gameObject5.transform.position;
					}
				}
				else
				{
					this.leftArrow.SetActive(false);
				}
			}
			if (this.rightArrow)
			{
				if (!string.IsNullOrEmpty(ftue_Content.rightArrowFocus))
				{
					GameObject gameObject6 = GameObject.Find(ftue_Content.rightArrowFocus);
					if (gameObject6)
					{
						this.rightArrow.SetActive(true);
						this.rightArrow.transform.SetParent(gameObject6.transform);
						this.rightArrow.GetComponent<RectTransform>().localScale = FTUE_Manager.RIGHT_ARROW_SCALE;
						this.rightArrow.transform.position = gameObject6.transform.position;
					}
				}
				else
				{
					this.rightArrow.SetActive(false);
				}
			}
		}
		if (ftue_Content.seenOnShow && !this.CompletedFTUEsteps.Contains(this.curElement.ftueName))
		{
			this.CompletedFTUEsteps.Add(this.curElement.ftueName);
			GameController.Instance.GlobalPlayerData.Set("FTUE", Json.Serialize(this.CompletedFTUEsteps));
			GameController.Instance.GlobalPlayerData.Save();
		}
		this.hasClickedMenu = false;
		if (this.background)
		{
			this.background.SetActive(true);
		}
	}

	// Token: 0x0600042D RID: 1069 RVA: 0x00016B50 File Offset: 0x00014D50
	private void NextFTUE()
	{
		if (this.curElement == null)
		{
			this.curContentIndex = -1;
			return;
		}
		foreach (GraphicRaycaster graphicRaycaster in this.graphicRayCasters)
		{
			if (graphicRaycaster != null)
			{
				graphicRaycaster.enabled = true;
			}
		}
		this.curContentIndex++;
		if (this.curElement.ftueContents.Count <= this.curContentIndex)
		{
			this.CompleteFTUE();
		}
		else
		{
			if (!this.debug && this.curElement != null && !this.curElement.debug)
			{
				GameController.Instance.AnalyticService.SendFTUEAnalyticEvent(this.curElement.ftueName, this.curElementIndex + this.curContentIndex - 1);
			}
			this.UpdateFTUEContent();
		}
		this.OnOrientationChanged(OrientationController.Instance.CurrentOrientation);
	}

	// Token: 0x0600042E RID: 1070 RVA: 0x00016C48 File Offset: 0x00014E48
	private void HideFTUE()
	{
		if (this.background)
		{
			this.background.SetActive(false);
		}
		if (this.leftArrow)
		{
			this.leftArrow.transform.SetParent(base.transform);
		}
		if (this.rightArrow)
		{
			this.rightArrow.transform.SetParent(base.transform);
		}
		for (int i = 1; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(false);
		}
		for (int j = 0; j < this.graphicRayCasters.Count; j++)
		{
			if (this.graphicRayCasters[j] != null)
			{
				this.graphicRayCasters[j].enabled = true;
			}
		}
		this.eatingClicks = false;
		base.transform.SetParent(this.initialParent);
	}

	// Token: 0x0600042F RID: 1071 RVA: 0x00016D38 File Offset: 0x00014F38
	private void CompleteFTUE()
	{
		this.HideFTUE();
		string ftueName = this.curElement.ftueName;
		if (this.curElement != null)
		{
			if (!this.CompletedFTUEsteps.Contains(this.curElement.ftueName))
			{
				this.CompletedFTUEsteps.Add(this.curElement.ftueName);
			}
			GameController.Instance.GlobalPlayerData.Set("FTUE", Json.Serialize(this.CompletedFTUEsteps));
			GameController.Instance.GlobalPlayerData.Save();
			if (!this.debug && this.curElement != null && !this.curElement.debug)
			{
				GameController.Instance.AnalyticService.SendFTUEAnalyticEvent(this.curElement.ftueName, this.curElementIndex + this.curContentIndex);
			}
		}
		this.curElement = null;
		this.curContentIndex = -1;
		if (this.OnCompleteCallback != null)
		{
			this.OnCompleteCallback();
			this.OnCompleteCallback = null;
		}
		MessageBroker.Default.Publish<FTUEFinished>(new FTUEFinished(ftueName));
		this.inputCheckDisposable.Dispose();
	}

	// Token: 0x06000430 RID: 1072 RVA: 0x00016E46 File Offset: 0x00015046
	private void CancelFTUE()
	{
		this.HideFTUE();
		this.curElement = null;
		this.curContentIndex = -1;
		if (this.OnCompleteCallback != null)
		{
			this.OnCompleteCallback();
			this.OnCompleteCallback = null;
		}
		this.inputCheckDisposable.Dispose();
	}

	// Token: 0x06000431 RID: 1073 RVA: 0x00016E84 File Offset: 0x00015084
	public void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		if (this.curElement == null || this.curContentIndex == -1)
		{
			return;
		}
		FTUE_Manager.FTUE_Content ftue_Content = this.curElement.ftueContents[this.curContentIndex];
		string text = orientation.IsPortrait ? ftue_Content.portraitParent : ftue_Content.landscapeParent;
		if (!string.IsNullOrEmpty(text))
		{
			GameObject gameObject = GameObject.Find(text);
			if (null == gameObject)
			{
				Debug.LogWarning(string.Concat(new string[]
				{
					"Unable to find FTUE transform parent \"",
					text,
					"\" for FTUE \"",
					this.curElement.ftueName,
					"\" FTUE will be added to the root"
				}));
			}
			base.transform.SetParent(gameObject ? gameObject.transform : this.initialParent);
		}
		else
		{
			base.transform.SetParent(this.initialParent);
		}
		base.transform.localScale = Vector3.one;
		if (!GameController.Instance.UnlockService.ShowsUnlockMissions)
		{
			base.GetComponent<RectTransform>().anchoredPosition3D = (orientation.IsPortrait ? ftue_Content.portraitPosition : ftue_Content.landscapePosition);
			return;
		}
		if (ftue_Content.portraitPosition.y > 0f)
		{
			base.GetComponent<RectTransform>().anchoredPosition3D = (orientation.IsPortrait ? new Vector3(ftue_Content.portraitPosition.x, ftue_Content.portraitPosition.y - 130f, ftue_Content.portraitPosition.z) : ftue_Content.landscapePosition);
			return;
		}
		base.GetComponent<RectTransform>().anchoredPosition3D = (orientation.IsPortrait ? ftue_Content.portraitPosition : ftue_Content.landscapePosition);
	}

	// Token: 0x06000432 RID: 1074 RVA: 0x00017018 File Offset: 0x00015218
	private void OnUpgradePurchase(Upgrade upgrade)
	{
		ManagerUpgrade managerUpgrade = upgrade as ManagerUpgrade;
		if (managerUpgrade != null)
		{
			if (this.curElement == this.GetFTUE("Managers"))
			{
				this.CompleteFTUE();
			}
			if (managerUpgrade.name == "Mama Sean")
			{
				if (FTUE_Manager.ShowFTUE("BuyMultiplier", null))
				{
					GameObject.Find(this.curElement.ftueContents[this.curContentIndex].leftArrowFocus).GetComponent<Button>().onClick.AddListener(new UnityAction(this.OnClickMultiplierButton));
					return;
				}
			}
			else if (managerUpgrade.name == "Forest Trump")
			{
				FTUE_Manager.ShowFTUE("Managers>5", null);
			}
		}
	}

	// Token: 0x06000433 RID: 1075 RVA: 0x000170C4 File Offset: 0x000152C4
	public void OnClickMultiplierButton()
	{
		if (this.curElement == this.GetFTUE("BuyMultiplier"))
		{
			this.CompleteFTUE();
		}
	}

	// Token: 0x06000434 RID: 1076 RVA: 0x000170E0 File Offset: 0x000152E0
	private void OnVenturePurchase(VentureModel venture)
	{
		if (!FTUE_Manager.GetSeenFTUE(this.AdWatchTrigger.FTUE_Name) && this.AdWatchTrigger != null && venture.Id == this.AdWatchTrigger.VentureTarget)
		{
			venture.TotalOwned.First(x => x >= this.AdWatchTrigger.Qty).Subscribe(delegate(double _)
			{
				if (FTUE_Manager.ShowFTUE(this.AdWatchTrigger.FTUE_Name, null))
				{
					this.SetButtonListner(this.curElement.ftueContents[this.curContentIndex].leftArrowFocus).ToObservable(false).StartAsCoroutine(default(CancellationToken));
				}
			}).AddTo(base.gameObject);
		}
		if (venture.Id == "bank" && !this.CompletedFTUEsteps.Contains("GoldShop"))
		{
			base.StartCoroutine(this.ShowGoldStoreFTUE());
		}
	}

	// Token: 0x06000435 RID: 1077 RVA: 0x00017184 File Offset: 0x00015384
	private IEnumerator SetButtonListner(string buttonId)
	{
		GameObject go = null;
		while (go == null)
		{
			go = GameObject.Find(buttonId);
			yield return null;
		}
		go.GetComponent<Button>().onClick.AddListener(new UnityAction(this.OnClickAdsButton));
		yield break;
	}

	// Token: 0x06000436 RID: 1078 RVA: 0x0001719A File Offset: 0x0001539A
	public void OnClickAdsButton()
	{
		if (this.curElement == this.GetFTUE("Ads"))
		{
			this.CompleteFTUE();
		}
	}

	// Token: 0x06000437 RID: 1079 RVA: 0x000171B5 File Offset: 0x000153B5
	public void OnClickStoreButton()
	{
		if (this.curElement == this.GetFTUE("GoldShop"))
		{
			this.CompleteFTUE();
		}
	}

	// Token: 0x06000438 RID: 1080 RVA: 0x000171D0 File Offset: 0x000153D0
	public void OnMissionModalClosed()
	{
		if (this.curElement == this.GetFTUE("EventMission_Step1") || this.curElement == this.GetFTUE("EventMission_Step2") || this.curElement == this.GetFTUE("EventMission_Step3"))
		{
			this.CancelFTUE();
		}
	}

	// Token: 0x06000439 RID: 1081 RVA: 0x0001721C File Offset: 0x0001541C
	public void OnFirstMissionPointsClaimed()
	{
		if (this.curElement == this.GetFTUE("EventMission_Step2"))
		{
			this.CompleteFTUE();
		}
	}

	// Token: 0x0600043A RID: 1082 RVA: 0x00017237 File Offset: 0x00015437
	public void OnFirstMissionRewardClaimed()
	{
		if (this.curElement == this.GetFTUE("EventMission_Step3"))
		{
			this.CompleteFTUE();
		}
	}

	// Token: 0x0600043B RID: 1083 RVA: 0x00017252 File Offset: 0x00015452
	private IEnumerator FirstFTUE()
	{
		yield return new WaitForSeconds(1f);
		FTUE_Manager.ShowFTUE("FirstFTUE", null);
		VentureModel lemons = GameController.Instance.game.VentureModels.FirstOrDefault(v => v.Id == "lemon");
		if (lemons == null)
		{
			yield break;
		}
		while (!this.initialized)
		{
			yield return null;
		}
		while (GameController.Instance.game.CashOnHand.Value == 0.0)
		{
			yield return null;
		}
		while (GameController.Instance.game.CashOnHand.Value < lemons.CostForNext.Value)
		{
			yield return null;
		}
		this.NextFTUE();
		while (lemons.TotalOwned.Value < 2.0)
		{
			yield return null;
		}
		this.NextFTUE();
		VentureModel news = GameController.Instance.game.VentureModels.FirstOrDefault(v => v.Id == "news");
		while (GameController.Instance.game.CashOnHand.Value < news.CostForNext.Value)
		{
			yield return null;
		}
		this.NextFTUE();
		while (news.TotalOwned.Value < 1.0)
		{
			yield return null;
		}
		this.NextFTUE();
		yield break;
	}

	// Token: 0x0600043C RID: 1084 RVA: 0x00017261 File Offset: 0x00015461
	private IEnumerator OfferwallFTUE()
	{
		string ftueName = "Offerwall";
		while (GameController.Instance == null)
		{
			yield return null;
		}
		if (this.CompletedFTUEsteps.Contains(ftueName))
		{
			yield break;
		}
		VentureModel oil = GameController.Instance.game.VentureModels.FirstOrDefault(v => v.Id == "oil");
		while (oil.TotalOwned.Value < 1.0)
		{
			yield return new WaitForSeconds(1f);
		}
		yield return new WaitForSeconds(3f);
		FTUE_Manager.ShowFTUE(ftueName, null);
		yield break;
	}

	// Token: 0x0600043D RID: 1085 RVA: 0x00017270 File Offset: 0x00015470
	private IEnumerator MonitorManagersFTUE()
	{
		while (GameController.Instance == null)
		{
			yield return null;
		}
		for (;;)
		{
			if (GameController.Instance.UpgradeService.Managers.FindAll(m => m.currency == Upgrade.Currency.InGameCash && !m.IsPurchased.Value && m.cost <= GameController.Instance.game.CashOnHand.Value).Count != 0)
			{
				break;
			}
			yield return new WaitForSeconds(1f);
		}
		FTUE_Manager.ShowFTUE("Managers", null);
		for (;;)
		{
			if (GameController.Instance.game.VentureModels.Count(v => v.IsManaged.Value) > 5)
			{
				break;
			}
			yield return new WaitForSeconds(1f);
		}
		FTUE_Manager.ShowFTUE("Managers>5", null);
		yield break;
	}

	// Token: 0x0600043E RID: 1086 RVA: 0x00017278 File Offset: 0x00015478
	private IEnumerator AdMultiplierReminder()
	{
		yield return new WaitForSeconds(this.secondsUntilAdWatchReminder);
		if (!GameController.Instance.ProfitBoostAdService.VideoMultiplierActive)
		{
			FTUE_Manager.ShowFTUE("ProfitAdReminder", null);
		}
		yield break;
	}

	// Token: 0x0600043F RID: 1087 RVA: 0x00017287 File Offset: 0x00015487
	private IEnumerator ShowGoldStoreFTUE()
	{
		yield return new WaitForSeconds(3f);
		FTUE_Manager.ShowFTUE("GoldShop", null);
		FTUE_Manager.FTUE_Content ftue_Content = this.curElement.ftueContents[this.curContentIndex];
		GameObject gameObject;
		if (OrientationController.Instance.CurrentOrientation.IsPortrait)
		{
			gameObject = GameObject.Find(ftue_Content.portraitRightArrowFocus);
		}
		else
		{
			gameObject = GameObject.Find(ftue_Content.rightArrowFocus);
		}
		if (gameObject != null)
		{
			gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(this.OnClickStoreButton));
		}
		yield break;
	}

	// Token: 0x040003A5 RID: 933
	public const string ANGEL_RESET_THRESHOLD_REMINDER_FTUE_PART = "AngelResetThreshold";

	// Token: 0x040003A6 RID: 934
	public const string EVENT_ANGEL_RESET_TUTORIAL_FTUE_A = "EventAngelResetTutorial_A";

	// Token: 0x040003A7 RID: 935
	public const string EVENT_ANGEL_RESET_TUTORIAL_FTUE_B = "EventAngelResetTutorial_B";

	// Token: 0x040003A8 RID: 936
	public FTUE_Manager.VentureUnlockTrigger AdWatchTrigger;

	// Token: 0x040003AA RID: 938
	public float secondsUntilAdWatchReminder = 120f;

	// Token: 0x040003AB RID: 939
	public static Vector3 RIGHT_ARROW_SCALE = new Vector3(-1f, 1f, 1f);

	// Token: 0x040003AC RID: 940
	public bool debug;

	// Token: 0x040003AD RID: 941
	public GameObject[] ftueGuys;

	// Token: 0x040003AE RID: 942
	public GameObject background;

	// Token: 0x040003AF RID: 943
	public GameObject speechBubbleTop;

	// Token: 0x040003B0 RID: 944
	public GameObject speechBubbleBottom;

	// Token: 0x040003B1 RID: 945
	public GameObject leftArrow;

	// Token: 0x040003B2 RID: 946
	public GameObject rightArrow;

	// Token: 0x040003B3 RID: 947
	public List<FTUE_Manager.FTUE_Element> ftueElements = new List<FTUE_Manager.FTUE_Element>();

	// Token: 0x040003B4 RID: 948
	public ReactiveCollection<string> CompletedFTUEsteps = new ReactiveCollection<string>();

	// Token: 0x040003B5 RID: 949
	private FTUE_Manager.FTUE_Element curElement;

	// Token: 0x040003B6 RID: 950
	private int curElementIndex;

	// Token: 0x040003B7 RID: 951
	private int curContentIndex = -1;

	// Token: 0x040003B8 RID: 952
	private Transform initialParent;

	// Token: 0x040003B9 RID: 953
	private Action OnCompleteCallback;

	// Token: 0x040003BA RID: 954
	private List<GraphicRaycaster> graphicRayCasters = new List<GraphicRaycaster>();

	// Token: 0x040003BB RID: 955
	private string queuedFTUE;

	// Token: 0x040003BC RID: 956
	private Action queuedCallback;

	// Token: 0x040003BD RID: 957
	private bool initialized;

	// Token: 0x040003BE RID: 958
	private IDisposable inputCheckDisposable = Disposable.Empty;

	// Token: 0x040003BF RID: 959
	private bool eatingClicks;

	// Token: 0x040003C0 RID: 960
	private const string PORTRAIT_MENU_PATH = "/Panel Canvases/MenuPanelController/MenuPanelPortrait/ClosedCanvasGroup/Content/Btn_Menu";

	// Token: 0x040003C1 RID: 961
	private bool hasClickedMenu;

	// Token: 0x040003C2 RID: 962
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040003C3 RID: 963
	private CompositeDisposable ftueDisposable = new CompositeDisposable();

	// Token: 0x040003C4 RID: 964
	private CompositeDisposable welcomeBackDisposable = new CompositeDisposable();

	// Token: 0x040003C5 RID: 965
	private CompositeDisposable gameStateDisposable = new CompositeDisposable();

	// Token: 0x040003C6 RID: 966
	private IAngelInvestorService angelInvestorService;

	// Token: 0x040003C7 RID: 967
	private UnfoldingId unfoldingStepId = new UnfoldingId
	{
		Id = "ShowAdFTUE"
	};

	// Token: 0x0200077B RID: 1915
	[Serializable]
	public class VentureUnlockTrigger
	{
		// Token: 0x040027B7 RID: 10167
		public string FTUE_Name;

		// Token: 0x040027B8 RID: 10168
		public string VentureTarget;

		// Token: 0x040027B9 RID: 10169
		public double Qty;
	}

	// Token: 0x0200077C RID: 1916
	[Serializable]
	public class FTUE_Content
	{
		// Token: 0x040027BA RID: 10170
		[TextArea(3, 10)]
		public string text = "This is FTUE text";

		// Token: 0x040027BB RID: 10171
		public int fontSize = 30;

		// Token: 0x040027BC RID: 10172
		public bool speechBubbleTop;

		// Token: 0x040027BD RID: 10173
		public bool showHelmet;

		// Token: 0x040027BE RID: 10174
		public int guyIndex;

		// Token: 0x040027BF RID: 10175
		public string landscapeParent;

		// Token: 0x040027C0 RID: 10176
		public Vector3 landscapePosition = Vector3.zero;

		// Token: 0x040027C1 RID: 10177
		public string portraitParent;

		// Token: 0x040027C2 RID: 10178
		public Vector3 portraitPosition = Vector3.zero;

		// Token: 0x040027C3 RID: 10179
		public bool eatClicks;

		// Token: 0x040027C4 RID: 10180
		public bool nextFtueOnClick = true;

		// Token: 0x040027C5 RID: 10181
		public bool dismissOnClick = true;

		// Token: 0x040027C6 RID: 10182
		public bool seenOnShow;

		// Token: 0x040027C7 RID: 10183
		public bool isMenu;

		// Token: 0x040027C8 RID: 10184
		[TextArea(1, 3)]
		public string leftArrowFocus;

		// Token: 0x040027C9 RID: 10185
		[TextArea(1, 3)]
		public string rightArrowFocus;

		// Token: 0x040027CA RID: 10186
		[TextArea(1, 3)]
		public string portraitLeftArrowFocus;

		// Token: 0x040027CB RID: 10187
		[TextArea(1, 3)]
		public string portraitRightArrowFocus;
	}

	// Token: 0x0200077D RID: 1917
	[Serializable]
	public class FTUE_Element
	{
		// Token: 0x06002709 RID: 9993 RVA: 0x000A58EA File Offset: 0x000A3AEA
		public void Show()
		{
			if (!FTUE_Manager.Instance)
			{
				return;
			}
			FTUE_Manager.Instance.ShowFTUE(this, null);
		}

		// Token: 0x040027CC RID: 10188
		public string ftueName = "FTUE Name";

		// Token: 0x040027CD RID: 10189
		public bool debug;

		// Token: 0x040027CE RID: 10190
		public List<FTUE_Manager.FTUE_Content> ftueContents = new List<FTUE_Manager.FTUE_Content>();
	}
}
