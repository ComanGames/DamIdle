using System;
using UnityEngine;

// Token: 0x020001F7 RID: 503
[Serializable]
public class ProfitBoostSetup
{
	// Token: 0x04000C63 RID: 3171
	public float minRechargeTime;

	// Token: 0x04000C64 RID: 3172
	public float maxRechargeTime;

	// Token: 0x04000C65 RID: 3173
	public float minEffectTime;

	// Token: 0x04000C66 RID: 3174
	public float maxEffectTime;

	// Token: 0x04000C67 RID: 3175
	public int minBoostEffect;

	// Token: 0x04000C68 RID: 3176
	public int maxBoostEffect;

	// Token: 0x04000C69 RID: 3177
	public float minDiscount;

	// Token: 0x04000C6A RID: 3178
	public float maxDiscount;

	// Token: 0x04000C6B RID: 3179
	public float fightBackEffect;

	// Token: 0x04000C6C RID: 3180
	public ProfitBoostSetup.BoostStyle style;

	// Token: 0x04000C6D RID: 3181
	public ProfitBoostSetup.BoostDisplay display;

	// Token: 0x04000C6E RID: 3182
	public Color boostTextColor;

	// Token: 0x04000C6F RID: 3183
	public string notificationTitle = "Deploy the profit martians!";

	// Token: 0x04000C70 RID: 3184
	public string notificationBody = "Your Profit Martians are ready to launch! They await your command on Mars. You may fire when ready.";

	// Token: 0x04000C71 RID: 3185
	public string infoText;

	// Token: 0x020008BA RID: 2234
	public enum BoostStyle
	{
		// Token: 0x04002BD0 RID: 11216
		all,
		// Token: 0x04002BD1 RID: 11217
		random
	}

	// Token: 0x020008BB RID: 2235
	public enum BoostDisplay
	{
		// Token: 0x04002BD3 RID: 11219
		inline,
		// Token: 0x04002BD4 RID: 11220
		corner
	}
}
