using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum TransitionEvents : uint
    {
        KILL_Y_VEL = 1 << 0,
        KILL_X_VEL = 1 << 1,
        KILL_Z_VEL = 1 << 2,
        CLEAN_HITBOXES = 1 << 3,
        EXIT_STUN = 1 << 4,

    }
}