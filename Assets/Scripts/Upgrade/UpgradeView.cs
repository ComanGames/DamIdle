using System;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200011B RID: 283
public class UpgradeView : MonoBehaviour
{
	// Token: 0x1700008C RID: 140
	// (get) Token: 0x06000793 RID: 1939 RVA: 0x000285CD File Offset: 0x000267CD
	public Upgrade Upgrade
	{
		get
		{
			return this.upgrade;
		}
	}

	// Token: 0x06000794 RID: 1940 RVA: 0x000285D5 File Offset: 0x000267D5
	private void OnDisable()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000795 RID: 1941 RVA: 0x000285E4 File Offset: 0x000267E4
	private void CompletePurchase()
	{
		GameController.Instance.UpgradeService.Purchase(this.upgrade).Subscribe(delegate(bool x)
		{
		}, delegate(Exception onError)
		{
			GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Oh no!!", string.Format("We ran into a problem purchasing your upgrade!", Array.Empty<object>()), null, null, PopupModal.PopupOptions.OK, "Dandy!", null, false);
			this.btn_Buy.interactable = true;
		});
	}

	// Token: 0x06000796 RID: 1942 RVA: 0x00028638 File Offset: 0x00026838
	public void ShowConfirmAngelInvestorSpend(double angelCost)
	{
		string arg = NumberFormat.ConvertNormal(angelCost, 1000000.0, 0);
		string title = string.Format("Are you sure you want to spend {0} angels?", arg);
		double num = Math.Ceiling(angelCost / GameController.Instance.AngelService.AngelsOnHand.Value * 100.0);
		if (num >= 15.0)
		{
			title = string.Format("Egads! Are you sure you want to spend {0:N0}% of your angel investors? ", num);
		}
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData(title, "", new Action(this.CompletePurchase), PopupModal.PopupOptions.OK_Cancel, "Yes", "No", false, delegate()
		{
			this.btn_Buy.interactable = true;
		}, "");
	}

	// Token: 0x06000797 RID: 1943 RVA: 0x000286F0 File Offset: 0x000268F0
	public void WireData(Upgrade upgrade, Sprite icon, Action<Upgrade> onClick)
	{
		this.disposables.Clear();
		this.btn_Buy.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.btn_Buy.interactable = false;
			if (upgrade.currency == Upgrade.Currency.AngelInvestors && GameController.Instance.AngelService.ShouldPromptOnAngelPurchase(upgrade.cost))
			{
				this.ShowConfirmAngelInvestorSpend(upgrade.cost);
				return;
			}
			this.CompletePurchase();
		}).AddTo(this.disposables);
		this.upgrade = upgrade;
		this.upgrade.IsPurchaseable.Subscribe(delegate(bool x)
		{
			this.btn_Buy.interactable = x;
			this.btn_Buy.image.color = (x ? this.enabledBtnColor : this.disabledBtnColor);
		}).AddTo(this.disposables);
		(from x in this.upgrade.IsPurchased
		where x
		select x).Subscribe(delegate(bool _)
		{
			this.disposables.Dispose();
		}).AddTo(this.disposables);
		base.name = this.upgrade.name;
		this.img_Icon.sprite = icon;
		this.txt_UpgradeName.text = this.upgrade.name;
		if (this.upgrade.currency == Upgrade.Currency.Megabucks)
		{
			this.txt_Description.text = "Permanent " + this.upgrade.Effect;
		}
		else
		{
			this.txt_Description.text = this.upgrade.Effect;
		}
		switch (this.upgrade.currency)
		{
		case Upgrade.Currency.InGameCash:
			this.txt_Price.text = string.Format("${0}", NumberFormat.Convert(this.upgrade.cost, 1000000000.0, true, 3));
			return;
		case Upgrade.Currency.AngelInvestors:
			this.txt_Price.text = string.Format("{0} Angel{1}", NumberFormat.ConvertNormal(this.upgrade.cost, 1000000.0, 0), (this.upgrade.cost != 1.0) ? "s" : "");
			return;
		case Upgrade.Currency.Megabucks:
			this.txt_Price.text = NumberFormat.ConvertNormal(this.upgrade.cost, 1000000.0, 0);
			return;
		default:
			return;
		}
	}

	// Token: 0x0400070B RID: 1803
	[SerializeField]
	private Text txt_Description;

	// Token: 0x0400070C RID: 1804
	[SerializeField]
	private Text txt_UpgradeName;

	// Token: 0x0400070D RID: 1805
	[SerializeField]
	private Text txt_Price;

	// Token: 0x0400070E RID: 1806
	[SerializeField]
	private Button btn_Buy;

	// Token: 0x0400070F RID: 1807
	[SerializeField]
	private Image img_Icon;

	// Token: 0x04000710 RID: 1808
	[SerializeField]
	private Color enabledBtnColor;

	// Token: 0x04000711 RID: 1809
	[SerializeField]
	private Color disabledBtnColor;

	// Token: 0x04000712 RID: 1810
	private Upgrade upgrade;

	// Token: 0x04000713 RID: 1811
	private CompositeDisposable disposables = new CompositeDisposable();
}
