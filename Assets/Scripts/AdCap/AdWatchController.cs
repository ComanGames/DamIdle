using System;
using AdCap.Ads;
using HHTools.Navigation;
using UniRx;
using UnityEngine;

// Token: 0x0200003E RID: 62
public class AdWatchController : MonoBehaviour
{
	// Token: 0x06000166 RID: 358 RVA: 0x00002718 File Offset: 0x00000918
	private void Start()
	{
	}

	// Token: 0x06000167 RID: 359 RVA: 0x00008F74 File Offset: 0x00007174
	private void OnAdstateChanged(AdWatchService.EAdServiceState adState)
	{
		if (adState != AdWatchService.EAdServiceState.Idle)
		{
			if (adState != AdWatchService.EAdServiceState.Watching)
			{
				return;
			}
			this.loadingModal = this.navService.CreateModal<LoadingModal>(NavModals.LOADING, false);
			if (null != this.loadingModal)
			{
				this.loadingModal.WireData("Advertising is the life blood of Capitalism!");
			}
		}
		else if (null != this.loadingModal)
		{
			this.loadingModal.CloseModal(Unit.Default);
			return;
		}
	}

	// Token: 0x06000168 RID: 360 RVA: 0x00008FE0 File Offset: 0x000071E0
	private void OnAdhocRewardPressed(OnAdHocButtonPressEvent pressed)
	{
		AdHocRewardPanel adHocRewardPanel = this.navService.CreateModal<AdHocRewardPanel>(NavModals.AD_WATCH_AD_HOC_REWARD, false);
		if (null != adHocRewardPanel)
		{
			adHocRewardPanel.WireData();
		}
	}

	// Token: 0x04000198 RID: 408
	private NavigationService navService;

	// Token: 0x04000199 RID: 409
	private AdWatchService adWatchService;

	// Token: 0x0400019A RID: 410
	private LoadingModal loadingModal;
}
