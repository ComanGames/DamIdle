using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000108 RID: 264
public class TutorialSpeechBubble : MonoBehaviour
{
	// Token: 0x060006F5 RID: 1781 RVA: 0x00025500 File Offset: 0x00023700
	public void WireData(ReactiveProperty<string> copy, ReactiveProperty<float> delay)
	{
		delay.Subscribe(delegate(float x)
		{
			this.canvasGroup.alpha = x / TutorialController.CLICK_DELAY;
		}).AddTo(base.gameObject);
		copy.Subscribe(delegate(string x)
		{
			this.txt_Copy.text = x;
		}).AddTo(base.gameObject);
	}

	// Token: 0x04000696 RID: 1686
	[SerializeField]
	private CanvasGroup canvasGroup;

	// Token: 0x04000697 RID: 1687
	[SerializeField]
	private Text txt_Copy;
}
