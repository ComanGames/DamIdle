using System;
using UnityEngine;

// Token: 0x020000E8 RID: 232
[CreateAssetMenu(fileName = "themedata-PLANETTHEME-venture-animations", menuName = "New Venture Animation")]
public class VentureAnimation : ScriptableObject
{
	// Token: 0x040005B1 RID: 1457
	[SerializeField]
	public StringSpriteListObjectDictionary VentureAnimatedSprites = new StringSpriteListObjectDictionary();
}
