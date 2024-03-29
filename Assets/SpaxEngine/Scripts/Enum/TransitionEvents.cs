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
        FLIP_FACING = 1 << 5,
        ADD_RESOURCES = 1 << 6,
        FACE_OPPONENT = 1 << 7,

        DODGE_RESOURCE_CHECK = 1 << 29,
        GRAB_TECH = 1 << 30,

    }
}