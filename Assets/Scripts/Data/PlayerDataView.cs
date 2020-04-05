using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001F2 RID: 498
public class PlayerDataView : MonoBehaviour
{
	// Token: 0x06000E8A RID: 3722 RVA: 0x00041284 File Offset: 0x0003F484
	private void Start()
	{
		this._Target.Subscribe(new Action<double>(this.OnTargetChanged));
		GameController.Instance.State.Take(1).Subscribe(new Action<GameState>(this.OnStateReady)).AddTo(this);
	}

	// Token: 0x06000E8B RID: 3723 RVA: 0x000412D4 File Offset: 0x0003F4D4
	private void OnStateReady(GameState state)
	{
		IObservable<double> source = Observable.Never<double>();
		PlayerDataView.PlayerDataType type = this._Type;
		if (type != PlayerDataView.PlayerDataType.Global)
		{
			if (type == PlayerDataView.PlayerDataType.Planet)
			{
				source = state.planetPlayerData.GetObservable(this._PlayerDataName, 0.0);
			}
		}
		else
		{
			source = GameController.Instance.GlobalPlayerData.GetObservable(this._PlayerDataName, 0.0);
		}
		source.Subscribe(delegate(double v)
		{
			this._Target.Value = v;
		}).AddTo(this);
	}

	// Token: 0x06000E8C RID: 3724 RVA: 0x0004134C File Offset: 0x0003F54C
	private void OnTargetChanged(double value)
	{
		this.disposables.Clear();
		if (Math.Abs(this._CurrentValue - this._Target.Value) <= (double)this._SecondsToComplete)
		{
			this._CurrentTime = this._SecondsToComplete - Time.deltaTime;
		}
		else
		{
			this._CurrentTime = 0f;
		}
		Observable.EveryUpdate().TakeWhile((long _) => this._CurrentTime < this._SecondsToComplete).Subscribe(delegate(long _)
		{
			if (this.shouldRoundToInt)
			{
				this._CurrentTime += Time.deltaTime;
				this._CurrentValue = (double)this.Lerp(this._CurrentValue, this._Target.Value, this._CurrentTime / this._SecondsToComplete);
				this._Textbox.text = string.Format(this._FormatString, this._CurrentValue);
				return;
			}
			this._Textbox.text = string.Format(this._FormatString, value);
		}).AddTo(this.disposables);
	}

	// Token: 0x06000E8D RID: 3725 RVA: 0x000413EA File Offset: 0x0003F5EA
	private int Lerp(double start, double end, float t)
	{
		return Mathf.RoundToInt((1f - t) * Convert.ToSingle(start) + t * Convert.ToSingle(end));
	}

	// Token: 0x04000C2F RID: 3119
	[SerializeField]
	private PlayerDataView.PlayerDataType _Type;

	// Token: 0x04000C30 RID: 3120
	[SerializeField]
	private string _PlayerDataName;

	// Token: 0x04000C31 RID: 3121
	[SerializeField]
	private Text _Textbox;

	// Token: 0x04000C32 RID: 3122
	[SerializeField]
	private float _SecondsToComplete = 1f;

	// Token: 0x04000C33 RID: 3123
	[SerializeField]
	private string _FormatString = "{0}";

	// Token: 0x04000C34 RID: 3124
	[SerializeField]
	private bool shouldRoundToInt = true;

	// Token: 0x04000C35 RID: 3125
	private ReactiveProperty<double> _Target = new ReactiveProperty<double>(0.0);

	// Token: 0x04000C36 RID: 3126
	private float _CurrentTime;

	// Token: 0x04000C37 RID: 3127
	private double _CurrentValue;

	// Token: 0x04000C38 RID: 3128
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x020008B6 RID: 2230
	private enum PlayerDataType
	{
		// Token: 0x04002BBB RID: 11195
		Global,
		// Token: 0x04002BBC RID: 11196
		Planet
	}
}
