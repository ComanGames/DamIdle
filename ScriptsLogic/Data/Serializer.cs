using System;
using System.Text;
using LitJson;
using UnityEngine;

// Token: 0x02000220 RID: 544
public static class Serializer
{
	// Token: 0x06000F87 RID: 3975 RVA: 0x00047B3C File Offset: 0x00045D3C
	public static string Serialize<T>(T obj)
	{
		try
		{
			JsonWriter jsonWriter = new JsonWriter();
			jsonWriter.TypeHinting = true;
			jsonWriter.UseAssemblyQualifiedName = false;
			JsonMapper.ToJson(obj, jsonWriter);
			string text = jsonWriter.ToString();
			return "." + Convert.ToBase64String(CLZF2.Compress(Encoding.UTF8.GetBytes(text))) + "|" + ExportImport.Md5Sum(text);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		return null;
	}

	// Token: 0x06000F88 RID: 3976 RVA: 0x00047BB8 File Offset: 0x00045DB8
	public static T Deserialize<T>(string saveData)
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
					return default(T);
				}
				string @string = Encoding.UTF8.GetString(CLZF2.Decompress(Convert.FromBase64String(array[0].Substring(1))));
				if (!array[1].Equals(ExportImport.Md5Sum(@string)))
				{
					Debug.LogWarning("Checksum doesn't match!");
					return default(T);
				}
				saveData = @string;
				if (Application.isEditor)
				{
					Debug.Log("Loaded obj: " + saveData);
				}
			}
			return JsonMapper.ToObject<T>(new JsonReader(saveData)
			{
				TypeHinting = true
			});
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		return default(T);
	}
}
