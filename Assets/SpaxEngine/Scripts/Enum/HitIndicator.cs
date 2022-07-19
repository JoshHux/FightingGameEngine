using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FightingGameEngine.Enum
{
    public enum HitIndicator : uint
    {
        WHIFF = 0,
        HIT = 1,
        COUNTER_HIT = 1 << 1,
        BLOCKED = 1 << 2,
        GRABBED = 1 << 3,
        SUPER = 1 << 4,
        CLASH = 1 << 5,


    }
}