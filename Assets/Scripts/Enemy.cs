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

    [SyncVar(hook = nameof(OnCardCountChanged))]
    public int CardCount;



    public GameObject EnemyCardPrefab;

    public GameObject Hand;
    

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
        CardCount = 0;
        //TODO: reset other bits
    }

    void OnCardCountChanged(int oldCardCount, int newCardCount)
    {
        if(newCardCount == 0)
        {
            clearHand();
            return;
        }

        if(newCardCount == oldCardCount-1)
        {
            removeOneCard();
            return;
        }

        //if we are here then we should be creating a new hand at the start of a new game

        clearHand();

        for(int i = 0; i < newCardCount; i++)
        {
            GameObject playerCard = Instantiate(EnemyCardPrefab, Vector2.zero, Quaternion.identity, Hand.transform);
        }
    }

    private void clearHand()
    {
        foreach(Transform child in Hand.transform)
        {
            Destroy(child.gameObject);
        }

        Hand.transform.DetachChildren();
    }

    private void removeOneCard()
    {
        foreach(Transform child in Hand.transform)
        {
            Destroy(child.gameObject);
            break;
        }
    }

}
