using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FightingGameEngine.Enum
{
    public enum HitIndicator : int
    {
        WHIFF = 0,
        COUNTER_HIT = 1,
        BLOCKED = 1 << 1,
        GRABBED = 1 << 2,
        SUPER = 1 << 3,
        CLASH = 1 << 4,


    }
}