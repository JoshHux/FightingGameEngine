using UnityEngine;
using TMPro;

namespace FightingGameEngine
{
    public class WinUiHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private GameObject _winUI;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Reset()
        {
            this._winUI.SetActive(false);
        }

        public void TriggerWin(int winnningPlayer)
        {
            this._winUI.SetActive(true);
            this._text.text = "player " + (winnningPlayer + 1) + " wins! \n backspace - character select \n space - rematch";
        }
    }
}