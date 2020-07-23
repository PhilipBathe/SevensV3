using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public int SortOrder;
    public string Suit;
    public int Number;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Mouse Clicked");
        var player = GameObject.FindGameObjectsWithTag("Player").First(p => p.GetComponent<Player>().IsActive);
        player.GetComponent<Player>().SelectCard(this.gameObject);
    }
}
