using System;
using UnityEngine;

namespace AdCap
{
	// Token: 0x020006EB RID: 1771
	public class RandomThing : MonoBehaviour
	{
		// Token: 0x060024AD RID: 9389 RVA: 0x0009E990 File Offset: 0x0009CB90
		private void Awake()
		{
			Debug.LogFormat("[{0}] Awake!", new object[]
			{
				base.name
			});
		}

		// Token: 0x060024AE RID: 9390 RVA: 0x0009E9AB File Offset: 0x0009CBAB
		private void OnEnable()
		{
			Debug.LogFormat("[{0}] OnEnable!", new object[]
			{
				base.name
			});
		}

		// Token: 0x060024AF RID: 9391 RVA: 0x0009E9C6 File Offset: 0x0009CBC6
		private void Start()
		{
			Debug.LogFormat("[{0}] Start!", new object[]
			{
				base.name
			});
		}

		// Token: 0x060024B0 RID: 9392 RVA: 0x0009E9E1 File Offset: 0x0009CBE1
		private void OnDisable()
		{
			Debug.LogFormat("[{0}] OnDisable!", new object[]
			{
				base.name
			});
		}

		// Token: 0x060024B1 RID: 9393 RVA: 0x0009E9FC File Offset: 0x0009CBFC
		private void OnApplicationPause(bool isPaused)
		{
			Debug.LogFormat("[{0}] OnApplicationPause({1})!", new object[]
			{
				base.name,
				isPaused
			});
		}

		// Token: 0x060024B2 RID: 9394 RVA: 0x0009EA20 File Offset: 0x0009CC20
		private void OnApplicationFocus(bool isFocused)
		{
			Debug.LogFormat("[{0}] OnApplicationFocus({1})!", new object[]
			{
				base.name,
				isFocused
			});
		}
	}
}
