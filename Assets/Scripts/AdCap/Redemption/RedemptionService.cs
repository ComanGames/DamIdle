using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdCap.Store;
using HHTools.MiniJSON;
using Platforms;
using Platforms.Logger;
using PlayFab.ClientModels;
using UniRx;
using UnityEngine;

namespace AdCap.Redemption
{
	// Token: 0x020006FF RID: 1791
	public class RedemptionService : IRedemptionService
	{
		// Token: 0x17000313 RID: 787
		// (get) Token: 0x060024FE RID: 9470 RVA: 0x000A0875 File Offset: 0x0009EA75
		// (set) Token: 0x060024FF RID: 9471 RVA: 0x000A087D File Offset: 0x0009EA7D
		public Subject<RedemptionEvent> RedemptionEventCallback { get; set; }

		// Token: 0x06002500 RID: 9472 RVA: 0x000A0886 File Offset: 0x0009EA86
		public RedemptionService()
		{
			this.RedemptionEventCallback = new Subject<RedemptionEvent>();
		}

		// Token: 0x06002501 RID: 9473 RVA: 0x000A08B0 File Offset: 0x0009EAB0
		public void SubmitCode(string code, Action<string> onSuccess, Action<string> onFail)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>
			{
				{
					"Content-Type",
					"application/json"
				}
			};
			string s = Json.Serialize(new Dictionary<string, string>
			{
				{
					"code",
					code
				}
			});
			ObservableWeb.Post("https://api.hyperhippo.ca/adcap/v1/redeem", Encoding.UTF8.GetBytes(s), headers, null).CatchIgnore(delegate(WWWErrorException ex)
			{
				onFail("Your code could not be redeemed.");
			}).CatchIgnore(delegate(UnityWebRequestErrorException ex)
			{
				onFail("Your code could not be redeemed.");
			}).Subscribe(onSuccess, delegate(Exception error)
			{
				onFail("Your code could not be redeemed.");
			});
		}

		// Token: 0x06002502 RID: 9474 RVA: 0x000A0944 File Offset: 0x0009EB44
		public void Init()
		{
			this.logger = Platforms.Logger.Logger.GetLogger(this);
			this.logger.Info("Initializing....");
			GameController.Instance.OnLoadNewPlanetPre += this.PreLoadPlanet;
			MessageBroker.Default.Receive<WelcomeBackSequenceCompleted>().Subscribe(delegate(WelcomeBackSequenceCompleted _)
			{
				this.OnWelcomeBackSequenceCompleted();
			}).AddTo(this.disposables);
			this.CheckForRedemption();
			this.logger.Info("Initialized");
		}

		// Token: 0x06002503 RID: 9475 RVA: 0x000A09C0 File Offset: 0x0009EBC0
		public void Dispose()
		{
			this.disposables.Dispose();
		}

		// Token: 0x06002504 RID: 9476 RVA: 0x000A09CD File Offset: 0x0009EBCD
		public void CheckForRedemption()
		{
			Helper.GetPlatformAccount().PlayFab.GetUserData(delegate(GetUserDataResult x)
			{
				if (x.Data != null && x.Data.ContainsKey("redemption") && !string.IsNullOrEmpty(x.Data["redemption"].Value))
				{
					x.Data["redemption"].Value.Split(new char[]
					{
						','
					}).ToList<string>().ForEach(delegate(string d)
					{
						this.codesToRedeem.Push(d);
					});
					this.TriggerRedeemSequenceIfNeeded();
				}
			}, null);
		}

		// Token: 0x06002505 RID: 9477 RVA: 0x000A09EB File Offset: 0x0009EBEB
		public void RedeemCode(string code)
		{
			this.codesToRedeem.Push(code);
			this.TriggerRedeemSequenceIfNeeded();
		}

		// Token: 0x06002506 RID: 9478 RVA: 0x000A09FF File Offset: 0x0009EBFF
		private void PreLoadPlanet()
		{
			this.hasWelcomeScreenBeenSeen = false;
		}

		// Token: 0x06002507 RID: 9479 RVA: 0x000A0A08 File Offset: 0x0009EC08
		private void OnWelcomeBackSequenceCompleted()
		{
			this.hasWelcomeScreenBeenSeen = true;
			this.TriggerRedeemSequenceIfNeeded();
		}

		// Token: 0x06002508 RID: 9480 RVA: 0x000A0A17 File Offset: 0x0009EC17
		private void TriggerRedeemSequenceIfNeeded()
		{
			if (this.codesToRedeem.Count > 0 && this.hasWelcomeScreenBeenSeen)
			{
				this.SubmitRedemptionCode(this.codesToRedeem.Peek());
			}
		}

		// Token: 0x06002509 RID: 9481 RVA: 0x000A0A40 File Offset: 0x0009EC40
		private void SubmitRedemptionCode(string code)
		{
			RedemptionEvent value = default(RedemptionEvent);
			if (string.IsNullOrEmpty(code) || code.Length < 4 || code.Length > 50)
			{
				value.Type = RedemptionEventType.RedemptionCompleted;
				value.ErrorMessage = "Your code could not be redeemed.";
				value.Success = false;
			}
			else
			{
				value.Type = RedemptionEventType.RedemptionStarted;
				value.Success = true;
			}
			this.RedemptionEventCallback.OnNext(value);
			this.SubmitCode(code, new Action<string>(this.OnCodeSubmitResponseSuccess), new Action<string>(this.RedemptionError));
		}

		// Token: 0x0600250A RID: 9482 RVA: 0x000A0ACC File Offset: 0x0009ECCC
		private void OnCodeSubmitResponseSuccess(string response)
		{
			Dictionary<string, object> dictionary = Json.Deserialize(response) as Dictionary<string, object>;
			if (dictionary == null || !dictionary.ContainsKey("product_id"))
			{
				RedemptionEvent value = new RedemptionEvent
				{
					Type = RedemptionEventType.RedemptionCompleted,
					ErrorMessage = "Your code could not be redeemed. No product returned",
					Success = false
				};
				this.RedemptionEventCallback.OnNext(value);
				return;
			}
			try
			{
				string text = dictionary["product_id"] as string;
				if (string.IsNullOrEmpty(text))
				{
					throw new Exception(string.Format("Invalid product [{0}]", text ?? "null"));
				}
				string[] array = text.Split(new char[]
				{
					'.'
				});
				if (array.Length != 2)
				{
					throw new Exception(string.Format("Invalid product [{0}]", text));
				}
				string text2 = array[0];
				string text3 = array[1];
				if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text3))
				{
					throw new Exception("Invalid params key=" + (text2 ?? "null") + " val=" + (text3 ?? "null"));
				}
				this.ProcessProduct(text2, text3);
				this.PopCode();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				this.PopCode();
				RedemptionEvent value2 = new RedemptionEvent
				{
					Type = RedemptionEventType.RedemptionCompleted,
					ErrorMessage = "Your code could not be redeemed. Exception = " + ex.Message,
					Success = false
				};
				this.RedemptionEventCallback.OnNext(value2);
			}
		}

		// Token: 0x0600250B RID: 9483 RVA: 0x000A0C40 File Offset: 0x0009EE40
		private void RedemptionError(string error)
		{
			this.PopCode();
			this.logger.Error("RedemptionError: " + error);
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				ErrorMessage = error,
				Success = false
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x0600250C RID: 9484 RVA: 0x000A0C98 File Offset: 0x0009EE98
		private void PopCode()
		{
			if (this.codesToRedeem.Count == 0)
			{
				return;
			}
			this.codesToRedeem.Pop();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (this.codesToRedeem.Count > 0)
			{
				this.TriggerRedeemSequenceIfNeeded();
				dictionary.Add("redemption", string.Join(",", this.codesToRedeem.ToArray()));
			}
			else
			{
				dictionary.Add("redemption", null);
			}
			Helper.GetPlatformAccount().PlayFab.UpdateUserData(dictionary, delegate(UpdateUserDataResult _)
			{
			}, null);
		}

		// Token: 0x0600250D RID: 9485 RVA: 0x000A0D38 File Offset: 0x0009EF38
		private void ProcessProduct(string key, string val)
		{
			string text = key.ToLower();
			uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
			if (num <= 2020089236U)
			{
				if (num <= 957692078U)
				{
					if (num != 895779448U)
					{
						if (num == 957692078U)
						{
							if (text == "itemization")
							{
								this.GrantItemization(val);
								return;
							}
						}
					}
					else if (text == "badge")
					{
						if (val.ToLower() == "allbadges")
						{
							this.GrantAllBadges();
							return;
						}
						Debug.LogError("Granting specific badge");
						this.GrantBadge(val.Replace("_", " "));
						return;
					}
				}
				else if (num != 1705436065U)
				{
					if (num != 1934827483U)
					{
						if (num == 2020089236U)
						{
							if (text == "itemization2")
							{
								this.GrantItemization(val);
								return;
							}
						}
					}
					else if (text == "angels")
					{
						this.GrantAngels(double.Parse(val));
						return;
					}
				}
				else if (text == "hacks")
				{
					this.GrantHacks();
					return;
				}
			}
			else if (num <= 2671260646U)
			{
				if (num != 2326275489U)
				{
					if (num == 2671260646U)
					{
						if (text == "item")
						{
							this.GrantItem(val);
							return;
						}
					}
				}
				else if (text == "megabucks")
				{
					this.GrantMegabucks(double.Parse(val));
					return;
				}
			}
			else if (num != 2951325068U)
			{
				if (num != 3103933528U)
				{
					if (num == 3966162835U)
					{
						if (text == "gold")
						{
							this.GrantGold(int.Parse(val));
							return;
						}
					}
				}
				else if (text == "achievement")
				{
					this.GrantUnlock(val.Replace("_", " "));
					return;
				}
			}
			else if (text == "flux_capitalor")
			{
				this.GrantFluxCapitalor(double.Parse(val));
				return;
			}
			this.RedemptionError("Your code could not be redeemed.");
		}

		// Token: 0x0600250E RID: 9486 RVA: 0x000A0F60 File Offset: 0x0009F160
		private void GrantItem(string itemConfig)
		{
			string[] array = itemConfig.Split(new string[]
			{
				"---"
			}, StringSplitOptions.None);
			string id = array[0];
			int num = 1;
			if (array.Length > 1)
			{
				int.TryParse(array[1].ToString(), out num);
			}
			if (GameController.Instance.GlobalPlayerData.inventory.GetItemById(id) != null)
			{
				GameController.Instance.GlobalPlayerData.inventory.AddItem(id, num, true, true);
				RedemptionItem item = new RedemptionItem
				{
					id = id,
					Qty = (double)num,
					Product = Product.InventoryItem
				};
				RedemptionEvent value = new RedemptionEvent
				{
					Type = RedemptionEventType.RedemptionCompleted,
					Item = item,
					Success = true
				};
				this.RedemptionEventCallback.OnNext(value);
				return;
			}
			this.RedemptionError("Your code could not be redeemed.");
		}

		// Token: 0x0600250F RID: 9487 RVA: 0x000A1028 File Offset: 0x0009F228
		private void GrantAngels(double amount)
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.Angels,
				Qty = amount
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x06002510 RID: 9488 RVA: 0x000A1078 File Offset: 0x0009F278
		private void GrantGold(int amount)
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.Gold,
				Qty = (double)amount
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x06002511 RID: 9489 RVA: 0x000A10CC File Offset: 0x0009F2CC
		private void GrantUnlock(string name)
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.Unlock,
				Qty = 1.0,
				id = name
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x06002512 RID: 9490 RVA: 0x000A112C File Offset: 0x0009F32C
		private void GrantMegabucks(double amount)
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.Megabucks,
				Qty = amount
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x06002513 RID: 9491 RVA: 0x000A117C File Offset: 0x0009F37C
		private void GrantFluxCapitalor(double amount)
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.FluxCapitalor,
				Qty = amount
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x06002514 RID: 9492 RVA: 0x000A11CC File Offset: 0x0009F3CC
		private void GrantBadge(string badgeName)
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.Badge,
				Qty = 1.0,
				id = badgeName
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x06002515 RID: 9493 RVA: 0x000A122C File Offset: 0x0009F42C
		private void GrantItemization(string id)
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.Itemization,
				Qty = 1.0,
				id = id
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x06002516 RID: 9494 RVA: 0x000A128C File Offset: 0x0009F48C
		private void GrantItemization2(string id)
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.Itemization2,
				Qty = 1.0,
				id = id
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x06002517 RID: 9495 RVA: 0x000A12EC File Offset: 0x0009F4EC
		private void GrantHacks()
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.Hack,
				Qty = 1.0
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x06002518 RID: 9496 RVA: 0x000A1344 File Offset: 0x0009F544
		private void GrantAllBadges()
		{
			RedemptionItem item = new RedemptionItem
			{
				Product = Product.AllBadges,
				Qty = 1.0
			};
			RedemptionEvent value = new RedemptionEvent
			{
				Type = RedemptionEventType.RedemptionCompleted,
				Item = item,
				Success = true
			};
			this.RedemptionEventCallback.OnNext(value);
		}

		// Token: 0x04002607 RID: 9735
		private const string PLAYFAB_CS_AUTO_REDEMPTION_KEY = "redemption";

		// Token: 0x04002608 RID: 9736
		private CompositeDisposable disposables = new CompositeDisposable();

		// Token: 0x04002609 RID: 9737
		private Stack<string> codesToRedeem = new Stack<string>();

		// Token: 0x0400260A RID: 9738
		private bool hasWelcomeScreenBeenSeen;

		// Token: 0x0400260B RID: 9739
		private const string _URL = "https://api.hyperhippo.ca/adcap/v1/redeem";

		// Token: 0x0400260C RID: 9740
		private Platforms.Logger.Logger logger;
	}
}
