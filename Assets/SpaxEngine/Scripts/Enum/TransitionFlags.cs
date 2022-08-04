using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum TransitionFlags : uint
    {
        //when a character becomes grounded
        GROUNDED = 1 << 0,
        //when a character becomes airborne
        AIRBORNE = 1 << 1,
        //when a character makes contact with the wall
        WALLED = 1 << 3,
        //when a character gets hit
        GOT_HIT = 1 << 4,
        //when a character lands a hit
        LANDED_HIT = 1 << 5,
        //when a character blocks a hit
        BLOCKED_HIT = GOT_HIT | 1 << 6,
        //when a character's current state ends
        STATE_END = 1 << 30,
    }
}