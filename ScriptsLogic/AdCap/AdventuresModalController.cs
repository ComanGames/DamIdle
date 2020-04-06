using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200022A RID: 554
public class AdventuresModalController : ModalController
{
	// Token: 0x06000FED RID: 4077 RVA: 0x00048EF9 File Offset: 0x000470F9
	protected override void Awake()
	{
		this.Init();
		base.Awake();
	}

	// Token: 0x06000FEE RID: 4078 RVA: 0x00048F07 File Offset: 0x00047107
	public override void OnIntroFinished()
	{
		this.InitPanels();
		base.OnIntroFinished();
	}

	// Token: 0x06000FEF RID: 4079 RVA: 0x00048F18 File Offset: 0x00047118
	private void InitPanels()
	{
		this.eventPanel.Init(GameController.Instance.EventService);
		if (GameController.Instance.game.IsEventPlanet)
		{
			GameController.Instance.UnlockService.OnAllUnlocksDone.Subscribe(delegate(bool areAllEarned)
			{
				this.toggleMap["CurrencyExchange"].gameObject.SetActive(areAllEarned);
				if (areAllEarned)
				{
					FTUE_Manager.ShowFTUE("CXUnlocked", null);
				}
			}).AddTo(base.gameObject);
			this.toggleMap["MegaBoosts"].isOn = false;
		}
		this.toggleMap[this.currentPanel].isOn = true;
		(from i in GameController.Instance.IconService.EventPanelIcon
		where i != null
		select i).Subscribe(delegate(Sprite icon)
		{
			this.PanelButtonIcon.sprite = icon;
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000FF0 RID: 4080 RVA: 0x00048FF4 File Offset: 0x000471F4
	private void Init()
	{
		GameController.Instance.EventService.EventUnlocked.Subscribe(new Action<bool>(this.OnEventUnlocked)).AddTo(base.gameObject);
	}

	// Token: 0x06000FF1 RID: 4081 RVA: 0x00049022 File Offset: 0x00047222
	public void ShowMegaBoosts()
	{
		if (this.isInitialized)
		{
			this.toggleMap["MegaBoosts"].isOn = true;
			return;
		}
		this.currentPanel = "MegaBoosts";
	}

	// Token: 0x06000FF2 RID: 4082 RVA: 0x0004904E File Offset: 0x0004724E
	public void ShowEventPanel()
	{
		if (this.isInitialized)
		{
			this.toggleMap["Events"].isOn = true;
			return;
		}
		this.currentPanel = "Events";
	}

	// Token: 0x06000FF3 RID: 4083 RVA: 0x0004907A File Offset: 0x0004727A
	public void ShowMissionControl()
	{
		if (this.isInitialized)
		{
			this.toggleMap["MissionControl"].isOn = true;
			return;
		}
		this.currentPanel = "MissionControl";
	}

	// Token: 0x06000FF4 RID: 4084 RVA: 0x000490A6 File Offset: 0x000472A6
	public void ShowCurrencyExchangeControl()
	{
		if (this.isInitialized)
		{
			this.toggleMap["CurrencyExchange"].isOn = true;
			return;
		}
		this.currentPanel = "CurrencyExchange";
	}

	// Token: 0x06000FF5 RID: 4085 RVA: 0x000490D2 File Offset: 0x000472D2
	private void OnEventUnlocked(bool isAvailable)
	{
		this.toggleMap["Events"].gameObject.SetActive(isAvailable);
	}

	// Token: 0x04000D8B RID: 3467
	[SerializeField]
	private EventPanel eventPanel;

	// Token: 0x04000D8C RID: 3468
	public Image PanelButtonIcon;

	// Token: 0x04000D8D RID: 3469
	private bool isInitialized;

	// Token: 0x04000D8E RID: 3470
	private string currentPanel = "MissionControl";
}
