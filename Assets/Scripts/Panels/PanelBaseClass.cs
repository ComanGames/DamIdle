using System;
using UniRx;
using UnityEngine;

// Token: 0x0200022C RID: 556
public class PanelBaseClass : MonoBehaviour
{
	// Token: 0x06000FFB RID: 4091 RVA: 0x00049184 File Offset: 0x00047384
	public virtual void Init(ModalController modalController)
	{
		this.ParentModalController = modalController;
	}

	// Token: 0x06000FFC RID: 4092 RVA: 0x0004918D File Offset: 0x0004738D
	public virtual void CloseModal()
	{
		this.ParentModalController.CloseModal(Unit.Default);
	}

	// Token: 0x06000FFD RID: 4093 RVA: 0x00002718 File Offset: 0x00000918
	public virtual void OnShowPanel()
	{
	}

	// Token: 0x04000D8F RID: 3471
	protected ModalController ParentModalController;
}
