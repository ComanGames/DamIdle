using System;
using System.Collections.Generic;
using HHTools.Navigation;
using UniRx;
using UnityEngine;

// Token: 0x0200015E RID: 350
public static class VentureAdapter
{
	// Token: 0x06000B0B RID: 2827 RVA: 0x00031758 File Offset: 0x0002F958
	public static void InitView(VentureModel model, VentureView view, List<Sprite> sprites, VentureViewColorData colorData, bool isEventPlanet, ClampedReactiveDouble cashOnHand, Action<VentureModel> buyVentureAction, Action<double> onFinished, Action<VentureModel> boostVentureAction)
	{
		Material material = view.HHSImg_ProgressBar.material;
		view.HHSImg_ProgressBar.material = new Material(material);
		model.TotalOwned.CombineLatest(model.AchievementTarget.StartWith(0), (double o, int t) => o).Subscribe(delegate(double o)
		{
			if (o <= (double)model.AchievementTarget.Value && GameController.Instance.UnlockService.ShowsUnlockOutOfOnVentureView)
			{
				view.NumOwned.text = o.ToString() + "/" + model.AchievementTarget.Value;
				return;
			}
			view.NumOwned.text = o.ToString();
		}).AddTo(view.gameObject);
		model.ProfitOnNext.Subscribe(delegate(double p)
		{
			string[] array = NumberFormat.Convert(p, 1000000.0, true, 3).Split(new char[]
			{
				' '
			});
			view.Profit.text = array[0];
			view.ProfitNumber.text = ((array.Length == 2) ? array[1] : "");
		}).AddTo(view.gameObject);
		(from x in model.TotalOwned
		where x > 0.0
		select x).Take(1).Subscribe(delegate(double o)
		{
			model.UnlockState.Value = VentureModel.EUnlockState.Purchased;
		}).AddTo(view.gameObject);
		model.TotalOwned.CombineLatest(model.AchievementTarget, (double totalOwned, int achievementTarget) => VentureAdapter.UpdateAchievmentProgress((float)achievementTarget, (float)model.LastAchievementTarget.Value, totalOwned)).Subscribe(delegate(float x)
		{
			model.AchievementProgressBarVal.Value = x;
		}).AddTo(view.gameObject);
		model.AchievementProgressBarVal.Subscribe(delegate(float v)
		{
			if (GameController.Instance.UnlockService.ShowsProgressOnVentureView)
			{
				view.Img_AchievementProgress.fillAmount = v;
				return;
			}
			view.Img_AchievementProgress.fillAmount = 0f;
		}).AddTo(view.gameObject);
		model.TimedBonusMultiplier.Subscribe(delegate(float timed)
		{
			float value = model.EffectiveCoolDownTime.Value;
			if (value < 0.2f && timed > 0f)
			{
				view.HHSImg_ProgressBar.fillAmount = 1f;
				view.HHSImg_ProgressBar.material.SetFloat("_ShowAnimTex", 1f);
				view.HHSImg_ProgressBar.material.SetFloat("_Speed", -4f);
				return;
			}
			if (value < 0.2f)
			{
				view.HHSImg_ProgressBar.fillAmount = 1f;
				view.HHSImg_ProgressBar.material.SetFloat("_ShowAnimTex", 1f);
				view.HHSImg_ProgressBar.material.SetFloat("_Speed", -2f);
				return;
			}
			view.HHSImg_ProgressBar.material.SetFloat("_ShowAnimTex", 0f);
		}).AddTo(view.gameObject);
		Func<float, bool> <>9__34;
		Action<float> <>9__35;
		Func<float, bool> <>9__36;
		Action<float> <>9__37;
		Func<float, bool> <>9__38;
		Action<float> <>9__39;
		model.IsRunning.Subscribe(delegate(bool running)
		{
			view.Img_HighlightFrame.enabled = !running;
			view.RunIconButton.interactable = !running;
			view.RunBarButton.interactable = !running;
			if (running)
			{
				IObservable<float> effectiveCoolDownTime = model.EffectiveCoolDownTime;
				Func<float, bool> predicate;
				if ((predicate = <>9__34) == null)
				{
					predicate = (<>9__34 = ((float _) => model.IsRunning.Value));
				}
				IObservable<float> source = effectiveCoolDownTime.TakeWhile(predicate);
				Action<float> onNext;
				if ((onNext = <>9__35) == null)
				{
					onNext = (<>9__35 = delegate(float time)
					{
						if (time < 0.2f)
						{
							view.HHSImg_ProgressBar.fillAmount = 1f;
							view.HHSImg_ProgressBar.material.SetFloat("_ShowAnimTex", 1f);
							view.HHSImg_ProgressBar.material.SetFloat("_Speed", -2f);
							return;
						}
						view.HHSImg_ProgressBar.material.SetFloat("_ShowAnimTex", 0f);
					});
				}
				source.Subscribe(onNext).AddTo(view.gameObject);
				IObservable<float> progressTimer = model.ProgressTimer;
				Func<float, bool> predicate2;
				if ((predicate2 = <>9__36) == null)
				{
					predicate2 = (<>9__36 = ((float _) => model.IsRunning.Value));
				}
				IObservable<float> source2 = progressTimer.TakeWhile(predicate2);
				Action<float> onNext2;
				if ((onNext2 = <>9__37) == null)
				{
					onNext2 = (<>9__37 = delegate(float progress)
					{
						if (model.EffectiveCoolDownTime.Value >= 0.2f)
						{
							view.HHSImg_ProgressBar.fillAmount = progress / model.EffectiveCoolDownTime.Value;
							TimeSpan timeSpan = new TimeSpan(0, 0, (int)(model.EffectiveCoolDownTime.Value - progress));
							view.RunTimer.text = string.Format("{0:00}:{1:00}:{2:00}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
							return;
						}
						view.RunTimer.text = "00:00:00";
					});
				}
				source2.Subscribe(onNext2).AddTo(view.gameObject);
				return;
			}
			view.HHSImg_ProgressBar.fillAmount = 0f;
			view.HHSImg_ProgressBar.material.SetFloat("_ShowAnimTex", 0f);
			IObservable<float> effectiveCoolDownTime2 = model.EffectiveCoolDownTime;
			Func<float, bool> predicate3;
			if ((predicate3 = <>9__38) == null)
			{
				predicate3 = (<>9__38 = ((float _) => !model.IsRunning.Value));
			}
			IObservable<float> source3 = effectiveCoolDownTime2.TakeWhile(predicate3);
			Action<float> onNext3;
			if ((onNext3 = <>9__39) == null)
			{
				onNext3 = (<>9__39 = delegate(float time)
				{
					if (model.EffectiveCoolDownTime.Value >= 0.2f)
					{
						TimeSpan timeSpan = new TimeSpan(0, 0, (int)(model.EffectiveCoolDownTime.Value - model.ProgressTimer.Value));
						view.RunTimer.text = string.Format("{0:00}:{1:00}:{2:00}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
						return;
					}
					view.RunTimer.text = "00:00:00";
				});
			}
			source3.Subscribe(onNext3).AddTo(view.gameObject);
		}).AddTo(view.gameObject);
		(from _ in view.RunIconButton.OnClickAsObservable().Merge(new IObservable<Unit>[]
		{
			view.RunBarButton.OnClickAsObservable()
		})
		where !model.IsRunning.Value && model.TotalOwned.Value > 0.0
		select _).Subscribe(delegate(Unit _)
		{
			model.Run();
		}).AddTo(view.gameObject);
		Color cantBuyColor;
		ColorUtility.TryParseHtmlString("#B0AB96", out cantBuyColor);
		Color canBuyColor;
		ColorUtility.TryParseHtmlString("#D7D4C5", out canBuyColor);
		model.CostForNext.CombineLatest(GameController.Instance.game.CashOnHand, (double cost, double cash) => cost).CombineLatest(model.CanAfford, (double cost, double bn) => new Tuple<double, double>(cost, bn)).Subscribe(delegate(Tuple<double, double> tuple)
		{
			bool flag = tuple.Item1 > 0.0 && GameController.Instance.game.CashOnHand.Value >= tuple.Item1;
			view.BuyButton.interactable = (view.UnpurchasedStateButton.interactable = flag);
			view.Img_IconUnlocked.color = new Color(view.Img_IconUnlocked.color.r, view.Img_IconUnlocked.color.g, view.Img_IconUnlocked.color.b, flag ? 1f : 0.5f);
			view.Img_UnlockedVentureBG.color = (flag ? canBuyColor : cantBuyColor);
			view.BuyNumber.text = string.Format("Buy\nx{0}", tuple.Item2);
		}).AddTo(view.gameObject);
		model.CostForNext.Subscribe(delegate(double c)
		{
			string[] array = NumberFormat.Convert(c, 1000000.0, true, 3).Split(new char[]
			{
				' '
			});
			view.CostPer.text = array[0];
			view.CostPerNumber.text = ((array.Length == 2) ? array[1] : "");
			view.UnpurchasedCostPer.text = NumberFormat.Convert(c, 1E+15, true, 3);
		}).AddTo(view.gameObject);
		model.ShowCPS.Subscribe(delegate(bool show)
		{
			view.Profit.enabled = !show;
			view.ProfitNumber.enabled = !show;
			view.CashPerSec.enabled = show;
		}).AddTo(view.gameObject);
		model.CashPerSec.Subscribe(delegate(double c)
		{
			string str = NumberFormat.Convert(c, 1000000.0, true, 3);
			view.CashPerSec.text = str + " /sec";
		}).AddTo(view.gameObject);
		model.FinishedRunning.Subscribe(onFinished).AddTo(view.gameObject);
		if (!isEventPlanet)
		{
			view.SetViewColors(colorData);
			(from b in model.IsBoosted
			where b
			select b).Subscribe(delegate(bool _)
			{
				if (GameController.Instance.game.IsEventPlanet || (!GameController.Instance.game.IsEventPlanet && !GameController.Instance.GildingService.AllVenturesBoosted.Value))
				{
					view.Boost();
				}
				VentureAdapter.UpdateBoostedGraphics(model, view, colorData);
			}).AddTo(view.gameObject);
			(from b in GameController.Instance.GildingService.AllVenturesBoosted
			where b
			select b).Subscribe(delegate(bool _)
			{
				if (!GameController.Instance.game.IsEventPlanet)
				{
					view.BoostPlatinum();
				}
				VentureAdapter.UpdateBoostedGraphics(model, view, colorData);
			}).AddTo(view.gameObject);
			model.gildLevel.Subscribe(delegate(int gl)
			{
				VentureAdapter.UpdateBoostedGraphics(model, view, colorData);
			}).AddTo(view.gameObject);
		}
		else
		{
			model.gildLevel.Subscribe(delegate(int gl)
			{
				VentureAdapter.OnGildLevelChanged(gl, model, view, colorData);
			}).AddTo(view.gameObject);
		}
		model.IsProfitBoosted.Subscribe(delegate(bool x)
		{
			VentureAdapter.OnProfitBoosterStateChanged(x, model, view, colorData);
		}).AddTo(view.gameObject);
		(from _ in view.BuyButton.OnClickAsObservable()
		where cashOnHand.Value >= model.CostForNext.Value
		select _).Subscribe(delegate(Unit _)
		{
			buyVentureAction(model);
		}).AddTo(view.gameObject);
		MainUIController.instance.IsBoostedState.Subscribe(delegate(bool b)
		{
			bool active = b && view.PurchasedState.activeSelf && !model.IsBoosted.Value;
			view.BoostPurchaseState.SetActive(active);
		}).AddTo(view.gameObject);
		view.IconAnimator.Init(sprites);
		view.Img_IconLocked.sprite = sprites[0];
		view.Img_IconUnlocked.sprite = sprites[0];
		Action <>9__40;
		(from _ in view.BuyBoostBannerButton.OnClickAsObservable().Merge(new IObservable<Unit>[]
		{
			view.BuyBoostCertificateButton.OnClickAsObservable()
		})
		where !model.IsBoosted.Value
		select _).Subscribe(delegate(Unit _)
		{
			string text = string.Format("Gild {0}?", model.Name);
			string text2 = string.Format("Spend 1 Mega Ticket for a x{0} profit boost?", model.CalculateGildBonus(model.gildLevel.Value + 1));
			PopupModal popupModal = GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false);
			string title = text;
			string body = text2;
			Action okCallback;
			if ((okCallback = <>9__40) == null)
			{
				okCallback = (<>9__40 = delegate()
				{
					boostVentureAction(model);
				});
			}
			popupModal.WireData(title, body, okCallback, PopupModal.PopupOptions.OK_Cancel, "Yes", "No", true, null, "");
		}).AddTo(view.gameObject);
		(from _ in view.UnpurchasedStateButton.OnClickAsObservable()
		where cashOnHand.Value >= model.CostForNext.Value
		select _).Subscribe(delegate(Unit _)
		{
			buyVentureAction(model);
		}).AddTo(view.gameObject);
		(from x in model.BonusEffect
		where !string.IsNullOrEmpty(x)
		select x).Subscribe(delegate(string effect)
		{
			view.BonusEffectAnimator.Play(effect);
		}).AddTo(view.gameObject);
		view.ventureName.text = model.Name;
		view.WireData(model);
	}

	// Token: 0x06000B0C RID: 2828 RVA: 0x00031E47 File Offset: 0x00030047
	private static float UpdateAchievmentProgress(float achievementTarget, float lastAchievementTarget, double totalOwned)
	{
		if (achievementTarget <= 0f)
		{
			return 1f;
		}
		return (float)(totalOwned - (double)lastAchievementTarget) / (achievementTarget - lastAchievementTarget);
	}

	// Token: 0x06000B0D RID: 2829 RVA: 0x00031E60 File Offset: 0x00030060
	private static void OnProfitBoosterStateChanged(bool isBoosted, VentureModel model, VentureView view, VentureViewColorData colorData)
	{
		VentureAdapter.UpdateBoostedGraphics(model, view, colorData);
	}

	// Token: 0x06000B0E RID: 2830 RVA: 0x00031E60 File Offset: 0x00030060
	private static void OnGildLevelChanged(int gl, VentureModel model, VentureView view, VentureViewColorData colorData)
	{
		VentureAdapter.UpdateBoostedGraphics(model, view, colorData);
	}

	// Token: 0x06000B0F RID: 2831 RVA: 0x00031E6C File Offset: 0x0003006C
	private static void UpdateBoostedGraphics(VentureModel model, VentureView view, VentureViewColorData colorData)
	{
		if (model.IsProfitBoosted.Value)
		{
			view.BoostProfitBooster();
			return;
		}
		if (!GameController.Instance.game.IsEventPlanet)
		{
			if (GameController.Instance.GildingService.AllVenturesBoosted.Value)
			{
				view.BoostPlatinum();
				return;
			}
			if (model.IsBoosted.Value)
			{
				view.Boost();
				return;
			}
			view.SetViewColors(colorData);
			return;
		}
		else
		{
			int value = model.gildLevel.Value;
			if (value == 1)
			{
				view.Boost();
				return;
			}
			if (value > 1)
			{
				view.BoostPlatinum();
				return;
			}
			view.SetViewColors(colorData);
			return;
		}
	}
}
