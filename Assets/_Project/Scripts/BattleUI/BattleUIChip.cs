using System.Collections;
using DG.Tweening;
using UnityEngine;
namespace FightingGameEngine.BattleUI
{
    public class BattleUIChip : MonoBehaviour
    {
        private SkinnedMeshRenderer _meshRenderer;
        private AudioSource _audioSource;

        [SerializeField] private AudioClip _audioClip;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this._meshRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>();
            this._audioSource = this.GetComponent<AudioSource>();
        }

        public void SetMaterial(Material newMat)
        {
            if (this._meshRenderer == null) { this._meshRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>(); }
            Debug.Log(this._meshRenderer.materials[0]);
            this._meshRenderer.materials[0] = newMat;
        }

        public void CashOutChip()
        {
            this._audioSource.clip = this._audioClip;
            this._audioSource.Play();
            StartCoroutine(PlayCashout());
        }

        private IEnumerator PlayCashout()
        {
            this.transform.DORotate(new Vector3(0, 180, 0), 0.1f);
            this.transform.DOMoveY(this.transform.position.y + 5f, 0.1f);

            yield return new WaitForSeconds(0.1f);
            this.gameObject.SetActive(false);


        }
    }
}