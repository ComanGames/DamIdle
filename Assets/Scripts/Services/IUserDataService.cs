using System;
using System.Collections.Generic;
using AdCap;
using UniRx;

// Token: 0x02000055 RID: 85
public interface IUserDataService : IDisposable
{
	// Token: 0x060002BD RID: 701
	void Init(UserTargetingService userTargetingService);

	// Token: 0x1700003A RID: 58
	// (get) Token: 0x060002BE RID: 702
	ReactiveProperty<string> UserName { get; }

	// Token: 0x060002BF RID: 703
	string ListAbTestGroups();

	// Token: 0x060002C0 RID: 704
	bool IsTestGroupMember(string testGroup);

	// Token: 0x060002C1 RID: 705
	bool IsTestGroupMember(string testId, string testGroup);


	// Token: 0x060002C3 RID: 707
	void HandleUserTargetingInfo(UserTargetingInfoRequestResult requestResult);
}
