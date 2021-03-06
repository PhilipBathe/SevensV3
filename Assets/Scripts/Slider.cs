﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slider : MonoBehaviour
{
    private Text sliderValueText;

    void Start()
    {
        sliderValueText = GetComponent<Text>();
    }

    public void HandleValueChanged(float value)
    {
        sliderValueText.text = value.ToString();
    }
}
