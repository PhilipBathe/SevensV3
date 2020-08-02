using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class CommonPlayerUI : NetworkBehaviour
{
    public Text StatusText;
    public Transform DealerTransform;
    public Transform ThinkingTransform;
    public Transform LastGoTransform;
    public Transform PlacedTransform;
    public Transform PlayerTypeTransform;

    private bool hasPlaced;

    public void SetStatusText(string statusText)
    {
        if(hasPlaced == false)
        {
            StatusText.text = statusText;
        }
    }

    public void ShowIsDealer()
    {
        //Debug.Log("ShowIsDealer");
        DealerTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/dealer");
    }

    public void ClearIsDealer()
    {
        //Debug.Log("ClearIsDealer");
        DealerTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/transparent");
    }

    public void ShowIsThinking()
    {
        ThinkingTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/thinking");
    }

    public void ClearIsThinking()
    {
        ThinkingTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/transparent"); 
    }

    public void ShowLastGo(PlayingCard card)
    {
        LastGoTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/{card.CardName}"); 
    }

    public void ClearLastGo()
    {
        LastGoTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/transparent"); 
    }

    public void ShowKnock()
    {
        LastGoTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/knock");
    }

    public void ShowPlaced(int position)
    {
        var placedSpriteName = position > 9 ? "bad" : position.ToString();
        PlacedTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/{placedSpriteName}");
        hasPlaced = true;
    }

    public void ClearPlaced()
    {
        PlacedTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/transparent"); 
        hasPlaced = false;
    }

    public void ShowIsAI()
    {
        PlayerTypeTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/ai"); 
    }

    public void ShowIsSittingOut()
    {
        PlayerTypeTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/chair"); 
    }

    public void ClearPlayerType()
    {
        PlayerTypeTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/transparent"); 
    }

    public void ShowIsTableHost()
    {
        PlayerTypeTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/pirate-captain"); 
    }

    public void ClearAll(string statusText)
    {
        hasPlaced = false;
        SetStatusText(statusText);
        ClearIsDealer();
        ClearIsThinking();
        ClearLastGo();
        ClearPlaced();
        //don't try to clear player type at the moment
    }


}
