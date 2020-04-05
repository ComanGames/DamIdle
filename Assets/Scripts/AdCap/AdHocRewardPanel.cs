using System;
using AdCap.Ads;
using HHTools.Navigation;
using Platforms;
using Platforms.Ad;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000187 RID: 391
public class AdHocRewardPanel : AnimatedModal
{
	// Token: 0x06000C5B RID: 3163 RVA: 0x0003788C File Offset: 0x00035A8C
	protected override void Awake()
	{
		this.gameController = GameController.Instance;
		this.btn_WatchAd.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnWatchAdClicked)).AddTo(base.gameObject);
		this.btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.SkipAdhoc)).AddTo(base.gameObject);
		this.adWatchService = this.gameController.AdWatchService;
		this.adHocRewardService = this.gameController.AdHocRewardService;
		base.Awake();
	}

	// Token: 0x06000C5C RID: 3164 RVA: 0x0003791C File Offset: 0x00035B1C
	public void WireData()
	{
		RewardData value = this.adHocRewardService.CurrentReward.Value;
		Item itemById = this.gameController.GlobalPlayerData.inventory.GetItemById(value.Id);
		if (itemById != null)
		{
			this.txt_RewardDescription.text = string.Format("{0} {1}", value.Qty, itemById.ItemName);
			this.img_Reward.sprite = Resources.Load<Sprite>(itemById.GetPathToIcon());
			return;
		}
		Debug.LogErrorFormat("Could not find reward [{0}] for ForAdHocrewardPanel", new object[]
		{
			value.Id
		});
	}

	// Token: 0x06000C5D RID: 3165 RVA: 0x000379B0 File Offset: 0x00035BB0
	private void OnWatchAdClicked(Unit u)
	{
		this.gameController.AnalyticService.SendAdStartEvent("AdHoc", "Rewarded Video", this.gameController.game.planetName);
		this.adWatchService.WatchAd(AdType.RewardedVideo, "AdHoc").Take(1).Subscribe(new Action<Unit>(this.OnAdWatchSuccess), delegate(Exception e)
		{
			this.OnAdFailed(e.Message);
		});
	}

	// Token: 0x06000C5E RID: 3166 RVA: 0x00037A1C File Offset: 0x00035C1C
	private void OnAdFailed(string error)
	{
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Oops, Something Went Wrong!", "Please try again in a bit, or contact us if the issue persists", null, "", PopupModal.PopupOptions.OK, "Ok", "", true);
		bool value = Helper.GetPlatformAd().AdReadyMap[AdType.Interstitial].Value;
		bool value2 = Helper.GetPlatformAd().AdReadyMap[AdType.RewardedVideo].Value;
		this.gameController.AnalyticService.SendAdFinished("AdHoc", "Error=" + error, this.gameController.game.planetName, value2.ToString() + ":" + value.ToString());
	}

	// Token: 0x06000C5F RID: 3167 RVA: 0x00037AD4 File Offset: 0x00035CD4
	private void OnAdWatchSuccess(Unit u)
	{
		RewardData value = this.adHocRewardService.CurrentReward.Value;
		string reward = string.Format("AdHoc {0}x{1}", value.Id, value.Qty);
		this.gameController.AnalyticService.SendAdFinished("AdHoc", "Rewarded Video", this.gameController.game.planetName, reward);
		if (this.gameController.GrantRewardService.GrantReward(value, "AdHoc", "AdHoc Reward", false) != null)
		{
			this.adHocRewardService.OnAdHocWatched();
			this.CloseModal(u);
		}
	}

	// Token: 0x06000C60 RID: 3168 RVA: 0x00037B6C File Offset: 0x00035D6C
	private void SkipAdhoc(Unit u)
	{
		MessageBroker.Default.Publish<OnAdHocSkippedEvent>(default(OnAdHocSkippedEvent));
		this.CloseModal(u);
	}

	// Token: 0x06000C61 RID: 3169 RVA: 0x00037B94 File Offset: 0x00035D94
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

	// Token: 0x04000A8A RID: 2698
	[SerializeField]
	private Text txt_RewardDescription;

	// Token: 0x04000A8B RID: 2699
	[SerializeField]
	private Button btn_WatchAd;

	// Token: 0x04000A8C RID: 2700
	[SerializeField]
	private Button btn_Close;

	// Token: 0x04000A8D RID: 2701
	[SerializeField]
	private Image img_Reward;

	// Token: 0x04000A8E RID: 2702
	[SerializeField]
	private RectTransform tform_TV;

	// Token: 0x04000A8F RID: 2703
	[SerializeField]
	private RectTransform tform_Contents;

	// Token: 0x04000A90 RID: 2704
	private DestroyOnHideModel destroyOnHideModel;

	// Token: 0x04000A91 RID: 2705
	private AdWatchService adWatchService;

	// Token: 0x04000A92 RID: 2706
	private AdHocRewardService adHocRewardService;

	// Token: 0x04000A93 RID: 2707
	private IGameController gameController;
}
