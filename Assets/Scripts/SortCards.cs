using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SortCards : MonoBehaviour
{
    public void Run()
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

        GameObject[] cardsOrdered = cards.OrderBy(go => go.GetComponent<Card>().SortOrder).ToArray();

        for (int i = 0; i < cardsOrdered.Length; i++)

        {
            cardsOrdered[i].transform.SetSiblingIndex(i);
        }
    }
}
