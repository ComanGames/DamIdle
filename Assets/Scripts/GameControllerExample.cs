using System;
using System.Collections.Generic;
using OneSignalPush.MiniJSON;
using UnityEngine;

// Token: 0x0200013B RID: 315
public class GameControllerExample : MonoBehaviour
{
	// Token: 0x0600097A RID: 2426 RVA: 0x0002C01C File Offset: 0x0002A21C
	private void Start()
	{
		GameControllerExample.extraMessage = null;
		OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.VERBOSE, OneSignal.LOG_LEVEL.NONE);
		OneSignal.SetRequiresUserPrivacyConsent(GameControllerExample.requiresUserPrivacyConsent);
		OneSignal.StartInit("78e8aff3-7ce2-401f-9da0-2d41f287ebaf", null).HandleNotificationReceived(new OneSignal.NotificationReceived(GameControllerExample.HandleNotificationReceived)).HandleNotificationOpened(new OneSignal.NotificationOpened(GameControllerExample.HandleNotificationOpened)).EndInit();
		OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;
		OneSignal.permissionObserver += this.OneSignal_permissionObserver;
		OneSignal.subscriptionObserver += this.OneSignal_subscriptionObserver;
		OneSignal.emailSubscriptionObserver += this.OneSignal_emailSubscriptionObserver;
		OneSignal.GetPermissionSubscriptionState();
	}

	// Token: 0x0600097B RID: 2427 RVA: 0x0002C0B4 File Offset: 0x0002A2B4
	private void OneSignal_subscriptionObserver(OSSubscriptionStateChanges stateChanges)
	{
		Debug.Log("SUBSCRIPTION stateChanges: " + stateChanges);
		Debug.Log("SUBSCRIPTION stateChanges.to.userId: " + stateChanges.to.userId);
		Debug.Log("SUBSCRIPTION stateChanges.to.subscribed: " + stateChanges.to.subscribed.ToString());
	}

	// Token: 0x0600097C RID: 2428 RVA: 0x0002C10A File Offset: 0x0002A30A
	private void OneSignal_permissionObserver(OSPermissionStateChanges stateChanges)
	{
		Debug.Log("PERMISSION stateChanges.from.status: " + stateChanges.from.status);
		Debug.Log("PERMISSION stateChanges.to.status: " + stateChanges.to.status);
	}

	// Token: 0x0600097D RID: 2429 RVA: 0x0002C14C File Offset: 0x0002A34C
	private void OneSignal_emailSubscriptionObserver(OSEmailSubscriptionStateChanges stateChanges)
	{
		Debug.Log("EMAIL stateChanges.from.status: " + stateChanges.from.emailUserId + ", " + stateChanges.from.emailAddress);
		Debug.Log("EMAIL stateChanges.to.status: " + stateChanges.to.emailUserId + ", " + stateChanges.to.emailAddress);
	}

	// Token: 0x0600097E RID: 2430 RVA: 0x0002C1B0 File Offset: 0x0002A3B0
	private static void HandleNotificationReceived(OSNotification notification)
	{
		OSNotificationPayload payload = notification.payload;
		string body = payload.body;
		MonoBehaviour.print("GameControllerExample:HandleNotificationReceived: " + body);
		MonoBehaviour.print("displayType: " + notification.displayType);
		GameControllerExample.extraMessage = "Notification received with text: " + body;
		Dictionary<string, object> additionalData = payload.additionalData;
		if (additionalData == null)
		{
			Debug.Log("[HandleNotificationReceived] Additional Data == null");
			return;
		}
		Debug.Log("[HandleNotificationReceived] message " + body + ", additionalData: " + Json.Serialize(additionalData));
	}

	// Token: 0x0600097F RID: 2431 RVA: 0x0002C234 File Offset: 0x0002A434
	public static void HandleNotificationOpened(OSNotificationOpenedResult result)
	{
		OSNotificationPayload payload = result.notification.payload;
		string body = payload.body;
		string actionID = result.action.actionID;
		MonoBehaviour.print("GameControllerExample:HandleNotificationOpened: " + body);
		GameControllerExample.extraMessage = "Notification opened with text: " + body;
		Dictionary<string, object> additionalData = payload.additionalData;
		if (additionalData == null)
		{
			Debug.Log("[HandleNotificationOpened] Additional Data == null");
		}
		else
		{
			Debug.Log("[HandleNotificationOpened] message " + body + ", additionalData: " + Json.Serialize(additionalData));
		}
		if (actionID != null)
		{
			GameControllerExample.extraMessage = "Pressed ButtonId: " + actionID;
		}
	}

	// Token: 0x06000980 RID: 2432 RVA: 0x0002C2C4 File Offset: 0x0002A4C4
	private void OnGUI()
	{
		GUIStyle guistyle = new GUIStyle("button");
		guistyle.fontSize = 30;
		GUIStyle guistyle2 = new GUIStyle("box");
		guistyle2.fontSize = 30;
		new GUIStyle("textField").fontSize = 30;
		float x = 50f;
		float width = (float)Screen.width - 120f;
		float width2 = (float)Screen.width - 20f;
		float num = 120f;
		float num2 = GameControllerExample.requiresUserPrivacyConsent ? 720f : 630f;
		float num3 = 200f;
		float num4 = 90f;
		float height = 60f;
		GUI.Box(new Rect(10f, num, width2, num2), "Test Menu", guistyle2);
		float num5 = 0f;
		if (GUI.Button(new Rect(x, num3 + num5 * num4, width, height), "SendTags", guistyle))
		{
			OneSignal.SendTag("UnityTestKey", "TestValue");
			OneSignal.SendTags(new Dictionary<string, string>
			{
				{
					"UnityTestKey2",
					"value2"
				},
				{
					"UnityTestKey3",
					"value3"
				}
			});
		}
		num5 += 1f;
		if (GUI.Button(new Rect(x, num3 + num5 * num4, width, height), "GetIds", guistyle))
		{
			OneSignal.IdsAvailable(delegate(string userId, string pushToken)
			{
				GameControllerExample.extraMessage = "UserID:\n" + userId + "\n\nPushToken:\n" + pushToken;
			});
		}
		num5 += 1f;
		if (GUI.Button(new Rect(x, num3 + num5 * num4, width, height), "TestNotification", guistyle))
		{
			GameControllerExample.extraMessage = "Waiting to get a OneSignal userId. Uncomment OneSignal.SetLogLevel in the Start method if it hangs here to debug the issue.";
			OneSignal.IdsAvailable(delegate(string userId, string pushToken)
			{
				if (pushToken != null)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary["contents"] = new Dictionary<string, string>
					{
						{
							"en",
							"Test Message"
						}
					};
					dictionary["include_player_ids"] = new List<string>
					{
						userId
					};
					dictionary["send_after"] = DateTime.Now.ToUniversalTime().AddSeconds(30.0).ToString("U");
					GameControllerExample.extraMessage = "Posting test notification now.";
					OneSignal.PostNotification(dictionary, delegate(Dictionary<string, object> responseSuccess)
					{
						GameControllerExample.extraMessage = "Notification posted successful! Delayed by about 30 secounds to give you time to press the home button to see a notification vs an in-app alert.\n" + Json.Serialize(responseSuccess);
					}, delegate(Dictionary<string, object> responseFailure)
					{
						GameControllerExample.extraMessage = "Notification failed to post:\n" + Json.Serialize(responseFailure);
					});
					return;
				}
				GameControllerExample.extraMessage = "ERROR: Device is not registered.";
			});
		}
		num5 += 1f;
		this.email = GUI.TextField(new Rect(x, num3 + num5 * num4, width, height), this.email, guistyle);
		num5 += 1f;
		if (GUI.Button(new Rect(x, num3 + num5 * num4, width, height), "SetEmail", guistyle))
		{
			GameControllerExample.extraMessage = "Setting email to " + this.email;
			OneSignal.SetEmail(this.email, delegate()
			{
				Debug.Log("Successfully set email");
			}, delegate(Dictionary<string, object> error)
			{
				Debug.Log("Encountered error setting email: " + Json.Serialize(error));
			});
		}
		num5 += 1f;
		if (GUI.Button(new Rect(x, num3 + num5 * num4, width, height), "LogoutEmail", guistyle))
		{
			GameControllerExample.extraMessage = "Logging Out of example@example.com";
			OneSignal.LogoutEmail(delegate()
			{
				Debug.Log("Successfully logged out of email");
			}, delegate(Dictionary<string, object> error)
			{
				Debug.Log("Encountered error logging out of email: " + Json.Serialize(error));
			});
		}
		if (GameControllerExample.requiresUserPrivacyConsent)
		{
			num5 += 1f;
			if (GUI.Button(new Rect(x, num3 + num5 * num4, width, height), OneSignal.UserProvidedConsent() ? "Revoke Privacy Consent" : "Provide Privacy Consent", guistyle))
			{
				GameControllerExample.extraMessage = "Providing user privacy consent";
				OneSignal.UserDidProvideConsent(!OneSignal.UserProvidedConsent());
			}
		}
		if (GameControllerExample.extraMessage != null)
		{
			guistyle2.alignment = TextAnchor.UpperLeft;
			guistyle2.wordWrap = true;
			GUI.Box(new Rect(10f, num + num2 + 20f, (float)(Screen.width - 20), (float)Screen.height - (num + num2 + 40f)), GameControllerExample.extraMessage, guistyle2);
		}
	}

	// Token: 0x04000802 RID: 2050
	private static string extraMessage;

	// Token: 0x04000803 RID: 2051
	public string email = "Email Address";

	// Token: 0x04000804 RID: 2052
	private static bool requiresUserPrivacyConsent;
}
