using System;
using System.Collections.Generic;
using AdComm;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

// Token: 0x02000200 RID: 512
public class PlanetThemeService
{
	// Token: 0x06000ED9 RID: 3801 RVA: 0x00043368 File Offset: 0x00041568
	public PlanetThemeService()
	{
		GameController.Instance.OnLoadNewPlanetPre += this.OnLoadNewPlanetPre;
		this.stateDisplosables.Add(this.AngelImageCache);
		this.stateDisplosables.Add(this.BackgroundImageCache);
		this.stateDisplosables.Add(this.ManagerIconsCache);
		this.stateDisplosables.Add(this.VentureIconsCache);
		this.stateDisplosables.Add(this.BackgroundMusicCache);
		this.stateDisplosables.Add(this.BoosterCache);
		this.stateDisplosables.Add(this.CurrencyImageCache);
		this.stateDisplosables.Add(this.PlanetCompletePostCardImageCache);
		this.stateDisplosables.Add(this.SwagCache);
		this.stateDisplosables.Add(this.VentureAnimatedSpritesCache);
		this.stateDisplosables.Add(this.VentureViewLocalDataCache);
	}

	// Token: 0x06000EDA RID: 3802 RVA: 0x000434DC File Offset: 0x000416DC
	private void OnLoadNewPlanetPre()
	{
		foreach (IDisposable disposable in this.stateDisplosables)
		{
			disposable.Dispose();
		}
		if (!string.IsNullOrEmpty(this.assetBundleName))
		{
			GameController.Instance.HhAssetBundleManager.UnloadAssetBundle(this.assetBundleName, true, true);
			this.assetBundleName = null;
		}
	}

	// Token: 0x06000EDB RID: 3803 RVA: 0x00043558 File Offset: 0x00041758
	public void UpdateAssetBundle(IAssetBundle assetBundle, string assetBundleName)
	{
		this.assetBundle = assetBundle;
		this.assetBundleName = assetBundleName;
	}

	// Token: 0x06000EDC RID: 3804 RVA: 0x00043568 File Offset: 0x00041768
	private IObservable<T> GetAssetFromBundle<T>(string assetName) where T : Object
	{
		return Observable.Create<T>(delegate(IObserver<T> observer)
		{
			T value = this.assetBundle.LoadAsset<T>(assetName);
			observer.OnNext(value);
			observer.OnCompleted();
			return Disposable.Empty;
		});
	}

	// Token: 0x1700013A RID: 314
	// (get) Token: 0x06000EDD RID: 3805 RVA: 0x0004358D File Offset: 0x0004178D
	public IObservable<Sprite> AngelImage
	{
		get
		{
			if (this.AngelImageCache == null)
			{
				this.AngelImageCache = new PlanetThemeService.PlanetAssetCache<Sprite>();
			}
			return this.AngelImageCache.GetObservable("angel", new Action<string, Action<IObservable<Sprite>>>(this.FetchObservableAsset<Sprite>));
		}
	}

	// Token: 0x1700013B RID: 315
	// (get) Token: 0x06000EDE RID: 3806 RVA: 0x000435BE File Offset: 0x000417BE
	public IObservable<Sprite> BackgroundImage
	{
		get
		{
			if (this.BackgroundImageCache == null)
			{
				this.BackgroundImageCache = new PlanetThemeService.PlanetAssetCache<Sprite>();
			}
			return this.BackgroundImageCache.GetObservable("background", new Action<string, Action<IObservable<Sprite>>>(this.FetchObservableAsset<Sprite>));
		}
	}

	// Token: 0x1700013C RID: 316
	// (get) Token: 0x06000EDF RID: 3807 RVA: 0x000435EF File Offset: 0x000417EF
	public IObservable<IconDataScriptableObject> ManagerIcons
	{
		get
		{
			if (this.ManagerIconsCache == null)
			{
				this.ManagerIconsCache = new PlanetThemeService.PlanetAssetCache<IconDataScriptableObject>();
			}
			return this.ManagerIconsCache.GetObservable("icons-manager", new Action<string, Action<IObservable<IconDataScriptableObject>>>(this.FetchObservableAsset<IconDataScriptableObject>));
		}
	}

	// Token: 0x1700013D RID: 317
	// (get) Token: 0x06000EE0 RID: 3808 RVA: 0x00043620 File Offset: 0x00041820
	public IObservable<IconDataScriptableObject> VentureIcons
	{
		get
		{
			if (this.VentureIconsCache == null)
			{
				this.VentureIconsCache = new PlanetThemeService.PlanetAssetCache<IconDataScriptableObject>();
			}
			return this.VentureIconsCache.GetObservable("icons-venture", new Action<string, Action<IObservable<IconDataScriptableObject>>>(this.FetchObservableAsset<IconDataScriptableObject>));
		}
	}

	// Token: 0x1700013E RID: 318
	// (get) Token: 0x06000EE1 RID: 3809 RVA: 0x00043651 File Offset: 0x00041851
	public IObservable<AudioClip> BackgroundMusic
	{
		get
		{
			if (this.BackgroundMusicCache == null)
			{
				this.BackgroundMusicCache = new PlanetThemeService.PlanetAssetCache<AudioClip>();
			}
			return this.BackgroundMusicCache.GetObservable("music", new Action<string, Action<IObservable<AudioClip>>>(this.FetchObservableAsset<AudioClip>));
		}
	}

	// Token: 0x1700013F RID: 319
	// (get) Token: 0x06000EE2 RID: 3810 RVA: 0x00043682 File Offset: 0x00041882
	public IObservable<GameObject> Booster
	{
		get
		{
			if (this.BoosterCache == null)
			{
				this.BoosterCache = new PlanetThemeService.PlanetAssetCache<GameObject>();
			}
			return this.BoosterCache.GetObservable("booster", new Action<string, Action<IObservable<GameObject>>>(this.FetchObservableAsset<GameObject>));
		}
	}

	// Token: 0x17000140 RID: 320
	// (get) Token: 0x06000EE3 RID: 3811 RVA: 0x000436B3 File Offset: 0x000418B3
	public IObservable<Sprite> CurrencyImage
	{
		get
		{
			if (this.CurrencyImageCache == null)
			{
				this.CurrencyImageCache = new PlanetThemeService.PlanetAssetCache<Sprite>();
			}
			return this.CurrencyImageCache.GetObservable("currency", new Action<string, Action<IObservable<Sprite>>>(this.FetchObservableAsset<Sprite>));
		}
	}

	// Token: 0x17000141 RID: 321
	// (get) Token: 0x06000EE4 RID: 3812 RVA: 0x000436E4 File Offset: 0x000418E4
	public IObservable<Sprite> PlanetCompletePostCardImage
	{
		get
		{
			if (this.PlanetCompletePostCardImageCache == null)
			{
				this.PlanetCompletePostCardImageCache = new PlanetThemeService.PlanetAssetCache<Sprite>();
			}
			return this.PlanetCompletePostCardImageCache.GetObservable("postcard", new Action<string, Action<IObservable<Sprite>>>(this.FetchObservableAsset<Sprite>));
		}
	}

	// Token: 0x17000142 RID: 322
	// (get) Token: 0x06000EE5 RID: 3813 RVA: 0x00043715 File Offset: 0x00041915
	public IObservable<SwagPrefabSetup> Swag
	{
		get
		{
			if (this.SwagCache == null)
			{
				this.SwagCache = new PlanetThemeService.PlanetAssetCache<SwagPrefabSetup>();
			}
			return this.SwagCache.GetObservable("swag", new Action<string, Action<IObservable<SwagPrefabSetup>>>(this.FetchObservableAsset<SwagPrefabSetup>));
		}
	}

	// Token: 0x17000143 RID: 323
	// (get) Token: 0x06000EE6 RID: 3814 RVA: 0x00043746 File Offset: 0x00041946
	public IObservable<VentureAnimation> VentureAnimatedSprites
	{
		get
		{
			if (this.VentureAnimatedSpritesCache == null)
			{
				this.VentureAnimatedSpritesCache = new PlanetThemeService.PlanetAssetCache<VentureAnimation>();
			}
			return this.VentureAnimatedSpritesCache.GetObservable("venture-animations", new Action<string, Action<IObservable<VentureAnimation>>>(this.FetchObservableAsset<VentureAnimation>));
		}
	}

	// Token: 0x17000144 RID: 324
	// (get) Token: 0x06000EE7 RID: 3815 RVA: 0x00043777 File Offset: 0x00041977
	public IObservable<VentureColors> VentureViewColorData
	{
		get
		{
			if (this.VentureViewLocalDataCache == null)
			{
				this.VentureViewLocalDataCache = new PlanetThemeService.PlanetAssetCache<VentureColors>();
			}
			return this.VentureViewLocalDataCache.GetObservable("venture-colordata", new Action<string, Action<IObservable<VentureColors>>>(this.FetchObservableAsset<VentureColors>));
		}
	}

	// Token: 0x06000EE8 RID: 3816 RVA: 0x000437A8 File Offset: 0x000419A8
	private void FetchObservableAsset<T>(string assetName, Action<IObservable<T>> action) where T : Object
	{
		action(this.GetAssetFromBundle<T>(assetName));
	}

	// Token: 0x04000CC3 RID: 3267
	private IAssetBundle assetBundle;

	// Token: 0x04000CC4 RID: 3268
	private string assetBundleName;

	// Token: 0x04000CC5 RID: 3269
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000CC6 RID: 3270
	private List<IDisposable> stateDisplosables = new List<IDisposable>();

	// Token: 0x04000CC7 RID: 3271
	private PlanetThemeService.PlanetAssetCache<Sprite> AngelImageCache = new PlanetThemeService.PlanetAssetCache<Sprite>();

	// Token: 0x04000CC8 RID: 3272
	private PlanetThemeService.PlanetAssetCache<Sprite> BackgroundImageCache = new PlanetThemeService.PlanetAssetCache<Sprite>();

	// Token: 0x04000CC9 RID: 3273
	private PlanetThemeService.PlanetAssetCache<IconDataScriptableObject> ManagerIconsCache = new PlanetThemeService.PlanetAssetCache<IconDataScriptableObject>();

	// Token: 0x04000CCA RID: 3274
	private PlanetThemeService.PlanetAssetCache<IconDataScriptableObject> VentureIconsCache = new PlanetThemeService.PlanetAssetCache<IconDataScriptableObject>();

	// Token: 0x04000CCB RID: 3275
	private PlanetThemeService.PlanetAssetCache<AudioClip> BackgroundMusicCache = new PlanetThemeService.PlanetAssetCache<AudioClip>();

	// Token: 0x04000CCC RID: 3276
	private PlanetThemeService.PlanetAssetCache<GameObject> BoosterCache = new PlanetThemeService.PlanetAssetCache<GameObject>();

	// Token: 0x04000CCD RID: 3277
	private PlanetThemeService.PlanetAssetCache<Sprite> CurrencyImageCache = new PlanetThemeService.PlanetAssetCache<Sprite>();

	// Token: 0x04000CCE RID: 3278
	private PlanetThemeService.PlanetAssetCache<Sprite> PlanetCompletePostCardImageCache = new PlanetThemeService.PlanetAssetCache<Sprite>();

	// Token: 0x04000CCF RID: 3279
	private PlanetThemeService.PlanetAssetCache<SwagPrefabSetup> SwagCache = new PlanetThemeService.PlanetAssetCache<SwagPrefabSetup>();

	// Token: 0x04000CD0 RID: 3280
	private PlanetThemeService.PlanetAssetCache<VentureAnimation> VentureAnimatedSpritesCache = new PlanetThemeService.PlanetAssetCache<VentureAnimation>();

	// Token: 0x04000CD1 RID: 3281
	private PlanetThemeService.PlanetAssetCache<VentureColors> VentureViewLocalDataCache = new PlanetThemeService.PlanetAssetCache<VentureColors>();

	// Token: 0x020008BF RID: 2239
	private class PlanetAssetCache<T> : IDisposable
    {
		// Token: 0x06002BFB RID: 11259 RVA: 0x000AEDD9 File Offset: 0x000ACFD9
		public void Dispose()
		{
			this.disposable.Dispose();
			this.cachedValue = default(T);
			this.cachedLoader = null;
		}

		// Token: 0x06002BFC RID: 11260 RVA: 0x000AEDF9 File Offset: 0x000ACFF9
		public IObservable<T> GetObservable(string assetName, Action<string, Action<IObservable<T>>> creationFunction)
		{
			Action<IObservable<T>> <>9__1;
			return Observable.Create<T>(delegate(IObserver<T> observer)
			{
				if (this.cachedValue != null)
				{
					observer.OnNext(this.cachedValue);
					observer.OnCompleted();
				}
				else
				{
					if (this.cachedLoader == null)
					{
						Action<string, Action<IObservable<T>>> creationFunction2 = creationFunction;
						string assetName2 = assetName;
						Action<IObservable<T>> arg;
						if ((arg = <>9__1) == null)
						{
							arg = (<>9__1 = delegate(IObservable<T> x)
							{
								this.cachedLoader = x;
							});
						}
						creationFunction2(assetName2, arg);
					}
					this.cachedLoader.Subscribe(delegate(T x)
					{
						this.cachedValue = x;
						observer.OnNext(this.cachedValue);
						observer.OnCompleted();
					}).AddTo(this.disposable);
				}
				return Disposable.Empty;
			});
		}

		// Token: 0x04002BE2 RID: 11234
		private IObservable<T> cachedLoader;

		// Token: 0x04002BE3 RID: 11235
		private T cachedValue;

		// Token: 0x04002BE4 RID: 11236
		private CompositeDisposable disposable = new CompositeDisposable();
	}
}
