using System;
using System.Collections.Generic;
using System.Linq;
using AdCap;
using Platforms.Logger;
using UniRx;

// Token: 0x02000054 RID: 84
public class UserDataService : IUserDataService, IDisposable
{
	// Token: 0x17000037 RID: 55
	// (get) Token: 0x060002B2 RID: 690 RVA: 0x0000F2B5 File Offset: 0x0000D4B5
	public ReactiveProperty<string> UserName { get; }

	// Token: 0x060002B3 RID: 691 RVA: 0x0000F2BD File Offset: 0x0000D4BD
	public UserDataService()
	{
		this.UserName = new ReactiveProperty<string>();
		this.logger = Logger.GetLogger(this);
	}

	// Token: 0x060002B4 RID: 692 RVA: 0x0000F2DC File Offset: 0x0000D4DC
	public void Init(UserTargetingService userTargetingService)
	{
		this.userTargetingService = userTargetingService;
		if (this.playFabSegments != null)
		{
			this.userTargetingService.SetExternalSegments(this.playFabSegments);
		}
		this.userTargetingService.Evaluate();
		this.logger.Info("TestGroups=" + this.ListAbTestGroups());
		this.initialized = true;
	}

	// Token: 0x060002B5 RID: 693 RVA: 0x00002718 File Offset: 0x00000918
	public void Dispose()
	{
	}

	// Token: 0x060002B6 RID: 694 RVA: 0x0000F336 File Offset: 0x0000D536
	public string ListAbTestGroups()
	{
		if (!this.initialized)
		{
			return string.Empty;
		}
		return this.userTargetingService.GetActiveTestGroupCsv();
	}

	// Token: 0x060002B7 RID: 695 RVA: 0x0000F354 File Offset: 0x0000D554
	public bool IsTestGroupMember(string testId)
	{
		if (testId[0] == '!')
		{
			string testGroup = testId.Substring(1);
			return !this.userTargetingService.IsAnyTestInGroupActive(testGroup);
		}
		return this.userTargetingService.IsInTestGroup(testId);
	}

	// Token: 0x060002B8 RID: 696 RVA: 0x0000F390 File Offset: 0x0000D590
	public bool IsTestGroupMember(string testId, string testGroup)
	{
		return this.userTargetingService.IsInTestGroup(testId, testGroup);
	}

	// Token: 0x060002B9 RID: 697 RVA: 0x0000F3A0 File Offset: 0x0000D5A0

	// Token: 0x060002BA RID: 698 RVA: 0x0000F3F6 File Offset: 0x0000D5F6
	public void HandleUserTargetingInfo(UserTargetingInfoRequestResult requestResult)
	{
		UserTargetingService.SetLegacyTestGroups(requestResult.TestGroups);
	}

	// Token: 0x17000038 RID: 56
	// (get) Token: 0x060002BB RID: 699 RVA: 0x0000F40F File Offset: 0x0000D60F
	public static bool UsesNewFtbbArt
	{
		get
		{
			return false;
		}
	}

	// Token: 0x17000039 RID: 57
	// (get) Token: 0x060002BC RID: 700 RVA: 0x0000F40F File Offset: 0x0000D60F
	public static bool HideBundleTimer
	{
		get
		{
			return false;
		}
	}

	// Token: 0x04000253 RID: 595
	private Logger logger;

	// Token: 0x04000254 RID: 596
	private UserTargetingService userTargetingService;

	// Token: 0x04000255 RID: 597
	private string[] playFabSegments;

	// Token: 0x04000256 RID: 598
	private bool initialized;
}
