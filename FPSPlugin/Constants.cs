using System;
using BlockID = System.UInt16;

namespace FPS
{
	internal static class Constants
	{
		internal const string FPS_DIRECTORY_PATH = "./fps/";
		internal const string REPLAYS_DIRECTORY_PATH = "./fps/replays/";
		internal const string GAME_PROPERTIES_FILE_PATH = "./fps/game.properties";
        internal const float GRAVITY = 9.81f;
        internal const BlockID GUN_BLOCK = 41;
        internal const float MIN_GUN_VELOCITY = 50f;
        internal const float MAX_GUN_VELOCITY = 300f;
        internal const uint MS_GUN_RELOAD = 200;
        internal const uint GUN_DAMAGE = 1;
        internal const float GUN_FRAME_LENGTH = 1;
        internal const BlockID ROCKET_BLOCK = 42;
        internal const float MIN_ROCKET_VELOCITY = 10f;
        internal const float MAX_ROCKET_VELOCITY = 62f;
        internal const uint MS_ROCKET_RELOAD = 2000;
        internal const uint ROCKET_DAMAGE = 1;
        internal const float ROCKET_FRAME_LENGTH = 4f;
        internal const uint MS_UPDATE_WEAPON_ANIMATIONS = 50;
        internal const uint MS_UPDATE_ROUND_STATUS = 50;
        internal const uint MS_ROUND_TICK = 50;
        internal const float MAX_MOVE_DISTANCE = 1.5625f;
    }
}

