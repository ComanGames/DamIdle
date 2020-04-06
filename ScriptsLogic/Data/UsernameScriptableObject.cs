using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001DB RID: 475
[CreateAssetMenu(fileName = "Username", menuName = "Create Username Object")]
public class UsernameScriptableObject : ScriptableObject
{
	// Token: 0x04000BE7 RID: 3047
	public List<string> Adjectives;

	// Token: 0x04000BE8 RID: 3048
	public List<string> Nouns;
}
