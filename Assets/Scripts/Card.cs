using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Card : MonoBehaviour
{
    public int SortOrder;
    public string Suit;
    public int Number;

    void OnMouseUp()
    {
        var player = GameObject.FindGameObjectsWithTag("Player").First(p => p.GetComponent<Player>().IsActive);
        player.GetComponent<Player>().SelectCard(this.gameObject);
    }
}
