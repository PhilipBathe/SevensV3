using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class AIManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNumberOfAIPlayersChanged))]
    public int NumberOfAIPlayers = 0;

    public UnityEvent<int> NumberChangedEvent = new UnityEvent<int>();

    public void SetNumberOfAIPlayers(float numberOfPlayers)
    {
        int newNumber = (int)numberOfPlayers;

        NumberChangedEvent.Invoke(newNumber);
    }

    [Client]
    private void OnNumberOfAIPlayersChanged(int oldNumber, int newNumber)
    {
        var aiNumberSlider = GameObject.Find("PlayerNumberSlider").GetComponent<UnityEngine.UI.Slider>();
        if(aiNumberSlider.value != newNumber)
        {
            aiNumberSlider.value = newNumber;
        }
    }
}
