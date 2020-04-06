using System;
using UniRx;

// Token: 0x0200004D RID: 77
public class BuyMultiplierService
{
	// Token: 0x06000272 RID: 626 RVA: 0x0000DF35 File Offset: 0x0000C135
	public void Init(GameController gameController)
	{
		gameController.State.Subscribe(new Action<GameState>(this.OnGameStateChanged)).AddTo(this.disposable);
	}

	// Token: 0x06000273 RID: 627 RVA: 0x0000DF5A File Offset: 0x0000C15A
	private void OnGameStateChanged(GameState state)
	{
		this.BuyCount.Value = 1;
	}

	// Token: 0x04000223 RID: 547
	public readonly ReactiveProperty<int> BuyCount = new ReactiveProperty<int>(1);

	// Token: 0x04000224 RID: 548
	private CompositeDisposable disposable = new CompositeDisposable();
}
