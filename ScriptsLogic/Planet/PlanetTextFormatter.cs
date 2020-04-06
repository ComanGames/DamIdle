using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020001E9 RID: 489
[RequireComponent(typeof(Text))]
public class PlanetTextFormatter : MonoBehaviour
{
	// Token: 0x06000E3D RID: 3645 RVA: 0x00040130 File Offset: 0x0003E330
	private void Start()
	{
		this.text = base.GetComponent<Text>();
		this.originalText = this.text.text;
		GameController.Instance.State.Subscribe(new Action<GameState>(this.OnStateReady)).AddTo(this);
	}

	// Token: 0x06000E3E RID: 3646 RVA: 0x0004017C File Offset: 0x0003E37C
	private void OnStateReady(GameState state)
	{
		if (state.planetName.ToLower() == "moon" && !this.removeTheFromMoon)
		{
			this.text.text = this.originalText.Replace("[planet]", "the " + state.planetTitle);
			return;
		}
		this.text.text = this.originalText.Replace("[planet]", state.planetTitle);
	}

	// Token: 0x04000C16 RID: 3094
	private Text text;

	// Token: 0x04000C17 RID: 3095
	private string originalText;

	// Token: 0x04000C18 RID: 3096
	public bool removeTheFromMoon;
}
