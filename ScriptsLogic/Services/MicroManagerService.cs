using System;
using UniRx;

// Token: 0x020001FF RID: 511
public class MicroManagerService : IDisposable
{
	// Token: 0x06000ED3 RID: 3795 RVA: 0x00043198 File Offset: 0x00041398
	public void Dispose()
	{
		this.stateDisposables.Dispose();
		this.disposables.Dispose();
	}

	// Token: 0x06000ED4 RID: 3796 RVA: 0x000431B0 File Offset: 0x000413B0
	public void Init()
	{
		GameController.Instance.State.Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
		GameController.Instance.OnLoadNewPlanetPre += delegate()
		{
			this.areVenturesMicroManaged.Value = false;
		};
	}

	// Token: 0x06000ED5 RID: 3797 RVA: 0x000431F0 File Offset: 0x000413F0
	public void BoostVentures()
	{
		if (this.state != null && !this.state.IsEventPlanet)
		{
			MessageBroker.Default.Publish<MicroManagerService.MicroManagerBoostValues>(new MicroManagerService.MicroManagerBoostValues
			{
				effectiveManagerBonusMultiplier = this.effectiveManagerBonusMultiplier,
				managerBonusDuration = this.managerBonusTime
			});
			return;
		}
		this.areVenturesMicroManaged.Value = false;
	}

	// Token: 0x06000ED6 RID: 3798 RVA: 0x0004324C File Offset: 0x0004144C
	private void OnStateChanged(GameState state)
	{
		this.state = state;
		if (this.state != null && !this.state.IsEventPlanet)
		{
			PlayerData planetPlayerData = GameController.Instance.game.planetPlayerData;
			bool @bool = planetPlayerData.GetBool("isMicroManagerEnabled");
			if (@bool)
			{
				int @int = planetPlayerData.GetInt("microManagerPowerUps", 0);
				int num = (@int > 0) ? (@int * 2) : 1;
				bool bool2 = planetPlayerData.GetBool("isMicroManagerBoostedOnPlanet");
				int int2 = planetPlayerData.GetInt("MMBoostMultuplier", 1);
				num *= (bool2 ? (this.microManagerPlanetBoostValue * int2) : 1);
				this.effectiveManagerBonusMultiplier = this.managerBonusMultiplier + (float)num;
			}
			this.areVenturesMicroManaged.Value = @bool;
			return;
		}
		this.areVenturesMicroManaged.Value = false;
	}

	// Token: 0x04000CBB RID: 3259
	public ReactiveProperty<bool> areVenturesMicroManaged = new BoolReactiveProperty(false);

	// Token: 0x04000CBC RID: 3260
	private GameState state;

	// Token: 0x04000CBD RID: 3261
	private float managerBonusTime = 0.25f;

	// Token: 0x04000CBE RID: 3262
	private float managerBonusMultiplier = 3f;

	// Token: 0x04000CBF RID: 3263
	private int microManagerPlanetBoostValue = 2;

	// Token: 0x04000CC0 RID: 3264
	private float effectiveManagerBonusMultiplier;

	// Token: 0x04000CC1 RID: 3265
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000CC2 RID: 3266
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x020008BE RID: 2238
	public struct MicroManagerBoostValues
	{
		// Token: 0x04002BE0 RID: 11232
		public float managerBonusDuration;

		// Token: 0x04002BE1 RID: 11233
		public float effectiveManagerBonusMultiplier;
	}
}
