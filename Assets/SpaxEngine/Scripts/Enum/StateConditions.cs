using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum StateConditions : uint
    {
        GROUNDED = 1 << 0,
        AIRBORNE = 1 << 1,
        APPLY_GRAVITY = 1 << 2,
        STALL_GRAVITY = 1 << 3,
        APPLY_FRICTION = 1 << 4,
        CAN_MOVE = 1 << 5,
        AUTO_TURN = 1 << 6,

        INVULNERABLE_STRIKE = 1 << 21,
        INVULNERABLE_GRAB = 1 << 22,

        GUARD_POINT_MID = 1 << 23,
        GUARD_POINT_LOW = 1 << 24,
        GUARD_POINT_HIGH = 1 << 25,
        STUN_STATE = 1 << 26,
        BUFFER_INPUT = 1 << 27,
        NO_PARENT_CANCEL = 1 << 28,
        NO_PARENT_TRANS = 1 << 29,
        NO_PARENT_COND = (uint)(1 << 30),

    }
}