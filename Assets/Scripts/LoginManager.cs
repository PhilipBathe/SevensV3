using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
   public InputField playerNameField;

    void Start()
    {
        playerNameField.text = PlayerPrefs.GetString("username").Trim();
    }

    public void JoinTable() 
    {
        PlayerPrefs.SetString("username", playerNameField.text);
        PlayerPrefs.Save();
        //Debug.Log(name.text);

        SceneManager.LoadScene("TableScene");
    }
}
