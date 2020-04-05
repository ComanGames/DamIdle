using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000A9 RID: 169
public class LeaderboardPanel : MonoBehaviour
{
	// Token: 0x06000499 RID: 1177 RVA: 0x00018680 File Offset: 0x00016880
	public void Init()
	{
		this.spinner.SetActive(true);
		this.IsInitialized = true;
		if (GameController.Instance.game.progressionType == PlanetProgressionType.Missions)
		{
			this.txt_scoreHeader.text = "Goal Points";
			return;
		}
		this.txt_scoreHeader.text = "Lifetime Angels";
	}

	// Token: 0x0600049A RID: 1178 RVA: 0x000186D3 File Offset: 0x000168D3
	public void WireData(List<LeaderboardItem> items)
	{
		this.spinner.SetActive(false);
		if (items != null)
		{
			this.PopulateLeaderboardData(items);
			return;
		}
		Object.Instantiate<GameObject>(this.leaderboardTakeActionStatePrefab, this.parent, false).GetComponent<LeaderboardTakeActionPanel>().WireData();
	}

	// Token: 0x0600049B RID: 1179 RVA: 0x00018708 File Offset: 0x00016908
	private void PopulateLeaderboardData(List<LeaderboardItem> items)
	{
		int num = 0;
		int num2 = 0;
		foreach (LeaderboardItem leaderboardItem in items)
		{
			num2++;
			LeaderboardItemView component = Object.Instantiate<GameObject>(this.itemPrefab).GetComponent<LeaderboardItemView>();
			component.transform.SetParent(this.parent);
			component.transform.localScale = Vector3.one;
			component.transform.localPosition = Vector3.one;
			component.Init(leaderboardItem);
			if (leaderboardItem.me)
			{
				num = num2;
				this.myEntry = component;
			}
		}
		float num3 = 1f - (float)(num - 1) / (float)num2;
		num3 = Mathf.Round(num3 * 100f) * 5f / 5f / 100f;
		this.normalizedPositions.Remove(this.scroll.transform.parent);
		this.normalizedPositions.Add(this.scroll.transform.parent, num3);
		base.StartCoroutine(this.ScrollToPosition(num3));
	}

	// Token: 0x0600049C RID: 1180 RVA: 0x00018830 File Offset: 0x00016A30
	private IEnumerator ScrollToPosition(float scrollPosition)
	{
		yield return new WaitForEndOfFrame();
		this.scroll.verticalNormalizedPosition = scrollPosition;
		yield break;
	}

	// Token: 0x04000418 RID: 1048
	[SerializeField]
	private GameObject leaderboardTakeActionStatePrefab;

	// Token: 0x04000419 RID: 1049
	[SerializeField]
	private GameObject itemPrefab;

	// Token: 0x0400041A RID: 1050
	[SerializeField]
	private GameObject spinner;

	// Token: 0x0400041B RID: 1051
	[SerializeField]
	private Transform parent;

	// Token: 0x0400041C RID: 1052
	[SerializeField]
	private ScrollRect scroll;

	// Token: 0x0400041D RID: 1053
	[SerializeField]
	private Text txt_scoreHeader;

	// Token: 0x0400041E RID: 1054
	private Dictionary<Transform, float> normalizedPositions = new Dictionary<Transform, float>();

	// Token: 0x0400041F RID: 1055
	private LeaderboardItemView myEntry;

	// Token: 0x04000420 RID: 1056
	[HideInInspector]
	public bool IsInitialized;
}
