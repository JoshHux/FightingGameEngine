using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FightingGameEngine.Gameplay
{
    public class FightingCharacterController : ControllableObject
    {
        protected override void StateCleanUpdate() { }
        protected override void PreUpdate() { }
        protected override void PostPhysUpdate() { }
        protected override void HitboxQueryUpdate() { }
        protected override void HurtboxQueryUpdate() { }
    }
}