using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000016 RID: 22
public class CareerModalController : ModalController
{
	// Token: 0x06000051 RID: 81 RVA: 0x00003294 File Offset: 0x00001494
	protected override void Awake()
	{
		this.gameState = GameController.Instance.game;
		this.UpdateLabels();
		Observable.Interval(TimeSpan.FromMilliseconds(500.0)).Subscribe(delegate(long _)
		{
			this.UpdateLabels();
		}).AddTo(base.gameObject);
		base.Awake();
	}

	// Token: 0x06000052 RID: 82 RVA: 0x000032ED File Offset: 0x000014ED
	public override void OnIntroFinished()
	{
		this.InitPanels();
		base.OnIntroFinished();
		FTUE_Manager.ShowFTUE("ChangingRoom", null);
	}

	// Token: 0x06000053 RID: 83 RVA: 0x00003307 File Offset: 0x00001507
	private void InitPanels()
	{
		this.inventoryPanel.Init(this);
		this._swagPanelUi.Init(this);
		this.statsUI.Init(this);
	}

	// Token: 0x06000054 RID: 84 RVA: 0x00003330 File Offset: 0x00001530
	private void UpdateLabels()
	{
		int num;
		int num2;
		string text = NumberFormat.GetPostFix(Math.Round(this.gameState.SessionCash.Value).ToString("F0"), false, out num, out num2) + "aire!";
		this.titleText.text = char.ToUpper(text[0]).ToString() + text.Substring(1);
	}

	// Token: 0x06000055 RID: 85 RVA: 0x000033A0 File Offset: 0x000015A0
	public void ShowPlayerInventory(ItemType itemType, int slotIndex)
	{
		this.inventoryPanel.SetupViewForType(itemType, slotIndex, false, true);
	}

	// Token: 0x06000056 RID: 86 RVA: 0x000033B1 File Offset: 0x000015B1
	protected override void ToggleCanvasGroup(bool isOn, CanvasGroup cGroup)
	{
		if (isOn && cGroup.name == "InventoryPanel")
		{
			this.inventoryPanel.SetupViewForAllTypes();
		}
		base.ToggleCanvasGroup(isOn, cGroup);
	}

	// Token: 0x06000057 RID: 87 RVA: 0x000033DB File Offset: 0x000015DB
	public void ShowSwag()
	{
		this.ShowPanel("Swag");
	}

	// Token: 0x06000058 RID: 88 RVA: 0x000033E8 File Offset: 0x000015E8
	public void ShowInventory()
	{
		this.ShowPanel("Inventory");
	}

	// Token: 0x06000059 RID: 89 RVA: 0x000033F5 File Offset: 0x000015F5
	public void ShowStats()
	{
		this.ShowPanel("Stats");
	}

	// Token: 0x0400005D RID: 93
	[SerializeField]
	private Text titleText;

	// Token: 0x0400005E RID: 94
	[SerializeField]
	private InventoryPanel inventoryPanel;

	// Token: 0x0400005F RID: 95
	[SerializeField]
	private CanvasGroup playerInventoryCanvasGroup;

	// Token: 0x04000060 RID: 96
	[SerializeField]
	private SwagPanel _swagPanelUi;

	// Token: 0x04000061 RID: 97
	[SerializeField]
	private StatsUI statsUI;

	// Token: 0x04000062 RID: 98
	private GameState gameState;
}
