using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Token: 0x02000261 RID: 609
public class VentureViewHack : MonoBehaviour
{
	// Token: 0x06001104 RID: 4356 RVA: 0x0004E3F4 File Offset: 0x0004C5F4
	private void Start()
	{
		this._VentureView = base.GetComponent<VentureView>();
		this._VentureModel = GameController.Instance.game.VentureModels.First(v => v.Id == base.gameObject.name);
		this._GuiButton = this._VentureView.RunTimer.gameObject.AddComponent<Button>();
		this._GuiButton.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this._NewQuantity = this._VentureModel.NumOwned_Base.Value.ToString();
			this._FirstLoop = true;
			this._Gui = new Action(this.ShowGui);
		}).AddTo(this);
	}

	// Token: 0x06001105 RID: 4357 RVA: 0x0004E471 File Offset: 0x0004C671
	private void OnDestroy()
	{
		Object.Destroy(this._GuiButton);
	}

	// Token: 0x06001106 RID: 4358 RVA: 0x0004E480 File Offset: 0x0004C680
	private void ShowGui()
	{
		bool flag;
		if (this._Keyboard != null)
		{
			this._NewQuantity = this._Keyboard.text;
			flag = this._Keyboard.done;
		}
		else
		{
			flag = (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return);
		}
		if (flag)
		{
			this._VentureModel.NumOwned_Base.Value = (double)Convert.ToInt32(this._NewQuantity);
			this._Gui = delegate()
			{
			};
			this._Keyboard = null;
			return;
		}
		GUI.Box(new Rect(10f, 10f, 150f, 50f), "Change Quantity");
		GUI.SetNextControlName("NewQuantity");
		this._NewQuantity = GUI.TextField(new Rect(30f, 30f, 100f, 20f), this._NewQuantity, 6);
		if (this._FirstLoop)
		{
			this._FirstLoop = false;
			GUI.FocusControl("NewQuantity");
		}
	}

	// Token: 0x06001107 RID: 4359 RVA: 0x0004E592 File Offset: 0x0004C792
	private void OnGUI()
	{
		this._Gui();
	}

	// Token: 0x04000EAF RID: 3759
	private VentureView _VentureView;

	// Token: 0x04000EB0 RID: 3760
	private VentureModel _VentureModel;

	// Token: 0x04000EB1 RID: 3761
	private Button _GuiButton;

	// Token: 0x04000EB2 RID: 3762
	private string _NewQuantity = "0";

	// Token: 0x04000EB3 RID: 3763
	private Action _Gui = delegate()
	{
	};

	// Token: 0x04000EB4 RID: 3764
	private bool _FirstLoop;

	// Token: 0x04000EB5 RID: 3765
	private TouchScreenKeyboard _Keyboard;
}
