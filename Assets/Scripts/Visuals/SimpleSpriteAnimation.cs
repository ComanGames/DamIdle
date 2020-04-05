using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000206 RID: 518
public class SimpleSpriteAnimation : MonoBehaviour
{
	// Token: 0x06000F0F RID: 3855 RVA: 0x00045A01 File Offset: 0x00043C01
	public void Start()
	{
		if (this.PlayOnEnable && this.Sprites.Count > 0)
		{
			this.Init(this.Sprites);
		}
	}

	// Token: 0x06000F10 RID: 3856 RVA: 0x00045A28 File Offset: 0x00043C28
	public void Init(List<Sprite> spriteList)
	{
		this.animIndex = 0;
		this.Sprites = spriteList;
		if (this.Sprites.Count != 0)
		{
			this.Img_Image.sprite = this.Sprites[this.animIndex];
		}
		if (this.PlayOnEnable)
		{
			this.Play();
		}
	}

	// Token: 0x06000F11 RID: 3857 RVA: 0x00045A7A File Offset: 0x00043C7A
	public void Play()
	{
		if (base.isActiveAndEnabled)
		{
			this.animIndex = 0;
			base.StartCoroutine(this.CycleSprites());
		}
	}

	// Token: 0x06000F12 RID: 3858 RVA: 0x00045A98 File Offset: 0x00043C98
	public void PlayLoop()
	{
		this.IsLoop = true;
		this.Play();
	}

	// Token: 0x06000F13 RID: 3859 RVA: 0x00045AA7 File Offset: 0x00043CA7
	public void Stop()
	{
		this.isPlaying = false;
		base.StopAllCoroutines();
	}

	// Token: 0x06000F14 RID: 3860 RVA: 0x00045AB6 File Offset: 0x00043CB6
	private IEnumerator CycleSprites()
	{
		this.isPlaying = true;
		float time = this.animTime;
		while (this.isPlaying)
		{
			time -= Time.deltaTime;
			if (time <= 0f)
			{
				time = this.animTime;
				Image img_Image = this.Img_Image;
				List<Sprite> sprites = this.Sprites;
				int num = this.animIndex + 1;
				this.animIndex = num;
				img_Image.sprite = sprites[num % this.Sprites.Count];
			}
			yield return null;
			if (!this.IsLoop && this.animIndex >= this.Sprites.Count)
			{
				this.isPlaying = false;
			}
		}
		if (this.Sprites.Count > 0)
		{
			this.Img_Image.sprite = this.Sprites[0];
		}
		yield break;
	}

	// Token: 0x06000F15 RID: 3861 RVA: 0x00045AC5 File Offset: 0x00043CC5
	private void OnDestroy()
	{
		base.StopAllCoroutines();
	}

	// Token: 0x04000CF8 RID: 3320
	public Image Img_Image;

	// Token: 0x04000CF9 RID: 3321
	public bool IsLoop;

	// Token: 0x04000CFA RID: 3322
	public bool PlayOnEnable;

	// Token: 0x04000CFB RID: 3323
	public List<Sprite> Sprites = new List<Sprite>();

	// Token: 0x04000CFC RID: 3324
	private float animTime = 0.075f;

	// Token: 0x04000CFD RID: 3325
	private int animIndex;

	// Token: 0x04000CFE RID: 3326
	private bool isPlaying;
}
