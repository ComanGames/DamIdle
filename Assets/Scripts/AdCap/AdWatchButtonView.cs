using System;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200003D RID: 61
public class AdWatchButtonView : MonoBehaviour
{
	// Token: 0x06000162 RID: 354 RVA: 0x00008EB7 File Offset: 0x000070B7
	public void Awake()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x06000163 RID: 355 RVA: 0x00008EC8 File Offset: 0x000070C8
	private void OnAdTimerChanged(double time)
	{
		if (time > 0.0)
		{
			this.txt_TimeRemaining.text = time.ToCountdownTrim();
			this.img_Highlight.enabled = false;
			this.img_adWatch.color = this.activeColor;
			return;
		}
		this.img_Highlight.enabled = true;
		this.txt_TimeRemaining.text = "00:00:00";
		this.img_adWatch.color = this.inactiveColor;
	}

	// Token: 0x06000164 RID: 356 RVA: 0x00008F40 File Offset: 0x00007140
	private void ShowAdWatchPanel(Unit u)
	{
		AdProfitBoostModal adProfitBoostModal = GameController.Instance.NavigationService.CreateModal<AdProfitBoostModal>(NavModals.AD_WATCH_PROFIT_BOOST, false);
		if (null != adProfitBoostModal)
		{
			adProfitBoostModal.WireData();
		}
	}

	// Token: 0x04000191 RID: 401
	[SerializeField]
	private Button btn_AdWatch;

	// Token: 0x04000192 RID: 402
	[SerializeField]
	private Text txt_TimeRemaining;

	// Token: 0x04000193 RID: 403
	[SerializeField]
	private Image img_Highlight;

	// Token: 0x04000194 RID: 404
	[SerializeField]
	private Image img_adWatch;

	// Token: 0x04000195 RID: 405
	[SerializeField]
	private Color activeColor;

	// Token: 0x04000196 RID: 406
	[SerializeField]
	private Color inactiveColor;

	// Token: 0x04000197 RID: 407
	private ProfitBoostAdService profitBoostAdService;
}
