using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    #region [Section] Manager Header
    public static AppManager Instance { get; private set; }
    public static Boolean IsInitialized => Instance != null;
    #endregion
    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Instance = this;
    }

    private void Start() 
    {
        MapManager.Instance.OpenMap("proceduralmap.3000.793197.164.map");
    }
}
