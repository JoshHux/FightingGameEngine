namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class AnimationFrameData
    {
        [UnityEngine.SerializeField] private int _atFrame;
        [UnityEngine.SerializeField] private string _animationName;
        //whether or not we ignore applying this animation if we're already doing that animation
        [UnityEngine.SerializeField] private bool _skipIfSameName;
        [UnityEngine.SerializeField] private int _startFrame;
        [UnityEngine.SerializeField] private string _soundFx;
        [UnityEngine.SerializeField] private string _vfx;

        public int AtFrame { get { return this._atFrame; } }
        public string AnimationName { get { return this._animationName; } }
        public bool SkipIfSameName { get { return this._skipIfSameName; } }
        public int StartFrame { get { return this._startFrame; } }
        public string SoundFX { get { return this._soundFx; } }
        public string VFX { get { return this._vfx; } }
    }
}