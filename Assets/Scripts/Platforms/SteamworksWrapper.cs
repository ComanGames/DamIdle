using System;
using System.Collections.Generic;
using System.Linq;
using Platforms.Logger;
using Steamworks;
using UniRx;

namespace Platforms
{
	// Token: 0x020006D3 RID: 1747
	public class SteamworksWrapper : IDisposable
	{
		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06002416 RID: 9238 RVA: 0x0009CB7D File Offset: 0x0009AD7D
		public static SteamworksWrapper Instance
		{
			get
			{
				SteamworksWrapper.instance = (SteamworksWrapper.instance ?? new SteamworksWrapper());
				return SteamworksWrapper.instance;
			}
		}

		// Token: 0x06002417 RID: 9239 RVA: 0x0009CB98 File Offset: 0x0009AD98
		public virtual void Init()
		{
			if (this.initCalled)
			{
				return;
			}
			this.initCalled = true;
			this.logger = Logger.GetLogger(this);
			this.DllValidation();
			if (!SteamAPI.Init())
			{
				throw new Exception("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
			}
			this.gameId = new CGameID(SteamUtils.GetAppID()).m_GameID;
			Observable.EveryUpdate().Subscribe(new Action<long>(this.Update)).AddTo(this.disposables);
		}

		// Token: 0x06002418 RID: 9240 RVA: 0x0009CC11 File Offset: 0x0009AE11
		public virtual void Update(long callCount)
		{
			SteamAPI.RunCallbacks();
			if (this.storeStats)
			{
				this.storeStats = !SteamUserStats.StoreStats();
			}
		}

		// Token: 0x06002419 RID: 9241 RVA: 0x0009CC2E File Offset: 0x0009AE2E
		public virtual void Dispose()
		{
			if (this.authSessionTicketResponseCallback != null)
			{
				this.authSessionTicketResponseCallback.Unregister();
			}
			if (this.microTxnAuthorizationResponseCallback != null)
			{
				this.microTxnAuthorizationResponseCallback.Unregister();
			}
			this.disposables.Dispose();
			SteamAPI.Shutdown();
			SteamworksWrapper.instance = null;
		}

		// Token: 0x0600241A RID: 9242 RVA: 0x0009CC6C File Offset: 0x0009AE6C
		public virtual void GetAuthSessionTicket(Action<string> onSuccess, Action<EResult> onError)
		{
			this.logger.Debug("Getting session auth ticket.");
			if (this.authSessionTicketResponseCallback != null)
			{
				this.authSessionTicketResponseCallback.Unregister();
			}
			byte[] ticket = new byte[2048];
			this.authSessionTicketResponseCallback = Callback<GetAuthSessionTicketResponse_t>.Create(delegate(GetAuthSessionTicketResponse_t result)
			{
				this.authSessionTicketResponseCallback.Unregister();
				if (result.m_eResult == EResult.k_EResultOK)
				{
					this.logger.Info("Successfully validated session auth ticket.");
					onSuccess(BitConverter.ToString(ticket, 0, ticket.Length).Replace("-", ""));
					return;
				}
				this.logger.Error("Unable to validate session auth ticket: [{0}]", new object[]
				{
					result.m_eResult
				});
				onError(result.m_eResult);
			});
			uint num;
			if (SteamUser.GetAuthSessionTicket(ticket, ticket.Length, out num) == HAuthTicket.Invalid)
			{
				EResult eresult = EResult.k_EResultUnexpectedError;
				this.logger.Error("Unable to get session auth ticket for user [{0}], AppId [{1}]: [{2}]", new object[]
				{
					SteamUser.GetSteamID(),
					SteamUtils.GetAppID(),
					eresult
				});
				onError(eresult);
			}
		}

		// Token: 0x0600241B RID: 9243 RVA: 0x0009CD48 File Offset: 0x0009AF48
		public virtual void MicroTransactionMonitor(Action<string> onSuccess, Action<EResult> onError)
		{
			if (this.microTxnAuthorizationResponseCallback != null)
			{
				this.microTxnAuthorizationResponseCallback.Unregister();
			}
			this.microTxnAuthorizationResponseCallback = Callback<MicroTxnAuthorizationResponse_t>.Create(delegate(MicroTxnAuthorizationResponse_t result)
			{
				if ((ulong)result.m_unAppID != this.gameId)
				{
					return;
				}
				this.microTxnAuthorizationResponseCallback.Unregister();
				if (Convert.ToBoolean(result.m_bAuthorized))
				{
					onSuccess(result.m_ulOrderID.ToString("X"));
					return;
				}
				onError(EResult.k_EResultCancelled);
			});
		}

		// Token: 0x0600241C RID: 9244 RVA: 0x0009CD9A File Offset: 0x0009AF9A
		private void DllValidation()
		{
			if (!Packsize.Test())
			{
				throw new Exception("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			}
			if (!DllCheck.Test())
			{
				throw new Exception("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			}
		}

		// Token: 0x0600241D RID: 9245 RVA: 0x0009CDC0 File Offset: 0x0009AFC0
		public string FileReadAsString(string filename, int maximumSize)
		{
			return new string(this.FileReadAsChars(filename, maximumSize));
		}

		// Token: 0x0600241E RID: 9246 RVA: 0x0009CDD0 File Offset: 0x0009AFD0
		public char[] FileReadAsChars(string filename, int maximumSize)
		{
			byte[] array = new byte[maximumSize * 2];
			int num = SteamRemoteStorage.FileRead(filename, array, array.Length);
			char[] array2 = new char[num / 2];
			Buffer.BlockCopy(array, 0, array2, 0, num);
			return array2;
		}

		// Token: 0x0600241F RID: 9247 RVA: 0x0009CE08 File Offset: 0x0009B008
		public byte[] FileRead(string filename, int maximumSize)
		{
			byte[] array = new byte[maximumSize * 2];
			int count = SteamRemoteStorage.FileRead(filename, array, array.Length);
			return array.Take(count).ToArray<byte>();
		}

		// Token: 0x06002420 RID: 9248 RVA: 0x0009CE38 File Offset: 0x0009B038
		public bool FileWrite(string fileName, string data)
		{
			byte[] array = new byte[data.Length * 2];
			Buffer.BlockCopy(data.ToCharArray(), 0, array, 0, array.Length);
			return this.FileWrite(fileName, array, array.Length);
		}

		// Token: 0x06002421 RID: 9249 RVA: 0x0009CE6F File Offset: 0x0009B06F
		public bool FileWrite(string fileName, byte[] data, int dataLength)
		{
			return SteamRemoteStorage.FileWrite(fileName, data, dataLength);
		}

		// Token: 0x06002422 RID: 9250 RVA: 0x0009CE79 File Offset: 0x0009B079
		public IObservable<Tuple<string, RemoteStorageFileWriteAsyncComplete_t>> FileWriteAsync(string fileName, byte[] data, int dataLength)
		{
			return new SteamworksWrapper.RemoteStorageCallWrapper(fileName, data, dataLength);
		}

		// Token: 0x06002423 RID: 9251 RVA: 0x0009CE83 File Offset: 0x0009B083
		public bool FileExists(string fileName)
		{
			return SteamRemoteStorage.FileExists(fileName);
		}

		// Token: 0x06002424 RID: 9252 RVA: 0x0009CE8B File Offset: 0x0009B08B
		public int GetFileCount()
		{
			return SteamRemoteStorage.GetFileCount();
		}

		// Token: 0x06002425 RID: 9253 RVA: 0x0009CE94 File Offset: 0x0009B094
		public void DeleteAllCloudFiles()
		{
			int fileCount = SteamRemoteStorage.GetFileCount();
			for (int i = 0; i < fileCount; i++)
			{
				int num = 0;
				SteamRemoteStorage.FileDelete(SteamRemoteStorage.GetFileNameAndSize(i, out num));
			}
		}

		// Token: 0x06002426 RID: 9254 RVA: 0x0009CEC4 File Offset: 0x0009B0C4
		public virtual void RequestCurrentStats(Dictionary<string, Achievement> achievements, Action onComplete)
		{
			if (this.userStatsReceivedCallback != null)
			{
				this.userStatsReceivedCallback.Unregister();
			}
			this.userStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(delegate(UserStatsReceived_t result)
			{
				if (result.m_nGameID != this.gameId)
				{
					return;
				}
				this.userStatsReceivedCallback.Unregister();
				this.userStatsReceivedCallback = null;
				if (result.m_eResult != EResult.k_EResultOK)
				{
					return;
				}
				foreach (string text in achievements.Keys)
				{
					bool achieved;
					if (SteamUserStats.GetAchievement(text, out achieved))
					{
						achievements[text].Name = SteamUserStats.GetAchievementDisplayAttribute(text, "name");
						achievements[text].Description = SteamUserStats.GetAchievementDisplayAttribute(text, "desc");
						achievements[text].Achieved = achieved;
					}
					else
					{
						this.logger.Warning("SteamUserStats.GetAchievement failed for Achievement [{0}].  Is it registered in the Steam Partner site?", new object[]
						{
							text
						});
					}
				}
				onComplete();
			});
			SteamUserStats.RequestCurrentStats();
		}

		// Token: 0x06002427 RID: 9255 RVA: 0x0009CF1C File Offset: 0x0009B11C
		public bool GetAchievement(string achievementId, out bool achieved)
		{
			return SteamUserStats.GetAchievement(achievementId, out achieved);
		}

		// Token: 0x06002428 RID: 9256 RVA: 0x0009CF25 File Offset: 0x0009B125
		public virtual void SetAchievement(string achievementId)
		{
			SteamUserStats.SetAchievement(achievementId);
			this.storeStats = true;
		}

		// Token: 0x06002429 RID: 9257 RVA: 0x0009CF38 File Offset: 0x0009B138
		public virtual void UpdateProgressForAchievement(string achievementName, uint currentProgress, uint maxProgress, Action<bool> onComplete)
		{
			if (currentProgress >= maxProgress)
			{
				this.userAchievementStoredCallback = Callback<UserAchievementStored_t>.Create(delegate(UserAchievementStored_t result)
				{
					if (result.m_nGameID != this.gameId)
					{
						return;
					}
					this.userAchievementStoredCallback.Unregister();
					onComplete(result.m_nCurProgress == 0U && result.m_nMaxProgress == 0U);
				});
				SteamUserStats.SetAchievement(achievementName);
			}
			else
			{
				this.userAchievementStoredCallback = Callback<UserAchievementStored_t>.Create(delegate(UserAchievementStored_t result)
				{
					if (result.m_nGameID != this.gameId)
					{
						return;
					}
					this.userAchievementStoredCallback.Unregister();
					onComplete(result.m_nCurProgress == 0U && result.m_nMaxProgress == 0U);
				});
				SteamUserStats.IndicateAchievementProgress(achievementName, currentProgress, maxProgress);
			}
			this.storeStats = true;
		}

		// Token: 0x0600242A RID: 9258 RVA: 0x0009CFA5 File Offset: 0x0009B1A5
		public virtual void ResetAllStats()
		{
			SteamUserStats.ResetAllStats(false);
			this.storeStats = true;
		}

		// Token: 0x0600242B RID: 9259 RVA: 0x0009CFB5 File Offset: 0x0009B1B5
		public bool DlcInstalled(int dlcId)
		{
			return SteamApps.BIsDlcInstalled((AppId_t)((uint)dlcId));
		}

		// Token: 0x040024E0 RID: 9440
		private static SteamworksWrapper instance;

		// Token: 0x040024E1 RID: 9441
		private Callback<GetAuthSessionTicketResponse_t> authSessionTicketResponseCallback;

		// Token: 0x040024E2 RID: 9442
		private Callback<MicroTxnAuthorizationResponse_t> microTxnAuthorizationResponseCallback;

		// Token: 0x040024E3 RID: 9443
		private Callback<UserAchievementStored_t> userAchievementStoredCallback;

		// Token: 0x040024E4 RID: 9444
		private Callback<UserStatsReceived_t> userStatsReceivedCallback;

		// Token: 0x040024E5 RID: 9445
		private CompositeDisposable disposables = new CompositeDisposable();

		// Token: 0x040024E6 RID: 9446
		private bool storeStats;

		// Token: 0x040024E7 RID: 9447
		private bool initCalled;

		// Token: 0x040024E8 RID: 9448
		private ulong gameId;

		// Token: 0x040024E9 RID: 9449
		private Logger logger;

		// Token: 0x02000A2F RID: 2607
		public class RemoteStorageCallWrapper : IObservable<Tuple<string, RemoteStorageFileWriteAsyncComplete_t>>
		{
			// Token: 0x06003127 RID: 12583 RVA: 0x000B8BB3 File Offset: 0x000B6DB3
			public RemoteStorageCallWrapper(string fileName, byte[] data, int dataLength)
			{
				this.fileName = fileName;
				this.data = data;
				this.dataLength = (uint)dataLength;
			}

			// Token: 0x06003128 RID: 12584 RVA: 0x000B8BD0 File Offset: 0x000B6DD0
			private void OnComplete(RemoteStorageFileWriteAsyncComplete_t result, bool failure)
			{
				if (failure || result.m_eResult != EResult.k_EResultOK)
				{
					this.observer.OnError(new Exception(string.Format("RemoteStorageFileWriteAsync failed for file [{0}]: [{1}]", this.fileName, result.m_eResult)));
					return;
				}
				this.observer.OnNext(new Tuple<string, RemoteStorageFileWriteAsyncComplete_t>(this.fileName, result));
				this.observer.OnCompleted();
			}

			// Token: 0x06003129 RID: 12585 RVA: 0x000B8C37 File Offset: 0x000B6E37
			public IDisposable Subscribe(IObserver<Tuple<string, RemoteStorageFileWriteAsyncComplete_t>> observer)
			{
				this.observer = observer;
				return Observable.Timer(TimeSpan.FromSeconds(0.0)).Subscribe(delegate(long _)
				{
					this.callResult = CallResult<RemoteStorageFileWriteAsyncComplete_t>.Create(null);
					this.handle = SteamRemoteStorage.FileWriteAsync(this.fileName, this.data, this.dataLength);
					this.callResult.Set(this.handle, new CallResult<RemoteStorageFileWriteAsyncComplete_t>.APIDispatchDelegate(this.OnComplete));
				});
			}

			// Token: 0x0400303A RID: 12346
			private CallResult<RemoteStorageFileWriteAsyncComplete_t> callResult;

			// Token: 0x0400303B RID: 12347
			private IObserver<Tuple<string, RemoteStorageFileWriteAsyncComplete_t>> observer;

			// Token: 0x0400303C RID: 12348
			private string fileName;

			// Token: 0x0400303D RID: 12349
			private byte[] data;

			// Token: 0x0400303E RID: 12350
			private uint dataLength;

			// Token: 0x0400303F RID: 12351
			private SteamAPICall_t handle;
		}
	}
}
