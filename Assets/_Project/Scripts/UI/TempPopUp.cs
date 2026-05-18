using UnityEngine;
using UnityEngine.InputSystem;

public class TempPopUp : MonoBehaviour
{
    public GameObject _popUp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame) { this.Hide(); }
    }

    public void Appear() { this._popUp.SetActive(true); }
    public void Hide() { this._popUp.SetActive(false); }
}
