using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    private Dictionary<string, GameObject> suitHolders = new Dictionary<string, GameObject>();

    void Start()
    {
        foreach(Transform child in this.transform)
        {
            if(child.tag == "SuitSlot")
            {
                suitHolders.Add(child.transform.name, child.gameObject);
            }
        }
    }

    public void ClearBoard()
    {
        foreach(var suit in suitHolders)
        {
            foreach(Transform child in suit.Value.transform)
            {
                if (child.tag == "Card")
                {
                    Destroy(child.gameObject);
                }
            }

            suit.Value.transform.DetachChildren();
        }
    }

    public void PlayCard(GameObject card)
    {
        Debug.Log($"playing card {card.name}");
        GameObject suitSlot = suitHolders[card.GetComponent<Card>().Suit];

        card.transform.SetParent(suitSlot.transform, false);
        card.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(80, 80);

        var cardNumber = card.GetComponent<Card>().Number;
        float y = 0f;

        if(cardNumber < 7)
        {
            y = (cardNumber - 7) * 5f - 50f;
        }

        if(cardNumber > 7)
        {
            y = (cardNumber - 7) * 5f + 50f;
        }

        card.transform.localPosition = new Vector3(0f, y, Math.Abs(cardNumber - 7) * -0.01f);

    }

    public bool IsCardPlayable(GameObject card)
    {
        var cardNumber = card.GetComponent<Card>().Number;

        if(cardNumber == 7)
        {
            return true;
        }

        GameObject suitSlot = suitHolders[card.GetComponent<Card>().Suit];

        foreach(Transform child in suitSlot.transform)
        {
            if (child.tag == "Card")
            {
                if(Math.Abs(child.GetComponent<Card>().Number - cardNumber) == 1)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
