using FixMath.NET;
namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class AnimationFrameData
    {
        [UnityEngine.SerializeField] private int _atFrame;
        [UnityEngine.SerializeField] private string _animationName;
        [UnityEngine.SerializeField] private Fix64 _animationSpeed = 1;
        //whether or not we ignore applying this animation if we're already doing that animation
        [UnityEngine.SerializeField] private bool _skipIfSameName;
        [UnityEngine.SerializeField] private int _startFrame;
        [UnityEngine.SerializeField] private string _soundFx;
        [UnityEngine.SerializeField] private int _vfx;
        [UnityEngine.SerializeField] private FVector3 _vfxPosition;
        [UnityEngine.SerializeField] private FVector3 _vfxRotation;

        public int AtFrame { get { return this._atFrame; } }
        public string AnimationName { get { return this._animationName; } }
        public Fix64 AnimationSpeed { get { return this._animationSpeed; } }
        public bool SkipIfSameName { get { return this._skipIfSameName; } }
        public int StartFrame { get { return this._startFrame; } }
        public string SoundFX { get { return this._soundFx; } }
        public int VFX { get { return this._vfx; } }
        public FVector3 VFXPosition { get { return this._vfxPosition; } }
        public FVector3 VFXRotation { get { return this._vfxRotation; } }
    }
}