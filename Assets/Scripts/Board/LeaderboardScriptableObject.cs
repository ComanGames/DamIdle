using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001D1 RID: 465
[CreateAssetMenu(fileName = "Leaderboard", menuName = "Create Leaderboard Object")]
public class LeaderboardScriptableObject : ScriptableObject
{
	// Token: 0x04000BCE RID: 3022
	public List<LeaderboardItem> Items;
}
