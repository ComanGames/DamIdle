using System;
using System.Collections.Generic;

// Token: 0x02000105 RID: 261
[Serializable]
public class TutorialStep
{
	// Token: 0x04000676 RID: 1654
	public string StepId;

	// Token: 0x04000677 RID: 1655
	public string BlockId;

	// Token: 0x04000678 RID: 1656
	public int Index;

	// Token: 0x04000679 RID: 1657
	public string Platforms;

	// Token: 0x0400067A RID: 1658
	public string ABTestGroup;

	// Token: 0x0400067B RID: 1659
	public bool IsMenu;

	// Token: 0x0400067C RID: 1660
	public string InteractableTarget;

	// Token: 0x0400067D RID: 1661
	public string HighlightTarget;

	// Token: 0x0400067E RID: 1662
	public bool HasEffect;

	// Token: 0x0400067F RID: 1663
	public string Copy;

	// Token: 0x04000680 RID: 1664
	public bool HasOkButton;

	// Token: 0x04000681 RID: 1665
	public string CharacterName;

	// Token: 0x04000682 RID: 1666
	public string CharacterPositionPortrait;

	// Token: 0x04000683 RID: 1667
	public string CharacterPositionLandscape;

	// Token: 0x04000684 RID: 1668
	public ETutorialArrowPosition ArrowPositionPortrait;

	// Token: 0x04000685 RID: 1669
	public ETutorialArrowPosition ArrowPositionLandscape;

	// Token: 0x04000686 RID: 1670
	public ESpeechBubblePosition SpeechBubblePositionPortrait;

	// Token: 0x04000687 RID: 1671
	public ESpeechBubblePosition SpeechBubblePositionLandscape;

	// Token: 0x04000688 RID: 1672
	public TriggerData LocationTrigger;

	// Token: 0x04000689 RID: 1673
	public List<TriggerData> EndTriggers = new List<TriggerData>();
}
