using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

// Token: 0x020000BD RID: 189
public class PopupAnimatorBase : ModalAnimatorBase
{
	// Token: 0x06000516 RID: 1302 RVA: 0x0001A7BA File Offset: 0x000189BA
	public override void Init(AnimatedModal modal)
	{
		this.rectTransform.localScale = new Vector3(0.2f, 0.2f, 1f);
		base.Init(modal);
	}

	// Token: 0x06000517 RID: 1303 RVA: 0x0001A7E2 File Offset: 0x000189E2
	public override IEnumerator Intro()
	{
		float num = 0.25f;
		this.modal.Content.DOFade(1f, num);
		this.rectTransform.DOScale(Vector3.one, num);
		yield return new WaitForSeconds(num);
		this.OnAnimFinished.OnNext("Intro");
		yield break;
	}

	// Token: 0x06000518 RID: 1304 RVA: 0x0001A7F1 File Offset: 0x000189F1
	public override IEnumerator Outro()
	{
		float num = 0.15f;
		this.modal.Content.DOFade(0f, num);
		this.rectTransform.DOScale(new Vector3(0.2f, 0.2f, 1f), num);
		yield return new WaitForSeconds(num);
		this.OnAnimFinished.OnNext("Outro");
		yield break;
	}
}
