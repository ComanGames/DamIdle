using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200016B RID: 363
public class NewUnlockToastBar : MonoBehaviour
{
	// Token: 0x06000B9A RID: 2970 RVA: 0x00035248 File Offset: 0x00033448
	private void Awake()
	{
		this.toastRect = this.go_toastRoot.GetComponent<RectTransform>();
		this.toastOnscreenPosition = this.toastRect.anchoredPosition;
		this.toastRect.anchoredPosition = new Vector2(this.toastRect.anchoredPosition.x, this.toastRect.anchoredPosition.y + (float)this.toastHeight);
		this.go_toastRoot.SetActive(false);
		GameController.Instance.State.Take(1).DelayFrame(1, FrameCountType.Update).Subscribe(new Action<GameState>(this.OnGameStateReady)).AddTo(base.gameObject);
	}

	// Token: 0x06000B9B RID: 2971 RVA: 0x000352EF File Offset: 0x000334EF
	private void OnDestroy()
	{
		this.stateDisposables.Dispose();
		this.unlockDisposables.Dispose();
	}

	// Token: 0x06000B9C RID: 2972 RVA: 0x00035307 File Offset: 0x00033507
	private void DismissToast()
	{
		this.toastingUnlock = null;
		if (this.toastSequence != null)
		{
			this.toastSequence.Kill(false);
			this.toastSequence = this.EndCurrentToast(0.1f);
		}
	}

	// Token: 0x06000B9D RID: 2973 RVA: 0x00035338 File Offset: 0x00033538
	private Sequence EndCurrentToast(float closeTime)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(this.toastRect.DOAnchorPosY(this.toastOnscreenPosition.y + (float)this.toastHeight, closeTime, false));
		sequence.AppendCallback(delegate
		{
			this.toastingUnlock = null;
			this.toastSequence = null;
			if (this.toastQueue.Count > 0)
			{
				this.TryAndShowUnlockToast();
				return;
			}
			this.go_toastRoot.SetActive(false);
		});
		return sequence;
	}

	// Token: 0x06000B9E RID: 2974 RVA: 0x00035384 File Offset: 0x00033584
	private void OnUnlockAchieved(Unlock unlock)
	{
		if (unlock == null)
		{
			return;
		}
		if (unlock.Reward is UnlockRewardVentureCooldownTime || unlock.Reward is UnlockRewardVentureProfitPer || unlock.Reward is UnlockRewardEveryVentureCooldownTime || unlock.Reward is UnlockRewardEveryVentureProfitPer)
		{
			this.toastQueue.Add(unlock);
			this.TryAndShowUnlockToast();
		}
	}

	// Token: 0x06000B9F RID: 2975 RVA: 0x000353DC File Offset: 0x000335DC
	private void TryAndShowUnlockToast()
	{
		if (this.toastingUnlock != null || this.toastQueue.Count == 0)
		{
			return;
		}
		this.toastingUnlock = this.toastQueue[0];
		this.toastQueue.RemoveAt(0);
		if (this.toastingUnlock is SingleVentureUnlock)
		{
			Unlock unlock = this.toastingUnlock;
		}
		else if (this.toastingUnlock is EveryVentureUnlock)
		{
			Unlock unlock2 = this.toastingUnlock;
		}
		this.txt_toastTitle.text = this.toastingUnlock.name;
		this.txt_toastDescription.text = this.toastingUnlock.Bonus(GameController.Instance.game);
		this.txt_toastProgress.text = this.toastingUnlock.amountToEarn + "/" + this.toastingUnlock.amountToEarn;
		this.img_toastReward.sprite = MainUIController.instance.GetUnlockSprite(this.toastingUnlock);
		if (GameController.Instance.game.IsEventPlanet)
		{
			this.img_toastReward.rectTransform.localScale = new Vector3(0.6f, 0.6f, 0f);
		}
		else
		{
			this.img_toastReward.rectTransform.localScale = new Vector3(1f, 1f, 0f);
		}
		this.go_toastRoot.SetActive(true);
		this.toastSequence = DOTween.Sequence();
		this.toastSequence.Append(this.toastRect.DOAnchorPosY(this.toastOnscreenPosition.y, 0.5f, false));
		this.toastSequence.AppendInterval(1.5f);
		this.toastSequence.Append(this.EndCurrentToast(0.25f));
	}

	// Token: 0x06000BA0 RID: 2976 RVA: 0x00035590 File Offset: 0x00033790
	private void OnGameStateReady(GameState state)
	{
		if (!GameController.Instance.UnlockService.ShowsNewToast)
		{
			return;
		}
		GameController.Instance.UnlockService.OnUnlockAchievedFirstTime.Subscribe(new Action<Unlock>(this.OnUnlockAchieved)).AddTo(base.gameObject);
		this.btn_dismissToast.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.DismissToast();
		}).AddTo(base.gameObject);
	}

	// Token: 0x04000A0A RID: 2570
	[SerializeField]
	private GameObject go_toastRoot;

	// Token: 0x04000A0B RID: 2571
	[SerializeField]
	private Image img_toastReward;

	// Token: 0x04000A0C RID: 2572
	[SerializeField]
	private Text txt_toastProgress;

	// Token: 0x04000A0D RID: 2573
	[SerializeField]
	private Text txt_toastTitle;

	// Token: 0x04000A0E RID: 2574
	[SerializeField]
	private Text txt_toastDescription;

	// Token: 0x04000A0F RID: 2575
	[SerializeField]
	private Button btn_dismissToast;

	// Token: 0x04000A10 RID: 2576
	private const float TOAST_ONSCREEN_TIME = 1.5f;

	// Token: 0x04000A11 RID: 2577
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x04000A12 RID: 2578
	private CompositeDisposable unlockDisposables = new CompositeDisposable();

	// Token: 0x04000A13 RID: 2579
	private List<Unlock> toastQueue = new List<Unlock>();

	// Token: 0x04000A14 RID: 2580
	private Unlock toastingUnlock;

	// Token: 0x04000A15 RID: 2581
	private RectTransform toastRect;

	// Token: 0x04000A16 RID: 2582
	private Vector2 toastOnscreenPosition;

	// Token: 0x04000A17 RID: 2583
	private Sequence toastSequence;

	// Token: 0x04000A18 RID: 2584
	private int toastHeight = 135;
}
