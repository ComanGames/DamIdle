using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001AC RID: 428
public class EventPromoModal : AnimatedModal
{
	// Token: 0x06000CDE RID: 3294 RVA: 0x00039940 File Offset: 0x00037B40
	protected override void Awake()
	{
		this.btn_Launch.OnClickAsObservable().Subscribe(new Action<Unit>(this.LaunchEvent)).AddTo(base.gameObject);
		this.btn_Close.OnClickAsObservable().Subscribe(new Action<Unit>(this.CloseModal)).AddTo(base.gameObject);
		base.Awake();
	}

	// Token: 0x06000CDF RID: 3295 RVA: 0x000399A4 File Offset: 0x00037BA4
	public void WireData(EventModel eventData)
	{
		Color tintColor = eventData.TintColor;
		this.eventId = eventData.Id;
		if (eventData != null)
		{
			this.img_Promo.color = eventData.TintColor;
			GameController.Instance.IconService.LoadPromoImage(eventData.PlanetTheme).Subscribe(delegate(Sprite sprite)
			{
				this.img_Promo.sprite = sprite;
				this.img_Promo.color = Color.white;
			}).AddTo(base.gameObject);
		}
		this.txt_Banner.text = eventData.Name;
		this.txt_Title.text = eventData.PromoTitle;
		this.txt_Body.text = eventData.PromoBody;
		this.img_Banner.color = tintColor;
		this.img_PromoBg.color = tintColor;
		GameController.Instance.GlobalPlayerData.Set("last_promo_event_id_seen", eventData.Id);
	}

	// Token: 0x06000CE0 RID: 3296 RVA: 0x00039A70 File Offset: 0x00037C70
	private void LaunchEvent(Unit u)
	{
		string result = string.Format("{0}:{1}", GameController.Instance.game.planetName, this.eventId);
		GameController.Instance.AnalyticService.SendNavActionAnalytics("EventLaunchClicked", "Event_Promo_Modal", result);
		GameController.Instance.LoadPlanetScene(this.eventId, false);
		this.CloseModal(u);
	}

	// Token: 0x04000AE7 RID: 2791
	public Image img_Promo;

	// Token: 0x04000AE8 RID: 2792
	public Text txt_Banner;

	// Token: 0x04000AE9 RID: 2793
	public Text txt_Title;

	// Token: 0x04000AEA RID: 2794
	public Text txt_Body;

	// Token: 0x04000AEB RID: 2795
	public Image img_Banner;

	// Token: 0x04000AEC RID: 2796
	public Image img_PromoBg;

	// Token: 0x04000AED RID: 2797
	public Button btn_Launch;

	// Token: 0x04000AEE RID: 2798
	public Button btn_Close;

	// Token: 0x04000AEF RID: 2799
	private string eventId;
}
