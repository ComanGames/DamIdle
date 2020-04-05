using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// Token: 0x02000211 RID: 529
public class SwagShare : MonoBehaviour
{
	// Token: 0x06000F5E RID: 3934 RVA: 0x0004736E File Offset: 0x0004556E
	public void CaptureAndShareScreenshot()
	{
		this.SetObstructionsState(false);
		base.StartCoroutine(this._CaptureScreenshot());
	}

	// Token: 0x06000F5F RID: 3935 RVA: 0x00047384 File Offset: 0x00045584
	private void SetObstructionsState(bool enabled)
	{
		foreach (GameObject gameObject in this.obstructionList)
		{
			gameObject.SetActive(enabled);
		}
	}

	// Token: 0x06000F60 RID: 3936 RVA: 0x000473D8 File Offset: 0x000455D8
	private IEnumerator _CaptureScreenshot()
	{
		Vector3[] array = new Vector3[4];
		this.targetRect.GetWorldCorners(array);
		Vector3[] screenCorners = new Vector3[4];
		Camera camera = Camera.allCameras.FirstOrDefault((Camera c) => c.gameObject.name == "NavigationCamera");
		if (null == camera)
		{
			yield break;
		}
		screenCorners[0] = camera.WorldToScreenPoint(array[0]);
		screenCorners[1] = camera.WorldToScreenPoint(array[1]);
		screenCorners[2] = camera.WorldToScreenPoint(array[2]);
		screenCorners[3] = camera.WorldToScreenPoint(array[3]);
		yield return new WaitForEndOfFrame();
		int num = Mathf.FloorToInt(screenCorners[0].x);
		int num2 = Mathf.FloorToInt(screenCorners[0].y);
		int num3 = Mathf.FloorToInt(screenCorners[3].x - screenCorners[0].x);
		int num4 = Mathf.FloorToInt(screenCorners[1].y - screenCorners[0].y);
		Texture2D texture2D = new Texture2D(num3, num4);
		texture2D.ReadPixels(new Rect((float)num, (float)num2, (float)num3, (float)num4), 0, 0);
		texture2D.Apply();
		this.SetObstructionsState(true);
		this.ShareScreenshot(texture2D);
		yield break;
	}

	// Token: 0x06000F61 RID: 3937 RVA: 0x000473E8 File Offset: 0x000455E8
	private void ShareScreenshot(Texture2D screenshot)
	{
		string playAdCapURL = GameController.Instance.DataService.ExternalData.GeneralConfig[0].PlayAdCapURL;
		this.ShareViaGeneric("AdVenture Capitalist Swag", playAdCapURL, screenshot);
	}

	// Token: 0x06000F62 RID: 3938 RVA: 0x00002718 File Offset: 0x00000918
	private void ShareViaGeneric(string subject, string body, Texture2D screenshot)
	{
	}

	// Token: 0x06000F63 RID: 3939 RVA: 0x00047424 File Offset: 0x00045624
	private void AndroidIntentShare(string subject, string body, Texture2D screenshot, string packageContext = null, string className = null)
	{
		string text = Path.Combine(Application.persistentDataPath, string.Format("my_swag_{0}.png", DateTime.Now.ToString("yyyyMMddHHmmss")));
		File.WriteAllBytes(text, screenshot.EncodeToPNG());
	}

	// Token: 0x04000D40 RID: 3392
	[SerializeField]
	private RectTransform targetRect;

	// Token: 0x04000D41 RID: 3393
	public List<GameObject> obstructionList;
}
