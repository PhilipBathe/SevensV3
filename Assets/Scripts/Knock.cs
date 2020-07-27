using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knock : MonoBehaviour
{
    public Animator KnockButtonAnimator;
    private GameObject activePlayer;

    public void SetActivePlayer(GameObject player)
    {
        activePlayer = player;
    }

    public void OnClick()
    {
        KnockButtonAnimator.SetBool("isHidden", true);
        activePlayer.GetComponent<NetworkPlayer>().Knock();
    }
}
