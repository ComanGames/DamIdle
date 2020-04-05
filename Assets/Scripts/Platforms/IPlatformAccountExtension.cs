using System;
using Platforms.Logger;
using PlayFab;
using PlayFab.ClientModels;

namespace Platforms
{
	// Token: 0x020006C4 RID: 1732
	public interface IPlatformAccountExtension : IPlatform
	{
		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06002326 RID: 8998
		PlatformType PlatformType { get; }

		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x06002327 RID: 8999
		string DisplayName { get; }

		// Token: 0x06002328 RID: 9000
		IPlatformAccountExtension InitPlatform(PlatformAccount account, Logger logger, PlayFabWrapper playFab);

		// Token: 0x06002329 RID: 9001
		void DisposePlatform();

		// Token: 0x0600232A RID: 9002
		void Login(Action<LoginResult> onResult, Action<PlayFabError> onError);

		// Token: 0x0600232B RID: 9003
		void LinkAccount(bool force, Action onSuccess, Action<PlayFabError> onError);

		// Token: 0x0600232C RID: 9004
		void UnlinkAccount(Action onSuccess, Action<PlayFabError> onError);
	}
}
