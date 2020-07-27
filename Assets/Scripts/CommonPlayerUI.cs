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

    public void SetStatusText(string statusText)
    {
        StatusText.text = statusText;
    }

    public void ShowIsDealer()
    {
        Debug.Log("ShowIsDealer");
        DealerTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/dealer");
    }

    public void ClearIsDealer()
    {
        Debug.Log("ClearIsDealer");
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

    public void ShowLastGo(Sprite sprite)
    {
        LastGoTransform.GetComponent<Image>().sprite = sprite;
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
    }

    public void ClearPlaced()
    {
        PlacedTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/transparent"); 
    }

    public void ClearAll()
    {
        SetStatusText(string.Empty);
        ClearIsDealer();
        ClearIsThinking();
        ClearLastGo();
        ClearPlaced();
    }


}
