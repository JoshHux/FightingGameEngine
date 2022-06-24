using UnityEditor;
using UnityEngine;

namespace Spax
{
    public class SpaxBehavior : MonoBehaviour
    {

        void Awake() { this.SpaxAwake(); }
        void Start() { this.SpaxStart(); }

        // Start is called before the first frame update
        public void SpaxAwake()
        {
            this.OnAwake();
        }

        public void SpaxStart()
        {
            this.OnStart();
        }

        protected virtual void OnStart() { }

        protected virtual void OnAwake() { }
    }
}
