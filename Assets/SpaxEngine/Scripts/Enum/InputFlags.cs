using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum InputFlags : uint
    {
        NO_INTERRUPT = 1 << 2,
        DIR_4WAY = 1 << 3,

        BACKEND_FLAGS = DIR_4WAY,
    }
}