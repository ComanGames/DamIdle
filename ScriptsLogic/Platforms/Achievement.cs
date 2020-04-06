using System;

namespace Platforms
{
	// Token: 0x020006BF RID: 1727
	public class Achievement
	{
		// Token: 0x06002314 RID: 8980 RVA: 0x0009960A File Offset: 0x0009780A
		public Achievement(string name, string desc)
		{
			this.Name = name;
			this.Description = desc;
		}

		// Token: 0x04002481 RID: 9345
		public string Name;

		// Token: 0x04002482 RID: 9346
		public string Description;

		// Token: 0x04002483 RID: 9347
		public bool Achieved;
	}
}
