using System;
using UnityEngine;

// Token: 0x020000E5 RID: 229
[CreateAssetMenu(fileName = "PlanetData", menuName = "New Planet Data")]
public class PlanetData : ScriptableObject
{
	// Token: 0x0400059A RID: 1434
	[Header("Planet Info")]
	[SerializeField]
	public string PlanetName;

	// Token: 0x0400059B RID: 1435
	[SerializeField]
	public string DisplayName;

	// Token: 0x0400059C RID: 1436
	[SerializeField]
	public string PurchaseId;

	// Token: 0x0400059D RID: 1437
	[SerializeField]
	public string CurrencyName;

	// Token: 0x0400059E RID: 1438
	[SerializeField]
	public string PlanetInfo;

	// Token: 0x0400059F RID: 1439
	[SerializeField]
	public int DisplayPriority;

	// Token: 0x040005A0 RID: 1440
	[SerializeField]
	public Sprite MissionImage;

	// Token: 0x040005A1 RID: 1441
	[SerializeField]
	public Color ColorTint = Color.white;

	// Token: 0x040005A2 RID: 1442
	[SerializeField]
	public string UnlockMessage = "";

	// Token: 0x040005A3 RID: 1443
	[SerializeField]
	public PlatformAchievementsScriptableObject PlatformAchievements;

	// Token: 0x040005A4 RID: 1444
	[SerializeField]
	public bool hasBooster;
}
