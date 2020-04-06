using System;
using System.Collections.Generic;
using System.Text;
using LitJson;
using UnityEngine;

// Token: 0x0200015A RID: 346
public static class GameStateSaveLoad
{
	// Token: 0x06000AC7 RID: 2759 RVA: 0x000311AB File Offset: 0x0002F3AB
	public static void AddSaveHook(Action<string, string> hook)
	{
		GameStateSaveLoad.PlatformSpecificSave = (Action<string, string>)Delegate.Combine(GameStateSaveLoad.PlatformSpecificSave, hook);
	}

	// Token: 0x06000AC8 RID: 2760 RVA: 0x000311C2 File Offset: 0x0002F3C2
	public static void RemoveSaveHook(Action<string, string> hook)
	{
		GameStateSaveLoad.PlatformSpecificSave = (Action<string, string>)Delegate.Remove(GameStateSaveLoad.PlatformSpecificSave, hook);
	}

	// Token: 0x06000AC9 RID: 2761 RVA: 0x000311DC File Offset: 0x0002F3DC
	public static void Load(string planetName, Action<GameStateSaveData> onComplete)
	{
		string text = "GameStateData_" + planetName;
		if (PlayerPrefs.HasKey(text))
		{
			string @string = PlayerPrefs.GetString(text);
			onComplete(Serializer.Deserialize<GameStateSaveData>(@string));
			return;
		}
		if (PlayerPrefs.HasKey("GameState_" + planetName))
		{
			GameStateSaveLoad.LoadLegacyGameState(PlayerPrefs.GetString("GameState_" + planetName), onComplete);
			return;
		}
		Debug.LogWarningFormat("No save for [{0}] so returning null", new object[]
		{
			text
		});
		onComplete(null);
	}

	// Token: 0x06000ACA RID: 2762 RVA: 0x00031258 File Offset: 0x0002F458
	private static void LoadLegacyGameState(string loadData, Action<GameStateSaveData> onComplete)
	{
		if (string.IsNullOrEmpty(loadData))
		{
			onComplete(null);
		}
		GameState gameState = Serializer.Deserialize<GameState>(loadData);
		if (gameState == null)
		{
			onComplete(null);
		}
		gameState.CashOnHand.Value = gameState.cash;
		gameState.SessionCash.Value = gameState.totalCash;
		gameState.TotalPreviousCash.Value = gameState.totalPreviousCash;
		gameState.HighestMultiplierPurchased.Value = Mathf.Max(gameState.highestMultiplierPurchase, 4);
		GameStateSaveData gameStateSaveData = gameState.BuildGameStateSaveData();
		gameStateSaveData.angelInvestors = gameState.angelInvestors;
		gameStateSaveData.angelInvestorsSpent = gameState.angelInvestorsSpent;
		using (List<Venture>.Enumerator enumerator = gameState.ventures.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Venture venture = enumerator.Current;
				VentureSaveData ventureSaveData = gameStateSaveData.ventures.Find(v => v.id == venture.id);
				ventureSaveData.numOwned = (double)venture.baseAmount;
				ventureSaveData.timeRun = (float)venture.currentTime;
			}
		}
		gameState.Dispose();
		onComplete(gameStateSaveData);
	}

	// Token: 0x06000ACB RID: 2763 RVA: 0x00031380 File Offset: 0x0002F580
	public static void Save(GameState state, double currentTime)
	{
		if (currentTime > 0.0)
		{
			state.timestamp = currentTime;
		}
		try
		{
			string text = Serializer.Serialize<GameStateSaveData>(state.BuildGameStateSaveData());
			GameStateSaveLoad.PlatformSpecificSave(text, state.planetName.ToLower());
			PlayerPrefs.SetString("GameStateData_" + state.planetName, text);
			PlayerPrefs.Save();
		}
		catch (Exception ex)
		{
			Debug.LogError("[GameStateSaveLoad] Failed to save:" + ex.Message + ", " + ex.StackTrace);
		}
	}

	// Token: 0x06000ACC RID: 2764 RVA: 0x00031414 File Offset: 0x0002F614
	public static string SerializeGameState(object state)
	{
		try
		{
			JsonWriter jsonWriter = new JsonWriter();
			jsonWriter.TypeHinting = true;
			jsonWriter.UseAssemblyQualifiedName = false;
			JsonMapper.ToJson(state, jsonWriter);
			string text = jsonWriter.ToString();
			return "." + Convert.ToBase64String(CLZF2.Compress(Encoding.UTF8.GetBytes(text))) + "|" + ExportImport.Md5Sum(text);
		}
		catch (Exception ex)
		{
			Debug.LogError("[GameStateSaveLoad] Error occured Serializing:" + ex.Message + ", " + ex.StackTrace);
		}
		return null;
	}

	// Token: 0x04000941 RID: 2369
    private static Action<string, string> PlatformSpecificSave = delegate(string a, string b) { };
}
