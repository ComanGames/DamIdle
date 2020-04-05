using System;
using System.Collections;
using DG.Tweening;
using UniRx;
using UnityEngine;

// Token: 0x020000B8 RID: 184
public class ModalAnimatorBase : MonoBehaviour
{
	// Token: 0x060004F0 RID: 1264 RVA: 0x00019C10 File Offset: 0x00017E10
	public virtual void Init(AnimatedModal modal)
	{
		this.modal = modal;
		this.modal.Content.blocksRaycasts = false;
		(from x in this.modal.OnModalRegisteredCb
		where x
		select x).Subscribe(new Action<bool>(this.OnModalRegistered));
	}

	// Token: 0x060004F1 RID: 1265 RVA: 0x00019C77 File Offset: 0x00017E77
	protected virtual void OnDestroy()
	{
		this.currentAnimation.Dispose();
	}

	// Token: 0x060004F2 RID: 1266 RVA: 0x00019C84 File Offset: 0x00017E84
	public virtual void PlayIntro()
	{
		this.currentAnimation = Observable.FromCoroutine(new Func<IEnumerator>(this.Intro), false).Subscribe<Unit>();
	}

	// Token: 0x060004F3 RID: 1267 RVA: 0x00019CA4 File Offset: 0x00017EA4
	public virtual void PlayFade()
	{
		this.currentAnimation = Observable.FromCoroutine(new Func<IEnumerator>(this.FadeInContent), false).Subscribe<Unit>();
	}

	// Token: 0x060004F4 RID: 1268 RVA: 0x00019CC3 File Offset: 0x00017EC3
	public virtual void PlayOutro()
	{
		this.currentAnimation = Observable.FromCoroutine(new Func<IEnumerator>(this.Outro), false).Subscribe<Unit>();
	}

	// Token: 0x060004F5 RID: 1269 RVA: 0x00019CE3 File Offset: 0x00017EE3
	public virtual IEnumerator Intro()
	{
		float num = 0.25f;
		this.modal.Content.alpha = 0f;
		this.rectTransform.DOAnchorMin(new Vector2(0f, 0f), num, false);
		this.rectTransform.DOAnchorMax(new Vector2(1f, 1f), num, false);
		yield return new WaitForSeconds(num);
		this.OnAnimFinished.OnNext("Intro");
		yield break;
	}

	// Token: 0x060004F6 RID: 1270 RVA: 0x00019CF2 File Offset: 0x00017EF2
	public IEnumerator FadeInContent()
	{
		float num = 0.25f;
		this.modal.Content.DOFade(1f, num);
		yield return new WaitForSeconds(num);
		this.modal.Content.blocksRaycasts = true;
		yield break;
	}

	// Token: 0x060004F7 RID: 1271 RVA: 0x00019D01 File Offset: 0x00017F01
	public virtual IEnumerator Outro()
	{
		float num = 0.25f;
		this.rectTransform.DOAnchorMin(new Vector2(0f, -1f), num, false);
		this.rectTransform.DOAnchorMax(new Vector2(1f, 0f), num, false);
		yield return new WaitForSeconds(num);
		this.OnAnimFinished.OnNext("Outro");
		yield break;
	}

	// Token: 0x060004F8 RID: 1272 RVA: 0x00019D10 File Offset: 0x00017F10
	protected virtual void OnModalRegistered(bool isRegistered)
	{
		this.PlayIntro();
	}

	// Token: 0x0400045E RID: 1118
	public Subject<string> OnAnimFinished = new Subject<string>();

	// Token: 0x0400045F RID: 1119
	protected IDisposable currentAnimation = Disposable.Empty;

	// Token: 0x04000460 RID: 1120
	protected AnimatedModal modal;

	// Token: 0x04000461 RID: 1121
	[SerializeField]
	protected RectTransform rectTransform;
}
