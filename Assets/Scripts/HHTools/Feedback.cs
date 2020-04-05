using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HHTools.MiniJSON;
using UnityEngine;

namespace HHTools
{
	// Token: 0x020006DF RID: 1759
	public class Feedback
	{
		// Token: 0x06002471 RID: 9329 RVA: 0x0009DAE7 File Offset: 0x0009BCE7
		public static IEnumerator SubmitFeedback(int gameId, int promoId, string feedback, Action onComplete = null, Action onError = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"g",
					gameId
				},
				{
					"p",
					promoId
				},
				{
					"f",
					feedback
				}
			};
			if (Application.isEditor)
			{
				dictionary.Add("t", 1);
			}
			Dictionary<string, string> headers = new Dictionary<string, string>
			{
				{
					"Content-Type",
					"application/json"
				}
			};
			WWW w = new WWW("https://api.hyperhippo.ca/feedback/v1/feedback", Encoding.UTF8.GetBytes(Json.Serialize(dictionary)), headers);
			yield return w;
			if (!string.IsNullOrEmpty(w.error))
			{
				Debug.LogError("[Feedback] " + w.error);
				if (onError != null)
				{
					onError();
				}
			}
			if (onComplete != null)
			{
				onComplete();
			}
			yield break;
		}

		// Token: 0x0400251A RID: 9498
		private const string FeedbackServerUrl = "https://api.hyperhippo.ca/feedback/v1/feedback";
	}
}
