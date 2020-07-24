using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatePack : MonoBehaviour
{
    public GameObject CardPrefab;

    private string[] suits = new string[] {"Diamond", "Club", "Heart", "Spade"};

    void Start()
    {
        //Debug.Log("Creating Pack");

        string cardName = string.Empty;
        string id = string.Empty;
        int sortOrder = 0;

        foreach(var suit in suits)
        {
            for(int i = 1; i < 14; i++)
            {
                id = $"0{i}";
                cardName = suit + id.Substring(id.Length - 2);
                addCard(cardName, sortOrder++, suit, i);
            }
        }
    }

    private void addCard(string cardName, int sortOrder, string suit, int number)
    {
        GameObject newCard = Instantiate(CardPrefab, this.transform.position, this.transform.rotation);
        newCard.name = cardName;
        newCard.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/{cardName}");
        newCard.GetComponent<Card>().SortOrder = sortOrder;
        newCard.GetComponent<Card>().Suit = suit;
        newCard.GetComponent<Card>().Number = number;

        // Vector2 S = newCard.GetComponent<SpriteRenderer>().sprite.bounds.size;
        // newCard.GetComponent<BoxCollider2D>().size = S;

        newCard.transform.SetParent(this.transform, false);
    }


}
