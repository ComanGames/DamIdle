using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Platforms.Logger;
using UnityEngine;

namespace Platforms
{
	// Token: 0x020006D0 RID: 1744
	public class Helper
	{
		// Token: 0x060023F9 RID: 9209 RVA: 0x0009BB8C File Offset: 0x00099D8C
		public static void SetPlatformAccount(PlatformAccount platform)
		{
			Helper.accountInstance = platform;
		}

		// Token: 0x060023FA RID: 9210 RVA: 0x0009BB94 File Offset: 0x00099D94
		public static PlatformAccount GetPlatformAccount()
		{
			return Helper.GetPlatformAccount(Resources.Load<TextAsset>("playfab-settings").text);
		}

		// Token: 0x060023FB RID: 9211 RVA: 0x0009BBAA File Offset: 0x00099DAA
		public static PlatformAccount GetPlatformAccount(string titleId)
		{
			if (Helper.accountInstance == null)
			{
				Helper.accountInstance = new PlatformAccount().Init(titleId, null);
			}
			return Helper.accountInstance;
		}

		// Token: 0x060023FC RID: 9212 RVA: 0x0009BBC9 File Offset: 0x00099DC9
		public static void DisposePlatformAccount()
		{
			if (Helper.accountInstance != null)
			{
				Helper.accountInstance.Dispose();
				Helper.accountInstance = null;
			}
		}

		// Token: 0x060023FD RID: 9213 RVA: 0x0009BBE2 File Offset: 0x00099DE2
		public static void SetPlatformStore(IPlatformStore platform)
		{
			Helper.storeInstance = platform;
		}

		// Token: 0x060023FE RID: 9214 RVA: 0x0009BBEA File Offset: 0x00099DEA
		public static IPlatformStore GetPlatformStore()
		{
			if (Helper.storeInstance == null)
			{
				Helper.storeInstance = new PlatformStore();
				if (Helper.storeInstance != null && Helper.accountInstance != null)
				{
					Helper.storeInstance.Init(Helper.accountInstance.PlayFab, Helper.accountInstance);
				}
			}
			return Helper.storeInstance;
		}

		// Token: 0x060023FF RID: 9215 RVA: 0x0009BC2A File Offset: 0x00099E2A
		public static void DisposePlatformStore()
		{
			if (Helper.storeInstance != null)
			{
				Helper.storeInstance.Dispose();
				Helper.storeInstance = null;
			}
		}

		// Token: 0x06002400 RID: 9216 RVA: 0x0009BC43 File Offset: 0x00099E43
		public static void SetPlatformAd(PlatformAd platform)
		{
			Helper.adInstance = platform;
		}

		// Token: 0x06002401 RID: 9217 RVA: 0x0009BC4B File Offset: 0x00099E4B
		public static IPlatformAd GetPlatformAd()
		{
			if (Helper.adInstance == null)
			{
				Helper.adInstance = new PlatformAd();
				Helper.adInstance.Init();
			}
			return Helper.adInstance;
		}

		// Token: 0x06002402 RID: 9218 RVA: 0x0009BC6D File Offset: 0x00099E6D
		public static void DisposePlatformAd()
		{
			if (Helper.adInstance != null)
			{
				Helper.adInstance.Dispose();
				Helper.adInstance = null;
			}
		}

		// Token: 0x06002403 RID: 9219 RVA: 0x0009BC88 File Offset: 0x00099E88
		public static List<Helper.PlatformContainer<T>> GetAvailableInstances<T>(PlatformType platformType) where T : IPlatform
		{
			T[] instancesForAssignableTypes = Helper.GetInstancesForAssignableTypes<T>();
			List<Helper.PlatformContainer<T>> list = new List<Helper.PlatformContainer<T>>();
			foreach (T platform in instancesForAssignableTypes)
			{
				int num = platform.EnabledForPlatform(platformType);
				if (num > -1)
				{
					list.Add(new Helper.PlatformContainer<T>
					{
						Platform = platform,
						Priority = num
					});
				}
			}
			list = (from v in list
			orderby v.Priority descending
			select v).ToList<Helper.PlatformContainer<T>>();
			StringBuilder stringBuilder = new StringBuilder().AppendLine(string.Format("[{0}] platforms available for type [{1}]:", typeof(T), platformType));
			foreach (Helper.PlatformContainer<T> platformContainer in list)
			{
				StringBuilder stringBuilder2 = stringBuilder.Append("\t");
				T platform2 = platformContainer.Platform;
				StringBuilder stringBuilder3 = stringBuilder2.Append(platform2.GetType().FullName).Append(" [");
				int priority = platformContainer.Priority;
				stringBuilder3.Append(priority.ToString()).AppendLine("]");
			}
			Platforms.Logger.Logger.GetLogger(typeof(Helper).FullName).Info(stringBuilder.ToString());
			return list;
		}

		// Token: 0x06002404 RID: 9220 RVA: 0x0009BDF0 File Offset: 0x00099FF0
		public static T GetPlatformInstance<T>() where T : IPlatform
		{
			PlatformType platformType = Helper.GetPlatformType();
			List<Helper.PlatformContainer<T>> availableInstances = Helper.GetAvailableInstances<T>(platformType);
			if (availableInstances.Count == 0)
			{
				throw new PlatformHelperException(string.Format("Unable to determine appropriate platform for type [{0}]", platformType));
			}
			return availableInstances[0].Platform;
		}

		// Token: 0x06002405 RID: 9221 RVA: 0x0009BE34 File Offset: 0x0009A034
		public static T[] GetInstancesForAssignableTypes<T>()
		{
			List<Type> assignableTypes = Helper.GetAssignableTypes(typeof(T), typeof(Helper).Namespace, true);
			T[] array = new T[assignableTypes.Count];
			for (int i = 0; i < assignableTypes.Count; i++)
			{
				array[i] = (T)((object)Activator.CreateInstance(assignableTypes[i]));
			}
			return array;
		}

		// Token: 0x06002406 RID: 9222 RVA: 0x0009BE97 File Offset: 0x0009A097
		public static List<Type> GetAssignableTypes<T>()
		{
			return Helper.GetAssignableTypes(typeof(T), typeof(Helper).Namespace, true);
		}

		// Token: 0x06002407 RID: 9223 RVA: 0x0009BEB8 File Offset: 0x0009A0B8
		public static PlatformType GetPlatformType()
		{
			return PlatformType.Steam;
		}

		// Token: 0x06002408 RID: 9224 RVA: 0x0009BEBC File Offset: 0x0009A0BC
		private static List<Type> GetAssignableTypes(Type baseType, string namespaceName, bool includeChildren)
		{
			List<Type> list = new List<Type>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Assembly assembly = assemblies[i];
				if (!assembly.FullName.StartsWith("Unity") && !assembly.FullName.StartsWith("Boo") && !assembly.FullName.StartsWith("Mono") && !assembly.FullName.StartsWith("System") && !assembly.FullName.StartsWith("mscorlib"))
				{
					try
					{
						foreach (Type type in assemblies[i].GetTypes())
						{
							if ((string.IsNullOrEmpty(namespaceName) || (includeChildren && !string.IsNullOrEmpty(type.Namespace) && type.Namespace.StartsWith(namespaceName)) || (!includeChildren && type.Namespace == namespaceName)) && type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
							{
								list.Add(type);
							}
						}
					}
					catch (ReflectionTypeLoadException)
					{
					}
				}
			}
			return list;
		}

		// Token: 0x06002409 RID: 9225 RVA: 0x0009BFFC File Offset: 0x0009A1FC
		public static string GetMajorMinorVersion()
		{
			string result;
			try
			{
				string[] array = Application.version.Split(new char[]
				{
					'.'
				});
				result = array[0] + "." + array[1];
			}
			catch
			{
				result = "<unknown>";
			}
			return result;
		}

		// Token: 0x040024D8 RID: 9432
		private static PlatformAccount accountInstance;

		// Token: 0x040024D9 RID: 9433
		private static IPlatformStore storeInstance;

		// Token: 0x040024DA RID: 9434
		private static PlatformAd adInstance;

		// Token: 0x02000A2C RID: 2604
		public struct PlatformContainer<T>
		{
			// Token: 0x04003031 RID: 12337
			public int Priority;

			// Token: 0x04003032 RID: 12338
			public T Platform;
		}
	}
}
