using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000AA RID: 170
public class LeaderboardQuickButtonView : MonoBehaviour
{
	// Token: 0x0600049E RID: 1182 RVA: 0x0001885C File Offset: 0x00016A5C
	public void ShowAttentionIndicator(bool isEnabled, bool forceShow = false)
	{
		string planetName = GameController.Instance.planetName;
		bool @bool = GameController.Instance.game.planetPlayerData.GetBool(planetName + "IS_LB_ATTENTION_DISMISSED");
		if (this.attentionIndicatorPrefab != null)
		{
			if (forceShow || (isEnabled && !@bool))
			{
				this.attentionIndicatorPrefab.SetActive(true);
				return;
			}
			this.attentionIndicatorPrefab.SetActive(false);
			if (this.isDismissedOnShow)
			{
				GameController.Instance.game.planetPlayerData.Set(planetName + "IS_LB_ATTENTION_DISMISSED", "true");
				return;
			}
		}
		else
		{
			Debug.LogError("LeaderboardQuickButtonView is missing AttentionIndicator prefab Reference");
		}
	}

	// Token: 0x17000070 RID: 112
	// (get) Token: 0x0600049F RID: 1183 RVA: 0x000188FC File Offset: 0x00016AFC
	public bool isAttentionIndicatorActive
	{
		get
		{
			return this.attentionIndicatorPrefab.activeSelf;
		}
	}

	// Token: 0x04000421 RID: 1057
	[SerializeField]
	public Button button;

	// Token: 0x04000422 RID: 1058
	[SerializeField]
	private GameObject attentionIndicatorPrefab;

	// Token: 0x04000423 RID: 1059
	[SerializeField]
	private bool isDismissedOnShow = true;
}
