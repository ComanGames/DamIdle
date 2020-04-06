using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000251 RID: 593
public class QuickBuyUpgradeView : MonoBehaviour
{
	// Token: 0x06001092 RID: 4242 RVA: 0x0004BA28 File Offset: 0x00049C28
	private void Start()
	{
		GameController.Instance.IsInitialized.First(init => init).Subscribe(delegate(bool __)
		{
			GameController.Instance.UpgradeService.IsInitialized.First(init => init).Subscribe(delegate(bool ___)
			{
				this.upgradeService = GameController.Instance.UpgradeService;
				this.upgradeService.PurchasableUpgradesMap[this.currency].ObserveCountChanged(true).Subscribe(delegate(int _)
				{
					this.OnUpgradePurchaseableChanged();
				}).AddTo(base.gameObject);
				GameController.Instance.game.CashOnHand.Throttle(this.updateSpan).Subscribe(delegate(double _)
				{
					this.OnUpgradePurchaseableChanged();
				}).AddTo(base.gameObject);
				this.btn_Buy.OnClickAsObservable().Subscribe(delegate(Unit _)
				{
					this.OnClick();
				}).AddTo(base.gameObject);
				Observable.FromEvent(delegate(Action h)
				{
					GameController.Instance.OnSoftResetPost += h;
				}, delegate(Action h)
				{
					GameController.Instance.OnSoftResetPost -= h;
				}).Subscribe(delegate(Unit _)
				{
					this.HandleOnSoftResetPost();
				}).AddTo(this);
			}).AddTo(base.gameObject);
		}).AddTo(base.gameObject);
	}

	// Token: 0x06001093 RID: 4243 RVA: 0x0004BA80 File Offset: 0x00049C80
	private void HandleOnSoftResetPost()
	{
		this.upgradeService.QuikBuyUnlocked.Value = false;
	}

	// Token: 0x06001094 RID: 4244 RVA: 0x0004BA93 File Offset: 0x00049C93
	private void OnClick()
	{
		this.upgradeService.PurchaseAllAvailableUpgradesOfCurrency(this.currency);
	}

	// Token: 0x06001095 RID: 4245 RVA: 0x0004BAA8 File Offset: 0x00049CA8
	private void OnUpgradePurchaseableChanged()
	{
		List<Upgrade> quickPurchaseableUpgrades = this.upgradeService.GetQuickPurchaseableUpgrades(this.currency);
		this.btn_Buy.interactable = (quickPurchaseableUpgrades.Count > 0);
		this.btn_Buy.image.color = (this.btn_Buy.interactable ? this.enabledBtnColor : this.disabledBtnColor);
		string format = (this.currency == Upgrade.Currency.InGameCash) ? "${0}" : "{0}";
		Text text = this.txt_Cost;
		string text2;
		if (quickPurchaseableUpgrades.Count <= 0)
		{
			text2 = "";
		}
		else
		{
			text2 = string.Format(format, NumberFormat.Convert(quickPurchaseableUpgrades.Sum(up => up.cost), 1000000000.0, true, 3));
		}
		text.text = text2;
	}

	// Token: 0x04000E33 RID: 3635
	[SerializeField]
	private Upgrade.Currency currency;

	// Token: 0x04000E34 RID: 3636
	[SerializeField]
	private Text txt_Cost;

	// Token: 0x04000E35 RID: 3637
	[SerializeField]
	private Button btn_Buy;

	// Token: 0x04000E36 RID: 3638
	[SerializeField]
	private Color enabledBtnColor;

	// Token: 0x04000E37 RID: 3639
	[SerializeField]
	private Color disabledBtnColor;

	// Token: 0x04000E38 RID: 3640
	private TimeSpan updateSpan = TimeSpan.FromSeconds(0.5);

	// Token: 0x04000E39 RID: 3641
	private UpgradeService upgradeService;
}
