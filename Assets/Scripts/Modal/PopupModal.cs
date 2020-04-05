using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000250 RID: 592
public class PopupModal : AnimatedModal
{
	// Token: 0x06001089 RID: 4233 RVA: 0x0004B480 File Offset: 0x00049680
	private void Start()
	{
		this.btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.CloseModal)).AddTo(base.gameObject);
		this.btn_Confirm.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnConfirmClicked)).AddTo(base.gameObject);
		this.btn_Cancel.OnClickAsObservable().Subscribe(new Action<Unit>(this.OnCancelClicked)).AddTo(base.gameObject);
	}

	// Token: 0x0600108A RID: 4234 RVA: 0x0004B508 File Offset: 0x00049708
	public void WireData(string title, GameObject target = null, string method = "", PopupModal.PopupOptions options = PopupModal.PopupOptions.OK_Cancel, string confirmButtonText = "Yes", string cancelButtonText = "No", bool showClose = true)
	{
		this.txt_Header.text = title;
		this.txt_Copy.text = "";
		this.txt_Copy.gameObject.SetActive(false);
		this.target = target;
		this.okCallback = null;
		this.cancelCallback = null;
		this.method = method;
		this.txt_Bottom.gameObject.SetActive(false);
		this.SetupButtons(options, confirmButtonText, cancelButtonText, showClose);
	}

	// Token: 0x0600108B RID: 4235 RVA: 0x0004B580 File Offset: 0x00049780
	public void WireData(string title, string body, GameObject target = null, string method = "", PopupModal.PopupOptions options = PopupModal.PopupOptions.OK_Cancel, string confirmButtonText = "Yes", string cancelButtonText = "No", bool showClose = true)
	{
		this.txt_Header.text = title;
		this.txt_Copy.text = body;
		this.txt_Copy.gameObject.SetActive(!string.IsNullOrEmpty(body));
		this.target = target;
		this.okCallback = null;
		this.cancelCallback = null;
		this.method = method;
		this.txt_Bottom.gameObject.SetActive(false);
		this.SetupButtons(options, confirmButtonText, cancelButtonText, showClose);
	}

	// Token: 0x0600108C RID: 4236 RVA: 0x0004B5FC File Offset: 0x000497FC
	public void WireData(string title, string body, Action okCallback, PopupModal.PopupOptions options = PopupModal.PopupOptions.OK_Cancel, string confirmButtonText = "Yes", string cancelButtonText = "No", bool showClose = true, Action cancelCallback = null, string bottomText = "")
	{
		this.txt_Header.text = title;
		this.txt_Copy.text = body;
		this.txt_Copy.gameObject.SetActive(!string.IsNullOrEmpty(body));
		this.target = null;
		this.okCallback = okCallback;
		this.cancelCallback = cancelCallback;
		this.txt_Bottom.gameObject.SetActive(!string.IsNullOrEmpty(bottomText));
		this.txt_Bottom.text = bottomText;
		this.SetupButtons(options, confirmButtonText, cancelButtonText, showClose);
	}

	// Token: 0x0600108D RID: 4237 RVA: 0x0004B688 File Offset: 0x00049888
	private void SetupButtons(PopupModal.PopupOptions options, string confirmButtonText, string cancelButtonText, bool showClose)
	{
		this.btn_Cancel.image.color = this.clr_BtnRed;
		this.btn_Confirm.image.color = this.clr_BtnGreen;
		this.txt_Confirm.text = confirmButtonText;
		this.txt_Cancel.text = cancelButtonText;
		this.btn_Close.gameObject.SetActive(showClose);
		this.go_IconContainer.SetActive(false);
		switch (options)
		{
		case PopupModal.PopupOptions.OK:
			this.btn_Confirm.gameObject.SetActive(true);
			this.btn_Cancel.gameObject.SetActive(false);
			return;
		case PopupModal.PopupOptions.OK_Cancel:
			this.btn_Confirm.gameObject.SetActive(true);
			this.btn_Cancel.gameObject.SetActive(true);
			return;
		case PopupModal.PopupOptions.OK_Left:
			this.btn_Confirm.gameObject.SetActive(true);
			this.btn_Cancel.gameObject.SetActive(false);
			return;
		case PopupModal.PopupOptions.Cancel:
			this.btn_Confirm.gameObject.SetActive(false);
			this.btn_Cancel.gameObject.SetActive(true);
			return;
		case PopupModal.PopupOptions.AngelsGold:
			this.go_IconContainer.SetActive(true);
			this.btn_Cancel.gameObject.SetActive(true);
			this.btn_Confirm.gameObject.SetActive(true);
			this.btn_Cancel.image.color = this.clr_BtnPurple;
			this.btn_Confirm.image.color = this.clr_BtnGold;
			this.txt_Header.text = string.Format("<color=#{0}>{1}</color>", this.PurpleTextHex, this.txt_Header.text);
			this.txt_Confirm.text = string.Format("<color=#{0}>{1}</color>", this.GoldTextHex, this.txt_Confirm.text);
			return;
		case PopupModal.PopupOptions.AngelsFree:
			this.go_IconContainer.SetActive(true);
			this.btn_Cancel.gameObject.SetActive(true);
			this.btn_Confirm.gameObject.SetActive(true);
			this.txt_Header.text = string.Format("<color=#{0}>{1}</color>", this.PurpleTextHex, this.txt_Header.text);
			this.btn_Cancel.image.color = this.clr_BtnPurple;
			this.btn_Confirm.image.color = this.clr_BtnGreen;
			return;
		case PopupModal.PopupOptions.None:
			this.btn_Confirm.gameObject.SetActive(false);
			this.btn_Cancel.gameObject.SetActive(false);
			return;
		case PopupModal.PopupOptions.Two_Green_Buttons:
			this.btn_Confirm.gameObject.SetActive(true);
			this.btn_Cancel.gameObject.SetActive(true);
			this.btn_Cancel.image.color = this.clr_BtnGreen;
			this.btn_Confirm.image.color = this.clr_BtnGreen;
			return;
		default:
			return;
		}
	}

	// Token: 0x0600108E RID: 4238 RVA: 0x0004B944 File Offset: 0x00049B44
	private void OnConfirmClicked(Unit u)
	{
		this.btn_Confirm.interactable = false;
		if (this.target)
		{
			this.target.SendMessage(this.method, SendMessageOptions.DontRequireReceiver);
		}
		else if (this.okCallback != null)
		{
			this.okCallback();
		}
		this.CloseModal(u);
	}

	// Token: 0x0600108F RID: 4239 RVA: 0x0004B998 File Offset: 0x00049B98
	private void OnCancelClicked(Unit u)
	{
		if (this.cancelCallback != null)
		{
			this.cancelCallback();
		}
		this.CloseModal(u);
	}

	// Token: 0x06001090 RID: 4240 RVA: 0x0004B9B4 File Offset: 0x00049BB4
	protected override void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		this.tform_Panel.sizeDelta = (orientation.IsPortrait ? new Vector2(650f, this.tform_Panel.sizeDelta.y) : new Vector2(950f, this.tform_Panel.sizeDelta.y));
	}

	// Token: 0x04000E1E RID: 3614
	[SerializeField]
	private Button btn_Confirm;

	// Token: 0x04000E1F RID: 3615
	[SerializeField]
	private Button btn_Cancel;

	// Token: 0x04000E20 RID: 3616
	[SerializeField]
	private Text txt_Confirm;

	// Token: 0x04000E21 RID: 3617
	[SerializeField]
	private Text txt_Cancel;

	// Token: 0x04000E22 RID: 3618
	[SerializeField]
	private Button btn_Close;

	// Token: 0x04000E23 RID: 3619
	[SerializeField]
	private GameObject go_IconContainer;

	// Token: 0x04000E24 RID: 3620
	[SerializeField]
	private Text txt_Header;

	// Token: 0x04000E25 RID: 3621
	[SerializeField]
	private Text txt_Copy;

	// Token: 0x04000E26 RID: 3622
	[SerializeField]
	private Text txt_Bottom;

	// Token: 0x04000E27 RID: 3623
	[SerializeField]
	private RectTransform tform_Panel;

	// Token: 0x04000E28 RID: 3624
	[SerializeField]
	private Color clr_BtnGreen;

	// Token: 0x04000E29 RID: 3625
	[SerializeField]
	private Color clr_BtnRed;

	// Token: 0x04000E2A RID: 3626
	[SerializeField]
	private Color clr_BtnPurple;

	// Token: 0x04000E2B RID: 3627
	[SerializeField]
	private Color clr_BtnGold;

	// Token: 0x04000E2C RID: 3628
	public RectTransform rectTransform;

	// Token: 0x04000E2D RID: 3629
	private string GoldTextHex = "9d7d4a";

	// Token: 0x04000E2E RID: 3630
	private string PurpleTextHex = "bc8dad";

	// Token: 0x04000E2F RID: 3631
	private GameObject target;

	// Token: 0x04000E30 RID: 3632
	private Action okCallback;

	// Token: 0x04000E31 RID: 3633
	private Action cancelCallback;

	// Token: 0x04000E32 RID: 3634
	private string method;

	// Token: 0x02000904 RID: 2308
	public enum PopupOptions
	{
		// Token: 0x04002CF6 RID: 11510
		OK,
		// Token: 0x04002CF7 RID: 11511
		OK_Cancel,
		// Token: 0x04002CF8 RID: 11512
		OK_Left,
		// Token: 0x04002CF9 RID: 11513
		Cancel,
		// Token: 0x04002CFA RID: 11514
		AngelsGold,
		// Token: 0x04002CFB RID: 11515
		AngelsFree,
		// Token: 0x04002CFC RID: 11516
		None,
		// Token: 0x04002CFD RID: 11517
		Two_Green_Buttons
	}
}
