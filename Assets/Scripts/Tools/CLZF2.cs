using System;

// Token: 0x020001A1 RID: 417
public static class CLZF2
{
	// Token: 0x06000C98 RID: 3224 RVA: 0x00038734 File Offset: 0x00036934
	public static byte[] Compress(byte[] inputBytes)
	{
		int num = inputBytes.Length * 2;
		byte[] src = new byte[num];
		int num2;
		for (num2 = CLZF2.lzf_compress(inputBytes, ref src); num2 == 0; num2 = CLZF2.lzf_compress(inputBytes, ref src))
		{
			num *= 2;
			src = new byte[num];
		}
		byte[] array = new byte[num2];
		Buffer.BlockCopy(src, 0, array, 0, num2);
		return array;
	}

	// Token: 0x06000C99 RID: 3225 RVA: 0x00038784 File Offset: 0x00036984
	public static byte[] Decompress(byte[] inputBytes)
	{
		int num = inputBytes.Length * 2;
		byte[] src = new byte[num];
		int num2;
		for (num2 = CLZF2.lzf_decompress(inputBytes, ref src); num2 == 0; num2 = CLZF2.lzf_decompress(inputBytes, ref src))
		{
			num *= 2;
			src = new byte[num];
		}
		byte[] array = new byte[num2];
		Buffer.BlockCopy(src, 0, array, 0, num2);
		return array;
	}

	// Token: 0x06000C9A RID: 3226 RVA: 0x000387D4 File Offset: 0x000369D4
	public static int lzf_compress(byte[] input, ref byte[] output)
	{
		int num = input.Length;
		int num2 = output.Length;
		Array.Clear(CLZF2.HashTable, 0, (int)CLZF2.HSIZE);
		uint num3 = 0U;
		uint num4 = 0U;
		uint num5 = (uint)((int)input[(int)num3] << 8 | (int)input[(int)(num3 + 1U)]);
		int num6 = 0;
		for (;;)
		{
			if ((ulong)num3 < (ulong)((long)(num - 2)))
			{
				num5 = (num5 << 8 | (uint)input[(int)(num3 + 2U)]);
				long num7 = (long)((ulong)((num5 ^ num5 << 5) >> (int)(24U - CLZF2.HLOG - num5 * 5U) & CLZF2.HSIZE - 1U));
				long num8 = CLZF2.HashTable[(int)(checked((IntPtr)num7))];
				CLZF2.HashTable[(int)(checked((IntPtr)num7))] = (long)((ulong)num3);
				long num9;
				if ((num9 = (long)((ulong)num3 - (ulong)num8 - 1UL)) < (long)((ulong)CLZF2.MAX_OFF) && (ulong)(num3 + 4U) < (ulong)((long)num) && num8 > 0L && input[(int)(checked((IntPtr)num8))] == input[(int)num3] && input[(int)(checked((IntPtr)(unchecked(num8 + 1L))))] == input[(int)(num3 + 1U)] && input[(int)(checked((IntPtr)(unchecked(num8 + 2L))))] == input[(int)(num3 + 2U)])
				{
					uint num10 = 2U;
					uint num11 = (uint)(num - (int)num3 - (int)num10);
					num11 = ((num11 > CLZF2.MAX_REF) ? CLZF2.MAX_REF : num11);
					if ((ulong)num4 + (ulong)((long)num6) + 1UL + 3UL >= (ulong)((long)num2))
					{
						break;
					}
					do
					{
						num10 += 1U;
					}
					while (num10 < num11 && input[(int)(checked((IntPtr)(unchecked(num8 + (long)((ulong)num10)))))] == input[(int)(num3 + num10)]);
					if (num6 != 0)
					{
						output[(int)num4++] = (byte)(num6 - 1);
						num6 = -num6;
						do
						{
							output[(int)num4++] = input[(int)(checked((IntPtr)(unchecked((ulong)num3 + (ulong)((long)num6)))))];
						}
						while (++num6 != 0);
					}
					num10 -= 2U;
					num3 += 1U;
					if (num10 < 7U)
					{
						output[(int)num4++] = (byte)((num9 >> 8) + (long)((ulong)((ulong)num10 << 5)));
					}
					else
					{
						output[(int)num4++] = (byte)((num9 >> 8) + 224L);
						output[(int)num4++] = (byte)(num10 - 7U);
					}
					output[(int)num4++] = (byte)num9;
					num3 += num10 - 1U;
					num5 = (uint)((int)input[(int)num3] << 8 | (int)input[(int)(num3 + 1U)]);
					num5 = (num5 << 8 | (uint)input[(int)(num3 + 2U)]);
					CLZF2.HashTable[(int)((num5 ^ num5 << 5) >> (int)(24U - CLZF2.HLOG - num5 * 5U) & CLZF2.HSIZE - 1U)] = (long)((ulong)num3);
					num3 += 1U;
					num5 = (num5 << 8 | (uint)input[(int)(num3 + 2U)]);
					CLZF2.HashTable[(int)((num5 ^ num5 << 5) >> (int)(24U - CLZF2.HLOG - num5 * 5U) & CLZF2.HSIZE - 1U)] = (long)((ulong)num3);
					num3 += 1U;
					continue;
				}
			}
			else if ((ulong)num3 == (ulong)((long)num))
			{
				goto IL_2A1;
			}
			num6++;
			num3 += 1U;
			if ((long)num6 == (long)((ulong)CLZF2.MAX_LIT))
			{
				if ((ulong)(num4 + 1U + CLZF2.MAX_LIT) >= (ulong)((long)num2))
				{
					return 0;
				}
				output[(int)num4++] = (byte)(CLZF2.MAX_LIT - 1U);
				num6 = -num6;
				do
				{
					output[(int)num4++] = input[(int)(checked((IntPtr)(unchecked((ulong)num3 + (ulong)((long)num6)))))];
				}
				while (++num6 != 0);
			}
		}
		return 0;
		IL_2A1:
		if (num6 != 0)
		{
			if ((ulong)num4 + (ulong)((long)num6) + 1UL >= (ulong)((long)num2))
			{
				return 0;
			}
			output[(int)num4++] = (byte)(num6 - 1);
			num6 = -num6;
			do
			{
				output[(int)num4++] = input[(int)(checked((IntPtr)(unchecked((ulong)num3 + (ulong)((long)num6)))))];
			}
			while (++num6 != 0);
		}
		return (int)num4;
	}

	// Token: 0x06000C9B RID: 3227 RVA: 0x00038AC8 File Offset: 0x00036CC8
	public static int lzf_decompress(byte[] input, ref byte[] output)
	{
		int num = input.Length;
		int num2 = output.Length;
		uint num3 = 0U;
		uint num4 = 0U;
		for (;;)
		{
			uint num5 = (uint)input[(int)num3++];
			if (num5 < 32U)
			{
				num5 += 1U;
				if ((ulong)(num4 + num5) > (ulong)((long)num2))
				{
					break;
				}
				do
				{
					output[(int)num4++] = input[(int)num3++];
				}
				while ((num5 -= 1U) != 0U);
			}
			else
			{
				uint num6 = num5 >> 5;
				int num7 = (int)(num4 - ((num5 & 31U) << 8) - 1U);
				if (num6 == 7U)
				{
					num6 += (uint)input[(int)num3++];
				}
				num7 -= (int)input[(int)num3++];
				if ((ulong)(num4 + num6 + 2U) > (ulong)((long)num2))
				{
					return 0;
				}
				if (num7 < 0)
				{
					return 0;
				}
				output[(int)num4++] = output[num7++];
				output[(int)num4++] = output[num7++];
				do
				{
					output[(int)num4++] = output[num7++];
				}
				while ((num6 -= 1U) != 0U);
			}
			if ((ulong)num3 >= (ulong)((long)num))
			{
				return (int)num4;
			}
		}
		return 0;
	}

	// Token: 0x04000AB1 RID: 2737
	private static readonly uint HLOG = 14U;

	// Token: 0x04000AB2 RID: 2738
	private static readonly uint HSIZE = 16384U;

	// Token: 0x04000AB3 RID: 2739
	private static readonly uint MAX_LIT = 32U;

	// Token: 0x04000AB4 RID: 2740
	private static readonly uint MAX_OFF = 8192U;

	// Token: 0x04000AB5 RID: 2741
	private static readonly uint MAX_REF = 264U;

	// Token: 0x04000AB6 RID: 2742
	private static readonly long[] HashTable = new long[CLZF2.HSIZE];
}
