using System;
using UniRx;
using UnityEngine;
using Utils;

// Token: 0x020001E6 RID: 486
public class OrientationController
{
	// Token: 0x1700012C RID: 300
	// (get) Token: 0x06000E31 RID: 3633 RVA: 0x0003FEA7 File Offset: 0x0003E0A7
	public static OrientationController Instance
	{
		get
		{
			OrientationController result;
			if ((result = OrientationController._Instance) == null)
			{
				result = (OrientationController._Instance = new OrientationController());
			}
			return result;
		}
	}

	// Token: 0x1700012D RID: 301
	// (get) Token: 0x06000E32 RID: 3634 RVA: 0x0003FEBD File Offset: 0x0003E0BD
	public IObservable<OrientationChangedEvent> OrientationStream
	{
		get
		{
			return this._CurrentOrientation.AsObservable<OrientationChangedEvent>();
		}
	}

	// Token: 0x1700012E RID: 302
	// (get) Token: 0x06000E33 RID: 3635 RVA: 0x0003FECA File Offset: 0x0003E0CA
	public OrientationChangedEvent CurrentOrientation
	{
		get
		{
			return this._CurrentOrientation.Value;
		}
	}

	// Token: 0x06000E34 RID: 3636 RVA: 0x0003FED8 File Offset: 0x0003E0D8
	~OrientationController()
	{
		this.dispossable.Dispose();
	}

	// Token: 0x06000E35 RID: 3637 RVA: 0x0003FF0C File Offset: 0x0003E10C
	public void Init(TimerService timerService)
	{
		this.dispossable.Dispose();
		this.dispossable = timerService.GetTimer(TimerService.TimerGroups.State).Subscribe(delegate(TimeSpan time)
		{
			if ((float)Screen.width != this.prevScreenWidth)
			{
				this.OrientationChange();
			}
		});
	}

	// Token: 0x06000E36 RID: 3638 RVA: 0x0003FF38 File Offset: 0x0003E138
	private void OrientationChange()
	{
		this.prevScreenWidth = (float)Screen.width;
		OrientationChangedEvent value = new OrientationChangedEvent
		{
			IsPortrait = (this.prevScreenWidth < (float)Screen.height),
			AspectRatio = this.GetAspectRatio(Screen.width, Screen.height)
		};
		this._CurrentOrientation.Value = value;
	}

	// Token: 0x06000E37 RID: 3639 RVA: 0x0003FF94 File Offset: 0x0003E194
	private Vector2 GetAspectRatio(int x, int y)
	{
		float num = (float)x / (float)y;
		int num2 = 0;
		do
		{
			num2++;
		}
		while (Math.Round((double)(num * (float)num2), 2) != (double)Mathf.RoundToInt(num * (float)num2));
		return new Vector2((float)Math.Round((double)(num * (float)num2), 2), (float)num2);
	}

	// Token: 0x04000C0C RID: 3084
	private static OrientationController _Instance;

	// Token: 0x04000C0D RID: 3085
	private float prevScreenWidth;

	// Token: 0x04000C0E RID: 3086
	private readonly ReactiveProperty<OrientationChangedEvent> _CurrentOrientation = new ReactiveProperty<OrientationChangedEvent>(new OrientationChangedEvent
	{
		IsPortrait = true,
		AspectRatio = new Vector2(16f, 9f)
	});

	// Token: 0x04000C0F RID: 3087
	private IDisposable dispossable = Disposable.Empty;
}
