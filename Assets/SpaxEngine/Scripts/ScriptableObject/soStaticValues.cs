using UnityEngine;
namespace FightingGameEngine
{

    [CreateAssetMenu(fileName = "StaticValues", menuName = "StaticValues", order = 1)]

    public class soStaticValues : ScriptableObject
    {
        [SerializeField] private int _inputBuffer = 3;
        [SerializeField] private int _inputLeniency = 10;

        public int InputBuffer { get { return this._inputBuffer; } }
        public int InputLeniency { get { return this._inputLeniency; } }

    }
}