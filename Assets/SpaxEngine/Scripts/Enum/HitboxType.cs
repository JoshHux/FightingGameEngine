using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum HitboxType : uint
    {
        STRIKE_MID = 1 << 0,
        STRIKE_LOW = 1 << 1,
        STRIKE_HIGH = 1 << 2,
        GRAB_GROUNDED = 1 << 3,
        GRAB_AIRBORNE = 1 << 4,

        STRIKE = STRIKE_MID | STRIKE_LOW | STRIKE_HIGH,
        GRAB = GRAB_AIRBORNE | GRAB_GROUNDED,
    }
}