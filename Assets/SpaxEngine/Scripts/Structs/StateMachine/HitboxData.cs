using FixMath.NET;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Data
{
    [System.Serializable]
    public struct HitboxData
    {
        public int Duration;
        public int UniversalStateCause;
        public HitboxType Type;
        public FVector2 Position;
        public FVector2 Dimensions;

        //if multiple hitboxes hit the opponent, priority will decide which one takes priority
        //higher priority wins out
        public int Priority;

        //basic stun and damage values
        public int Damage;
        public int ChipDamage;
        public int Hitstun;
        //minimum hitstun value, only really useful with "cinematic" attacks like grabs or supers
        public int MinHitstun;
        public int BlockStun;
        public int Hitstop;
        public int BlockStop;

        //special values, like air untech time
        public int MinDamage;
        public int UntechTime;
        public int CrouchStunMod;
        public int CounterStunMod;

        //values that interact with the opponent aside from damage and stun

        //knockback applied when opponent is grounded
        public FVector2 GroundedKnockback;
        //knockback applied when opponent is airborne
        public FVector2 AirborneKnockback;

        //whether or not we change the gravity of the hit character
        public bool changeHitGravity;
        //the gravity applied to the hit character (temporarily replaces mass value in status), only applied when changeHitGravity is set to true
        public Fix64 HitGravity;

        //wall and ground bounce count, while the hit character is in stun, they will mpounce on the respective surface than many times
        //if 0, then the respective bounces don't occur
        public int GroundBounces;
        //scaling applied to the respective bounce, once bounced, reflective velocity = velocity (x or y) * scaling
        public Fix64 GroundBounceMultiplier;
        //wall and ground bounce count, while the hit character is in stun, they will mpounce on the respective surface than many times
        //if 0, then the respective bounces don't occur
        public int WallBounces;
        //scaling applied to the respective bounce, once bounced, reflective velocity = velocity (x or y) * scaling
        public Fix64 WallBounceMultiplier;

        //proration values, applied on the next state onwards
        public Fix64 InitProration;
        public Fix64 ForcedProration;

        //change in resources when hitbox connects
        public ResourceData ResourceChange;

        //data for what the move can be cancelled into
        public CancelConditions OnHitCancel;
        public CancelConditions OnCounterHitCancel;
        public CancelConditions OnBlockedHitCancel;

        public bool IsValid()
        {
            bool ret = this.Duration > 0;

            return ret;
        }

    }
}