using System;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Token: 0x0200017E RID: 382
public class RootUIUnlockMissionsView : MonoBehaviour, IComparer<Unlock>
{
	// Token: 0x06000C2E RID: 3118 RVA: 0x00036F88 File Offset: 0x00035188
	private void Start()
	{
		this.listItemPrefab.gameObject.SetActive(false);
		GameController.Instance.State.Take(1).DelayFrame(1, FrameCountType.Update).Subscribe(new Action<GameState>(this.OnGameStateReady)).AddTo(base.gameObject);
	}

	// Token: 0x06000C2F RID: 3119 RVA: 0x00036FDA File Offset: 0x000351DA
	private void OnDestroy()
	{
		this.stateDisposables.Dispose();
	}

	// Token: 0x06000C30 RID: 3120 RVA: 0x00036FE8 File Offset: 0x000351E8
	private void OnGameStateReady(GameState state)
	{
		this.stateDisposables.Clear();
		if (!GameController.Instance.UnlockService.ShowsUnlockMissions)
		{
			return;
		}
		this.state = state;
		while (this.currentUnlockMissions.Count > 0)
		{
			Unlock key = this.currentUnlockMissions[0];
			this.currentUnlockMissions.RemoveAt(0);
			Object.DestroyImmediate(this.viewsByUnlock[key].gameObject);
		}
		this.viewsByUnlock = new Dictionary<Unlock, RootUIUnlockMissionListItemView>();
		this.currentMissionTargets = new List<string>();
		this.remainingQueue = (from x in GameController.Instance.UnlockService.Unlocks
		where !x.EverClaimed.Value
		select x).ToList<Unlock>();
		this.remainingQueue.Sort(this);
		this.btn_openUnlocks.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			GameController.Instance.AnalyticService.SendNavActionAnalytics("Open_Unlocks", "RootUIEveryVentureUnlockView", "");
			GameController.Instance.NavigationService.CreateModal<UnlocksModalController>(NavModals.UNLOCKS, false);
			FTUE_Manager.ShowFTUE("Unlocks", null);
		}).AddTo(this.stateDisposables);
		while (this.currentUnlockMissions.Count < 3 && this.remainingQueue.Count > 0)
		{
			this.PopulateNextItem();
		}
		this.UpdateShouldBeShown();
	}

	// Token: 0x06000C31 RID: 3121 RVA: 0x00037120 File Offset: 0x00035320
	private void PopulateNextItem()
	{
		if (this.remainingQueue.Count > 0)
		{
			for (int i = 0; i < this.remainingQueue.Count; i++)
			{
				Unlock unlock = this.remainingQueue[i];
				if (unlock is SingleVentureUnlock)
				{
					string ventureName = (unlock as SingleVentureUnlock).ventureName;
					if (!this.currentMissionTargets.Contains(ventureName))
					{
						this.remainingQueue.RemoveAt(i);
						this.AddUnlockView(unlock);
						return;
					}
				}
				else if (!this.currentMissionTargets.Contains("all"))
				{
					this.remainingQueue.RemoveAt(i);
					this.AddUnlockView(unlock);
					return;
				}
			}
			Unlock data = this.remainingQueue[0];
			this.remainingQueue.RemoveAt(0);
			this.AddUnlockView(data);
		}
	}

	// Token: 0x06000C32 RID: 3122 RVA: 0x000371E0 File Offset: 0x000353E0
	private void AddUnlockView(Unlock data)
	{
		RootUIUnlockMissionListItemView rootUIUnlockMissionListItemView = Object.Instantiate<RootUIUnlockMissionListItemView>(this.listItemPrefab, this.listRoot, false);
		rootUIUnlockMissionListItemView.gameObject.SetActive(true);
		rootUIUnlockMissionListItemView.WireData(data);
		this.currentUnlockMissions.Add(data);
		this.viewsByUnlock.Add(data, rootUIUnlockMissionListItemView);
		string targetId;
		if (data is SingleVentureUnlock)
		{
			targetId = (data as SingleVentureUnlock).ventureName;
		}
		else
		{
			targetId = "all";
		}
		this.currentMissionTargets.Add(targetId);
		(from x in data.Claimed
		where x
		select x).Take(1).Subscribe(delegate(bool claimed)
		{
			this.currentMissionTargets.Remove(targetId);
			this.currentUnlockMissions.Remove(data);
			Component component = this.viewsByUnlock[data];
			this.viewsByUnlock.Remove(data);
			Object.DestroyImmediate(component.gameObject);
			this.PopulateNextItem();
			this.UpdateShouldBeShown();
		}).AddTo(this.stateDisposables);
	}

	// Token: 0x06000C33 RID: 3123 RVA: 0x000372E2 File Offset: 0x000354E2
	private void UpdateShouldBeShown()
	{
		if (this.currentUnlockMissions.Count == 0 && this.remainingQueue.Count == 0)
		{
			this.ShouldBeShown.Value = false;
			return;
		}
		this.ShouldBeShown.Value = true;
	}

	// Token: 0x06000C34 RID: 3124 RVA: 0x00037318 File Offset: 0x00035518
	public int Compare(Unlock x, Unlock y)
	{
		bool flag = false;
		bool flag2 = false;
		double num;
		if (x is SingleVentureUnlock)
		{
			num = this.CalculateSingleVentureUnlockCost(x as SingleVentureUnlock);
			flag = true;
		}
		else
		{
			num = this.CalculateEveryVentureUnlockCost(x as EveryVentureUnlock);
		}
		double num2;
		if (y is SingleVentureUnlock)
		{
			num2 = this.CalculateSingleVentureUnlockCost(y as SingleVentureUnlock);
			flag2 = true;
		}
		else
		{
			num2 = this.CalculateEveryVentureUnlockCost(y as EveryVentureUnlock);
		}
		if (num == num2)
		{
			if (flag && !flag2)
			{
				return -1;
			}
			if (!flag && flag2)
			{
				return 1;
			}
			return 0;
		}
		else
		{
			if (num >= num2)
			{
				return 1;
			}
			return -1;
		}
	}

	// Token: 0x06000C35 RID: 3125 RVA: 0x00037394 File Offset: 0x00035594
	private double CalculateSingleVentureUnlockCost(SingleVentureUnlock unlock)
	{
		return this.state.VentureModels.FirstOrDefault((VentureModel x) => x.Name == unlock.ventureName).CalculateCostFromZeroForUnlock((double)unlock.amountToEarn);
	}

	// Token: 0x06000C36 RID: 3126 RVA: 0x000373DC File Offset: 0x000355DC
	private double CalculateEveryVentureUnlockCost(EveryVentureUnlock unlock)
	{
		double num = 0.0;
		for (int i = 0; i < this.state.VentureModels.Count; i++)
		{
			double num2 = this.state.VentureModels[i].CalculateCostFromZeroForUnlock((double)unlock.amountToEarn);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	// Token: 0x04000A63 RID: 2659
	[SerializeField]
	private RootUIUnlockMissionListItemView listItemPrefab;

	// Token: 0x04000A64 RID: 2660
	[SerializeField]
	private RectTransform listRoot;

	// Token: 0x04000A65 RID: 2661
	[SerializeField]
	private Button btn_openUnlocks;

	// Token: 0x04000A66 RID: 2662
	public ReactiveProperty<bool> ShouldBeShown = new ReactiveProperty<bool>();

	// Token: 0x04000A67 RID: 2663
	private List<string> currentMissionTargets = new List<string>();

	// Token: 0x04000A68 RID: 2664
	private List<Unlock> currentUnlockMissions = new List<Unlock>();

	// Token: 0x04000A69 RID: 2665
	private Dictionary<Unlock, RootUIUnlockMissionListItemView> viewsByUnlock = new Dictionary<Unlock, RootUIUnlockMissionListItemView>();

	// Token: 0x04000A6A RID: 2666
	private List<Unlock> remainingQueue;

	// Token: 0x04000A6B RID: 2667
	private CompositeDisposable stateDisposables = new CompositeDisposable();

	// Token: 0x04000A6C RID: 2668
	private GameState state;
}
