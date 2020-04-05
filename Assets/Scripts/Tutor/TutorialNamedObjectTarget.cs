using System;
using UnityEngine;

// Token: 0x02000102 RID: 258
public class TutorialNamedObjectTarget : MonoBehaviour
{
	// Token: 0x060006D1 RID: 1745 RVA: 0x000244FC File Offset: 0x000226FC
	private void Start()
	{
		if (!this.registered && this.targetObj.name == this.requiredName)
		{
			this.registered = true;
			GameController.Instance.TutorialService.RegisterTarget(this.TutorialTargetId, base.gameObject);
		}
	}

	// Token: 0x060006D2 RID: 1746 RVA: 0x0002454C File Offset: 0x0002274C
	private void OnEnable()
	{
		if (!this.registered && this.targetObj.name == this.requiredName)
		{
			this.registered = true;
			GameController.Instance.TutorialService.RegisterTarget(this.TutorialTargetId, base.gameObject);
		}
	}

	// Token: 0x060006D3 RID: 1747 RVA: 0x0002459C File Offset: 0x0002279C
	private void OnDestroy()
	{
		if (this.registered && this.targetObj.name == this.requiredName)
		{
			GameController.Instance.TutorialService.UnregisterTarget(this.TutorialTargetId, base.gameObject);
			this.registered = false;
		}
	}

	// Token: 0x060006D4 RID: 1748 RVA: 0x000245EC File Offset: 0x000227EC
	private void OnDisable()
	{
		if (this.registered && this.targetObj.name == this.requiredName)
		{
			GameController.Instance.TutorialService.UnregisterTarget(this.TutorialTargetId, base.gameObject);
			this.registered = false;
		}
	}

	// Token: 0x04000656 RID: 1622
	public GameObject targetObj;

	// Token: 0x04000657 RID: 1623
	public string requiredName;

	// Token: 0x04000658 RID: 1624
	public string TutorialTargetId;

	// Token: 0x04000659 RID: 1625
	private bool registered;
}
