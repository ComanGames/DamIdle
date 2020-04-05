using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000CD RID: 205
public class PlanetMilestonesRootProgressBarSegment : MonoBehaviour
{
	// Token: 0x06000588 RID: 1416 RVA: 0x0001CE50 File Offset: 0x0001B050
	public void WireData(UserPlanetMilestone milestone, int label)
	{
		this.milestone = milestone;
		this.img_icon.gameObject.SetActive(false);
		this.txt_label.text = string.Concat(label);
		this.milestone.State.Subscribe(delegate(UserPlanetMilestone.PlanetMilestoneState x)
		{
			this.img_icon.gameObject.SetActive(x == UserPlanetMilestone.PlanetMilestoneState.CLAIMED);
			this.txt_label.gameObject.SetActive(x != UserPlanetMilestone.PlanetMilestoneState.CLAIMED);
			this.img_breadcrumb.gameObject.SetActive(x == UserPlanetMilestone.PlanetMilestoneState.COMPLETE);
		}).AddTo(base.gameObject);
	}

	// Token: 0x040004F5 RID: 1269
	[SerializeField]
	private Text txt_label;

	// Token: 0x040004F6 RID: 1270
	[SerializeField]
	private Image img_icon;

	// Token: 0x040004F7 RID: 1271
	[SerializeField]
	private Image img_breadcrumb;

	// Token: 0x040004F8 RID: 1272
	private UserPlanetMilestone milestone;
}
