using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using Platforms.Logger;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Token: 0x020000FF RID: 255
public class TutorialController : MonoBehaviour
{
	// Token: 0x060006BC RID: 1724 RVA: 0x00023F18 File Offset: 0x00022118
	private void Awake()
	{
		this.ClearTutorial();
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		this.Content.gameObject.SetActive(false);
		this.btn_OnClick.gameObject.SetActive(false);
		this.btn_OnClick.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnClick)).AddTo(base.gameObject);
	}

	// Token: 0x060006BD RID: 1725 RVA: 0x00023F84 File Offset: 0x00022184
	public void Init()
	{
		this.tutorialService = GameController.Instance.TutorialService;
		this.navService = GameController.Instance.NavigationService;
		this.tutorialService.CurrentStep.Subscribe(new Action<TutorialStep>(this.OnTutorialStepChanged)).AddTo(base.gameObject);
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
		this.speechBubbles.ForEach(delegate(SpeechBubbleContent x)
		{
			x.SpeechBubble.WireData(this.currentCopy, this.interactableTimer);
		});
		this.Content.gameObject.SetActive(true);
	}

	// Token: 0x060006BE RID: 1726 RVA: 0x00024028 File Offset: 0x00022228
	private void OnClick(Unit u)
	{
		this.interactableTimer.Value = 0f;
		this.btn_OnClick.gameObject.SetActive(false);
		this.tutorialService.NextStep();
	}

	// Token: 0x060006BF RID: 1727 RVA: 0x00024056 File Offset: 0x00022256
	private void OnOrientationChanged(OrientationChangedEvent evt)
	{
		this.isPortrait = evt.IsPortrait;
		this.OnTutorialStepChanged(this.tutorialService.CurrentStep.Value);
	}

	// Token: 0x060006C0 RID: 1728 RVA: 0x0002407A File Offset: 0x0002227A
	private IEnumerator WaitForTarget(string target)
	{
		this.targetDisposable.Clear();
		bool hasTarget = true;
		if (!this.tutorialService.TargetMap.ContainsKey(target))
		{
			hasTarget = false;
			this.tutorialService.TargetMap.ObserveAdd().First((DictionaryAddEvent<string, List<GameObject>> x) => x.Key == target).Subscribe(delegate(DictionaryAddEvent<string, List<GameObject>> x)
			{
				hasTarget = true;
			}).AddTo(this.targetDisposable);
		}
		while (!hasTarget)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x060006C1 RID: 1729 RVA: 0x00024090 File Offset: 0x00022290
	private void OnTutorialStepChanged(TutorialStep step)
	{
		this.logger.Debug("<color=green>tutorial step {0}</color>", new object[]
		{
			(step == null) ? "Empty" : step.StepId
		});
		this.stepDisposables.Clear();
		this.currentStep = step;
		this.ClearStep();
		if (this.currentStep != null)
		{
			Observable.FromCoroutine(new Func<IEnumerator>(this.WireStep), false).Subscribe<Unit>().AddTo(this.stepDisposables);
			return;
		}
		this.ClearTutorial();
	}

	// Token: 0x060006C2 RID: 1730 RVA: 0x00024110 File Offset: 0x00022310
	private void ClearStep()
	{
		this.stepDisposables.Clear();
		this.interactableTimer.Value = 0f;
		this.highlightObjects.Clear();
		this.highlightBackgrounds.ForEach(delegate(TutorialHighlight bg)
		{
			if (bg != null)
			{
				Object.DestroyImmediate(bg.gameObject);
			}
		});
		this.highlightBackgrounds.Clear();
		if (this.go_Arrow != null)
		{
			Object.DestroyImmediate(this.go_Arrow);
		}
		this.characterContainer.SetActive(false);
		if (null != this.interactableObject)
		{
			ScrollRect[] componentsInParent = this.interactableObject.GetComponentsInParent<ScrollRect>();
			for (int i = 0; i < componentsInParent.Length; i++)
			{
				componentsInParent[i].enabled = true;
			}
			this.interactableObject = null;
		}
		if (null != this.interactableRaycaster)
		{
			Object.DestroyImmediate(this.interactableRaycaster);
		}
		if (null != this.interactableCanvas)
		{
			Object.DestroyImmediate(this.interactableCanvas);
		}
		this.highlightCanvases.ForEach(delegate(Canvas canvas)
		{
			if (null != canvas)
			{
				Object.DestroyImmediate(canvas);
			}
		});
		this.highlightCanvases.Clear();
	}

	// Token: 0x060006C3 RID: 1731 RVA: 0x0002423C File Offset: 0x0002243C
	private void ClearTutorial()
	{
		this.ClearStep();
		this.canvasGroup.alpha = 0f;
		this.canvasGroup.blocksRaycasts = false;
		this.Content.SetParent(base.transform, false);
	}

	// Token: 0x060006C4 RID: 1732 RVA: 0x00024272 File Offset: 0x00022472
	private IEnumerator WireStep()
	{
		this.canvasGroup.alpha = 1f;
		this.canvasGroup.blocksRaycasts = true;
		yield return this.SetCharacterPositionAndCopy();
		if (this.currentStep.IsMenu && this.isPortrait)
		{
			yield return this.OpenPortraitMenu();
		}
		yield return this.SetLocation(this.currentStep.LocationTrigger.Id);
		yield return this.SetHighlightTargets(this.currentStep.HighlightTarget, this.currentStep.HasEffect);
		yield return this.SetInteractableTarget(this.currentStep.InteractableTarget);
		yield return this.SetArrow(this.interactableObject, this.isPortrait ? this.currentStep.ArrowPositionPortrait : this.currentStep.ArrowPositionLandscape);
		while (this.interactableTimer.Value < TutorialController.CLICK_DELAY)
		{
			this.interactableTimer.Value += Time.deltaTime;
			yield return null;
		}
		this.btn_OnClick.gameObject.SetActive(this.currentStep.EndTriggers.Count == 0);
		yield break;
	}

	// Token: 0x060006C5 RID: 1733 RVA: 0x00024281 File Offset: 0x00022481
	private IEnumerator OpenPortraitMenu()
	{
		yield return null;
		yield return this.SetLocation("Root");
		yield return this.SetHighlightTargets("OpenMenu", false);
		yield return this.SetInteractableTarget("OpenMenu");
		yield return this.SetArrow(this.interactableObject, ETutorialArrowPosition.PointingDown);
		bool openedMenu = false;
		this.navService.CurrentLocation.First((string x) => x == "Menu").Subscribe(delegate(string _)
		{
			openedMenu = true;
		}).AddTo(this.stepDisposables);
		while (!openedMenu)
		{
			yield return null;
		}
		Object.DestroyImmediate(this.interactableObject.GetComponent<GraphicRaycaster>());
		Object.DestroyImmediate(this.interactableObject.GetComponent<Canvas>());
		this.interactableObject = null;
		yield break;
	}

	// Token: 0x060006C6 RID: 1734 RVA: 0x00024290 File Offset: 0x00022490
	private IEnumerator SetLocation(string location)
	{
		yield return this.WaitForTarget(location);
		List<GameObject> list;
		if (!this.tutorialService.TargetMap.TryGetValue(location, out list))
		{
			this.logger.Error("Could not find tutorial location [{0}]", new object[]
			{
				location
			});
			yield break;
		}
		this.Content.SetParent(list[0].transform, false);
		this.Content.transform.localPosition = new Vector3(this.Content.transform.localPosition.x, this.Content.transform.localPosition.y, 0f);
		this.Content.transform.localScale = Vector3.one;
		yield break;
	}

	// Token: 0x060006C7 RID: 1735 RVA: 0x000242A6 File Offset: 0x000224A6
	private IEnumerator SetCharacterPositionAndCopy()
	{
		if (!string.IsNullOrEmpty(this.currentStep.CharacterName))
		{
			string target = this.isPortrait ? this.currentStep.CharacterPositionPortrait : this.currentStep.CharacterPositionLandscape;
			yield return this.WaitForTarget(target);
			List<GameObject> list;
			if (!this.tutorialService.TargetMap.TryGetValue(target, out list))
			{
				this.logger.Error("Could not find character position [{0}]", new object[]
				{
					target
				});
				yield break;
			}
			Sprite sprite;
			if (this.characterMap.TryGetValue(this.currentStep.CharacterName, out sprite))
			{
				this.img_Character.sprite = sprite;
				this.characterContainer.transform.SetParent(list[0].transform, false);
				this.characterContainer.transform.localPosition = Vector3.zero;
				this.characterContainer.transform.localScale = Vector3.one;
				this.characterContainer.SetActive(true);
			}
			else
			{
				this.logger.Error("Could not find character sprite with key [{0}]", new object[]
				{
					this.currentStep.CharacterName
				});
			}
			this.SetSpeechText();
			target = null;
		}
		else
		{
			this.characterContainer.SetActive(false);
		}
		yield break;
	}

	// Token: 0x060006C8 RID: 1736 RVA: 0x000242B5 File Offset: 0x000224B5
	private IEnumerator SetArrow(GameObject target, ETutorialArrowPosition arrowPos)
	{
		if (null == target)
		{
			yield break;
		}
		Vector3 rot = Vector3.zero;
		Vector2 anchorMinMax = Vector2.zero;
		switch (arrowPos)
		{
		case ETutorialArrowPosition.None:
			yield break;
		case ETutorialArrowPosition.PointingDown:
			anchorMinMax = new Vector2(0.5f, 1f);
			rot.z = 270f;
			break;
		case ETutorialArrowPosition.PointingUp:
			anchorMinMax = new Vector2(0.5f, 0f);
			rot.z = 90f;
			break;
		case ETutorialArrowPosition.PointingRight:
			anchorMinMax = new Vector2(0f, 0.5f);
			break;
		case ETutorialArrowPosition.PointingLeft:
			anchorMinMax = new Vector2(1f, 0.5f);
			rot.z = 180f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		RectTransform parent = (RectTransform)target.transform;
		if (null == this.go_Arrow)
		{
			this.go_Arrow = Object.Instantiate<GameObject>(this.ArrowPrefab);
		}
		RectTransform arrowRect = this.go_Arrow.GetComponent<RectTransform>();
		this.go_Arrow.transform.SetParent(parent, false);
		this.go_Arrow.transform.localScale = Vector3.one;
		Canvas.ForceUpdateCanvases();
		yield return null;
		arrowRect.anchorMin = anchorMinMax;
		arrowRect.anchorMax = anchorMinMax;
		this.go_Arrow.transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
		this.go_Arrow.SetActive(true);
		yield break;
	}

	// Token: 0x060006C9 RID: 1737 RVA: 0x000242D4 File Offset: 0x000224D4
	private void SetSpeechText()
	{
		ESpeechBubblePosition espeechBubblePosition = this.isPortrait ? this.currentStep.SpeechBubblePositionPortrait : this.currentStep.SpeechBubblePositionLandscape;
		int count = this.speechBubbles.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.speechBubbles[i].SpeechBubblePosition == espeechBubblePosition)
			{
				this.speechBubbles[i].Container.SetActive(true);
				this.speechBubbles[i].Go_Ok.SetActive(this.currentStep.HasOkButton);
			}
			else
			{
				this.speechBubbles[i].Container.SetActive(false);
			}
		}
		if (this.currentStep.Copy.Contains("{0}"))
		{
			this.SetAngelResetText();
			return;
		}
		this.currentCopy.Value = this.currentStep.Copy;
	}

	// Token: 0x060006CA RID: 1738 RVA: 0x000243B4 File Offset: 0x000225B4
	private void SetAngelResetText()
	{
		IAngelInvestorService angelService = GameController.Instance.AngelService;
		double num = angelService.CalculateAngelInvestors(GameController.Instance.game.TotalPreviousCash.Value);
		string arg = NumberFormat.Convert(angelService.GetRewardAngelCount() / num * 100.0, 1E+15, false, 0);
		this.currentCopy.Value = string.Format(this.currentStep.Copy, arg);
	}

	// Token: 0x060006CB RID: 1739 RVA: 0x00024424 File Offset: 0x00022624
	private IEnumerator SetInteractableTarget(string target)
	{
		if (!string.IsNullOrEmpty(target))
		{
			yield return this.WaitForTarget(target);
			List<GameObject> list;
			if (!this.tutorialService.TargetMap.TryGetValue(target, out list))
			{
				this.logger.Error("Could not find highlight target [{0}]", new object[]
				{
					target
				});
				yield break;
			}
			this.interactableObject = list[0];
			ScrollRect[] componentsInParent = this.interactableObject.GetComponentsInParent<ScrollRect>();
			for (int i = 0; i < componentsInParent.Length; i++)
			{
				componentsInParent[i].enabled = false;
			}
			Canvas canvas = this.interactableObject.GetComponent<Canvas>();
			if (canvas == null)
			{
				canvas = this.interactableObject.AddComponent<Canvas>();
				this.interactableCanvas = canvas;
			}
			GraphicRaycaster x = this.interactableObject.GetComponent<GraphicRaycaster>();
			if (x == null)
			{
				x = this.interactableObject.AddComponent<GraphicRaycaster>();
				this.interactableRaycaster = x;
			}
			canvas.overrideSorting = true;
			canvas.sortingOrder = 100;
		}
		yield break;
	}

	// Token: 0x060006CC RID: 1740 RVA: 0x0002443A File Offset: 0x0002263A
	private IEnumerator SetHighlightTargets(string target, bool hasEffect)
	{
		if (!string.IsNullOrEmpty(target))
		{
			yield return this.WaitForTarget(target);
			List<GameObject> list;
			if (!this.tutorialService.TargetMap.TryGetValue(target, out list))
			{
				this.logger.Error("Could not find highlight target [{0}]", new object[]
				{
					target
				});
				yield break;
			}
			list.ForEach(delegate(GameObject highlightTarget)
			{
				this.highlightObjects.Add(highlightTarget);
				Canvas canvas = highlightTarget.GetComponent<Canvas>();
				if (canvas == null)
				{
					canvas = highlightTarget.AddComponent<Canvas>();
					this.highlightCanvases.Add(canvas);
				}
				canvas.overrideSorting = true;
				canvas.sortingOrder = 100;
				TutorialHighlight tutorialHighlight = Object.Instantiate<TutorialHighlight>(this.HighlightBGPrefab);
				((RectTransform)tutorialHighlight.transform).SetAndStretchToParentSizeWithMod((RectTransform)highlightTarget.transform, -10f);
				tutorialHighlight.transform.localScale = Vector3.one;
				tutorialHighlight.GetComponent<Canvas>().sortingOrder = canvas.sortingOrder - 10;
				tutorialHighlight.gameObject.SetActive(true);
				tutorialHighlight.HighlightEffect.SetActive(hasEffect);
				this.highlightBackgrounds.Add(tutorialHighlight);
			});
		}
		yield break;
	}

	// Token: 0x04000636 RID: 1590
	public static float CLICK_DELAY = 0.5f;

	// Token: 0x04000637 RID: 1591
	[SerializeField]
	private Transform Content;

	// Token: 0x04000638 RID: 1592
	[SerializeField]
	private CanvasGroup canvasGroup;

	// Token: 0x04000639 RID: 1593
	[SerializeField]
	private StringSpriteDictionary characterMap = new StringSpriteDictionary();

	// Token: 0x0400063A RID: 1594
	[SerializeField]
	private List<SpeechBubbleContent> speechBubbles = new List<SpeechBubbleContent>();

	// Token: 0x0400063B RID: 1595
	[SerializeField]
	private GameObject characterContainer;

	// Token: 0x0400063C RID: 1596
	[SerializeField]
	private Image img_Character;

	// Token: 0x0400063D RID: 1597
	[SerializeField]
	private TutorialHighlight HighlightBGPrefab;

	// Token: 0x0400063E RID: 1598
	[SerializeField]
	private GameObject ArrowPrefab;

	// Token: 0x0400063F RID: 1599
	[SerializeField]
	private Button btn_OnClick;

	// Token: 0x04000640 RID: 1600
	private NavigationService navService;

	// Token: 0x04000641 RID: 1601
	private TutorialService tutorialService;

	// Token: 0x04000642 RID: 1602
	private bool isPortrait = true;

	// Token: 0x04000643 RID: 1603
	private GameObject go_Arrow;

	// Token: 0x04000644 RID: 1604
	private GameObject interactableObject;

	// Token: 0x04000645 RID: 1605
	private Canvas interactableCanvas;

	// Token: 0x04000646 RID: 1606
	private GraphicRaycaster interactableRaycaster;

	// Token: 0x04000647 RID: 1607
	private List<TutorialHighlight> highlightBackgrounds = new List<TutorialHighlight>();

	// Token: 0x04000648 RID: 1608
	private List<GameObject> highlightObjects = new List<GameObject>();

	// Token: 0x04000649 RID: 1609
	private List<Canvas> highlightCanvases = new List<Canvas>();

	// Token: 0x0400064A RID: 1610
	private CompositeDisposable targetDisposable = new CompositeDisposable();

	// Token: 0x0400064B RID: 1611
	private CompositeDisposable stepDisposables = new CompositeDisposable();

	// Token: 0x0400064C RID: 1612
	private Platforms.Logger.Logger logger;

	// Token: 0x0400064D RID: 1613
	private TutorialStep currentStep;

	// Token: 0x0400064E RID: 1614
	private ReactiveProperty<string> currentCopy = new ReactiveProperty<string>();

	// Token: 0x0400064F RID: 1615
	private ReactiveProperty<float> interactableTimer = new ReactiveProperty<float>();

	// Token: 0x04000650 RID: 1616
	private int debugStepCount;
}
