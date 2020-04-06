using System;
using UniRx;
using UnityEngine;

// Token: 0x020000B6 RID: 182
public class AnimatedModal : ModalBase
{
	// Token: 0x060004EB RID: 1259 RVA: 0x00019B44 File Offset: 0x00017D44
	protected override void Awake()
	{
		this.Content = base.GetComponentInChildren<CanvasGroup>();
		this.Content.alpha = 0f;
		this.AnimatorBase = base.GetComponent<ModalAnimatorBase>();
		this.AnimatorBase.OnAnimFinished.Subscribe(new Action<string>(this.OnAnimFinished)).AddTo(base.gameObject);
		this.AnimatorBase.Init(this);
		base.Awake();
	}

	// Token: 0x060004EC RID: 1260 RVA: 0x00019BB3 File Offset: 0x00017DB3
	public virtual void OnIntroFinished()
	{
		this.AnimatorBase.PlayFade();
	}

	// Token: 0x060004ED RID: 1261 RVA: 0x00019BC0 File Offset: 0x00017DC0
	public override void CloseModal(Unit _)
	{
		this.NavService.UnregisterModal(this);
		this.AnimatorBase.PlayOutro();
	}

	// Token: 0x060004EE RID: 1262 RVA: 0x00019BD9 File Offset: 0x00017DD9
	private void OnAnimFinished(string anim)
	{
		if (anim == "Outro")
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (!(anim == "Intro"))
		{
			return;
		}
		this.OnIntroFinished();
	}

	// Token: 0x0400045C RID: 1116
	[HideInInspector]
	public ModalAnimatorBase AnimatorBase;

	// Token: 0x0400045D RID: 1117
	public CanvasGroup Content;
}
