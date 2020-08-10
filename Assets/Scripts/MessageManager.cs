using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MessageManager : MonoBehaviour
{
    private Text messageText;
    private Animator messageAnimator;

    void  Start() {
        messageText = GameObject.Find("MessageText").GetComponent<Text>();
        messageAnimator = GameObject.Find("Messages").GetComponent<Animator>();
    }

    /// wipes previous messages
    public void ShowNewMessage(string message)
    {
        showMessagePanel();
        messageText.text = message;
    }

    /// adds to previous messages
    public void AddMessage(string message)
    {
        showMessagePanel();
        messageText.text += Environment.NewLine + message;
    }

    private void showMessagePanel()
    {
        messageAnimator.SetBool("isHidden", false);
    }

    public void HideMessagePanel()
    {
        messageAnimator.SetBool("isHidden", true);
    }
}
