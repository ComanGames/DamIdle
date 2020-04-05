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
using Object = UnityEngine.Object;
using Random = System.Random;

// Token: 0x020000E1 RID: 225
public class Root : MonoBehaviour
{
	// Token: 0x06000607 RID: 1543 RVA: 0x00020628 File Offset: 0x0001E828
	private void Awake()
	{
		JsonMapper.RegisterImporter<int, double>((int value) => (double)value);
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		GameController.Instance.NavigationService.Init(this.NavigationCanvas);
		GameController.Instance.AnalyticService.Init(GameController.Instance);
		this.waitingForExternalEvents = 2;
		Object.DontDestroyOnLoad(this);
	}

	private void RestoreOrSetUsernameInPlayfab(string currentSavedPlayfabName, string platformName)
	{
		if (!string.IsNullOrEmpty(currentSavedPlayfabName))
		{
			GameController.Instance.UserDataService.UserName.Value = currentSavedPlayfabName;
			return;
		}
		UsernameScriptableObject usernameScriptableObject = Resources.Load<UsernameScriptableObject>("Usernames");
		Random random = new Random();
		string userName = string.Format("{0} {1} {2}", usernameScriptableObject.Adjectives[random.Next(usernameScriptableObject.Adjectives.Count)], usernameScriptableObject.Nouns[random.Next(usernameScriptableObject.Nouns.Count)], random.Next(1000));
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
	private GameObject HockeyAppAndroid;

	// Token: 0x04000576 RID: 1398
	[SerializeField]
	private GameObject HockeyAppIos;

	// Token: 0x04000577 RID: 1399
	// Token: 0x04000578 RID: 1400
	[SerializeField]
	private GameObject OneSignal;

	// Token: 0x04000579 RID: 1401
	[SerializeField]
	private Transform NavigationCanvas;
}
