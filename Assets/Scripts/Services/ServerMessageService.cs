using System;
using System.Collections.Generic;
using HyperHippo;
using Platforms;
using PlayFab.ClientModels;
using UniRx;

// Token: 0x020000ED RID: 237
public class ServerMessageService : IDisposable
{
	// Token: 0x06000638 RID: 1592 RVA: 0x00021A4E File Offset: 0x0001FC4E
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000639 RID: 1593 RVA: 0x00002718 File Offset: 0x00000918
	public void Init()
	{
	}

	// Token: 0x1700007F RID: 127
	// (get) Token: 0x0600063A RID: 1594 RVA: 0x00021A5B File Offset: 0x0001FC5B
	public bool HasPendingNews
	{
		get
		{
			return this.pendingNewsItems.Count > 0;
		}
	}

	// Token: 0x0600063B RID: 1595 RVA: 0x00021A6B File Offset: 0x0001FC6B
	public TitleNewsItem GetAndRemoveNextPendingNewsItem()
	{
		if (this.pendingNewsItems.Count > 0)
		{
			TitleNewsItem result = this.pendingNewsItems[0];
			this.pendingNewsItems.RemoveAt(0);
			return result;
		}
		return null;
	}

	// Token: 0x0600063C RID: 1596 RVA: 0x00021A95 File Offset: 0x0001FC95
	private void CheckServerForMessage(long t)
	{
		Helper.GetPlatformAccount().PlayFab.GetTitleNews(5, new Action<GetTitleNewsResult>(this.OnEventTitleNews), null);
	}

	// Token: 0x0600063D RID: 1597 RVA: 0x00021AB4 File Offset: 0x0001FCB4
	private void OnEventTitleNews(GetTitleNewsResult newsResult)
	{
		double @double = this.globalPlayerData.GetDouble("LastNewsUpdate", 0.0);
		for (int i = 0; i < newsResult.News.Count; i++)
		{
			if (Util.UnixTime(newsResult.News[i].Timestamp) > @double && !this.ExistsInPending(newsResult.News[i]))
			{
				this.pendingNewsItems.Add(newsResult.News[i]);
			}
		}
	}

	// Token: 0x0600063E RID: 1598 RVA: 0x00021B38 File Offset: 0x0001FD38
	private bool ExistsInPending(TitleNewsItem item)
	{
		for (int i = 0; i < this.pendingNewsItems.Count; i++)
		{
			if (item.NewsId == this.pendingNewsItems[i].NewsId)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x040005BC RID: 1468
	private const int CHECK_SERVER_INTERVAL_MINUTES = 30;

	// Token: 0x040005BD RID: 1469
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x040005BE RID: 1470
	private PlayerData globalPlayerData;

	// Token: 0x040005BF RID: 1471
	private List<TitleNewsItem> pendingNewsItems = new List<TitleNewsItem>();
}
