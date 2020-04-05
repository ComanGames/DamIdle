using System;
using AdCap;
using UniRx;
using UnityEngine;

// Token: 0x0200009C RID: 156
public class FTUE_Selector : MonoBehaviour
{
	// Token: 0x06000449 RID: 1097 RVA: 0x00017431 File Offset: 0x00015631
	private void Awake()
	{
		MessageBroker.Default.Receive<WelcomeBackSequenceCompleted>().First<WelcomeBackSequenceCompleted>().Subscribe(delegate(WelcomeBackSequenceCompleted _)
		{
			if (FeatureConfig.IsFlagSet("NewFTUE"))
			{
				this.NewFTUE.gameObject.SetActive(true);
				this.NewFTUE.Init();
				return;
			}
			this.NewFTUE.gameObject.SetActive(false);
			this.TutorialController.Init();
		}).AddTo(base.gameObject);
	}

	// Token: 0x040003C9 RID: 969
	[SerializeField]
	private FTUE_Manager NewFTUE;

	// Token: 0x040003CA RID: 970
	[SerializeField]
	private TutorialController TutorialController;
}
