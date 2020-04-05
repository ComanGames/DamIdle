using System;
using HHTools.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000166 RID: 358
public class EventInfoPopupUI : MonoBehaviour
{
	// Token: 0x06000B74 RID: 2932 RVA: 0x000342A4 File Offset: 0x000324A4
	private void Awake()
	{
		this.button = base.GetComponent<Button>();
		this.button.OnClickAsObservable().Subscribe(delegate(Unit _)
		{
			this.OnClickButton();
		});
	}

	// Token: 0x06000B75 RID: 2933 RVA: 0x000342CF File Offset: 0x000324CF
	public void Setup(EventModel model)
	{
		this.model = model;
		this.model.State.Subscribe(delegate(EventState state)
		{
			if (state != EventState.current)
			{
				Object.Destroy(base.gameObject);
			}
		}).AddTo(base.gameObject);
	}

	// Token: 0x06000B76 RID: 2934 RVA: 0x00034300 File Offset: 0x00032500
	private void OnClickButton()
	{
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData(this.model.Name, this.model.FeatureTutorialText, null, "", PopupModal.PopupOptions.OK, "Dandy!", "", false);
	}

	// Token: 0x040009E8 RID: 2536
	private Button button;

	// Token: 0x040009E9 RID: 2537
	private EventModel model;
}
