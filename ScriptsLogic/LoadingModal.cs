using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000242 RID: 578
public class LoadingModal : AnimatedModal
{
	// Token: 0x06001043 RID: 4163 RVA: 0x0004A277 File Offset: 0x00048477
	public void WireData(string subTitle)
	{
		this.txt_Message.text = subTitle;
	}

	// Token: 0x04000DE1 RID: 3553
	[SerializeField]
	private Text txt_Message;
}
