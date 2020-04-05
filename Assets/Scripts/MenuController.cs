using System;
using UniRx;
using UnityEngine;

// Token: 0x020000B2 RID: 178
public class MenuController : MonoBehaviour
{
	// Token: 0x060004C9 RID: 1225 RVA: 0x0001942D File Offset: 0x0001762D
	private void Awake()
	{
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
	}

	// Token: 0x060004CA RID: 1226 RVA: 0x00019456 File Offset: 0x00017656
	private void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		this.portriatMenu.OnOrientationChanged(orientation);
		this.landscapeMenu.OnOrientationChanged(orientation);
	}

	// Token: 0x04000447 RID: 1095
	[SerializeField]
	private MenuPanelPortrait portriatMenu;

	// Token: 0x04000448 RID: 1096
	[SerializeField]
	private MenuPanelLandscape landscapeMenu;
}
