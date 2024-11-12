#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using FixMath.NET;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Commands
{

    public class CommandFactory
    {
        public static SetHitboxEvent SetHitboxes(in string[] parameters, in soStateData state, int lnNum)
        {
            int n = Int32.Parse(parameters[0]);

            if (n < 0 || n >= state.get_hit_set_len()) { throw new ArgumentException("Line " + lnNum + " in " + state.name + ":\nOut of bounds error index is " + n); }

            return new SetHitboxEvent(state.GetHitboxSet(n));
        }

        public static SetHurtboxEvent SetHurtboxes(in string[] parameters, in soStateData state, int lnNum)
        {
            int n = Int32.Parse(parameters[0]);

            if (n < 0 || n >= state.get_hurt_set_len()) { throw new ArgumentException("Line " + lnNum + " in " + state.name + ":\nOut of bounds error index is " + n); }

            return new SetHurtboxEvent(state.GetHurtboxSet(n));
        }


        public static SetVelEvent SetVel(in string[] parameters, in soStateData state, int lnNum)
        {
            //get the velocity from an array
            var dim = parameters[0].Split(',');
            if (dim.Length > 2) { throw new ArgumentException("Line " + lnNum + " in " + state.name + ":\nToo many dimensions!"); }

            FVector2 vel = new FVector2(Fix64.Parse(dim[0]), Fix64.Parse(dim[1]));

            return new SetVelEvent(vel);
        }

        public static ApplyVelEvent ApplyVel(in string[] parameters, in soStateData state, int lnNum)
        {
            //get the velocity from an array
            var dim = parameters[0].Split(',');
            if (dim.Length > 2) { throw new ArgumentException("Line " + lnNum + " in " + state.name + ":\nToo many dimensions!"); }

            FVector2 vel = new FVector2(Fix64.Parse(dim[0]), Fix64.Parse(dim[1]));
            //UnityEngine.Debug.Log(vel.x);
            return new ApplyVelEvent(vel);
        }


        public static ApplyRsrcEvent ApplyRsrc(in string[] parameters, in soStateData state, int lnNum)
        {

            var r1 = Int16.Parse(parameters[0]);
            var r2 = Int16.Parse(parameters[1]);
            var r3 = Int16.Parse(parameters[2]);
            var r4 = Int16.Parse(parameters[3]);
            var r5 = Int16.Parse(parameters[4]);
            var r6 = Int16.Parse(parameters[5]);
            var r7 = Int16.Parse(parameters[6]);
            var r8 = Int16.Parse(parameters[7]);
            var r9 = Int16.Parse(parameters[8]);
            UnityEngine.Debug.Log(parameters[0]);
            UnityEngine.Debug.Log(parameters[1]);
            UnityEngine.Debug.Log(parameters[2]);
            UnityEngine.Debug.Log(parameters[3]);
            UnityEngine.Debug.Log(parameters[4]);
            UnityEngine.Debug.Log(parameters[5]);
            UnityEngine.Debug.Log(parameters[6]);
            UnityEngine.Debug.Log(parameters[7]);
            UnityEngine.Debug.Log(parameters[8]);

            var rsrc = new ResourceData(r1, r2, r3, r4, r5, r6, r7, r8, r9);

            return new ApplyRsrcEvent(rsrc);
        }

        public static SetRsrcEvent SetRsrc(in string[] parameters, in soStateData state, int lnNum)
        {
            var r1 = Int16.Parse(parameters[0]);
            var r2 = Int16.Parse(parameters[1]);
            var r3 = Int16.Parse(parameters[2]);
            var r4 = Int16.Parse(parameters[3]);
            var r5 = Int16.Parse(parameters[4]);
            var r6 = Int16.Parse(parameters[5]);
            var r7 = Int16.Parse(parameters[6]);
            var r8 = Int16.Parse(parameters[7]);
            var r9 = Int16.Parse(parameters[8]);


            var rsrc = new ResourceData(r1, r2, r3, r4, r5, r6, r7, r8, r9);

            return new SetRsrcEvent(rsrc);
        }

        public static TeleportEvent Teleport(in string[] parameters, in soStateData state, int lnNum)
        {

            //get the position from an array
            var dim = parameters[0].Split(',');
            if (dim.Length > 2) { throw new ArgumentException("Line " + lnNum + " in " + state.name + ":\nToo many dimensions!"); }
            FVector2 pos = new FVector2(Fix64.Parse(dim[0]), Fix64.Parse(dim[1]));

            TeleportMode m = 0;
            //parse into mode
            System.Enum.TryParse<TeleportMode>(parameters[1], out m);

            return new TeleportEvent(pos, m);
        }

        public static SuperFlashEvent SuperFlash(in string[] parameters, in soStateData state, int lnNum)
        {
            int n = Int32.Parse(parameters[0]);
            return new SuperFlashEvent(n);
        }

        public static SetGravityEvent SetGrav(in string[] parameters, in soStateData state, int lnNum)
        {
            Fix64 n = Fix64.Parse(parameters[0]);
            return new SetGravityEvent(n);
        }

        public static ToggleConditionEvent ToggleCond(in string[] parameters, in soStateData state, int lnNum)
        {

            StateConditions m = 0;
            //parse into conditions
            System.Enum.TryParse<StateConditions>(parameters[0], out m);
            //UnityEngine.Debug.Log(m);
            return new ToggleConditionEvent(m);
        }

        public static ToggleCancelEvent ToggleCancel(in string[] parameters, in soStateData state, int lnNum)
        {


            CancelConditions m = 0;
            //parse into conditions
            System.Enum.TryParse<CancelConditions>(parameters[0], out m);
            return new ToggleCancelEvent(m);
        }

        public static ArmorEvent SetArmor(in string[] parameters, in soStateData state, int lnNum)
        {
            int n = Int32.Parse(parameters[0]);
            return new ArmorEvent(n);
        }


        public static ProjectileSpawnEvent SpawnProjectile(in string[] parameters, in soStateData state, int lnNum)
        {
            int p = Int32.Parse(parameters[0]);
            //get the position from an array
            var dim = parameters[1].Split(',');
            if (dim.Length > 2) { throw new ArgumentException("Line " + lnNum + " in " + state.name + ":\nToo many dimensions!"); }

            FVector2 pos = new FVector2(Fix64.Parse(dim[0]), Fix64.Parse(dim[1]));

            Fix64 rot = Fix64.Parse(parameters[2]);
            return new ProjectileSpawnEvent(p, pos, rot);
        }

        public static CondTimerEvent CondTimer(in string[] parameters, in soStateData state, int lnNum)
        {
            int d = Int32.Parse(parameters[0]);
            StateConditions m = 0;
            //parse into conditions
            System.Enum.TryParse<StateConditions>(parameters[0], out m);
            return new CondTimerEvent(d, m);
        }
    }
}
#endif