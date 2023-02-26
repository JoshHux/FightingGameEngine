#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{

    public class CommandFactory
    {
        public static SetHitboxEvent SetHitboxes(in string[] parameters, in soStateData state, int lnNum)
        {
            int n = Int32.Parse(parameters[0]);

            if (n < 0 || n >= state.get_hit_set_len()) { UnityEngine.Debug.LogError("Line " + lnNum + " in " + state.name + ":\nOut of bounds error index is " + n); }

            return new SetHitboxEvent(state.GetHitboxSet(n));
        }

        public static SetHurtboxEvent SetHurtboxes(in string[] parameters, in soStateData state, int lnNum)
        {
            int n = Int32.Parse(parameters[0]);

            if (n < 0 || n >= state.get_hurt_set_len()) { UnityEngine.Debug.LogError("Line " + lnNum + " in " + state.name + ":\nOut of bounds error index is " + n); }

            return new SetHurtboxEvent(state.GetHurtboxSet(n));
        }
    }
}
#endif