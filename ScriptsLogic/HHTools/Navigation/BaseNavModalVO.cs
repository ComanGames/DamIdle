using System;

namespace HHTools.Navigation
{
	// Token: 0x020006E4 RID: 1764
	public class BaseNavModalVO
	{
		public string Name { get; private set; }

		public string ResourcePath { get; private set; }

		public Type ParameterType { get; private set; }

		public BaseNavModalVO(string pName, string pPath)
		{
			this.Name = pName;
			this.ResourcePath = pPath;
		}
	}
}
