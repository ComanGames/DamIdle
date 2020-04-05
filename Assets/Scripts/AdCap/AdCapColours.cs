using System;
using System.Collections.Generic;

// Token: 0x0200011D RID: 285
public static class AdCapColours
{
	// Token: 0x060007A0 RID: 1952 RVA: 0x00028A10 File Offset: 0x00026C10
	public static void LoadColourData(List<ColourData> colourDataList)
	{
		if (AdCapColours.initialized)
		{
			return;
		}
		AdCapColours.initialized = true;
		foreach (ColourData colourData in colourDataList)
		{
			AdCapColours.ColourMap.Add((ColourNames)Enum.Parse(typeof(ColourNames), colourData.ColourTypeName), new ColourMaping(colourData.ColourHexValue));
		}
	}

	// Token: 0x04000716 RID: 1814
	public static Dictionary<ColourNames, ColourMaping> ColourMap = new Dictionary<ColourNames, ColourMaping>();

	// Token: 0x04000717 RID: 1815
	private static bool initialized;
}
