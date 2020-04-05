using System;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// Token: 0x020001F8 RID: 504
public class ProfitBoosterView : MonoBehaviour
{
	// Token: 0x06000EBC RID: 3772 RVA: 0x00042760 File Offset: 0x00040960
	public void Init()
	{
		this.AddAudioSources();
		this.SetupBooster();
		if (this.infoButton)
		{
			this.infoButton.OnClickAsObservable().Subscribe(delegate(Unit _)
			{
				string title = string.Format(this.originalName, "");
				GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData(title, this.Setup.infoText, null, "", PopupModal.PopupOptions.OK, "Dandy", "No", true);
			}).AddTo(base.gameObject);
		}
	}

	// Token: 0x06000EBD RID: 3773 RVA: 0x000427AE File Offset: 0x000409AE
	private void SetupBooster()
	{
		GameController.Instance.State.Take(1).Subscribe(delegate(GameState state)
		{
			IObservable<Unit> onDeployClick = this.deployButton.OnClickAsObservable();
			IObservable<Unit> observable = this.boostButton.OnClickAsObservable();
			observable.Subscribe(delegate(Unit _)
			{
				if (this.soundEffects.clickSounds.Length != 0)
				{
					int num = Random.Range(0, this.soundEffects.clickSounds.Length);
					this.clickSource.clip = this.soundEffects.clickSounds[num];
					this.clickSource.Play();
				}
			}).AddTo(base.gameObject);
			this.originalName = this.boostName.text;
			this.boostMultiplier.gameObject.SetActive(this.Setup.display == ProfitBoostSetup.BoostDisplay.corner);
			this.Booster.Value = new ProfitBooster(state, this.Setup, onDeployClick, observable, this.profitBoosterType);
			this.Booster.Value.TimeLeftString.SubscribeToText(this.timer).AddTo(base.gameObject);
			this.Booster.Value.TimeLeftPercent.Subscribe(delegate(float p)
			{
				this.slider.value = Mathf.Clamp(p, 0f, 1f);
			}).AddTo(base.gameObject);
			this.Booster.Value.CurrentState.Subscribe(delegate(ProfitBoosterState s)
			{
				this.deployButton.interactable = (s == ProfitBoosterState.Ready);
			}).AddTo(base.gameObject);
			(from s in this.Booster.Value.CurrentState
			select s == ProfitBoosterState.Active).Subscribe(delegate(bool effectActive)
			{
				GameObject[] array = this.visualEffects;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(effectActive);
				}
				if (effectActive)
				{
					if (this.animator)
					{
						this.animator.Play();
					}
					this.deployStartSource.Play();
					this.deployLoopSource.Play((this.deployStartSource.clip != null) ? Convert.ToUInt64(this.deployStartSource.clip.length) : 0UL);
					if (this.highlightPulse != null)
					{
						this.highlightPulse.SetActive(true);
						return;
					}
				}
				else
				{
					if (this.animator)
					{
						this.animator.Stop();
					}
					this.deployLoopSource.Stop();
					this.deployEndSource.Play();
					if (this.highlightPulse != null)
					{
						this.highlightPulse.SetActive(false);
					}
				}
			}).AddTo(base.gameObject);
			(from s in this.Booster.Value.CurrentState
			select s == ProfitBoosterState.Active).CombineLatest(this.Booster.Value.TotalBoost, (bool ea, float tb) => new Tuple<bool, float>(ea, tb)).Subscribe(delegate(Tuple<bool, float> tuple)
			{
				ProfitBoostSetup.BoostDisplay display = this.Setup.display;
				if (display == ProfitBoostSetup.BoostDisplay.inline)
				{
					string arg = string.Format("<color=#{0}>x", this.ColorToHex(this.Setup.boostTextColor));
					string arg2 = tuple.Item1 ? (arg + tuple.Item2 + "</color>") : "";
					this.boostName.text = string.Format(this.originalName, arg2);
					return;
				}
				if (display != ProfitBoostSetup.BoostDisplay.corner)
				{
					return;
				}
				this.boostMultiplier.text = "x" + tuple.Item2;
			}).AddTo(base.gameObject);
			(from e in this.Booster.Value.CurrentState
			where e == ProfitBoosterState.Recharging
			select e).Subscribe(delegate(ProfitBoosterState _)
			{
				FTUE_Manager.ShowFTUE("ProfitMartiansIAP", null);
			}).AddTo(base.gameObject);
			(from b in this.Booster.Value.ProfitBoosterBoost
			where b > 0f
			select b).Take(1).Subscribe(delegate(float boost)
			{
				this.boostMultiplier.color = this.Setup.boostTextColor;
			}).AddTo(base.gameObject);
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000EBE RID: 3774 RVA: 0x000427E0 File Offset: 0x000409E0
	private void AddAudioSources()
	{
		this.deployStartSource = base.gameObject.AddComponent<AudioSource>();
		this.deployLoopSource = base.gameObject.AddComponent<AudioSource>();
		this.deployEndSource = base.gameObject.AddComponent<AudioSource>();
		this.clickSource = base.gameObject.AddComponent<AudioSource>();
		this.deployStartSource.clip = this.soundEffects.deployStart;
		this.deployLoopSource.clip = this.soundEffects.deployLoop;
		this.deployEndSource.clip = this.soundEffects.deployEnd;
		this.deployLoopSource.loop = true;
	}

	// Token: 0x06000EBF RID: 3775 RVA: 0x0004287F File Offset: 0x00040A7F
	private string ColorToHex(Color32 color)
	{
		return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	}

	// Token: 0x06000EC0 RID: 3776 RVA: 0x000428BC File Offset: 0x00040ABC
	public void UpdateIconSize()
	{
		RectTransform rectTransform = (RectTransform)this.IconContainer.parent;
		this.IconContainer.sizeDelta = new Vector2(rectTransform.rect.height, rectTransform.rect.height);
		this.InfoContainer.offsetMin = new Vector2(rectTransform.rect.height + 10f, this.InfoContainer.anchorMin.y);
	}

	// Token: 0x06000EC1 RID: 3777 RVA: 0x0004293A File Offset: 0x00040B3A
	private void OnDestroy()
	{
		if (this.Booster.Value != null)
		{
			this.Booster.Value.Dispose();
		}
	}

	// Token: 0x04000C72 RID: 3186
	public ProfitBoosterType profitBoosterType;

	// Token: 0x04000C73 RID: 3187
	public Button deployButton;

	// Token: 0x04000C74 RID: 3188
	public Button boostButton;

	// Token: 0x04000C75 RID: 3189
	public Text boostMultiplier;

	// Token: 0x04000C76 RID: 3190
	public Text timer;

	// Token: 0x04000C77 RID: 3191
	public Text boostName;

	// Token: 0x04000C78 RID: 3192
	public Slider slider;

	// Token: 0x04000C79 RID: 3193
	public GameObject[] visualEffects;

	// Token: 0x04000C7A RID: 3194
	public SimpleSpriteAnimation animator;

	// Token: 0x04000C7B RID: 3195
	public Button infoButton;

	// Token: 0x04000C7C RID: 3196
	public GameObject highlightPulse;

	// Token: 0x04000C7D RID: 3197
	public RectTransform IconContainer;

	// Token: 0x04000C7E RID: 3198
	public RectTransform InfoContainer;

	// Token: 0x04000C7F RID: 3199
	public ProfitBoosterView.SFX soundEffects;

	// Token: 0x04000C80 RID: 3200
	public ProfitBoostSetup Setup;

	// Token: 0x04000C81 RID: 3201
	public ReactiveProperty<ProfitBooster> Booster = new ReactiveProperty<ProfitBooster>();

	// Token: 0x04000C82 RID: 3202
	private string originalName;

	// Token: 0x04000C83 RID: 3203
	private AudioSource deployStartSource;

	// Token: 0x04000C84 RID: 3204
	private AudioSource deployLoopSource;

	// Token: 0x04000C85 RID: 3205
	private AudioSource deployEndSource;

	// Token: 0x04000C86 RID: 3206
	private AudioSource clickSource;

	// Token: 0x020008BC RID: 2236
	[Serializable]
	public class SFX
	{
		// Token: 0x04002BD5 RID: 11221
		public AudioClip[] clickSounds;

		// Token: 0x04002BD6 RID: 11222
		public AudioClip deployStart;

		// Token: 0x04002BD7 RID: 11223
		public AudioClip deployLoop;

		// Token: 0x04002BD8 RID: 11224
		public AudioClip deployEnd;
	}
}
