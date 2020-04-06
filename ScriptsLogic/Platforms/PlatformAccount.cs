using System;
using System.Collections.Generic;
using System.Linq;
using Platforms.Logger;
using PlayFab;
using PlayFab.ClientModels;
using UniRx;
using UnityEngine;

namespace Platforms
{
	// Token: 0x020006C8 RID: 1736
	public class PlatformAccount
	{
		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x0600234D RID: 9037 RVA: 0x000996D4 File Offset: 0x000978D4
		public PlatformType PlatformType
		{
			get
			{
				return this.Active.PlatformType;
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x0600234E RID: 9038 RVA: 0x000996E4 File Offset: 0x000978E4
		public string DataVersion
		{
			get
			{
				string result;
				if ((result = this.dataVersion) == null)
				{
					result = (this.dataVersion = this.SetDataVersion());
				}
				return result;
			}
		}

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x0600234F RID: 9039 RVA: 0x0009970A File Offset: 0x0009790A
		// (set) Token: 0x06002350 RID: 9040 RVA: 0x00099712 File Offset: 0x00097912
		public bool IsLoggedIn { get; private set; }

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06002351 RID: 9041 RVA: 0x0009971B File Offset: 0x0009791B
		// (set) Token: 0x06002352 RID: 9042 RVA: 0x00099723 File Offset: 0x00097923
		public PlayFabWrapper PlayFab { get; protected set; }

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06002353 RID: 9043 RVA: 0x0009972C File Offset: 0x0009792C
		// (set) Token: 0x06002354 RID: 9044 RVA: 0x00099734 File Offset: 0x00097934
		public LoginResult LastLoginResult { get; protected set; }

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06002355 RID: 9045 RVA: 0x0009973D File Offset: 0x0009793D
		// (set) Token: 0x06002356 RID: 9046 RVA: 0x00099745 File Offset: 0x00097945
		public PlatformAccount.TitleDataConfigModel TitleDataConfig { get; private set; }

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06002357 RID: 9047 RVA: 0x0009974E File Offset: 0x0009794E
		public IObservable<LoginResult> OnLogin
		{
			get
			{
				return from _ in this.onLogin.First(v => v)
				select this.LastLoginResult;
			}
		}

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06002358 RID: 9048 RVA: 0x0009978B File Offset: 0x0009798B
		public IObservable<Unit> OnLogout
		{
			get
			{
				return (from v in this.onLogin
				where !v
				select v).Take(1).AsUnitObservable<bool>();
			}
		}

		// Token: 0x06002359 RID: 9049 RVA: 0x000997C2 File Offset: 0x000979C2
		public void SetPlatforms(List<IPlatformAccountExtension> platforms)
		{
			this.platforms = platforms;
		}

		// Token: 0x0600235A RID: 9050 RVA: 0x000997CC File Offset: 0x000979CC
		public T GetExtension<T>() where T : IPlatformAccountExtension
		{
			for (int i = 0; i < this.platforms.Count; i++)
			{
				if (this.platforms[i] is T)
				{
					return (T)((object)this.platforms[i]);
				}
			}
			throw new PlatformAccount.UnknownPlatformAccountException(string.Format("Unable to find instance of IPlatformAccount for type [{0}]", typeof(T).FullName));
		}

		// Token: 0x0600235B RID: 9051 RVA: 0x00099834 File Offset: 0x00097A34
		public PlatformAccount()
		{
			this.logger = Platforms.Logger.Logger.GetLogger(this);
			this.onLogin.Subscribe(delegate(bool v)
			{
				this.IsLoggedIn = v;
			});
			this.OnActiveExtensionChanged.Subscribe(delegate(IPlatformAccountExtension active)
			{
				this.logger.Info("Active extension changed to [{0}]", new object[]
				{
					active.GetType()
				});
			});
		}

		// Token: 0x0600235C RID: 9052 RVA: 0x000998BC File Offset: 0x00097ABC
		public void Init()
		{
			this.Init(Resources.Load<TextAsset>("playfab-settings").text, null);
		}

		// Token: 0x0600235D RID: 9053 RVA: 0x000998D8 File Offset: 0x00097AD8
		public virtual PlatformAccount Init(string titleId, ITransportPlugin http = null)
		{
			Type type = base.GetType();
			this.logger.Info("Initializing [{0}]", new object[]
			{
				type.Name
			});
			this.PlayFab = new PlayFabWrapper(titleId, null);
			if (this.platforms == null)
			{
				this.platforms = (from p in Helper.GetAvailableInstances<IPlatformAccountExtension>(Helper.GetPlatformType())
				select p.Platform).ToList<IPlatformAccountExtension>();
			}
			string @string = PlayerPrefs.GetString("_PA_PlatformKey", string.Empty);
			foreach (IPlatformAccountExtension platformAccountExtension in this.platforms)
			{
				platformAccountExtension.InitPlatform(this, this.logger, this.PlayFab);
				if (platformAccountExtension.GetType().FullName == @string)
				{
					this.SetActiveExtension(platformAccountExtension);
					this.Active = platformAccountExtension;
				}
			}
			if (this.Active == null)
			{
				this.Active = this.platforms[0];
			}
			this.OnActiveExtensionChanged.OnNext(this.Active);
			this.logger.Info("Initialized");
			return this;
		}

		// Token: 0x0600235E RID: 9054 RVA: 0x00099A1C File Offset: 0x00097C1C
		public void Dispose()
		{
			this.logger.Info("Disposing");
			this.platforms.ForEach(delegate(IPlatformAccountExtension platform)
			{
				platform.DisposePlatform();
			});
			this.PlayFab.Dispose();
			this.disposables.Dispose();
		}

		// Token: 0x0600235F RID: 9055 RVA: 0x00099A7C File Offset: 0x00097C7C
		public void SetActiveExtension(IPlatformAccountExtension extension)
		{
			this.logger.Info("Setting active extension to [{0}]", new object[]
			{
				extension.GetType().FullName
			});
			this.Active = extension;
			if (Application.isPlaying)
			{
				PlayerPrefs.SetString("_PA_PlatformKey", extension.GetType().FullName);
			}
			this.OnActiveExtensionChanged.OnNext(this.Active);
		}

		// Token: 0x06002360 RID: 9056 RVA: 0x00099AE1 File Offset: 0x00097CE1
		public void SetActiveExtension<T>() where T : IPlatformAccountExtension
		{
			this.SetActiveExtension(this.GetExtension<T>());
		}

		// Token: 0x06002361 RID: 9057 RVA: 0x00099AF4 File Offset: 0x00097CF4
		private string SetDataVersion()
		{
			Dictionary<string, string> dictionary = SimpleConfigFile.LoadConfigIfAvailable("data-version", true);
			if (dictionary == null)
			{
				return Helper.GetMajorMinorVersion();
			}
			return dictionary["v"];
		}

		// Token: 0x06002362 RID: 9058 RVA: 0x00099B21 File Offset: 0x00097D21
		public void Login()
		{
			this.Login(null, null);
		}

		// Token: 0x06002363 RID: 9059 RVA: 0x00099B2C File Offset: 0x00097D2C
		public void Login(Action<LoginResult> onSuccess, Action<PlayFabError> onError)
		{
			if (onSuccess != null)
			{
				this.onLoginSuccess.Add(onSuccess);
			}
			if (onError != null)
			{
				this.onLoginFail.Add(onError);
			}
			if (this.loggingIn)
			{
				return;
			}
			this.loggingIn = true;
			this.logger.Trace("Initiating login on extension [{0}]...", new object[]
			{
				this.Active.GetType()
			});
			this.Active.Login(new Action<LoginResult>(this.OnLoginSuccess), new Action<PlayFabError>(this.OnLoginFail));
		}

		// Token: 0x06002364 RID: 9060 RVA: 0x00099BB0 File Offset: 0x00097DB0
		private void OnLoginSuccess(LoginResult loginResult)
		{
			this.logger.Info("Successfully logged in, PlayFab Id: [{0}]", new object[]
			{
				loginResult.PlayFabId
			});
			bool flag = this.LastLoginResult == null;
			this.LastLoginResult = loginResult;
			if (flag)
			{
				this.OnInitialLogin();
				return;
			}
			this.PostLoginSuccess(loginResult);
		}

		// Token: 0x06002365 RID: 9061 RVA: 0x00099BFC File Offset: 0x00097DFC
		private void PostLoginSuccess(LoginResult loginResult)
		{
			for (int i = 0; i < this.onLoginSuccess.Count; i++)
			{
				this.onLoginSuccess[i](loginResult);
			}
			this.onLoginSuccess.Clear();
			this.onLoginFail.Clear();
			this.onLogin.OnNext(true);
			this.loggingIn = false;
		}

		// Token: 0x06002366 RID: 9062 RVA: 0x00099C5C File Offset: 0x00097E5C
		private void OnLoginFail(PlayFabError error)
		{
			this.logger.Trace("Login failed.", Array.Empty<object>());
			for (int i = 0; i < this.onLoginFail.Count; i++)
			{
				this.onLoginFail[i](error);
			}
			this.onLoginSuccess.Clear();
			this.onLoginFail.Clear();
			this.onLogin.OnNext(false);
			this.loggingIn = false;
		}

		// Token: 0x06002367 RID: 9063 RVA: 0x00099CD0 File Offset: 0x00097ED0
		private void OnInitialLogin()
		{
			this.logger.Trace("Executing initial login tasks...", Array.Empty<object>());
			if (this.LastLoginResult == null)
			{
				this.logger.Exception("Game data configuration returned from CloudScript call is null.", new NullReferenceException());
				return;
			}
			string text = "";
			if (this.LastLoginResult.InfoResultPayload != null)
			{
				if (this.LastLoginResult.InfoResultPayload.UserReadOnlyData != null)
				{
					UserDataRecord userDataRecord;
					if (this.LastLoginResult.InfoResultPayload.UserReadOnlyData.TryGetValue("CloudScriptVersion", out userDataRecord))
					{
						this.PlayFab.CloudScriptRevisionOption = (CloudScriptRevisionOption)Enum.Parse(typeof(CloudScriptRevisionOption), userDataRecord.Value);
					}
					this.logger.Trace("Attempting to read config-{0} from UserReadOnlyData...", new object[]
					{
						this.DataVersion
					});
					UserDataRecord userDataRecord2;
					if (this.LastLoginResult.InfoResultPayload.UserReadOnlyData.TryGetValue("config-" + this.DataVersion, out userDataRecord2))
					{
						text = userDataRecord2.Value;
						this.logger.Trace("Read config-{0} from UserReadOnlyData succesfully", new object[]
						{
							this.DataVersion
						});
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					this.logger.Trace("Attempting to read config-{0} from TitleData...", new object[]
					{
						this.DataVersion
					});
					string text2;
					if (this.LastLoginResult.InfoResultPayload.TitleData.TryGetValue("config-" + this.dataVersion, out text2))
					{
						text = text2;
						this.logger.Trace("Read config-{0} from TitleData succesfully", new object[]
						{
							this.DataVersion
						});
					}
				}
			}
			else
			{
				this.OnLoginFail(new PlayFabError
				{
					Error = PlayFabErrorCode.CloudScriptAPIRequestError,
					ErrorMessage = "InfoResultPayload is null"
				});
			}
			if (string.IsNullOrEmpty(text))
			{
				this.OnLoginFail(new PlayFabError
				{
					Error = PlayFabErrorCode.CloudScriptAPIRequestError,
					ErrorMessage = "Failed to get TitleDataConfig Json"
				});
				return;
			}
			this.TitleDataConfig = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer, "").DeserializeObject<PlatformAccount.TitleDataConfigModel>(text);
			if (this.TitleDataConfig != null)
			{
				this.PostLoginSuccess(this.LastLoginResult);
				return;
			}
			this.OnLoginFail(new PlayFabError
			{
				Error = PlayFabErrorCode.CloudScriptAPIRequestError,
				ErrorMessage = "Failed to Deserialize TitleDataConfig"
			});
		}

		// Token: 0x06002368 RID: 9064 RVA: 0x00099EFC File Offset: 0x000980FC
		private void RetryLogin()
		{
			Observable.Timer(TimeSpan.FromSeconds(1.0)).TakeUntil(from v in this.onLogin
			where v
			select v).Subscribe(delegate(long __)
			{
				this.Login(null, delegate(PlayFabError _)
				{
					this.RetryLogin();
				});
			});
		}

		// Token: 0x0400249C RID: 9372
		public const string GAME_CONFIG_CLOUD_SCRIPT_FUNCTION = "DataConfig";

		// Token: 0x0400249D RID: 9373
		public const string ACTIVE_PLATFORM_KEY = "_PA_PlatformKey";

		// Token: 0x0400249E RID: 9374
		public const string DATA_VERSION_RESOURCE = "data-version";

		// Token: 0x040024A3 RID: 9379
		public ReplaySubject<IPlatformAccountExtension> OnActiveExtensionChanged = new ReplaySubject<IPlatformAccountExtension>(1);

		// Token: 0x040024A4 RID: 9380
		public IPlatformAccountExtension Active;

		// Token: 0x040024A5 RID: 9381
		private List<IPlatformAccountExtension> platforms;

		// Token: 0x040024A6 RID: 9382
		private Platforms.Logger.Logger logger;

		// Token: 0x040024A7 RID: 9383
		private ReplaySubject<bool> onLogin = new ReplaySubject<bool>(1);

		// Token: 0x040024A8 RID: 9384
		private CompositeDisposable disposables = new CompositeDisposable();

		// Token: 0x040024A9 RID: 9385
		private string dataVersion;

		// Token: 0x040024AA RID: 9386
		private List<Action<LoginResult>> onLoginSuccess = new List<Action<LoginResult>>();

		// Token: 0x040024AB RID: 9387
		private List<Action<PlayFabError>> onLoginFail = new List<Action<PlayFabError>>();

		// Token: 0x040024AC RID: 9388
		private bool loggingIn;

		// Token: 0x02000A10 RID: 2576
		public class UnknownPlatformAccountException : Exception
		{
			// Token: 0x060030D5 RID: 12501 RVA: 0x00066C57 File Offset: 0x00064E57
			public UnknownPlatformAccountException(string err) : base(err)
			{
			}
		}

		// Token: 0x02000A11 RID: 2577
		[Serializable]
		public class TitleDataConfigModel
		{
			// Token: 0x04002FF0 RID: 12272
			public string GameBalanceKey;

			// Token: 0x04002FF1 RID: 12273
			public int DataVersion;

			// Token: 0x04002FF2 RID: 12274
			public string LocalizationKey;

			// Token: 0x04002FF3 RID: 12275
			public int LocalizationVersion;

			// Token: 0x04002FF4 RID: 12276
			public string MinVersion;

			// Token: 0x04002FF5 RID: 12277
			public PlatformAccount.StoreDataConfigModel StoreConfig;

			// Token: 0x04002FF6 RID: 12278
			public PlatformAccount.AssetBundleConfigModel AssetBundleConfig;
		}

		// Token: 0x02000A12 RID: 2578
		[Serializable]
		public class StoreDataConfigModel
		{
			// Token: 0x04002FF7 RID: 12279
			public string CatalogVersion;

			// Token: 0x04002FF8 RID: 12280
			public Dictionary<string, string[]> Store;
		}

		// Token: 0x02000A13 RID: 2579
		[Serializable]
		public class AssetBundleConfigModel
		{
			// Token: 0x060030D8 RID: 12504 RVA: 0x000B83FC File Offset: 0x000B65FC
			public PlatformAccount.AssetBundlePlatformConfig GetPlatformConfig()
			{
				return this.PlatformConfigs[Utility.GetPlatformName()];
			}

			// Token: 0x04002FF9 RID: 12281
			public Dictionary<string, PlatformAccount.AssetBundlePlatformConfig> PlatformConfigs;
		}

		// Token: 0x02000A14 RID: 2580
		[Serializable]
		public class AssetBundlePlatformConfig
		{
			// Token: 0x04002FFA RID: 12282
			public string DirectoryRoot;

			// Token: 0x04002FFB RID: 12283
			public string ManifestVersion;
		}
	}
}
