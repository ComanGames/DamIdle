using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000B5 RID: 181
public class MenuPanelPortrait : MenuPanelBase
{
	// Token: 0x060004DC RID: 1244 RVA: 0x000198B0 File Offset: 0x00017AB0
	private void Start()
	{
		this.btn_TintClose.OnClickAsObservable().Subscribe(new Action<Unit>(this.HideMenu)).AddTo(base.gameObject);
		this.btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.HideMenu)).AddTo(base.gameObject);
		this.btn_Menu.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowMenu)).AddTo(base.gameObject);
		this.btn_StoreRoot.OnClickAsObservable().Subscribe(new Action<Unit>(this.ShowStore)).AddTo(base.gameObject);
		NotificationController.instance.HasNotification.Subscribe(new Action<bool>(this.OnNotificationChanged)).AddTo(base.gameObject);
		this.HideMenu(Unit.Default);
	}

	// Token: 0x060004DD RID: 1245 RVA: 0x00019990 File Offset: 0x00017B90
	public void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		base.gameObject.SetActive(orientation.IsPortrait);
		if (!orientation.IsPortrait)
		{
			this.HideMenu(Unit.Default);
		}
	}

	// Token: 0x060004DE RID: 1246 RVA: 0x000199B6 File Offset: 0x00017BB6
	protected override void ShowCareer(Unit u)
	{
		this.HideMenu(u);
		base.ShowCareer(u);
	}

	// Token: 0x060004DF RID: 1247 RVA: 0x000199C6 File Offset: 0x00017BC6
	protected override void ShowUnlocks(Unit u)
	{
		this.HideMenu(u);
		base.ShowUnlocks(u);
	}

	// Token: 0x060004E0 RID: 1248 RVA: 0x000199D6 File Offset: 0x00017BD6
	protected override void ShowUpgrades(Unit u)
	{
		this.HideMenu(u);
		base.ShowUpgrades(u);
	}

	// Token: 0x060004E1 RID: 1249 RVA: 0x000199E6 File Offset: 0x00017BE6
	protected override void ShowManagers(Unit u)
	{
		this.HideMenu(u);
		base.ShowManagers(u);
	}

	// Token: 0x060004E2 RID: 1250 RVA: 0x000199F6 File Offset: 0x00017BF6
	protected override void ShowInvestors(Unit u)
	{
		this.HideMenu(u);
		base.ShowInvestors(u);
	}

	// Token: 0x060004E3 RID: 1251 RVA: 0x00019A06 File Offset: 0x00017C06
	protected override void ShowAdventures(Unit u)
	{
		this.HideMenu(u);
		base.ShowAdventures(u);
	}

	// Token: 0x060004E4 RID: 1252 RVA: 0x00019A16 File Offset: 0x00017C16
	protected override void ShowStore(Unit u)
	{
		this.HideMenu(u);
		base.ShowStore(u);
	}

	// Token: 0x060004E5 RID: 1253 RVA: 0x00019A26 File Offset: 0x00017C26
	protected override void ShowConnect(Unit u)
	{
		this.HideMenu(u);
		base.ShowConnect(u);
	}

	// Token: 0x060004E6 RID: 1254 RVA: 0x00019A38 File Offset: 0x00017C38
	private void ShowMenu(Unit u)
	{
		GameController.Instance.NavigationService.OnOpenMenu();
		this.animator.SetBool("IsOpen", true);
		this.openGroup.blocksRaycasts = true;
		this.closedGroup.blocksRaycasts = false;
		if (KongregateAPI.GetAPI() != null)
		{
			KongregateAPI.GetAPI().Analytics.AddEvent("delta.menu.open", null);
		}
	}

	// Token: 0x060004E7 RID: 1255 RVA: 0x00019A9C File Offset: 0x00017C9C
	private void HideMenu(Unit u)
	{
		GameController.Instance.NavigationService.OnCloseMenu();
		this.animator.SetBool("IsOpen", false);
		this.openGroup.alpha = 0f;
		this.openGroup.blocksRaycasts = false;
		this.closedGroup.alpha = 1f;
		this.closedGroup.blocksRaycasts = true;
	}

	// Token: 0x060004E8 RID: 1256 RVA: 0x00019B01 File Offset: 0x00017D01
	private void OnNotificationChanged(bool hasNofications)
	{
		this.notification.gameObject.SetActive(hasNofications);
	}

	// Token: 0x060004E9 RID: 1257 RVA: 0x00019B14 File Offset: 0x00017D14
	protected override void OnClickShowOfferwall(Unit u)
	{
		GameController.Instance.AnalyticService.SendAdStartEvent("Offerwall", "Offerwall", "Offerwall_Root_Portrait");
		GameController.Instance.OfferwallService.ShowOfferwall();
	}

	// Token: 0x04000454 RID: 1108
	[SerializeField]
	private CanvasGroup closedGroup;

	// Token: 0x04000455 RID: 1109
	[SerializeField]
	private Button btn_Close;

	// Token: 0x04000456 RID: 1110
	[SerializeField]
	private Button btn_StoreRoot;

	// Token: 0x04000457 RID: 1111
	[SerializeField]
	private Button btn_Menu;

	// Token: 0x04000458 RID: 1112
	[SerializeField]
	private Button btn_TintClose;

	// Token: 0x04000459 RID: 1113
	[SerializeField]
	private GameObject notification;

	// Token: 0x0400045A RID: 1114
	[SerializeField]
	private CanvasGroup openGroup;

	// Token: 0x0400045B RID: 1115
	[SerializeField]
	private Animator animator;
}
