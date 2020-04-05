using System;
using System.Collections.Generic;
using Platforms.Logger;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Events;
using PlayFab.EventsModels;
using PlayFab.Internal;
using PlayFab.Json;
using PlayFab.SharedModels;
using UniRx;
using UnityEngine;

namespace Platforms
{
	// Token: 0x020006CE RID: 1742
	public class PlayFabWrapper
	{
		// Token: 0x060023B0 RID: 9136 RVA: 0x0009AF9C File Offset: 0x0009919C
		public PlayFabWrapper(string titleId, ITransportPlugin http = null)
		{
			this.logger = Platforms.Logger.Logger.GetLogger(this);
			this.logger.Info("PlayFab titleId set to [{0}]", new object[]
			{
				titleId
			});
			PlayFabSettings.TitleId = titleId;
			PlayFabSettings.RequestType = WebRequestType.UnityWebRequest;
			switch (this.logger.CurrentLevel)
			{
			case LogLevel.Trace:
				PlayFabSettings.LogLevel = PlayFabLogLevel.All;
				break;
			case LogLevel.Debug:
				PlayFabSettings.LogLevel = PlayFabLogLevel.Debug;
				break;
			case LogLevel.Info:
				PlayFabSettings.LogLevel = PlayFabLogLevel.Info;
				break;
			case LogLevel.Warning:
				PlayFabSettings.LogLevel = PlayFabLogLevel.Warning;
				break;
			case LogLevel.Error:
				PlayFabSettings.LogLevel = PlayFabLogLevel.Error;
				break;
			default:
				PlayFabSettings.LogLevel = PlayFabLogLevel.None;
				break;
			}
			if (http != null)
			{
				PluginManager.SetPlugin(http, PluginContract.PlayFab_Transport, "");
			}
			PlayFabHttp.ApiProcessingEventHandler += this.OnPlayFabEvent;
			this.events = PlayFabEvents.Init();
			this.events.OnGlobalErrorEvent += this.OnPlayFabError;
		}

		// Token: 0x060023B1 RID: 9137 RVA: 0x0009B0BA File Offset: 0x000992BA
		public void Dispose()
		{
			PlayFabHttp.ApiProcessingEventHandler -= this.OnPlayFabEvent;
			this.events.OnGlobalErrorEvent -= this.OnPlayFabError;
			this.disposables.Dispose();
		}

		// Token: 0x060023B2 RID: 9138 RVA: 0x0009B0EF File Offset: 0x000992EF
		public bool IsClientLoggedIn()
		{
			return PlayFabClientAPI.IsClientLoggedIn();
		}

		// Token: 0x060023B3 RID: 9139 RVA: 0x0009B0F8 File Offset: 0x000992F8
		public void LoginWithMobile(Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			if (Application.isEditor)
			{
				this.LoginWithPlayFabDeveloper(onResult, onError);
				return;
			}
			RuntimePlatform platform = Application.platform;
			if (platform != RuntimePlatform.IPhonePlayer)
			{
				if (platform == RuntimePlatform.Android)
				{
					this.LoginWithAndroidDeviceId(SystemInfo.deviceUniqueIdentifier, onResult, onError);
					return;
				}
			}
			else
			{
				this.LoginWithIosDeviceId(SystemInfo.deviceUniqueIdentifier, onResult, onError);
			}
		}

		// Token: 0x060023B4 RID: 9140 RVA: 0x0009B13F File Offset: 0x0009933F
		public void LoginWithPlayFab(string username, string password, Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest
			{
				TitleId = PlayFabSettings.TitleId,
				Username = username,
				Password = password,
				InfoRequestParameters = this.StandardLoginRequestParams
			}, onResult, onError, null, null);
		}

		// Token: 0x060023B5 RID: 9141 RVA: 0x00002718 File Offset: 0x00000918
		public void LoginWithPlayFabDeveloper(Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
		}

		// Token: 0x060023B6 RID: 9142 RVA: 0x0009B175 File Offset: 0x00099375
		public void LoginWithAndroidDeviceId(string deviceId, Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LoginWithAndroidDeviceID(new LoginWithAndroidDeviceIDRequest
			{
				TitleId = PlayFabSettings.TitleId,
				AndroidDeviceId = deviceId,
				InfoRequestParameters = this.StandardLoginRequestParams,
				CreateAccount = new bool?(true)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023B7 RID: 9143 RVA: 0x0009B1AF File Offset: 0x000993AF
		public void LoginWithGoogleAccount(string serverAuthCode, Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest
			{
				TitleId = PlayFabSettings.TitleId,
				ServerAuthCode = serverAuthCode,
				InfoRequestParameters = this.StandardLoginRequestParams,
				CreateAccount = new bool?(true)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023B8 RID: 9144 RVA: 0x0009B1E9 File Offset: 0x000993E9
		public void LoginWithIosDeviceId(string deviceId, Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LoginWithIOSDeviceID(new LoginWithIOSDeviceIDRequest
			{
				TitleId = PlayFabSettings.TitleId,
				DeviceId = deviceId,
				InfoRequestParameters = this.StandardLoginRequestParams,
				CreateAccount = new bool?(true)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023B9 RID: 9145 RVA: 0x0009B223 File Offset: 0x00099423
		public void LoginWithGameCenter(string playerId, Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LoginWithGameCenter(new LoginWithGameCenterRequest
			{
				TitleId = PlayFabSettings.TitleId,
				PlayerId = playerId,
				InfoRequestParameters = this.StandardLoginRequestParams,
				CreateAccount = new bool?(true)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023BA RID: 9146 RVA: 0x0009B25D File Offset: 0x0009945D
		public void LoginWithSteam(string ticket, Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LoginWithSteam(new LoginWithSteamRequest
			{
				TitleId = PlayFabSettings.TitleId,
				SteamTicket = ticket,
				InfoRequestParameters = this.StandardLoginRequestParams,
				CreateAccount = new bool?(true)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023BB RID: 9147 RVA: 0x0009B297 File Offset: 0x00099497
		public void LoginWithFacebook(string ticket, Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest
			{
				TitleId = PlayFabSettings.TitleId,
				AccessToken = ticket,
				InfoRequestParameters = this.StandardLoginRequestParams,
				CreateAccount = new bool?(true)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023BC RID: 9148 RVA: 0x0009B2D4 File Offset: 0x000994D4
		public void LoginWithKongregate(string ticket, string kongregateID, Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LoginWithKongregate(new LoginWithKongregateRequest
			{
				TitleId = PlayFabSettings.TitleId,
				AuthTicket = ticket,
				KongregateId = kongregateID,
				InfoRequestParameters = this.StandardLoginRequestParams,
				CreateAccount = new bool?(true)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023BD RID: 9149 RVA: 0x0009B321 File Offset: 0x00099521
		public void LoginWithCustomId(string customId, Action<LoginResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
			{
				TitleId = PlayFabSettings.TitleId,
				CustomId = customId,
				InfoRequestParameters = this.StandardLoginRequestParams,
				CreateAccount = new bool?(true)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023BE RID: 9150 RVA: 0x0009B35B File Offset: 0x0009955B
		public void LinkCustomId(string customId, bool forceLink, Action<LinkCustomIDResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest
			{
				CustomId = customId,
				ForceLink = new bool?(forceLink)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023BF RID: 9151 RVA: 0x0009B37F File Offset: 0x0009957F
		public void UnlinkCustomId(Action<UnlinkCustomIDResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest(), onResult, onError, null, null);
		}

		// Token: 0x060023C0 RID: 9152 RVA: 0x0009B38F File Offset: 0x0009958F
		public void LinkAndroidDeviceId(string deviceId, bool forceLink, Action<LinkAndroidDeviceIDResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LinkAndroidDeviceID(new LinkAndroidDeviceIDRequest
			{
				AndroidDeviceId = deviceId,
				ForceLink = new bool?(forceLink)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023C1 RID: 9153 RVA: 0x0009B3B3 File Offset: 0x000995B3
		public void UnlinkAndroidDeviceId(Action<UnlinkAndroidDeviceIDResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.UnlinkAndroidDeviceID(new UnlinkAndroidDeviceIDRequest(), onResult, onError, null, null);
		}

		// Token: 0x060023C2 RID: 9154 RVA: 0x0009B3C3 File Offset: 0x000995C3
		public void LinkGoogleAccount(string serverAuthCode, bool forceLink, Action<LinkGoogleAccountResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LinkGoogleAccount(new LinkGoogleAccountRequest
			{
				ServerAuthCode = serverAuthCode,
				ForceLink = new bool?(forceLink)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023C3 RID: 9155 RVA: 0x0009B3E7 File Offset: 0x000995E7
		public void UnlinkGoogleAccount(Action<UnlinkGoogleAccountResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.UnlinkGoogleAccount(new UnlinkGoogleAccountRequest(), onResult, onError, null, null);
		}

		// Token: 0x060023C4 RID: 9156 RVA: 0x0009B3F7 File Offset: 0x000995F7
		public void LinkIosDeviceId(string deviceId, bool forceLink, Action<LinkIOSDeviceIDResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LinkIOSDeviceID(new LinkIOSDeviceIDRequest
			{
				DeviceId = deviceId,
				ForceLink = new bool?(forceLink)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023C5 RID: 9157 RVA: 0x0009B41B File Offset: 0x0009961B
		public void UnlinkIosDeviceId(Action<UnlinkIOSDeviceIDResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.UnlinkIOSDeviceID(new UnlinkIOSDeviceIDRequest(), onResult, onError, null, null);
		}

		// Token: 0x060023C6 RID: 9158 RVA: 0x0009B42B File Offset: 0x0009962B
		public void LinkGameCenterAccount(string gameCenterId, bool forceLink, Action<LinkGameCenterAccountResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.LinkGameCenterAccount(new LinkGameCenterAccountRequest
			{
				GameCenterId = gameCenterId,
				ForceLink = new bool?(forceLink)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023C7 RID: 9159 RVA: 0x0009B44F File Offset: 0x0009964F
		public void UnlinkGameCenterAccount(Action<UnlinkGameCenterAccountResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.UnlinkGameCenterAccount(new UnlinkGameCenterAccountRequest(), onResult, onError, null, null);
		}

		// Token: 0x060023C8 RID: 9160 RVA: 0x0009B45F File Offset: 0x0009965F
		public void GetTitleData(Action<GetTitleDataResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), onResult, onError, null, null);
		}

		// Token: 0x060023C9 RID: 9161 RVA: 0x0009B46F File Offset: 0x0009966F
		public void GetTitleData(string key, Action<GetTitleDataResult> onResult, Action<PlayFabError> onError)
		{
			this.GetTitleData(new List<string>
			{
				key
			}, onResult, onError);
		}

		// Token: 0x060023CA RID: 9162 RVA: 0x0009B485 File Offset: 0x00099685
		public void GetTitleData(List<string> keys, Action<GetTitleDataResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetTitleData(new GetTitleDataRequest
			{
				Keys = keys
			}, onResult, onError, null, null);
		}

		// Token: 0x060023CB RID: 9163 RVA: 0x0009B49C File Offset: 0x0009969C
		public void GetTitleNews(int newsCount, Action<GetTitleNewsResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetTitleNews(new GetTitleNewsRequest
			{
				Count = new int?(newsCount)
			}, onResult, onError, null, null);
		}

		// Token: 0x060023CC RID: 9164 RVA: 0x0009B4B8 File Offset: 0x000996B8
		public void GetTime(Action<GetTimeResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetTime(new GetTimeRequest(), onResult, onError, null, null);
		}

		// Token: 0x060023CD RID: 9165 RVA: 0x0009B4C8 File Offset: 0x000996C8
		public void GetCatalogItems(string catalogVersion, Action<GetCatalogItemsResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest
			{
				CatalogVersion = catalogVersion
			}, onResult, onError, null, null);
		}

		// Token: 0x060023CE RID: 9166 RVA: 0x0009B4DF File Offset: 0x000996DF
		public void GetStoreItems(string storeId, string catalogVersion, Action<GetStoreItemsResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetStoreItems(new GetStoreItemsRequest
			{
				StoreId = storeId,
				CatalogVersion = catalogVersion
			}, onResult, onError, null, null);
		}

		// Token: 0x060023CF RID: 9167 RVA: 0x0009B4FE File Offset: 0x000996FE
		public void GetUserData(Action<GetUserDataResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetUserData(new GetUserDataRequest(), onResult, onError, null, null);
		}

		// Token: 0x060023D0 RID: 9168 RVA: 0x0009B50E File Offset: 0x0009970E
		public void GetUserData(List<string> keys, Action<GetUserDataResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetUserData(new GetUserDataRequest
			{
				Keys = keys
			}, onResult, onError, null, null);
		}

		// Token: 0x060023D1 RID: 9169 RVA: 0x0009B525 File Offset: 0x00099725
		public void GetUserReadOnlyData(Action<GetUserDataResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest(), onResult, onError, null, null);
		}

		// Token: 0x060023D2 RID: 9170 RVA: 0x0009B535 File Offset: 0x00099735
		public void GetUserReadOnlyData(string key, Action<GetUserDataResult> onResult, Action<PlayFabError> onError)
		{
			this.GetUserReadOnlyData(new List<string>
			{
				key
			}, onResult, onError);
		}

		// Token: 0x060023D3 RID: 9171 RVA: 0x0009B54B File Offset: 0x0009974B
		public void GetUserReadOnlyData(List<string> keys, Action<GetUserDataResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest
			{
				Keys = keys
			}, onResult, onError, null, null);
		}

		// Token: 0x060023D4 RID: 9172 RVA: 0x0009B562 File Offset: 0x00099762
		public void UpdateUserData(string key, string data, Action<UpdateUserDataResult> onResult, Action<PlayFabError> onError)
		{
			this.UpdateUserData(new Dictionary<string, string>
			{
				{
					key,
					data
				}
			}, onResult, onError);
		}

		// Token: 0x060023D5 RID: 9173 RVA: 0x0009B57A File Offset: 0x0009977A
		public void UpdateUserData(Dictionary<string, string> data, Action<UpdateUserDataResult> onResult, Action<PlayFabError> error)
		{
			PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
			{
				Data = data,
				Permission = new UserDataPermission?(UserDataPermission.Private)
			}, onResult, error, null, null);
		}

		// Token: 0x060023D6 RID: 9174 RVA: 0x0009B59D File Offset: 0x0009979D
		public void UpdateUserTitleDisplayName(string userName, Action<UpdateUserTitleDisplayNameResult> onResult, Action<PlayFabError> error)
		{
			PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
			{
				DisplayName = userName
			}, onResult, error, null, null);
		}

		// Token: 0x060023D7 RID: 9175 RVA: 0x0009B5B4 File Offset: 0x000997B4
		public void GetUserInventory(Action<GetUserInventoryResult> onResult, Action<PlayFabError> error)
		{
			PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), onResult, error, null, null);
		}

		// Token: 0x060023D8 RID: 9176 RVA: 0x0009B5C4 File Offset: 0x000997C4
		public void UnlockContainerItem(string itemId, string catalogId, Action<UnlockContainerItemResult> onResult, Action<PlayFabError> error)
		{
			PlayFabClientAPI.UnlockContainerItem(new UnlockContainerItemRequest
			{
				ContainerItemId = itemId,
				CatalogVersion = catalogId
			}, onResult, error, null, null);
		}

		// Token: 0x060023D9 RID: 9177 RVA: 0x0009B5E3 File Offset: 0x000997E3
		public void ConsumeItem(string itemInstanceId, int consumeCount, Action<ConsumeItemResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.ConsumeItem(new ConsumeItemRequest
			{
				ItemInstanceId = itemInstanceId,
				ConsumeCount = consumeCount
			}, onResult, onError, null, null);
		}

		// Token: 0x060023DA RID: 9178 RVA: 0x0009B602 File Offset: 0x00099802
		public void ExecuteCloudScript(string functionName, Action<ExecuteCloudScriptResult> onResult, Action<PlayFabError> error)
		{
			this.ExecuteCloudScript(functionName, null, onResult, error);
		}

		// Token: 0x060023DB RID: 9179 RVA: 0x0009B610 File Offset: 0x00099810
		public void ExecuteCloudScript(string functionName, object functionParameters, Action<ExecuteCloudScriptResult> onResult, Action<PlayFabError> error)
		{
			PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
			{
				FunctionName = functionName,
				FunctionParameter = functionParameters,
				GeneratePlayStreamEvent = new bool?(true),
				RevisionSelection = new CloudScriptRevisionOption?(this.CloudScriptRevisionOption)
			}, delegate(ExecuteCloudScriptResult result)
			{
				if (result.Logs != null)
				{
					for (int i = 0; i < result.Logs.Count; i++)
					{
						string level = result.Logs[i].Level;
						if (!(level == "Debug"))
						{
							if (!(level == "Info"))
							{
								if (level == "Error")
								{
									this.logger.Error(result.Logs[i].Message);
								}
							}
							else
							{
								this.logger.Info(result.Logs[i].Message);
							}
						}
						else
						{
							this.logger.Debug(result.Logs[i].Message);
						}
					}
				}
				if (onResult != null)
				{
					onResult(result);
				}
			}, error, null, null);
		}

		// Token: 0x060023DC RID: 9180 RVA: 0x0009B676 File Offset: 0x00099876
		public void ExecuteCloudScript<T>(string functionName, Action<ExecuteCloudScriptResult> onResult, Action<PlayFabError> error)
		{
			this.ExecuteCloudScript<T>(functionName, null, onResult, error);
		}

		// Token: 0x060023DD RID: 9181 RVA: 0x0009B682 File Offset: 0x00099882
		public void ExecuteCloudScript<T>(string functionName, object functionParameters, Action<ExecuteCloudScriptResult> onResult, Action<PlayFabError> error)
		{
			PlayFabClientAPI.ExecuteCloudScript<T>(new ExecuteCloudScriptRequest
			{
				FunctionName = functionName,
				FunctionParameter = functionParameters,
				GeneratePlayStreamEvent = new bool?(true),
				RevisionSelection = new CloudScriptRevisionOption?(this.CloudScriptRevisionOption)
			}, onResult, error, null, null);
		}

		// Token: 0x060023DE RID: 9182 RVA: 0x0009B6BE File Offset: 0x000998BE
		public void StartPurchase(CatalogItem item, string store, Action<StartPurchaseResult> onResult, Action<PlayFabError> onError)
		{
			this.StartPurchase(item.ItemId, item.CatalogVersion, store, onResult, onError);
		}

		// Token: 0x060023DF RID: 9183 RVA: 0x0009B6D8 File Offset: 0x000998D8
		public void StartPurchase(string itemId, string catalogVersion, string store, Action<StartPurchaseResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.StartPurchase(new StartPurchaseRequest
			{
				CatalogVersion = catalogVersion,
				StoreId = store,
				Items = new List<ItemPurchaseRequest>
				{
					new ItemPurchaseRequest
					{
						ItemId = itemId,
						Quantity = 1U
					}
				}
			}, onResult, onError, null, null);
		}

		// Token: 0x060023E0 RID: 9184 RVA: 0x0009B727 File Offset: 0x00099927
		public void PayForPurchase(string orderId, string providerName, string currency, Action<PayForPurchaseResult> onResult, Action<PlayFabError> onError, string providerTransactionID = null)
		{
			PlayFabClientAPI.PayForPurchase(new PayForPurchaseRequest
			{
				OrderId = orderId,
				ProviderName = providerName,
				Currency = currency,
				ProviderTransactionId = providerTransactionID
			}, onResult, onError, null, null);
		}

		// Token: 0x060023E1 RID: 9185 RVA: 0x0009B756 File Offset: 0x00099956
		public void ConfirmPurchase(string orderId, Action<ConfirmPurchaseResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.ConfirmPurchase(new ConfirmPurchaseRequest
			{
				OrderId = orderId
			}, onResult, onError, null, null);
		}

		// Token: 0x060023E2 RID: 9186 RVA: 0x0009B76D File Offset: 0x0009996D
		public void PurchaseItem(string itemId, string catalogVersion, string storeId, string virtualCurrency, int price, Action<PurchaseItemResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
			{
				ItemId = itemId,
				CatalogVersion = catalogVersion,
				StoreId = storeId,
				Price = price,
				VirtualCurrency = virtualCurrency
			}, onResult, onError, null, null);
		}

		// Token: 0x060023E3 RID: 9187 RVA: 0x0009B7A4 File Offset: 0x000999A4
		public void ValidateGooglePlayPurchase(string receiptJson, string signature, Action<ValidateGooglePlayPurchaseResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.ValidateGooglePlayPurchase(new ValidateGooglePlayPurchaseRequest
			{
				ReceiptJson = receiptJson,
				Signature = signature
			}, onResult, onError, null, null);
		}

		// Token: 0x060023E4 RID: 9188 RVA: 0x0009B7C3 File Offset: 0x000999C3
		public void ValidateIOSReceipt(string receiptData, string currencyCode, int purchasePrice, Action<ValidateIOSReceiptResult> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.ValidateIOSReceipt(new ValidateIOSReceiptRequest
			{
				ReceiptData = receiptData,
				CurrencyCode = currencyCode,
				PurchasePrice = purchasePrice
			}, onResult, onError, null, null);
		}

		// Token: 0x060023E5 RID: 9189 RVA: 0x0009B7EA File Offset: 0x000999EA
		public void WritePlayerEvent(string eventName, Action<WriteEventResponse> onResult, Action<PlayFabError> onError)
		{
			this.WritePlayerEvent(eventName, null, onResult, onError);
		}

		// Token: 0x060023E6 RID: 9190 RVA: 0x0009B7F6 File Offset: 0x000999F6
		public void WritePlayerEvent(string eventName, Dictionary<string, object> body, Action<WriteEventResponse> onResult, Action<PlayFabError> onError)
		{
			PlayFabClientAPI.WritePlayerEvent(new WriteClientPlayerEventRequest
			{
				EventName = eventName,
				Body = body
			}, onResult, onError, null, null);
		}

		// Token: 0x060023E7 RID: 9191 RVA: 0x0009B815 File Offset: 0x00099A15
		public void WriteTelemetryEvents(WriteEventsRequest request, Action<WriteEventsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
		{
			PlayFabEventsAPI.WriteTelemetryEvents(request, resultCallback, errorCallback, customData, extraHeaders);
		}

		// Token: 0x060023E8 RID: 9192 RVA: 0x0009B823 File Offset: 0x00099A23
		public void GetPlayerSegments(Action<GetPlayerSegmentsResult> onResult, Action<PlayFabError> error)
		{
			PlayFabClientAPI.GetPlayerSegments(new GetPlayerSegmentsRequest(), onResult, error, null, null);
		}

		// Token: 0x060023E9 RID: 9193 RVA: 0x0009B834 File Offset: 0x00099A34
		public void GetLeaderboard(string id, Action<List<PlayerLeaderboardEntry>> onResult, Action<string> onError)
		{
			GetLeaderboardRequest getLeaderboardRequest = new GetLeaderboardRequest();
			getLeaderboardRequest.MaxResultsCount = new int?(100);
			getLeaderboardRequest.StatisticName = id;
			getLeaderboardRequest.StartPosition = 0;
			Action<PlayFabError> errorCallback = delegate(PlayFabError x)
			{
				if (onError != null)
				{
					onError(x.ErrorMessage);
				}
			};
			PlayFabClientAPI.GetLeaderboard(getLeaderboardRequest, delegate(GetLeaderboardResult result)
			{
				onResult(result.Leaderboard);
			}, errorCallback, null, null);
		}

		// Token: 0x060023EA RID: 9194 RVA: 0x0009B898 File Offset: 0x00099A98
		public void GetLeaderboardAroundPlayer(string id, Action<List<PlayerLeaderboardEntry>> onResult, Action<string> onError, int maxResult = 100)
		{
			maxResult = Mathf.Clamp(maxResult, 0, 100);
			GetLeaderboardAroundPlayerRequest getLeaderboardAroundPlayerRequest = new GetLeaderboardAroundPlayerRequest();
			getLeaderboardAroundPlayerRequest.MaxResultsCount = new int?(maxResult);
			getLeaderboardAroundPlayerRequest.StatisticName = id;
			Action<PlayFabError> errorCallback = delegate(PlayFabError x)
			{
				if (onError != null)
				{
					onError(x.ErrorMessage);
				}
			};
			PlayFabClientAPI.GetLeaderboardAroundPlayer(getLeaderboardAroundPlayerRequest, delegate(GetLeaderboardAroundPlayerResult result)
			{
				onResult(result.Leaderboard);
			}, errorCallback, null, null);
		}

		// Token: 0x060023EB RID: 9195 RVA: 0x0009B8FF File Offset: 0x00099AFF
		public void UpdatePlayerStatistic(StatisticUpdate statistic, Action callback, Action<string> onError)
		{
			this.UpdatePlayerStatistics(new List<StatisticUpdate>
			{
				statistic
			}, callback, onError);
		}

		// Token: 0x060023EC RID: 9196 RVA: 0x0009B918 File Offset: 0x00099B18
		public void UpdatePlayerStatistics(List<StatisticUpdate> statistics, Action callback, Action<string> onError)
		{
			UpdatePlayerStatisticsRequest updatePlayerStatisticsRequest = new UpdatePlayerStatisticsRequest();
			updatePlayerStatisticsRequest.Statistics = statistics;
			Action<PlayFabError> errorCallback = delegate(PlayFabError x)
			{
				if (onError != null)
				{
					onError(x.ErrorMessage);
				}
			};
			PlayFabClientAPI.UpdatePlayerStatistics(updatePlayerStatisticsRequest, delegate(UpdatePlayerStatisticsResult result)
			{
				callback();
			}, errorCallback, null, null);
		}

		// Token: 0x060023ED RID: 9197 RVA: 0x0009B968 File Offset: 0x00099B68
		private void OnPlayFabError(PlayFabRequestCommon request, PlayFabError error)
		{
			this.logger.Error("PlayFab request [{0}] threw error [{1}]: [{2}]\n\t[{3}]\n\t[{4}]\n", new object[]
			{
				request,
				error.Error,
				error.ErrorMessage,
				JsonWrapper.SerializeObject(error.ErrorDetails),
				JsonWrapper.SerializeObject(error.CustomData)
			});
		}

		// Token: 0x060023EE RID: 9198 RVA: 0x0009B9C4 File Offset: 0x00099BC4
		private void OnPlayFabEvent(ApiProcessingEventArgs e)
		{
			this.apiProcessingEvent.OnNext(e);
			if (e.EventType == ApiProcessingEventType.Pre)
			{
				if (this.logger.CurrentLevel <= LogLevel.Trace)
				{
					this.logger.Trace("SND >> PlayFab: [{0}]\n{1}", new object[]
					{
						e.Request,
						JsonWrapper.SerializeObject(e.Request)
					});
				}
				else
				{
					this.logger.Debug("SND >> PlayFab: [{0}]", new object[]
					{
						e.Request
					});
				}
				this.requestEventStream.OnNext(e.Request);
				return;
			}
			if (this.logger.CurrentLevel <= LogLevel.Trace)
			{
				this.logger.Trace("RCV << PlayFab: [{0}]\n{1}", new object[]
				{
					e.Result,
					JsonWrapper.SerializeObject(e.Result)
				});
			}
			else
			{
				this.logger.Debug("RCV << PlayFab: [{0}]: [{1}]", new object[]
				{
					e.Result,
					e.Result
				});
			}
			this.responseEventStream.OnNext(e.Result);
		}

		// Token: 0x060023EF RID: 9199 RVA: 0x0009BAC9 File Offset: 0x00099CC9
		public IObservable<T> RequestEventSteam<T>() where T : PlayFabRequestCommon
		{
			return this.requestEventStream.OfType<PlayFabRequestCommon, T>();
		}

		// Token: 0x060023F0 RID: 9200 RVA: 0x0009BAD6 File Offset: 0x00099CD6
		public IObservable<T> ResponseEventSteam<T>() where T : PlayFabResultCommon
		{
			return this.responseEventStream.OfType<PlayFabResultCommon, T>();
		}

		// Token: 0x060023F1 RID: 9201 RVA: 0x0009BAE3 File Offset: 0x00099CE3
		public IObservable<ApiProcessingEventArgs> ApiProcessingEvent<T>()
		{
			return (from v in this.apiProcessingEvent
			where v.Request is T
			select v).AsObservable<ApiProcessingEventArgs>();
		}

		// Token: 0x060023F2 RID: 9202 RVA: 0x0009BB14 File Offset: 0x00099D14
		public void SetLogLevel(LogLevel logLevel)
		{
			this.logger.SetLevel(logLevel);
		}

		// Token: 0x040024CB RID: 9419
		public CloudScriptRevisionOption CloudScriptRevisionOption;

		// Token: 0x040024CC RID: 9420
		public GetPlayerCombinedInfoRequestParams StandardLoginRequestParams = new GetPlayerCombinedInfoRequestParams
		{
			GetUserReadOnlyData = true
		};

		// Token: 0x040024CD RID: 9421
		private Subject<PlayFabRequestCommon> requestEventStream = new Subject<PlayFabRequestCommon>();

		// Token: 0x040024CE RID: 9422
		private Subject<PlayFabResultCommon> responseEventStream = new Subject<PlayFabResultCommon>();

		// Token: 0x040024CF RID: 9423
		private Subject<ApiProcessingEventArgs> apiProcessingEvent = new Subject<ApiProcessingEventArgs>();

		// Token: 0x040024D0 RID: 9424
		private CompositeDisposable disposables = new CompositeDisposable();

		// Token: 0x040024D1 RID: 9425
		private PlayFabEvents events;

		// Token: 0x040024D2 RID: 9426
		private Platforms.Logger.Logger logger;

		// Token: 0x02000A26 RID: 2598
		public class Exception : System.Exception
		{
			// Token: 0x0600310F RID: 12559 RVA: 0x000B8899 File Offset: 0x000B6A99
			public Exception(PlayFabError error)
			{
				this.Error = error;
			}

			// Token: 0x06003110 RID: 12560 RVA: 0x000B88A8 File Offset: 0x000B6AA8
			public override string ToString()
			{
				return this.Error.ToString();
			}

			// Token: 0x04003026 RID: 12326
			public readonly PlayFabError Error;
		}
	}
}
