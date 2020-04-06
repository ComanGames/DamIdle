using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000CC RID: 204
public class PlanetMilestonesRootProgressBar : MonoBehaviour
{
	// Token: 0x06000583 RID: 1411 RVA: 0x0001CC54 File Offset: 0x0001AE54
	public void WireData()
	{
		this.segmentPrefab.gameObject.SetActive(false);
		GameController.Instance.PlanetThemeService.VentureViewColorData.Subscribe(delegate(VentureColors y)
		{
			this.img_bg.color = y.VentureViewColorData.ProgressFrame;
		}).AddTo(base.gameObject);
		this.planetMilestoneService = GameController.Instance.PlanetMilestoneService;
		List<UserPlanetMilestone> userMilestonesForCurrentPlanet = this.planetMilestoneService.GetUserMilestonesForCurrentPlanet();
		List<UserPlanetMilestone> userMilestonesForCurrentPlanet2 = GameController.Instance.PlanetMilestoneService.GetUserMilestonesForCurrentPlanet();
		this.MaxTarget = userMilestonesForCurrentPlanet2.Max(x => x.TargetAmount);
		for (int i = 0; i < userMilestonesForCurrentPlanet.Count; i++)
		{
			PlanetMilestonesRootProgressBarSegment planetMilestonesRootProgressBarSegment = Object.Instantiate<PlanetMilestonesRootProgressBarSegment>(this.segmentPrefab, this.milestoneProgressSegmentRoot, false);
			planetMilestonesRootProgressBarSegment.gameObject.SetActive(true);
			planetMilestonesRootProgressBarSegment.WireData(userMilestonesForCurrentPlanet[i], i + 1);
			this.segments.Add(planetMilestonesRootProgressBarSegment);
		}
		GameController.Instance.PlanetMilestoneService.CurrentNormalizedProgress.Subscribe(delegate(float percent)
		{
			this.HImg_ProgressBar.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
			this.HImg_ProgressBar.GetComponent<RectTransform>().anchorMax = new Vector2(percent, 1f);
		}).AddTo(base.gameObject);
		this.go_completeState.SetActive(false);
		(from x in GameController.Instance.PlanetMilestoneService.CurrentMilestone
		where x == null
		select x).Subscribe(delegate(UserPlanetMilestone _)
		{
			this.go_completeState.SetActive(true);
			this.milestoneProgressSegmentRoot.gameObject.SetActive(false);
		}).AddTo(base.gameObject);
	}

	// Token: 0x040004EC RID: 1260
	[SerializeField]
	private Image img_bg;

	// Token: 0x040004ED RID: 1261
	[SerializeField]
	private Text txt_title;

	// Token: 0x040004EE RID: 1262
	[SerializeField]
	private Transform milestoneProgressSegmentRoot;

	// Token: 0x040004EF RID: 1263
	[SerializeField]
	private PlanetMilestonesRootProgressBarSegment segmentPrefab;

	// Token: 0x040004F0 RID: 1264
	[SerializeField]
	private Image HImg_ProgressBar;

	// Token: 0x040004F1 RID: 1265
	[SerializeField]
	private GameObject go_completeState;

	// Token: 0x040004F2 RID: 1266
	private PlanetMilestoneService planetMilestoneService;

	// Token: 0x040004F3 RID: 1267
	private List<PlanetMilestonesRootProgressBarSegment> segments = new List<PlanetMilestonesRootProgressBarSegment>();

	// Token: 0x040004F4 RID: 1268
	private double MaxTarget;
}
