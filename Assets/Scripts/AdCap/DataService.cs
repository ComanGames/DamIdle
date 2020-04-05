using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using Platforms;
using Platforms.Logger;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using UniRx;
using UnityEngine;

namespace AdCap
{
	// Token: 0x020006E6 RID: 1766
	public class DataService : IDisposable
	{
		// Token: 0x17000302 RID: 770
		// (get) Token: 0x0600248C RID: 9356 RVA: 0x0009E165 File Offset: 0x0009C365
		public static string GameDataPath
		{
			get
			{
				return Application.dataPath + "/_/Editor/StandaloneGameData.txt";
			}
		}

		// Token: 0x0600248D RID: 9357 RVA: 0x0009E176 File Offset: 0x0009C376
		public DataService()
		{
			this.logger = Platforms.Logger.Logger.GetLogger(this);
		}

		// Token: 0x0600248E RID: 9358 RVA: 0x00002718 File Offset: 0x00000918
		public void Dispose()
		{
		}

		// Token: 0x0600248F RID: 9359 RVA: 0x0009E1AC File Offset: 0x0009C3AC
		public void StorePlayerSegments(List<string> segments)
		{
			this.PlayerSegments = segments;
			this.PlayerSegmentsString = string.Join(",", segments.ToArray());
			Debug.Log("[DataService] Player belongs to segments {" + this.PlayerSegmentsString + "}");
		}

		// Token: 0x06002490 RID: 9360 RVA: 0x0009E1E5 File Offset: 0x0009C3E5
		public void StorePlayValueUSD(uint value)
		{
			this.ValueToDateUSD.Value = value;
		}

		// Token: 0x06002491 RID: 9361 RVA: 0x0009E1F4 File Offset: 0x0009C3F4
		public void Init(Action onComplete, Action<Exception> onError)
		{
			if (this.initialized)
			{
				onComplete();
				return;
			}
			this.playFab = Helper.GetPlatformAccount().PlayFab;
			this.logger.Debug("Initializing...");
			this.initialized = true;
			this.onError = onError;
			this.onComplete = delegate()
			{
				this.logger.Debug("Initialization complete.");
				onComplete();
			};
			this.titleDataConfig = Helper.GetPlatformAccount().TitleDataConfig;
			this.GetGameData().AsSingleUnitObservable<Unit>().Subscribe(delegate(Unit _)
			{
				this.onComplete();
			}, this.onError);
		}

		// Token: 0x06002492 RID: 9362 RVA: 0x0009E29C File Offset: 0x0009C49C
		private IObservable<Unit> GetGameData()
		{
			return Observable.Create<Unit>((IObserver<Unit> observer) => this.GetGameDataStream().Retry(2).Subscribe(delegate(string json)
			{
				try
				{
					this.ExternalData = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer, "").DeserializeObject<InventoryJsonDataObject>(json, new DataService.CustomPocoJsonSerializerStrategy());
				}
				catch (Exception obj)
				{
					this.WipeCachedGameData();
					this.onError(obj);
					return;
				}
				observer.OnNext(Unit.Default);
				observer.OnCompleted();
			}, this.onError));
		}

		// Token: 0x06002493 RID: 9363 RVA: 0x0009E2B0 File Offset: 0x0009C4B0
		private IObservable<string> GetGameDataStream()
		{
			string dataVersion = Helper.GetPlatformAccount().DataVersion;
			new Dictionary<string, string>().Add("TitleKey", "config-" + dataVersion);
			return Observable.Create<string>(delegate(IObserver<string> observer)
			{
				this.logger.Debug("Downloading game data config for data version [{0}]", new object[]
				{
					dataVersion
				});
				if (this.titleDataConfig == null)
				{
					observer.OnError(new Exception(string.Format("Unable to find game configuration data for data version [{0}]", dataVersion)));
				}
				else
				{
					this.MinimumClientVersion = this.titleDataConfig.MinVersion;
					if (string.IsNullOrEmpty(this.titleDataConfig.GameBalanceKey))
					{
						observer.OnError(new Exception(string.Format("Key [{0}] not found in title data", this.titleDataConfig.GameBalanceKey)));
					}
					else
					{
						string resourcePath = string.Format("{0}/{1}-{2}-{3}.gamedata", new object[]
						{
							Application.persistentDataPath,
							this.titleDataConfig.GameBalanceKey,
							dataVersion,
							this.titleDataConfig.DataVersion
						});
						if (File.Exists(resourcePath))
						{
							string text = File.ReadAllText(resourcePath);
							this.logger.Debug("Using cached game data for config [{0}]", new object[]
							{
								this.titleDataConfig.GameBalanceKey
							});
							this.logger.Trace("Cached data for config [{0}]: [{1}]", new object[]
							{
								resourcePath,
								text
							});
							observer.OnNext(text);
							observer.OnCompleted();
						}
						else
						{
							this.logger.Debug("Downloading game data for config [{0}]", new object[]
							{
								this.titleDataConfig.GameBalanceKey
							});
							this.playFab.GetTitleData(delegate(GetTitleDataResult dataResult)
							{
								try
								{
									string text2 = dataResult.Data[this.titleDataConfig.GameBalanceKey];
									this.logger.Trace("Processing game data: [{0}]", new object[]
									{
										text2
									});
									string text3 = GZipStream.UncompressString(Convert.FromBase64String(text2));
									File.WriteAllText(resourcePath, text3);
									observer.OnNext(text3);
									observer.OnCompleted();
								}
								catch (ZlibException error)
								{
									observer.OnError(error);
								}
							}, delegate(PlayFabError error)
							{
								observer.OnError(error.ToException());
							});
						}
					}
				}
				return Disposable.Empty;
			});
		}

		// Token: 0x06002494 RID: 9364 RVA: 0x0009E30C File Offset: 0x0009C50C
		private void WipeCachedGameData()
		{
			string[] files = Directory.GetFiles(Application.persistentDataPath);
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].EndsWith(".gamedata"))
				{
					File.Delete(files[i]);
				}
			}
		}

		// Token: 0x06002495 RID: 9365 RVA: 0x00002718 File Offset: 0x00000918
		public void Reset()
		{
		}

		// Token: 0x0400254A RID: 9546
		public const string GAME_DATA_PATH = "StandaloneGameData.txt";

		// Token: 0x0400254B RID: 9547
		public const string DATA_VERSION_RESOURCE = "data-version";

		// Token: 0x0400254C RID: 9548
		public const string CLOUD_SCRIPT_FUNCTION = "GetOverridableTitleData";

		// Token: 0x0400254D RID: 9549
		public InventoryJsonDataObject ExternalData;

		// Token: 0x0400254E RID: 9550
		public string PlayerSegmentsString = "";

		// Token: 0x0400254F RID: 9551
		public List<string> PlayerSegments = new List<string>();

		// Token: 0x04002550 RID: 9552
		public readonly ReactiveProperty<uint> ValueToDateUSD = new ReactiveProperty<uint>(0U);

		// Token: 0x04002551 RID: 9553
		public string MinimumClientVersion;

		// Token: 0x04002552 RID: 9554
		private Action onComplete;

		// Token: 0x04002553 RID: 9555
		private Action<Exception> onError;

		// Token: 0x04002554 RID: 9556
		private Platforms.Logger.Logger logger;

		// Token: 0x04002555 RID: 9557
		private string dataVersion;

		// Token: 0x04002556 RID: 9558
		private bool initialized;

		// Token: 0x04002557 RID: 9559
		private PlatformAccount.TitleDataConfigModel titleDataConfig;

		// Token: 0x04002558 RID: 9560
		private PlayFabWrapper playFab;

		// Token: 0x02000A3D RID: 2621
		public class CustomPocoJsonSerializerStrategy : PocoJsonSerializerStrategy
		{
			// Token: 0x06003162 RID: 12642 RVA: 0x000B9CA4 File Offset: 0x000B7EA4
			public CustomPocoJsonSerializerStrategy() : this(null)
			{
			}

			// Token: 0x06003163 RID: 12643 RVA: 0x000B9CAD File Offset: 0x000B7EAD
			public CustomPocoJsonSerializerStrategy(Platforms.Logger.Logger logger)
			{
				this.logger = logger;
			}

			// Token: 0x06003164 RID: 12644 RVA: 0x000B9CBC File Offset: 0x000B7EBC
			public override object DeserializeObject(object value, Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				string value2 = value as string;
				if (type == typeof(Guid) && string.IsNullOrEmpty(value2))
				{
					return default(Guid);
				}
				if (value == null)
				{
					return null;
				}
				if (string.IsNullOrEmpty(value2))
				{
					return base.DeserializeObject(value, type);
				}
				if (type.GetTypeInfo().IsEnum)
				{
					return Enum.Parse(type, value2);
				}
				if (type == typeof(DateTime))
				{
					return DateTime.ParseExact(value.ToString(), "yyyy/M/dd HH:mm", null);
				}
				if (type == typeof(Color))
				{
					string[] array = value.ToString().Split(new char[]
					{
						','
					});
					return new Color((float)int.Parse(array[0]) / 255f, (float)int.Parse(array[1]) / 255f, (float)int.Parse(array[2]) / 255f);
				}
				object result;
				try
				{
					result = base.DeserializeObject(value, type);
				}
				catch (FormatException ex)
				{
					FormatException ex2 = new FormatException(string.Format("Unable to deserialize value [{0}] to type [{1}]: [{2}]", value, type, ex.Message));
					if (this.logger != null)
					{
						this.logger.Exception(ex2);
					}
					throw ex2;
				}
				return result;
			}

			// Token: 0x04003068 RID: 12392
			private Platforms.Logger.Logger logger;
		}
	}
}
