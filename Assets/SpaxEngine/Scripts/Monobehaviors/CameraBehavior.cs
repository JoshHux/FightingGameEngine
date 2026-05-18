using Spax;
using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;
using System.Collections;
namespace FightingGameEngine
{
    public class CameraBehavior : MonoBehaviour
    {
        public static CameraBehavior Instance;
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private float _minCamSize = 10f;
        [SerializeField] private float _maxCamSize = 15f;

        [SerializeField] private Vector2 _minCorner;

        [SerializeField] private Vector2 _maxCorner;
        public GameObject test;
        void Awake()
        {
            CameraBehavior.Instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //this._camera = this.GetComponent<Camera>();

            this._camera.Lens.OrthographicSize = this._minCamSize;
        }

        // Update is called once per frame
        void Update()
        {
            if (SpaxManager.Instance.Players.Count < 2) { return; }
            //update position
            var pos1 = SpaxManager.Instance.Players[0].transform.position;
            var pos2 = SpaxManager.Instance.Players[1].transform.position;

            this.transform.position = new Vector3((pos1.x + pos2.x) / 2f, (pos1.y + pos2.y) / 2f, this.transform.position.z);

            //adjust size            
            var height = this._camera.Lens.OrthographicSize;
            var width = height * this._camera.Lens.Aspect;

            if ((pos1 - pos2).magnitude > 0.91f * width) { this._camera.Lens.OrthographicSize = Mathf.Lerp(this._camera.Lens.OrthographicSize, this._camera.Lens.OrthographicSize * 1.6f, 0.01f); }
            else if ((pos1 - pos2).magnitude < 0.89f * width) { this._camera.Lens.OrthographicSize = Mathf.Lerp(this._camera.Lens.OrthographicSize, this._camera.Lens.OrthographicSize * 0.4f, 0.01f); }

            this._camera.Lens.OrthographicSize = Mathf.Clamp(this._camera.Lens.OrthographicSize, this._minCamSize, this._maxCamSize);


            //correct position
            //height = this._camera.orthographicSize;
            //width = height * this._camera.aspect;


            //var posModifier = new Vector3(0, 0, 0);
            //
            //if (this.transform.position.x - width / 2 < this._minCorner.x) { posModifier.x = this._minCorner.x - (this.transform.position.x - width / 2); }
            //else if (this.transform.position.x + width / 2 > this._maxCorner.x) { posModifier.x = this._maxCorner.x - (this.transform.position.x + width / 2); }
            //
            //if (this.transform.position.y - height / 2 < this._minCorner.y)
            //{
            //    posModifier.y = this._minCorner.y - (this.transform.position.y - (height / 2f));
            //    Debug.Log(posModifier.y + this.transform.position.y - (height / 2f));
            //}
            //else if (this.transform.position.y + height / 2 > this._maxCorner.y) { posModifier.y = this._maxCorner.y - (this.transform.position.y + height / 2); }
            //
            //test.transform.position = this.transform.position + new Vector3(0, -height / 2f, 0);
            //
            //this.transform.position += posModifier;


        }

        public void ShakeCam(float intensity, float duration)
        {
            //this._camera.transform.DOShakePosition(0.1f, new Vector3(intensity, intensity, 0));
            StartCoroutine(this._ProcessShake(intensity, duration));
            //Debug.Log("got");
        }

        private IEnumerator _ProcessShake(float shakeIntensity = 5f, float shakeDuration = 0.1f)
        {
            var noise = this._camera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            noise.AmplitudeGain = shakeIntensity;
            yield return new WaitForSeconds(shakeDuration);
            noise.AmplitudeGain = 0f;
        }

        public void Noise(float amplitudeGain, float frequencyGain)
        {
            //this._camera.topRig.Noise.m_AmplitudeGain = amplitudeGain;
            //this._camera.middleRig.Noise.m_AmplitudeGain = amplitudeGain;
            //this._camera.no.Noise.m_AmplitudeGain = amplitudeGain;
            // 
            //this._camera.topRig.Noise.m_FrequencyGain = frequencyGain;
            //this._camera.middleRig.Noise.m_FrequencyGain = frequencyGain;
            //this._camera.bottomRig.Noise.m_FrequencyGain = frequencyGain;      

        }
    }
}