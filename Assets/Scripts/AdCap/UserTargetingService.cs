using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using PlayFab.ClientModels;
using UnityEngine;
using UserTargeting;
using UserTargeting.NCalcExtensions;
using UserTargeting.Parameters;
using UserTargeting.Segments;
using UserTargeting.TestGroups;

namespace AdCap
{
	// Token: 0x020006EA RID: 1770
	public class UserTargetingService
	{
		// Token: 0x0600249C RID: 9372 RVA: 0x0009E428 File Offset: 0x0009C628
		public void Initialize(IDateTimeService dateTimeService, SegmentData[] segmentData, TestGroupData[] testGroupData)
		{
			NCalcFactory.AddParameterHandler(new EvaluateParameterHandler(this.HandleParameters));
			UserTargetingService.InitializeGeneralParameters();
			this.dateTimeService = dateTimeService;
			this.segmentService.Initialize(segmentData, UserTargetingService.parameterStore);
			this.testGroupService.Initialize(testGroupData, this.segmentService);
			this.LoadSaveData();
			this.Evaluate();
		}

		// Token: 0x0600249D RID: 9373 RVA: 0x0009E481 File Offset: 0x0009C681
		public bool IsInTestGroup(string testGroup)
		{
			return this.testGroupService.IsGroupActive(testGroup);
		}

		// Token: 0x0600249E RID: 9374 RVA: 0x0009E48F File Offset: 0x0009C68F
		public bool IsInTestGroup(string testGroup, string subGroup)
		{
			return this.testGroupService.IsGroupActive(testGroup, subGroup);
		}

		// Token: 0x0600249F RID: 9375 RVA: 0x0009E49E File Offset: 0x0009C69E
		public bool IsAnyTestInGroupActive(string testGroup)
		{
			return this.testGroupService.IsAnyTestInGroupActive(testGroup);
		}

		// Token: 0x060024A0 RID: 9376 RVA: 0x0009E4AC File Offset: 0x0009C6AC
		public string GetActiveTestGroupCsv()
		{
			return this.testGroupService.GetActiveGroupsCsv();
		}

		// Token: 0x060024A1 RID: 9377 RVA: 0x0009E4B9 File Offset: 0x0009C6B9
		public UserTargetingSaveData ToSaveData()
		{
			return new UserTargetingSaveData
			{
				TestGroupsEntered = this.testGroupService.ToSaveData()
			};
		}

		// Token: 0x060024A2 RID: 9378 RVA: 0x0009E4D4 File Offset: 0x0009C6D4
		public void Evaluate()
		{
			this.segmentService.Evaluate();
			this.testGroupService.Evaluate();
			foreach (string text in this.testGroupService.ActiveGroupTags())
			{
				if (text.StartsWith("!"))
				{
					FeatureConfig.SetFlag(text.Substring(1), false);
				}
				else
				{
					FeatureConfig.SetFlag(text, true);
				}
			}
			this.PersistSaveData();
		}

		// Token: 0x060024A3 RID: 9379 RVA: 0x0009E564 File Offset: 0x0009C764
		public void SetExternalSegments(IEnumerable<string> segments)
		{
			foreach (string id in segments)
			{
				SegmentModel segmentModel;
				if (this.segmentService.TryGetSegment(id, out segmentModel))
				{
					segmentModel.ForceActive(true);
				}
			}
		}

		// Token: 0x060024A4 RID: 9380 RVA: 0x0009E5BC File Offset: 0x0009C7BC
		private void HandleParameters(string param, ParameterArgs args)
		{
			if (param == "UtcNow")
			{
				args.Result = this.dateTimeService.UtcNow;
				return;
			}
			if (GameController.Instance.GlobalPlayerData.Has(param))
			{
				args.Result = GameController.Instance.GlobalPlayerData.GetDouble(param, 0.0);
				return;
			}
			if (UserTargetingService.playerStatistics != null)
			{
				foreach (StatisticValue statisticValue in UserTargetingService.playerStatistics)
				{
					if (statisticValue.StatisticName == param)
					{
						args.Result = statisticValue.Value;
						break;
					}
				}
			}
		}

		// Token: 0x060024A5 RID: 9381 RVA: 0x0009E68C File Offset: 0x0009C88C
		private void LoadSaveData()
		{
			if (UserTargetingService.testGroupOverrides != null)
			{
				this.testGroupService.InitializeData(UserTargetingService.testGroupOverrides);
				return;
			}
			if (PlayerPrefs.HasKey("usertargeting.savedata"))
			{
				UserTargetingSaveData userTargetingSaveData = Serializer.Deserialize<UserTargetingSaveData>(PlayerPrefs.GetString("usertargeting.savedata"));
				if (userTargetingSaveData != null && userTargetingSaveData.TestGroupsEntered != null)
				{
					this.testGroupService.InitializeData(userTargetingSaveData.TestGroupsEntered);
					return;
				}
			}
			else if (UserTargetingService.legacyTestGroups != null)
			{
				this.testGroupService.InitializeData(UserTargetingService.legacyTestGroups);
			}
		}

		// Token: 0x060024A6 RID: 9382 RVA: 0x0009E704 File Offset: 0x0009C904
		private void PersistSaveData()
		{
			string value = Serializer.Serialize<UserTargetingSaveData>(this.ToSaveData());
			PlayerPrefs.SetString("usertargeting.savedata", value);
		}

		// Token: 0x060024A7 RID: 9383 RVA: 0x0009E728 File Offset: 0x0009C928
		public static void SetLegacyTestGroups(Dictionary<string, string> testGroups)
		{
			UserTargetingService.legacyTestGroups = (from kvp in testGroups
			select new TestGroupSaveData
			{
				TestGroup = kvp.Key,
				TestSubGroup = kvp.Value
			}).ToArray<TestGroupSaveData>();
		}

		// Token: 0x060024A8 RID: 9384 RVA: 0x0009E75C File Offset: 0x0009C95C
		public static void InitializePlayFabParameters(LoginResult loginResult)
		{
			GetPlayerCombinedInfoResultPayload infoResultPayload = loginResult.InfoResultPayload;
			ParameterStore parameterStore = UserTargetingService.parameterStore;
			string param = "TotalValueToDateInUSD";
			PlayerProfileModel playerProfile = infoResultPayload.PlayerProfile;
			parameterStore.SetParameter(param, ((playerProfile != null) ? playerProfile.TotalValueToDateInUSD : null) ?? 0U);
			TimeSpan? timeSpan = DateTime.Now - loginResult.InfoResultPayload.AccountInfo.TitleInfo.FirstLogin;
			UserTargetingService.parameterStore.SetParameter("FirstLogin", (timeSpan != null) ? timeSpan.GetValueOrDefault().TotalMinutes : 2147483647.0);
			UserTargetingService.parameterStore.SetParameter("DaysRetained", (timeSpan != null) ? timeSpan.GetValueOrDefault().TotalDays : 0.0);
			ParameterStore parameterStore2 = UserTargetingService.parameterStore;
			string param2 = "FirstLoginDate";
			UserAccountInfo accountInfo = infoResultPayload.AccountInfo;
			DateTime? dateTime;
			if (accountInfo == null)
			{
				dateTime = null;
			}
			else
			{
				UserTitleInfo titleInfo = accountInfo.TitleInfo;
				dateTime = ((titleInfo != null) ? titleInfo.FirstLogin : null);
			}
			parameterStore2.SetParameter(param2, dateTime ?? DateTime.Now);
			UserTargetingService.playerStatistics = infoResultPayload.PlayerStatistics;
			UserDataRecord userDataRecord;
			if (loginResult.InfoResultPayload.UserReadOnlyData.TryGetValue("test_groups", out userDataRecord))
			{
				try
				{
					UserTargetingService.testGroupOverrides = Serializer.Deserialize<string[]>(userDataRecord.Value).Select(delegate(string s)
					{
						string[] array = s.Split(new char[]
						{
							'|'
						});
						return new TestGroupSaveData
						{
							TestGroup = array[0],
							TestSubGroup = array[1]
						};
					}).ToArray<TestGroupSaveData>();
				}
				catch
				{
					Debug.LogError("Failed to parse test_groups\n" + userDataRecord.Value);
				}
			}
		}

		// Token: 0x060024A9 RID: 9385 RVA: 0x0009E950 File Offset: 0x0009CB50
		private static void InitializeGeneralParameters()
		{
			UserTargetingService.parameterStore.SetParameter("DeviceType", UserTargetingService.GetPlatform());
		}

		// Token: 0x060024AA RID: 9386 RVA: 0x0002D064 File Offset: 0x0002B264
		private static string GetPlatform()
		{
			return "Steam";
		}

		// Token: 0x0400256A RID: 9578
		private const string UserTargetingSaveDataKey = "usertargeting.savedata";

		// Token: 0x0400256B RID: 9579
		private SegmentService segmentService = new SegmentService();

		// Token: 0x0400256C RID: 9580
		private TestGroupService testGroupService = new TestGroupService();

		// Token: 0x0400256D RID: 9581
		private IDateTimeService dateTimeService;

		// Token: 0x0400256E RID: 9582
		private static readonly ParameterStore parameterStore = new ParameterStore();

		// Token: 0x0400256F RID: 9583
		private static List<StatisticValue> playerStatistics;

		// Token: 0x04002570 RID: 9584
		private static TestGroupSaveData[] testGroupOverrides;

		// Token: 0x04002571 RID: 9585
		private static TestGroupSaveData[] legacyTestGroups;
	}
}
