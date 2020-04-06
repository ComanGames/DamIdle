using System;
using UnityEngine;

// Token: 0x0200014F RID: 335
public class GameControllerView : MonoBehaviour
{
	// Token: 0x06000A7D RID: 2685 RVA: 0x0002E933 File Offset: 0x0002CB33
	private void Awake()
	{
		GameController.Instance.Setup();
	}

	// Token: 0x040008A7 RID: 2215
	private GameController _GameController;
}
