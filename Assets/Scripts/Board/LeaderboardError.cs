﻿using System;

// Token: 0x020001C8 RID: 456
public enum LeaderboardError
{
	// Token: 0x04000B93 RID: 2963
	NA,
	// Token: 0x04000B94 RID: 2964
	INVALID_DATA_RETURNED,
	// Token: 0x04000B95 RID: 2965
	FAILED_TO_DESERIALIZE,
	// Token: 0x04000B96 RID: 2966
	CLOUDSCRIPT_TIMEOUT_RETRY,
	// Token: 0x04000B97 RID: 2967
	CLOUDSCRIPT_TIMEOUT_ABORT,
	// Token: 0x04000B98 RID: 2968
	CLOUDSCRIPT_ERROR_RETRY,
	// Token: 0x04000B99 RID: 2969
	CLOUDSCRIPT_ERROR_ABORT,
	// Token: 0x04000B9A RID: 2970
	POST_SCORE_ERROR_RETRY,
	// Token: 0x04000B9B RID: 2971
	POST_SCORE_ERROR_ABORT,
	// Token: 0x04000B9C RID: 2972
	UNKOWN_ERROR
}