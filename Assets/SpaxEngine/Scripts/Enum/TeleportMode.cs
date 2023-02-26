using System;
namespace FightingGameEngine.Enum
{
    public enum TeleportMode : byte
    {
        NONE = 0,
        PLAYER_RELATIVE = 1 << 0,
        OPPONENT_RELATIVE = 1 << 1,
    }

}
