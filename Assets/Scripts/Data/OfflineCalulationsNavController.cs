using System;
using HHTools.Navigation;
using UniRx;
using UnityEngine;

// Token: 0x020000BF RID: 191
public class OfflineCalulationsNavController : MonoBehaviour
{
	// Token: 0x0600051F RID: 1311 RVA: 0x0001A85C File Offset: 0x00018A5C
	private void Awake()
	{
		GameController.Instance.IsInitialized.First((bool x) => x).Subscribe(delegate(bool _)
		{
			GameController.Instance.OfflineEarningsCalculated.Subscribe(new Action<OfflineEarnings>(this.OnOfflineEarningsCalculated)).AddTo(base.gameObject);
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000520 RID: 1312 RVA: 0x0001A8B4 File Offset: 0x00018AB4
	private void OnOfflineEarningsCalculated(OfflineEarnings offline)
	{
		if (offline.CashEarned > 0.0 && offline.Elapsed > 0.0)
		{
			GameController.Instance.NavigationService.CreateModal<WelcomeBack>(NavModals.WELCOME_BACK, false).WireData(offline);
		}
	}
}
