using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum InputFlags : uint
    {
        NO_INTERRUPT = 1 << 2,
        DIR_4WAY = 1 << 3,
        ANY_IS_DOWN = 1 << 28,
        CHECK_CONTROLLER = 1 << 29,
        CHECK_IS_UP = (uint)(1 << 30),

        BACKEND_FLAGS = DIR_4WAY | CHECK_IS_UP,
    }
}