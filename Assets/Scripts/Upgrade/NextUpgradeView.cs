using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001E2 RID: 482
public class NextUpgradeView : MonoBehaviour
{
	// Token: 0x06000E0E RID: 3598 RVA: 0x0003EBD0 File Offset: 0x0003CDD0
	private void Start()
	{
		GameController.Instance.IsInitialized.First((bool x) => x).Subscribe(delegate(bool _)
		{
			this.Init();
		}).AddTo(base.gameObject);
		this.btn_OnClick.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnClick();
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000E0F RID: 3599 RVA: 0x0003EC50 File Offset: 0x0003CE50
	private void OnDestroy()
	{
		this.gameController.OnSoftResetPost -= this.OnReset;
		this.gameController.OnHardResetPost -= this.OnReset;
		this.disposables.Clear();
		this.upgradeDisposables.Clear();
	}

	// Token: 0x06000E10 RID: 3600 RVA: 0x0003ECA1 File Offset: 0x0003CEA1
	private void OnReset()
	{
		Object.Destroy(base.gameObject);
	}

	// Token: 0x06000E11 RID: 3601 RVA: 0x0003ECB0 File Offset: 0x0003CEB0
	private void Init()
	{
		this.gameController = GameController.Instance;
		this.gameController.UpgradeService.NextUpgrade.Subscribe(delegate(Upgrade x)
		{
			if (x == null)
			{
				base.gameObject.SetActive(false);
				return;
			}
			this.WireData(x);
		}).AddTo(this.disposables);
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.HandleOrientationChanged)).AddTo(this.disposables);
		this.gameController.OnSoftResetPost += this.OnReset;
		this.gameController.OnHardResetPost += this.OnReset;
	}

	// Token: 0x06000E12 RID: 3602 RVA: 0x0003ED4A File Offset: 0x0003CF4A
	private void HandleOrientationChanged(OrientationChangedEvent orientation)
	{
		base.transform.localScale = (orientation.IsPortrait ? (Vector3.one * 0.8f) : Vector3.one);
	}

	// Token: 0x06000E13 RID: 3603 RVA: 0x0003ED78 File Offset: 0x0003CF78
	private void WireData(Upgrade upgrade)
	{
		this.upgradeDisposables.Clear();
		this.gameController.PlanetThemeService.VentureIcons.Subscribe(delegate(IconDataScriptableObject x)
		{
			this.img_Icon.sprite = x.iconMap[upgrade.imageName.ToLower()];
		}).AddTo(base.gameObject);
		VentureUpgrade ventureUpgrade = upgrade as VentureUpgrade;
		AIUpgrade aiupgrade = upgrade as AIUpgrade;
		EverythingUpgrade everythingUpgrade = upgrade as EverythingUpgrade;
		if (ventureUpgrade != null)
		{
			this.txt_Multiplier.text = "x" + ventureUpgrade.profitBonus;
		}
		else if (everythingUpgrade != null)
		{
			this.txt_Multiplier.text = "x" + everythingUpgrade.profitBonus;
		}
		else if (aiupgrade != null)
		{
			this.txt_Multiplier.text = "+" + (aiupgrade.effectivenessAmount * 100.0).ToString("0") + "%";
		}
		else
		{
			this.txt_Multiplier.text = "?";
		}
		upgrade.IsPurchaseable.Subscribe(new Action<bool>(this.UpdateView)).AddTo(this.upgradeDisposables);
		base.gameObject.SetActive(true);
	}

	// Token: 0x06000E14 RID: 3604 RVA: 0x0003EEC0 File Offset: 0x0003D0C0
	private void UpdateView(bool isPurchaseable)
	{
		this.btn_OnClick.interactable = isPurchaseable;
		this.flagBuy.SetActive(isPurchaseable);
		this.flagNext.SetActive(!isPurchaseable);
	}

	// Token: 0x06000E15 RID: 3605 RVA: 0x0003EEE9 File Offset: 0x0003D0E9
	private void OnClick()
	{
		this.gameController.UpgradeService.PurchaseNextUpgrade();
	}

	// Token: 0x04000BFE RID: 3070
	[SerializeField]
	private Image img_Icon;

	// Token: 0x04000BFF RID: 3071
	[SerializeField]
	private Text txt_Multiplier;

	// Token: 0x04000C00 RID: 3072
	[SerializeField]
	private Button btn_OnClick;

	// Token: 0x04000C01 RID: 3073
	[SerializeField]
	private GameObject flagBuy;

	// Token: 0x04000C02 RID: 3074
	[SerializeField]
	private GameObject flagNext;

	// Token: 0x04000C03 RID: 3075
	private GameController gameController;

	// Token: 0x04000C04 RID: 3076
	private CompositeDisposable upgradeDisposables = new CompositeDisposable();

	// Token: 0x04000C05 RID: 3077
	private CompositeDisposable disposables = new CompositeDisposable();
}
