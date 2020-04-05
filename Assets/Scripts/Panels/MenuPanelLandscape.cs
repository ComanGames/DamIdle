using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000B4 RID: 180
public class MenuPanelLandscape : MenuPanelBase
{
	// Token: 0x060004D8 RID: 1240 RVA: 0x00019835 File Offset: 0x00017A35
	private void Start()
	{
		this.btn_Swag.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowCareer)).AddTo(base.gameObject);
	}

	// Token: 0x060004D9 RID: 1241 RVA: 0x00019860 File Offset: 0x00017A60
	public void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		base.gameObject.SetActive(!orientation.IsPortrait);
	}

	// Token: 0x060004DA RID: 1242 RVA: 0x00019876 File Offset: 0x00017A76
	protected override void OnClickShowOfferwall(Unit u)
	{
		GameController.Instance.AnalyticService.SendAdStartEvent("Offerwall", "Offerwall", "Offerwall_Root_Landscape");
		GameController.Instance.OfferwallService.ShowOfferwall();
	}

	// Token: 0x04000453 RID: 1107
	[SerializeField]
	protected Button btn_Swag;
}
