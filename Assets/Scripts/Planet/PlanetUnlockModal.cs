using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000CE RID: 206
public class PlanetUnlockModal : AnimatedModal
{
	// Token: 0x0600058B RID: 1419 RVA: 0x0001CF00 File Offset: 0x0001B100
	public void WireData(PlanetData data)
	{
		this.btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.CloseModal)).AddTo(base.gameObject);
		this.missionPanel.WireData(data);
		this.txt_UnlockMessage.text = data.UnlockMessage;
		this.txt_Title.text = string.Format("{0} Is Ready For Launch", data.DisplayName);
	}

	// Token: 0x040004F9 RID: 1273
	[SerializeField]
	private PlanetMissionPanel missionPanel;

	// Token: 0x040004FA RID: 1274
	[SerializeField]
	private Button btn_Close;

	// Token: 0x040004FB RID: 1275
	[SerializeField]
	private Text txt_UnlockMessage;

	// Token: 0x040004FC RID: 1276
	[SerializeField]
	private Text txt_Title;

	// Token: 0x040004FD RID: 1277
	private const string TITLE = "{0} Is Ready For Launch";
}
