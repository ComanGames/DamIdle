using System;
using System.Text;
using LitJson;
using UnityEngine;

// Token: 0x020001BE RID: 446
[Obsolete("This class exists for migration purposes only.  Use PlayerData instead.")]
public class PlayerItems
{
	// Token: 0x1700011D RID: 285
	// (get) Token: 0x06000D55 RID: 3413 RVA: 0x0003BCAB File Offset: 0x00039EAB
	public int multipliers
	{
		get
		{
			return this._multipliers;
		}
	}

	// Token: 0x1700011E RID: 286
	// (get) Token: 0x06000D56 RID: 3414 RVA: 0x0003BCB3 File Offset: 0x00039EB3
	public int kongMultiplers
	{
		get
		{
			return this._kongMultipliers;
		}
	}

	// Token: 0x1700011F RID: 287
	// (get) Token: 0x06000D57 RID: 3415 RVA: 0x0003BCBB File Offset: 0x00039EBB
	public bool hasGoldenSuit
	{
		get
		{
			return this._hasGoldenSuit;
		}
	}

	// Token: 0x17000120 RID: 288
	public int virtualCurrency
	{
		get
		{
			return this._virtualCurrency;
		}
	}

	// Token: 0x06000D59 RID: 3417 RVA: 0x0003BCCC File Offset: 0x00039ECC
	public static PlayerItems Load()
	{
		string @string = PlayerPrefs.GetString(PlayerItems.saveName, "");
		if (string.IsNullOrEmpty(@string))
		{
			return null;
		}
		return PlayerItems.DeserializePlayerItems(@string);
	}

	// Token: 0x06000D5A RID: 3418 RVA: 0x0003BCF9 File Offset: 0x00039EF9
	public static void Clear()
	{
		PlayerPrefs.DeleteKey(PlayerItems.saveName);
	}

	// Token: 0x06000D5B RID: 3419 RVA: 0x0003BD08 File Offset: 0x00039F08
	private static PlayerItems DeserializePlayerItems(string saveData)
	{
		try
		{
			if (saveData.StartsWith("."))
			{
				string[] array = saveData.Split(new char[]
				{
					'|'
				});
				if (array.Length != 2)
				{
					Debug.LogWarning("Invalid import data!");
					return null;
				}
				string @string = Encoding.UTF8.GetString(CLZF2.Decompress(Convert.FromBase64String(array[0].Substring(1))));
				if (!array[1].Equals(ExportImport.Md5Sum(@string)))
				{
					Debug.LogWarning("Checksum doesn't match!");
					return null;
				}
				saveData = @string;
			}
			return JsonMapper.ToObject<PlayerItems>(new JsonReader(saveData)
			{
				TypeHinting = true
			});
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		return null;
	}

	// Token: 0x04000B46 RID: 2886
	private static string saveName = "026a8bw";

	// Token: 0x04000B47 RID: 2887
	private int _multipliers;

	// Token: 0x04000B48 RID: 2888
	private int _kongMultipliers;

	private bool _hasGoldenSuit;

	// Token: 0x04000B4A RID: 2890
	private int _virtualCurrency;
}
