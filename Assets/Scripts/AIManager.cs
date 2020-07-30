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
    public UnityEvent<int> WineLevelChangedEvent = new UnityEvent<int>();

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

    public void SetWineLevel_1()
    {
        WineLevelChangedEvent.Invoke(1);
    }

    public void SetWineLevel_2()
    {
        WineLevelChangedEvent.Invoke(2);
    }

    public void SetWineLevel_3()
    {
        WineLevelChangedEvent.Invoke(3);
    }

    public void SetWineLevel_4()
    {
        WineLevelChangedEvent.Invoke(4);
    }
}
