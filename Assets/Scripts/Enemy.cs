using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    public string PlayerName;

    [SyncVar(hook = nameof(OnPlayerColorChanged))]
    public string PlayerColor;

    [SyncVar]
    public int SeatNumber;

    [SyncVar(hook = nameof(OnParentChanged))]
    public GameObject Parent;


    [SyncVar(hook = nameof(OnStatusChanged))]
    public string Status;
    

    void OnPlayerNameChanged(string oldPlayerName, string newPlayerName)
    {
        foreach (Text text in gameObject.GetComponentsInChildren<Text> ())
        {
            if(text.name == "PlayerNameText")
            {
                text.text = newPlayerName;
            }
        }
    }

    void OnPlayerColorChanged(string oldPlayerColor, string newPlayerColor)
    {

        Transform background = null;
        Sprite sprite = Resources.Load<Sprite>($"Sprites/enemy_background_{newPlayerColor}");

        if(sprite == null)
        {
            sprite = Resources.Load<Sprite>($"Sprites/enemy_background_black");
            Debug.Log($"Couldn't find background resource for color '{newPlayerColor}'");
        }

        foreach(Transform child in this.transform)
        {
            if(child.name == "Background")
            {
                background = child;
            }
        }

        if(background == null)
        {
            return;
        }

        background.GetComponent<Image>().sprite = sprite;
    }

    void OnParentChanged(GameObject oldParent, GameObject newParent)
    {
        //TODO: hide ourselves?
        this.transform.SetParent(newParent.transform, false);
    }

    void OnStatusChanged(string oldStatus, string newStatus)
    {
        foreach (Text text in gameObject.GetComponentsInChildren<Text> ())
        {
            if(text.name == "StatusText")
            {
                text.text = newStatus;
            }
        }
    }

    [Server]
    public void Reset() {
        Status = string.Empty;
        //TODO: reset other bits
    }

}
