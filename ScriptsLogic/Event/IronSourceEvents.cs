using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IronSourceJSON;
using UnityEngine;

// Token: 0x02000133 RID: 307
public class IronSourceEvents : MonoBehaviour
{
	// Token: 0x06000837 RID: 2103 RVA: 0x00029B26 File Offset: 0x00027D26
	private void Awake()
	{
		base.gameObject.name = "IronSourceEvents";
		Object.DontDestroyOnLoad(base.gameObject);
	}

	// Token: 0x14000002 RID: 2
	// (add) Token: 0x06000838 RID: 2104 RVA: 0x00029B44 File Offset: 0x00027D44
	// (remove) Token: 0x06000839 RID: 2105 RVA: 0x00029B78 File Offset: 0x00027D78
	private static event Action<IronSourceError> _onRewardedVideoAdShowFailedEvent;

	// Token: 0x14000003 RID: 3
	// (add) Token: 0x0600083A RID: 2106 RVA: 0x00029BAB File Offset: 0x00027DAB
	// (remove) Token: 0x0600083B RID: 2107 RVA: 0x00029BCC File Offset: 0x00027DCC
	public static event Action<IronSourceError> onRewardedVideoAdShowFailedEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdShowFailedEvent == null || !IronSourceEvents._onRewardedVideoAdShowFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdShowFailedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdShowFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdShowFailedEvent -= value;
			}
		}
	}

	// Token: 0x0600083C RID: 2108 RVA: 0x00029BE8 File Offset: 0x00027DE8
	public void onRewardedVideoAdShowFailed(string description)
	{
		if (IronSourceEvents._onRewardedVideoAdShowFailedEvent != null)
		{
			IronSourceError errorFromErrorObject = this.getErrorFromErrorObject(description);
			IronSourceEvents._onRewardedVideoAdShowFailedEvent(errorFromErrorObject);
		}
	}

	// Token: 0x14000004 RID: 4
	// (add) Token: 0x0600083D RID: 2109 RVA: 0x00029C10 File Offset: 0x00027E10
	// (remove) Token: 0x0600083E RID: 2110 RVA: 0x00029C44 File Offset: 0x00027E44
	private static event Action _onRewardedVideoAdOpenedEvent;

	// Token: 0x14000005 RID: 5
	// (add) Token: 0x0600083F RID: 2111 RVA: 0x00029C77 File Offset: 0x00027E77
	// (remove) Token: 0x06000840 RID: 2112 RVA: 0x00029C98 File Offset: 0x00027E98
	public static event Action onRewardedVideoAdOpenedEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdOpenedEvent == null || !IronSourceEvents._onRewardedVideoAdOpenedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdOpenedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdOpenedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdOpenedEvent -= value;
			}
		}
	}

	// Token: 0x06000841 RID: 2113 RVA: 0x00029CB2 File Offset: 0x00027EB2
	public void onRewardedVideoAdOpened(string empty)
	{
		if (IronSourceEvents._onRewardedVideoAdOpenedEvent != null)
		{
			IronSourceEvents._onRewardedVideoAdOpenedEvent();
		}
	}

	// Token: 0x14000006 RID: 6
	// (add) Token: 0x06000842 RID: 2114 RVA: 0x00029CC8 File Offset: 0x00027EC8
	// (remove) Token: 0x06000843 RID: 2115 RVA: 0x00029CFC File Offset: 0x00027EFC
	private static event Action _onRewardedVideoAdClosedEvent;

	// Token: 0x14000007 RID: 7
	// (add) Token: 0x06000844 RID: 2116 RVA: 0x00029D2F File Offset: 0x00027F2F
	// (remove) Token: 0x06000845 RID: 2117 RVA: 0x00029D50 File Offset: 0x00027F50
	public static event Action onRewardedVideoAdClosedEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdClosedEvent == null || !IronSourceEvents._onRewardedVideoAdClosedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdClosedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdClosedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdClosedEvent -= value;
			}
		}
	}

	// Token: 0x06000846 RID: 2118 RVA: 0x00029D6A File Offset: 0x00027F6A
	public void onRewardedVideoAdClosed(string empty)
	{
		if (IronSourceEvents._onRewardedVideoAdClosedEvent != null)
		{
			IronSourceEvents._onRewardedVideoAdClosedEvent();
		}
	}

	// Token: 0x14000008 RID: 8
	// (add) Token: 0x06000847 RID: 2119 RVA: 0x00029D80 File Offset: 0x00027F80
	// (remove) Token: 0x06000848 RID: 2120 RVA: 0x00029DB4 File Offset: 0x00027FB4
	private static event Action _onRewardedVideoAdStartedEvent;

	// Token: 0x14000009 RID: 9
	// (add) Token: 0x06000849 RID: 2121 RVA: 0x00029DE7 File Offset: 0x00027FE7
	// (remove) Token: 0x0600084A RID: 2122 RVA: 0x00029E08 File Offset: 0x00028008
	public static event Action onRewardedVideoAdStartedEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdStartedEvent == null || !IronSourceEvents._onRewardedVideoAdStartedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdStartedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdStartedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdStartedEvent -= value;
			}
		}
	}

	// Token: 0x0600084B RID: 2123 RVA: 0x00029E22 File Offset: 0x00028022
	public void onRewardedVideoAdStarted(string empty)
	{
		if (IronSourceEvents._onRewardedVideoAdStartedEvent != null)
		{
			IronSourceEvents._onRewardedVideoAdStartedEvent();
		}
	}

	// Token: 0x1400000A RID: 10
	// (add) Token: 0x0600084C RID: 2124 RVA: 0x00029E38 File Offset: 0x00028038
	// (remove) Token: 0x0600084D RID: 2125 RVA: 0x00029E6C File Offset: 0x0002806C
	private static event Action _onRewardedVideoAdEndedEvent;

	// Token: 0x1400000B RID: 11
	// (add) Token: 0x0600084E RID: 2126 RVA: 0x00029E9F File Offset: 0x0002809F
	// (remove) Token: 0x0600084F RID: 2127 RVA: 0x00029EC0 File Offset: 0x000280C0
	public static event Action onRewardedVideoAdEndedEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdEndedEvent == null || !IronSourceEvents._onRewardedVideoAdEndedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdEndedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdEndedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdEndedEvent -= value;
			}
		}
	}

	// Token: 0x06000850 RID: 2128 RVA: 0x00029EDA File Offset: 0x000280DA
	public void onRewardedVideoAdEnded(string empty)
	{
		if (IronSourceEvents._onRewardedVideoAdEndedEvent != null)
		{
			IronSourceEvents._onRewardedVideoAdEndedEvent();
		}
	}

	// Token: 0x1400000C RID: 12
	// (add) Token: 0x06000851 RID: 2129 RVA: 0x00029EF0 File Offset: 0x000280F0
	// (remove) Token: 0x06000852 RID: 2130 RVA: 0x00029F24 File Offset: 0x00028124
	private static event Action<IronSourcePlacement> _onRewardedVideoAdRewardedEvent;

	// Token: 0x1400000D RID: 13
	// (add) Token: 0x06000853 RID: 2131 RVA: 0x00029F57 File Offset: 0x00028157
	// (remove) Token: 0x06000854 RID: 2132 RVA: 0x00029F78 File Offset: 0x00028178
	public static event Action<IronSourcePlacement> onRewardedVideoAdRewardedEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdRewardedEvent == null || !IronSourceEvents._onRewardedVideoAdRewardedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdRewardedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdRewardedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdRewardedEvent -= value;
			}
		}
	}

	// Token: 0x06000855 RID: 2133 RVA: 0x00029F94 File Offset: 0x00028194
	public void onRewardedVideoAdRewarded(string description)
	{
		if (IronSourceEvents._onRewardedVideoAdRewardedEvent != null)
		{
			IronSourcePlacement placementFromObject = this.getPlacementFromObject(description);
			IronSourceEvents._onRewardedVideoAdRewardedEvent(placementFromObject);
		}
	}

	// Token: 0x1400000E RID: 14
	// (add) Token: 0x06000856 RID: 2134 RVA: 0x00029FBC File Offset: 0x000281BC
	// (remove) Token: 0x06000857 RID: 2135 RVA: 0x00029FF0 File Offset: 0x000281F0
	private static event Action<IronSourcePlacement> _onRewardedVideoAdClickedEvent;

	// Token: 0x1400000F RID: 15
	// (add) Token: 0x06000858 RID: 2136 RVA: 0x0002A023 File Offset: 0x00028223
	// (remove) Token: 0x06000859 RID: 2137 RVA: 0x0002A044 File Offset: 0x00028244
	public static event Action<IronSourcePlacement> onRewardedVideoAdClickedEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdClickedEvent == null || !IronSourceEvents._onRewardedVideoAdClickedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdClickedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdClickedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdClickedEvent -= value;
			}
		}
	}

	// Token: 0x0600085A RID: 2138 RVA: 0x0002A060 File Offset: 0x00028260
	public void onRewardedVideoAdClicked(string description)
	{
		if (IronSourceEvents._onRewardedVideoAdClickedEvent != null)
		{
			IronSourcePlacement placementFromObject = this.getPlacementFromObject(description);
			IronSourceEvents._onRewardedVideoAdClickedEvent(placementFromObject);
		}
	}

	// Token: 0x14000010 RID: 16
	// (add) Token: 0x0600085B RID: 2139 RVA: 0x0002A088 File Offset: 0x00028288
	// (remove) Token: 0x0600085C RID: 2140 RVA: 0x0002A0BC File Offset: 0x000282BC
	private static event Action<bool> _onRewardedVideoAvailabilityChangedEvent;

	// Token: 0x14000011 RID: 17
	// (add) Token: 0x0600085D RID: 2141 RVA: 0x0002A0EF File Offset: 0x000282EF
	// (remove) Token: 0x0600085E RID: 2142 RVA: 0x0002A110 File Offset: 0x00028310
	public static event Action<bool> onRewardedVideoAvailabilityChangedEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAvailabilityChangedEvent == null || !IronSourceEvents._onRewardedVideoAvailabilityChangedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAvailabilityChangedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAvailabilityChangedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAvailabilityChangedEvent -= value;
			}
		}
	}

	// Token: 0x0600085F RID: 2143 RVA: 0x0002A12C File Offset: 0x0002832C
	public void onRewardedVideoAvailabilityChanged(string stringAvailable)
	{
		bool obj = stringAvailable == "true";
		if (IronSourceEvents._onRewardedVideoAvailabilityChangedEvent != null)
		{
			IronSourceEvents._onRewardedVideoAvailabilityChangedEvent(obj);
		}
	}

	// Token: 0x14000012 RID: 18
	// (add) Token: 0x06000860 RID: 2144 RVA: 0x0002A160 File Offset: 0x00028360
	// (remove) Token: 0x06000861 RID: 2145 RVA: 0x0002A194 File Offset: 0x00028394
	private static event Action<string, bool> _onRewardedVideoAvailabilityChangedDemandOnlyEvent;

	// Token: 0x14000013 RID: 19
	// (add) Token: 0x06000862 RID: 2146 RVA: 0x0002A1C7 File Offset: 0x000283C7
	// (remove) Token: 0x06000863 RID: 2147 RVA: 0x0002A1E8 File Offset: 0x000283E8
	public static event Action<string, bool> onRewardedVideoAvailabilityChangedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAvailabilityChangedDemandOnlyEvent == null || !IronSourceEvents._onRewardedVideoAvailabilityChangedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAvailabilityChangedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAvailabilityChangedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAvailabilityChangedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x06000864 RID: 2148 RVA: 0x0002A204 File Offset: 0x00028404
	public void onRewardedVideoAvailabilityChangedDemandOnly(string args)
	{
		if (IronSourceEvents._onRewardedVideoAvailabilityChangedDemandOnlyEvent != null && !string.IsNullOrEmpty(args))
		{
			List<object> list = Json.Deserialize(args) as List<object>;
			bool arg = list[1].ToString().ToLower() == "true";
			string arg2 = list[0].ToString();
			IronSourceEvents._onRewardedVideoAvailabilityChangedDemandOnlyEvent(arg2, arg);
		}
	}

	// Token: 0x14000014 RID: 20
	// (add) Token: 0x06000865 RID: 2149 RVA: 0x0002A268 File Offset: 0x00028468
	// (remove) Token: 0x06000866 RID: 2150 RVA: 0x0002A29C File Offset: 0x0002849C
	private static event Action<string> _onRewardedVideoAdOpenedDemandOnlyEvent;

	// Token: 0x14000015 RID: 21
	// (add) Token: 0x06000867 RID: 2151 RVA: 0x0002A2CF File Offset: 0x000284CF
	// (remove) Token: 0x06000868 RID: 2152 RVA: 0x0002A2F0 File Offset: 0x000284F0
	public static event Action<string> onRewardedVideoAdOpenedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdOpenedDemandOnlyEvent == null || !IronSourceEvents._onRewardedVideoAdOpenedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdOpenedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdOpenedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdOpenedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x06000869 RID: 2153 RVA: 0x0002A30A File Offset: 0x0002850A
	public void onRewardedVideoAdOpenedDemandOnly(string instanceId)
	{
		if (IronSourceEvents._onRewardedVideoAdOpenedDemandOnlyEvent != null)
		{
			IronSourceEvents._onRewardedVideoAdOpenedDemandOnlyEvent(instanceId);
		}
	}

	// Token: 0x14000016 RID: 22
	// (add) Token: 0x0600086A RID: 2154 RVA: 0x0002A320 File Offset: 0x00028520
	// (remove) Token: 0x0600086B RID: 2155 RVA: 0x0002A354 File Offset: 0x00028554
	private static event Action<string> _onRewardedVideoAdClosedDemandOnlyEvent;

	// Token: 0x14000017 RID: 23
	// (add) Token: 0x0600086C RID: 2156 RVA: 0x0002A387 File Offset: 0x00028587
	// (remove) Token: 0x0600086D RID: 2157 RVA: 0x0002A3A8 File Offset: 0x000285A8
	public static event Action<string> onRewardedVideoAdClosedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdClosedDemandOnlyEvent == null || !IronSourceEvents._onRewardedVideoAdClosedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdClosedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdClosedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdClosedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x0600086E RID: 2158 RVA: 0x0002A3C2 File Offset: 0x000285C2
	public void onRewardedVideoAdClosedDemandOnly(string instanceId)
	{
		if (IronSourceEvents._onRewardedVideoAdClosedDemandOnlyEvent != null)
		{
			IronSourceEvents._onRewardedVideoAdClosedDemandOnlyEvent(instanceId);
		}
	}

	// Token: 0x14000018 RID: 24
	// (add) Token: 0x0600086F RID: 2159 RVA: 0x0002A3D8 File Offset: 0x000285D8
	// (remove) Token: 0x06000870 RID: 2160 RVA: 0x0002A40C File Offset: 0x0002860C
	private static event Action<string, IronSourcePlacement> _onRewardedVideoAdRewardedDemandOnlyEvent;

	// Token: 0x14000019 RID: 25
	// (add) Token: 0x06000871 RID: 2161 RVA: 0x0002A43F File Offset: 0x0002863F
	// (remove) Token: 0x06000872 RID: 2162 RVA: 0x0002A460 File Offset: 0x00028660
	public static event Action<string, IronSourcePlacement> onRewardedVideoAdRewardedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdRewardedDemandOnlyEvent == null || !IronSourceEvents._onRewardedVideoAdRewardedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdRewardedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdRewardedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdRewardedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x06000873 RID: 2163 RVA: 0x0002A47C File Offset: 0x0002867C
	public void onRewardedVideoAdRewardedDemandOnly(string args)
	{
		if (IronSourceEvents._onRewardedVideoAdRewardedDemandOnlyEvent != null && !string.IsNullOrEmpty(args))
		{
			List<object> list = Json.Deserialize(args) as List<object>;
			string arg = list[0].ToString();
			IronSourcePlacement placementFromObject = this.getPlacementFromObject(list[1]);
			IronSourceEvents._onRewardedVideoAdRewardedDemandOnlyEvent(arg, placementFromObject);
		}
	}

	// Token: 0x1400001A RID: 26
	// (add) Token: 0x06000874 RID: 2164 RVA: 0x0002A4CC File Offset: 0x000286CC
	// (remove) Token: 0x06000875 RID: 2165 RVA: 0x0002A500 File Offset: 0x00028700
	private static event Action<string, IronSourceError> _onRewardedVideoAdShowFailedDemandOnlyEvent;

	// Token: 0x1400001B RID: 27
	// (add) Token: 0x06000876 RID: 2166 RVA: 0x0002A533 File Offset: 0x00028733
	// (remove) Token: 0x06000877 RID: 2167 RVA: 0x0002A554 File Offset: 0x00028754
	public static event Action<string, IronSourceError> onRewardedVideoAdShowFailedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdShowFailedDemandOnlyEvent == null || !IronSourceEvents._onRewardedVideoAdShowFailedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdShowFailedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdShowFailedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdShowFailedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x06000878 RID: 2168 RVA: 0x0002A570 File Offset: 0x00028770
	public void onRewardedVideoAdShowFailedDemandOnly(string args)
	{
		if (IronSourceEvents._onRewardedVideoAdShowFailedDemandOnlyEvent != null && !string.IsNullOrEmpty(args))
		{
			List<object> list = Json.Deserialize(args) as List<object>;
			IronSourceError errorFromErrorObject = this.getErrorFromErrorObject(list[1]);
			string arg = list[0].ToString();
			IronSourceEvents._onRewardedVideoAdShowFailedDemandOnlyEvent(arg, errorFromErrorObject);
		}
	}

	// Token: 0x1400001C RID: 28
	// (add) Token: 0x06000879 RID: 2169 RVA: 0x0002A5C0 File Offset: 0x000287C0
	// (remove) Token: 0x0600087A RID: 2170 RVA: 0x0002A5F4 File Offset: 0x000287F4
	private static event Action<string, IronSourcePlacement> _onRewardedVideoAdClickedDemandOnlyEvent;

	// Token: 0x1400001D RID: 29
	// (add) Token: 0x0600087B RID: 2171 RVA: 0x0002A627 File Offset: 0x00028827
	// (remove) Token: 0x0600087C RID: 2172 RVA: 0x0002A648 File Offset: 0x00028848
	public static event Action<string, IronSourcePlacement> onRewardedVideoAdClickedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onRewardedVideoAdClickedDemandOnlyEvent == null || !IronSourceEvents._onRewardedVideoAdClickedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdClickedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onRewardedVideoAdClickedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onRewardedVideoAdClickedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x0600087D RID: 2173 RVA: 0x0002A664 File Offset: 0x00028864
	public void onRewardedVideoAdClickedDemandOnly(string args)
	{
		if (IronSourceEvents._onRewardedVideoAdClickedDemandOnlyEvent != null && !string.IsNullOrEmpty(args))
		{
			List<object> list = Json.Deserialize(args) as List<object>;
			string arg = list[0].ToString();
			IronSourcePlacement placementFromObject = this.getPlacementFromObject(list[1]);
			IronSourceEvents._onRewardedVideoAdClickedDemandOnlyEvent(arg, placementFromObject);
		}
	}

	// Token: 0x1400001E RID: 30
	// (add) Token: 0x0600087E RID: 2174 RVA: 0x0002A6B4 File Offset: 0x000288B4
	// (remove) Token: 0x0600087F RID: 2175 RVA: 0x0002A6E8 File Offset: 0x000288E8
	private static event Action _onInterstitialAdReadyEvent;

	// Token: 0x1400001F RID: 31
	// (add) Token: 0x06000880 RID: 2176 RVA: 0x0002A71B File Offset: 0x0002891B
	// (remove) Token: 0x06000881 RID: 2177 RVA: 0x0002A73C File Offset: 0x0002893C
	public static event Action onInterstitialAdReadyEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdReadyEvent == null || !IronSourceEvents._onInterstitialAdReadyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdReadyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdReadyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdReadyEvent -= value;
			}
		}
	}

	// Token: 0x06000882 RID: 2178 RVA: 0x0002A756 File Offset: 0x00028956
	public void onInterstitialAdReady()
	{
		if (IronSourceEvents._onInterstitialAdReadyEvent != null)
		{
			IronSourceEvents._onInterstitialAdReadyEvent();
		}
	}

	// Token: 0x14000020 RID: 32
	// (add) Token: 0x06000883 RID: 2179 RVA: 0x0002A76C File Offset: 0x0002896C
	// (remove) Token: 0x06000884 RID: 2180 RVA: 0x0002A7A0 File Offset: 0x000289A0
	private static event Action<IronSourceError> _onInterstitialAdLoadFailedEvent;

	// Token: 0x14000021 RID: 33
	// (add) Token: 0x06000885 RID: 2181 RVA: 0x0002A7D3 File Offset: 0x000289D3
	// (remove) Token: 0x06000886 RID: 2182 RVA: 0x0002A7F4 File Offset: 0x000289F4
	public static event Action<IronSourceError> onInterstitialAdLoadFailedEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdLoadFailedEvent == null || !IronSourceEvents._onInterstitialAdLoadFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdLoadFailedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdLoadFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdLoadFailedEvent -= value;
			}
		}
	}

	// Token: 0x06000887 RID: 2183 RVA: 0x0002A810 File Offset: 0x00028A10
	public void onInterstitialAdLoadFailed(string description)
	{
		if (IronSourceEvents._onInterstitialAdLoadFailedEvent != null)
		{
			IronSourceError errorFromErrorObject = this.getErrorFromErrorObject(description);
			IronSourceEvents._onInterstitialAdLoadFailedEvent(errorFromErrorObject);
		}
	}

	// Token: 0x14000022 RID: 34
	// (add) Token: 0x06000888 RID: 2184 RVA: 0x0002A838 File Offset: 0x00028A38
	// (remove) Token: 0x06000889 RID: 2185 RVA: 0x0002A86C File Offset: 0x00028A6C
	private static event Action _onInterstitialAdOpenedEvent;

	// Token: 0x14000023 RID: 35
	// (add) Token: 0x0600088A RID: 2186 RVA: 0x0002A89F File Offset: 0x00028A9F
	// (remove) Token: 0x0600088B RID: 2187 RVA: 0x0002A8C0 File Offset: 0x00028AC0
	public static event Action onInterstitialAdOpenedEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdOpenedEvent == null || !IronSourceEvents._onInterstitialAdOpenedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdOpenedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdOpenedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdOpenedEvent -= value;
			}
		}
	}

	// Token: 0x0600088C RID: 2188 RVA: 0x0002A8DA File Offset: 0x00028ADA
	public void onInterstitialAdOpened(string empty)
	{
		if (IronSourceEvents._onInterstitialAdOpenedEvent != null)
		{
			IronSourceEvents._onInterstitialAdOpenedEvent();
		}
	}

	// Token: 0x14000024 RID: 36
	// (add) Token: 0x0600088D RID: 2189 RVA: 0x0002A8F0 File Offset: 0x00028AF0
	// (remove) Token: 0x0600088E RID: 2190 RVA: 0x0002A924 File Offset: 0x00028B24
	private static event Action _onInterstitialAdClosedEvent;

	// Token: 0x14000025 RID: 37
	// (add) Token: 0x0600088F RID: 2191 RVA: 0x0002A957 File Offset: 0x00028B57
	// (remove) Token: 0x06000890 RID: 2192 RVA: 0x0002A978 File Offset: 0x00028B78
	public static event Action onInterstitialAdClosedEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdClosedEvent == null || !IronSourceEvents._onInterstitialAdClosedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdClosedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdClosedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdClosedEvent -= value;
			}
		}
	}

	// Token: 0x06000891 RID: 2193 RVA: 0x0002A992 File Offset: 0x00028B92
	public void onInterstitialAdClosed(string empty)
	{
		if (IronSourceEvents._onInterstitialAdClosedEvent != null)
		{
			IronSourceEvents._onInterstitialAdClosedEvent();
		}
	}

	// Token: 0x14000026 RID: 38
	// (add) Token: 0x06000892 RID: 2194 RVA: 0x0002A9A8 File Offset: 0x00028BA8
	// (remove) Token: 0x06000893 RID: 2195 RVA: 0x0002A9DC File Offset: 0x00028BDC
	private static event Action _onInterstitialAdShowSucceededEvent;

	// Token: 0x14000027 RID: 39
	// (add) Token: 0x06000894 RID: 2196 RVA: 0x0002AA0F File Offset: 0x00028C0F
	// (remove) Token: 0x06000895 RID: 2197 RVA: 0x0002AA30 File Offset: 0x00028C30
	public static event Action onInterstitialAdShowSucceededEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdShowSucceededEvent == null || !IronSourceEvents._onInterstitialAdShowSucceededEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdShowSucceededEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdShowSucceededEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdShowSucceededEvent -= value;
			}
		}
	}

	// Token: 0x06000896 RID: 2198 RVA: 0x0002AA4A File Offset: 0x00028C4A
	public void onInterstitialAdShowSucceeded(string empty)
	{
		if (IronSourceEvents._onInterstitialAdShowSucceededEvent != null)
		{
			IronSourceEvents._onInterstitialAdShowSucceededEvent();
		}
	}

	// Token: 0x14000028 RID: 40
	// (add) Token: 0x06000897 RID: 2199 RVA: 0x0002AA60 File Offset: 0x00028C60
	// (remove) Token: 0x06000898 RID: 2200 RVA: 0x0002AA94 File Offset: 0x00028C94
	private static event Action<IronSourceError> _onInterstitialAdShowFailedEvent;

	// Token: 0x14000029 RID: 41
	// (add) Token: 0x06000899 RID: 2201 RVA: 0x0002AAC7 File Offset: 0x00028CC7
	// (remove) Token: 0x0600089A RID: 2202 RVA: 0x0002AAE8 File Offset: 0x00028CE8
	public static event Action<IronSourceError> onInterstitialAdShowFailedEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdShowFailedEvent == null || !IronSourceEvents._onInterstitialAdShowFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdShowFailedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdShowFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdShowFailedEvent -= value;
			}
		}
	}

	// Token: 0x0600089B RID: 2203 RVA: 0x0002AB04 File Offset: 0x00028D04
	public void onInterstitialAdShowFailed(string description)
	{
		if (IronSourceEvents._onInterstitialAdShowFailedEvent != null)
		{
			IronSourceError errorFromErrorObject = this.getErrorFromErrorObject(description);
			IronSourceEvents._onInterstitialAdShowFailedEvent(errorFromErrorObject);
		}
	}

	// Token: 0x1400002A RID: 42
	// (add) Token: 0x0600089C RID: 2204 RVA: 0x0002AB2C File Offset: 0x00028D2C
	// (remove) Token: 0x0600089D RID: 2205 RVA: 0x0002AB60 File Offset: 0x00028D60
	private static event Action _onInterstitialAdClickedEvent;

	// Token: 0x1400002B RID: 43
	// (add) Token: 0x0600089E RID: 2206 RVA: 0x0002AB93 File Offset: 0x00028D93
	// (remove) Token: 0x0600089F RID: 2207 RVA: 0x0002ABB4 File Offset: 0x00028DB4
	public static event Action onInterstitialAdClickedEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdClickedEvent == null || !IronSourceEvents._onInterstitialAdClickedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdClickedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdClickedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdClickedEvent -= value;
			}
		}
	}

	// Token: 0x060008A0 RID: 2208 RVA: 0x0002ABCE File Offset: 0x00028DCE
	public void onInterstitialAdClicked(string empty)
	{
		if (IronSourceEvents._onInterstitialAdClickedEvent != null)
		{
			IronSourceEvents._onInterstitialAdClickedEvent();
		}
	}

	// Token: 0x1400002C RID: 44
	// (add) Token: 0x060008A1 RID: 2209 RVA: 0x0002ABE4 File Offset: 0x00028DE4
	// (remove) Token: 0x060008A2 RID: 2210 RVA: 0x0002AC18 File Offset: 0x00028E18
	private static event Action<string> _onInterstitialAdReadyDemandOnlyEvent;

	// Token: 0x1400002D RID: 45
	// (add) Token: 0x060008A3 RID: 2211 RVA: 0x0002AC4B File Offset: 0x00028E4B
	// (remove) Token: 0x060008A4 RID: 2212 RVA: 0x0002AC6C File Offset: 0x00028E6C
	public static event Action<string> onInterstitialAdReadyDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdReadyDemandOnlyEvent == null || !IronSourceEvents._onInterstitialAdReadyDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdReadyDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdReadyDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdReadyDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x060008A5 RID: 2213 RVA: 0x0002AC86 File Offset: 0x00028E86
	public void onInterstitialAdReadyDemandOnly(string instanceId)
	{
		if (IronSourceEvents._onInterstitialAdReadyDemandOnlyEvent != null)
		{
			IronSourceEvents._onInterstitialAdReadyDemandOnlyEvent(instanceId);
		}
	}

	// Token: 0x1400002E RID: 46
	// (add) Token: 0x060008A6 RID: 2214 RVA: 0x0002AC9C File Offset: 0x00028E9C
	// (remove) Token: 0x060008A7 RID: 2215 RVA: 0x0002ACD0 File Offset: 0x00028ED0
	private static event Action<string, IronSourceError> _onInterstitialAdLoadFailedDemandOnlyEvent;

	// Token: 0x1400002F RID: 47
	// (add) Token: 0x060008A8 RID: 2216 RVA: 0x0002AD03 File Offset: 0x00028F03
	// (remove) Token: 0x060008A9 RID: 2217 RVA: 0x0002AD24 File Offset: 0x00028F24
	public static event Action<string, IronSourceError> onInterstitialAdLoadFailedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdLoadFailedDemandOnlyEvent == null || !IronSourceEvents._onInterstitialAdLoadFailedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdLoadFailedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdLoadFailedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdLoadFailedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x060008AA RID: 2218 RVA: 0x0002AD40 File Offset: 0x00028F40
	public void onInterstitialAdLoadFailedDemandOnly(string args)
	{
		if (IronSourceEvents._onInterstitialAdLoadFailedDemandOnlyEvent != null && !string.IsNullOrEmpty(args))
		{
			List<object> list = Json.Deserialize(args) as List<object>;
			IronSourceError errorFromErrorObject = this.getErrorFromErrorObject(list[1]);
			string arg = list[0].ToString();
			IronSourceEvents._onInterstitialAdLoadFailedDemandOnlyEvent(arg, errorFromErrorObject);
		}
	}

	// Token: 0x14000030 RID: 48
	// (add) Token: 0x060008AB RID: 2219 RVA: 0x0002AD90 File Offset: 0x00028F90
	// (remove) Token: 0x060008AC RID: 2220 RVA: 0x0002ADC4 File Offset: 0x00028FC4
	private static event Action<string> _onInterstitialAdOpenedDemandOnlyEvent;

	// Token: 0x14000031 RID: 49
	// (add) Token: 0x060008AD RID: 2221 RVA: 0x0002ADF7 File Offset: 0x00028FF7
	// (remove) Token: 0x060008AE RID: 2222 RVA: 0x0002AE18 File Offset: 0x00029018
	public static event Action<string> onInterstitialAdOpenedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdOpenedDemandOnlyEvent == null || !IronSourceEvents._onInterstitialAdOpenedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdOpenedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdOpenedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdOpenedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x060008AF RID: 2223 RVA: 0x0002AE32 File Offset: 0x00029032
	public void onInterstitialAdOpenedDemandOnly(string instanceId)
	{
		if (IronSourceEvents._onInterstitialAdOpenedDemandOnlyEvent != null)
		{
			IronSourceEvents._onInterstitialAdOpenedDemandOnlyEvent(instanceId);
		}
	}

	// Token: 0x14000032 RID: 50
	// (add) Token: 0x060008B0 RID: 2224 RVA: 0x0002AE48 File Offset: 0x00029048
	// (remove) Token: 0x060008B1 RID: 2225 RVA: 0x0002AE7C File Offset: 0x0002907C
	private static event Action<string> _onInterstitialAdClosedDemandOnlyEvent;

	// Token: 0x14000033 RID: 51
	// (add) Token: 0x060008B2 RID: 2226 RVA: 0x0002AEAF File Offset: 0x000290AF
	// (remove) Token: 0x060008B3 RID: 2227 RVA: 0x0002AED0 File Offset: 0x000290D0
	public static event Action<string> onInterstitialAdClosedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdClosedDemandOnlyEvent == null || !IronSourceEvents._onInterstitialAdClosedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdClosedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdClosedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdClosedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x060008B4 RID: 2228 RVA: 0x0002AEEA File Offset: 0x000290EA
	public void onInterstitialAdClosedDemandOnly(string instanceId)
	{
		if (IronSourceEvents._onInterstitialAdClosedDemandOnlyEvent != null)
		{
			IronSourceEvents._onInterstitialAdClosedDemandOnlyEvent(instanceId);
		}
	}

	// Token: 0x14000034 RID: 52
	// (add) Token: 0x060008B5 RID: 2229 RVA: 0x0002AF00 File Offset: 0x00029100
	// (remove) Token: 0x060008B6 RID: 2230 RVA: 0x0002AF34 File Offset: 0x00029134
	private static event Action<string> _onInterstitialAdShowSucceededDemandOnlyEvent;

	// Token: 0x14000035 RID: 53
	// (add) Token: 0x060008B7 RID: 2231 RVA: 0x0002AF67 File Offset: 0x00029167
	// (remove) Token: 0x060008B8 RID: 2232 RVA: 0x0002AF88 File Offset: 0x00029188
	public static event Action<string> onInterstitialAdShowSucceededDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdShowSucceededDemandOnlyEvent == null || !IronSourceEvents._onInterstitialAdShowSucceededDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdShowSucceededDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdShowSucceededDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdShowSucceededDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x060008B9 RID: 2233 RVA: 0x0002AFA2 File Offset: 0x000291A2
	public void onInterstitialAdShowSucceededDemandOnly(string instanceId)
	{
		if (IronSourceEvents._onInterstitialAdShowSucceededDemandOnlyEvent != null)
		{
			IronSourceEvents._onInterstitialAdShowSucceededDemandOnlyEvent(instanceId);
		}
	}

	// Token: 0x14000036 RID: 54
	// (add) Token: 0x060008BA RID: 2234 RVA: 0x0002AFB8 File Offset: 0x000291B8
	// (remove) Token: 0x060008BB RID: 2235 RVA: 0x0002AFEC File Offset: 0x000291EC
	private static event Action<string, IronSourceError> _onInterstitialAdShowFailedDemandOnlyEvent;

	// Token: 0x14000037 RID: 55
	// (add) Token: 0x060008BC RID: 2236 RVA: 0x0002B01F File Offset: 0x0002921F
	// (remove) Token: 0x060008BD RID: 2237 RVA: 0x0002B040 File Offset: 0x00029240
	public static event Action<string, IronSourceError> onInterstitialAdShowFailedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdShowFailedDemandOnlyEvent == null || !IronSourceEvents._onInterstitialAdShowFailedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdShowFailedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdShowFailedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdShowFailedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x060008BE RID: 2238 RVA: 0x0002B05C File Offset: 0x0002925C
	public void onInterstitialAdShowFailedDemandOnly(string args)
	{
		if (IronSourceEvents._onInterstitialAdLoadFailedDemandOnlyEvent != null && !string.IsNullOrEmpty(args))
		{
			List<object> list = Json.Deserialize(args) as List<object>;
			IronSourceError errorFromErrorObject = this.getErrorFromErrorObject(list[1]);
			string arg = list[0].ToString();
			IronSourceEvents._onInterstitialAdShowFailedDemandOnlyEvent(arg, errorFromErrorObject);
		}
	}

	// Token: 0x14000038 RID: 56
	// (add) Token: 0x060008BF RID: 2239 RVA: 0x0002B0AC File Offset: 0x000292AC
	// (remove) Token: 0x060008C0 RID: 2240 RVA: 0x0002B0E0 File Offset: 0x000292E0
	private static event Action<string> _onInterstitialAdClickedDemandOnlyEvent;

	// Token: 0x14000039 RID: 57
	// (add) Token: 0x060008C1 RID: 2241 RVA: 0x0002B113 File Offset: 0x00029313
	// (remove) Token: 0x060008C2 RID: 2242 RVA: 0x0002B134 File Offset: 0x00029334
	public static event Action<string> onInterstitialAdClickedDemandOnlyEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdClickedDemandOnlyEvent == null || !IronSourceEvents._onInterstitialAdClickedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdClickedDemandOnlyEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdClickedDemandOnlyEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdClickedDemandOnlyEvent -= value;
			}
		}
	}

	// Token: 0x060008C3 RID: 2243 RVA: 0x0002B14E File Offset: 0x0002934E
	public void onInterstitialAdClickedDemandOnly(string instanceId)
	{
		if (IronSourceEvents._onInterstitialAdClickedDemandOnlyEvent != null)
		{
			IronSourceEvents._onInterstitialAdClickedDemandOnlyEvent(instanceId);
		}
	}

	// Token: 0x1400003A RID: 58
	// (add) Token: 0x060008C4 RID: 2244 RVA: 0x0002B164 File Offset: 0x00029364
	// (remove) Token: 0x060008C5 RID: 2245 RVA: 0x0002B198 File Offset: 0x00029398
	private static event Action _onInterstitialAdRewardedEvent;

	// Token: 0x1400003B RID: 59
	// (add) Token: 0x060008C6 RID: 2246 RVA: 0x0002B1CB File Offset: 0x000293CB
	// (remove) Token: 0x060008C7 RID: 2247 RVA: 0x0002B1EC File Offset: 0x000293EC
	public static event Action onInterstitialAdRewardedEvent
	{
		add
		{
			if (IronSourceEvents._onInterstitialAdRewardedEvent == null || !IronSourceEvents._onInterstitialAdRewardedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdRewardedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onInterstitialAdRewardedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onInterstitialAdRewardedEvent -= value;
			}
		}
	}

	// Token: 0x060008C8 RID: 2248 RVA: 0x0002B206 File Offset: 0x00029406
	public void onInterstitialAdRewarded(string empty)
	{
		if (IronSourceEvents._onInterstitialAdRewardedEvent != null)
		{
			IronSourceEvents._onInterstitialAdRewardedEvent();
		}
	}

	// Token: 0x1400003C RID: 60
	// (add) Token: 0x060008C9 RID: 2249 RVA: 0x0002B21C File Offset: 0x0002941C
	// (remove) Token: 0x060008CA RID: 2250 RVA: 0x0002B250 File Offset: 0x00029450
	private static event Action _onOfferwallOpenedEvent;

	// Token: 0x1400003D RID: 61
	// (add) Token: 0x060008CB RID: 2251 RVA: 0x0002B283 File Offset: 0x00029483
	// (remove) Token: 0x060008CC RID: 2252 RVA: 0x0002B2A4 File Offset: 0x000294A4
	public static event Action onOfferwallOpenedEvent
	{
		add
		{
			if (IronSourceEvents._onOfferwallOpenedEvent == null || !IronSourceEvents._onOfferwallOpenedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallOpenedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onOfferwallOpenedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallOpenedEvent -= value;
			}
		}
	}

	// Token: 0x060008CD RID: 2253 RVA: 0x0002B2BE File Offset: 0x000294BE
	public void onOfferwallOpened(string empty)
	{
		if (IronSourceEvents._onOfferwallOpenedEvent != null)
		{
			IronSourceEvents._onOfferwallOpenedEvent();
		}
	}

	// Token: 0x1400003E RID: 62
	// (add) Token: 0x060008CE RID: 2254 RVA: 0x0002B2D4 File Offset: 0x000294D4
	// (remove) Token: 0x060008CF RID: 2255 RVA: 0x0002B308 File Offset: 0x00029508
	private static event Action<IronSourceError> _onOfferwallShowFailedEvent;

	// Token: 0x1400003F RID: 63
	// (add) Token: 0x060008D0 RID: 2256 RVA: 0x0002B33B File Offset: 0x0002953B
	// (remove) Token: 0x060008D1 RID: 2257 RVA: 0x0002B35C File Offset: 0x0002955C
	public static event Action<IronSourceError> onOfferwallShowFailedEvent
	{
		add
		{
			if (IronSourceEvents._onOfferwallShowFailedEvent == null || !IronSourceEvents._onOfferwallShowFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallShowFailedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onOfferwallShowFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallShowFailedEvent -= value;
			}
		}
	}

	// Token: 0x060008D2 RID: 2258 RVA: 0x0002B378 File Offset: 0x00029578
	public void onOfferwallShowFailed(string description)
	{
		if (IronSourceEvents._onOfferwallShowFailedEvent != null)
		{
			IronSourceError errorFromErrorObject = this.getErrorFromErrorObject(description);
			IronSourceEvents._onOfferwallShowFailedEvent(errorFromErrorObject);
		}
	}

	// Token: 0x14000040 RID: 64
	// (add) Token: 0x060008D3 RID: 2259 RVA: 0x0002B3A0 File Offset: 0x000295A0
	// (remove) Token: 0x060008D4 RID: 2260 RVA: 0x0002B3D4 File Offset: 0x000295D4
	private static event Action _onOfferwallClosedEvent;

	// Token: 0x14000041 RID: 65
	// (add) Token: 0x060008D5 RID: 2261 RVA: 0x0002B407 File Offset: 0x00029607
	// (remove) Token: 0x060008D6 RID: 2262 RVA: 0x0002B428 File Offset: 0x00029628
	public static event Action onOfferwallClosedEvent
	{
		add
		{
			if (IronSourceEvents._onOfferwallClosedEvent == null || !IronSourceEvents._onOfferwallClosedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallClosedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onOfferwallClosedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallClosedEvent -= value;
			}
		}
	}

	// Token: 0x060008D7 RID: 2263 RVA: 0x0002B442 File Offset: 0x00029642
	public void onOfferwallClosed(string empty)
	{
		if (IronSourceEvents._onOfferwallClosedEvent != null)
		{
			IronSourceEvents._onOfferwallClosedEvent();
		}
	}

	// Token: 0x14000042 RID: 66
	// (add) Token: 0x060008D8 RID: 2264 RVA: 0x0002B458 File Offset: 0x00029658
	// (remove) Token: 0x060008D9 RID: 2265 RVA: 0x0002B48C File Offset: 0x0002968C
	private static event Action<IronSourceError> _onGetOfferwallCreditsFailedEvent;

	// Token: 0x14000043 RID: 67
	// (add) Token: 0x060008DA RID: 2266 RVA: 0x0002B4BF File Offset: 0x000296BF
	// (remove) Token: 0x060008DB RID: 2267 RVA: 0x0002B4E0 File Offset: 0x000296E0
	public static event Action<IronSourceError> onGetOfferwallCreditsFailedEvent
	{
		add
		{
			if (IronSourceEvents._onGetOfferwallCreditsFailedEvent == null || !IronSourceEvents._onGetOfferwallCreditsFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onGetOfferwallCreditsFailedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onGetOfferwallCreditsFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onGetOfferwallCreditsFailedEvent -= value;
			}
		}
	}

	// Token: 0x060008DC RID: 2268 RVA: 0x0002B4FC File Offset: 0x000296FC
	public void onGetOfferwallCreditsFailed(string description)
	{
		if (IronSourceEvents._onGetOfferwallCreditsFailedEvent != null)
		{
			IronSourceError errorFromErrorObject = this.getErrorFromErrorObject(description);
			IronSourceEvents._onGetOfferwallCreditsFailedEvent(errorFromErrorObject);
		}
	}

	// Token: 0x14000044 RID: 68
	// (add) Token: 0x060008DD RID: 2269 RVA: 0x0002B524 File Offset: 0x00029724
	// (remove) Token: 0x060008DE RID: 2270 RVA: 0x0002B558 File Offset: 0x00029758
	private static event Action<Dictionary<string, object>> _onOfferwallAdCreditedEvent;

	// Token: 0x14000045 RID: 69
	// (add) Token: 0x060008DF RID: 2271 RVA: 0x0002B58B File Offset: 0x0002978B
	// (remove) Token: 0x060008E0 RID: 2272 RVA: 0x0002B5AC File Offset: 0x000297AC
	public static event Action<Dictionary<string, object>> onOfferwallAdCreditedEvent
	{
		add
		{
			if (IronSourceEvents._onOfferwallAdCreditedEvent == null || !IronSourceEvents._onOfferwallAdCreditedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallAdCreditedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onOfferwallAdCreditedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallAdCreditedEvent -= value;
			}
		}
	}

	// Token: 0x060008E1 RID: 2273 RVA: 0x0002B5C6 File Offset: 0x000297C6
	public void onOfferwallAdCredited(string json)
	{
		if (IronSourceEvents._onOfferwallAdCreditedEvent != null)
		{
			IronSourceEvents._onOfferwallAdCreditedEvent(Json.Deserialize(json) as Dictionary<string, object>);
		}
	}

	// Token: 0x14000046 RID: 70
	// (add) Token: 0x060008E2 RID: 2274 RVA: 0x0002B5E4 File Offset: 0x000297E4
	// (remove) Token: 0x060008E3 RID: 2275 RVA: 0x0002B618 File Offset: 0x00029818
	private static event Action<bool> _onOfferwallAvailableEvent;

	// Token: 0x14000047 RID: 71
	// (add) Token: 0x060008E4 RID: 2276 RVA: 0x0002B64B File Offset: 0x0002984B
	// (remove) Token: 0x060008E5 RID: 2277 RVA: 0x0002B66C File Offset: 0x0002986C
	public static event Action<bool> onOfferwallAvailableEvent
	{
		add
		{
			if (IronSourceEvents._onOfferwallAvailableEvent == null || !IronSourceEvents._onOfferwallAvailableEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallAvailableEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onOfferwallAvailableEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onOfferwallAvailableEvent -= value;
			}
		}
	}

	// Token: 0x060008E6 RID: 2278 RVA: 0x0002B688 File Offset: 0x00029888
	public void onOfferwallAvailable(string stringAvailable)
	{
		bool obj = stringAvailable == "true";
		if (IronSourceEvents._onOfferwallAvailableEvent != null)
		{
			IronSourceEvents._onOfferwallAvailableEvent(obj);
		}
	}

	// Token: 0x14000048 RID: 72
	// (add) Token: 0x060008E7 RID: 2279 RVA: 0x0002B6BC File Offset: 0x000298BC
	// (remove) Token: 0x060008E8 RID: 2280 RVA: 0x0002B6F0 File Offset: 0x000298F0
	private static event Action _onBannerAdLoadedEvent;

	// Token: 0x14000049 RID: 73
	// (add) Token: 0x060008E9 RID: 2281 RVA: 0x0002B723 File Offset: 0x00029923
	// (remove) Token: 0x060008EA RID: 2282 RVA: 0x0002B744 File Offset: 0x00029944
	public static event Action onBannerAdLoadedEvent
	{
		add
		{
			if (IronSourceEvents._onBannerAdLoadedEvent == null || !IronSourceEvents._onBannerAdLoadedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdLoadedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onBannerAdLoadedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdLoadedEvent -= value;
			}
		}
	}

	// Token: 0x060008EB RID: 2283 RVA: 0x0002B75E File Offset: 0x0002995E
	public void onBannerAdLoaded()
	{
		if (IronSourceEvents._onBannerAdLoadedEvent != null)
		{
			IronSourceEvents._onBannerAdLoadedEvent();
		}
	}

	// Token: 0x1400004A RID: 74
	// (add) Token: 0x060008EC RID: 2284 RVA: 0x0002B774 File Offset: 0x00029974
	// (remove) Token: 0x060008ED RID: 2285 RVA: 0x0002B7A8 File Offset: 0x000299A8
	private static event Action<IronSourceError> _onBannerAdLoadFailedEvent;

	// Token: 0x1400004B RID: 75
	// (add) Token: 0x060008EE RID: 2286 RVA: 0x0002B7DB File Offset: 0x000299DB
	// (remove) Token: 0x060008EF RID: 2287 RVA: 0x0002B7FC File Offset: 0x000299FC
	public static event Action<IronSourceError> onBannerAdLoadFailedEvent
	{
		add
		{
			if (IronSourceEvents._onBannerAdLoadFailedEvent == null || !IronSourceEvents._onBannerAdLoadFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdLoadFailedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onBannerAdLoadFailedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdLoadFailedEvent -= value;
			}
		}
	}

	// Token: 0x060008F0 RID: 2288 RVA: 0x0002B818 File Offset: 0x00029A18
	public void onBannerAdLoadFailed(string description)
	{
		if (IronSourceEvents._onBannerAdLoadFailedEvent != null)
		{
			IronSourceError errorFromErrorObject = this.getErrorFromErrorObject(description);
			IronSourceEvents._onBannerAdLoadFailedEvent(errorFromErrorObject);
		}
	}

	// Token: 0x1400004C RID: 76
	// (add) Token: 0x060008F1 RID: 2289 RVA: 0x0002B840 File Offset: 0x00029A40
	// (remove) Token: 0x060008F2 RID: 2290 RVA: 0x0002B874 File Offset: 0x00029A74
	private static event Action _onBannerAdClickedEvent;

	// Token: 0x1400004D RID: 77
	// (add) Token: 0x060008F3 RID: 2291 RVA: 0x0002B8A7 File Offset: 0x00029AA7
	// (remove) Token: 0x060008F4 RID: 2292 RVA: 0x0002B8C8 File Offset: 0x00029AC8
	public static event Action onBannerAdClickedEvent
	{
		add
		{
			if (IronSourceEvents._onBannerAdClickedEvent == null || !IronSourceEvents._onBannerAdClickedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdClickedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onBannerAdClickedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdClickedEvent -= value;
			}
		}
	}

	// Token: 0x060008F5 RID: 2293 RVA: 0x0002B8E2 File Offset: 0x00029AE2
	public void onBannerAdClicked()
	{
		if (IronSourceEvents._onBannerAdClickedEvent != null)
		{
			IronSourceEvents._onBannerAdClickedEvent();
		}
	}

	// Token: 0x1400004E RID: 78
	// (add) Token: 0x060008F6 RID: 2294 RVA: 0x0002B8F8 File Offset: 0x00029AF8
	// (remove) Token: 0x060008F7 RID: 2295 RVA: 0x0002B92C File Offset: 0x00029B2C
	private static event Action _onBannerAdScreenPresentedEvent;

	// Token: 0x1400004F RID: 79
	// (add) Token: 0x060008F8 RID: 2296 RVA: 0x0002B95F File Offset: 0x00029B5F
	// (remove) Token: 0x060008F9 RID: 2297 RVA: 0x0002B980 File Offset: 0x00029B80
	public static event Action onBannerAdScreenPresentedEvent
	{
		add
		{
			if (IronSourceEvents._onBannerAdScreenPresentedEvent == null || !IronSourceEvents._onBannerAdScreenPresentedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdScreenPresentedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onBannerAdScreenPresentedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdScreenPresentedEvent -= value;
			}
		}
	}

	// Token: 0x060008FA RID: 2298 RVA: 0x0002B99A File Offset: 0x00029B9A
	public void onBannerAdScreenPresented()
	{
		if (IronSourceEvents._onBannerAdScreenPresentedEvent != null)
		{
			IronSourceEvents._onBannerAdScreenPresentedEvent();
		}
	}

	// Token: 0x14000050 RID: 80
	// (add) Token: 0x060008FB RID: 2299 RVA: 0x0002B9B0 File Offset: 0x00029BB0
	// (remove) Token: 0x060008FC RID: 2300 RVA: 0x0002B9E4 File Offset: 0x00029BE4
	private static event Action _onBannerAdScreenDismissedEvent;

	// Token: 0x14000051 RID: 81
	// (add) Token: 0x060008FD RID: 2301 RVA: 0x0002BA17 File Offset: 0x00029C17
	// (remove) Token: 0x060008FE RID: 2302 RVA: 0x0002BA38 File Offset: 0x00029C38
	public static event Action onBannerAdScreenDismissedEvent
	{
		add
		{
			if (IronSourceEvents._onBannerAdScreenDismissedEvent == null || !IronSourceEvents._onBannerAdScreenDismissedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdScreenDismissedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onBannerAdScreenDismissedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdScreenDismissedEvent -= value;
			}
		}
	}

	// Token: 0x060008FF RID: 2303 RVA: 0x0002BA52 File Offset: 0x00029C52
	public void onBannerAdScreenDismissed()
	{
		if (IronSourceEvents._onBannerAdScreenDismissedEvent != null)
		{
			IronSourceEvents._onBannerAdScreenDismissedEvent();
		}
	}

	// Token: 0x14000052 RID: 82
	// (add) Token: 0x06000900 RID: 2304 RVA: 0x0002BA68 File Offset: 0x00029C68
	// (remove) Token: 0x06000901 RID: 2305 RVA: 0x0002BA9C File Offset: 0x00029C9C
	private static event Action _onBannerAdLeftApplicationEvent;

	// Token: 0x14000053 RID: 83
	// (add) Token: 0x06000902 RID: 2306 RVA: 0x0002BACF File Offset: 0x00029CCF
	// (remove) Token: 0x06000903 RID: 2307 RVA: 0x0002BAF0 File Offset: 0x00029CF0
	public static event Action onBannerAdLeftApplicationEvent
	{
		add
		{
			if (IronSourceEvents._onBannerAdLeftApplicationEvent == null || !IronSourceEvents._onBannerAdLeftApplicationEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdLeftApplicationEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onBannerAdLeftApplicationEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onBannerAdLeftApplicationEvent -= value;
			}
		}
	}

	// Token: 0x06000904 RID: 2308 RVA: 0x0002BB0A File Offset: 0x00029D0A
	public void onBannerAdLeftApplication()
	{
		if (IronSourceEvents._onBannerAdLeftApplicationEvent != null)
		{
			IronSourceEvents._onBannerAdLeftApplicationEvent();
		}
	}

	// Token: 0x14000054 RID: 84
	// (add) Token: 0x06000905 RID: 2309 RVA: 0x0002BB20 File Offset: 0x00029D20
	// (remove) Token: 0x06000906 RID: 2310 RVA: 0x0002BB54 File Offset: 0x00029D54
	private static event Action<string> _onSegmentReceivedEvent;

	// Token: 0x14000055 RID: 85
	// (add) Token: 0x06000907 RID: 2311 RVA: 0x0002BB87 File Offset: 0x00029D87
	// (remove) Token: 0x06000908 RID: 2312 RVA: 0x0002BBA8 File Offset: 0x00029DA8
	public static event Action<string> onSegmentReceivedEvent
	{
		add
		{
			if (IronSourceEvents._onSegmentReceivedEvent == null || !IronSourceEvents._onSegmentReceivedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onSegmentReceivedEvent += value;
			}
		}
		remove
		{
			if (IronSourceEvents._onSegmentReceivedEvent.GetInvocationList().Contains(value))
			{
				IronSourceEvents._onSegmentReceivedEvent -= value;
			}
		}
	}

	// Token: 0x06000909 RID: 2313 RVA: 0x0002BBC2 File Offset: 0x00029DC2
	public void onSegmentReceived(string segmentName)
	{
		if (IronSourceEvents._onSegmentReceivedEvent != null)
		{
			IronSourceEvents._onSegmentReceivedEvent(segmentName);
		}
	}

	// Token: 0x0600090A RID: 2314 RVA: 0x0002BBD8 File Offset: 0x00029DD8
	private IronSourceError getErrorFromErrorObject(object descriptionObject)
	{
		Dictionary<string, object> dictionary = null;
		if (descriptionObject is IDictionary)
		{
			dictionary = (descriptionObject as Dictionary<string, object>);
		}
		else if (descriptionObject is string && !string.IsNullOrEmpty(descriptionObject.ToString()))
		{
			dictionary = (Json.Deserialize(descriptionObject.ToString()) as Dictionary<string, object>);
		}
		IronSourceError result = new IronSourceError(-1, "");
		if (dictionary != null && dictionary.Count > 0)
		{
			int errorCode = Convert.ToInt32(dictionary["error_code"].ToString());
			string errorDescription = dictionary["error_description"].ToString();
			result = new IronSourceError(errorCode, errorDescription);
		}
		return result;
	}

	// Token: 0x0600090B RID: 2315 RVA: 0x0002BC64 File Offset: 0x00029E64
	private IronSourcePlacement getPlacementFromObject(object placementObject)
	{
		Dictionary<string, object> dictionary = null;
		if (placementObject is IDictionary)
		{
			dictionary = (placementObject as Dictionary<string, object>);
		}
		else if (placementObject is string)
		{
			dictionary = (Json.Deserialize(placementObject.ToString()) as Dictionary<string, object>);
		}
		IronSourcePlacement result = null;
		if (dictionary != null && dictionary.Count > 0)
		{
			int rewardAmount = Convert.ToInt32(dictionary["placement_reward_amount"].ToString());
			string rewardName = dictionary["placement_reward_name"].ToString();
			result = new IronSourcePlacement(dictionary["placement_name"].ToString(), rewardName, rewardAmount);
		}
		return result;
	}

	// Token: 0x040007BF RID: 1983
	private const string ERROR_CODE = "error_code";

	// Token: 0x040007C0 RID: 1984
	private const string ERROR_DESCRIPTION = "error_description";

	// Token: 0x040007C1 RID: 1985
	private const string INSTANCE_ID_KEY = "instanceId";

	// Token: 0x040007C2 RID: 1986
	private const string PLACEMENT_KEY = "placement";
}
