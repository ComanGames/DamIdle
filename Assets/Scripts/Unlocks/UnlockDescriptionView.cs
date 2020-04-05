using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000183 RID: 387
public class UnlockDescriptionView : MonoBehaviour
{
	// Token: 0x06000C49 RID: 3145 RVA: 0x000375F0 File Offset: 0x000357F0
	private void Awake()
	{
		this.cGrp_Description.alpha = 0f;
		this.cGrp_Description.blocksRaycasts = false;
		this.btn_Click.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.timer = 0f;
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000C4A RID: 3146 RVA: 0x00037641 File Offset: 0x00035841
	private void OnDestroy()
	{
		if (this.showDescriptionCR != null)
		{
			base.StopCoroutine(this.showDescriptionCR);
			this.showDescriptionCR = null;
		}
	}

	// Token: 0x06000C4B RID: 3147 RVA: 0x00037660 File Offset: 0x00035860
	public void WireData(string desciption, string unlockName)
	{
		this.txt_UnlockName.text = unlockName;
		this.txt_UnlockDescription.text = desciption;
		if (this.timer <= 0f)
		{
			this.timer = 5f;
			this.showDescriptionCR = base.StartCoroutine(this.ShowDescription());
			return;
		}
		this.timer = 5f;
	}

	// Token: 0x06000C4C RID: 3148 RVA: 0x000376BB File Offset: 0x000358BB
	private IEnumerator ShowDescription()
	{
		this.cGrp_Description.blocksRaycasts = true;
		while (this.cGrp_Description.alpha < 1f)
		{
			this.cGrp_Description.alpha += 0.1f;
			yield return null;
		}
		while (this.timer > 0f)
		{
			this.timer -= Time.deltaTime;
			yield return null;
		}
		this.cGrp_Description.blocksRaycasts = false;
		while (this.cGrp_Description.alpha > 0f)
		{
			this.cGrp_Description.alpha -= 0.1f;
			yield return null;
		}
		yield break;
	}

	// Token: 0x04000A7B RID: 2683
	[SerializeField]
	private Text txt_UnlockName;

	// Token: 0x04000A7C RID: 2684
	[SerializeField]
	private Text txt_UnlockDescription;

	// Token: 0x04000A7D RID: 2685
	[SerializeField]
	private CanvasGroup cGrp_Description;

	// Token: 0x04000A7E RID: 2686
	[SerializeField]
	private Button btn_Click;

	// Token: 0x04000A7F RID: 2687
	private float timer;

	// Token: 0x04000A80 RID: 2688
	private const int SHOW_TIME = 5;

	// Token: 0x04000A81 RID: 2689
	private Coroutine showDescriptionCR;
}
