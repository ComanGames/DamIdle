using System;
using HHTools.Navigation;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001F3 RID: 499
[RequireComponent(typeof(Button))]
public class PopupButton : MonoBehaviour
{
	// Token: 0x06000E90 RID: 3728 RVA: 0x00041465 File Offset: 0x0003F665
	private void Awake()
	{
		base.gameObject.AddComponent<ObservableDestroyTrigger>();
		base.GetComponent<Button>().OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnClick();
		}).AddTo(this);
	}

	// Token: 0x06000E91 RID: 3729 RVA: 0x00041498 File Offset: 0x0003F698
	private void OnClick()
	{
		GameObject target = string.IsNullOrEmpty(this.CallbackMethod) ? null : base.gameObject;
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData(this.Title, this.Text, target, this.CallbackMethod, this.PopupOptions, this.ConfirmButtonText, this.CancelButtonText, this.ShowCloseButton);
	}

	// Token: 0x04000C39 RID: 3129
	[SerializeField]
	private string Title;

	// Token: 0x04000C3A RID: 3130
	[SerializeField]
	[TextArea]
	private string Text;

	// Token: 0x04000C3B RID: 3131
	[SerializeField]
	private string CallbackMethod;

	// Token: 0x04000C3C RID: 3132
	[SerializeField]
	private string ConfirmButtonText = "Dandy!";

	// Token: 0x04000C3D RID: 3133
	[SerializeField]
	private string CancelButtonText = "";

	// Token: 0x04000C3E RID: 3134
	[SerializeField]
	private bool ShowCloseButton;

	// Token: 0x04000C3F RID: 3135
	[SerializeField]
	private PopupModal.PopupOptions PopupOptions;
}
