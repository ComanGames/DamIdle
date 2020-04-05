using System;
using System.Linq;
using AdCap.Store;
using HHTools.Navigation;
using Platforms;
using Platforms.Ad;
using Platforms.Logger;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200003C RID: 60
public class AdProfitBoostModal : AnimatedModal
{
	// Token: 0x0600014C RID: 332 RVA: 0x00008640 File Offset: 0x00006840
	public void WireData()
	{
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		this.gameController = GameController.Instance;
		this.greenTextHex = AdCapColours.ColourMap[ColourNames.TextColourGreen].HexColor;
		this.btn_Action.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnActionClicked();
		}).AddTo(base.gameObject);
		this.btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.CloseModal)).AddTo(base.gameObject);
		this.boostWithAdStoreItem = this.gameController.StoreService.CurrentCatalog.FirstOrDefault((AdCapStoreItem x) => x.Id == "ad_watch_boost_store");
		if (this.boostWithAdStoreItem != null)
		{
			this.txt_btn_Boost.text = string.Format("Boost for {0} Gold", this.boostWithAdStoreItem.Cost.Price);
			this.btn_Boost.OnClickAsObservable().Subscribe(delegate(Unit _)
			{
				this.OnBoostWithGoldClicked();
			}).AddTo(base.gameObject);
		}
		else
		{
			this.btn_Boost.gameObject.SetActive(false);
		}
		this.adWatchService = this.gameController.AdWatchService;
		this.profitBoostAdService = this.gameController.ProfitBoostAdService;
		this.profitBoostAdService.AvailableProfitAds.Subscribe(new Action<int>(this.OnAvailableAdsChanged)).AddTo(base.gameObject);
		this.profitBoostAdService.AdMultiplierBonus.Subscribe(delegate(float _)
		{
			this.OnAvailableAdsChanged(this.profitBoostAdService.AvailableProfitAds.Value);
		}).AddTo(base.gameObject);
		this.btn_Boost.gameObject.SetActive(!this.gameController.game.IsEventPlanet);
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
	}

	// Token: 0x0600014D RID: 333 RVA: 0x00008830 File Offset: 0x00006A30
	public override void CloseModal(Unit u)
	{
		this.disposables.Dispose();
		base.CloseModal(u);
	}

	// Token: 0x0600014E RID: 334 RVA: 0x00008844 File Offset: 0x00006A44
	private void OnAvailableAdsChanged(int numAds)
	{
		this.disposables.Clear();
		if (this.profitBoostAdService.AvailableProfitAds.Value == 6)
		{
			this.WireNoAdsWatched();
			return;
		}
		if (this.profitBoostAdService.AvailableProfitAds.Value > 0)
		{
			this.WireSomeAdsWatched();
			return;
		}
		this.WireAllAdsWatched();
	}

	// Token: 0x0600014F RID: 335 RVA: 0x00008896 File Offset: 0x00006A96
	private void OnActionClicked()
	{
		if (this.profitBoostAdService.AvailableProfitAds.Value <= 0)
		{
			this.CloseModal(Unit.Default);
			return;
		}
		this.WatchAd();
	}

	// Token: 0x06000150 RID: 336 RVA: 0x000088C0 File Offset: 0x00006AC0
	private void WireNoAdsWatched()
	{
		this.txt_Title.text = "Psst!";
		this.txt_subText.text = "Looking for a quick boost?";
		this.txt_Message.text = string.Format("Watch this fine quality ad and the \ninvestments on this planet will receive \na <color={0}>x{1} Profit Boost for 4 hours!</color>", this.greenTextHex, this.profitBoostAdService.AdMultiplierBonus.Value);
		this.txt_WatchButton.text = "Watch";
		this.txt_AdsRemaining.text = string.Format("You have <color={0}>{1} more</color> ad bonuses\nthat you can use today!", this.greenTextHex, this.profitBoostAdService.AvailableProfitAds.Value);
	}

	// Token: 0x06000151 RID: 337 RVA: 0x00008960 File Offset: 0x00006B60
	private void WireSomeAdsWatched()
	{
		this.profitBoostAdService.AdMultiplierBonus.Subscribe(delegate(float x)
		{
			this.txt_Title.text = string.Format("Enjoy your x{0} boost!", x);
		}).AddTo(this.disposables);
		this.profitBoostAdService.AdProfitBoostTimer.Subscribe(delegate(double x)
		{
			this.txt_subText.text = string.Format("{0} Remaining", Mathf.Round((float)(x*10))/10f);
		}).AddTo(this.disposables);
		this.txt_Message.text = string.Format("<color={0}>Watch another ad</color> to increase your \ntimer by {1} hours. It's frugal-licious.", this.greenTextHex, 4);
		this.txt_WatchButton.text = "Watch More";
		this.txt_AdsRemaining.text = string.Format("You have <color={0}>{1} more</color> ad bonuses\nthat you can use today!", this.greenTextHex, this.profitBoostAdService.AvailableProfitAds.Value);
	}

	// Token: 0x06000152 RID: 338 RVA: 0x00008A20 File Offset: 0x00006C20
	private void WireAllAdsWatched()
	{
		this.profitBoostAdService.AdMultiplierBonus.Subscribe(delegate(float x)
		{
			this.txt_Title.text = string.Format("Enjoy Your x{0} Boost!", x);
		}).AddTo(this.disposables);
		this.profitBoostAdService.AdProfitBoostTimer.Subscribe(delegate(double x)
		{
			this.txt_subText.text = string.Format("{0} Remaining",Mathf.Round((float) x*10)/10f);
		}).AddTo(this.disposables);
		this.txt_Message.text = string.Format("<color={0}>Watch ads tomorrow</color> to refresh your \ntimer by {1} hours. It's frugal-licious.", this.greenTextHex, 4);
		this.txt_WatchButton.text = "Continue";
		this.txt_AdsRemaining.text = string.Format("You have <color={0}>{1} more</color> ad bonuses\nthat you can use today!", this.greenTextHex, this.profitBoostAdService.AvailableProfitAds.Value);
	}

	// Token: 0x06000153 RID: 339 RVA: 0x00008AE0 File Offset: 0x00006CE0
	private void OnBoostWithGoldClicked()
	{
		this.gameController.AnalyticService.SendNavActionAnalytics(this.boostWithAdStoreItem.Id, this.gameController.game.planetName + "_AdProfitBoost", "buy");
		this.gameController.StoreService.AttemptPurchase(this.boostWithAdStoreItem, this.boostWithAdStoreItem.Cost.Currency);
	}

	// Token: 0x06000154 RID: 340 RVA: 0x00008B50 File Offset: 0x00006D50
	protected override void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		if (orientation.IsPortrait)
		{
			this.tform_TV.anchorMin = new Vector2(0.5f, 0.5f);
			this.tform_TV.anchorMax = new Vector2(0.5f, 0.5f);
			this.tform_TV.anchoredPosition = new Vector2(0f, 318f);
			this.tform_Contents.anchorMin = new Vector2(0.5f, 0.5f);
			this.tform_Contents.anchorMax = new Vector2(0.5f, 0.5f);
			this.tform_Contents.anchoredPosition = new Vector2(0f, -318f);
			return;
		}
		this.tform_TV.anchorMin = new Vector2(0.5f, 0.5f);
		this.tform_TV.anchorMax = new Vector2(0.5f, 0.5f);
		this.tform_TV.anchoredPosition = new Vector2(-318f, 0f);
		this.tform_Contents.anchorMin = new Vector2(0.5f, 0.5f);
		this.tform_Contents.anchorMax = new Vector2(0.5f, 0.5f);
		this.tform_Contents.anchoredPosition = new Vector2(318f, 0f);
	}

	// Token: 0x06000155 RID: 341 RVA: 0x00008CA4 File Offset: 0x00006EA4
	private void WatchAd()
	{
		this.gameController.AnalyticService.SendAdStartEvent("Root UI", "Rewarded Video", this.gameController.game.planetName);
		this.adWatchService.WatchAd(AdType.RewardedVideo, "2xMultiplier").Take(1).Subscribe(delegate(Unit _)
		{
			this.OnAdWatchedSuccess();
		}, delegate(Exception e)
		{
			this.OnAdWatchedFailed(e.Message);
		});
	}

	// Token: 0x06000156 RID: 342 RVA: 0x00008D10 File Offset: 0x00006F10
	private void OnAdWatchedSuccess()
	{
		this.gameController.AnalyticService.SendAdFinished("Root UI", "Rewarded Video", this.gameController.game.planetName, "2x Multiplier");
		this.profitBoostAdService.OnProfitBoostAdWatched();
	}

	// Token: 0x06000157 RID: 343 RVA: 0x00008D4C File Offset: 0x00006F4C
	private void OnAdWatchedFailed(string error)
	{
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Oops, Something Went Wrong!", "Please try again in a bit, or contact us if the issue persists", null, "", PopupModal.PopupOptions.OK, "Ok", "", true);
		bool value = Helper.GetPlatformAd().AdReadyMap[AdType.Interstitial].Value;
		bool value2 = Helper.GetPlatformAd().AdReadyMap[AdType.RewardedVideo].Value;
		this.gameController.AnalyticService.SendAdFinished("Root UI", "Error=" + error, this.gameController.game.planetName, value2.ToString() + ":" + value.ToString());
		this.logger.Error(error);
	}

	// Token: 0x04000176 RID: 374
	private const string AD_WATCH_BOOST_STORE_ITEM_ID = "ad_watch_boost_store";

	// Token: 0x04000177 RID: 375
	private const string AD_PLACEMENT = "2xMultiplier";

	// Token: 0x04000178 RID: 376
	[SerializeField]
	private Text txt_Title;

	// Token: 0x04000179 RID: 377
	[SerializeField]
	private Text txt_subText;

	// Token: 0x0400017A RID: 378
	[SerializeField]
	private Text txt_Message;

	// Token: 0x0400017B RID: 379
	[SerializeField]
	private Text txt_AdsRemaining;

	// Token: 0x0400017C RID: 380
	[SerializeField]
	private Text txt_WatchButton;

	// Token: 0x0400017D RID: 381
	[SerializeField]
	private Button btn_Action;

	// Token: 0x0400017E RID: 382
	[SerializeField]
	private Button btn_Boost;

	// Token: 0x0400017F RID: 383
	[SerializeField]
	private Text txt_btn_Boost;

	// Token: 0x04000180 RID: 384
	[SerializeField]
	private Button btn_Close;

	// Token: 0x04000181 RID: 385
	[SerializeField]
	private RectTransform tform_TV;

	// Token: 0x04000182 RID: 386
	[SerializeField]
	private RectTransform tform_Contents;

	// Token: 0x04000183 RID: 387
	private const string NO_ADS_WATCHED_MESSAGE = "Watch this fine quality ad and the \ninvestments on this planet will receive \na <color={0}>x{1} Profit Boost for 4 hours!</color>";

	// Token: 0x04000184 RID: 388
	private const string SOME_ADS_TITLE = "Enjoy your x{0} boost!";

	// Token: 0x04000185 RID: 389
	private const string WATCH_ANOTHER_AD = "<color={0}>Watch another ad</color> to increase your \ntimer by {1} hours. It's frugal-licious.";

	// Token: 0x04000186 RID: 390
	private const string ALL_ADS_WATCHED_TITLE = "Enjoy Your x{0} Boost!";

	// Token: 0x04000187 RID: 391
	private const string WATCH_ADS_TOMORROW = "<color={0}>Watch ads tomorrow</color> to refresh your \ntimer by {1} hours. It's frugal-licious.";

	// Token: 0x04000188 RID: 392
	private const string ADS_REMAINING_TEXT = "You have <color={0}>{1} more</color> ad bonuses\nthat you can use today!";

	// Token: 0x04000189 RID: 393
	private const string BOOST_WITH_GOLD_BUTTON_TEXT = "Boost for {0} Gold";

	// Token: 0x0400018A RID: 394
	private IGameController gameController;

	// Token: 0x0400018B RID: 395
	private AdWatchService adWatchService;

	// Token: 0x0400018C RID: 396
	private ProfitBoostAdService profitBoostAdService;

	// Token: 0x0400018D RID: 397
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x0400018E RID: 398
	private AdCapStoreItem boostWithAdStoreItem;

	// Token: 0x0400018F RID: 399
	private Platforms.Logger.Logger logger;

	// Token: 0x04000190 RID: 400
	private string greenTextHex;
}
