using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000192 RID: 402
public class SerializableDictionaryIgnoreCase<TValue> : Dictionary<string, TValue>, ISerializationCallbackReceiver
{
	// Token: 0x06000C85 RID: 3205 RVA: 0x000384E0 File Offset: 0x000366E0
	public SerializableDictionaryIgnoreCase()
	{
	}

	// Token: 0x06000C86 RID: 3206 RVA: 0x000384E8 File Offset: 0x000366E8
	public SerializableDictionaryIgnoreCase(IDictionary<string, TValue> dict) : base(dict.Count)
	{
		foreach (KeyValuePair<string, TValue> keyValuePair in dict)
		{
			base[keyValuePair.Key] = keyValuePair.Value;
		}
	}

	// Token: 0x06000C87 RID: 3207 RVA: 0x0003854C File Offset: 0x0003674C
	public void CopyFrom(IDictionary<string, TValue> dict)
	{
		base.Clear();
		foreach (KeyValuePair<string, TValue> keyValuePair in dict)
		{
			base[keyValuePair.Key] = keyValuePair.Value;
		}
	}

	// Token: 0x06000C88 RID: 3208 RVA: 0x000385A8 File Offset: 0x000367A8
	public void OnAfterDeserialize()
	{
		if (this.m_keys == null || this.m_values == null || this.m_keys.Length != this.m_values.Length)
		{
			return;
		}
		base.Clear();
		int num = this.m_keys.Length;
		for (int i = 0; i < num; i++)
		{
			base[this.m_keys[i]] = this.m_values[i];
		}
		this.m_keys = null;
		this.m_values = null;
	}

	// Token: 0x06000C89 RID: 3209 RVA: 0x0003861C File Offset: 0x0003681C
	public void OnBeforeSerialize()
	{
		int count = base.Count;
		this.m_keys = new string[count];
		this.m_values = new TValue[count];
		int num = 0;
		foreach (KeyValuePair<string, TValue> keyValuePair in this)
		{
			this.m_keys[num] = keyValuePair.Key;
			this.m_values[num] = keyValuePair.Value;
			num++;
		}
	}

	// Token: 0x04000AAD RID: 2733
	[SerializeField]
	private string[] m_keys;

	// Token: 0x04000AAE RID: 2734
	[SerializeField]
	private TValue[] m_values;
}
