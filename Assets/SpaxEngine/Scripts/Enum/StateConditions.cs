using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum StateConditions : int
    {
        GROUNDED = 1 << 0,
        AIRBORNE = 1 << 1,
        GUARD_POINT_LOW = 1 << 2,
        GUARD_POINT_MID = 1 << 3,
        GUARD_POINT_HIGH = 1 << 4,
        APPLY_GRAVITY = 1 << 5,
        STALL_GRAVITY = 1 << 6,
        BUFFER_INPUT = 1 << 27,
        NO_PARENT_CANCEL = 1 << 28,
        NO_PARENT_TRANS = 1 << 29,
        NO_PARENT_COND = 1 << 30,

    }
}