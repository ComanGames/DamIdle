using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000184 RID: 388
public class UnlockItemView : MonoBehaviour
{
	// Token: 0x06000C4F RID: 3151 RVA: 0x000376D7 File Offset: 0x000358D7
	public void WireData(Sprite icon, string amount, string description)
	{
		this.img_Icon.sprite = icon;
		this.txt_Amount.text = amount;
		this.txt_Description.text = description;
	}

	// Token: 0x04000A82 RID: 2690
	[SerializeField]
	private Image img_Icon;

	// Token: 0x04000A83 RID: 2691
	[SerializeField]
	private Text txt_Amount;

	// Token: 0x04000A84 RID: 2692
	[SerializeField]
	private Text txt_Description;
}
