using System;
using UnityEngine;

// Token: 0x02000246 RID: 582
public static class PanelController
{
	// Token: 0x06001063 RID: 4195 RVA: 0x0004AB40 File Offset: 0x00048D40
	private static void ShowRectTransform(RectTransform rectTransform)
	{
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.GetComponent<Canvas>().enabled = true;
		rectTransform.gameObject.BroadcastMessage("OnShowModal", SendMessageOptions.DontRequireReceiver);
	}

	// Token: 0x06001064 RID: 4196 RVA: 0x0004AB6A File Offset: 0x00048D6A
	private static void HideRectTransform(RectTransform rectTransform)
	{
		rectTransform.GetComponent<Canvas>().enabled = false;
		rectTransform.gameObject.BroadcastMessage("OnHideModal", SendMessageOptions.DontRequireReceiver);
	}

	// Token: 0x06001065 RID: 4197 RVA: 0x0004AB89 File Offset: 0x00048D89
	public static void Show(this RectTransform value)
	{
		PanelController.ShowRectTransform(value);
	}

	// Token: 0x06001066 RID: 4198 RVA: 0x0004AB91 File Offset: 0x00048D91
	public static void Hide(this RectTransform value)
	{
		if (value)
		{
			PanelController.HideRectTransform(value);
		}
	}
}
