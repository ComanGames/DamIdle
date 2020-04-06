using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200005B RID: 91
public class EventTimedMissionsListItem : BaseMissionListItem
{
	// Token: 0x06000326 RID: 806 RVA: 0x00012F08 File Offset: 0x00011108
	public override void Init(UserEventMission mission, Action<Transform, UserEventMission> onClaimClickedCallback)
	{
		base.Init(mission, onClaimClickedCallback);
		mission.TimeRemaining.Subscribe(delegate(double x)
		{
			if (x <= 0.0)
			{
				this.txt_timeUntilRefresh.text = 0.0.ToCountdownTrim();
				this.txt_timeUntilRefresh.color = Color.white;
				return;
			}
			this.txt_timeUntilRefresh.text = x.ToCountdownTrim();
			this.txt_timeUntilRefresh.color = Color.white;
		}).AddTo(this.disposables);
	}

	// Token: 0x06000327 RID: 807 RVA: 0x00012F38 File Offset: 0x00011138
	protected override void UpdateState()
	{
		base.UpdateState();
		switch (this.mission.State.Value)
		{
		case EventMissionState.ACTIVE:
			this.txt_expires_in.color = new Color(0.117647059f, 0.117647059f, 0.117647059f, 1f);
			this.txt_expires_in.text = "EXPIRES IN";
			return;
		case EventMissionState.COMPLETE:
			this.txt_expires_in.color = new Color(0.117647059f, 0.117647059f, 0.117647059f, 1f);
			this.txt_expires_in.text = "";
			this.txt_timeUntilRefresh.text = "COMPLETE";
			return;
		case EventMissionState.CLAIMED:
			this.txt_expires_in.color = new Color(0.117647059f, 0.117647059f, 0.117647059f, 1f);
			return;
		case EventMissionState.EXPIRED:
			this.txt_expires_in.color = new Color(0.4745098f, 0.113725491f, 0.07450981f, 1f);
			this.txt_expires_in.text = "EXPIRED";
			return;
		default:
			return;
		}
	}

	// Token: 0x040002AE RID: 686
	[SerializeField]
	private Text txt_timeUntilRefresh;

	// Token: 0x040002AF RID: 687
	[SerializeField]
	private Text txt_expires_in;
}
