using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using AdCap;
using HHTools.Navigation;
using LitJson;
using Platforms;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000E1 RID: 225
public class Root : MonoBehaviour
{
	// Token: 0x06000607 RID: 1543 RVA: 0x00020628 File Offset: 0x0001E828
	private void Awake()
	{
		JsonMapper.RegisterImporter<int, double>((int value) => (double)value);
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Object.Instantiate<GameObject>(this.IronSource, base.gameObject.transform, false);
		Object.Instantiate<GameObject>(this.KongGameObject, base.gameObject.transform, false);
		GameController.Instance.NavigationService.Init(this.NavigationCanvas);
		GameController.Instance.AnalyticService.Init(GameController.Instance);
		this.waitingForExternalEvents = 2;
		Object.DontDestroyOnLoad(this);
	}

	// Token: 0x06000608 RID: 1544 RVA: 0x000206D4 File Offset: 0x0001E8D4
	// Token: 0x06000609 RID: 1545 RVA: 0x00020784 File Offset: 0x0001E984
	private void OnPlayFabLogin(LoginResult loginResult)
	{
		string displayName = loginResult.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
		string displayName2 = Helper.GetPlatformAccount().Active.DisplayName;
		this.RestoreOrSetUsernameInPlayfab(displayName, displayName2);
		this.waitingForExternalEvents--;
		GetPlayerCombinedInfoResultPayload infoResultPayload = loginResult.InfoResultPayload;
		uint? num;
		if (infoResultPayload == null)
		{
			num = null;
		}
		else
		{
			PlayerProfileModel playerProfile = infoResultPayload.PlayerProfile;
			num = ((playerProfile != null) ? playerProfile.TotalValueToDateInUSD : null);
		}
		uint value = num ?? 0U;
		GameController.Instance.DataService.StorePlayValueUSD(value);
		UserTargetingService.InitializePlayFabParameters(loginResult);
		this.RetrieveUserTargetingInfoThenContinue(0);
	}

	// Token: 0x0600060A RID: 1546 RVA: 0x00020830 File Offset: 0x0001EA30
	private void RestoreOrSetUsernameInPlayfab(string currentSavedPlayfabName, string platformName)
	{
		if (!string.IsNullOrEmpty(currentSavedPlayfabName))
		{
			GameController.Instance.UserDataService.UserName.Value = currentSavedPlayfabName;
			return;
		}
		if (string.IsNullOrEmpty(platformName))
		{
			this.UpdateUsernameInPlayfab(platformName);
			return;
		}
		UsernameScriptableObject usernameScriptableObject = Resources.Load<UsernameScriptableObject>("Usernames");
		Random random = new Random();
		string userName = string.Format("{0} {1} {2}", usernameScriptableObject.Adjectives[random.Next(usernameScriptableObject.Adjectives.Count)], usernameScriptableObject.Nouns[random.Next(usernameScriptableObject.Nouns.Count)], random.Next(1000));
		this.UpdateUsernameInPlayfab(userName);
	}

	// Token: 0x0600060B RID: 1547 RVA: 0x000208D8 File Offset: 0x0001EAD8
	private void UpdateUsernameInPlayfab(string userName)
	{
		Helper.GetPlatformAccount().PlayFab.UpdateUserTitleDisplayName(userName, delegate(UpdateUserTitleDisplayNameResult result)
		{
			GameController.Instance.UserDataService.UserName.Value = userName;
		}, delegate(PlayFabError error)
		{
			Debug.LogError("Error updating displayname");
		});
	}

	// Token: 0x0600060C RID: 1548 RVA: 0x00020934 File Offset: 0x0001EB34
	private void ExecuteUserTargetingInfoRequest(Action<ExecuteCloudScriptResult> onResult, Action<PlayFabError> error)
	{
		Dictionary<string, string> functionParameters = new Dictionary<string, string>
		{
			{
				"DataVersion",
				Helper.GetPlatformAccount().DataVersion
			},
			{
				"ClientVersion",
				Application.version
			}
		};
		Helper.GetPlatformAccount().PlayFab.ExecuteCloudScript("GetUserTargetingInfo", functionParameters, onResult, error);
	}

	// Token: 0x0600060D RID: 1549 RVA: 0x00020984 File Offset: 0x0001EB84
	private void RetrieveUserTargetingInfoThenContinue(int retryCount)
	{
		Action <>9__2;
		this.ExecuteUserTargetingInfoRequest(delegate(ExecuteCloudScriptResult result)
		{
			string text = "";
			try
			{
				text = result.FunctionResult.ToString();
				UserTargetingInfoRequestResult userTargetingInfoRequestResult = JsonMapper.ToObject<UserTargetingInfoRequestResult>(text);
				if (string.IsNullOrEmpty(userTargetingInfoRequestResult.ErrorMessage))
				{
					GameController.Instance.UserDataService.HandleUserTargetingInfo(userTargetingInfoRequestResult);
					GameController instance = GameController.Instance;
					Action onComplete;
					if ((onComplete = <>9__2) == null)
					{
						onComplete = (<>9__2 = delegate()
						{
							this.waitingForExternalEvents--;
						});
					}
					instance.Init(onComplete);
				}
				else
				{
					this.RetrievingABTestGroupErrorOccured("Cloud Error retrieving UserTargetingInfoRequestResult", "Error: " + userTargetingInfoRequestResult.ErrorMessage, retryCount);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				this.RetrievingABTestGroupErrorOccured("Error deserializing UserTargetingInfoRequestResult", "Data: " + text, retryCount);
			}
		}, delegate(PlayFabError error)
		{
			this.RetrievingABTestGroupErrorOccured("Failed Control Group Fetch", "Root", retryCount);
		});
	}

	// Token: 0x0600060E RID: 1550 RVA: 0x000209C4 File Offset: 0x0001EBC4
	private void RetrievingABTestGroupErrorOccured(string errorReasonForAnalytics, string source, int retryCounts)
	{
		KongLogUtil.LogConsole(string.Concat(new object[]
		{
			"[Hippo] Attempt:",
			retryCounts,
			" Retrieving Control Groups Failed: ",
			source
		}));
		if (retryCounts >= 2)
		{
			Root.ShowConnectionErrorAndForceClose(errorReasonForAnalytics, source);
			return;
		}
		retryCounts++;
		Debug.Log("Retrying to retrieve AB Test Group. Attempt:" + retryCounts);
		this.RetrieveUserTargetingInfoThenContinue(retryCounts);
	}

	// Token: 0x0600060F RID: 1551 RVA: 0x00020A2B File Offset: 0x0001EC2B
	private void OnLoginFailed(PlayFabError error)
	{
		Debug.LogErrorFormat("Locking out, title=[{0}], error=[{1}]", new object[]
		{
			"Unable to authenticate",
			error.ToString()
		});
		Root.ShowConnectionErrorAndForceClose("Failed to Login", (error == null) ? "Root" : error.ErrorMessage);
	}

	// Token: 0x06000610 RID: 1552 RVA: 0x00020A68 File Offset: 0x0001EC68
	private void OnPlatformInitialized(bool isInitialized)
	{
		if (!isInitialized)
		{
			return;
		}
		this.waitingForExternalEvents--;
	}

	// Token: 0x06000611 RID: 1553 RVA: 0x00020A7C File Offset: 0x0001EC7C
	private IEnumerator Start()
	{
		while (this.waitingForExternalEvents > 0)
		{
			yield return null;
		}
		SceneManager.LoadSceneAsync("LoadScene");
		yield break;
	}

	// Token: 0x06000612 RID: 1554 RVA: 0x00020A8C File Offset: 0x0001EC8C
	public static void ShowConnectionErrorAndForceClose(string errorReasonForanalytic, string source)
	{
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Connection Error", "Unable to connect with the server. Please ensure you have a stable internet or data connection and try again.", new Action(Application.Quit), PopupModal.PopupOptions.OK, "Ok", "", false, null, "");
		try
		{
			GameController.Instance.AnalyticService.SendTaskCompleteEvent("ForceClose", errorReasonForanalytic, source);
		}
		catch (Exception ex)
		{
			Debug.LogError("Kong Analytics not initialized. Trying to send analytic with no internet - " + ex.Message);
		}
	}

	// Token: 0x04000572 RID: 1394
	public const string CLOUD_SCRIPT_FUNCTION = "GetUserTargetingInfo";

	// Token: 0x04000573 RID: 1395
	[HideInInspector]
	public int waitingForExternalEvents;

	// Token: 0x04000574 RID: 1396
	[SerializeField]
	private GameObject KongGameObject;

	// Token: 0x04000575 RID: 1397
	[SerializeField]
	private GameObject HockeyAppAndroid;

	// Token: 0x04000576 RID: 1398
	[SerializeField]
	private GameObject HockeyAppIos;

	// Token: 0x04000577 RID: 1399
	[SerializeField]
	private GameObject IronSource;

	// Token: 0x04000578 RID: 1400
	[SerializeField]
	private GameObject OneSignal;

	// Token: 0x04000579 RID: 1401
	[SerializeField]
	private Transform NavigationCanvas;
}
