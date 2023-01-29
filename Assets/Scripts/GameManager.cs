using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    void Start()
    {
  
        Application.targetFrameRate= 60;                   // Sets a target framrate for Unity
        QualitySettings.vSyncCount = 0;                    // Limits the vsync coutn to 0 thus turning off vsync

    }

 }
