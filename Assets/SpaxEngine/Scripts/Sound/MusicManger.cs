using UnityEngine;
namespace FightingGameEngine.sound
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance;
        private AudioSource _source;

        [SerializeField] private AudioClip _startingClip;

        void Awake()
        {
            MusicManager.Instance = this;

            DontDestroyOnLoad(this.gameObject);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this._source = this.GetComponent<AudioSource>();


            this.SetMusic(this._startingClip);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetMusic(AudioClip clip)
        {
            this._source.clip = clip;

            this._source.Play();
        }
    }
}