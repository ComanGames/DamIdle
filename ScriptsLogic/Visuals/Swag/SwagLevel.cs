using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000036 RID: 54
[Serializable]
public class SwagLevel
{
	// Token: 0x0600010B RID: 267 RVA: 0x00007154 File Offset: 0x00005354
	public void ToggleImages(bool show)
	{
		foreach (Image image in this.images)
		{
			image.enabled = show;
		}
		if (!show)
		{
			return;
		}
		foreach (Image image2 in this.disableImages)
		{
			image2.enabled = false;
		}
	}

	// Token: 0x04000140 RID: 320
	public string name;

	// Token: 0x04000141 RID: 321
	public string achievementName;

	// Token: 0x04000142 RID: 322
	[HideInInspector]
	public int achievementActivator;

	// Token: 0x04000143 RID: 323
	public List<Image> images;

	// Token: 0x04000144 RID: 324
	public List<Image> disableImages;
}
