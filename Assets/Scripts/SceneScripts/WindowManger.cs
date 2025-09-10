using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public List<GenericWindow> windows;

    public Windows defaultWindow; //제일 먼저 열린 위도우 사용

    public Windows CurrentWindow { get; private set; }

    private void Start()
    {
        foreach (var window in windows)
        {
            window.Init(this);
            //window.Close();
            window.gameObject.SetActive(false);
        }

        CurrentWindow = defaultWindow;
        windows[(int)CurrentWindow].Open();
    }

    public void Open(Windows id, bool allClose)
    {
        windows[(int)CurrentWindow].Close();
        CurrentWindow = id;
        windows[(int)CurrentWindow].Open();
    }
}
