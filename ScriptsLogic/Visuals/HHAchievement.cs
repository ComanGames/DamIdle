using System;
using UniRx;

// Token: 0x02000164 RID: 356
public class HHAchievement
{
	// Token: 0x17000108 RID: 264
	// (get) Token: 0x06000B68 RID: 2920 RVA: 0x00034047 File Offset: 0x00032247
	// (set) Token: 0x06000B69 RID: 2921 RVA: 0x0003404F File Offset: 0x0003224F
	public string Id { get; private set; }

	// Token: 0x06000B6A RID: 2922 RVA: 0x00034058 File Offset: 0x00032258
	public HHAchievement(string id)
	{
		this.Id = id;
	}

	// Token: 0x06000B6B RID: 2923 RVA: 0x00034074 File Offset: 0x00032274
	~HHAchievement()
	{
		this.Dispose();
	}

	// Token: 0x06000B6C RID: 2924 RVA: 0x000340A0 File Offset: 0x000322A0
	public void Dispose()
	{
		this._Disposable.Dispose();
	}

	// Token: 0x06000B6D RID: 2925 RVA: 0x000340B0 File Offset: 0x000322B0
	public HHAchievement SetStream<T>(IObservable<T> stream, Action<HHAchievement> onEarned)
	{
		this._Disposable = stream.TakeWhile(_ => !this.Achieved).Subscribe(delegate(T _)
		{
			onEarned(this);
		});
		return this;
	}

	// Token: 0x040009D8 RID: 2520
	public bool Achieved;

	// Token: 0x040009D9 RID: 2521
	private IDisposable _Disposable = Disposable.Empty;
}
