using System;
using System.Collections.Generic;
using AdCap.Store;
using LitJson;
using UniRx;
using UnityEngine;

// Token: 0x020001F1 RID: 497
public class PlayerData
{
	public string Name { get; internal set; }

	public PlayerData()
	{
	}

	// Token: 0x06000E72 RID: 3698 RVA: 0x00040BD8 File Offset: 0x0003EDD8
	public PlayerData(string name)
	{
		this.Name = name;
		PlayerData.playerDatas.Add(name, this);
		this.Load();
	}

	// Token: 0x06000E73 RID: 3699 RVA: 0x00040C25 File Offset: 0x0003EE25
	public static void AddSaveHook(Action<string, string> hook)
	{
		PlayerData.PlatformSpecificSave = (Action<string, string>)Delegate.Combine(PlayerData.PlatformSpecificSave, hook);
	}

	// Token: 0x06000E74 RID: 3700 RVA: 0x00040C3C File Offset: 0x0003EE3C
	public static void RemoveSaveHook(Action<string, string> hook)
	{
		PlayerData.PlatformSpecificSave = (Action<string, string>)Delegate.Remove(PlayerData.PlatformSpecificSave, hook);
	}

    public static Action<string, string> PlatformSpecificSave { get; set; }

    // Token: 0x06000E75 RID: 3701 RVA: 0x00040C53 File Offset: 0x0003EE53
	public IObservable<double> GetObservable(string key, double defaultValue = 0.0)
	{
		if (!this._Subscriptions.ContainsKey(key))
		{
			this._Subscriptions.Add(key, new ReactiveProperty<double>(this.GetDouble(key, defaultValue)));
		}
		return this._Subscriptions[key].AsObservable<double>();
	}

	// Token: 0x06000E76 RID: 3702 RVA: 0x00040C90 File Offset: 0x0003EE90
	public static PlayerData GetPlayerData(string name)
	{
		PlayerData result;
		try
		{
			result = PlayerData.playerDatas[name];
		}
		catch (KeyNotFoundException)
		{
			result = new PlayerData(name);
		}
		return result;
	}

	// Token: 0x06000E77 RID: 3703 RVA: 0x00040CC8 File Offset: 0x0003EEC8
	public static void Clear()
	{
		PlayerData.playerDatas.Clear();
	}

	// Token: 0x06000E78 RID: 3704 RVA: 0x00040CD4 File Offset: 0x0003EED4
	public string Get(string name, string defaultValue = "")
	{
		if (this.playerData.ContainsKey(name))
		{
			return this.playerData[name];
		}
		return defaultValue;
	}

	// Token: 0x06000E79 RID: 3705 RVA: 0x00040CF4 File Offset: 0x0003EEF4
	public PlayerData Set(string name, string value)
	{
		bool flag = !this.playerData.ContainsKey(name) || this.playerData[name] != value;
		if (this.playerData.ContainsKey(name))
		{
			this.playerData[name] = value;
		}
		else
		{
			this.playerData.Add(name, value);
		}
		if (flag)
		{
			this.Save();
			if (this._Subscriptions.ContainsKey(name))
			{
				this._Subscriptions[name].Value = Convert.ToDouble(value);
			}
		}
		return this;
	}

	// Token: 0x06000E7A RID: 3706 RVA: 0x00040D7C File Offset: 0x0003EF7C
	public PlayerData Set(string name, double value)
	{
		string text = value.ToString();
		bool flag = !this.playerData.ContainsKey(name) || this.playerData[name] != text;
		if (this.playerData.ContainsKey(name))
		{
			this.playerData[name] = text;
		}
		else
		{
			this.playerData.Add(name, text);
		}
		if (flag)
		{
			this.Save();
			if (this._Subscriptions.ContainsKey(name))
			{
				this._Subscriptions[name].Value = value;
			}
		}
		return this;
	}

	// Token: 0x06000E7B RID: 3707 RVA: 0x00040E08 File Offset: 0x0003F008
	public bool GetBool(string name)
	{
		string text = this.Get(name, "0");
		return !string.IsNullOrEmpty(text) && (text == "1" || text.ToLower() == "true");
	}

	// Token: 0x06000E7C RID: 3708 RVA: 0x00040E4B File Offset: 0x0003F04B
	public void SetBool(string name, bool value)
	{
		this.Set(name, value ? "1" : "0");
	}

	// Token: 0x06000E7D RID: 3709 RVA: 0x00040E64 File Offset: 0x0003F064
	public double GetDouble(string name, double defaultValue = 0.0)
	{
		string text = this.Get(name, "");
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				return double.Parse(text);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
			}
			return defaultValue;
		}
		return defaultValue;
	}

	// Token: 0x06000E7E RID: 3710 RVA: 0x00040EB0 File Offset: 0x0003F0B0
	public float GetFloat(string name)
	{
		string text = this.Get(name, "");
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				return float.Parse(text);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
			}
		}
		return 0f;
	}

	// Token: 0x06000E7F RID: 3711 RVA: 0x00040F00 File Offset: 0x0003F100
	public int GetInt(string name, int defaultValue = 0)
	{
		string text = this.Get(name, "");
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				return int.Parse(text);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
			}
			return defaultValue;
		}
		return defaultValue;
	}

	// Token: 0x06000E80 RID: 3712 RVA: 0x00040F4C File Offset: 0x0003F14C
	public bool Has(string name)
	{
		return this.playerData.ContainsKey(name);
	}

	// Token: 0x06000E81 RID: 3713 RVA: 0x00040F5A File Offset: 0x0003F15A
	public void Remove(string name)
	{
		if (this.Has(name))
		{
			this.playerData.Remove(name);
		}
	}

	// Token: 0x06000E82 RID: 3714 RVA: 0x00040F74 File Offset: 0x0003F174
	public bool Spend(string name, double amount)
	{
		if (!this.Has(name))
		{
			return false;
		}
		double num = this.GetDouble(name, 0.0);
		if (num < amount)
		{
			return false;
		}
		num -= amount;
		this.Set(name, num.ToString());
		this.Save();
		if (this._Subscriptions.ContainsKey(name))
		{
			this._Subscriptions[name].Value = num;
		}
		return true;
	}

	// Token: 0x06000E83 RID: 3715 RVA: 0x00040FE0 File Offset: 0x0003F1E0
	public double Add(string name, double amount)
	{
		if (!this.Has(name))
		{
			this.Set(name, amount.ToString());
			return amount;
		}
		double num = this.GetDouble(name, 0.0);
		num += amount;
		this.Set(name, num.ToString());
		if (this._Subscriptions.ContainsKey(name))
		{
			this._Subscriptions[name].Value = num;
		}
		return num;
	}

	// Token: 0x06000E84 RID: 3716 RVA: 0x0004104C File Offset: 0x0003F24C
	public void Save()
	{
		string text = Serializer.Serialize<PlayerData>(this);
		PlayerData.PlatformSpecificSave(text, this.Name.ToLower());
		PlayerPrefs.SetString("PlayerData_" + this.Name, text);
		PlayerPrefs.Save();
	}

	// Token: 0x06000E85 RID: 3717 RVA: 0x00041094 File Offset: 0x0003F294
	public void Load()
	{
		string text = string.Format("PlayerData_{0}", this.Name);
		string @string = PlayerPrefs.GetString(text, "");
		if (string.IsNullOrEmpty(@string))
		{
			Debug.LogFormat("[{0}] not found", new object[]
			{
				text
			});
			this.TryLoadPlayerItems();
		}
		else
		{
			this.playerData = Serializer.Deserialize<PlayerData>(@string).playerData;
		}
		this.OnDataLoaded();
	}

	// Token: 0x06000E86 RID: 3718 RVA: 0x000410F9 File Offset: 0x0003F2F9
	public static void DeletePlayerData(string name)
	{
		PlayerData.playerDatas.Remove(name);
		new PlayerData(name);
		PlayerData.playerDatas[name].Save();
	}

	// Token: 0x06000E87 RID: 3719 RVA: 0x00041120 File Offset: 0x0003F320
	private void OnDataLoaded()
	{
		foreach (KeyValuePair<string, ReactiveProperty<double>> keyValuePair in this._Subscriptions)
		{
			keyValuePair.Value.Value = this.GetDouble(keyValuePair.Key, 0.0);
		}
	}

	// Token: 0x06000E88 RID: 3720 RVA: 0x00041190 File Offset: 0x0003F390
	private void TryLoadPlayerItems()
	{
		PlayerItems playerItems = PlayerItems.Load();
		if (playerItems == null)
		{
			return;
		}
		PlayerData playerData = PlayerData.GetPlayerData("Global");
		if (playerData != null)
		{
			if (playerItems.virtualCurrency > 0)
			{
				playerData.Set("Gold", playerItems.virtualCurrency.ToString());
			}
			if (playerItems.hasGoldenSuit)
			{
				playerData.Set("Gold Suit", "1");
			}
			playerData.Save();
		}
		PlayerData playerData2 = PlayerData.GetPlayerData("Earth");
		if (playerData2 != null)
		{
			if ((float)playerItems.multipliers > 0f)
			{
				playerData2.Set("Multipliers", playerItems.multipliers.ToString());
			}
			if ((float)playerItems.kongMultiplers > 0f)
			{
				playerData2.Set("KongMultipliers", playerItems.kongMultiplers.ToString());
			}
			playerData2.Save();
		}
		PlayerItems.Clear();
	}


	// Token: 0x04000C29 RID: 3113
	private static Dictionary<string, PlayerData> playerDatas = new Dictionary<string, PlayerData>();

	// Token: 0x04000C2B RID: 3115
	private Dictionary<string, string> playerData = new Dictionary<string, string>();

	// Token: 0x04000C2C RID: 3116
	private Dictionary<string, ReactiveProperty<double>> _Subscriptions = new Dictionary<string, ReactiveProperty<double>>();

	// Token: 0x04000C2D RID: 3117
	public IInventoryService inventory;

	// Token: 0x04000C2E RID: 3118
	public List<AdCapStoreItem> StoreItemsLive = new List<AdCapStoreItem>();
}
