using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
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
					int num = x.Count(y => !y);
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
		select x.Sum(y => Math.Min(y, (double)this.currentUnlock.Value.amountToEarn))).Subscribe(delegate(double x)
		{
			this.currentMax.Value = x;
		}).AddTo(this.stateDisposables);
	}

	// Token: 0x04000A3C RID: 2620
	[SerializeField]
	private GameObject go_progressRoot;

	// Token: 0x04000A3D RID: 2621
	[SerializeField]
	private HhSlicedFilledImage progressFill;

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

    public class HhSlicedFilledImage : MaskableGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
    {
        // Token: 0x17000199 RID: 409
        // (get) Token: 0x060013C1 RID: 5057 RVA: 0x0005A896 File Offset: 0x00058A96
        // (set) Token: 0x060013C2 RID: 5058 RVA: 0x0005A89E File Offset: 0x00058A9E
        public Sprite sprite
        {
            get
            {
                return this.m_Sprite;
            }
            set
            {
                if (HhSlicedFilledImage.SetPropertyUtility.SetClass<Sprite>(ref this.m_Sprite, value))
                {
                    this.SetAllDirty();
                }
            }
        }

        // Token: 0x1700019A RID: 410
        // (get) Token: 0x060013C3 RID: 5059 RVA: 0x0005A8B4 File Offset: 0x00058AB4
        // (set) Token: 0x060013C4 RID: 5060 RVA: 0x0005A8D1 File Offset: 0x00058AD1
        public Sprite overrideSprite
        {
            get
            {
                if (!(this.m_OverrideSprite == null))
                {
                    return this.m_OverrideSprite;
                }
                return this.sprite;
            }
            set
            {
                if (HhSlicedFilledImage.SetPropertyUtility.SetClass<Sprite>(ref this.m_OverrideSprite, value))
                {
                    this.SetAllDirty();
                }
            }
        }

        // Token: 0x1700019B RID: 411
        // (get) Token: 0x060013C5 RID: 5061 RVA: 0x0005A8E7 File Offset: 0x00058AE7
        // (set) Token: 0x060013C6 RID: 5062 RVA: 0x0005A8EF File Offset: 0x00058AEF
        public bool preserveAspect
        {
            get
            {
                return this.m_PreserveAspect;
            }
            set
            {
                if (HhSlicedFilledImage.SetPropertyUtility.SetStruct<bool>(ref this.m_PreserveAspect, value))
                {
                    this.SetVerticesDirty();
                }
            }
        }

        // Token: 0x1700019C RID: 412
        // (get) Token: 0x060013C7 RID: 5063 RVA: 0x0005A905 File Offset: 0x00058B05
        // (set) Token: 0x060013C8 RID: 5064 RVA: 0x0005A90D File Offset: 0x00058B0D
        public HhSlicedFilledImage.FillMethod fillMethod
        {
            get
            {
                return this.m_FillMethod;
            }
            set
            {
                if (HhSlicedFilledImage.SetPropertyUtility.SetStruct<HhSlicedFilledImage.FillMethod>(ref this.m_FillMethod, value))
                {
                    this.SetVerticesDirty();
                }
            }
        }

        // Token: 0x1700019D RID: 413
        // (get) Token: 0x060013C9 RID: 5065 RVA: 0x0005A923 File Offset: 0x00058B23
        // (set) Token: 0x060013CA RID: 5066 RVA: 0x0005A92B File Offset: 0x00058B2B
        public float fillAmount
        {
            get
            {
                return this.m_FillAmount;
            }
            set
            {
                if (HhSlicedFilledImage.SetPropertyUtility.SetStruct<float>(ref this.m_FillAmount, Mathf.Clamp01(value)))
                {
                    this.SetVerticesDirty();
                }
            }
        }

        // Token: 0x1700019E RID: 414
        // (get) Token: 0x060013CB RID: 5067 RVA: 0x0005A946 File Offset: 0x00058B46
        // (set) Token: 0x060013CC RID: 5068 RVA: 0x0005A94E File Offset: 0x00058B4E
        public float eventAlphaThreshold
        {
            get
            {
                return this.m_EventAlphaThreshold;
            }
            set
            {
                this.m_EventAlphaThreshold = value;
            }
        }

        // Token: 0x060013CD RID: 5069 RVA: 0x0005A957 File Offset: 0x00058B57
        protected HhSlicedFilledImage()
        {
            base.useLegacyMeshGeneration = false;
        }

        // Token: 0x1700019F RID: 415
        // (get) Token: 0x060013CE RID: 5070 RVA: 0x0005A97C File Offset: 0x00058B7C
        public override Texture mainTexture
        {
            get
            {
                if (!(this.overrideSprite == null))
                {
                    return this.overrideSprite.texture;
                }
                if (this.material != null && this.material.mainTexture != null)
                {
                    return this.material.mainTexture;
                }
                return Graphic.s_WhiteTexture;
            }
        }

        // Token: 0x170001A0 RID: 416
        // (get) Token: 0x060013CF RID: 5071 RVA: 0x0005A9D8 File Offset: 0x00058BD8
        public bool hasBorder
        {
            get
            {
                return this.overrideSprite != null && this.overrideSprite.border.sqrMagnitude > 0f;
            }
        }

        // Token: 0x170001A1 RID: 417
        // (get) Token: 0x060013D0 RID: 5072 RVA: 0x0005AA10 File Offset: 0x00058C10
        public float pixelsPerUnit
        {
            get
            {
                float num = 100f;
                if (this.sprite)
                {
                    num = this.sprite.pixelsPerUnit;
                }
                float num2 = 100f;
                if (base.canvas)
                {
                    num2 = base.canvas.referencePixelsPerUnit;
                }
                return num / num2;
            }
        }

        // Token: 0x060013D1 RID: 5073 RVA: 0x00002718 File Offset: 0x00000918
        public virtual void OnBeforeSerialize()
        {
        }

        // Token: 0x060013D2 RID: 5074 RVA: 0x0005AA5E File Offset: 0x00058C5E
        public virtual void OnAfterDeserialize()
        {
            this.m_FillAmount = Mathf.Clamp(this.m_FillAmount, 0f, 1f);
        }

        // Token: 0x060013D3 RID: 5075 RVA: 0x0005AA7C File Offset: 0x00058C7C
        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            Vector4 vector = (this.overrideSprite == null) ? Vector4.zero : DataUtility.GetPadding(this.overrideSprite);
            Vector2 vector2 = (this.overrideSprite == null) ? Vector2.zero : new Vector2(this.overrideSprite.rect.width, this.overrideSprite.rect.height);
            Rect pixelAdjustedRect = base.GetPixelAdjustedRect();
            int num = Mathf.RoundToInt(vector2.x);
            int num2 = Mathf.RoundToInt(vector2.y);
            Vector4 vector3 = new Vector4(vector.x / (float)num, vector.y / (float)num2, ((float)num - vector.z) / (float)num, ((float)num2 - vector.w) / (float)num2);
            if (shouldPreserveAspect && vector2.sqrMagnitude > 0f)
            {
                float num3 = vector2.x / vector2.y;
                float num4 = pixelAdjustedRect.width / pixelAdjustedRect.height;
                if (num3 > num4)
                {
                    float height = pixelAdjustedRect.height;
                    pixelAdjustedRect.height = pixelAdjustedRect.width * (1f / num3);
                    pixelAdjustedRect.y += (height - pixelAdjustedRect.height) * base.rectTransform.pivot.y;
                }
                else
                {
                    float width = pixelAdjustedRect.width;
                    pixelAdjustedRect.width = pixelAdjustedRect.height * num3;
                    pixelAdjustedRect.x += (width - pixelAdjustedRect.width) * base.rectTransform.pivot.x;
                }
            }
            vector3 = new Vector4(pixelAdjustedRect.x + pixelAdjustedRect.width * vector3.x, pixelAdjustedRect.y + pixelAdjustedRect.height * vector3.y, pixelAdjustedRect.x + pixelAdjustedRect.width * vector3.z, pixelAdjustedRect.y + pixelAdjustedRect.height * vector3.w);
            return vector3;
        }

        // Token: 0x060013D4 RID: 5076 RVA: 0x0005AC74 File Offset: 0x00058E74
        public override void SetNativeSize()
        {
            if (this.overrideSprite != null)
            {
                float x = this.overrideSprite.rect.width / this.pixelsPerUnit;
                float y = this.overrideSprite.rect.height / this.pixelsPerUnit;
                base.rectTransform.anchorMax = base.rectTransform.anchorMin;
                base.rectTransform.sizeDelta = new Vector2(x, y);
                this.SetAllDirty();
            }
        }

        // Token: 0x060013D5 RID: 5077 RVA: 0x0005ACF3 File Offset: 0x00058EF3
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (this.overrideSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }
            this.GenerateSlicedFilledSprite(toFill);
        }

        // Token: 0x060013D6 RID: 5078 RVA: 0x0005AD14 File Offset: 0x00058F14
        private void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
        {
            Vector4 drawingDimensions = this.GetDrawingDimensions(lPreserveAspect);
            Vector4 vector = (this.overrideSprite != null) ? DataUtility.GetOuterUV(this.overrideSprite) : Vector4.zero;
            Color32 color = this.color;
            vh.Clear();
            vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.y), color, new Vector2(vector.x, vector.y));
            vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.w), color, new Vector2(vector.x, vector.w));
            vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.w), color, new Vector2(vector.z, vector.w));
            vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.y), color, new Vector2(vector.z, vector.y));
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        // Token: 0x060013D7 RID: 5079 RVA: 0x0005AE14 File Offset: 0x00059014
        private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax, Vector2 uv2Min, Vector2 uv2Max)
        {
            int currentVertCount = vertexHelper.currentVertCount;
            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uvMin.x, uvMin.y), new Vector2(uv2Min.x, uv2Min.y), Vector3.zero, Vector4.zero);
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uvMin.x, uvMax.y), new Vector2(uv2Min.x, uv2Max.y), Vector3.zero, Vector4.zero);
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uvMax.x, uvMax.y), new Vector2(uv2Max.x, uv2Max.y), Vector3.zero, Vector4.zero);
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uvMax.x, uvMin.y), new Vector2(uv2Max.x, uv2Min.y), Vector3.zero, Vector4.zero);
            vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
        }

        // Token: 0x060013D8 RID: 5080 RVA: 0x0005AF78 File Offset: 0x00059178
        private Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
        {
            for (int i = 0; i <= 1; i++)
            {
                float num = border[i] + border[i + 2];
                if (rect.size[i] < num && num != 0f)
                {
                    float num2 = rect.size[i] / num;
                    ref Vector4 ptr = ref border;
                    int index = i;
                    ptr[index] *= num2;
                    ptr = ref border;
                    index = i + 2;
                    ptr[index] *= num2;
                }
            }
            return border;
        }

        // Token: 0x060013D9 RID: 5081 RVA: 0x0005B010 File Offset: 0x00059210
        private void GenerateSlicedFilledSprite(VertexHelper toFill)
        {
            if (!this.hasBorder)
            {
                this.GenerateSimpleSprite(toFill, false);
                return;
            }
            Vector4 vector;
            Vector4 vector2;
            Vector4 vector3;
            Vector4 vector4;
            if (this.overrideSprite != null)
            {
                vector = DataUtility.GetOuterUV(this.overrideSprite);
                vector2 = DataUtility.GetInnerUV(this.overrideSprite);
                vector3 = DataUtility.GetPadding(this.overrideSprite);
                vector4 = this.overrideSprite.border;
            }
            else
            {
                vector = Vector4.zero;
                vector2 = Vector4.zero;
                vector3 = Vector4.zero;
                vector4 = Vector4.zero;
            }
            Rect pixelAdjustedRect = base.GetPixelAdjustedRect();
            float pixelsPerUnit = this.pixelsPerUnit;
            vector4 = this.GetAdjustedBorders(vector4 / pixelsPerUnit, pixelAdjustedRect);
            vector3 /= pixelsPerUnit;
            HhSlicedFilledImage.s_VertScratch[0] = new Vector2(vector3.x, vector3.y);
            HhSlicedFilledImage.s_VertScratch[3] = new Vector2(pixelAdjustedRect.width - vector3.z, pixelAdjustedRect.height - vector3.w);
            HhSlicedFilledImage.s_VertScratch[1].x = vector4.x;
            HhSlicedFilledImage.s_VertScratch[1].y = vector4.y;
            HhSlicedFilledImage.s_VertScratch[2].x = pixelAdjustedRect.width - vector4.z;
            HhSlicedFilledImage.s_VertScratch[2].y = pixelAdjustedRect.height - vector4.w;
            for (int i = 0; i < 4; i++)
            {
                Vector2[] array = HhSlicedFilledImage.s_VertScratch;
                int num = i;
                array[num].x = array[num].x + pixelAdjustedRect.x;
                Vector2[] array2 = HhSlicedFilledImage.s_VertScratch;
                int num2 = i;
                array2[num2].y = array2[num2].y + pixelAdjustedRect.y;
            }
            HhSlicedFilledImage.s_UVScratch[0] = new Vector2(vector.x, vector.y);
            HhSlicedFilledImage.s_UVScratch[1] = new Vector2(vector2.x, vector2.y);
            HhSlicedFilledImage.s_UVScratch[2] = new Vector2(vector2.z, vector2.w);
            HhSlicedFilledImage.s_UVScratch[3] = new Vector2(vector.z, vector.w);
            float num3 = HhSlicedFilledImage.s_VertScratch[2].x + pixelAdjustedRect.width / 2f;
            if (num3 < 0f)
            {
                float num4 = 1f - HhSlicedFilledImage.s_VertScratch[2].x / (HhSlicedFilledImage.s_VertScratch[2].x - num3);
                Vector2[] array3 = HhSlicedFilledImage.s_VertScratch;
                int num5 = 2;
                array3[num5].x = array3[num5].x - num3;
                Vector2[] array4 = HhSlicedFilledImage.s_UVScratch;
                int num6 = 2;
                array4[num6].x = array4[num6].x - num4 / 2f;
            }
            bool flag = false;
            if (HhSlicedFilledImage.s_VertScratch[2].x < HhSlicedFilledImage.s_VertScratch[1].x)
            {
                flag = true;
                float num7 = 1f - HhSlicedFilledImage.s_VertScratch[1].x / HhSlicedFilledImage.s_VertScratch[2].x;
                HhSlicedFilledImage.s_VertScratch[1].x = HhSlicedFilledImage.s_VertScratch[2].x;
                Vector2[] array5 = HhSlicedFilledImage.s_UVScratch;
                int num8 = 0;
                array5[num8].x = array5[num8].x + num7 / 2f;
            }
            Color32 color = this.color;
            toFill.Clear();
            if (this.fillMethod == HhSlicedFilledImage.FillMethod.Horizontal)
            {
                float num9 = (HhSlicedFilledImage.s_VertScratch[3].x - HhSlicedFilledImage.s_VertScratch[0].x) * this.m_FillAmount;
                for (int j = 2; j >= 0; j--)
                {
                    int num10 = j + 1;
                    float num11 = HhSlicedFilledImage.s_VertScratch[j].x - HhSlicedFilledImage.s_VertScratch[0].x;
                    float num12 = HhSlicedFilledImage.s_VertScratch[num10].x - HhSlicedFilledImage.s_VertScratch[0].x;
                    int num13 = 2;
                    while (num13 >= 0 && num9 > num11)
                    {
                        if (!flag || j != 1)
                        {
                            int num14 = num13 + 1;
                            float num15 = (num9 > num12) ? 0f : (num12 - num9);
                            float num16 = 0f;
                            if (num15 > 0f)
                            {
                                num16 = (HhSlicedFilledImage.s_VertScratch[num10].x - HhSlicedFilledImage.s_VertScratch[j].x - num15) / (HhSlicedFilledImage.s_VertScratch[num10].x - HhSlicedFilledImage.s_VertScratch[j].x);
                                num16 = (HhSlicedFilledImage.s_UVScratch[num10].x - HhSlicedFilledImage.s_UVScratch[j].x) * (1f - num16);
                            }
                            Vector2 vector5 = new Vector2(HhSlicedFilledImage.s_VertScratch[j].x, HhSlicedFilledImage.s_VertScratch[num13].y);
                            Vector2 vector6 = new Vector2(HhSlicedFilledImage.s_VertScratch[num10].x - num15, HhSlicedFilledImage.s_VertScratch[num14].y);
                            HhSlicedFilledImage.AddQuad(toFill, vector5, vector6, color, new Vector2(HhSlicedFilledImage.s_UVScratch[j].x, HhSlicedFilledImage.s_UVScratch[num13].y), new Vector2(HhSlicedFilledImage.s_UVScratch[num10].x - num16, HhSlicedFilledImage.s_UVScratch[num14].y), new Vector2((vector5.x - pixelAdjustedRect.xMin) / pixelAdjustedRect.width, (vector5.y - pixelAdjustedRect.yMin) / pixelAdjustedRect.height), new Vector2((vector6.x - pixelAdjustedRect.xMin) / pixelAdjustedRect.width, (vector6.y - pixelAdjustedRect.yMin) / pixelAdjustedRect.height));
                        }
                        num13--;
                    }
                }
                return;
            }
            if (this.fillMethod == HhSlicedFilledImage.FillMethod.Vertical)
            {
                float num17 = (HhSlicedFilledImage.s_VertScratch[3].y - HhSlicedFilledImage.s_VertScratch[0].y) * this.m_FillAmount;
                for (int k = 2; k >= 0; k--)
                {
                    int num18 = k + 1;
                    float num19 = HhSlicedFilledImage.s_VertScratch[k].y - HhSlicedFilledImage.s_VertScratch[0].y;
                    float num20 = HhSlicedFilledImage.s_VertScratch[num18].y - HhSlicedFilledImage.s_VertScratch[0].y;
                    int num21 = 2;
                    while (num21 >= 0 && num17 > num19)
                    {
                        if (!flag || k != 1)
                        {
                            int num22 = num21 + 1;
                            float num23 = (num17 > num20) ? 0f : (num20 - num17);
                            float num24 = 0f;
                            if (num23 > 0f)
                            {
                                num24 = (HhSlicedFilledImage.s_VertScratch[num18].y - HhSlicedFilledImage.s_VertScratch[k].y - num23) / (HhSlicedFilledImage.s_VertScratch[num18].y - HhSlicedFilledImage.s_VertScratch[k].y);
                                num24 = (HhSlicedFilledImage.s_UVScratch[num18].y - HhSlicedFilledImage.s_UVScratch[k].y) * (1f - num24);
                            }
                            HhSlicedFilledImage.AddQuad(toFill, new Vector2(HhSlicedFilledImage.s_VertScratch[num21].x, HhSlicedFilledImage.s_VertScratch[k].y), new Vector2(HhSlicedFilledImage.s_VertScratch[num22].x, HhSlicedFilledImage.s_VertScratch[num18].y - num23), color, new Vector2(HhSlicedFilledImage.s_UVScratch[num21].x, HhSlicedFilledImage.s_UVScratch[k].y), new Vector2(HhSlicedFilledImage.s_UVScratch[num22].x, HhSlicedFilledImage.s_UVScratch[num18].y - num24), Vector2.zero, Vector2.zero);
                        }
                        num21--;
                    }
                }
            }
        }

        // Token: 0x060013DA RID: 5082 RVA: 0x00002718 File Offset: 0x00000918
        public virtual void CalculateLayoutInputHorizontal()
        {
        }

        // Token: 0x060013DB RID: 5083 RVA: 0x00002718 File Offset: 0x00000918
        public virtual void CalculateLayoutInputVertical()
        {
        }

        // Token: 0x170001A2 RID: 418
        // (get) Token: 0x060013DC RID: 5084 RVA: 0x0005B7DD File Offset: 0x000599DD
        public virtual float minWidth
        {
            get
            {
                return 0f;
            }
        }

        // Token: 0x170001A3 RID: 419
        // (get) Token: 0x060013DD RID: 5085 RVA: 0x0005B7E4 File Offset: 0x000599E4
        public virtual float preferredWidth
        {
            get
            {
                if (this.overrideSprite == null)
                {
                    return 0f;
                }
                return this.overrideSprite.rect.size.x / this.pixelsPerUnit;
            }
        }

        // Token: 0x170001A4 RID: 420
        // (get) Token: 0x060013DE RID: 5086 RVA: 0x0005B824 File Offset: 0x00059A24
        public virtual float flexibleWidth
        {
            get
            {
                return -1f;
            }
        }

        // Token: 0x170001A5 RID: 421
        // (get) Token: 0x060013DF RID: 5087 RVA: 0x0005B7DD File Offset: 0x000599DD
        public virtual float minHeight
        {
            get
            {
                return 0f;
            }
        }

        // Token: 0x170001A6 RID: 422
        // (get) Token: 0x060013E0 RID: 5088 RVA: 0x0005B82C File Offset: 0x00059A2C
        public virtual float preferredHeight
        {
            get
            {
                if (this.overrideSprite == null)
                {
                    return 0f;
                }
                return this.overrideSprite.rect.size.y / this.pixelsPerUnit;
            }
        }

        // Token: 0x170001A7 RID: 423
        // (get) Token: 0x060013E1 RID: 5089 RVA: 0x0005B824 File Offset: 0x00059A24
        public virtual float flexibleHeight
        {
            get
            {
                return -1f;
            }
        }

        // Token: 0x170001A8 RID: 424
        // (get) Token: 0x060013E2 RID: 5090 RVA: 0x0000F40F File Offset: 0x0000D60F
        public virtual int layoutPriority
        {
            get
            {
                return 0;
            }
        }

        // Token: 0x060013E3 RID: 5091 RVA: 0x0005B86C File Offset: 0x00059A6C
        public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (this.m_EventAlphaThreshold >= 1f)
            {
                return true;
            }
            Sprite overrideSprite = this.overrideSprite;
            if (overrideSprite == null)
            {
                return true;
            }
            Vector2 vector;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out vector);
            Rect pixelAdjustedRect = base.GetPixelAdjustedRect();
            vector.x += base.rectTransform.pivot.x * pixelAdjustedRect.width;
            vector.y += base.rectTransform.pivot.y * pixelAdjustedRect.height;
            vector = this.MapCoordinate(vector, pixelAdjustedRect);
            Rect textureRect = overrideSprite.textureRect;
            Vector2 vector2 = new Vector2(vector.x / textureRect.width, vector.y / textureRect.height);
            float x = Mathf.Lerp(textureRect.x, textureRect.xMax, vector2.x) / (float)overrideSprite.texture.width;
            float y = Mathf.Lerp(textureRect.y, textureRect.yMax, vector2.y) / (float)overrideSprite.texture.height;
            bool result;
            try
            {
                result = (overrideSprite.texture.GetPixelBilinear(x, y).a >= this.m_EventAlphaThreshold);
            }
            catch (UnityException ex)
            {
                Debug.LogError("Using clickAlphaThreshold lower than 1 on Image whose sprite texture cannot be read. " + ex.Message + " Also make sure to disable sprite packing for this sprite.", this);
                result = true;
            }
            return result;
        }

        // Token: 0x060013E4 RID: 5092 RVA: 0x0005B9D4 File Offset: 0x00059BD4
        private Vector2 MapCoordinate(Vector2 local, Rect rect)
        {
            Rect rect2 = this.sprite.rect;
            Vector4 border = this.sprite.border;
            Vector4 adjustedBorders = this.GetAdjustedBorders(border / this.pixelsPerUnit, rect);
            for (int i = 0; i < 2; i++)
            {
                if (local[i] > adjustedBorders[i])
                {
                    if (rect.size[i] - local[i] <= adjustedBorders[i + 2])
                    {
                        ref Vector2 ptr = ref local;
                        int index = i;
                        ptr[index] -= rect.size[i] - rect2.size[i];
                    }
                    else
                    {
                        ref Vector2 ptr = ref local;
                        int index = i;
                        ptr[index] -= adjustedBorders[i];
                        local[i] = Mathf.Repeat(local[i], rect2.size[i] - border[i] - border[i + 2]);
                        ptr = ref local;
                        index = i;
                        ptr[index] += border[i];
                    }
                }
            }
            return local;
        }

        // Token: 0x04001051 RID: 4177
        [FormerlySerializedAs("m_Frame")]
        [SerializeField]
        private Sprite m_Sprite;

        // Token: 0x04001052 RID: 4178
        [NonSerialized]
        private Sprite m_OverrideSprite;

        // Token: 0x04001053 RID: 4179
        [SerializeField]
        private bool m_PreserveAspect;

        // Token: 0x04001054 RID: 4180
        [SerializeField]
        private HhSlicedFilledImage.FillMethod m_FillMethod;

        // Token: 0x04001055 RID: 4181
        [Range(0f, 1f)]
        [SerializeField]
        private float m_FillAmount = 1f;

        // Token: 0x04001056 RID: 4182
        private float m_EventAlphaThreshold = 1f;

        // Token: 0x04001057 RID: 4183
        private static readonly Vector2[] s_VertScratch = new Vector2[4];

        // Token: 0x04001058 RID: 4184
        private static readonly Vector2[] s_UVScratch = new Vector2[4];

        // Token: 0x02000923 RID: 2339
        public enum FillMethod
        {
            // Token: 0x04002D53 RID: 11603
            Horizontal,
            // Token: 0x04002D54 RID: 11604
            Vertical
        }

        // Token: 0x02000924 RID: 2340
        internal static class SetPropertyUtility
        {
            // Token: 0x06002D83 RID: 11651 RVA: 0x000B13B4 File Offset: 0x000AF5B4
            public static bool SetColor(ref Color currentValue, Color newValue)
            {
                if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
                {
                    return false;
                }
                currentValue = newValue;
                return true;
            }

            // Token: 0x06002D84 RID: 11652 RVA: 0x000B1403 File Offset: 0x000AF603
            public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
            {
                if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                {
                    return false;
                }
                currentValue = newValue;
                return true;
            }

            // Token: 0x06002D85 RID: 11653 RVA: 0x000B1424 File Offset: 0x000AF624
            public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
            {
                if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                {
                    return false;
                }
                currentValue = newValue;
                return true;
            }
        }
    }
