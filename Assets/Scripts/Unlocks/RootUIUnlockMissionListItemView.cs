using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200017D RID: 381
public class RootUIUnlockMissionListItemView : MonoBehaviour
{
	// Token: 0x06000C21 RID: 3105 RVA: 0x00036908 File Offset: 0x00034B08
	public void Start()
	{
		if (this.currentUnlock == null)
		{
			this.go_activeRoot.SetActive(false);
			this.go_claimRoot.SetActive(false);
		}
		GameController.Instance.IsInitialized.First((bool x) => x).Subscribe(new Action<bool>(this.OnGameControllerInitiliazed)).AddTo(base.gameObject);
	}

	// Token: 0x06000C22 RID: 3106 RVA: 0x00036980 File Offset: 0x00034B80
	private void OnGameControllerInitiliazed(bool isInit)
	{
		GameController.Instance.AngelService.AngelResetCount.Subscribe(delegate(int _)
		{
			this.OnReset();
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000C23 RID: 3107 RVA: 0x000369AE File Offset: 0x00034BAE
	private void OnReset()
	{
		if (this.currentUnlock != null)
		{
			this.WireData(this.currentUnlock);
		}
	}

	// Token: 0x06000C24 RID: 3108 RVA: 0x000369C4 File Offset: 0x00034BC4
	public void WireData(Unlock u)
	{
		this.unlockDisposables.Clear();
		this.currentUnlock = u;
		if (this.currentUnlock is SingleVentureUnlock)
		{
			SingleVentureUnlock single = this.currentUnlock as SingleVentureUnlock;
			GameController.Instance.game.VentureModels.FirstOrDefault((VentureModel x) => x.Name == single.ventureName).TotalOwned.Subscribe(delegate(double owned)
			{
				this.progressFill.fillAmount = (float)(owned / (double)this.currentUnlock.amountToEarn);
				this.txt_progressText.text = owned + "/" + this.currentUnlock.amountToEarn;
			}).AddTo(this.unlockDisposables);
		}
		else
		{
			int total = GameController.Instance.game.VentureModels.Count;
			(from x in (from x in GameController.Instance.game.VentureModels
			select x.TotalOwned).CombineLatest<double>()
			select new Tuple<double, double>(x.Sum((double y) => Math.Min(y, (double)this.currentUnlock.amountToEarn)), Math.Min(x.Min(), (double)this.currentUnlock.amountToEarn))).Subscribe(delegate(Tuple<double, double> pair)
			{
				this.progressFill.fillAmount = (float)(pair.Item1 / (double)(this.currentUnlock.amountToEarn * total));
				this.txt_progressText.text = pair.Item2 + "/" + this.currentUnlock.amountToEarn;
			}).AddTo(this.unlockDisposables);
		}
		this.ShowReward(this.currentUnlock);
		this.img_icon.sprite = MainUIController.instance.GetUnlockSprite(this.currentUnlock);
		if (GameController.Instance.game.IsEventPlanet)
		{
			this.img_icon.rectTransform.localScale = new Vector3(0.6f, 0.6f, 0f);
		}
		else
		{
			this.img_icon.rectTransform.localScale = new Vector3(1f, 1f, 0f);
		}
		this.txt_objective.text = this.currentUnlock.Goal(GameController.Instance.game);
		u.Earned.CombineLatest(u.Claimed, delegate(bool earned, bool claimed)
		{
			if (!earned)
			{
				this.go_activeRoot.SetActive(true);
				this.go_claimRoot.SetActive(false);
			}
			else if (!claimed)
			{
				this.go_activeRoot.SetActive(false);
				this.go_claimRoot.SetActive(true);
				this.unlockDisposables.Clear();
			}
			else
			{
				this.go_activeRoot.SetActive(false);
				this.go_claimRoot.SetActive(false);
			}
			return Unit.Default;
		}).Subscribe<Unit>().AddTo(base.gameObject);
		this.btn_claim.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			GameController.Instance.UnlockService.ClaimUnlock(this.currentUnlock);
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000C25 RID: 3109 RVA: 0x00036BD3 File Offset: 0x00034DD3
	private void OnDestroy()
	{
		this.unlockDisposables.Dispose();
	}

	// Token: 0x06000C26 RID: 3110 RVA: 0x00036BE0 File Offset: 0x00034DE0
	private void ShowReward(Unlock unlock)
	{
		if (unlock == null)
		{
			this.img_currentRewards.ForEach(delegate(Image x)
			{
				x.gameObject.SetActive(false);
			});
			return;
		}
		this.img_currentRewards.ForEach(delegate(Image img)
		{
			img.gameObject.SetActive(true);
		});
		if (unlock.Reward is UnlockRewardBadge)
		{
			Item item = GameController.Instance.GlobalPlayerData.inventory.GetItemById((unlock.Reward as UnlockRewardBadge).badgeId);
			Action<Image> <>9__5;
			(from v in GameController.Instance.IconService.AreBadgesLoaded
			where v
			select v).Subscribe(delegate(bool x)
			{
				List<Image> list = this.img_currentRewards;
				Action<Image> action;
				if ((action = <>9__5) == null)
				{
					action = (<>9__5 = delegate(Image img)
					{
						img.sprite = GameController.Instance.IconService.GetBadgeIcon(item.IconName);
					});
				}
				list.ForEach(action);
			}).AddTo(base.gameObject);
			return;
		}
		if (unlock.Reward is UnlockRewardItemizationItem)
		{
			Item item = GameController.Instance.GlobalPlayerData.inventory.GetItemById((unlock.Reward as UnlockRewardItemizationItem).rewardData.Id);
			Action<Image> <>9__8;
			(from v in GameController.Instance.IconService.AreBadgesLoaded
			where v
			select v).Subscribe(delegate(bool x)
			{
				List<Image> list = this.img_currentRewards;
				Action<Image> action;
				if ((action = <>9__8) == null)
				{
					action = (<>9__8 = delegate(Image img)
					{
						img.sprite = GameController.Instance.IconService.GetBadgeIcon(item.IconName);
					});
				}
				list.ForEach(action);
			}).AddTo(base.gameObject);
			return;
		}
		if (unlock.Reward is UnlockRewardGold)
		{
			Item item = GameController.Instance.GlobalPlayerData.inventory.GetItemById("gold");
			this.img_currentRewards.ForEach(delegate(Image img)
			{
				img.sprite = Resources.Load<Sprite>(item.GetPathToIcon());
			});
			return;
		}
		if (unlock.Reward is UnlockRewardMegaBucks)
		{
			Item item = GameController.Instance.GlobalPlayerData.inventory.GetItemById("megabucks");
			this.img_currentRewards.ForEach(delegate(Image img)
			{
				img.sprite = Resources.Load<Sprite>(item.GetPathToIcon());
			});
			return;
		}
		this.img_currentRewards.ForEach(delegate(Image img)
		{
			img.gameObject.SetActive(false);
		});
	}

	// Token: 0x04000A57 RID: 2647
	[SerializeField]
	private GameObject go_activeRoot;

	// Token: 0x04000A58 RID: 2648
	[SerializeField]
	private Image img_icon;

	// Token: 0x04000A59 RID: 2649
	[SerializeField]
	private Text txt_objective;

	// Token: 0x04000A5A RID: 2650
	[SerializeField]
	private HHSlicedFilledImage progressFill;

	// Token: 0x04000A5B RID: 2651
	[SerializeField]
	private Text txt_progressText;

	// Token: 0x04000A5C RID: 2652
	[SerializeField]
	private List<Image> img_currentRewards;

	// Token: 0x04000A5D RID: 2653
	[SerializeField]
	private GameObject go_claimRoot;

	// Token: 0x04000A5E RID: 2654
	[SerializeField]
	private Button btn_claim;

	// Token: 0x04000A5F RID: 2655
	private ReactiveProperty<double> currentMax = new ReactiveProperty<double>(0.0);

	// Token: 0x04000A60 RID: 2656
	private Unlock currentUnlock;

	// Token: 0x04000A61 RID: 2657
	private CompositeDisposable unlockDisposables = new CompositeDisposable();

	// Token: 0x04000A62 RID: 2658
	private bool isinitialized;
}
