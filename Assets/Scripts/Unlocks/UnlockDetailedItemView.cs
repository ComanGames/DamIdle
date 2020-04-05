using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000165 RID: 357
public class UnlockDetailedItemView : MonoBehaviour
{
	// Token: 0x06000B6E RID: 2926 RVA: 0x000340FB File Offset: 0x000322FB
	private void Awake()
	{
		this.btn_Click.onClick.AddListener(delegate()
		{
			if (this.clickedAction != null)
			{
				this.clickedAction(this.unlock);
			}
		});
	}

	// Token: 0x06000B6F RID: 2927 RVA: 0x00034119 File Offset: 0x00032319
	private void OnDisable()
	{
		this.disposable.Dispose();
	}

	// Token: 0x06000B70 RID: 2928 RVA: 0x00034128 File Offset: 0x00032328
	public void WireData(Unlock unlock, GameState gameState, Sprite icon, Action<Unlock> clickedAction)
	{
		this.disposable.Dispose();
		this.unlock = unlock;
		this.clickedAction = clickedAction;
		string text = NumberFormat.ConvertNormal((double)unlock.amountToEarn, 1E+15, 0);
		this.txt_value.text = text;
		this.img_Icon.sprite = icon;
		this.img_Icon.preserveAspect = true;
		this.img_GoldAward.enabled = (unlock.Reward is UnlockRewardGold && !gameState.IsEventPlanet);
		this.img_StarAward.enabled = (unlock.Reward is UnlockRewardBadge || unlock.Reward is UnlockRewardItemizationItem || unlock.Reward is UnlockRewardTimewarpDaily || unlock.Reward is UnlockRewardTimeWarpExpress || (unlock.Reward is UnlockRewardGold && gameState.IsEventPlanet));
		this.img_MegaBucksAward.enabled = (unlock.Reward is UnlockRewardMegaBucks);
		this.disposable = unlock.Earned.Subscribe(new Action<bool>(this.SetAchieved));
	}

	// Token: 0x06000B71 RID: 2929 RVA: 0x0003423C File Offset: 0x0003243C
	private void SetAchieved(bool achieved)
	{
		this.img_Background.sprite = (achieved ? this.sprite_Unlocked : this.sprite_Locked);
		this.txt_value.color = (achieved ? this.yellow : this.gray);
	}

	// Token: 0x040009DA RID: 2522
	[SerializeField]
	private Text txt_value;

	// Token: 0x040009DB RID: 2523
	[SerializeField]
	private Button btn_Click;

	// Token: 0x040009DC RID: 2524
	[SerializeField]
	private Image img_Background;

	// Token: 0x040009DD RID: 2525
	[SerializeField]
	private Image img_Icon;

	// Token: 0x040009DE RID: 2526
	[SerializeField]
	private Image img_GoldAward;

	// Token: 0x040009DF RID: 2527
	[SerializeField]
	private Image img_StarAward;

	// Token: 0x040009E0 RID: 2528
	[SerializeField]
	private Image img_MegaBucksAward;

	// Token: 0x040009E1 RID: 2529
	[SerializeField]
	private Color yellow;

	// Token: 0x040009E2 RID: 2530
	[SerializeField]
	private Color gray;

	// Token: 0x040009E3 RID: 2531
	[SerializeField]
	private Sprite sprite_Unlocked;

	// Token: 0x040009E4 RID: 2532
	[SerializeField]
	private Sprite sprite_Locked;

	// Token: 0x040009E5 RID: 2533
	private Unlock unlock;

	// Token: 0x040009E6 RID: 2534
	private Action<Unlock> clickedAction;

	// Token: 0x040009E7 RID: 2535
	private IDisposable disposable = Disposable.Empty;
}
