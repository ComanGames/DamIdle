using System;
using System.Collections.Generic;
using System.Linq;
using Platforms;
using Platforms.Ad;
using Platforms.Logger;
using UniRx;
using UnityEngine;

// Token: 0x02000005 RID: 5
public class OfferwallService : IDisposable
{
	// Token: 0x06000010 RID: 16 RVA: 0x00002318 File Offset: 0x00000518
	public void Init(IGrantRewardService grantRewardService, ScheduledOfferService scheduledOfferService)
	{
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		this.logger.Debug("Initializing...");
		this.grantRewardService = grantRewardService;
		this.scheduledOfferService = scheduledOfferService;
		IPlatformAd platformAd = Helper.GetPlatformAd();
		if (platformAd == null || !platformAd.AdReadyMap.ContainsKey(AdType.OfferWall))
		{
			this.logger.Error("No Offerwal AdType setup in PlatformAd");
			return;
		}
		MessageBroker.Default.Receive<RegisterOfferWallButtonCommand>().Subscribe(new Action<RegisterOfferWallButtonCommand>(this.AddOfferwallButton)).AddTo(this.disposables);
		MessageBroker.Default.Receive<UnRegisterOfferWallButtonCommand>().Subscribe(new Action<UnRegisterOfferWallButtonCommand>(this.RemoveOfferwallButton)).AddTo(this.disposables);
		(from x in GameController.Instance.IsInitialized
		where x
		select x).Take(1).Subscribe(delegate(bool x)
		{
			platformAd.OfferWallCreditsReceived.Subscribe(new Action<int>(this.OfferwallAdCreditedEvent)).AddTo(this.disposables);
		}).AddTo(this.disposables);
		platformAd.AdReadyMap[AdType.OfferWall].Subscribe(new Action<bool>(this.OnOfferwallAvailableEvent)).AddTo(this.disposables);
		this.ShowOfferwallButtons(false);
		InventoryJsonDataObject data = AdCapExternalDataStorage.Data;
		this.currentOfferwallConfig.Value = data.OfferwallConfig.FirstOrDefault<OfferwallConfig>();
		bool.TryParse(PlayerPrefs.GetString("OfferwallServiceStatus", "false"), out this.IsServiceUnlocked);
		if (this.IsServiceUnlocked)
		{
			this.CheckAndShowButtons();
		}
		else
		{
			this.MonitorUnlockTriggers();
		}
		this.logger.Debug("Initialized");
	}

	// Token: 0x06000011 RID: 17 RVA: 0x000024C4 File Offset: 0x000006C4
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000012 RID: 18 RVA: 0x000024D4 File Offset: 0x000006D4
	public void AddOfferwallButton(RegisterOfferWallButtonCommand command)
	{
		GameObject button = command.Button;
		if (!this.OfferwallButtons.Contains(button))
		{
			this.OfferwallButtons.Add(button);
			this.scheduledOfferService.MonitorForActiveOfferOfType(ScheduledOfferType.OFFERWALL_SALE).Subscribe(delegate(ScheduledOfferModel model)
			{
				bool active = model != null;
				command.SaleBadge.transform.gameObject.SetActive(active);
				if (model != null)
				{
					command.SaleBadgingText.text = (model.Config as ScheduledOfferwallSaleConfig).BadgeText;
				}
			}).AddTo(command.SaleBadge);
			this.CheckAndShowButtons();
		}
	}

	// Token: 0x06000013 RID: 19 RVA: 0x00002548 File Offset: 0x00000748
	public void RemoveOfferwallButton(UnRegisterOfferWallButtonCommand cmd)
	{
		if (this.OfferwallButtons.Contains(cmd.Button))
		{
			this.OfferwallButtons.Remove(cmd.Button);
		}
	}

	// Token: 0x06000014 RID: 20 RVA: 0x00002570 File Offset: 0x00000770
	private void MonitorUnlockTriggers()
	{
		string text = "Steam";
		if (this.currentOfferwallConfig.Value != null && this.currentOfferwallConfig.Value.Platforms.ToLower().Contains(text.ToLower()))
		{
			GameController.Instance.TriggerService.MonitorTriggers(this.currentOfferwallConfig.Value.TriggerDatas, true).First(x => x).Subscribe(delegate(bool x)
			{
				if (x)
				{
					this.IsServiceUnlocked = true;
					PlayerPrefs.SetString("OfferwallServiceStatus", "true");
					this.CheckAndShowButtons();
				}
			}).AddTo(this.disposables);
		}
	}

	// Token: 0x06000015 RID: 21 RVA: 0x00002616 File Offset: 0x00000816
	private void OnOfferwallAvailableEvent(bool isOfferwallAvailable)
	{
		this.IsOfferwallAvailable = isOfferwallAvailable;
		this.CheckAndShowButtons();
	}

	// Token: 0x06000016 RID: 22 RVA: 0x00002625 File Offset: 0x00000825
	private void CheckAndShowButtons()
	{
		if (this.IsServiceUnlocked && this.IsOfferwallAvailable)
		{
			this.ShowOfferwallButtons(true);
			return;
		}
		this.ShowOfferwallButtons(false);
	}

	// Token: 0x06000017 RID: 23 RVA: 0x00002648 File Offset: 0x00000848
	private void OfferwallAdCreditedEvent(int numberOfCredits)
	{
		if (numberOfCredits > 0)
		{
			RewardData rewardData = new RewardData("gold", ERewardType.Gold, numberOfCredits);
			this.grantRewardService.GrantReward(rewardData, "offerwall", GameController.Instance.planetName, false);
		}
	}

	// Token: 0x06000018 RID: 24 RVA: 0x00002684 File Offset: 0x00000884
	private void ShowOfferwallButtons(bool isEnabled = false)
	{
		MessageBroker.Default.Publish<OfferwallButtonStatus>(new OfferwallButtonStatus(isEnabled));
		foreach (GameObject gameObject in this.OfferwallButtons)
		{
			gameObject.SetActive(isEnabled);
		}
	}

	// Token: 0x06000019 RID: 25 RVA: 0x000026E8 File Offset: 0x000008E8
	public void ShowOfferwall()
	{
		Helper.GetPlatformAd().ShowAd(AdType.OfferWall, "").Subscribe(new Action<ShowResult>(this.OnOfferwallSuccess), new Action<Exception>(this.OnOfferwallError));
	}

	// Token: 0x0600001A RID: 26 RVA: 0x00002718 File Offset: 0x00000918
	private void OnOfferwallSuccess(ShowResult result)
	{
	}

	// Token: 0x0600001B RID: 27 RVA: 0x00002718 File Offset: 0x00000918
	private void OnOfferwallError(Exception error)
	{
	}

	// Token: 0x0400000A RID: 10
	private const string OFFERWALL_SERVICE_UNLOCKED_KEY = "OfferwallServiceStatus";

	// Token: 0x0400000B RID: 11
	private bool IsServiceUnlocked;

	// Token: 0x0400000C RID: 12
	private bool IsOfferwallAvailable = true;

	// Token: 0x0400000D RID: 13
	public List<GameObject> OfferwallButtons = new List<GameObject>();

	// Token: 0x0400000E RID: 14
	public readonly ReactiveProperty<OfferwallConfig> currentOfferwallConfig = new ReactiveProperty<OfferwallConfig>();

	// Token: 0x0400000F RID: 15
	private IGrantRewardService grantRewardService;

	// Token: 0x04000010 RID: 16
	private ScheduledOfferService scheduledOfferService;

	// Token: 0x04000011 RID: 17
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000012 RID: 18
	private Platforms.Logger.Logger logger;
}
