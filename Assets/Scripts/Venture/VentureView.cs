using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// Token: 0x02000163 RID: 355
public class VentureView : MonoBehaviour
{
	// Token: 0x06000B54 RID: 2900 RVA: 0x00033818 File Offset: 0x00031A18
	private void Start()
	{
		this.defaultRunTimerColor = this.RunTimer.color;
		this.GildingService = GameController.Instance.GildingService;
		this.GildingService.ShowingGild.Subscribe(new Action<bool>(this.TogglePurchaseButton)).AddTo(base.gameObject);
		GameController.Instance.State.Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(base.gameObject);
		this.Btn_GildButton.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnGildClicked)).AddTo(base.gameObject);
	}

	// Token: 0x06000B55 RID: 2901 RVA: 0x000338BD File Offset: 0x00031ABD
	private void OnDestroy()
	{
		this.stateDisposables.Dispose();
	}

	// Token: 0x06000B56 RID: 2902 RVA: 0x000338CC File Offset: 0x00031ACC
	private void OnStateChanged(GameState game)
	{
		this.stateDisposables.Clear();
		this.state = game;
		(from f in this.state.fluxCapitalorQuantity
		select f > 0).DistinctUntilChanged<bool>().Subscribe(delegate(bool f)
		{
			this.RunTimer.color = (f ? Color.cyan : this.defaultRunTimerColor);
		}).AddTo(this.stateDisposables);
		base.StartCoroutine(this.PlayAnimation());
		this.venture.UnlockState.Subscribe(new Action<VentureModel.EUnlockState>(this.OnVentureUnlockStateChanged)).AddTo(this.stateDisposables);
	}

	// Token: 0x06000B57 RID: 2903 RVA: 0x00033974 File Offset: 0x00031B74
	public void WireData(VentureModel venture)
	{
		this.venture = venture;
		(from l in this.venture.gildLevel
		where l > 0
		select l).Subscribe(delegate(int _)
		{
			this.BoostGildButton();
		}).AddTo(this.stateDisposables);
		this.Txt_UnlockTargetAmount.text = this.venture.UnlockTargetAmount;
		this.Img_UnlockTargetIcon.sprite = this.venture.UnlockTargetSprite;
		this.Txt_NameLocked.text = this.venture.Name;
		venture.IsRunning.Subscribe(delegate(bool x)
		{
            Debug.Log("Do some animation");

		}).AddTo(base.gameObject);
		this.venture.UnlocksToClaim.Subscribe(delegate(int x)
		{
			if (GameController.Instance.UnlockService.CanClaimUnlocksFromRoot)
			{
				this.IconContainer.gameObject.SetActive(x == 0);
				this.go_claimUnlockState.SetActive(x > 0);
				this.txt_claimUnlockCount.text = (x.ToString() ?? "");
				return;
			}
			this.IconContainer.gameObject.SetActive(true);
			this.go_claimUnlockState.SetActive(false);
		}).AddTo(base.gameObject);
		this.btn_claimUnlock.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.venture.ClaimAllUnclaimedUnlocks();
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000B58 RID: 2904 RVA: 0x00033A8C File Offset: 0x00031C8C
	private void OnVentureUnlockStateChanged(VentureModel.EUnlockState unlockState)
	{
		base.gameObject.SetActive(unlockState > VentureModel.EUnlockState.Hidden);
		this.PurchasedState.SetActive(unlockState == VentureModel.EUnlockState.Purchased);
		this.UnlockedState.SetActive(unlockState == VentureModel.EUnlockState.Unlocked);
		this.LockedState.SetActive(unlockState == VentureModel.EUnlockState.Locked);
		this.UpdateIconSize(OrientationController.Instance.CurrentOrientation.IsPortrait);
	}

	// Token: 0x06000B59 RID: 2905 RVA: 0x00033AEA File Offset: 0x00031CEA
	private void OnGildClicked(Unit u)
	{
		this.GildingService.PurchaseGilding(this.venture);
	}

	// Token: 0x06000B5A RID: 2906 RVA: 0x00033B00 File Offset: 0x00031D00
	public void Boost()
	{
		this.Img_FrameBG.color = this.GoldBoostColor;
		this.RunIconButton.colors = ColorBlock.defaultColorBlock;
		this.BuyButton.colors = ColorBlock.defaultColorBlock;
		this.BuyButton.image.color = this.GoldBoostColor;
		this.BuyNumber.color = this.TextColourBoosted;
		this.BuyNumber.GetComponent<Outline>().enabled = false;
		this.CostPer.color = this.TextColourBoosted;
		this.CostPer.GetComponent<Outline>().enabled = false;
		this.CostPerNumber.color = this.TextColourBoosted;
		this.HHSImg_ProgressBar.color = this.GoldBoostColor;
		this.Img_AchievementProgress.color = this.GoldBoostColor;
		this.RunTimer.transform.parent.GetComponent<Image>().color = this.InfoColorBoosted;
	}

	// Token: 0x06000B5B RID: 2907 RVA: 0x00033BEC File Offset: 0x00031DEC
	public void BoostPlatinum()
	{
		this.Img_FrameBG.color = this.PlatinumBoostColor;
		this.RunIconButton.colors = ColorBlock.defaultColorBlock;
		this.BuyButton.colors = ColorBlock.defaultColorBlock;
		this.BuyButton.image.color = this.PlatinumBoostColor;
		this.BuyNumber.color = this.TextColourPlatinumBoosted;
		this.BuyNumber.GetComponent<Outline>().enabled = false;
		this.CostPer.color = this.TextColourPlatinumBoosted;
		this.CostPer.GetComponent<Outline>().enabled = false;
		this.CostPerNumber.color = this.TextColourPlatinumBoosted;
		this.HHSImg_ProgressBar.color = this.PlatinumBoostColor;
		this.Img_AchievementProgress.color = this.PlatinumBoostColor;
		this.RunTimer.transform.parent.GetComponent<Image>().color = this.InfoColorPlatinumBoosted;
	}

	// Token: 0x06000B5C RID: 2908 RVA: 0x00033CD7 File Offset: 0x00031ED7
	public void BoostGildButton()
	{
		if (this.Btn_GildButton != null)
		{
			this.Btn_GildButton.image.sprite = this.GildButtonPlatinumBoosted;
		}
	}

	// Token: 0x06000B5D RID: 2909 RVA: 0x00033CFD File Offset: 0x00031EFD
	public void BoostProfitBooster()
	{
		this.HHSImg_ProgressBar.color = this.colorData.ProgressBarBoosted;
	}

	// Token: 0x06000B5E RID: 2910 RVA: 0x00033D15 File Offset: 0x00031F15
	private IEnumerator PlayAnimation()
	{
		if (!this.IconAnimator)
		{
			yield break;
		}
		for (;;)
		{
			yield return new WaitForSeconds((float)Random.Range(10, 20));
			this.IconAnimator.Play();
		}
	}

	// Token: 0x06000B5F RID: 2911 RVA: 0x00033D24 File Offset: 0x00031F24
	public void UpdateIconSize(bool isPortrait)
	{
		RectTransform rectTransform = (RectTransform)this.IconContainer.parent;
		this.IconContainer.sizeDelta = new Vector2(rectTransform.rect.height, rectTransform.rect.height);
		this.InfoContainer.offsetMin = new Vector2(rectTransform.rect.height + 10f, this.InfoContainer.anchorMin.y);
		this.BuyBoostCertificateButton.GetComponent<LayoutElement>().preferredWidth = rectTransform.rect.height;
		this.ventureName.fontSize = (isPortrait ? 40 : 30);
	}

	// Token: 0x06000B60 RID: 2912 RVA: 0x00033DD4 File Offset: 0x00031FD4
	public void SetViewColors(VentureViewColorData colorData)
	{
		this.colorData = colorData;
		ColorBlock colors = this.RunIconButton.colors;
		colors.normalColor = colorData.FrameBgActive;
		colors.disabledColor = colorData.FrameBgInactive;
		this.RunIconButton.colors = colors;
		colors = this.BuyButton.colors;
		colors.normalColor = colorData.BuyButton;
		colors.pressedColor = colorData.BuyButtonPressed;
		colors.disabledColor = colorData.BuyButtonDisabled;
		this.BuyButton.colors = colors;
		this.Img_Frame.color = colorData.Frame;
		this.Img_ProgressFrame.color = colorData.ProgressFrame;
		this.HHSImg_ProgressBar.color = colorData.ProgressBar;
		this.Img_ProgressBarBG.color = colorData.ProgressBG;
	}

	// Token: 0x06000B61 RID: 2913 RVA: 0x00033EA0 File Offset: 0x000320A0
	private void TogglePurchaseButton(bool isEnabled)
	{
		isEnabled &= (this.venture.gildLevel.Value < this.GildingService.GildLevelLimit);
		if (GameController.Instance.game.IsEventPlanet)
		{
			this.Btn_GildButton.gameObject.SetActive(isEnabled);
			this.BuyBoostBannerButton.transform.parent.gameObject.SetActive(false);
			return;
		}
		isEnabled &= !this.venture.IsBoosted.Value;
		if ((int)this.venture.TotalOwned.Value == 0)
		{
			isEnabled = false;
		}
		this.BuyBoostBannerButton.transform.parent.gameObject.SetActive(isEnabled);
		this.Btn_GildButton.gameObject.SetActive(false);
	}

	// Token: 0x0400099C RID: 2460
	public Text ventureName;

	// Token: 0x0400099D RID: 2461
	public RectTransform IconContainer;

	// Token: 0x0400099E RID: 2462
	public RectTransform InfoContainer;

	// Token: 0x040009A0 RID: 2464
	public Image Img_HighlightFrame;

	// Token: 0x040009A1 RID: 2465
	public Image Img_Frame;

	// Token: 0x040009A2 RID: 2466
	public Image Img_FrameBG;

	// Token: 0x040009A3 RID: 2467
	public Image Img_ProgressFrame;

	// Token: 0x040009A4 RID: 2468
	public Image HHSImg_ProgressBar;

	// Token: 0x040009A5 RID: 2469
	public Image Img_ProgressBarBG;

	// Token: 0x040009A6 RID: 2470
	public Image Img_AchievementProgress;

	// Token: 0x040009A7 RID: 2471
	public Text NumOwned;

	// Token: 0x040009A8 RID: 2472
	public Text BuyNumber;

	// Token: 0x040009A9 RID: 2473
	public Text CostPer;

	// Token: 0x040009AA RID: 2474
	public Text CostPerNumber;

	// Token: 0x040009AB RID: 2475
	public Text CashPerSec;

	// Token: 0x040009AC RID: 2476
	public Text UnpurchasedCostPer;

	// Token: 0x040009AD RID: 2477
	public Text Profit;

	// Token: 0x040009AE RID: 2478
	public Text ProfitNumber;

	// Token: 0x040009AF RID: 2479
	public Text RunTimer;

	// Token: 0x040009B0 RID: 2480
	public Button RunIconButton;

	// Token: 0x040009B1 RID: 2481
	public Button RunBarButton;

	// Token: 0x040009B2 RID: 2482
	public Button BuyButton;

	// Token: 0x040009B3 RID: 2483
	public Button BuyButtonUnpurchased;

	// Token: 0x040009B4 RID: 2484
	public GameObject LockedState;

	// Token: 0x040009B5 RID: 2485
	public GameObject PurchasedState;

	// Token: 0x040009B6 RID: 2486
	public GameObject UnlockedState;

	// Token: 0x040009B7 RID: 2487
	public GameObject BoostPurchaseState;

	// Token: 0x040009B8 RID: 2488
	public Image Img_IconLocked;

	// Token: 0x040009B9 RID: 2489
	public Image Img_IconUnlocked;

	// Token: 0x040009BA RID: 2490
	public Image Img_UnlockedVentureBG;

	// Token: 0x040009BB RID: 2491
	public Image Img_UnlockTargetIcon;

	// Token: 0x040009BC RID: 2492
	public Text Txt_UnlockTargetAmount;

	// Token: 0x040009BD RID: 2493
	public Text Txt_NameLocked;

	// Token: 0x040009BE RID: 2494
	public Button UnpurchasedStateButton;

	// Token: 0x040009BF RID: 2495
	public Button BuyBoostBannerButton;

	// Token: 0x040009C0 RID: 2496
	public Button BuyBoostCertificateButton;

	// Token: 0x040009C1 RID: 2497
	public SimpleSpriteAnimation IconAnimator;

	// Token: 0x040009C2 RID: 2498
	public Animator BonusEffectAnimator;

	// Token: 0x040009C3 RID: 2499
	private Color defaultRunTimerColor;

	// Token: 0x040009C4 RID: 2500
	public Image BoostedAchievementProgressBar;

	// Token: 0x040009C5 RID: 2501
	public Button Btn_GildButton;

	// Token: 0x040009C6 RID: 2502
	[Header("Mega Boost")]
	public Color GoldBoostColor;

	// Token: 0x040009C7 RID: 2503
	public Color TextColourBoosted;

	// Token: 0x040009C8 RID: 2504
	public Color InfoColorBoosted;

	// Token: 0x040009C9 RID: 2505
	[Header("Platinum Boost")]
	public Color PlatinumBoostColor;

	// Token: 0x040009CA RID: 2506
	public Color TextColourPlatinumBoosted;

	// Token: 0x040009CB RID: 2507
	public Color InfoColorPlatinumBoosted;

	// Token: 0x040009CC RID: 2508
	public Sprite GildButtonPlatinumBoosted;

	// Token: 0x040009CD RID: 2509
	[Header("Boosted SpriteState")]
	public SpriteState BoostedSpriteState;

	// Token: 0x040009CE RID: 2510
	[Header("Platinum Boosted SpriteState")]
	public SpriteState PlatinumBoostedSpriteState;

	// Token: 0x040009CF RID: 2511
	[Header("Unlock Collection")]
	[SerializeField]
	private GameObject go_claimUnlockState;

	// Token: 0x040009D0 RID: 2512
	[SerializeField]
	private Button btn_claimUnlock;

	// Token: 0x040009D1 RID: 2513
	[SerializeField]
	private Text txt_claimUnlockCount;

	// Token: 0x040009D2 RID: 2514
	private VentureModel venture;

	// Token: 0x040009D3 RID: 2515
	private GameState state;

	// Token: 0x040009D4 RID: 2516
	private GildingService GildingService;

	// Token: 0x040009D5 RID: 2517
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x040009D6 RID: 2518
	private VentureViewColorData colorData;
}
