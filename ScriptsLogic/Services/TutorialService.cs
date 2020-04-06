using System;
using System.Collections.Generic;
using System.Linq;
using AdCap;
using Platforms.Logger;
using UniRx;
using UnityEngine;

// Token: 0x02000103 RID: 259
public class TutorialService : IDisposable
{
	// Token: 0x060006D6 RID: 1750 RVA: 0x0002463C File Offset: 0x0002283C
	public void Dispose()
	{
		this.disposables.Dispose();
		this.stepDisposables.Dispose();
		this.stateDisposables.Dispose();
		foreach (KeyValuePair<string, CompositeDisposable> keyValuePair in this.tutorialBlockDisposableMap)
		{
			keyValuePair.Value.Dispose();
		}
	}

	// Token: 0x060006D7 RID: 1751 RVA: 0x000246B8 File Offset: 0x000228B8
	public bool HasTutorialBlock(string blockId)
	{
		return this.currentTutorialBlockList.Any(x => x.BlockId == blockId);
	}

	// Token: 0x060006D8 RID: 1752 RVA: 0x000246EC File Offset: 0x000228EC
	public void Init(IGameController gameController, ITriggerService triggerService, DataService dataService, IUserDataService userDataService)
	{
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		this.triggerService = triggerService;
		this.dataService = dataService;
		this.userDataService = userDataService;
		this.gameController = gameController;
		this.LoadCompletedBlocks();
		this.LoadCompletedSteps();
		gameController.State.Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(this.disposables);
		gameController.NavigationService.AddBackButtonObservable(this.canUseBackButton);
		this.CurrentStep.Subscribe(delegate(TutorialStep x)
		{
			this.canUseBackButton.Value = (x == null);
		}).AddTo(this.disposables);
	}

	// Token: 0x060006D9 RID: 1753 RVA: 0x00024788 File Offset: 0x00022988
	public void NextStep()
	{
		this.logger.Trace("Next Step", Array.Empty<object>());
		if (this.CurrentStep.Value != null)
		{
			this.gameController.AnalyticService.SendTutorialEvent(this.CurrentStep.Value.StepId, this.CurrentStep.Value.Index, true);
			this.CompletedTutorialSteps.Add(this.CurrentStep.Value.StepId);
		}
		this.stepDisposables.Clear();
		if (this.remainingSteps.Count <= 0)
		{
			this.OnBlockFinished();
			return;
		}
		TutorialStep nextStep = this.remainingSteps.Dequeue();
		if (this.triggerService.CheckTrigger(nextStep.LocationTrigger))
		{
			this.OnStepTriggered(nextStep);
			return;
		}
		this.triggerService.MonitorTriggers(new List<TriggerData>
		{
			nextStep.LocationTrigger
		}, true).First(x => x).Subscribe(delegate(bool _)
		{
			this.OnStepTriggered(nextStep);
		}).AddTo(this.stepDisposables);
	}

	// Token: 0x060006DA RID: 1754 RVA: 0x000248CC File Offset: 0x00022ACC
	public void RegisterTarget(string key, GameObject go)
	{
		if (!this.TargetMap.ContainsKey(key))
		{
			this.TargetMap.Add(key, new List<GameObject>
			{
				go
			});
			return;
		}
		this.TargetMap[key].Add(go);
	}

	// Token: 0x060006DB RID: 1755 RVA: 0x00024908 File Offset: 0x00022B08
	public void UnregisterTarget(string key, GameObject go)
	{
		if (this.TargetMap.ContainsKey(key))
		{
			this.TargetMap[key].Remove(go);
			if (this.TargetMap[key].Count == 0)
			{
				this.TargetMap.Remove(key);
			}
		}
	}

	// Token: 0x060006DC RID: 1756 RVA: 0x00024958 File Offset: 0x00022B58
	private void OnStateChanged(GameState state)
	{
		this.ResetTutorials();
		this.FilterIds(this.userDataService, this.gameController.PlatformId, state);
		List<TutorialBlock> list = (from x in this.currentTutorialBlockList
		where x.ResetTriggers.Count > 0 && this.CompletedTutorialBlocks.Contains(x.BlockId)
		select x).ToList<TutorialBlock>();
		if (list.Count > 0)
		{
			list.ForEach(delegate(TutorialBlock block)
			{
				this.triggerService.MonitorTriggers(block.ResetTriggers, false).First(x => x).Subscribe(delegate(bool _)
				{
					this.OnBlockReset(block);
				}).AddTo(this.stateDisposables);
			});
		}
		this.currentTutorialBlockList.ForEach(new Action<TutorialBlock>(this.WireTutorialBlock));
	}

	// Token: 0x060006DD RID: 1757 RVA: 0x000249D4 File Offset: 0x00022BD4
	private void OnStepTriggered(TutorialStep step)
	{
		this.CurrentStep.Value = step;
		this.gameController.AnalyticService.SendTutorialEvent(this.CurrentStep.Value.StepId, this.CurrentStep.Value.Index, false);
		if (step.EndTriggers.Count > 0)
		{
			this.triggerService.MonitorTriggers(step.EndTriggers, false).First(x => x).DelayFrame(1, FrameCountType.Update).Subscribe(delegate(bool __)
			{
				this.NextStep();
			}).AddTo(this.stepDisposables);
		}
	}

	// Token: 0x060006DE RID: 1758 RVA: 0x00024A88 File Offset: 0x00022C88
	private void ResetTutorials()
	{
		this.stepDisposables.Clear();
		this.stateDisposables.Clear();
		foreach (KeyValuePair<string, CompositeDisposable> keyValuePair in this.tutorialBlockDisposableMap)
		{
			keyValuePair.Value.Dispose();
		}
		this.tutorialBlockDisposableMap.Clear();
		this.remainingSteps.Clear();
		this.pendingTutorialBlocks.Clear();
		this.CurrentStep.Value = null;
		this.currentTutorialBlock = null;
	}

	// Token: 0x060006DF RID: 1759 RVA: 0x00024B2C File Offset: 0x00022D2C
	private void FilterIds(IUserDataService userDataService, string platform, GameState state)
	{
		this.currentTutorialBlockList = (from x in this.dataService.ExternalData.TutorialConfig
		where x.PlanetId == state.planetTheme.Value || x.PlanetId == "Any" || (x.PlanetId == "Event" && state.IsEventPlanet)
		select x).ToList<TutorialBlock>();
		this.currentTutorialBlockList = (from x in this.currentTutorialBlockList
		where userDataService.IsTestGroupMember(x.ABTestGroup)
		select x).ToList<TutorialBlock>();
		this.currentTutorialBlockList = (from x in this.currentTutorialBlockList
		where x.Platforms.Contains(platform) || x.Platforms == "All"
		select x).ToList<TutorialBlock>();
		this.currentTutorialBlockList = (from x in this.currentTutorialBlockList
		where !this.CompletedTutorialBlocks.Contains(x.BlockId) || x.ResetTriggers.Count > 0
		select x).ToList<TutorialBlock>();
		this.tutorialSteps = (from x in this.dataService.ExternalData.TutorialSteps
		where userDataService.IsTestGroupMember(x.ABTestGroup)
		select x).ToList<TutorialStep>();
		this.tutorialSteps = (from x in this.tutorialSteps
		where x.Platforms.Contains(platform) || x.Platforms == "All"
		select x).ToList<TutorialStep>();
	}

	// Token: 0x060006E0 RID: 1760 RVA: 0x00024C3C File Offset: 0x00022E3C
	private void WireTutorialBlock(TutorialBlock block)
	{
		this.tutorialBlockDisposableMap.Add(block.BlockId, new CompositeDisposable());
		List<IEnumerable<TriggerData>> list = block.SkipTriggers.GroupBy(x => x.TriggerGroup, (trigger, @group) => group).ToList<IEnumerable<TriggerData>>();
		if (list.Count > 0 && list.Any(delegate(IEnumerable<TriggerData> x)
		{
			Func<TriggerData, bool> predicate= (y) => this.triggerService.CheckTrigger(y);
			return x.All(predicate);
		}))
		{
			this.OnTutorialBlockSkipped(block);
			return;
		}
		List<IEnumerable<TriggerData>> list2 = block.StartTriggers.GroupBy(x => x.TriggerGroup, (trigger, @group) => group).ToList<IEnumerable<TriggerData>>();
		if (list2.Count == 0 || list2.Any(delegate(IEnumerable<TriggerData> x)
		{
			Func<TriggerData, bool> predicate = (y) => triggerService.CheckTrigger(y);
			return x.All(predicate);
		}))
		{
			this.OnTutorialBlockTriggered(block);
			return;
		}
		this.triggerService.MonitorTriggers(block.StartTriggers, true).First(x => x).Subscribe(delegate(bool _)
		{
			this.OnTutorialBlockTriggered(block);
		}).AddTo(this.tutorialBlockDisposableMap[block.BlockId]);
		if (block.SkipTriggers.Count > 0)
		{
			this.triggerService.MonitorTriggers(block.SkipTriggers, true).First(x => x).Subscribe(delegate(bool _)
			{
				this.OnTutorialBlockSkipped(block);
			}).AddTo(this.tutorialBlockDisposableMap[block.BlockId]);
		}
	}

	// Token: 0x060006E1 RID: 1761 RVA: 0x00024E5C File Offset: 0x0002305C
	private void OnTutorialBlockTriggered(TutorialBlock block)
	{
		this.logger.Trace("OnTutorialBlockTriggered [{0}]", new object[]
		{
			block.BlockId
		});
		if (this.CurrentStep.Value == null && this.currentTutorialBlock == null)
		{
			this.StartTutorialBlock(block);
			return;
		}
		this.pendingTutorialBlocks.Enqueue(block);
	}

	// Token: 0x060006E2 RID: 1762 RVA: 0x00024EB4 File Offset: 0x000230B4
	private void OnTutorialBlockSkipped(TutorialBlock block)
	{
		this.logger.Trace("OnTutorialBlockSkipped [{0}]", new object[]
		{
			block.BlockId
		});
		this.CompletedTutorialBlocks.Add(block.BlockId);
		this.GetStepsForBlock(block).ForEach(delegate(TutorialStep step)
		{
			this.CompletedTutorialSteps.Add(step.StepId);
			this.gameController.AnalyticService.SendTutorialEvent(step.StepId, 0, true);
		});
		this.SaveCompletedSteps();
		this.SaveCompletedBlocks();
		this.ClearBlockSubscriptions(block);
	}

	// Token: 0x060006E3 RID: 1763 RVA: 0x00024F1C File Offset: 0x0002311C
	private List<TutorialStep> GetStepsForBlock(TutorialBlock block)
	{
		return (from step in this.tutorialSteps
		where step.BlockId == block.BlockId
		select step).ToList<TutorialStep>();
	}

	// Token: 0x060006E4 RID: 1764 RVA: 0x00024F54 File Offset: 0x00023154
	private void StartTutorialBlock(TutorialBlock block)
	{
		this.currentTutorialBlock = block;
		this.logger.Trace("StartTutorialBlock [{0}]", new object[]
		{
			block.BlockId
		});
		this.ClearBlockSubscriptions(block);
		int i = 1;
		this.GetStepsForBlock(block).ForEach(delegate(TutorialStep step)
		{
			i++;
			step.Index = i;
			this.remainingSteps.Enqueue(step);
		});
		this.NextStep();
	}

	// Token: 0x060006E5 RID: 1765 RVA: 0x00024FC0 File Offset: 0x000231C0
	private void OnBlockFinished()
	{
		this.logger.Trace("BlockFinished BlockId [{0}]", new object[]
		{
			this.CurrentStep.Value.BlockId
		});
		TutorialBlock completedBlock = this.currentTutorialBlockList.FirstOrDefault(x => x.BlockId == this.CurrentStep.Value.BlockId);
		this.CompletedTutorialBlocks.Add(completedBlock.BlockId);
		this.SaveCompletedSteps();
		this.SaveCompletedBlocks();
		this.currentTutorialBlock = null;
		this.CurrentStep.Value = null;
		while (this.pendingTutorialBlocks.Count > 0)
		{
			TutorialBlock tutorialBlock = this.pendingTutorialBlocks.Dequeue();
			if (!this.CompletedTutorialBlocks.Contains(tutorialBlock.BlockId))
			{
				this.StartTutorialBlock(tutorialBlock);
				break;
			}
		}
		if (completedBlock.ResetTriggers.Count > 0)
		{
			this.triggerService.MonitorTriggers(completedBlock.ResetTriggers, false).First(x => x).Subscribe(delegate(bool _)
			{
				this.OnBlockReset(completedBlock);
			}).AddTo(this.disposables);
		}
	}

	// Token: 0x060006E6 RID: 1766 RVA: 0x000250F8 File Offset: 0x000232F8
	private void OnBlockReset(TutorialBlock block)
	{
		this.logger.Trace("OnBlockReset [{0}]", new object[]
		{
			block.BlockId
		});
		this.ClearBlockSubscriptions(block);
		this.WireTutorialBlock(block);
	}

	// Token: 0x060006E7 RID: 1767 RVA: 0x00025127 File Offset: 0x00023327
	private void ClearBlockSubscriptions(TutorialBlock block)
	{
		if (this.tutorialBlockDisposableMap.ContainsKey(block.BlockId))
		{
			this.tutorialBlockDisposableMap[block.BlockId].Dispose();
			this.tutorialBlockDisposableMap.Remove(block.BlockId);
		}
	}

	// Token: 0x060006E8 RID: 1768 RVA: 0x00025164 File Offset: 0x00023364
	private void LoadCompletedBlocks()
	{
		string @string = PlayerPrefs.GetString("CompletedTutorialBlockIds");
		if (!string.IsNullOrEmpty(@string))
		{
			foreach (string item in @string.Split(new char[]
			{
				','
			}))
			{
				this.CompletedTutorialBlocks.Add(item);
			}
		}
	}

	// Token: 0x060006E9 RID: 1769 RVA: 0x000251B4 File Offset: 0x000233B4
	private void SaveCompletedBlocks()
	{
		string value = string.Join(",", this.CompletedTutorialBlocks.ToArray<string>());
		PlayerPrefs.SetString("CompletedTutorialBlockIds", value);
	}

	// Token: 0x060006EA RID: 1770 RVA: 0x000251E4 File Offset: 0x000233E4
	private void LoadCompletedSteps()
	{
		string @string = PlayerPrefs.GetString("CompletedTutorialStepIds");
		if (!string.IsNullOrEmpty(@string))
		{
			foreach (string item in @string.Split(new char[]
			{
				','
			}))
			{
				this.CompletedTutorialSteps.Add(item);
			}
		}
	}

	// Token: 0x060006EB RID: 1771 RVA: 0x00025234 File Offset: 0x00023434
	private void SaveCompletedSteps()
	{
		string value = string.Join(",", this.CompletedTutorialSteps.ToArray<string>());
		PlayerPrefs.SetString("CompletedTutorialStepIds", value);
	}

	// Token: 0x060006EC RID: 1772 RVA: 0x00025264 File Offset: 0x00023464
	public void SkipTutorialBlock(string id)
	{
		if (id == "All")
		{
			this.currentTutorialBlockList.ForEach(new Action<TutorialBlock>(this.OnTutorialBlockSkipped));
			this.stateDisposables.Clear();
			this.stepDisposables.Clear();
			this.CurrentStep.Value = null;
			this.pendingTutorialBlocks.Clear();
			this.remainingSteps.Clear();
			return;
		}
		if (id == "Current")
		{
			if (this.CurrentStep.Value != null)
			{
				this.stepDisposables.Clear();
				this.CurrentStep.Value = null;
				this.remainingSteps.Clear();
				return;
			}
		}
		else
		{
			this.OnTutorialBlockSkipped(this.currentTutorialBlockList.FirstOrDefault(x => x.BlockId == id));
		}
	}

	// Token: 0x0400065A RID: 1626
	public readonly ReactiveProperty<TutorialStep> CurrentStep = new ReactiveProperty<TutorialStep>();

	// Token: 0x0400065B RID: 1627
	public const string COMPLETED_BLOCK_IDS_SAVE_KEY = "CompletedTutorialBlockIds";

	// Token: 0x0400065C RID: 1628
	public const string COMPLETED_STEP_IDS_SAVE_KEY = "CompletedTutorialStepIds";

	// Token: 0x0400065D RID: 1629
	public ReactiveCollection<string> CompletedTutorialBlocks = new ReactiveCollection<string>();

	// Token: 0x0400065E RID: 1630
	public ReactiveCollection<string> CompletedTutorialSteps = new ReactiveCollection<string>();

	// Token: 0x0400065F RID: 1631
	public ReactiveDictionary<string, List<GameObject>> TargetMap = new ReactiveDictionary<string, List<GameObject>>();

	// Token: 0x04000660 RID: 1632
	private TutorialBlock currentTutorialBlock;

	// Token: 0x04000661 RID: 1633
	private List<TutorialBlock> currentTutorialBlockList = new List<TutorialBlock>();

	// Token: 0x04000662 RID: 1634
	private List<TutorialStep> tutorialSteps = new List<TutorialStep>();

	// Token: 0x04000663 RID: 1635
	private ITriggerService triggerService;

	// Token: 0x04000664 RID: 1636
	private DataService dataService;

	// Token: 0x04000665 RID: 1637
	private IUserDataService userDataService;

	// Token: 0x04000666 RID: 1638
	private IGameController gameController;

	// Token: 0x04000667 RID: 1639
	private Queue<TutorialBlock> pendingTutorialBlocks = new Queue<TutorialBlock>();

	// Token: 0x04000668 RID: 1640
	private Queue<TutorialStep> remainingSteps = new Queue<TutorialStep>();

	// Token: 0x04000669 RID: 1641
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x0400066A RID: 1642
	private CompositeDisposable stepDisposables = new CompositeDisposable();

	// Token: 0x0400066B RID: 1643
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x0400066C RID: 1644
	private Dictionary<string, CompositeDisposable> tutorialBlockDisposableMap = new Dictionary<string, CompositeDisposable>();

	// Token: 0x0400066D RID: 1645
	private Platforms.Logger.Logger logger;

	// Token: 0x0400066E RID: 1646
	private ReactiveProperty<bool> canUseBackButton = new ReactiveProperty<bool>();
}
