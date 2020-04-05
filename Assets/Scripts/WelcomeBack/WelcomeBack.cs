using System;
using HHTools.Navigation;
using Platforms;
using Platforms.Ad;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000262 RID: 610
public class WelcomeBack : AnimatedModal
{
	// Token: 0x0600110B RID: 4363 RVA: 0x0004E634 File Offset: 0x0004C834
	protected override void Awake()
	{
		this.btn_DoubleOfflineEarnings.gameObject.SetActive(false);
		this.btn_Continue.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnContinueClicked)).AddTo(base.gameObject);
		this.btn_DoubleOfflineEarnings.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnDoubleProfitsClicked)).AddTo(base.gameObject);
		GameController.Instance.PlanetThemeService.BackgroundImage.Subscribe(delegate(Sprite bg)
		{
			this.img_BG.sprite = bg;
		}).AddTo(base.gameObject);
		this.navService = GameController.Instance.NavigationService;
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
		base.Awake();
	}

	// Token: 0x0600110C RID: 4364 RVA: 0x0004E70C File Offset: 0x0004C90C
	public void WireData(OfflineEarnings earnings)
	{
		this.currentOfflineCash = earnings.CashEarned;
		int timeOffline = Convert.ToInt32(earnings.Elapsed);
		this.txt_OfflineTime.text = string.Format("You were offline for {0}.", this.TimeOffLine(timeOffline));
		this.txt_MoneyAmount.text = string.Format("${0}", NumberFormat.Convert(earnings.CashEarned, 1000000.0, true, 3));
		this.txt_DoubleOfflineEarnings.gameObject.SetActive(false);
		this.btn_DoubleOfflineEarnings.gameObject.SetActive(false);
	}

	// Token: 0x0600110D RID: 4365 RVA: 0x0004E79C File Offset: 0x0004C99C
	private void OnContinueClicked(Unit u)
	{
		this.CloseModal(u);
		MessageBroker.Default.Publish<WelcomeBackSequenceCompleted>(default(WelcomeBackSequenceCompleted));
	}

	// Token: 0x0600110E RID: 4366 RVA: 0x0004E7C4 File Offset: 0x0004C9C4
	private void OnDoubleProfitsClicked(Unit u)
	{
		GameController.Instance.AnalyticService.SendAdStartEvent("Welcome Back", "Rewarded Video", GameController.Instance.game.planetName);
		GameController.Instance.AdWatchService.WatchAd(AdType.RewardedVideo, "WelcomeAd").Take(1).Subscribe(delegate(Unit _)
		{
			this.ShowWelcomeBackAdPanel();
		}, delegate(Exception e)
		{
			this.OnAdFailed(e.Message);
		}).AddTo(base.gameObject);
	}

	// Token: 0x0600110F RID: 4367 RVA: 0x0004E83D File Offset: 0x0004CA3D
	private string TimeOffLine(int timeOffline)
	{
		return string.Format("{0:0000}:{1:00}:{2:00}", timeOffline / 3600, timeOffline / 60 % 60, timeOffline % 60);
	}

	// Token: 0x06001110 RID: 4368 RVA: 0x0004E86A File Offset: 0x0004CA6A
	protected override void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		this.lg_Button.spacing = (float)(orientation.IsPortrait ? 30 : 115);
	}

	// Token: 0x06001111 RID: 4369 RVA: 0x0004E888 File Offset: 0x0004CA88
	private void ShowWelcomeBackAdPanel()
	{
		this.CloseModal(Unit.Default);
		GameController.Instance.AnalyticService.SendAdFinished("Welcome Back", "Rewarded Video", GameController.Instance.game.planetName, "Welcome Ad");
		WelcomeBackAdMultiplierView welcomeBackAdMultiplierView = this.navService.CreateModal<WelcomeBackAdMultiplierView>(NavModals.AD_WATCH_WELCOME_BACK, false);
		if (null != welcomeBackAdMultiplierView)
		{
			welcomeBackAdMultiplierView.WireData(this.currentOfflineCash);
		}
	}

	// Token: 0x06001112 RID: 4370 RVA: 0x0004E8F4 File Offset: 0x0004CAF4
	private void OnAdFailed(string error)
	{
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Oops, Something Went Wrong!", "Please try again in a bit, or contact us if the issue persists", null, "", PopupModal.PopupOptions.OK, "Ok", "", true);
		bool value = Helper.GetPlatformAd().AdReadyMap[AdType.Interstitial].Value;
		bool value2 = Helper.GetPlatformAd().AdReadyMap[AdType.RewardedVideo].Value;
		GameController.Instance.AnalyticService.SendAdFinished("Welcome Back", "Error=" + error, GameController.Instance.game.planetName, value2.ToString() + ":" + value.ToString());
	}

	// Token: 0x04000EB6 RID: 3766
	[SerializeField]
	private Text txt_OfflineTime;

	// Token: 0x04000EB7 RID: 3767
	[SerializeField]
	private Text txt_MoneyAmount;

	// Token: 0x04000EB8 RID: 3768
	[SerializeField]
	private Text txt_DoubleOfflineEarnings;

	// Token: 0x04000EB9 RID: 3769
	[SerializeField]
	private Button btn_DoubleOfflineEarnings;

	// Token: 0x04000EBA RID: 3770
	[SerializeField]
	private Button btn_Continue;

	// Token: 0x04000EBB RID: 3771
	[SerializeField]
	private Image img_BG;

	// Token: 0x04000EBC RID: 3772
	[SerializeField]
	private HorizontalLayoutGroup lg_Button;

	// Token: 0x04000EBD RID: 3773
	private double currentOfflineCash;

	// Token: 0x04000EBE RID: 3774
	private NavigationService navService;

	// Token: 0x04000EBF RID: 3775
	private const string WATCH_AD_COPY = "Watch an ad for <color=#9CC06BFF>Double Earnings</color> (<color=#A3C7DAFF>${0}</color>)";
}
