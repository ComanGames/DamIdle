using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200003A RID: 58
public class AdCapStringReplacer : MonoBehaviour
{
	// Token: 0x0600013D RID: 317 RVA: 0x00008100 File Offset: 0x00006300
	private void Awake()
	{
		this.textComponent = base.GetComponent<Text>();
		if (null != this.textComponent && AdCapExternalDataStorage.Data != null)
		{
			AdCapString adCapString = AdCapExternalDataStorage.Data.AdCapStrings.Find(k => k.Key == this.AdCapStringId);
			if (adCapString != null)
			{
				this.textComponent.text = adCapString.Value;
			}
		}
	}

	// Token: 0x04000168 RID: 360
	public string AdCapStringId;

	// Token: 0x04000169 RID: 361
	private Text textComponent;
}
