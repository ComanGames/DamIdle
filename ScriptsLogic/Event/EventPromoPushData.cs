using System;

// Token: 0x02000095 RID: 149
[Serializable]
public class EventPromoPushData
{
	// Token: 0x04000392 RID: 914
	public bool active;

	// Token: 0x04000393 RID: 915
	public string eventName;

	// Token: 0x04000394 RID: 916
	public string bannerText;

	// Token: 0x04000395 RID: 917
	public string title;

	// Token: 0x04000396 RID: 918
	public string body;

	// Token: 0x04000397 RID: 919
	public string hexColor = "#4E60A2FF";

	// Token: 0x04000398 RID: 920
	public int version = 1;

	// Token: 0x04000399 RID: 921
	public int secondsUntilDisplay = 10;
}
