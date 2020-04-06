using System;
using System.Collections.Generic;
using System.Linq;
using AdComm;
using UniRx;
using UnityEngine;

// Token: 0x020001BF RID: 447
public class IconService
{
	// Token: 0x17000121 RID: 289
	// (get) Token: 0x06000D5E RID: 3422 RVA: 0x0003BDC4 File Offset: 0x00039FC4
	// (set) Token: 0x06000D5F RID: 3423 RVA: 0x0003BDCC File Offset: 0x00039FCC
	public ReactiveDictionary<string, Sprite> PromoImages { get; private set; }

	// Token: 0x17000122 RID: 290
	// (get) Token: 0x06000D60 RID: 3424 RVA: 0x0003BDD5 File Offset: 0x00039FD5
	// (set) Token: 0x06000D61 RID: 3425 RVA: 0x0003BDDD File Offset: 0x00039FDD
	public ReactiveProperty<Sprite> EventPanelIcon { get; private set; }

	// Token: 0x17000123 RID: 291
	// (get) Token: 0x06000D62 RID: 3426 RVA: 0x0003BDE6 File Offset: 0x00039FE6
	public IObservable<bool> AreBadgesLoaded
	{
		get
		{
			return this._AreBadgesLoaded;
		}
	}

	// Token: 0x06000D63 RID: 3427 RVA: 0x0003BDF0 File Offset: 0x00039FF0
	public IconService()
	{
		this.PromoImages = new ReactiveDictionary<string, Sprite>();
		this.EventPanelIcon = new ReactiveProperty<Sprite>();
	}

	// Token: 0x06000D64 RID: 3428 RVA: 0x0003BE50 File Offset: 0x0003A050
	public void Init()
	{
		this.LoadIcons();
		this.LoadBundledBadgeAssets();
		GameController.Instance.EventService.ActiveEvents.ObserveAdd().Subscribe(delegate(CollectionAddEvent<EventModel> evnt)
		{
			GameController.Instance.HhAssetBundleManager.GetBundleAsync("planetdata-" + evnt.Value.PlanetTheme.ToLower()).Subscribe(delegate(IAssetBundle bundle)
			{
				Sprite value = bundle.LoadAsset<Sprite>("panelIcon");
				this.EventPanelIcon.Value = value;
			}, delegate(Exception error)
			{
				Debug.LogError("panelIcon not found in bundle " + evnt.Value.Id.ToLower());
			}).AddTo(this._Disposables);
		}).AddTo(this._Disposables);
	}

	// Token: 0x06000D65 RID: 3429 RVA: 0x0003BE8F File Offset: 0x0003A08F
	public IObservable<Sprite> LoadPromoImage(string planetTheme)
	{
		return Observable.Create<Sprite>(delegate(IObserver<Sprite> observer)
		{
			if (this.PromoImages.ContainsKey(planetTheme))
			{
				observer.OnNext(this.PromoImages[planetTheme]);
				observer.OnCompleted();
			}
			else
			{
				GameController.Instance.HhAssetBundleManager.GetBundleAsync("planetdata-" + planetTheme.ToLower()).Subscribe(delegate(IAssetBundle b)
				{
					Sprite sprite = b.LoadAsset<Sprite>(planetTheme);
					if (sprite != null)
					{
						if (!this.PromoImages.ContainsKey(planetTheme))
						{
							this.PromoImages.Add(planetTheme, sprite);
						}
						observer.OnNext(sprite);
						observer.OnCompleted();
						return;
					}
					observer.OnError(new Exception("Error loading promo images for theme " + planetTheme));
				}).AddTo(this._Disposables);
			}
			return Disposable.Empty;
		});
	}

	// Token: 0x06000D66 RID: 3430 RVA: 0x0003BEB4 File Offset: 0x0003A0B4
	public void Dispose()
	{
		this._Disposables.Dispose();
	}

	// Token: 0x06000D67 RID: 3431 RVA: 0x0003BEC4 File Offset: 0x0003A0C4
	public Sprite GetGoldIcon(int amount)
	{
		string arg = "1";
		if (amount >= 1300)
		{
			arg = "5";
		}
		else if (amount >= 625)
		{
			arg = "4";
		}
		else if (amount >= 240)
		{
			arg = "3";
		}
		else if (amount >= 115)
		{
			arg = "2";
		}
		string iconName = string.Format("Icon_Gold{0}", arg);
		Icon icon = this.icons.FirstOrDefault(x => x.id == iconName);
		if (icon != null)
		{
			return icon.sprite;
		}
		return Resources.Load<Sprite>(string.Format("ItemIcons/{0}", iconName));
	}

	// Token: 0x06000D68 RID: 3432 RVA: 0x0003BF60 File Offset: 0x0003A160
	public Sprite GetSprite(string id)
	{
		if (this.icons.Exists(i => i.id == id))
		{
			return this.icons.Find(i => i.id == id).sprite;
		}
		return this.icons.Find(i => i.id == "default").sprite;
	}

	// Token: 0x06000D69 RID: 3433 RVA: 0x0003BFE0 File Offset: 0x0003A1E0
	private void LoadIcons()
	{
		foreach (IconsScriptableObject iconScriptableObject in Resources.LoadAll<IconsScriptableObject>("Icons"))
		{
			this.LoadIconsFromScriptableObject(iconScriptableObject);
		}
	}

	// Token: 0x06000D6A RID: 3434 RVA: 0x0003C014 File Offset: 0x0003A214
	public void LoadIconsFromScriptableObject(IconsScriptableObject iconScriptableObject)
	{
		using (List<Icon>.Enumerator enumerator = iconScriptableObject.Icons.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Icon icon = enumerator.Current;
				if (!this.icons.Exists(i => i.id == icon.id))
				{
					this.icons.Add(icon);
				}
			}
		}
	}

	// Token: 0x06000D6B RID: 3435 RVA: 0x0003C098 File Offset: 0x0003A298
	private void LoadBundledBadgeAssets()
	{
		GameController.Instance.HhAssetBundleManager.GetBundleAsync(this.BUNDLED_BADGES_BUNDLE_ID).Subscribe(delegate(IAssetBundle bundle)
		{
			Sprite[] badgeIcons = bundle.LoadAllAssets<Sprite>();
			GameController.Instance.IconService.StoreBadgeIcons(badgeIcons);
			this._AreBadgesLoaded.Value = true;
		}, delegate(Exception error)
		{
			Debug.LogError("Sprite badges bundle failed to load, we should handle this better");
		});
	}

	// Token: 0x06000D6C RID: 3436 RVA: 0x0003C0EB File Offset: 0x0003A2EB
	private void StoreBadgeIcons(Sprite[] badgeIcons)
	{
		this.bundledBadgeIcons = new List<Sprite>(badgeIcons);
	}

	// Token: 0x06000D6D RID: 3437 RVA: 0x0003C0FC File Offset: 0x0003A2FC
	public Sprite GetBadgeIcon(string badgename)
	{
		for (int i = 0; i < this.bundledBadgeIcons.Count; i++)
		{
			Sprite sprite = this.bundledBadgeIcons[i];
			if (sprite.name == badgename)
			{
				return sprite;
			}
		}
		Debug.LogWarning("(GetBadgeIcon) Unable to find badge icon " + badgename + " in bundled_badges bundle");
		return Sprite.Create(new Texture2D(256, 256), default(Rect), default(Vector2));
	}

	// Token: 0x04000B4B RID: 2891
	private CompositeDisposable _Disposables = new CompositeDisposable();

	// Token: 0x04000B4E RID: 2894
	private List<Icon> icons = new List<Icon>();

	// Token: 0x04000B4F RID: 2895
	private readonly BoolReactiveProperty _AreBadgesLoaded = new BoolReactiveProperty();

	// Token: 0x04000B50 RID: 2896
	private readonly string BUNDLED_BADGES_BUNDLE_ID = "bundled_badges";

	// Token: 0x04000B51 RID: 2897
	private List<Sprite> bundledBadgeIcons = new List<Sprite>();
}
