using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public int SortOrder;
    public string Suit;
    public int Number;
    public bool IsClickable;
    public PlayingCard PlayingCard;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(IsClickable)
        {
            //Debug.Log("Mouse Clicked");
            GameObject.Find("LocalNetworkPlayer").GetComponent<NetworkPlayer>().PlayCard(PlayingCard);
        }
    }
}
