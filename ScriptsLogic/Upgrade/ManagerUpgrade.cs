using System;
using LitJson;

// Token: 0x020000B0 RID: 176
public abstract class ManagerUpgrade : Upgrade
{
	// Token: 0x17000073 RID: 115
	// (get) Token: 0x060004BF RID: 1215
	[JsonIgnore(JsonIgnoreWhen.Serializing | JsonIgnoreWhen.Deserializing)]
	public abstract override string Effect { get; }

	// Token: 0x060004C0 RID: 1216
	public abstract override void Apply(GameState state);

	// Token: 0x060004C1 RID: 1217
	public abstract override void Copy(object other);
}
