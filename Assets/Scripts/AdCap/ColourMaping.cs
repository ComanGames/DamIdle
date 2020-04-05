using System;
using UnityEngine;

// Token: 0x0200011E RID: 286
public struct ColourMaping
{
	// Token: 0x060007A2 RID: 1954 RVA: 0x00028AA0 File Offset: 0x00026CA0
	public ColourMaping(string hexColor)
	{
		this.HexColor = hexColor;
		ColorUtility.TryParseHtmlString(hexColor, out this.Color);
	}

	// Token: 0x04000718 RID: 1816
	public Color Color;

	// Token: 0x04000719 RID: 1817
	public string HexColor;
}
