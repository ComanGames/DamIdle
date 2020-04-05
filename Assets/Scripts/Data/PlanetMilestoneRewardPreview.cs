using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000C3 RID: 195
public class PlanetMilestoneRewardPreview : MonoBehaviour
{
	// Token: 0x17000075 RID: 117
	// (get) Token: 0x0600053C RID: 1340 RVA: 0x0001B2C5 File Offset: 0x000194C5
	// (set) Token: 0x0600053D RID: 1341 RVA: 0x0001B2CD File Offset: 0x000194CD
	public double CurrentProgress { get; private set; }

	// Token: 0x17000076 RID: 118
	// (get) Token: 0x0600053E RID: 1342 RVA: 0x0001B2D6 File Offset: 0x000194D6
	// (set) Token: 0x0600053F RID: 1343 RVA: 0x0001B2DE File Offset: 0x000194DE
	public bool HasMadeProgress { get; private set; }

	// Token: 0x06000540 RID: 1344 RVA: 0x0001B2E8 File Offset: 0x000194E8
	public void WireData(UserPlanetMilestone pMilestone, UserPlanetMilestone pPreviousMilestone, Action<UserPlanetMilestone> OnClickAction)
	{
		this.disposables.Clear();
		this.milestone = pMilestone;
		this.previousMilestone = pPreviousMilestone;
		this.onClickAction = OnClickAction;
		this.inv = GameController.Instance.GlobalPlayerData.inventory.GetItemById(this.milestone.Reward.Id);
		this.item_icon.Setup(this.milestone.Reward);
		if (null != this.go_reward_amount_parent)
		{
			int qty = this.milestone.Reward.Qty;
			this.txt_reward_amount.text = this.milestone.Reward.Qty.ToString();
			this.go_reward_amount_parent.SetActive(qty > 1);
		}
		this.UpdateState();
		float fillAmount = (float)(this.milestone.CurrentCount.Value / this.milestone.TargetAmount);
		this.HHSImg_ProgressBar.fillAmount = fillAmount;
		this.CurrentProgress = this.milestone.CurrentCount.Value;
		this.txt_progress.text = string.Format("{0}/{1} pts", NumberFormat.ConvertNormal(this.milestone.CurrentCount.Value, 1E+15, 0), NumberFormat.ConvertNormal(this.milestone.TargetAmount, 1E+15, 0));
		this.btn_claim.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.onClickAction(this.milestone);
		}).AddTo(this.disposables);
		this.UpdateIconState();
		this.milestone.CurrentCount.Subscribe(delegate(double x)
		{
			if (x > this.CurrentProgress)
			{
				this.HasMadeProgress = true;
			}
		}).AddTo(this.disposables);
		this.milestone.State.Subscribe(delegate(UserPlanetMilestone.PlanetMilestoneState state)
		{
			if (state == UserPlanetMilestone.PlanetMilestoneState.CLAIMED)
			{
				this.UpdateState();
			}
		}).AddTo(this.disposables);
		if (this.previousMilestone == null)
		{
			this.go_reward_locked_message.SetActive(false);
			this.item_icon.RestoreImage();
			return;
		}
		this.previousMilestone.State.Subscribe(delegate(UserPlanetMilestone.PlanetMilestoneState state)
		{
			this.UpdateState();
		}).AddTo(this.disposables);
	}

	// Token: 0x06000541 RID: 1345 RVA: 0x0001B500 File Offset: 0x00019700
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

	// Token: 0x06000542 RID: 1346 RVA: 0x0001B58E File Offset: 0x0001978E
	private void OnDestroy()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000543 RID: 1347 RVA: 0x0001B59C File Offset: 0x0001979C
	private void UpdateState()
	{
		this.go_attention_breadcrumb.gameObject.SetActive(false);
		this.go_claimed_check.gameObject.SetActive(false);
		this.btn_claim.interactable = false;
		this.txt_btn_claim_label.text = "Claim";
		this.go_sparkles.gameObject.SetActive(false);
		this.go_reward_rays.gameObject.SetActive(this.milestone.Reward.isHidden);
		this.go_newtag.gameObject.SetActive(this.inv.IsEquipable() && this.inv.Owned.Value == 0);
		if (this.previousMilestone != null)
		{
			if (this.previousMilestone.State.Value != UserPlanetMilestone.PlanetMilestoneState.COMPLETE)
			{
				bool flag = this.previousMilestone.State.Value == UserPlanetMilestone.PlanetMilestoneState.CLAIMED;
			}
		}
		this.txt_reward_name.text = this.inv.ItemName;
		this.UpdateIconState();
		switch (this.milestone.State.Value)
		{
		case UserPlanetMilestone.PlanetMilestoneState.ACTIVE:
			this.toggle_btn_image.ChangeSprite("incomplete");
			this.toggle_btn_image.Image.material = null;
			this.toggle_btn_image.Image.color = Color.white;
			this.txt_btn_claim_label.color = new Color(1f, 1f, 1f, 0.345098048f);
			break;
		case UserPlanetMilestone.PlanetMilestoneState.COMPLETE:
			this.toggle_btn_image.ChangeSprite("complete");
			this.toggle_btn_image.Image.material = null;
			this.toggle_btn_image.Image.color = Color.white;
			this.txt_btn_claim_label.color = Color.white;
			this.go_sparkles.gameObject.SetActive(true);
			this.go_attention_breadcrumb.gameObject.SetActive(true);
			this.btn_claim.interactable = true;
			this.txt_btn_claim_label.text = "Claim!";
			break;
		case UserPlanetMilestone.PlanetMilestoneState.CLAIMED:
			this.toggle_btn_image.ChangeSprite("claimed");
			this.toggle_btn_image.Image.material = this.mat_photoshop;
			this.toggle_btn_image.Image.color = new Color(0.4f, 0.521568656f, 0.247058824f, 1f);
			this.txt_btn_claim_label.color = Color.white;
			this.go_claimed_check.gameObject.SetActive(true);
			this.txt_btn_claim_label.text = "Claimed";
			break;
		}
		if (this.milestone.Reward.isHidden && this.milestone.State.Value == UserPlanetMilestone.PlanetMilestoneState.ACTIVE)
		{
			this.txt_reward_name.text = "???";
		}
	}

	// Token: 0x06000544 RID: 1348 RVA: 0x0001B864 File Offset: 0x00019A64
	public Sequence TweenToCurrentState(float time)
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Pause<Sequence>();
		if (this.CurrentProgress < this.milestone.CurrentCount.Value)
		{
			double value = this.milestone.CurrentCount.Value;
			double currentProgress = this.CurrentProgress;
			float endValue = (float)(this.milestone.CurrentCount.Value / this.milestone.TargetAmount);
			sequence.Append(DOTween.To(() => this.HHSImg_ProgressBar.fillAmount, delegate(float x)
			{
				this.HHSImg_ProgressBar.fillAmount = x;
				double num = Math.Floor(this.milestone.TargetAmount * (double)x);
				this.txt_progress.text = string.Format("{0}/{1} pts", num, this.milestone.TargetAmount);
			}, endValue, time).OnComplete(delegate
			{
				this.CurrentProgress = this.milestone.CurrentCount.Value;
				this.txt_progress.text = string.Format("{0}/{1} pts", this.milestone.CurrentCount.Value, this.milestone.TargetAmount);
				this.UpdateState();
			}).SetEase(Ease.Linear)).SetEase(Ease.Linear);
		}
		return sequence;
	}

	// Token: 0x04000498 RID: 1176
	[SerializeField]
	private GameObject go_button_container;

	// Token: 0x04000499 RID: 1177
	[SerializeField]
	private Button btn_claim;

	// Token: 0x0400049A RID: 1178
	[SerializeField]
	private ImageToggleMap toggle_btn_image;

	// Token: 0x0400049B RID: 1179
	[SerializeField]
	private Material mat_photoshop;

	// Token: 0x0400049C RID: 1180
	[SerializeField]
	private PlanetMilestoneRewardIconView item_icon;

	// Token: 0x0400049D RID: 1181
	[SerializeField]
	private GameObject go_reward_amount_parent;

	// Token: 0x0400049E RID: 1182
	[SerializeField]
	private Text txt_reward_amount;

	// Token: 0x0400049F RID: 1183
	[SerializeField]
	private HHSlicedFilledImage HHSImg_ProgressBar;

	// Token: 0x040004A0 RID: 1184
	[SerializeField]
	private GameObject go_attention_breadcrumb;

	// Token: 0x040004A1 RID: 1185
	[SerializeField]
	private GameObject go_claimed_check;

	// Token: 0x040004A2 RID: 1186
	[SerializeField]
	private GameObject go_reward_locked_message;

	// Token: 0x040004A3 RID: 1187
	[SerializeField]
	private GameObject go_sparkles;

	// Token: 0x040004A4 RID: 1188
	[SerializeField]
	private Text txt_btn_claim_label;

	// Token: 0x040004A5 RID: 1189
	[SerializeField]
	private Text txt_progress;

	// Token: 0x040004A6 RID: 1190
	[SerializeField]
	private Text txt_reward_name;

	// Token: 0x040004A7 RID: 1191
	[SerializeField]
	private GameObject go_reward_rays;

	// Token: 0x040004A8 RID: 1192
	[SerializeField]
	private GameObject go_newtag;

	// Token: 0x040004A9 RID: 1193
	private const string PROGRESS_TEXT_STRING = "{0}/{1} pts";

	// Token: 0x040004AA RID: 1194
	private UserPlanetMilestone milestone;

	// Token: 0x040004AB RID: 1195
	private UserPlanetMilestone previousMilestone;

	// Token: 0x040004AC RID: 1196
	private Action<UserPlanetMilestone> onClickAction;

	// Token: 0x040004AD RID: 1197
	private Item inv;

	// Token: 0x040004AE RID: 1198
	private CompositeDisposable disposables = new CompositeDisposable();
}
