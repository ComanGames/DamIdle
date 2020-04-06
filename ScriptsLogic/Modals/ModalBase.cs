using System;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

// Token: 0x020000BA RID: 186
[RequireComponent(typeof(Canvas))]
public class ModalBase : MonoBehaviour
{
	// Token: 0x060004FB RID: 1275 RVA: 0x00019D36 File Offset: 0x00017F36
	protected virtual void Awake()
	{
		base.gameObject.SetActive(false);
		OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged)).AddTo(base.gameObject);
	}

	// Token: 0x060004FC RID: 1276 RVA: 0x00019D6C File Offset: 0x00017F6C
	public virtual void Init(NavigationService navService)
	{
		this.NavService = navService;
		this.NavService.RegisterModal(this);
	}

	// Token: 0x060004FD RID: 1277 RVA: 0x00019D81 File Offset: 0x00017F81
	public virtual string GetLocation()
	{
		return string.Format("Modal:{0}", base.name);
	}

	// Token: 0x060004FE RID: 1278 RVA: 0x00019D93 File Offset: 0x00017F93
	public virtual void OnModalRegistered()
	{
		this.OnModalRegisteredCb.OnNext(true);
		base.gameObject.SetActive(true);
	}

	// Token: 0x060004FF RID: 1279 RVA: 0x00019DAD File Offset: 0x00017FAD
	public virtual void CloseModal(Unit _)
	{
		this.NavService.UnregisterModal(this);
		Object.Destroy(base.gameObject);
	}

	// Token: 0x06000500 RID: 1280 RVA: 0x00002718 File Offset: 0x00000918
	protected virtual void OnOrientationChanged(OrientationChangedEvent orientation)
	{
	}

	// Token: 0x04000462 RID: 1122
	public bool CanStack;

	// Token: 0x04000463 RID: 1123
	public bool AllowDuplicateModals;

	// Token: 0x04000464 RID: 1124
	public bool IgnoreTint;

	// Token: 0x04000465 RID: 1125
	public bool AllowBackButtonToClose = true;

	// Token: 0x04000466 RID: 1126
	public NavigationService NavService;

	// Token: 0x04000467 RID: 1127
	public Subject<bool> OnModalRegisteredCb = new Subject<bool>();
}
