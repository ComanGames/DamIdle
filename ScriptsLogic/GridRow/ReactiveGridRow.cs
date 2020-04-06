using System;
using UniRx;

// Token: 0x02000253 RID: 595
public class ReactiveGridRow<T>
{
	// Token: 0x04000E3F RID: 3647
	public string Id;

	// Token: 0x04000E40 RID: 3648
	public ReactiveCollection<T> Elements = new ReactiveCollection<T>();
}
