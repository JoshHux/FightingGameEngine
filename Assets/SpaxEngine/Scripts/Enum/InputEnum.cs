namespace FightingGameEngine.Enum
{
    [System.Flags]
    public enum InputEnum : uint
    {
        _1 = 1 << 0,
        _2 = 1 << 1,
        _3 = 1 << 2,
        _4 = 1 << 3,
        _6 = 1 << 4,
        _7 = 1 << 5,
        _8 = 1 << 6,
        _9 = 1 << 7,
        A = 1 << 8,
        B = 1 << 9,
        C = 1 << 10,
        D = 1 << 11,
        W = 1 << 12,
        X = 1 << 13,
        Y = 1 << 14,
        Z = 1 << 15,
        X_NONZERO = _1 | _4 | _7 | _3 | _6 | _9,
        Y_NONZERO = _1 | _2 | _3 | _7 | _8 | _9,
        X_POSITIVE = _3 | _6 | _9,
        Y_POSITIVE = _7 | _8 | _9,
        X_NEGATIVE = _1 | _4 | _7,
        Y_NEGATIVE = _1 | _2 | _3,
        X_ZERO = _2 | _8,
        Y_ZERO = _4 | _6,
        X_NONZERO_LOW = _1 | _3,
        X_NONZERO_MID = _4 | _6,
        X_NONZERO_UP = _7 | _9,
        DIRECTIONS = _1 | _2 | _3 | _4 | _6 | _7 | _8 | _9,
        BUTTONS = A | B | C | D | W | X | Y | Z,
    }
}