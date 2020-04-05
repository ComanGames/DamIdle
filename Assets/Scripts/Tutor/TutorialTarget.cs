using System;
using UnityEngine;

// Token: 0x02000109 RID: 265
public class TutorialTarget : MonoBehaviour
{
	// Token: 0x060006F9 RID: 1785 RVA: 0x00025560 File Offset: 0x00023760
	private void Start()
	{
		if (!this.registered && !string.IsNullOrEmpty(this.TutorialTargetId))
		{
			this.registered = true;
			GameController.Instance.TutorialService.RegisterTarget(this.TutorialTargetId, base.gameObject);
		}
	}

	// Token: 0x060006FA RID: 1786 RVA: 0x00025560 File Offset: 0x00023760
	private void OnEnable()
	{
		if (!this.registered && !string.IsNullOrEmpty(this.TutorialTargetId))
		{
			this.registered = true;
			GameController.Instance.TutorialService.RegisterTarget(this.TutorialTargetId, base.gameObject);
		}
	}

	// Token: 0x060006FB RID: 1787 RVA: 0x00025599 File Offset: 0x00023799
	private void OnDisable()
	{
		if (this.registered)
		{
			GameController.Instance.TutorialService.UnregisterTarget(this.TutorialTargetId, base.gameObject);
			this.registered = false;
		}
	}

	// Token: 0x060006FC RID: 1788 RVA: 0x00025599 File Offset: 0x00023799
	private void OnDestroy()
	{
		if (this.registered)
		{
			GameController.Instance.TutorialService.UnregisterTarget(this.TutorialTargetId, base.gameObject);
			this.registered = false;
		}
	}

	// Token: 0x04000698 RID: 1688
	public string TutorialTargetId;

	// Token: 0x04000699 RID: 1689
	private bool registered;
}
