using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum InputFlags : uint
    {
        PRESSED = 1 << 0,
        RELEASED = 1 << 1,
        NO_INTERRUPT = 1 << 2,
        DIR_4WAY = 1 << 3,
        CHECK_CONTROLLER = PRESSED | RELEASED,
    }
}