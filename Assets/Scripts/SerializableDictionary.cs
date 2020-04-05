using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000191 RID: 401
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	// Token: 0x06000C80 RID: 3200 RVA: 0x0003830A File Offset: 0x0003650A
	public SerializableDictionary()
	{
	}

	// Token: 0x06000C81 RID: 3201 RVA: 0x00038314 File Offset: 0x00036514
	public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict.Count)
	{
		foreach (KeyValuePair<TKey, TValue> keyValuePair in dict)
		{
			base[keyValuePair.Key] = keyValuePair.Value;
		}
	}

	// Token: 0x06000C82 RID: 3202 RVA: 0x00038378 File Offset: 0x00036578
	public void CopyFrom(IDictionary<TKey, TValue> dict)
	{
		base.Clear();
		foreach (KeyValuePair<TKey, TValue> keyValuePair in dict)
		{
			base[keyValuePair.Key] = keyValuePair.Value;
		}
	}

	// Token: 0x06000C83 RID: 3203 RVA: 0x000383D4 File Offset: 0x000365D4
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

	// Token: 0x06000C84 RID: 3204 RVA: 0x0003844C File Offset: 0x0003664C
	public void OnBeforeSerialize()
	{
		int count = base.Count;
		this.m_keys = new TKey[count];
		this.m_values = new TValue[count];
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
		{
			this.m_keys[num] = keyValuePair.Key;
			this.m_values[num] = keyValuePair.Value;
			num++;
		}
	}

	// Token: 0x04000AAB RID: 2731
	[SerializeField]
	private TKey[] m_keys;

	// Token: 0x04000AAC RID: 2732
	[SerializeField]
	private TValue[] m_values;
}
