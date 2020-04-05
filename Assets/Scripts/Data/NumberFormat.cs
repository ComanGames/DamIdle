using System;
using System.Collections.Generic;

// Token: 0x020001E5 RID: 485
public static class NumberFormat
{
	// Token: 0x06000E2A RID: 3626 RVA: 0x0003F5C0 File Offset: 0x0003D7C0
	public static string Convert(double unformatted, double largestUnformatted = 1E+15, bool trailingZeros = true, int numTrailingZeros = 3)
	{
		if (unformatted >= largestUnformatted)
		{
			return NumberFormat.GetConvertedString(Math.Round(unformatted).ToString("F0"), trailingZeros, numTrailingZeros, false);
		}
		if (trailingZeros)
		{
			return unformatted.ToString("N2");
		}
		return unformatted.ToString("N0");
	}

	// Token: 0x06000E2B RID: 3627 RVA: 0x0003F60C File Offset: 0x0003D80C
	public static string ConvertNormal(double unformatted, double largestUnformatted = 1E+15, int numTrailingZeros = 0)
	{
		if (unformatted < largestUnformatted)
		{
			return unformatted.ToString("N0");
		}
		return NumberFormat.GetConvertedString(Math.Round(unformatted).ToString("F0"), true, numTrailingZeros, false);
	}

	// Token: 0x06000E2C RID: 3628 RVA: 0x0003F648 File Offset: 0x0003D848
	private static string GetConvertedString(string raw, bool trailingZeros = true, int trailingZeroNum = 3, bool science = false)
	{
		string text = "";
		int num = 0;
		int startIndex = 0;
		string postFix = NumberFormat.GetPostFix(raw, science, out startIndex, out num);
		int num2 = num;
		if (trailingZeros && trailingZeroNum > 0)
		{
			num2 = num - trailingZeroNum;
			if (trailingZeroNum > 0)
			{
				text = raw.Insert(startIndex, ".");
			}
		}
		else
		{
			text = raw;
		}
		try
		{
			text = text.Remove(text.Length - num2);
			if (text.Contains("."))
			{
				text = text.TrimEnd(new char[]
				{
					'0'
				});
				text = text.TrimEnd(new char[]
				{
					'.'
				});
			}
			text = text + " " + postFix;
		}
		catch (ArgumentOutOfRangeException)
		{
			text = "In.fin";
		}
		return text;
	}

	// Token: 0x06000E2D RID: 3629 RVA: 0x0003F6FC File Offset: 0x0003D8FC
	public static string ConvertFractionalExpression(double current, double target, double largestUnformatted = 1000000.0, bool trailingZeros = true, int numTrailingZeros = 3)
	{
		string text = current.ToString("N0");
		string str = target.ToString("N0");
		if (target > largestUnformatted)
		{
			if (current != 0.0)
			{
				double num = Math.Floor(Math.Log10(current) / 3.0) * 3.0;
				if (num <= 305.0)
				{
					System.Convert.ToInt32(num);
				}
				double value = Math.Floor(Math.Log10(target) / 3.0) * 3.0;
				int num2 = (num > 305.0) ? 306 : System.Convert.ToInt32(value);
				double num3 = current / Math.Pow(10.0, (double)(num2 - 3)) / 1000.0;
				if (num3 > target)
				{
					num3 = target;
				}
				text = num3.ToString("F3");
				if (text.Contains("."))
				{
					text = text.TrimEnd(new char[]
					{
						'0'
					});
					text = text.TrimEnd(new char[]
					{
						'.'
					});
				}
			}
			str = NumberFormat.GetConvertedString(Math.Round(target).ToString("F0"), trailingZeros, numTrailingZeros, false);
		}
		return text + "/" + str;
	}

	// Token: 0x06000E2E RID: 3630 RVA: 0x0002971B File Offset: 0x0002791B
	private static string GetConvertedNormalString(string raw)
	{
		return "";
	}

	// Token: 0x06000E2F RID: 3631 RVA: 0x0003F83C File Offset: 0x0003DA3C
	public static string GetPostFix(string raw, bool science, out int preDecimal, out int postDecimal)
	{
		int num = raw.Length % 3;
		preDecimal = ((num == 0) ? 3 : num);
		postDecimal = raw.Length - preDecimal;
		string result;
		if (science)
		{
			result = "e" + postDecimal;
		}
		else if (postDecimal <= 306)
		{
			result = NumberFormat.illions[postDecimal];
		}
		else
		{
			result = "TOO MUCH";
		}
		return result;
	}

	// Token: 0x04000C0B RID: 3083
	public static Dictionary<int, string> illions = new Dictionary<int, string>
	{
		{
			0,
			"hundred"
		},
		{
			3,
			"thousand"
		},
		{
			6,
			"million"
		},
		{
			9,
			"billion"
		},
		{
			12,
			"trillion"
		},
		{
			15,
			"quadrillion"
		},
		{
			18,
			"quintillion"
		},
		{
			21,
			"sextillion"
		},
		{
			24,
			"septillion"
		},
		{
			27,
			"octillion"
		},
		{
			30,
			"nonillion"
		},
		{
			33,
			"decillion"
		},
		{
			36,
			"undecillion"
		},
		{
			39,
			"duodecillion"
		},
		{
			42,
			"tredecillion"
		},
		{
			45,
			"quattuordecillion"
		},
		{
			48,
			"quindecillion"
		},
		{
			51,
			"sexdecillion"
		},
		{
			54,
			"septendecillion"
		},
		{
			57,
			"octodecillion"
		},
		{
			60,
			"novemdecillion"
		},
		{
			63,
			"vigintillion"
		},
		{
			66,
			"unvigintillion"
		},
		{
			69,
			"duovigintillion"
		},
		{
			72,
			"tresvigintillion"
		},
		{
			75,
			"quattuorvigintillion"
		},
		{
			78,
			"quinvigintillion"
		},
		{
			81,
			"sexvigintillion"
		},
		{
			84,
			"septenvigintillion"
		},
		{
			87,
			"octovigintillion"
		},
		{
			90,
			"novemvigintillion"
		},
		{
			93,
			"trigintillion"
		},
		{
			96,
			"untrigintillion"
		},
		{
			99,
			"duotrigintillion"
		},
		{
			102,
			"tretrigintillion"
		},
		{
			105,
			"quattuortrigintillion"
		},
		{
			108,
			"quintrigintillion"
		},
		{
			111,
			"sextrigintillion"
		},
		{
			114,
			"septentrigintillion"
		},
		{
			117,
			"octotrigintillion"
		},
		{
			120,
			"novemtrigintillion"
		},
		{
			123,
			"quadragintillion"
		},
		{
			126,
			"unquadragintillion"
		},
		{
			129,
			"duoquadragintillion"
		},
		{
			132,
			"trequadragintillion"
		},
		{
			135,
			"quattuorquadragintillion"
		},
		{
			138,
			"quinquadragintillion"
		},
		{
			141,
			"sexquadragintillion"
		},
		{
			144,
			"septquadragintillion"
		},
		{
			147,
			"octoquadragintillion"
		},
		{
			150,
			"novemquadragintillion"
		},
		{
			153,
			"quinquagintillion"
		},
		{
			156,
			"unquinquagintillion"
		},
		{
			159,
			"duoquinquagintillion"
		},
		{
			162,
			"trequinquagintillion"
		},
		{
			165,
			"quattuorquinquagintillion"
		},
		{
			168,
			"quinquinquagintillion"
		},
		{
			171,
			"sexquinquagintillion"
		},
		{
			174,
			"septquinquagintillion"
		},
		{
			177,
			"octoquinquagintillion"
		},
		{
			180,
			"novemquinquagintillion"
		},
		{
			183,
			"sexagintillion"
		},
		{
			186,
			"unsexagintillion"
		},
		{
			189,
			"duosexagintillion"
		},
		{
			192,
			"tresexagintillion"
		},
		{
			195,
			"quattuorsexagintillion"
		},
		{
			198,
			"quinsexagintillion"
		},
		{
			201,
			"sexsexagintillion"
		},
		{
			204,
			"septsexagintillion"
		},
		{
			207,
			"octosexagintillion"
		},
		{
			210,
			"novemsexagintillion"
		},
		{
			213,
			"septuagintillion"
		},
		{
			216,
			"unseptuagintillion"
		},
		{
			219,
			"duoseptuagintillion"
		},
		{
			222,
			"treseptuagintillion"
		},
		{
			225,
			"quattuorseptuagintillion"
		},
		{
			228,
			"quinseptuagintillion"
		},
		{
			231,
			"sexseptuagintillion"
		},
		{
			234,
			"septseptuagintillion"
		},
		{
			237,
			"octoseptuagintillion"
		},
		{
			240,
			"novemseptuagintillion"
		},
		{
			243,
			"octogintillion"
		},
		{
			246,
			"unoctogintillion"
		},
		{
			249,
			"duooctogintillion"
		},
		{
			252,
			"treoctogintillion"
		},
		{
			255,
			"quattuoroctogintillion"
		},
		{
			258,
			"quinoctogintillion"
		},
		{
			261,
			"sexoctogintillion"
		},
		{
			264,
			"septoctogintillion"
		},
		{
			267,
			"octooctogintillion"
		},
		{
			270,
			"novemoctogintillion"
		},
		{
			273,
			"nonagintillion"
		},
		{
			276,
			"unnonagintillion"
		},
		{
			279,
			"duononagintillion"
		},
		{
			282,
			"trenonagintillion"
		},
		{
			285,
			"quattuornonagintillion"
		},
		{
			288,
			"quinnonagintillion"
		},
		{
			291,
			"sexnonagintillion"
		},
		{
			294,
			"septnonagintillion"
		},
		{
			297,
			"octononagintillion"
		},
		{
			300,
			"novemnonagintillion"
		},
		{
			303,
			"centillion"
		},
		{
			306,
			"uncentillion"
		}
	};
}
