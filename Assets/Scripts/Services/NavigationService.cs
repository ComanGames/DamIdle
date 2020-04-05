using System;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using Platforms.Logger;
using UniRx;
using UnityEngine;

// Token: 0x020000BB RID: 187
public class NavigationService : INavigationService
{
	// Token: 0x06000502 RID: 1282 RVA: 0x00019DE0 File Offset: 0x00017FE0
	public NavigationService()
	{
		this.logger = Platforms.Logger.Logger.GetLogger(this);
		this.tintModalPrefab = Resources.Load<GameObject>("Prefabs/Modals/Tint");
	}

	// Token: 0x06000503 RID: 1283 RVA: 0x00019E74 File Offset: 0x00018074
	public void Init(Transform navParent)
	{
		this.tform_MainCanvas = navParent;
		for (int i = 0; i < this.activeModals.Count; i++)
		{
			if (null != this.activeModals[i])
			{
				Object.Destroy(this.activeModals[i]);
			}
		}
		foreach (KeyValuePair<ModalBase, ModalBase> keyValuePair in this.tintsByModal)
		{
			Object.Destroy(keyValuePair.Value);
		}
		this.activeModals.Clear();
		this.pendingModals.Clear();
		this.tintsByModal.Clear();
		this.drawOrderIndex = 0;
		Observable.EveryUpdate().Subscribe(new Action<long>(this.BackButtonCheck)).AddTo(this.disposables);
	}

	// Token: 0x06000504 RID: 1284 RVA: 0x00019F58 File Offset: 0x00018158
	public void OnOpenMenu()
	{
		this.CurrentLocation.Value = "Menu";
	}

	// Token: 0x06000505 RID: 1285 RVA: 0x00019F6A File Offset: 0x0001816A
	public void OnCloseMenu()
	{
		this.CurrentLocation.Value = ((this.activeModals.Count == 0) ? "Root" : this.activeModals[0].GetLocation());
	}

	// Token: 0x06000506 RID: 1286 RVA: 0x00019F9C File Offset: 0x0001819C
	public void AddBackButtonObservable(ReactiveProperty<bool> arg)
	{
		this.canUseBackButton.Add(arg);
	}

	// Token: 0x06000507 RID: 1287 RVA: 0x00019FAA File Offset: 0x000181AA
	public void RemoveBackButtonObservable(ReactiveProperty<bool> arg)
	{
		this.canUseBackButton.Remove(arg);
	}

	// Token: 0x06000508 RID: 1288 RVA: 0x00019FB9 File Offset: 0x000181B9
	public string GetCurrentModalName()
	{
		if (this.activeModals.Count <= 0)
		{
			return "";
		}
		return this.activeModals[this.activeModals.Count - 1].name;
	}

	// Token: 0x06000509 RID: 1289 RVA: 0x00019FEC File Offset: 0x000181EC
	private void BackButtonCheck(long t)
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			if (this.activeModals.Count == 0)
			{
				Debug.Log("Quitting game");
				Application.Quit();
				return;
			}
			if (this.activeModals[0].AllowBackButtonToClose)
			{
				if (this.canUseBackButton.All((ReactiveProperty<bool> x) => x.Value))
				{
					this.CloseModal();
				}
			}
		}
	}

	// Token: 0x0600050A RID: 1290 RVA: 0x0001A064 File Offset: 0x00018264
	public void CloseModal()
	{
		if (this.activeModals.Count > 0)
		{
			this.activeModals.Last<ModalBase>().CloseModal(Unit.Default);
		}
	}

	// Token: 0x0600050B RID: 1291 RVA: 0x0001A089 File Offset: 0x00018289
	public void CloseAllModals()
	{
		while (this.activeModals.Count > 0)
		{
			this.CloseModal();
		}
	}

	// Token: 0x0600050C RID: 1292 RVA: 0x0001A0A4 File Offset: 0x000182A4
	public T CreateModal<T>(NavModalVO<T> navModalVo, bool canStackOveride = false) where T : ModalBase
	{
		T t = Resources.Load<T>(navModalVo.ResourcePath + navModalVo.Name);
		GameObject gameObject = this.CreateModal(t.gameObject, canStackOveride);
		if (!(null == gameObject))
		{
			return gameObject.GetComponent<T>();
		}
		return default(T);
	}

	// Token: 0x0600050D RID: 1293 RVA: 0x0001A0F4 File Offset: 0x000182F4
	private GameObject CreateModal(GameObject prefab, bool canStackOveride = false)
	{
		if (prefab == null)
		{
			this.logger.Error("Attempting to create a modal from a null prefab");
			return null;
		}
		ModalBase component = prefab.GetComponent<ModalBase>();
		if (null == component)
		{
			Debug.LogError("Modal \"" + prefab.name + "\" Does not extend ModalBase and could not be created");
			return null;
		}
		if (!component.AllowDuplicateModals)
		{
			ModalBase modalBase = this.GetExistingModal(prefab, this.activeModals);
			modalBase = (modalBase ?? this.GetExistingModal(prefab, this.pendingModals.ToList<ModalBase>()));
			if (null != modalBase)
			{
				return modalBase.gameObject;
			}
		}
		this.logger.Debug("Creating [{0}]", new object[]
		{
			prefab.name
		});
		GameObject gameObject = Object.Instantiate<GameObject>(prefab, this.tform_MainCanvas, false);
		gameObject.name = prefab.name;
		ModalBase component2 = gameObject.GetComponent<ModalBase>();
		component2.CanStack = canStackOveride;
		component2.Init(this);
		return gameObject;
	}

	// Token: 0x0600050E RID: 1294 RVA: 0x0001A1D8 File Offset: 0x000183D8
	public T CreateModal<T, P>(NavModalVOWithParams<T, P> navModalVo, P modalParams, bool canStackOveride = false) where T : ModalBase, IModalWithParams<P>
	{
		T t = Resources.Load<T>(navModalVo.ResourcePath + navModalVo.Name);
		GameObject gameObject = this.CreateModal<P>(t.gameObject, modalParams, canStackOveride);
		if (!(null == gameObject))
		{
			return gameObject.GetComponent<T>();
		}
		return default(T);
	}

	// Token: 0x0600050F RID: 1295 RVA: 0x0001A22C File Offset: 0x0001842C
	private GameObject CreateModal<P>(GameObject prefab, P modalParams, bool canStackOveride = false)
	{
		if (prefab == null)
		{
			this.logger.Error("Attempting to create a modal from a null prefab");
			return null;
		}
		ModalBase component = prefab.GetComponent<ModalBase>();
		if (null == component)
		{
			Debug.LogError("Modal \"" + prefab.name + "\" Does not extend ModalBase and could not be created");
			return null;
		}
		if (!component.AllowDuplicateModals)
		{
			ModalBase modalBase = this.GetExistingModal(prefab, this.activeModals);
			modalBase = (modalBase ?? this.GetExistingModal(prefab, this.pendingModals.ToList<ModalBase>()));
			if (null != modalBase)
			{
				return modalBase.gameObject;
			}
		}
		this.logger.Debug("Creating [{0}]", new object[]
		{
			prefab.name
		});
		GameObject gameObject = Object.Instantiate<GameObject>(prefab, this.tform_MainCanvas, false);
		gameObject.name = prefab.name;
		ModalBase component2 = gameObject.GetComponent<ModalBase>();
		component2.CanStack = (component2.CanStack || canStackOveride);
		gameObject.GetComponent<IModalWithParams<P>>().SetParams(modalParams);
		component2.Init(this);
		return gameObject;
	}

	// Token: 0x06000510 RID: 1296 RVA: 0x0001A320 File Offset: 0x00018520
	public void RegisterModal(ModalBase modal)
	{
		if (modal == null)
		{
			this.logger.Error("Modal is null, ignoring registration.");
			return;
		}
		if (this.activeModals.Count == 0 || modal.CanStack)
		{
			if (!modal.IgnoreTint && (this.activeModals.Count == 0 || modal.CanStack))
			{
				this.CreateTint(modal);
			}
			Canvas component = modal.gameObject.GetComponent<Canvas>();
			component.renderMode = RenderMode.WorldSpace;
			component.overrideSorting = true;
			int num = 20;
			int num2 = this.drawOrderIndex;
			this.drawOrderIndex = num2 + 1;
			component.sortingOrder = num + num2;
			this.activeModals.Add(modal);
			modal.OnModalRegistered();
			this.UpdateCurrentModalName(modal);
			this.logger.Trace("Registering [{0}]", new object[]
			{
				modal.name
			});
			return;
		}
		this.pendingModals.Enqueue(modal);
		this.logger.Trace("Queueing [{0}]", new object[]
		{
			modal.name
		});
	}

	// Token: 0x06000511 RID: 1297 RVA: 0x0001A41C File Offset: 0x0001861C
	private void UpdateCurrentModalName(ModalBase modal)
	{
		ModalAnimatorBase component = modal.GetComponent<ModalAnimatorBase>();
		if (null != component)
		{
			(from x in component.OnAnimFinished
			where x == "Intro"
			select x).Take(1).Subscribe(delegate(string _)
			{
				this.CurrentLocation.Value = modal.GetLocation();
			}).AddTo(modal.gameObject);
			return;
		}
		this.CurrentLocation.Value = modal.GetLocation();
	}

	// Token: 0x06000512 RID: 1298 RVA: 0x0001A4BC File Offset: 0x000186BC
	public void UnregisterModal(ModalBase modal)
	{
		this.logger.Trace("Unregistering [{0}]", new object[]
		{
			modal.name
		});
		if (this.pendingModals.Contains(modal))
		{
			this.pendingModals = new Queue<ModalBase>(from x in this.pendingModals
			where x != modal
			select x);
		}
		if (this.activeModals.Remove(modal))
		{
			if (this.pendingModals.Count > 0)
			{
				this.RegisterModal(this.pendingModals.Dequeue());
			}
			else
			{
				this.CurrentLocation.Value = ((this.activeModals.Count > 0) ? this.activeModals[this.activeModals.Count - 1].GetLocation() : "Root");
				this.drawOrderIndex = ((this.activeModals.Count == 0) ? 0 : this.drawOrderIndex);
			}
			this.RemoveTint(modal);
		}
	}

	// Token: 0x06000513 RID: 1299 RVA: 0x0001A5CC File Offset: 0x000187CC
	private void CreateTint(ModalBase modal)
	{
		GameObject gameObject;
		if (this.pooledTints.Count == 0)
		{
			gameObject = Object.Instantiate<GameObject>(this.tintModalPrefab, this.tform_MainCanvas, false);
		}
		else
		{
			ModalBase modalBase = this.pooledTints.Pop();
			gameObject = modalBase.gameObject;
			modalBase.transform.SetParent(this.tform_MainCanvas, false);
		}
		this.logger.Debug("Creating TintModal for modal " + modal.name);
		gameObject.SetActive(true);
		gameObject.name = this.tintModalPrefab.name;
		Canvas component = gameObject.GetComponent<Canvas>();
		int num = 20;
		int num2 = this.drawOrderIndex;
		this.drawOrderIndex = num2 + 1;
		component.sortingOrder = num + num2;
		ModalBase component2 = gameObject.GetComponent<ModalBase>();
		component2.NavService = this;
		component2.OnModalRegistered();
		component2.name = this.tintModalPrefab.name + "-" + modal.name;
		this.tintsByModal.Add(modal, component2);
	}

	// Token: 0x06000514 RID: 1300 RVA: 0x0001A6B4 File Offset: 0x000188B4
	private void RemoveTint(ModalBase modal)
	{
		ModalBase modalBase = null;
		if (this.tintsByModal.TryGetValue(modal, out modalBase) && null != modalBase)
		{
			this.logger.Debug("Removing TintModal " + modalBase.name);
			this.tintsByModal.Remove(modal);
			modalBase.name = "PooledTint";
			modalBase.gameObject.SetActive(false);
			this.pooledTints.Push(modalBase);
			this.UnregisterModal(modalBase);
		}
	}

	// Token: 0x06000515 RID: 1301 RVA: 0x0001A730 File Offset: 0x00018930
	private ModalBase GetExistingModal(GameObject prefab, List<ModalBase> modals)
	{
		ModalBase result = null;
		for (int i = 0; i < modals.Count; i++)
		{
			if (null == modals[i])
			{
				this.logger.Error("A null modal was found.");
			}
			else if (!(modals[i].name != prefab.name) && (!(modals[i].name == prefab.name) || !modals[i].AllowDuplicateModals))
			{
				result = modals[i];
			}
		}
		return result;
	}

	// Token: 0x04000468 RID: 1128
	public const string ROOT_ID = "Root";

	// Token: 0x04000469 RID: 1129
	public const string MENU_ID = "Menu";

	// Token: 0x0400046A RID: 1130
	public readonly ReactiveProperty<string> CurrentLocation = new ReactiveProperty<string>("Root");

	// Token: 0x0400046B RID: 1131
	public EMenuState MenuState = EMenuState.Closed;

	// Token: 0x0400046C RID: 1132
	private Platforms.Logger.Logger logger;

	// Token: 0x0400046D RID: 1133
	private GameObject tintModalPrefab;

	// Token: 0x0400046E RID: 1134
	private List<ModalBase> activeModals = new List<ModalBase>();

	// Token: 0x0400046F RID: 1135
	private Queue<ModalBase> pendingModals = new Queue<ModalBase>();

	// Token: 0x04000470 RID: 1136
	private Dictionary<ModalBase, ModalBase> tintsByModal = new Dictionary<ModalBase, ModalBase>();

	// Token: 0x04000471 RID: 1137
	private Stack<ModalBase> pooledTints = new Stack<ModalBase>();

	// Token: 0x04000472 RID: 1138
	private const string ROOT_PREFAB_PATH = "Prefabs/Modals/";

	// Token: 0x04000473 RID: 1139
	private const int DRAW_ORDER_BASE = 20;

	// Token: 0x04000474 RID: 1140
	private int drawOrderIndex;

	// Token: 0x04000475 RID: 1141
	private Transform tform_MainCanvas;

	// Token: 0x04000476 RID: 1142
	private List<ReactiveProperty<bool>> canUseBackButton = new List<ReactiveProperty<bool>>
	{
		new ReactiveProperty<bool>(true)
	};

	// Token: 0x04000477 RID: 1143
	private CompositeDisposable disposables = new CompositeDisposable();
}
