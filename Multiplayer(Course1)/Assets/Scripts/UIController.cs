﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;//very usful thing to acsess in this script 
    private void Awake()
    {
        instance = this;
    }
    public TMP_Text overheatedMessage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
