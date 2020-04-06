using System;
using System.Collections.Generic;
using UniRx;

// Token: 0x02000252 RID: 594
public class ReactiveGrid<T>
{
	// Token: 0x0600109D RID: 4253 RVA: 0x0004BD18 File Offset: 0x00049F18
	public ReactiveGrid(int numElemsPerRow, string idPrefix)
	{
		this.numElemsPerRow = numElemsPerRow;
		this.idPrefix = idPrefix;
	}

	// Token: 0x0600109E RID: 4254 RVA: 0x0004BD4F File Offset: 0x00049F4F
	public ReactiveGridRow<T> GetRowById(string id)
	{
		return this.idToRow[id];
	}

	// Token: 0x0600109F RID: 4255 RVA: 0x0004BD5D File Offset: 0x00049F5D
	public void Clear()
	{
		this.Rows.Clear();
		this.AllElems.Clear();
		this.idToRow.Clear();
	}

	// Token: 0x060010A0 RID: 4256 RVA: 0x0004BD80 File Offset: 0x00049F80
	public string Add(T elem)
	{
		int num = (int)((float)this.AllElems.Count / (float)this.numElemsPerRow);
		bool flag = num == this.Rows.Count;
		this.AllElems.Add(elem);
		string text = this.idPrefix + num;
		ReactiveGridRow<T> reactiveGridRow;
		if (flag)
		{
			reactiveGridRow = new ReactiveGridRow<T>
			{
				Id = text
			};
		}
		else
		{
			reactiveGridRow = this.Rows[num];
		}
		reactiveGridRow.Elements.Add(elem);
		if (flag)
		{
			this.Rows.Add(reactiveGridRow);
			this.idToRow.Add(reactiveGridRow.Id, reactiveGridRow);
		}
		return text;
	}

	// Token: 0x060010A1 RID: 4257 RVA: 0x0004BE20 File Offset: 0x0004A020
	public void Remove(T elem)
	{
		int num = this.AllElems.IndexOf(elem);
		if (num == -1)
		{
			return;
		}
		int num2 = (int)((float)num / (float)this.numElemsPerRow);
		int num3 = num % this.numElemsPerRow;
		ReactiveGridRow<T> reactiveGridRow = this.Rows[num2];
		reactiveGridRow.Elements.Remove(elem);
		this.AllElems.Remove(elem);
		if (num2 == this.Rows.Count - 1 && reactiveGridRow.Elements.Count == 0)
		{
			this.Rows.Remove(reactiveGridRow);
			return;
		}
		for (int i = num; i < this.Rows.Count * this.numElemsPerRow; i++)
		{
			num2 = (int)((float)i / (float)this.numElemsPerRow);
			num3 = i % this.numElemsPerRow;
			ReactiveGridRow<T> reactiveGridRow2 = this.Rows[num2];
			if (i < this.AllElems.Count)
			{
				reactiveGridRow2.Elements[num3] = this.AllElems[i];
			}
			else if (num3 < reactiveGridRow2.Elements.Count)
			{
				for (int j = reactiveGridRow2.Elements.Count - 1; j >= num3; j--)
				{
					reactiveGridRow2.Elements.RemoveAt(j);
				}
			}
		}
		int num4 = this.Rows.Count - 1;
		while (num4 >= 0 && this.Rows[num4].Elements.Count == 0)
		{
			this.idToRow.Remove(this.Rows[num4].Id);
			this.Rows.RemoveAt(num4);
			num4--;
		}
	}

	// Token: 0x04000E3A RID: 3642
	public ReactiveCollection<ReactiveGridRow<T>> Rows = new ReactiveCollection<ReactiveGridRow<T>>();

	// Token: 0x04000E3B RID: 3643
	public List<T> AllElems = new List<T>();

	// Token: 0x04000E3C RID: 3644
	private string idPrefix;

	// Token: 0x04000E3D RID: 3645
	private int numElemsPerRow;

	// Token: 0x04000E3E RID: 3646
	private Dictionary<string, ReactiveGridRow<T>> idToRow = new Dictionary<string, ReactiveGridRow<T>>();
}
