using System;
using System.Collections.Generic;
using Platforms.Logger;
using PlayFab;
using PlayFab.ClientModels;
using Steamworks;
using UnityEngine;

namespace Platforms
{
	// Token: 0x020006D2 RID: 1746
	public class SteamPlatformAccount : IPlatformAccountExtension, IPlatform
	{
		// Token: 0x0600240C RID: 9228 RVA: 0x0009C04C File Offset: 0x0009A24C
		public int EnabledForPlatform(PlatformType platformType)
		{
			if (platformType != PlatformType.Steam || Application.isEditor)
			{
				return -1;
			}
			return 10;
		}

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x0600240D RID: 9229 RVA: 0x0009C05D File Offset: 0x0009A25D
		public string DisplayName
		{
			get
			{
				return SteamFriends.GetPersonaName();
			}
		}

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x0600240E RID: 9230 RVA: 0x0009BEB8 File Offset: 0x0009A0B8
		public PlatformType PlatformType
		{
			get
			{
				return PlatformType.Steam;
			}
		}

		// Token: 0x0600240F RID: 9231 RVA: 0x0009C064 File Offset: 0x0009A264
		public IPlatformAccountExtension InitPlatform(PlatformAccount account, Platforms.Logger.Logger logger, PlayFabWrapper playFab)
		{
			this.logger = logger;
			this.playFab = playFab;
			this.Steam.Init();
			return this;
		}

		// Token: 0x06002410 RID: 9232 RVA: 0x0009C080 File Offset: 0x0009A280
		public void Login(Action<LoginResult> onLogin, Action<PlayFabError> onError)
		{
			this.logger.Debug("Attempting to get ticket.");
			Action<LoginResult> <>9__2;
			Action<PlayFabError> <>9__3;
			this.Steam.GetAuthSessionTicket(delegate(string ticket)
			{
				this.logger.Debug("Retrieved Steam ticket, authenticating with PlayFab.");
				PlayFabWrapper playFabWrapper = this.playFab;
				Action<LoginResult> onResult;
				if ((onResult = <>9__2) == null)
				{
					onResult = (<>9__2 = delegate(LoginResult res)
					{
						this.logger.Info("Successfully authenticated with PlayFab.");
						this.ImportSaves();
						if (onLogin != null)
						{
							onLogin(res);
						}
					});
				}
				Action<PlayFabError> onError2;
				if ((onError2 = <>9__3) == null)
				{
					onError2 = (<>9__3 = delegate(PlayFabError err)
					{
						this.logger.Error("Unable to authenticate with PlayFab: [{0}]\n  [{1]}", new object[]
						{
							err.Error,
							err.ToString()
						});
						if (onError != null)
						{
							onError(err);
						}
					});
				}
				playFabWrapper.LoginWithSteam(ticket, onResult, onError2);
			}, delegate(EResult errorResult)
			{
				this.logger.Error("Error retrieving Steam ticket: [{0}]", new object[]
				{
					errorResult
				});
				onError(new PlayFabError
				{
					Error = PlayFabErrorCode.UnknownError,
					ErrorMessage = errorResult.ToString()
				});
			});
		}

		// Token: 0x06002411 RID: 9233 RVA: 0x0009C0DC File Offset: 0x0009A2DC
		private void ImportSaves()
		{
			Debug.Log("BEGIN Steam save import");
			foreach (KeyValuePair<string, string> keyValuePair in this.importList)
			{
				SteamRemoteStorage.FileDelete(keyValuePair.Key + ".bak");
				if (SteamRemoteStorage.GetFileSize(keyValuePair.Key) == 0)
				{
					if (!Application.isEditor)
					{
						Debug.LogFormat("  Skipping [{0}], appears to be missing or empty", new object[]
						{
							keyValuePair.Key
						});
					}
					SteamRemoteStorage.FileDelete(keyValuePair.Key);
				}
				else
				{
					byte[] array = new byte[2000000];
					int num = SteamRemoteStorage.FileRead(keyValuePair.Key, array, array.Length);
					char[] array2 = new char[num / 2];
					Buffer.BlockCopy(array, 0, array2, 0, num);
					string text = new string(array2);
					if (text.Length <= 0)
					{
						if (!Application.isEditor)
						{
							Debug.LogFormat("  Skipping [{0}], appears to be missing, empty or corrupt", new object[]
							{
								keyValuePair.Key
							});
						}
						SteamRemoteStorage.FileDelete(keyValuePair.Key);
					}
					else
					{
						Debug.LogFormat("  Importing [{0}]", new object[]
						{
							keyValuePair.Key
						});
						PlayerPrefs.SetString(keyValuePair.Value, text);
					}
				}
			}
			Debug.Log("END Steam save import");
		}

		// Token: 0x06002412 RID: 9234 RVA: 0x0009C244 File Offset: 0x0009A444
		public void DisposePlatform()
		{
			this.Steam.Dispose();
		}

		// Token: 0x06002413 RID: 9235 RVA: 0x0003AA8D File Offset: 0x00038C8D
		public void LinkAccount(bool force, Action onSuccess, Action<PlayFabError> onError)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002414 RID: 9236 RVA: 0x0003AA8D File Offset: 0x00038C8D
		public void UnlinkAccount(Action onSuccess, Action<PlayFabError> onError)
		{
			throw new NotImplementedException();
		}

		// Token: 0x040024DB RID: 9435
		public SteamworksWrapper Steam = SteamworksWrapper.Instance;

		// Token: 0x040024DC RID: 9436
		private Platforms.Logger.Logger logger;

		// Token: 0x040024DD RID: 9437
		private PlayFabWrapper playFab;

		// Token: 0x040024DE RID: 9438
		private const int maximumDataLength = 1000000;

		// Token: 0x040024DF RID: 9439
		private readonly Dictionary<string, string> importList = new Dictionary<string, string>
		{
			{
				"gamestate.sav",
				"GameState"
			},
			{
				"earth-gamestate.sav",
				"GameState_Earth"
			},
			{
				"moon-gamestate.sav",
				"GameState_Moon"
			},
			{
				"mars-gamestate.sav",
				"GameState_Mars"
			},
			{
				"global-playerdata.sav",
				"PlayerData_Global"
			},
			{
				"earth-playerdata.sav",
				"PlayerData_Earth"
			},
			{
				"moon-playerdata.sav",
				"PlayerData_Moon"
			},
			{
				"mars-playerdata.sav",
				"PlayerData_Mars"
			},
			{
				"planetpanel.sav",
				"PlanetPanel_???"
			},
			{
				"earth-planetpanel.sav",
				"PlanetPanel_Earth"
			},
			{
				"moon-planetpanel.sav",
				"PlanetPanel_Moon"
			},
			{
				"mars-planetpanel.sav",
				"PlanetPanel_Mars"
			},
			{
				"earth-gamestate-3.sav",
				"GameStateData_Earth"
			},
			{
				"moon-gamestate-3.sav",
				"GameStateData_Moon"
			},
			{
				"mars-gamestate-3.sav",
				"GameStateData_Mars"
			},
			{
				"1512_holiday_728037-gamestate-3.sav",
				"GameStateData_1512_holiday_728037"
			},
			{
				"1512_holiday_728037-playerdata.sav",
				"PlayerData_1512_holiday_728037"
			},
			{
				"1512_holiday_728037-planetpanel.sav",
				"PlanetPanel_1512_holiday_728037"
			},
			{
				"1601_newyears_456263-gamestate-3.sav",
				"GameStateData_1601_newyears_456263"
			},
			{
				"1601_newyears_456263-playerdata.sav",
				"PlayerData_1601_newyears_456263"
			},
			{
				"1601_newyears_456263-planetpanel.sav",
				"PlanetPanel_1601_newyears_456263"
			},
			{
				"1602_valentines_434575-gamestate-3.sav",
				"GameStateData_1602_valentines_434575"
			},
			{
				"1602_valentines_434575-playerdata-3.sav",
				"PlayerData_1602_valentines_434575"
			},
			{
				"1602_valentines_434575-planetpanel-3.sav",
				"PlanetPanel_1602_valentines_434575"
			},
			{
				"1603_easter_290507-gamestate-3.sav",
				"GameStateData_1603_easter_290507"
			},
			{
				"1603_easter_290507-playerdata-3.sav",
				"PlayerData_1603_easter_290507"
			},
			{
				"1603_easter_290507-planetpanel-3.sav",
				"PlanetPanel_1603_easter_290507"
			},
			{
				"1604_hero_124816-gamestate-3.sav",
				"GameStateData_1604_hero_124816"
			},
			{
				"1604_hero_124816-playerdata-3.sav",
				"PlayerData_1604_hero_124816"
			},
			{
				"1604_hero_124816-planetpanel-3.sav",
				"PlanetPanel_1604_hero_124816"
			},
			{
				"1605_space_789456-gamestate-3.sav",
				"GameStateData_1605_space_789456"
			},
			{
				"1605_space_789456-playerdata-3.sav",
				"PlayerData_1605_space_789456"
			},
			{
				"1605_space_789456-planetpanel-3.sav",
				"PlanetPanel_1605_space_789456"
			},
			{
				"1606_arcade_451278-gamestate-3.sav",
				"GameStateData_1606_arcade_451278"
			},
			{
				"1606_arcade_451278-playerdata-3.sav",
				"PlayerData_1606_arcade_451278"
			},
			{
				"1606_arcade_451278-planetpanel-3.sav",
				"PlanetPanel_1606_arcade_451278"
			},
			{
				"1607_music_182937-gamestate-3.sav",
				"GameStateData_1607_music_182937"
			},
			{
				"1607_music_182937-playerdata.sav",
				"PlayerData_1607_music_182937"
			},
			{
				"1607_music_182937-planetpanel.sav",
				"PlanetPanel_1607_music_182937"
			},
			{
				"1608_sports_867530-gamestate-3.sav",
				"GameStateData_1608_sports_867530"
			},
			{
				"1608_sports_867530-playerdata.sav",
				"PlayerData_1608_sports_867530"
			},
			{
				"1608_sports_867530-planetpanel.sav",
				"PlanetPanel_1608_sports_867530"
			},
			{
				"1609_time_164973-gamestate-3.sav",
				"GameStateData_1609_time_164973"
			},
			{
				"1609_time_164973-playerdata.sav",
				"PlayerData_1609_time_164973"
			},
			{
				"1609_time_164973-planetpanel.sav",
				"PlanetPanel_1609_time_164973"
			},
			{
				"1609_valentines_replay-gamestate-3.sav",
				"GameStateData_1609_valentines_replay"
			},
			{
				"1609_valentines_replay-playerdata.sav",
				"PlayerData_1609_valentines_replay"
			},
			{
				"1609_valentines_replay-planetpanel.sav",
				"PlanetPanel_1609_valentines_replay"
			},
			{
				"1610_halloween_yeartwo-gamestate-3.sav",
				"GameStateData_1610_halloween_yeartwo"
			},
			{
				"1610_halloween_yeartwo-playerdata.sav",
				"PlayerData_1610_halloween_yeartwo"
			},
			{
				"1610_halloween_yeartwo-planetpanel.sav",
				"PlanetPanel_1610_halloween_yeartwo"
			},
			{
				"1610_hero_replaysurprise-gamestate-3.sav",
				"GameStateData_1610_hero_replaysurprise"
			},
			{
				"1610_hero_replaysurprise-playerdata.sav",
				"PlayerData_1610_hero_replaysurprise"
			},
			{
				"1610_hero_replaysurprise-planetpanel.sav",
				"PlanetPanel_1610_hero_replaysurprise"
			},
			{
				"1611_easter_replay-gamestate-3.sav",
				"GameStateData_1611_easter_replay"
			},
			{
				"1611_easter_replay-playerdata.sav",
				"PlayerData_1611_easter_replay"
			},
			{
				"1611_easter_replay-planetpanel.sav",
				"PlanetPanel_1611_easter_replay"
			},
			{
				"1611_blackfriday_yeartwo-gamestate-3.sav",
				"GameStateData_1611_blackfriday_yeartwo"
			},
			{
				"1611_blackfriday_yeartwo-playerdata.sav",
				"PlayerData_1611_blackfriday_yeartwo"
			},
			{
				"1611_blackfriday_yeartwo-planetpanel.sav",
				"PlanetPanel_1611_blackfriday_yeartwo"
			},
			{
				"1612_arcade_replay-gamestate-3.sav",
				"GameStateData_1612_arcade_replay"
			},
			{
				"1612_arcade_replay-playerdata.sav",
				"PlayerData_1612_arcade_replay"
			},
			{
				"1612_arcade_replay-planetpanel.sav",
				"PlanetPanel_1612_arcade_replay"
			},
			{
				"1612_holiday_yeartwo-gamestate-3.sav",
				"GameStateData_1612_holiday_yeartwo"
			},
			{
				"1612_holiday_yeartwo-playerdata.sav",
				"PlayerData_1612_holiday_yeartwo"
			},
			{
				"1612_holiday_yeartwo-planetpanel.sav",
				"PlanetPanel_1612_holiday_yeartwo"
			},
			{
				"1701_newyears_yeartwo-gamestate-3.sav",
				"GameStateData_1701_newyears_yeartwo"
			},
			{
				"1701_newyears_yeartwo-playerdata.sav",
				"PlayerData_1701_newyears_yeartwo"
			},
			{
				"1701_newyears_yeartwo-planetpanel.sav",
				"PlanetPanel_1701_newyears_yeartwo"
			},
			{
				"1702_valentines_yeartwo-gamestate-3.sav",
				"GameStateData_1702_valentines_yeartwo"
			},
			{
				"1702_valentines_yeartwo-playerdata.sav",
				"PlayerData_1702_valentines_yeartwo"
			},
			{
				"1702_valentines_yeartwo-planetpanel.sav",
				"PlanetPanel_1702_valentines_yeartwo"
			},
			{
				"1703_easter_yeartwo-gamestate-3.sav",
				"GameStateData_1703_easter_yeartwo"
			},
			{
				"1703_easter_yeartwo-playerdata.sav",
				"PlayerData_1703_easter_yeartwo"
			},
			{
				"1703_easter_yeartwo-planetpanel.sav",
				"PlanetPanel_1703_easter_yeartwo"
			},
			{
				"1704_hero_yeartwo-gamestate-3.sav",
				"GameStateData_1704_hero_yeartwo"
			},
			{
				"1704_hero_yeartwo-playerdata.sav",
				"PlayerData_1704_hero_yeartwo"
			},
			{
				"1704_hero_yeartwo-planetpanel.sav",
				"PlanetPanel_1704_hero_yeartwo"
			},
			{
				"1705_space_yeartwo-gamestate-3.sav",
				"GameStateData_1705_space_yeartwo"
			},
			{
				"1705_space_yeartwo-playerdata.sav",
				"PlayerData_1705_space_yeartwo"
			},
			{
				"1705_space_yeartwo-planetpanel.sav",
				"PlanetPanel_1705_space_yeartwo"
			},
			{
				"1706_arcade_yeartwo-gamestate-3.sav",
				"GameStateData_1706_arcade_yeartwo"
			},
			{
				"1706_arcade_yeartwo-playerdata.sav",
				"PlayerData_1706_arcade_yeartwo"
			},
			{
				"1706_arcade_yeartwo-planetpanel.sav",
				"PlanetPanel_1706_arcade_yeartwo"
			},
			{
				"1707_music_yeartwo-gamestate-3.sav",
				"GameStateData_1707_music_yeartwo"
			},
			{
				"1707_music_yeartwo-playerdata.sav",
				"PlayerData_1707_music_yeartwo"
			},
			{
				"1707_music_yeartwo-planetpanel.sav",
				"PlanetPanel_1707_music_yeartwo"
			},
			{
				"1708_sports_yeartwo-gamestate-3.sav",
				"GameStateData_1708_sports_yeartwo"
			},
			{
				"1708_sports_yeartwo-playerdata.sav",
				"PlayerData_1708_sports_yeartwo"
			},
			{
				"1708_sports_yeartwo-planetpanel.sav",
				"PlanetPanel_1708_sports_yeartwo"
			},
			{
				"1708_cartoon-gamestate-3.sav",
				"GameStateData_1708_cartoon"
			},
			{
				"1708_cartoon-playerdata.sav",
				"PlayerData_1708_cartoon"
			},
			{
				"1708_cartoon-planetpanel.sav",
				"PlanetPanel_1708_cartoon"
			},
			{
				"1710_themepark-gamestate-3.sav",
				"GameStateData_1710_themepark"
			},
			{
				"1710_themepark-playerdata.sav",
				"PlayerData_1710_themepark"
			},
			{
				"1710_themepark-planetpanel.sav",
				"PlanetPanel_1710_themepark"
			},
			{
				"1710_halloweenblockparty-gamestate-3.sav",
				"GameStateData_1710_halloweenblockparty"
			},
			{
				"1710_halloweenblockparty-playerdata.sav",
				"PlayerData_1710_halloweenblockparty"
			},
			{
				"1710_halloweenblockparty-planetpanel.sav",
				"PlanetPanel_1710_halloweenblockparty"
			},
			{
				"1711_thanksgiving-gamestate-3.sav",
				"GameStateData_1711_thanksgiving"
			},
			{
				"1711_thanksgiving-playerdata.sav",
				"PlayerData_1711_thanksgiving"
			},
			{
				"1711_thanksgiving-planetpanel.sav",
				"PlanetPanel_1711_thanksgiving"
			},
			{
				"1709_time_yeartwo-gamestate-3.sav",
				"GameStateData_1709_time_yeartwo"
			},
			{
				"1709_time_yeartwo-playerdata.sav",
				"PlayerData_1709_time_yeartwo"
			},
			{
				"1709_time_yeartwo-planetpanel.sav",
				"PlanetPanel_1709_time_yeartwo"
			},
			{
				"1710_halloween_yeartwo-gamestate-3.sav",
				"GameStateData_1710_halloween_yeartwo"
			},
			{
				"1710_halloween_yeartwo-playerdata.sav",
				"PlayerData_1710_halloween_yeartwo"
			},
			{
				"1710_halloween_yeartwo-planetpanel.sav",
				"PlanetPanel_1710_halloween_yeartwo"
			},
			{
				"1701_sports_replay-gamestate-3.sav",
				"GameStateData_1701_sports_replay"
			},
			{
				"1701_sports_replay-playerdata.sav",
				"PlayerData_1701_sports_replay"
			},
			{
				"1701_sports_replay-planetpanel.sav",
				"PlanetPanel_1701_sports_replay"
			},
			{
				"1703_space_replay-gamestate-3.sav",
				"GameStateData_1703_space_replay"
			},
			{
				"1703_space_replay-playerdata.sav",
				"PlayerData_1703_space_replay"
			},
			{
				"1703_space_replay-planetpanel.sav",
				"PlanetPanel_1703_space_replay"
			},
			{
				"1703_music_replay-gamestate-3.sav",
				"GameStateData_1703_music_replay"
			},
			{
				"1703_music_replay-playerdata.sav",
				"PlayerData_1703_music_replay"
			},
			{
				"1703_music_replay-planetpanel.sav",
				"PlanetPanel_1703_music_replay"
			},
			{
				"1704_time_replay-gamestate-3.sav",
				"GameStateData_1704_time_replay"
			},
			{
				"1704_time_replay-playerdata.sav",
				"PlayerData_1704_time_replay"
			},
			{
				"1704_time_replay-planetpanel.sav",
				"PlanetPanel_1704_time_replay"
			},
			{
				"1706_halloween_replay-gamestate-3.sav",
				"GameStateData_1706_halloween_replay"
			},
			{
				"1706_halloween_replay-playerdata.sav",
				"PlayerData_1706_halloween_replay"
			},
			{
				"1706_halloween_replay-planetpanel.sav",
				"PlanetPanel_1706_halloween_replay"
			},
			{
				"1707_blackfriday_replay-gamestate-3.sav",
				"GameStateData_1707_blackfriday_replay"
			},
			{
				"1707_blackfriday_replay-playerdata.sav",
				"PlayerData_1707_blackfriday_replay"
			},
			{
				"1707_blackfriday_replay-planetpanel.sav",
				"PlanetPanel_1707_blackfriday_replay"
			},
			{
				"1708_hero_replay-gamestate-3.sav",
				"GameStateData_1708_hero_replay"
			},
			{
				"1708_hero_replay-playerdata.sav",
				"PlayerData_1708_hero_replay"
			},
			{
				"1708_hero_replay-planetpanel.sav",
				"PlanetPanel_1708_hero_replay"
			},
			{
				"1709_valentines_replay-gamestate-3.sav",
				"GameStateData_1709_valentines_replay"
			},
			{
				"1709_valentines_replay-playerdata.sav",
				"PlayerData_1709_valentines_replay"
			},
			{
				"1709_valentines_replay-planetpanel.sav",
				"PlanetPanel_1709_valentines_replay"
			},
			{
				"1711_thanksgiving_replay-gamestate-3.sav",
				"GameStateData_1711_thanksgiving_replay"
			},
			{
				"1711_thanksgiving_replay-playerdata.sav",
				"PlayerData_1711_thanksgiving_replay"
			},
			{
				"1711_thanksgiving_replay-planetpanel.sav",
				"PlanetPanel_1711_thanksgiving_replay"
			},
			{
				"1712_xmas-gamestate-3.sav",
				"GameStateData_1712_xmas"
			},
			{
				"1712_xmas-playerdata.sav",
				"PlayerData_1712_xmas"
			},
			{
				"1712_xmas-planetpanel.sav",
				"PlanetPanel_1712_xmas"
			},
			{
				"1801_sports-gamestate-3.sav",
				"GameStateData_1801_sports"
			},
			{
				"1801_sports-playerdata.sav",
				"PlayerData_1801_sports"
			},
			{
				"1801_sports-planetpanel.sav",
				"PlanetPanel_1801_sports"
			},
			{
				"manager_mania_i-gamestate-3.sav",
				"GameStateData_manager_mania_i"
			},
			{
				"manager_mania_i-playerdata.sav",
				"PlayerData_manager_mania_i"
			},
			{
				"manager_mania_i-planetpanel.sav",
				"PlanetPanel_manager_mania_i"
			}
		};
	}
}
