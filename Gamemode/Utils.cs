﻿using System;
using System.Collections.Generic;

namespace FPSMO
{
	internal static class Utils
	{
		internal static int Clamp(int value, int min, int max)
		{
            return (value < min) ? min : (value > max) ? max : value;
        }

		internal static List<int> RandomSubset(int setCount, int subsetCount)
		{
			if (setCount < 0) throw new ArgumentException("Count must be positive");

            // This hat-based algorithm is called Fisher–Yates' shuffle
			var hat = new List<int>();

			for (int i = 0; i < setCount; i++)
				hat.Add(i);

			var result = new List<int>();
			var random = new Random();

			for (int i = 0; i < subsetCount; i++)
			{
				int j = random.Next(hat.Count);
				result.Add(hat[j]);
				hat.RemoveAt(j);
			}

			return result;
		}

		internal static string InsertSpaceBetweenCharacters(string text)
		{
			if (text.Length == 0) return text;

			char[] characters = text.ToCharArray();
			char[] result = new char[characters.Length * 2 - 1];
			bool lastCharacter;

			for (int i = 0; i < characters.Length; i++)
			{
				result[2 * i] = characters[i];
				lastCharacter = (i == characters.Length - 1);

				if (!lastCharacter) result[2 * i + 1] = ' ';
			}

			return new string(result);
		}
	}
}