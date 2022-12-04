using System;
namespace FPS
{
	internal class Symbol
	{
        private static char[] BOLD_ALPHABET_UPPERCASE = new char[]
        {
            '\u0192', '\u00bd', '\u00bc', '\u03b1', '\u00df', '\u0393',
            '\u03c0', '\u03a3', '\u03c3', '\u00b5', '\u03c4', '\u03a6',
            '\u0398', '\u03a9', '\u03b4', '\u221e', '\u03c6', '\u03b5',
            '\u2229', '\u2261', '\u00b1', '\u2265', '\u2264', '\u2320',
            '\u2321', '\u00f7'
        };

        private static char[] BOLD_ALPHABET_LOWERCASE = new char[]
        {
            '\u2551', '\u2557', '\u255d', '\u255c', '\u255b', '\u2510',
            '\u2514', '\u2534', '\u252c', '\u251c', '\u2500', '\u253c',
            '\u255e', '\u255f', '\u255a', '\u2554', '\u2569', '\u2566',
            '\u2560', '\u2550', '\u256c', '\u2567', '\u2568', '\u2564',
            '\u2565', '\u2559'
        };

        internal static char BOLD_UPPERCASE_A = BOLD_ALPHABET_UPPERCASE[0];
		internal static char BOLD_UPPERCASE_B = BOLD_ALPHABET_UPPERCASE[1];
		internal static char BOLD_UPPERCASE_C = BOLD_ALPHABET_UPPERCASE[2];
        internal static char BOLD_UPPERCASE_D = BOLD_ALPHABET_UPPERCASE[3];
        internal static char BOLD_UPPERCASE_E = BOLD_ALPHABET_UPPERCASE[4];
        internal static char BOLD_UPPERCASE_F = BOLD_ALPHABET_UPPERCASE[5];
        internal static char BOLD_UPPERCASE_G = BOLD_ALPHABET_UPPERCASE[6];
        internal static char BOLD_UPPERCASE_H = BOLD_ALPHABET_UPPERCASE[7];
        internal static char BOLD_UPPERCASE_I = BOLD_ALPHABET_UPPERCASE[8];
        internal static char BOLD_UPPERCASE_J = BOLD_ALPHABET_UPPERCASE[9];
        internal static char BOLD_UPPERCASE_K = BOLD_ALPHABET_UPPERCASE[10];
        internal static char BOLD_UPPERCASE_L = BOLD_ALPHABET_UPPERCASE[11];
        internal static char BOLD_UPPERCASE_M = BOLD_ALPHABET_UPPERCASE[12];
        internal static char BOLD_UPPERCASE_N = BOLD_ALPHABET_UPPERCASE[13];
        internal static char BOLD_UPPERCASE_O = BOLD_ALPHABET_UPPERCASE[14];
        internal static char BOLD_UPPERCASE_P = BOLD_ALPHABET_UPPERCASE[15];
        internal static char BOLD_UPPERCASE_Q = BOLD_ALPHABET_UPPERCASE[16];
        internal static char BOLD_UPPERCASE_R = BOLD_ALPHABET_UPPERCASE[17];
        internal static char BOLD_UPPERCASE_S = BOLD_ALPHABET_UPPERCASE[18];
        internal static char BOLD_UPPERCASE_T = BOLD_ALPHABET_UPPERCASE[19];
        internal static char BOLD_UPPERCASE_U = BOLD_ALPHABET_UPPERCASE[20];
        internal static char BOLD_UPPERCASE_V = BOLD_ALPHABET_UPPERCASE[21];
        internal static char BOLD_UPPERCASE_W = BOLD_ALPHABET_UPPERCASE[22];
        internal static char BOLD_UPPERCASE_X = BOLD_ALPHABET_UPPERCASE[23];
        internal static char BOLD_UPPERCASE_Y = BOLD_ALPHABET_UPPERCASE[24];
        internal static char BOLD_UPPERCASE_Z = BOLD_ALPHABET_UPPERCASE[25];

		internal static char BOLD_LOWERCASE_A = BOLD_ALPHABET_LOWERCASE[0];
		internal static char BOLD_LOWERCASE_B = BOLD_ALPHABET_LOWERCASE[1];
		internal static char BOLD_LOWERCASE_C = BOLD_ALPHABET_LOWERCASE[2];
        internal static char BOLD_LOWERCASE_D = BOLD_ALPHABET_LOWERCASE[3];
        internal static char BOLD_LOWERCASE_E = BOLD_ALPHABET_LOWERCASE[4];
        internal static char BOLD_LOWERCASE_F = BOLD_ALPHABET_LOWERCASE[5];
        internal static char BOLD_LOWERCASE_G = BOLD_ALPHABET_LOWERCASE[6];
        internal static char BOLD_LOWERCASE_H = BOLD_ALPHABET_LOWERCASE[7];
        internal static char BOLD_LOWERCASE_I = BOLD_ALPHABET_LOWERCASE[8];
        internal static char BOLD_LOWERCASE_J = BOLD_ALPHABET_LOWERCASE[9];
        internal static char BOLD_LOWERCASE_K = BOLD_ALPHABET_LOWERCASE[10];
        internal static char BOLD_LOWERCASE_L = BOLD_ALPHABET_LOWERCASE[11];
        internal static char BOLD_LOWERCASE_M = BOLD_ALPHABET_LOWERCASE[12];
        internal static char BOLD_LOWERCASE_N = BOLD_ALPHABET_LOWERCASE[13];
        internal static char BOLD_LOWERCASE_O = BOLD_ALPHABET_LOWERCASE[14];
        internal static char BOLD_LOWERCASE_P = BOLD_ALPHABET_LOWERCASE[15];
        internal static char BOLD_LOWERCASE_Q = BOLD_ALPHABET_LOWERCASE[16];
        internal static char BOLD_LOWERCASE_R = BOLD_ALPHABET_LOWERCASE[17];
        internal static char BOLD_LOWERCASE_S = BOLD_ALPHABET_LOWERCASE[18];
        internal static char BOLD_LOWERCASE_T = BOLD_ALPHABET_LOWERCASE[19];
        internal static char BOLD_LOWERCASE_U = BOLD_ALPHABET_LOWERCASE[20];
        internal static char BOLD_LOWERCASE_V = BOLD_ALPHABET_LOWERCASE[21];
        internal static char BOLD_LOWERCASE_W = BOLD_ALPHABET_LOWERCASE[22];
        internal static char BOLD_LOWERCASE_X = BOLD_ALPHABET_LOWERCASE[23];
        internal static char BOLD_LOWERCASE_Y = BOLD_ALPHABET_LOWERCASE[24];
        internal static char BOLD_LOWERCASE_Z = BOLD_ALPHABET_LOWERCASE[25];

        internal static char BOLD_BANG = '\u2248';
        internal static char BOLD_QUESTION_MARK = '\u00B0';

        internal static char HEART_FULL = '\u2665';
        internal static char HEART_HALF = '\u2591';
        internal static char HEART_EMPTY = '\u2558';
        internal static char SKULL = '\u2592';
        internal static char BOW = '\u2593';
        internal static char HAND_GUN = '\u2502';
        internal static char ROCKET = '\u2524';
        internal static char BAZOOKA = '\u2561';
        internal static char LASER_GUN = '\u2562';
        internal static char SWORD = '\u2556';
        internal static char SQUARE_FULL = '\u2555';
        internal static char SQUARE_HALF = '\u2563';
        internal static char BOW_OUTLINE = '\u2552';
        internal static char HANDGUN_OUTLINE = '\u2553';
        internal static char ROCKET_OUTLINE = '\u256B';
        internal static char BAZOOKA_OUTLINE = '\u256A';
        internal static char LASERGUN_OUTLINE = '\u2518';
        internal static char SWORD_OUTLINE = '\u250C';

        private static char[] _lowercaseAlphabet = new char[]
        { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
          'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        private static char[] _uppercaseAlphabet = new char[]
        { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
          'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        private const int _OFFSET_ASCII_UPPERCASE = 65;
        private const int _OFFSET_ASCII_LOWERCASE = 97;

        private static bool IsLowercaseAlphabet(char character)
        {
            return Array.Exists(_lowercaseAlphabet, c => (c == character));
        }

        private static bool IsUppercaseAlphabet(char character)
        {
            return Array.Exists(_uppercaseAlphabet, c => (c == character));
        }

        private static char Bold(char character)
        {
            if (IsLowercaseAlphabet(character))
            {
                int index = ((int)character) - _OFFSET_ASCII_LOWERCASE;
                return BOLD_ALPHABET_LOWERCASE[index];
            }
            else if (IsUppercaseAlphabet(character))
            {
                int index = ((int)character) - _OFFSET_ASCII_UPPERCASE;
                return BOLD_ALPHABET_UPPERCASE[index];
            }
            else if (character == '!')
            {
                return BOLD_BANG;
            }
            else if (character == '?')
            {
                return BOLD_QUESTION_MARK;
            }

            return character;
        }

        internal static string Bold(string text)
        {
            char[] textArray = text.ToCharArray();

            for (int i = 0; i < textArray.Length; i++)
            {
                textArray[i] = Bold(textArray[i]);
            }

            return new string(textArray);
        }
    }
}
