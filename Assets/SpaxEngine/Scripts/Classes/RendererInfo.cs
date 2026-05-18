using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;
using FightingGameEngine.Gameplay;
namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class RendererInfo
    {
        //ID of VFX to spawn
        private int _vfxID;
        //position of VFX to spawn
        private Vector3 _vfxPos;
        //list of VFX objects according to ID
        [UnityEngine.SerializeField] private soVFXValues _vfxValues;

        //were we hit this frame?
        [SerializeField] private TransitionFlags _relevantEvents;

        //public GameObject VFXObject { get { return this._vfxObject; } set { this._vfxObject = value; } }

        public int VFXID { get { return this._vfxID; } set { this._vfxID = value; } }
        public Vector3 VFXPos { get { return this._vfxPos; } set { this._vfxPos = value; } }
        public soVFXValues VFXValues { get { return this._vfxValues; } set { this._vfxValues = value; } }
        public TransitionFlags RelevantEvents { get { return this._relevantEvents; } set { this._relevantEvents = value; } }

        public RendererInfo(int vi, Vector3 vp, soVFXValues vv, TransitionFlags tf)
        {
            this._vfxID = vi;
            this._vfxPos = vp;
            this._vfxValues = vv;
            this._relevantEvents = tf;
        }

        //separate constructor so we don't reset the VFXValues every post render update
        public RendererInfo(int vi, Vector3 vp)
        {
            this._vfxID = vi;
            this._vfxPos = vp;
        }
    }
}