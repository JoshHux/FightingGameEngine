using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum StateConditions : uint
    {
        GROUNDED = 1 << 0,
        AIRBORNE = 1 << 1,
        GUARD_POINT_LOW = 1 << 2,
        GUARD_POINT_MID = 1 << 3,
        GUARD_POINT_HIGH = 1 << 4,
        APPLY_GRAVITY = 1 << 5,
        STALL_GRAVITY = 1 << 6,
        APPLY_FRICTION = 1 << 7,
        CAN_MOVE = 1 << 8,
        AUTO_TURN = 1 << 9,
        STUN_STATE = 1 << 26,
        BUFFER_INPUT = 1 << 27,
        NO_PARENT_CANCEL = 1 << 28,
        NO_PARENT_TRANS = 1 << 29,
        NO_PARENT_COND = (uint)(1 << 30),

    }
}