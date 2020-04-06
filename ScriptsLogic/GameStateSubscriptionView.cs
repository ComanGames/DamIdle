using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001B5 RID: 437
public class GameStateSubscriptionView : MonoBehaviour
{
	// Token: 0x06000D07 RID: 3335 RVA: 0x0003A81A File Offset: 0x00038A1A
	private void Start()
	{
		this._BaseString = this._Target.text;
		GameController.Instance.State.Subscribe(delegate(GameState state)
		{
			this.GetObservableForProperty(this._PropertyName, state).Subscribe(delegate(string v)
			{
				this._Target.text = string.Format(this._BaseString, v);
			}).AddTo(this);
		}).AddTo(this);
	}

	// Token: 0x06000D08 RID: 3336 RVA: 0x0003A850 File Offset: 0x00038A50
	private IObservable<string> GetObservableForProperty(GameStateSubscriptionView.GameStatePropertyViews propertyName, GameState state)
	{
		if (propertyName == GameStateSubscriptionView.GameStatePropertyViews.PurchasedMultiplier)
		{
			return (from v in state.PurchasedMultiplier
			select v.ToString()).AsObservable<string>();
		}
		if (propertyName != GameStateSubscriptionView.GameStatePropertyViews.fluxCapitalorMultiplier)
		{
			Debug.LogErrorFormat("[{0}] not accepted by this component", new object[]
			{
				propertyName
			});
			return Observable.Never<string>();
		}
		return (from v in state.fluxCapitalorQuantity
		select v.ToString()).AsObservable<string>();
	}

	// Token: 0x04000B13 RID: 2835
	[SerializeField]
	private Text _Target;

	// Token: 0x04000B14 RID: 2836
	[SerializeField]
	private GameStateSubscriptionView.GameStatePropertyViews _PropertyName;

	// Token: 0x04000B15 RID: 2837
	private string _BaseString;

	// Token: 0x02000888 RID: 2184
	private enum GameStatePropertyViews
	{
		// Token: 0x04002B45 RID: 11077
		PurchasedMultiplier,
		// Token: 0x04002B46 RID: 11078
		fluxCapitalorMultiplier
	}
}
