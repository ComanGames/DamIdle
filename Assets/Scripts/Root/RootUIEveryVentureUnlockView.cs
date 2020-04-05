using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200017C RID: 380
public class RootUIEveryVentureUnlockView : MonoBehaviour
{
	// Token: 0x06000C11 RID: 3089 RVA: 0x00036218 File Offset: 0x00034418
	private void Awake()
	{
		this.toastRect = this.go_toastRoot.GetComponent<RectTransform>();
		this.toastOnscreenPosition = this.toastRect.anchoredPosition;
		this.toastRect.anchoredPosition = new Vector2(this.toastRect.anchoredPosition.x, this.toastRect.anchoredPosition.y + 90f);
		this.go_toastRoot.SetActive(false);
		this.go_completeRoot.SetActive(false);
		this.go_progressRoot.SetActive(false);
		this.go_noRemainingEveryUnlocksRoot.SetActive(false);
		GameController.Instance.State.Take(1).DelayFrame(1, FrameCountType.Update).Subscribe(new Action<GameState>(this.OnGameStateReady)).AddTo(base.gameObject);
	}

	// Token: 0x06000C12 RID: 3090 RVA: 0x000362E1 File Offset: 0x000344E1
	private void OnDestroy()
	{
		this.stateDisposables.Dispose();
		this.unlockDisposables.Dispose();
	}

	// Token: 0x06000C13 RID: 3091 RVA: 0x000362F9 File Offset: 0x000344F9
	private void DismissToast()
	{
		this.toastingUnlock = null;
		if (this.toastSequence != null)
		{
			this.toastSequence.Kill(false);
			this.toastSequence = this.EndCurrentToast(0.1f);
		}
	}

	// Token: 0x06000C14 RID: 3092 RVA: 0x00036327 File Offset: 0x00034527
	private Sequence EndCurrentToast(float closeTime)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(this.toastRect.DOAnchorPosY(this.toastOnscreenPosition.y + 90f, closeTime, false));
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
			this.UpdateShouldBeShown();
		});
		return sequence;
	}

	// Token: 0x06000C15 RID: 3093 RVA: 0x00036368 File Offset: 0x00034568
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

	// Token: 0x06000C16 RID: 3094 RVA: 0x000363C0 File Offset: 0x000345C0
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

	// Token: 0x06000C17 RID: 3095 RVA: 0x00002718 File Offset: 0x00000918
	private void OnGameStateReady(GameState state)
	{
	}

	// Token: 0x06000C18 RID: 3096 RVA: 0x00036574 File Offset: 0x00034774
	private void UpdateShouldBeShown()
	{
		if ((from x in GameController.Instance.UnlockService.Unlocks
		where !x.EverClaimed.Value
		select x).ToList<Unlock>().Count != 0)
		{
			this.ShouldBeShown.Value = true;
			return;
		}
		if (this.toastQueue.Count == 0 && this.toastingUnlock == null)
		{
			this.ShouldBeShown.Value = false;
			return;
		}
		this.ShouldBeShown.Value = true;
	}

	// Token: 0x06000C19 RID: 3097 RVA: 0x000365FC File Offset: 0x000347FC
	private void RefreshUnlockQueue()
	{
		this.stateDisposables.Clear();
		List<EveryVentureUnlock> unlocks = (from x in GameController.Instance.UnlockService.Unlocks.OfType<EveryVentureUnlock>()
		orderby x.amountToEarn
		select x).ToList<EveryVentureUnlock>();
		List<EveryVentureUnlock> list = (from x in unlocks
		where !x.EverClaimed.Value
		select x).ToList<EveryVentureUnlock>();
		if (list.Count == 0)
		{
			List<Unlock> list2 = (from x in GameController.Instance.UnlockService.Unlocks
			where !x.EverClaimed.Value
			select x).ToList<Unlock>();
			if (list2.Count != 0)
			{
				(from x in list2
				select x.EverClaimed).CombineLatest<bool>().Subscribe(delegate(IList<bool> x)
				{
					int num = x.Count((bool y) => !y);
					this.go_noRemainingEveryUnlocksRoot.SetActive(num > 0);
					this.UpdateShouldBeShown();
				}).AddTo(this.stateDisposables);
			}
			else
			{
				this.go_noRemainingEveryUnlocksRoot.SetActive(true);
			}
			this.UpdateShouldBeShown();
			return;
		}
		this.UpdateShouldBeShown();
		this.currentUnlock.Value = list[0];
		this.RegisterVentureStream();
		this.currentUnlock.CombineLatest(this.currentMax, delegate(EveryVentureUnlock unlock, double current)
		{
			this.unlockDisposables.Clear();
			int num = unlocks.IndexOf(unlock);
			EveryVentureUnlock prevUnlock = null;
			if (num > 0)
			{
				prevUnlock = unlocks[num - 1];
			}
			unlock.Earned.CombineLatest(unlock.Claimed, delegate(bool earned, bool claimed)
			{
				if (!earned)
				{
					this.go_progressRoot.SetActive(true);
					this.go_completeRoot.SetActive(false);
					int num2 = GameController.Instance.game.VentureModels.Count * ((prevUnlock == null) ? 0 : prevUnlock.amountToEarn);
					int num3 = GameController.Instance.game.VentureModels.Count * unlock.amountToEarn - num2;
					this.txt_objective.text = unlock.Goal(GameController.Instance.game);
					this.progressFill.fillAmount = (float)((current - (double)num2) / (double)num3);
					this.img_currentReward.sprite = MainUIController.instance.GetUnlockSprite(unlock);
				}
				else if (!claimed)
				{
					this.go_progressRoot.SetActive(false);
					this.go_completeRoot.SetActive(true);
					this.img_claim_reward.sprite = MainUIController.instance.GetUnlockSprite(unlock);
				}
				return Unit.Default;
			}).Subscribe<Unit>().AddTo(this.unlockDisposables);
			return Unit.Default;
		}).Subscribe<Unit>().AddTo(this.stateDisposables);
		this.btn_claim.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			GameController.Instance.UnlockService.ClaimUnlock(this.currentUnlock.Value);
			this.RefreshUnlockQueue();
		}).AddTo(this.stateDisposables);
	}

	// Token: 0x06000C1A RID: 3098 RVA: 0x000367B0 File Offset: 0x000349B0
	private void RegisterVentureStream()
	{
		(from x in (from x in GameController.Instance.game.VentureModels
		select x.TotalOwned).CombineLatest<double>()
		select x.Sum((double y) => Math.Min(y, (double)this.currentUnlock.Value.amountToEarn))).Subscribe(delegate(double x)
		{
			this.currentMax.Value = x;
		}).AddTo(this.stateDisposables);
	}

	// Token: 0x04000A3C RID: 2620
	[SerializeField]
	private GameObject go_progressRoot;

	// Token: 0x04000A3D RID: 2621
	[SerializeField]
	private HHSlicedFilledImage progressFill;

	// Token: 0x04000A3E RID: 2622
	[SerializeField]
	private Text txt_objective;

	// Token: 0x04000A3F RID: 2623
	[SerializeField]
	private Image img_currentReward;

	// Token: 0x04000A40 RID: 2624
	[SerializeField]
	private List<Button> btn_openUnlocks;

	// Token: 0x04000A41 RID: 2625
	[SerializeField]
	private GameObject go_completeRoot;

	// Token: 0x04000A42 RID: 2626
	[SerializeField]
	private Button btn_claim;

	// Token: 0x04000A43 RID: 2627
	[SerializeField]
	private Image img_claim_reward;

	// Token: 0x04000A44 RID: 2628
	[SerializeField]
	private GameObject go_toastRoot;

	// Token: 0x04000A45 RID: 2629
	[SerializeField]
	private Image img_toastReward;

	// Token: 0x04000A46 RID: 2630
	[SerializeField]
	private Text txt_toastProgress;

	// Token: 0x04000A47 RID: 2631
	[SerializeField]
	private Text txt_toastTitle;

	// Token: 0x04000A48 RID: 2632
	[SerializeField]
	private Text txt_toastDescription;

	// Token: 0x04000A49 RID: 2633
	[SerializeField]
	private Button btn_dismissToast;

	// Token: 0x04000A4A RID: 2634
	[SerializeField]
	private GameObject go_noRemainingEveryUnlocksRoot;

	// Token: 0x04000A4B RID: 2635
	private const float TOAST_ONSCREEN_TIME = 1.5f;

	// Token: 0x04000A4C RID: 2636
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x04000A4D RID: 2637
	private CompositeDisposable unlockDisposables = new CompositeDisposable();

	// Token: 0x04000A4E RID: 2638
	private List<EveryVentureUnlock> everyVentureUnlocks = new List<EveryVentureUnlock>();

	// Token: 0x04000A4F RID: 2639
	public ReactiveProperty<bool> ShouldBeShown = new ReactiveProperty<bool>();

	// Token: 0x04000A50 RID: 2640
	private ReactiveProperty<double> currentMax = new ReactiveProperty<double>(0.0);

	// Token: 0x04000A51 RID: 2641
	private ReactiveProperty<EveryVentureUnlock> currentUnlock = new ReactiveProperty<EveryVentureUnlock>();

	// Token: 0x04000A52 RID: 2642
	private List<Unlock> toastQueue = new List<Unlock>();

	// Token: 0x04000A53 RID: 2643
	private Unlock toastingUnlock;

	// Token: 0x04000A54 RID: 2644
	private RectTransform toastRect;

	// Token: 0x04000A55 RID: 2645
	private Vector2 toastOnscreenPosition;

	// Token: 0x04000A56 RID: 2646
	private Sequence toastSequence;
}
