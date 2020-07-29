using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
   public InputField playerNameField;
   public InputField ipAddressField;

    void Start()
    {
        playerNameField.text = PlayerPrefs.GetString("username");
        ipAddressField.text = PlayerPrefs.GetString("ipAddress");
    }

    public void HostLan()
    {
        PlayerPrefs.SetString("username", playerNameField.text.Trim());
        joinTable("HostLan");
    }

    public void JoinOnline()
    {
        PlayerPrefs.SetString("username", playerNameField.text.Trim());
        joinTable("JoinOnline");
    }

    public void SinglePlayer()
    {
        joinTable("SinglePlayer");
    }

    public void LanServer()
    {
        joinTable("LanServer");
    }

    public void JoinLan()
    {
        PlayerPrefs.SetString("username", playerNameField.text.Trim());
        PlayerPrefs.SetString("ipAddress", ipAddressField.text.Trim());
        joinTable("JoinLan");
    }

    private void joinTable(string entranceChoice)
    {
        //TODO: use something other than magic strings here!
        PlayerPrefs.SetString("entranceChoice", entranceChoice);

        PlayerPrefs.Save();

        SceneManager.LoadScene("TableScene");
    }
}
