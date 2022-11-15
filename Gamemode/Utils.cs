using System;
namespace FPSMO
{
	internal static class Utils
	{
		internal static int Clamp(int value, int min, int max)
		{
            return (value < min) ? min : (value > max) ? max : value;
        }
	}
}