using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200024E RID: 590
public class ModalController : AnimatedModal
{
	// Token: 0x0600107B RID: 4219 RVA: 0x0004B088 File Offset: 0x00049288
	protected override void Awake()
	{
		PanelBaseClass[] componentsInChildren = base.GetComponentsInChildren<PanelBaseClass>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init(this);
		}
		using (Dictionary<string, Toggle>.Enumerator enumerator = this.toggleMap.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ModalController.<>c__DisplayClass6_0 CS$<>8__locals1 = new ModalController.<>c__DisplayClass6_0();
				CS$<>8__locals1.<>4__this = this;
				CS$<>8__locals1.kvp = enumerator.Current;
				CanvasGroup panel = this.panelMap[CS$<>8__locals1.kvp.Key];
				PanelBaseClass component = panel.GetComponent<PanelBaseClass>();
				if (null != component)
				{
					this.canvasGroupToPanelBaseMap.Add(panel, component);
				}
				CS$<>8__locals1.kvp.Value.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
				{
					CS$<>8__locals1.<>4__this.ToggleCanvasGroup(isOn, panel);
					if (isOn)
					{
						CS$<>8__locals1.<>4__this.currentToggle = CS$<>8__locals1.kvp.Key;
						GameController.Instance.AnalyticService.SendNavActionAnalytics(CS$<>8__locals1.kvp.Value.name, CS$<>8__locals1.<>4__this.name, "");
					}
				}).AddTo(base.gameObject);
				this.ToggleCanvasGroup(CS$<>8__locals1.kvp.Value.isOn, panel);
			}
		}
		base.Awake();
	}

	// Token: 0x0600107C RID: 4220 RVA: 0x0004B1C8 File Offset: 0x000493C8
	public override void Init(NavigationService navService)
	{
		base.Init(navService);
		foreach (KeyValuePair<string, Toggle> keyValuePair in this.toggleMap)
		{
			keyValuePair.Value.OnValueChangedAsObservable().Subscribe(delegate(bool isOn)
			{
				if (isOn)
				{
					this.NavService.CurrentLocation.Value = this.GetLocation();
				}
			}).AddTo(base.gameObject);
		}
	}

	// Token: 0x0600107D RID: 4221 RVA: 0x0004B244 File Offset: 0x00049444
	private void Start()
	{
		if (null != this.btn_Close)
		{
			this.btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.CloseModal)).AddTo(base.gameObject);
		}
	}

	// Token: 0x0600107E RID: 4222 RVA: 0x0004B27D File Offset: 0x0004947D
	protected virtual void ToggleCanvasGroup(bool isOn, CanvasGroup cGroup)
	{
		cGroup.alpha = (float)(isOn ? 1 : 0);
		cGroup.interactable = isOn;
		cGroup.blocksRaycasts = isOn;
		if (isOn && this.canvasGroupToPanelBaseMap.ContainsKey(cGroup))
		{
			this.canvasGroupToPanelBaseMap[cGroup].OnShowPanel();
		}
	}

	// Token: 0x0600107F RID: 4223 RVA: 0x0004B2BD File Offset: 0x000494BD
	public override string GetLocation()
	{
		return string.Format("{0}:{1}", base.GetLocation(), this.currentToggle);
	}

	// Token: 0x06001080 RID: 4224 RVA: 0x0004B2D5 File Offset: 0x000494D5
	public virtual void ShowPanel(string panelName)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			this.panelToOpenWhenenabled = panelName;
			return;
		}
		this.toggleMap[panelName].isOn = true;
	}

	// Token: 0x06001081 RID: 4225 RVA: 0x0004B300 File Offset: 0x00049500
	private void OnEnable()
	{
		if (!string.IsNullOrEmpty(this.panelToOpenWhenenabled))
		{
			string b = this.panelToOpenWhenenabled;
			this.panelToOpenWhenenabled = "";
			foreach (KeyValuePair<string, Toggle> keyValuePair in this.toggleMap)
			{
				keyValuePair.Value.isOn = (keyValuePair.Key == b);
				CanvasGroup cGroup = this.panelMap[keyValuePair.Key];
				this.ToggleCanvasGroup(keyValuePair.Value.isOn, cGroup);
			}
		}
	}

	// Token: 0x04000E16 RID: 3606
	[SerializeField]
	protected StringCanvasGroupDictionary panelMap = new StringCanvasGroupDictionary();

	// Token: 0x04000E17 RID: 3607
	[SerializeField]
	protected StringToggleDictionary toggleMap = new StringToggleDictionary();

	// Token: 0x04000E18 RID: 3608
	[SerializeField]
	private Button btn_Close;

	// Token: 0x04000E19 RID: 3609
	[SerializeField]
	protected Dictionary<CanvasGroup, PanelBaseClass> canvasGroupToPanelBaseMap = new Dictionary<CanvasGroup, PanelBaseClass>();

	// Token: 0x04000E1A RID: 3610
	private string panelToOpenWhenenabled = "";

	// Token: 0x04000E1B RID: 3611
	private string currentToggle;
}
