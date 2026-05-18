using UnityEngine;
using UnityEngine.SceneManagement;

namespace FightingGameEngine.CharacterSelect
{
    public class StartScreenManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ChangeScene(string newScene)
        {
            SceneManager.LoadScene(newScene);
        }
        public void Quit()
        {
            Application.Quit();
        }
    }
}