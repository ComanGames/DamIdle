using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000116 RID: 278
public class UpgradeModalController : ModalController
{
	// Token: 0x0600074E RID: 1870 RVA: 0x00026B60 File Offset: 0x00024D60
	protected override void Awake()
	{
		this.toggleMap["Cash"].isOn = true;
		GameController.Instance.game.CashOnHand.Sample(TimeSpan.FromMilliseconds(250.0)).Subscribe(delegate(double cash)
		{
			this.txt_CashOnHand.text = string.Format("${0}", NumberFormat.Convert(cash, 1000000.0, true, 3));
		}).AddTo(base.gameObject);
		GameController.Instance.AngelService.AngelsOnHand.Sample(TimeSpan.FromMilliseconds(250.0)).Subscribe(delegate(double angels)
		{
			this.txt_Angels.text = string.Format("{0} Angels", NumberFormat.ConvertNormal(angels, 1000000.0, 3));
		}).AddTo(base.gameObject);
		GameController.Instance.game.planetPlayerData.GetObservable("Multipliers", 0.0).CombineLatest(GameController.Instance.game.SubscriptionMultiplier, (double mult, float subscription) => mult * 3.0 + (double)subscription).Subscribe(delegate(double x)
		{
			this.txt_MultiplierValue.text = "x" + x;
		}).AddTo(base.gameObject);
		base.Awake();
	}

	// Token: 0x0600074F RID: 1871 RVA: 0x00026C7C File Offset: 0x00024E7C
	protected override void ToggleCanvasGroup(bool isOn, CanvasGroup cGroup)
	{
		if (isOn)
		{
			UpgradeService upgradeService = GameController.Instance.UpgradeService;
			Func<KeyValuePair<string, CanvasGroup>, bool> <>9__1;
			Func<Upgrade, bool> <>9__2;
			Func<Upgrade, bool> <>9__3;
			GameController.Instance.PlanetThemeService.VentureIcons.Subscribe(delegate(IconDataScriptableObject iconData)
			{
				IEnumerable<KeyValuePair<string, CanvasGroup>> panelMap = this.panelMap;
				Func<KeyValuePair<string, CanvasGroup>, bool> predicate;
				if ((predicate = <>9__1) == null)
				{
					predicate = (<>9__1 = ((KeyValuePair<string, CanvasGroup> panel) => panel.Value == cGroup));
				}
				string key = panelMap.FirstOrDefault(predicate).Key;
				if (key == "Cash")
				{
					IEnumerable<Upgrade> upgrades = upgradeService.Upgrades;
					Func<Upgrade, bool> predicate2;
					if ((predicate2 = <>9__2) == null)
					{
						predicate2 = (<>9__2 = ((Upgrade x) => x.currency == this.cashUpgradePanel.CurrencyType));
					}
					List<Upgrade> source = upgrades.Where(predicate2).ToList<Upgrade>();
					this.cashUpgradePanel.WireData(source.ToList<Upgrade>(), iconData, upgradeService.QuikBuyUnlocked);
					this.ClearUpgradeAvailable(Upgrade.Currency.InGameCash);
					return;
				}
				if (!(key == "Angels"))
				{
					key == "Store";
					return;
				}
				IEnumerable<Upgrade> upgrades2 = upgradeService.Upgrades;
				Func<Upgrade, bool> predicate3;
				if ((predicate3 = <>9__3) == null)
				{
					predicate3 = (<>9__3 = ((Upgrade x) => x.currency == this.angelUpgradePanel.CurrencyType));
				}
				List<Upgrade> source2 = upgrades2.Where(predicate3).ToList<Upgrade>();
				this.angelUpgradePanel.WireData(source2.ToList<Upgrade>(), iconData, upgradeService.QuikBuyUnlocked);
				this.ClearUpgradeAvailable(Upgrade.Currency.AngelInvestors);
			}).AddTo(base.gameObject);
		}
		base.ToggleCanvasGroup(isOn, cGroup);
	}

	// Token: 0x06000750 RID: 1872 RVA: 0x00026CF6 File Offset: 0x00024EF6
	private void ClearUpgradeAvailable(Upgrade.Currency currency)
	{
		GameController.Instance.UpgradeService.NewUpgradeAvailableMap[currency].Value = false;
	}

	// Token: 0x040006CF RID: 1743
	[SerializeField]
	private UpgradePanel cashUpgradePanel;

	// Token: 0x040006D0 RID: 1744
	[SerializeField]
	private UpgradePanel angelUpgradePanel;

	// Token: 0x040006D1 RID: 1745
	[SerializeField]
	private Text txt_CashOnHand;

	// Token: 0x040006D2 RID: 1746
	[SerializeField]
	private Text txt_Angels;

	// Token: 0x040006D3 RID: 1747
	[SerializeField]
	private Text txt_MultiplierValue;
}
