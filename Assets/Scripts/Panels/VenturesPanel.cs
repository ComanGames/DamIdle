using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Token: 0x020000E3 RID: 227
public class VenturesPanel : MonoBehaviour
{
	// Token: 0x06000622 RID: 1570 RVA: 0x00021327 File Offset: 0x0001F527
	private void Start()
	{
		GameController.Instance.State.Subscribe(new Action<GameState>(this.OnStateChanged)).AddTo(base.gameObject);
	}

	// Token: 0x06000623 RID: 1571 RVA: 0x00021350 File Offset: 0x0001F550
	private void OnDestroy()
	{
		this.stateDisposable.Dispose();
	}

	// Token: 0x06000624 RID: 1572 RVA: 0x00021360 File Offset: 0x0001F560
	public void OnStateChanged(GameState state)
	{
		this.stateDisposable.Dispose();
		this.state = state;
		this.isProfitBoosterDockedInPortrait = state.dockPortraitProfitBooster;
		this.profitBoosterRectTransform = null;
		this.ventureTransforms.Clear();
		this.VentureModelViewMap.Clear();
		this.venturesGridLayout.gameObject.SetActive(false);
		if (state.PlanetData.hasBooster)
		{
			GameController.Instance.PlanetThemeService.Booster.Subscribe(delegate(GameObject boosterPrefab)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(boosterPrefab, this.tform_VenturePortrait, false);
				if (gameObject != null)
				{
					ProfitBoosterView component = gameObject.GetComponent<ProfitBoosterView>();
					if (component)
					{
						component.Init();
					}
				}
				else
				{
					Debug.LogError("Failed to load profitbooster");
				}
				this.profitBooster = gameObject.GetComponent<ProfitBoosterView>();
				this.profitBoosterRectTransform = (RectTransform)gameObject.transform;
			}).AddTo(base.gameObject);
		}
		GameController.Instance.PlanetThemeService.VentureAnimatedSprites.Subscribe(delegate(VentureAnimation animations)
		{
			this.state.VentureModels.ToList<VentureModel>().ForEach(delegate(VentureModel x)
			{
				StringSpriteListObjectDictionary ventureAnimatedSprites = animations.VentureAnimatedSprites;
				GameController.Instance.PlanetThemeService.VentureViewColorData.Subscribe(delegate(VentureColors y)
				{
					this.CreateVentureView(x, this.VentureViewPrefab, ventureAnimatedSprites[x.Id].Sprites, y.VentureViewColorData);
				}).AddTo(this.gameObject);
			});
		}).AddTo(base.gameObject);
		this.venturesGridLayout.gameObject.SetActive(true);
		this.stateDisposable = OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.OnOrientationChanged));
		MainUIController.instance.ShowUnlocksMissionBar.Subscribe(delegate(bool _)
		{
			this.OnOrientationChanged(OrientationController.Instance.CurrentOrientation);
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000625 RID: 1573 RVA: 0x00021478 File Offset: 0x0001F678
	private void OnOrientationChanged(OrientationChangedEvent orientation)
	{
		this.scroll_Ventures.enabled = orientation.IsPortrait;
		if (orientation.IsPortrait)
		{
			if (this.profitBooster != null)
			{
				if (this.isProfitBoosterDockedInPortrait)
				{
					this.profitBoosterRectTransform.SetParent(this.tform_boosterPotraitParent);
				}
				else
				{
					this.profitBoosterRectTransform.SetParent(this.tform_VenturePortrait);
				}
			}
			for (int i = 0; i < this.ventureTransforms.Count; i++)
			{
				this.ventureTransforms[i].SetParent(this.tform_VenturePortrait);
			}
		}
		else
		{
			int num = 0;
			if (this.profitBooster != null)
			{
				this.profitBoosterRectTransform.SetAndStretchToParentSize(this.lanscapeVentureParents[0]);
				num++;
			}
			for (int j = 0; j < this.ventureTransforms.Count; j++)
			{
				this.ventureTransforms[j].SetAndStretchToParentSize(this.lanscapeVentureParents[j + num]);
			}
		}
		if (this.isProfitBoosterDockedInPortrait && orientation.IsPortrait && this.profitBooster != null)
		{
			this.go_booster_tint_portrait.SetActive(true);
			((RectTransform)this.scroll_Ventures.transform).offsetMin = new Vector2(((RectTransform)this.scroll_Ventures.transform).offsetMin.x, 130f);
		}
		else
		{
			this.go_booster_tint_portrait.SetActive(false);
			((RectTransform)this.scroll_Ventures.transform).offsetMin = new Vector2(((RectTransform)this.scroll_Ventures.transform).offsetMin.x, 0f);
		}
		RectTransform component = base.GetComponent<RectTransform>();
		if (!MainUIController.instance.ShowUnlocksMissionBar.Value)
		{
			if (GameController.Instance.PlanetMilestoneService.DoesCurrentPlanetHaveMilestones.Value)
			{
				component.offsetMax = new Vector2(component.offsetMax.x, -40f);
			}
			else
			{
				component.offsetMax = new Vector2(component.offsetMax.x, 0f);
			}
		}
		else if (GameController.Instance.PlanetMilestoneService.DoesCurrentPlanetHaveMilestones.Value)
		{
			component.offsetMax = new Vector2(component.offsetMax.x, -170f);
		}
		else
		{
			component.offsetMax = new Vector2(component.offsetMax.x, -130f);
		}
		this.UpdateSizes(orientation.IsPortrait).ToObservable(false).StartAsCoroutine(default(CancellationToken));
	}

	// Token: 0x06000626 RID: 1574 RVA: 0x000216E9 File Offset: 0x0001F8E9
	private IEnumerator UpdateSizes(bool isPortrait)
	{
		yield return null;
		foreach (KeyValuePair<VentureModel, VentureView> keyValuePair in this.VentureModelViewMap)
		{
			keyValuePair.Value.UpdateIconSize(isPortrait);
		}
		if (null != this.profitBooster)
		{
			this.profitBooster.UpdateIconSize();
		}
		yield break;
	}

	// Token: 0x06000627 RID: 1575 RVA: 0x00021700 File Offset: 0x0001F900
	private void CreateVentureView(VentureModel venture, VentureView prefab, List<Sprite> animatedSprites, VentureViewColorData colorData)
	{
		VentureView ventureView = Object.Instantiate<VentureView>(prefab, this.tform_VenturePortrait, false);
		if (null != ventureView)
		{
			ventureView.name = venture.Id;
			VentureAdapter.InitView(venture, ventureView, animatedSprites, colorData, this.state.IsEventPlanet, this.state.CashOnHand, new Action<VentureModel>(this.state.Purchase), new Action<double>(this.state.Finished), new Action<VentureModel>(this.state.BoostVenture));
			this.VentureModelViewMap.Add(venture, ventureView);
			this.ventureTransforms.Add((RectTransform)ventureView.transform);
		}
	}

	// Token: 0x06000628 RID: 1576 RVA: 0x000217AC File Offset: 0x0001F9AC
	private void OnHacksEnabled()
	{
		foreach (KeyValuePair<VentureModel, VentureView> keyValuePair in this.VentureModelViewMap)
		{
			if (keyValuePair.Value.gameObject.GetComponent<VentureViewHack>() == null)
			{
				keyValuePair.Value.gameObject.AddComponent<VentureViewHack>();
			}
		}
	}

	// Token: 0x0400058B RID: 1419
	private bool isProfitBoosterDockedInPortrait;

	// Token: 0x0400058C RID: 1420
	public Dictionary<VentureModel, VentureView> VentureModelViewMap = new Dictionary<VentureModel, VentureView>();

	// Token: 0x0400058D RID: 1421
	[SerializeField]
	private VentureView VentureViewPrefab;

	// Token: 0x0400058E RID: 1422
	[SerializeField]
	private Transform tform_VenturePortrait;

	// Token: 0x0400058F RID: 1423
	[SerializeField]
	private Transform tform_boosterPotraitParent;

	// Token: 0x04000590 RID: 1424
	[SerializeField]
	private GameObject go_booster_tint_portrait;

	// Token: 0x04000591 RID: 1425
	[SerializeField]
	private List<RectTransform> lanscapeVentureParents = new List<RectTransform>();

	// Token: 0x04000592 RID: 1426
	[SerializeField]
	private GridLayoutGroup venturesGridLayout;

	// Token: 0x04000593 RID: 1427
	[SerializeField]
	private ScrollRect scroll_Ventures;

	// Token: 0x04000594 RID: 1428
	private GameState state;

	// Token: 0x04000595 RID: 1429
	private List<RectTransform> ventureTransforms = new List<RectTransform>();

	// Token: 0x04000596 RID: 1430
	private ProfitBoosterView profitBooster;

	// Token: 0x04000597 RID: 1431
	private RectTransform profitBoosterRectTransform;

	// Token: 0x04000598 RID: 1432
	private IDisposable stateDisposable = Disposable.Empty;
}
