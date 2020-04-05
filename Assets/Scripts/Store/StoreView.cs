using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdCap.Store;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000258 RID: 600
public class StoreView : PanelBaseClass
{
	// Token: 0x060010CB RID: 4299 RVA: 0x0004D26C File Offset: 0x0004B46C
	public override void Init(ModalController modalController)
	{
		this.OnClickActionCached = new Action<AdCapStoreItem>(this.OnItemClicked);
		this.storeService = GameController.Instance.StoreService;
		if (this.handleOrientation)
		{
			OrientationController.Instance.OrientationStream.Subscribe(new Action<OrientationChangedEvent>(this.HandleOrientationChanged)).AddTo(base.gameObject);
		}
		for (int i = 0; i < this.storeService.CurrentCatalog.Count; i++)
		{
			this.OnItemAdded(this.storeService.CurrentCatalog[i]);
		}
		(from add in this.storeService.CurrentCatalog.ObserveAdd()
		select add.Value).Subscribe(new Action<AdCapStoreItem>(this.OnItemAdded)).AddTo(base.gameObject);
		(from remove in this.storeService.CurrentCatalog.ObserveRemove()
		select remove.Value).Subscribe(new Action<AdCapStoreItem>(this.OnItemRemoved)).AddTo(base.gameObject);
		base.Init(modalController);
	}

	// Token: 0x060010CC RID: 4300 RVA: 0x0004D3A5 File Offset: 0x0004B5A5
	public void ScrollTo(string itemId)
	{
		base.StartCoroutine(this.InternalScrollTo(itemId));
	}

	// Token: 0x060010CD RID: 4301 RVA: 0x0004D3B5 File Offset: 0x0004B5B5
	private IEnumerator InternalScrollTo(string itemId)
	{
		RectTransform target = null;
		for (int i = 0; i < this.scrollRect.content.childCount; i++)
		{
			Transform child = this.scrollRect.content.GetChild(i);
			if (child.name == itemId)
			{
				target = (RectTransform)child;
				break;
			}
		}
		if (target == null)
		{
			yield break;
		}
		yield return new WaitForSeconds(0.25f);
		object obj = (this.scrollRect.viewport != null) ? this.scrollRect.viewport : ((RectTransform)this.scrollRect.transform);
		Vector3[] fourCornersArray = new Vector3[4];
		object obj2 = obj;
		obj2.GetWorldCorners(fourCornersArray);
		Rect rect = ((RectTransform)obj2.transform).rect;
		float num = Mathf.Abs(this.scrollRect.content.rect.height) - Mathf.Abs(rect.height);
		float num2 = Mathf.Abs(target.localPosition.y);
		float targetNormalizedPosition = Mathf.Clamp01(1f - num2 / num);
		while (!Input.GetMouseButton(0))
		{
			float verticalNormalizedPosition = this.scrollRect.verticalNormalizedPosition;
			if (Mathf.Abs(verticalNormalizedPosition - targetNormalizedPosition) <= 0.001f)
			{
				this.scrollRect.verticalNormalizedPosition = targetNormalizedPosition;
				break;
			}
			float verticalNormalizedPosition2 = Mathf.Lerp(verticalNormalizedPosition, targetNormalizedPosition, Time.deltaTime * 5f);
			this.scrollRect.verticalNormalizedPosition = verticalNormalizedPosition2;
			yield return null;
		}
		yield break;
	}

	// Token: 0x060010CE RID: 4302 RVA: 0x0004D3CC File Offset: 0x0004B5CC
	private void HandleOrientationChanged(OrientationChangedEvent orientation)
	{
		if (orientation.IsPortrait)
		{
			this.scrollRect.content = ((this.tform_PortraitScroll != null) ? this.tform_PortraitScroll : this.tform_PortraitContent);
			this.cnvGrp_Portrait.alpha = 1f;
			this.cnvGrp_Portrait.blocksRaycasts = true;
			this.cnvGrp_Portrait.interactable = true;
			this.cnvGrp_Landscape.alpha = 0f;
			this.cnvGrp_Landscape.blocksRaycasts = false;
			this.cnvGrp_Landscape.interactable = false;
			return;
		}
		this.scrollRect.content = ((this.tform_LandscapeScroll != null) ? this.tform_LandscapeScroll : this.tform_LandscapeContent);
		this.cnvGrp_Portrait.alpha = 0f;
		this.cnvGrp_Portrait.blocksRaycasts = false;
		this.cnvGrp_Portrait.interactable = false;
		this.cnvGrp_Landscape.alpha = 1f;
		this.cnvGrp_Landscape.blocksRaycasts = true;
		this.cnvGrp_Landscape.interactable = true;
	}

	// Token: 0x060010CF RID: 4303 RVA: 0x0004D4D0 File Offset: 0x0004B6D0
	private void OnItemAdded(AdCapStoreItem item)
	{
		if (this.StoreIds.Contains(item.StoreId))
		{
			if (this.portraitPrefabMap.ContainsKey(item.StoreItemType))
			{
				GameObject prefab = this.portraitPrefabMap[item.StoreItemType];
				this.CreateItemView(item, prefab, this.tform_PortraitContent);
			}
			if (this.landscapePrefabMap.ContainsKey(item.StoreItemType))
			{
				GameObject prefab2 = this.landscapePrefabMap[item.StoreItemType];
				this.CreateItemView(item, prefab2, this.tform_LandscapeContent);
			}
			this.UpdateSortOrder();
		}
	}

	// Token: 0x060010D0 RID: 4304 RVA: 0x0004D55C File Offset: 0x0004B75C
	private void UpdateSortOrder()
	{
		this.itemViewList = (from view in this.itemViewList
		orderby view.Item.DisplayPriority
		select view).ToList<StoreItemViewBase>();
		for (int i = 0; i < this.itemViewList.Count; i++)
		{
			this.itemViewList[i].transform.SetAsLastSibling();
		}
	}

	// Token: 0x060010D1 RID: 4305 RVA: 0x0004D5CC File Offset: 0x0004B7CC
	private void CreateItemView(AdCapStoreItem item, GameObject prefab, Transform tForm)
	{
		GameObject gameObject = Object.Instantiate<GameObject>(prefab, tForm, false);
		if (null != gameObject)
		{
			StoreItemViewBase component = gameObject.GetComponent<StoreItemViewBase>();
			if (null != component)
			{
				gameObject.name = item.DisplayName;
				component.WireData(item, this.OnClickActionCached);
				this.itemViewList.Add(component);
			}
		}
	}

	// Token: 0x060010D2 RID: 4306 RVA: 0x0004D620 File Offset: 0x0004B820
	private void OnItemClicked(AdCapStoreItem item)
	{
		this.SendStoreAnalytics(item.Id, "buy");
		this.storeService.AttemptPurchase(item, item.Cost.Currency);
	}

	// Token: 0x060010D3 RID: 4307 RVA: 0x0004D64B File Offset: 0x0004B84B
	public void SendStoreAnalytics(string bundleId, string action)
	{
		GameController.Instance.AnalyticService.SendNavActionAnalytics(bundleId, GameController.Instance.game.planetName + "_" + this.uniqueStoreId, action);
	}

	// Token: 0x060010D4 RID: 4308 RVA: 0x0004D680 File Offset: 0x0004B880
	private void OnItemRemoved(AdCapStoreItem item)
	{
		List<StoreItemViewBase> list = (from i in this.itemViewList
		where i.Item.Id == item.Id
		select i).ToList<StoreItemViewBase>();
		for (int j = 0; j < list.Count; j++)
		{
			this.itemViewList.Remove(list[j]);
			Object.Destroy(list[j].gameObject);
		}
		this.UpdateSortOrder();
	}

	// Token: 0x04000E67 RID: 3687
	private const float EPSILON = 0.001f;

	// Token: 0x04000E68 RID: 3688
	[SerializeField]
	private string uniqueStoreId;

	// Token: 0x04000E69 RID: 3689
	[SerializeField]
	private List<string> StoreIds;

	// Token: 0x04000E6A RID: 3690
	[SerializeField]
	private RectTransform tform_PortraitContent;

	// Token: 0x04000E6B RID: 3691
	[SerializeField]
	private RectTransform tform_LandscapeContent;

	// Token: 0x04000E6C RID: 3692
	[SerializeField]
	private RectTransform tform_PortraitScroll;

	// Token: 0x04000E6D RID: 3693
	[SerializeField]
	private RectTransform tform_LandscapeScroll;

	// Token: 0x04000E6E RID: 3694
	[SerializeField]
	private CanvasGroup cnvGrp_Portrait;

	// Token: 0x04000E6F RID: 3695
	[SerializeField]
	private CanvasGroup cnvGrp_Landscape;

	// Token: 0x04000E70 RID: 3696
	[SerializeField]
	private ScrollRect scrollRect;

	// Token: 0x04000E71 RID: 3697
	[SerializeField]
	private StoreItemTypeGameObjectDictionary portraitPrefabMap = new StoreItemTypeGameObjectDictionary();

	// Token: 0x04000E72 RID: 3698
	[SerializeField]
	private StoreItemTypeGameObjectDictionary landscapePrefabMap = new StoreItemTypeGameObjectDictionary();

	// Token: 0x04000E73 RID: 3699
	[SerializeField]
	private bool handleOrientation;

	// Token: 0x04000E74 RID: 3700
	private List<StoreItemViewBase> itemViewList = new List<StoreItemViewBase>();

	// Token: 0x04000E75 RID: 3701
	private IStoreService storeService;

	// Token: 0x04000E76 RID: 3702
	private Action<AdCapStoreItem> OnClickActionCached;

	// Token: 0x02000909 RID: 2313
	public struct PrefabData
	{
		// Token: 0x04002D0B RID: 11531
		public GameObject Prefab;

		// Token: 0x04002D0C RID: 11532
		public List<Product> Products;

		// Token: 0x04002D0D RID: 11533
		public List<string> Ids;
	}
}
