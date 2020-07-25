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

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("Mouse Clicked");
        
        if(IsClickable)
        {
            var player = GameObject.FindGameObjectsWithTag("Player").First(p => p.GetComponent<Player>().IsActivePlayer);
            player.GetComponent<Player>().SelectCard(this.gameObject);
        }
    }

    private Sprite cardFront;
    private Sprite cardBack;

    public void Flip()
    {
        if(cardBack == null)
        {
            cardBack = Resources.Load<Sprite>($"Sprites/backColor_Black");
        }

        Sprite currentSprite = gameObject.GetComponent<Image>().sprite;

        if(cardFront == null)
        {
            cardFront = currentSprite;
        }

        if (currentSprite == cardFront)
        {
            gameObject.GetComponent<Image>().sprite = cardBack;
        }
        else
        {
            gameObject.GetComponent<Image>().sprite = cardFront;
        }
    }
}
