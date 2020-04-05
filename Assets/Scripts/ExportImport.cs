using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020001AD RID: 429
public class ExportImport
{
	// Token: 0x06000CE3 RID: 3299 RVA: 0x00039AF0 File Offset: 0x00037CF0
	public static string Md5Sum(string strToEncrypt)
	{
		strToEncrypt += "makeripples";
		byte[] bytes = new UTF8Encoding().GetBytes(strToEncrypt);
		byte[] array = new MD5CryptoServiceProvider().ComputeHash(bytes);
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			text += Convert.ToString(array[i], 16).PadLeft(2, '0');
		}
		return text.PadLeft(32, '0');
	}

	// Token: 0x06000CE4 RID: 3300 RVA: 0x00039B5C File Offset: 0x00037D5C
	public static void SetData(string data)
	{
		GameState gameState = Serializer.Deserialize<GameState>(data);
		if (gameState == null)
		{
			return;
		}
		PlayerPrefs.SetString("GameState_" + gameState.planetName, data);
		SceneManager.LoadScene(gameState.planetName);
	}
}
