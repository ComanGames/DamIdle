using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200020A RID: 522
public class MegaTicketStarburst : MonoBehaviour
{
	// Token: 0x06000F35 RID: 3893 RVA: 0x000469B9 File Offset: 0x00044BB9
	private void Start()
	{
		Observable.EveryEndOfFrame().Take(1).Subscribe(delegate(long _)
		{
			bool active = this.nameText.text.Contains("10");
			this.starburst.SetActive(active);
		}).AddTo(this);
	}

	// Token: 0x04000D24 RID: 3364
	[SerializeField]
	private Text nameText;

	// Token: 0x04000D25 RID: 3365
	[SerializeField]
	private GameObject starburst;
}
