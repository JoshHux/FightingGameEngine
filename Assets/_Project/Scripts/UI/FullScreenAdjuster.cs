using UnityEngine;

public class FullScreenAdjuster : MonoBehaviour
{
    void Awake()
    {
        this._fullScreen = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private bool _fullScreen;
    public void ToggleFullScreen()
    {
        this._fullScreen = !this._fullScreen;

        if (this._fullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
        }
    }
}
