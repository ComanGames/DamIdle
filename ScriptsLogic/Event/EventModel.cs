using System;
using UniRx;
using UnityEngine;

// Token: 0x02000080 RID: 128
public class EventModel
{
	// Token: 0x1700004F RID: 79
	// (get) Token: 0x060003AA RID: 938 RVA: 0x0001443C File Offset: 0x0001263C
	public string Id
	{
		get
		{
			return this.rawData.id;
		}
	}

	// Token: 0x17000050 RID: 80
	// (get) Token: 0x060003AB RID: 939 RVA: 0x00014449 File Offset: 0x00012649
	public string PlanetTheme
	{
		get
		{
			return this.rawData.planetTheme;
		}
	}

	// Token: 0x17000051 RID: 81
	// (get) Token: 0x060003AC RID: 940 RVA: 0x00014456 File Offset: 0x00012656
	public DateTime StartDate
	{
		get
		{
			return this.rawData.startDate;
		}
	}

	// Token: 0x17000052 RID: 82
	// (get) Token: 0x060003AD RID: 941 RVA: 0x00014463 File Offset: 0x00012663
	public DateTime EndDate
	{
		get
		{
			return this.rawData.endDate;
		}
	}

	// Token: 0x17000053 RID: 83
	// (get) Token: 0x060003AE RID: 942 RVA: 0x00014470 File Offset: 0x00012670
	public string Name
	{
		get
		{
			return this.rawData.name;
		}
	}

	// Token: 0x17000054 RID: 84
	// (get) Token: 0x060003AF RID: 943 RVA: 0x0001447D File Offset: 0x0001267D
	public Color TintColor
	{
		get
		{
			return this.rawData.tintColor;
		}
	}

	// Token: 0x17000055 RID: 85
	// (get) Token: 0x060003B0 RID: 944 RVA: 0x0001448A File Offset: 0x0001268A
	public string PromoTitle
	{
		get
		{
			return this.rawData.promoTitle;
		}
	}

	// Token: 0x17000056 RID: 86
	// (get) Token: 0x060003B1 RID: 945 RVA: 0x00014497 File Offset: 0x00012697
	public string PromoBody
	{
		get
		{
			return this.rawData.promoBody;
		}
	}

	// Token: 0x17000057 RID: 87
	// (get) Token: 0x060003B2 RID: 946 RVA: 0x000144A4 File Offset: 0x000126A4
	public int UnlockCount
	{
		get
		{
			return this.rawData.unlockCount;
		}
	}

	// Token: 0x17000058 RID: 88
	// (get) Token: 0x060003B3 RID: 947 RVA: 0x000144B1 File Offset: 0x000126B1
	public bool HasLeaderboard
	{
		get
		{
			return this.rawData.hasLeaderboard;
		}
	}

	// Token: 0x17000059 RID: 89
	// (get) Token: 0x060003B4 RID: 948 RVA: 0x000144BE File Offset: 0x000126BE
	public LeaderboardType LeaderboardType
	{
		get
		{
			return this.rawData.leaderboardType;
		}
	}

	// Token: 0x1700005A RID: 90
	// (get) Token: 0x060003B5 RID: 949 RVA: 0x000144CB File Offset: 0x000126CB
	public PlanetProgressionType ProgressionType
	{
		get
		{
			return this.rawData.progressionType;
		}
	}

	// Token: 0x1700005B RID: 91
	// (get) Token: 0x060003B6 RID: 950 RVA: 0x000144D8 File Offset: 0x000126D8
	public string FeatureTutorialText
	{
		get
		{
			return this.rawData.featureTutorialText;
		}
	}

	// Token: 0x060003B7 RID: 951 RVA: 0x000144E5 File Offset: 0x000126E5
	public EventModel(EventData data)
	{
		this.rawData = data;
	}

	// Token: 0x0400033B RID: 827
	private EventData rawData;

	// Token: 0x0400033C RID: 828
	public readonly ReactiveProperty<double> TimeRemaining = new ReactiveProperty<double>();

	// Token: 0x0400033D RID: 829
	public readonly ReactiveProperty<EventState> State = new ReactiveProperty<EventState>();

	// Token: 0x0400033E RID: 830
	public readonly ReactiveProperty<bool> ShouldShowEventPromo = new ReactiveProperty<bool>(false);
}
