using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000C4 RID: 196
public class PlanetMilestoneRewardsListItem : MonoBehaviour
{
	// Token: 0x17000077 RID: 119
	// (get) Token: 0x0600054D RID: 1357 RVA: 0x0001BA27 File Offset: 0x00019C27
	// (set) Token: 0x0600054E RID: 1358 RVA: 0x0001BA2F File Offset: 0x00019C2F
	public double CurrentProgress { get; private set; }

	// Token: 0x17000078 RID: 120
	// (get) Token: 0x0600054F RID: 1359 RVA: 0x0001BA38 File Offset: 0x00019C38
	// (set) Token: 0x06000550 RID: 1360 RVA: 0x0001BA40 File Offset: 0x00019C40
	public bool HasMadeProgress { get; set; }

	// Token: 0x06000551 RID: 1361 RVA: 0x0001BA4C File Offset: 0x00019C4C
	public void WireData(UserPlanetMilestone pMilestone, UserPlanetMilestone pPreviousMilestone, Action<UserPlanetMilestone> OnClickAction, int index)
	{
		this.disposables.Clear();
		this.milestone = pMilestone;
		this.previousMilestone = pPreviousMilestone;
		this.onClickAction = OnClickAction;
		this.index = index;
		this.txt_reward_index.text = this.index.ToString();
		this.inv = GameController.Instance.GlobalPlayerData.inventory.GetItemById(this.milestone.Reward.Id);
		this.item_icon.Setup(this.milestone.Reward);
		if (null != this.go_reward_amount_parent)
		{
			int qty = this.milestone.Reward.Qty;
			this.txt_reward_amount.text = this.milestone.Reward.Qty.ToString();
			this.go_reward_amount_parent.SetActive(qty > 1);
		}
		this.UpdateState();
		float fillAmount = (float)(this.milestone.DisplayCurrentCount.Value / this.milestone.DisplayCurrentTarget);
		this.HHSImg_PrefillProgressBar.fillAmount = (this.HHSImg_ProgressBar.fillAmount = fillAmount);
		this.CurrentProgress = this.milestone.DisplayCurrentCount.Value;
		this.btn_container.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.onClickAction(this.milestone);
		}).AddTo(this.disposables);
		this.UpdateIconState();
		this.milestone.DisplayCurrentCount.Subscribe(delegate(double x)
		{
			if (x > this.CurrentProgress)
			{
				this.HasMadeProgress = true;
			}
			float fillAmount2 = (float)(this.milestone.DisplayCurrentCount.Value / this.milestone.DisplayCurrentTarget);
			this.HHSImg_PrefillProgressBar.fillAmount = fillAmount2;
		}).AddTo(base.gameObject);
		this.milestone.State.Subscribe(delegate(UserPlanetMilestone.PlanetMilestoneState state)
		{
			if (state == UserPlanetMilestone.PlanetMilestoneState.CLAIMED)
			{
				Observable.Create<Unit>(delegate(IObserver<Unit> observer)
				{
					observer.OnNext(Unit.Default);
					observer.OnCompleted();
					return Disposable.Empty;
				}).Delay(TimeSpan.FromSeconds(1.0)).Subscribe(delegate(Unit _)
				{
					this.UpdateState();
				}).AddTo(base.gameObject);
			}
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000552 RID: 1362 RVA: 0x0001BBF8 File Offset: 0x00019DF8
	public void UpdateIconState()
	{
		UserPlanetMilestone userPlanetMilestone = this.previousMilestone;
		if (userPlanetMilestone == null)
		{
			this.item_icon.RestoreImage();
			return;
		}
		bool flag = userPlanetMilestone.State.Value == UserPlanetMilestone.PlanetMilestoneState.COMPLETE || userPlanetMilestone.State.Value == UserPlanetMilestone.PlanetMilestoneState.CLAIMED;
		if (this.milestone.Reward.isHidden && this.milestone.State.Value == UserPlanetMilestone.PlanetMilestoneState.ACTIVE)
		{
			this.item_icon.DarkOutImage();
			return;
		}
		if (flag)
		{
			this.item_icon.RestoreImage();
			return;
		}
		this.item_icon.GreyOutImage();
	}

	// Token: 0x06000553 RID: 1363 RVA: 0x0001BC86 File Offset: 0x00019E86
	private void OnDestroy()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000554 RID: 1364 RVA: 0x0001BC94 File Offset: 0x00019E94
	private void UpdateState()
	{
		this.go_rays.SetActive(this.milestone.Reward.isHidden);
		this.go_attention_breadcrumb.gameObject.SetActive(this.milestone.State.Value == UserPlanetMilestone.PlanetMilestoneState.COMPLETE);
		this.go_claimed_check.gameObject.SetActive(this.milestone.State.Value == UserPlanetMilestone.PlanetMilestoneState.CLAIMED);
		this.txt_reward_index.gameObject.SetActive(this.milestone.State.Value != UserPlanetMilestone.PlanetMilestoneState.CLAIMED);
		this.go_newtag.gameObject.SetActive(this.inv.IsEquipable() && this.inv.Owned.Value == 0);
		this.UpdateIconState();
	}

	// Token: 0x06000555 RID: 1365 RVA: 0x0001BD64 File Offset: 0x00019F64
	public Sequence TweenToCurrentState(float time)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Pause<Sequence>();
		if (this.CurrentProgress < this.milestone.DisplayCurrentCount.Value)
		{
			float endValue = (float)(this.milestone.DisplayCurrentCount.Value / this.milestone.DisplayCurrentTarget);
			sequence.Append(DOTween.To(() => this.HHSImg_ProgressBar.fillAmount, delegate(float x)
			{
				this.HHSImg_ProgressBar.fillAmount = x;
			}, endValue, time).OnComplete(delegate
			{
				this.CurrentProgress = this.milestone.DisplayCurrentCount.Value;
				this.UpdateState();
			}).SetEase(Ease.Linear)).SetEase(Ease.Linear);
		}
		return sequence;
	}

	// Token: 0x06000556 RID: 1366 RVA: 0x0001BDF8 File Offset: 0x00019FF8
	public void SetSelected(bool isSelected)
	{
		this.go_selected_background.gameObject.SetActive(isSelected);
	}

	// Token: 0x040004B1 RID: 1201
	private const string CHECK_INCOMPLETE = "incomplete";

	// Token: 0x040004B2 RID: 1202
	private const string CHECK_COMPLETE = "complete";

	// Token: 0x040004B3 RID: 1203
	private const string CHECK_CLAIMED = "claimed";

	// Token: 0x040004B4 RID: 1204
	[SerializeField]
	private Button btn_container;

	// Token: 0x040004B5 RID: 1205
	[SerializeField]
	private GameObject go_selected_background;

	// Token: 0x040004B6 RID: 1206
	[SerializeField]
	private PlanetMilestoneRewardIconView item_icon;

	// Token: 0x040004B7 RID: 1207
	[SerializeField]
	private GameObject go_reward_amount_parent;

	// Token: 0x040004B8 RID: 1208
	[SerializeField]
	private Text txt_reward_amount;

	// Token: 0x040004B9 RID: 1209
	[SerializeField]
	private HHSlicedFilledImage HHSImg_ProgressBar;

	// Token: 0x040004BA RID: 1210
	[SerializeField]
	private HHSlicedFilledImage HHSImg_PrefillProgressBar;

	// Token: 0x040004BB RID: 1211
	[SerializeField]
	private Text txt_reward_index;

	// Token: 0x040004BC RID: 1212
	[SerializeField]
	private GameObject go_attention_breadcrumb;

	// Token: 0x040004BD RID: 1213
	[SerializeField]
	private GameObject go_claimed_check;

	// Token: 0x040004BE RID: 1214
	[SerializeField]
	private GameObject go_rays;

	// Token: 0x040004BF RID: 1215
	[SerializeField]
	private GameObject go_newtag;

	// Token: 0x040004C0 RID: 1216
	private UserPlanetMilestone milestone;

	// Token: 0x040004C1 RID: 1217
	private UserPlanetMilestone previousMilestone;

	// Token: 0x040004C2 RID: 1218
	private Action<UserPlanetMilestone> onClickAction;

	// Token: 0x040004C3 RID: 1219
	private int index;

	// Token: 0x040004C4 RID: 1220
	private Item inv;

	// Token: 0x040004C5 RID: 1221
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040004C6 RID: 1222
	[SerializeField]
	public Transform Sparkles_target;
}
