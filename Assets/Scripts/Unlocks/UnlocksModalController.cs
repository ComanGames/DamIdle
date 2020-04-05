using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000185 RID: 389
public class UnlocksModalController : ModalController
{
	// Token: 0x06000C51 RID: 3153 RVA: 0x000376FD File Offset: 0x000358FD
	protected override void Awake()
	{
		GameController.Instance.UnlockService.OnUnlockAchieved.Subscribe(delegate(Unlock _)
		{
			this.OnUnlockAchieved();
		}).AddTo(base.gameObject);
		base.Awake();
	}

	// Token: 0x06000C52 RID: 3154 RVA: 0x00037731 File Offset: 0x00035931
	public override void OnIntroFinished()
	{
		this.InitPanels();
		base.OnIntroFinished();
	}

	// Token: 0x06000C53 RID: 3155 RVA: 0x00037740 File Offset: 0x00035940
	private void InitPanels()
	{
		this.unlocks = GameController.Instance.UnlockService.Unlocks;
		this.OnUnlockAchieved();
		MainUIController uiController = Object.FindObjectOfType<MainUIController>();
		this.unlockGalleryPanel.Init(GameController.Instance.game, uiController);
		this.unlocksPanel.Init(GameController.Instance.game, uiController);
	}

	// Token: 0x06000C54 RID: 3156 RVA: 0x0003779C File Offset: 0x0003599C
	private void OnUnlockAchieved()
	{
		int count = this.unlocks.Count;
		int count2 = this.unlocks.FindAll((Unlock a) => a.Earned.Value).Count;
		this.txt_UnlockCount.text = string.Format("{0}/{1}", count2, count);
	}

	// Token: 0x06000C55 RID: 3157 RVA: 0x00037808 File Offset: 0x00035A08
	protected override void ToggleCanvasGroup(bool isOn, CanvasGroup cGroup)
	{
		if (cGroup.gameObject.name == "UnlockGalleryPanel" && GameController.Instance.game.planetName.ToLower() == "moon")
		{
			MessageBroker.Default.Publish<AchievementUnlockedEvent>(new AchievementUnlockedEvent
			{
				Id = "Gallery Tour!"
			});
		}
		base.ToggleCanvasGroup(isOn, cGroup);
	}

	// Token: 0x04000A85 RID: 2693
	[SerializeField]
	private Text txt_UnlockCount;

	// Token: 0x04000A86 RID: 2694
	[SerializeField]
	private UnlockGalleryPanel unlockGalleryPanel;

	// Token: 0x04000A87 RID: 2695
	[SerializeField]
	private UnlocksPanel unlocksPanel;

	// Token: 0x04000A88 RID: 2696
	private List<Unlock> unlocks;
}
