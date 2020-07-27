using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class CommonPlayerUI : NetworkBehaviour
{
    public Transform DealerTransform;
    public Transform ThinkingTransform;
    public Transform LastGoTransform;

    public void ShowIsDealer()
    {
        DealerTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/dealer");
    }

    public void ClearIsDealer()
    {
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

    public void ShowKnock()
    {
        LastGoTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/knock");
    }


}
