using System;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace BongoCat
{
	public class BaseConverter
	{
		public static string EncodeToBase59(ulong number)
		{
			string text = string.Empty;
			while (number != 0)
			{
				text = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz"[(int)(number % (ulong)"0123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".Length)] + text;
				number /= (ulong)"0123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".Length;
			}
			return text;
		}

		public static ulong DecodeFromBase59(string number)
		{
			ulong num = 0uL;
			if (string.IsNullOrEmpty(number))
			{
				return 0uL;
			}
			if (number.Any((char c) => !"0123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".Contains(c)))
			{
				Debug.LogError("BaseConverter | Invalid value in " + number);
				return 0uL;
			}
			try
			{
				for (int num2 = 0; num2 < number.Length; num2++)
				{
					num += (ulong)((long)"0123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".IndexOf(number[num2]) * (long)(ulong)BigInteger.Pow("0123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".Length, number.Length - num2 - 1));
				}
				return num;
			}
			catch (OverflowException)
			{
				Debug.LogWarning("Value overflow in BaseConverter. Can't convert to valid long number. Returning 0.");
				return 0uL;
			}
		}
	}
}
