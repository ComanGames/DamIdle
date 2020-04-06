using System;
using UnityEngine;

// Token: 0x020000E4 RID: 228
[CreateAssetMenu(fileName = "NewIconData", menuName = "IconData")]
public class IconDataScriptableObject : ScriptableObject
{
	// Token: 0x04000599 RID: 1433
	public StringSpriteDictionary iconMap = new StringSpriteDictionary();
}
