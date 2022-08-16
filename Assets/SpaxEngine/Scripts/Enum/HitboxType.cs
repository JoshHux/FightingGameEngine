using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum HitboxType : uint
    {
        STRIKE_MID = 1 << 0,
        STRIKE_LOW = 1 << 1,
        STRIKE_HIGH = 1 << 2,
        GRAB_GROUNDED = GROUNDED_UNBLOCKABLE | 1 << 3,
        GRAB_AIRBORNE = AIR_UNBLOCKABLE | 1 << 4,
        PROJECTILE = 1 << 5,
        OFF_THE_GROUND = 1 << 6,
        GROUNDED_UNBLOCKABLE = 1 << 7,
        AIR_UNBLOCKABLE = 1 << 8,
        //STRIKE_UNBLOCKABLE = STRIKE_LOW | STRIKE_HIGH,
        TRUE_UNBLOCKABLE = AIR_UNBLOCKABLE | GROUNDED_UNBLOCKABLE,
        STRIKE = STRIKE_MID | STRIKE_LOW | STRIKE_HIGH,
        GRAB = 1 << 3 | 1 << 4,
    }
}