using System;
using System.Collections.Generic;
using System.Linq;
using HHTools.Navigation;
using HyperHippo;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000208 RID: 520
public class CurrencyExchange : PanelBaseClass
{
	// Token: 0x06000F1E RID: 3870 RVA: 0x00045C6B File Offset: 0x00043E6B
	private void Start()
	{
		GameController.Instance.State.Subscribe(delegate(GameState state)
		{
			this.Init(state);
		}).AddTo(this);
	}

	// Token: 0x06000F1F RID: 3871 RVA: 0x00045C90 File Offset: 0x00043E90
	private void Init(GameState state)
	{
		this.fromCurrencyText.text = state.PlanetData.CurrencyName.ToUpper();
		GameController.Instance.PlanetThemeService.CurrencyImage.Subscribe(delegate(Sprite currency)
		{
			this.currencyIcon.sprite = currency;
		}).AddTo(base.gameObject);
		if (this.sourceCashText)
		{
			this.sourceCashTextColor = this.sourceCashText.color;
		}
		bool flag = false;
		foreach (KeyValuePair<int, string> keyValuePair in NumberFormat.illions)
		{
			if ((flag || keyValuePair.Value == this.firstUnit) && CurrencyExchange.CalculateQuantityUnitValue(this.quantities[this.quantities.Length - 1], keyValuePair.Value) < 1.7976931348623157E+308)
			{
				flag = true;
			}
		}
		this.sourceCash.Value = this.CalculateCostOfMegaBucks(this.megaCashResult);
		if (this.megaCashText)
		{
			GameController.Instance.GlobalPlayerData.GetObservable("MegaBucksBalance", 0.0).Subscribe(new Action<double>(this.UpdateMegaBucksBalanceText)).AddTo(this);
		}
		if (this.sourceCashText)
		{
			this.sourceCash.Subscribe(new Action<double>(this.UpdateSourceCurrencyText));
			this.sourceCash.Merge(new IObservable<double>[]
			{
				state.CashOnHand.Sample(TimeSpan.FromSeconds(1.0))
			}).Subscribe(delegate(double _)
			{
				this.UpdateSourceCurrencyColor(this.sourceCash.Value, state.CashOnHand.Value);
			}).AddTo(this);
		}
		this.UpdateMegaBucksResultText();
	}

	// Token: 0x06000F20 RID: 3872 RVA: 0x00045E6C File Offset: 0x0004406C
	public override void OnShowPanel()
	{
		FTUE_Manager.ShowFTUE("CurrencyExchange", null);
	}

	// Token: 0x06000F21 RID: 3873 RVA: 0x00045E7C File Offset: 0x0004407C
	public void OnClickMegaBucksResultsButton(Button button)
	{
		if (button.name.Contains("Up"))
		{
			this.megaCashResult = Util.Clamp(this.megaCashResult + 1.0, 0.0, double.MaxValue);
		}
		else if (button.name.Contains("Down"))
		{
			this.megaCashResult = Util.Clamp(this.megaCashResult - 1.0, 0.0, double.MaxValue);
		}
		this.sourceCash.Value = this.CalculateCostOfMegaBucks(this.megaCashResult);
		this.UpdateMegaBucksResultText();
		this.megaBucksButtonUp.interactable = (this.CalculateCostOfMegaBucks(this.megaCashResult + 1.0) < double.MaxValue);
		this.megaBucksButtonDown.interactable = (this.megaCashResult > 0.0);
	}

	// Token: 0x06000F22 RID: 3874 RVA: 0x00045F6E File Offset: 0x0004416E
	private void UpdateMegaBucksBalanceText(double balance)
	{
		this.megaCashText.text = balance.ToString("0");
	}

	// Token: 0x06000F23 RID: 3875 RVA: 0x00045F87 File Offset: 0x00044187
	private void UpdateMegaBucksResultText()
	{
		if (this.megaCashResultsText)
		{
			this.megaCashResultsText.text = this.megaCashResult.ToString("0");
		}
	}

	// Token: 0x06000F24 RID: 3876 RVA: 0x00045FB4 File Offset: 0x000441B4
	private void UpdateSourceCurrencyText(double value)
	{
		string text = (value >= double.MaxValue) ? "INFINITE" : NumberFormat.Convert(value, 999999999.0, true, 3).ToUpper();
		this.sourceCashText.text = text;
	}

	// Token: 0x06000F25 RID: 3877 RVA: 0x00045FF7 File Offset: 0x000441F7
	private void UpdateSourceCurrencyColor(double sourceCashValue, double cashOnHand)
	{
		if (sourceCashValue >= 1.7976931348623157E+308 || sourceCashValue > cashOnHand)
		{
			this.sourceCashText.color = Color.red;
			return;
		}
		this.sourceCashText.color = this.sourceCashTextColor;
	}

	// Token: 0x06000F26 RID: 3878 RVA: 0x0004602C File Offset: 0x0004422C
	private static double CalculateQuantityUnitValue(string sQuantity, string sUnit)
	{
		double result = 0.0;
		try
		{
			double num = double.Parse(sQuantity);
			double num2 = NumberFormat.illions.Where(delegate(KeyValuePair<int, string> kvp)
			{
				KeyValuePair<int, string> keyValuePair = kvp;
				return keyValuePair.Value == sUnit;
			}).Select(delegate(KeyValuePair<int, string> kvp)
			{
				double x = 10.0;
				KeyValuePair<int, string> keyValuePair = kvp;
				return Math.Pow(x, (double)keyValuePair.Key);
			}).FirstOrDefault<double>();
			result = num * num2;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
		}
		return result;
	}

	// Token: 0x06000F27 RID: 3879 RVA: 0x000460B8 File Offset: 0x000442B8
	private double CalculateCostOfMegaBucks(double megaBucks)
	{
		if (megaBucks == 0.0)
		{
			return 0.0;
		}
		double @double = GameController.Instance.game.planetPlayerData.GetDouble("MegaBucksCreated", 0.0);
		double megaBucksExchangeBaseCost = GameController.Instance.game.megaBucksExchangeBaseCost;
		double megaBucksExchangeRatePercent = GameController.Instance.game.megaBucksExchangeRatePercent;
		double num = 0.0;
		int num2 = 0;
		while ((double)num2 < megaBucks)
		{
			num += megaBucksExchangeBaseCost * Math.Pow(megaBucksExchangeRatePercent, @double + (double)num2);
			num2++;
		}
		return num;
	}

	// Token: 0x06000F28 RID: 3880 RVA: 0x0004614C File Offset: 0x0004434C
	public void OnClickExchangeButton()
	{
		if (this.sourceCash.Value >= 1.7976931348623157E+308)
		{
			return;
		}
		if (GameController.Instance.game.CashOnHand.Value >= this.sourceCash.Value)
		{
			GameController.Instance.game.CashOnHand.Value -= this.sourceCash.Value;
			GameController.Instance.GlobalPlayerData.Add("MegaBucksBalance", this.megaCashResult);
			GameController.Instance.game.planetPlayerData.Add("MegaBucksCreated", this.megaCashResult);
			this.megaCashResult = 1.0;
			this.sourceCash.Value = this.CalculateCostOfMegaBucks(this.megaCashResult);
			this.UpdateMegaBucksResultText();
			return;
		}
		GameController.Instance.NavigationService.CreateModal<PopupModal>(NavModals.POPUP, false).WireData("Woah there Champ, you can't afford to exchange that much money!", null, "", PopupModal.PopupOptions.OK, "Got it", "", true);
	}

	// Token: 0x04000D04 RID: 3332
	public string[] quantities;

	// Token: 0x04000D05 RID: 3333
	public string firstUnit = "million";

	// Token: 0x04000D06 RID: 3334
	public Text fromCurrencyText;

	// Token: 0x04000D07 RID: 3335
	public Text megaCashText;

	// Token: 0x04000D08 RID: 3336
	public Text megaCashResultsText;

	// Token: 0x04000D09 RID: 3337
	public Text sourceCashText;

	// Token: 0x04000D0A RID: 3338
	public Button megaBucksButtonUp;

	// Token: 0x04000D0B RID: 3339
	public Button megaBucksButtonDown;

	// Token: 0x04000D0C RID: 3340
	public Button exchangeButton;

	// Token: 0x04000D0D RID: 3341
	[SerializeField]
	private Image currencyIcon;

	// Token: 0x04000D0E RID: 3342
	private double megaCashResult = 1.0;

	// Token: 0x04000D0F RID: 3343
	private DoubleReactiveProperty sourceCash = new DoubleReactiveProperty();

	// Token: 0x04000D10 RID: 3344
	private Color sourceCashTextColor = Color.white;
}
