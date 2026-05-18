using FixMath.NET;
using FightingGameEngine.Enum;
using MessagePack;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    [MessagePackObject]

    public struct HitboxData
    {
        [Key(0)] public int Duration;
        [Key(1)] public int UniversalStateCause;
        [Key(2)] public int CounterStateCause;
        [Key(3)] public HitboxType Type;
        [Key(4)] public FVector2 Position;
        [Key(5)] public FVector2 Dimensions;

        //if multiple hitboxes hit the opponent, priority will decide which one takes priority
        //higher priority wins out
        [Key(6)] public int Priority;

        //basic stun and damage values
        [Key(7)] public int Damage;
        [Key(8)] public int ChipDamage;
        [Key(9)] public int MinDamage;

        //stun values
        [Key(10)] public int Hitstun;
        //minimum hitstun value, only really useful with "cinematic" attacks like grabs or supers
        [Key(11)] public int MinHitstun;
        [Key(12)] public int BlockStun;
        [Key(13)] public int Hitstop;
        [Key(14)] public int BlockStop;

        //special values, like air untech time
        [Key(15)] public int UntechTime;
        [Key(16)] public int CrouchStunMod;

        //counter hit modifications
        [Key(17)] public int CounterUntechMod;
        [Key(18)] public int CounterStunMod;
        [Key(19)] public int CounterStopMod;

        //values that interact with the opponent aside from damage and stun

        //knockback applied when opponent is grounded
        [Key(20)] public FVector2 GroundedKnockback;
        //knockback applied when opponent is airborne
        [Key(21)] public FVector2 AirborneKnockback;

        //whether or not we change the gravity of the hit character
        [Key(22)] public bool changeHitGravity;
        //the gravity applied to the hit character (temporarily replaces mass value in status), only applied when changeHitGravity is set to true
        [Key(23)] public Fix64 HitGravity;

        //wall and ground bounce count, while the hit character is in stun, they will mpounce on the respective surface than many times
        //if 0, then the respective bounces don't occur
        [Key(24)] public int GroundBounces;
        //scaling applied to the respective bounce, once bounced, reflective velocity = velocity (x or y) * scaling
        [Key(25)] public Fix64 GroundBounceMultiplier;
        //wall and ground bounce count, while the hit character is in stun, they will mpounce on the respective surface than many times
        //if 0, then the respective bounces don't occur
        [Key(26)] public int WallBounces;
        //scaling applied to the respective bounce, once bounced, reflective velocity = velocity (x or y) * scaling
        [Key(27)] public Fix64 WallBounceMultiplier;

        //proration values, applied on the next state onwards
        [Key(28)] public Fix64 InitProration;
        [Key(29)] public Fix64 ForcedProration;

        //change in resources when hitbox connects
        [Key(30)] public ResourceData ResourceChange;

        //data for what the move can be cancelled into
        [Key(31)] public CancelConditions OnHitCancel;
        [Key(32)] public CancelConditions OnCounterHitCancel;
        [Key(33)] public CancelConditions OnBlockedHitCancel;

        //VFX data
        [Key(34)] public int Hitspark;

        public bool IsValid()
        {
            bool ret = this.Duration > 0;

            return ret;
        }

    }
}