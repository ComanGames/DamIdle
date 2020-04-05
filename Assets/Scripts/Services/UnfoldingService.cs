using System;
using System.Collections.Generic;
using System.Linq;
using AdCap;
using UniRx;
using UnityEngine;

// Token: 0x0200010E RID: 270
public class UnfoldingService : IDisposable
{
	// Token: 0x0600070A RID: 1802 RVA: 0x00025804 File Offset: 0x00023A04
	public void Dispose()
	{
		this.disposables.Dispose();
		this.stateDispsoables.Dispose();
	}

	// Token: 0x0600070B RID: 1803 RVA: 0x0002581C File Offset: 0x00023A1C
	public void Init(IGameController gameController, DataService dataService, ITriggerService triggerService, IUserDataService userDataService)
	{
		this.triggerService = triggerService;
		this.gameController = gameController;
		this.LoadCompletedIds();
		IEnumerable<UnfoldingData> source = from x in dataService.ExternalData.UnfoldingConfig
		where userDataService.IsTestGroupMember(x.ABTestGroup)
		select x;
		source = from x in source
		where !this.CompletedUnfoldingStepIds.Contains(x.StepId)
		select x;
		source = from x in source
		where x.Platforms.Contains(this.gameController.PlatformId) || x.Platforms == "All"
		select x;
		this.UnfoldingData = source.ToList<UnfoldingData>();
		this.gameController.State.Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
	}

	// Token: 0x0600070C RID: 1804 RVA: 0x000258CC File Offset: 0x00023ACC
	public bool HasUnfoldingTrigger(UnfoldingId unfoldingStepId)
	{
		return this.UnfoldingData.FirstOrDefault((UnfoldingData x) => x.StepId == unfoldingStepId.Id) != null || this.CompletedUnfoldingStepIds.Contains(unfoldingStepId.Id);
	}

	// Token: 0x0600070D RID: 1805 RVA: 0x00025917 File Offset: 0x00023B17
	private void OnStateChanged(GameState state)
	{
		this.stateDispsoables.Clear();
		this.UnfoldingData.ForEach(delegate(UnfoldingData ufData)
		{
			this.triggerService.MonitorTriggers(ufData.TriggerDatas, true).First((bool x) => x).Subscribe(delegate(bool _)
			{
				this.OnUnfoldingStepTriggered(ufData);
			}).AddTo(this.stateDispsoables);
		});
	}

	// Token: 0x0600070E RID: 1806 RVA: 0x0002593B File Offset: 0x00023B3B
	private void OnUnfoldingStepTriggered(UnfoldingData ufData)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics("Unfolding_Step", "Unlocked", ufData.StepId);
		this.CompletedUnfoldingStepIds.Add(ufData.StepId);
		this.SaveCompletedIds();
	}

	// Token: 0x0600070F RID: 1807 RVA: 0x00025974 File Offset: 0x00023B74
	private void LoadCompletedIds()
	{
		string @string = PlayerPrefs.GetString("UnfoldingIds");
		if (!string.IsNullOrEmpty(@string))
		{
			string[] array = @string.Split(new char[]
			{
				','
			});
			for (int i = 0; i < array.Length; i++)
			{
				this.CompletedUnfoldingStepIds.Add(array[i]);
			}
		}
	}

	// Token: 0x06000710 RID: 1808 RVA: 0x000259C4 File Offset: 0x00023BC4
	private void SaveCompletedIds()
	{
		string value = string.Join(",", this.CompletedUnfoldingStepIds.ToArray<string>());
		PlayerPrefs.SetString("UnfoldingIds", value);
	}

	// Token: 0x040006A7 RID: 1703
	public readonly ReactiveCollection<string> CompletedUnfoldingStepIds = new ReactiveCollection<string>();

	// Token: 0x040006A8 RID: 1704
	public List<UnfoldingData> UnfoldingData = new List<UnfoldingData>();

	// Token: 0x040006A9 RID: 1705
	private const string UNFOLDING_ID_KEY = "UnfoldingIds";

	// Token: 0x040006AA RID: 1706
	private ITriggerService triggerService;

	// Token: 0x040006AB RID: 1707
	private IGameController gameController;

	// Token: 0x040006AC RID: 1708
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040006AD RID: 1709
	private CompositeDisposable stateDispsoables = new CompositeDisposable();
}
