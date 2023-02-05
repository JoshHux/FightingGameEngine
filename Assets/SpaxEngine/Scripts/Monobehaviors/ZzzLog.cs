using UnityEngine;
using System.Collections;

public class ZzzLog : MonoBehaviour
{
    uint qsize = 15;  // number of messages to keep
    Queue myLogQueue = new Queue();

    public static ZzzLog Instance;
    string txt;

    void Start()
    {
        Debug.Log("Started up logging.");
        ZzzLog.Instance = this;
    }

    void OnEnable()
    {
        //Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        //Application.logMessageReceived -= HandleLog;
    }

    public void SetTxt(string t) { this.txt = t; }

    /*
        void HandleLog(string logString, string stackTrace, LogType type) {
            myLogQueue.Enqueue("[" + type + "] : " + logString);
            if (type == LogType.Exception)
                myLogQueue.Enqueue(stackTrace);
            while (myLogQueue.Count > qsize)
                myLogQueue.Dequeue();
        }

    */

    void OnGUI()
    {
        GUIStyle headStyle = new GUIStyle();
        headStyle.fontSize = 30;
        GUILayout.BeginArea(new Rect(Screen.width - 400, 400, 400, Screen.height));
        GUILayout.Label("\n" + string.Join("\n", txt), headStyle);
        GUILayout.EndArea();
    }

}