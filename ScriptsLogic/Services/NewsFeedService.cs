using System;
using LitJson;
using Platforms;
using PlayFab.ClientModels;
using UniRx;
using UnityEngine;

// Token: 0x020001A5 RID: 421
public class NewsFeedService : IDisposable
{
	// Token: 0x06000CAE RID: 3246 RVA: 0x00038E58 File Offset: 0x00037058
	public void Init(bool areNotificationsEnabled)
	{
		this.areNotificationsEnabled = areNotificationsEnabled;
		this.LoadNewsFromPlayerPrefs();
		Helper.GetPlatformAccount().PlayFab.GetTitleData("newsfeed_steam", new Action<GetTitleDataResult>(this.OnNewsRecieved), null);
	}

	// Token: 0x06000CAF RID: 3247 RVA: 0x00038E88 File Offset: 0x00037088
	public void Dispose()
	{
		this.disposables.Dispose();
	}

	// Token: 0x06000CB0 RID: 3248 RVA: 0x00038E98 File Offset: 0x00037098
	private void OnNewsRecieved(GetTitleDataResult result)
	{
		NewsFeedService.NewsPaper newsPaper = JsonUtility.FromJson<NewsFeedService.NewsPaper>(result.Data["newsfeed_steam"]);
		string text = string.Empty;
		if (this.Newspaper != null)
		{
			text = this.Newspaper.ReleaseDate;
		}
		this.Newspaper = newsPaper;
		if (text != this.Newspaper.ReleaseDate)
		{
			PlayerPrefs.SetString("AdCapNews", JsonMapper.ToJson(newsPaper));
			if (!string.IsNullOrEmpty(text))
			{
				PlayerPrefs.SetString("UnreadNews", "true");
				this.NewMessageNotificationActive(this.areNotificationsEnabled);
			}
			else
			{
				PlayerPrefs.SetString("UnreadNews", "false");
				this.NewMessageNotificationActive(false);
			}
			this.LoadNewsFromPlayerPrefs();
			return;
		}
		if (PlayerPrefs.GetString("UnreadNews") == "true")
		{
			this.NewMessageNotificationActive(this.areNotificationsEnabled);
		}
	}

	// Token: 0x06000CB1 RID: 3249 RVA: 0x00038F64 File Offset: 0x00037164
	private void LoadNewsFromPlayerPrefs()
	{
		if (PlayerPrefs.HasKey("AdCapNews"))
		{
			string @string = PlayerPrefs.GetString("AdCapNews");
			this.Newspaper = JsonMapper.ToObject<NewsFeedService.NewsPaper>(@string);
		}
		if (PlayerPrefs.GetString("UnreadNews") == "true")
		{
			this.NewMessageNotificationActive(this.areNotificationsEnabled);
		}
	}

	// Token: 0x06000CB2 RID: 3250 RVA: 0x00038FB6 File Offset: 0x000371B6
	private void NewMessageNotificationActive(bool isEnabled)
	{
		this.NewMessageNotificationEnabled.Value = isEnabled;
	}

	// Token: 0x06000CB3 RID: 3251 RVA: 0x00038FC4 File Offset: 0x000371C4
	public void MarkNewsAsRead()
	{
		PlayerPrefs.SetString("UnreadNews", "false");
		this.NewMessageNotificationActive(false);
	}

	// Token: 0x04000AC9 RID: 2761
	private const string TITLE_DATA_KEY = "newsfeed_steam";

	// Token: 0x04000ACA RID: 2762
	public NewsFeedService.NewsPaper Newspaper;

	// Token: 0x04000ACB RID: 2763
	public BoolReactiveProperty NewMessageNotificationEnabled = new BoolReactiveProperty();

	// Token: 0x04000ACC RID: 2764
	private CompositeDisposable disposables = new CompositeDisposable();

	// Token: 0x04000ACD RID: 2765
	private bool areNotificationsEnabled;

	// Token: 0x0200087C RID: 2172
	[Serializable]
	public class NewsArticle
	{
		// Token: 0x04002B25 RID: 11045
		public string newsArticleTitle;

		// Token: 0x04002B26 RID: 11046
		public string newsArticleBody;
	}

	// Token: 0x0200087D RID: 2173
	[Serializable]
	public class NewsPaper
	{
		// Token: 0x04002B27 RID: 11047
		public string ReleaseDate;

		// Token: 0x04002B28 RID: 11048
		public NewsFeedService.NewsArticle LatestNews;

		// Token: 0x04002B29 RID: 11049
		public NewsFeedService.NewsArticle ReleaseNotes;
	}
}
