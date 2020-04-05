using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Platforms;
using PlayFab;
using PlayFab.ClientModels;
using UniRx;
using UnityEngine;

// Token: 0x020001DA RID: 474
public class UniversalLeaderboardPlatform : ILeaderboardPlatform, IDisposable
{
	// Token: 0x06000DE7 RID: 3559 RVA: 0x0003E09C File Offset: 0x0003C29C
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000DE8 RID: 3560 RVA: 0x0003E0AC File Offset: 0x0003C2AC
	public void GetLeaderboardAroundPlayer(string eventId, LeaderboardType leaderboardType, Action<List<LeaderboardItem>> callback, Action<string> onError, int maxCount = 100)
	{
		Dictionary<string, string> parameters = new Dictionary<string, string>
		{
			{
				"eventName",
				eventId
			},
			{
				"leaderboardType",
				leaderboardType.ToString()
			}
		};
		Action<ExecuteCloudScriptResult> onSuccess = delegate(ExecuteCloudScriptResult result)
		{
			if (callback != null)
			{
				try
				{
					Leaderboard leaderboard = JsonUtility.FromJson<Leaderboard>(result.FunctionResult.ToString());
					if (leaderboard.Ranks != null && leaderboard.Ranks.Count > 0)
					{
						Leaderboard leaderboard2 = leaderboard;
						callback(leaderboard2.Ranks);
					}
					else
					{
						onError(string.Format("[ERR_{0}] Invalid Data!", this.GetErrorNumber(LeaderboardError.INVALID_DATA_RETURNED)));
						this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.INVALID_DATA_RETURNED), "Data: " + result.FunctionResult);
					}
				}
				catch (Exception)
				{
					onError(string.Format("[ERR_{0}] Failed to deserialize LB data!", this.GetErrorNumber(LeaderboardError.FAILED_TO_DESERIALIZE)));
					this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.FAILED_TO_DESERIALIZE), "Data: " + result.FunctionResult);
				}
			}
		};
		this.DoGetLeaderboard(parameters, onSuccess, onError, 0);
	}

	// Token: 0x06000DE9 RID: 3561 RVA: 0x0003E11C File Offset: 0x0003C31C
	private void DoGetLeaderboard(Dictionary<string, string> parameters, Action<ExecuteCloudScriptResult> onSuccess, Action<string> onError, int retrys)
	{
		Action<PlayFabError> error = delegate(PlayFabError result)
		{
			if (onError != null)
			{
				if (retrys <= 2)
				{
					this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.CLOUDSCRIPT_ERROR_RETRY), "Data: " + result.ErrorMessage);
					UniversalLeaderboardPlatform <>4__this = this;
					Dictionary<string, string> parameters2 = parameters;
					Action<ExecuteCloudScriptResult> onSuccess2 = onSuccess;
					Action<string> onError2 = onError;
					int retrys2 = retrys + 1;
					retrys = retrys2;
					<>4__this.DoGetLeaderboard(parameters2, onSuccess2, onError2, retrys2);
					return;
				}
				onError(string.Format("[ERR_{0}] Failed to get leaderboard", this.GetErrorNumber(LeaderboardError.CLOUDSCRIPT_ERROR_ABORT)));
				this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.CLOUDSCRIPT_ERROR_ABORT), "Error: " + result.ErrorMessage);
			}
		};
		Action<ExecuteCloudScriptResult> onResult = delegate(ExecuteCloudScriptResult result)
		{
			if (result.FunctionResult != null && !string.IsNullOrEmpty(result.FunctionResult.ToString()))
			{
				onSuccess(result);
				return;
			}
			if (retrys <= 2)
			{
				this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.CLOUDSCRIPT_TIMEOUT_RETRY), "Data: Timeout, retying " + retrys);
				UniversalLeaderboardPlatform <>4__this = this;
				Dictionary<string, string> parameters2 = parameters;
				Action<ExecuteCloudScriptResult> onSuccess2 = onSuccess;
				Action<string> onError2 = onError;
				int retrys2 = retrys + 1;
				retrys = retrys2;
				<>4__this.DoGetLeaderboard(parameters2, onSuccess2, onError2, retrys2);
				return;
			}
			onError(string.Format("[ERR_{0}] Script Timeout", this.GetErrorNumber(LeaderboardError.CLOUDSCRIPT_TIMEOUT_ABORT)));
			this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.CLOUDSCRIPT_TIMEOUT_ABORT), "Data: Timeout, aborting " + retrys);
		};
		Helper.GetPlatformAccount().PlayFab.ExecuteCloudScript("FetchUniversalLeaderboard", parameters, onResult, error);
	}

	// Token: 0x06000DEA RID: 3562 RVA: 0x0003E18C File Offset: 0x0003C38C
	public void GetLeaderboardToken(Action<int> onSuccess, Action<string> onError)
	{
		Helper.GetPlatformAccount().PlayFab.ExecuteCloudScript("GenerateLeaderboardToken", delegate(ExecuteCloudScriptResult obj)
		{
			int num = 0;
			int.TryParse(obj.FunctionResult.ToString(), out num);
			if (num <= 0)
			{
				onError("Error Generating Leaderboard Token");
				return;
			}
			onSuccess(num);
		}, delegate(PlayFabError error)
		{
			onError(error.ErrorMessage);
		});
	}

	// Token: 0x06000DEB RID: 3563 RVA: 0x0003E1DC File Offset: 0x0003C3DC
	public void GetLeaderboardTop100(string id, LeaderboardType leaderboardType, Action<List<LeaderboardItem>> callback, Action<string> onError)
	{
		Dictionary<string, string> parameters = new Dictionary<string, string>
		{
			{
				"eventName",
				id
			},
			{
				"requestType",
				"top100"
			},
			{
				"leaderboardType",
				leaderboardType.ToString()
			}
		};
		Action<ExecuteCloudScriptResult> onSuccess = delegate(ExecuteCloudScriptResult result)
		{
			if (callback != null)
			{
				try
				{
					Leaderboard leaderboard = JsonUtility.FromJson<Leaderboard>(result.FunctionResult.ToString());
					if (leaderboard.Ranks != null && leaderboard.Ranks.Count > 0)
					{
						Leaderboard leaderboard2 = leaderboard;
						callback(leaderboard2.Ranks);
					}
					else
					{
						onError(string.Format("[ERR_{0}] Invalid Data!", this.GetErrorNumber(LeaderboardError.INVALID_DATA_RETURNED)));
						this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.INVALID_DATA_RETURNED), "Data: " + result.FunctionResult);
					}
				}
				catch (Exception)
				{
					onError(string.Format("[ERR_{0}] Failed to deserialize LB data", this.GetErrorNumber(LeaderboardError.FAILED_TO_DESERIALIZE)));
					this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.FAILED_TO_DESERIALIZE), "Data: " + result.FunctionResult);
				}
			}
		};
		this.DoGetLeaderboard(parameters, onSuccess, onError, 0);
	}

	// Token: 0x06000DEC RID: 3564 RVA: 0x0003E25C File Offset: 0x0003C45C
	public void PostLeaderboardValue(string leaderboardId, LeaderboardType leaderboardType, int leaderboardSize, int value, Action onSuccess, Action<string> onError)
	{
		int leaderBoardToken = GameController.Instance.LeaderboardService.GetLeaderBoardToken();
		if (leaderBoardToken == 0)
		{
			onError("Invalid leaderboard token");
			return;
		}
		string hashSha = UniversalLeaderboardPlatform.GetHashSha256(leaderBoardToken.ToString() + "d41d8cd98f00b204e9800998ecf8427e" + value.ToString());
		string parameters = string.Concat(new object[]
		{
			"{\"angelScore\":",
			value,
			", \"clientValidationHash\":\"",
			hashSha,
			"\", \"leaderboardId\": \"",
			leaderboardId,
			"\", \"leaderboardType\": \"",
			leaderboardType.ToString(),
			"\", \"leaderboardSize\":",
			leaderboardSize,
			"}"
		});
		Action<ExecuteCloudScriptResult> onSuccess2 = delegate(ExecuteCloudScriptResult result)
		{
			if (onSuccess != null)
			{
				onSuccess();
			}
		};
		this.DoPostLeaderboardScore(parameters, onSuccess2, onError, 0);
	}

	// Token: 0x06000DED RID: 3565 RVA: 0x0003E33C File Offset: 0x0003C53C
	private void DoPostLeaderboardScore(string parameters, Action<ExecuteCloudScriptResult> onSuccess, Action<string> onError, int retrys)
	{
		Action<PlayFabError> error = delegate(PlayFabError result)
		{
			if (onError != null)
			{
				if (retrys <= 2)
				{
					this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.POST_SCORE_ERROR_RETRY), "Data: Post Score Error, retying " + retrys);
					UniversalLeaderboardPlatform <>4__this = this;
					string parameters2 = parameters;
					Action<ExecuteCloudScriptResult> onSuccess2 = onSuccess;
					Action<string> onError2 = onError;
					int retrys2 = retrys + 1;
					retrys = retrys2;
					<>4__this.DoPostLeaderboardScore(parameters2, onSuccess2, onError2, retrys2);
					return;
				}
				this.LogCustomLeaderboardError(this.GetErrorNumber(LeaderboardError.POST_SCORE_ERROR_ABORT), "Data: Post Score Error, aborting " + retrys);
				onError("Error: " + this.GetErrorNumber(LeaderboardError.POST_SCORE_ERROR_ABORT));
			}
		};
		Helper.GetPlatformAccount().PlayFab.ExecuteCloudScript("UpdateUniversalLeaderboard", parameters, onSuccess, error);
	}

	// Token: 0x06000DEE RID: 3566 RVA: 0x0003E3A4 File Offset: 0x0003C5A4
	public static string GetHashSha256(string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		byte[] array = new SHA256Managed().ComputeHash(bytes);
		string text2 = string.Empty;
		foreach (byte b in array)
		{
			text2 += string.Format("{0:x2}", b);
		}
		return text2;
	}

	// Token: 0x06000DEF RID: 3567 RVA: 0x0003E3FB File Offset: 0x0003C5FB
	private void LogCustomLeaderboardError(int errorNumber, string errorMessage)
	{
		GameController.Instance.AnalyticService.SendShortLeaderboardError(errorNumber, errorMessage);
	}

	// Token: 0x06000DF0 RID: 3568 RVA: 0x0003E410 File Offset: 0x0003C610
	private int GetErrorNumber(LeaderboardError error)
	{
		int result;
		switch (error)
		{
		case LeaderboardError.INVALID_DATA_RETURNED:
			result = 7702;
			break;
		case LeaderboardError.FAILED_TO_DESERIALIZE:
			result = 7701;
			break;
		case LeaderboardError.CLOUDSCRIPT_TIMEOUT_RETRY:
			result = 7703;
			break;
		case LeaderboardError.CLOUDSCRIPT_TIMEOUT_ABORT:
			result = 7704;
			break;
		case LeaderboardError.CLOUDSCRIPT_ERROR_RETRY:
			result = 7705;
			break;
		case LeaderboardError.CLOUDSCRIPT_ERROR_ABORT:
			result = 7706;
			break;
		case LeaderboardError.POST_SCORE_ERROR_RETRY:
			result = 7707;
			break;
		case LeaderboardError.POST_SCORE_ERROR_ABORT:
			result = 7708;
			break;
		case LeaderboardError.UNKOWN_ERROR:
			result = 7798;
			break;
		default:
			result = 7799;
			break;
		}
		return result;
	}

	// Token: 0x04000BE3 RID: 3043
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000BE4 RID: 3044
	private const int MAX_RETRY_COUNT = 2;

	// Token: 0x04000BE5 RID: 3045
	private const string LEADERBOARD_TOKEN_PFAB_KEY = "LeaderBoardToken";

	// Token: 0x04000BE6 RID: 3046
	private const string leaderboardKey = "d41d8cd98f00b204e9800998ecf8427e";
}
