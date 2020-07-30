using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using UnityEngine.UI;

public class AIManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNumberOfAIPlayersChanged))]
    public int NumberOfAIPlayers = 0;

    [SyncVar(hook = nameof(OnWineLevelChanged))]
    public int WineLevel = 1;

    public UnityEvent<int> NumberChangedEvent = new UnityEvent<int>();
    public UnityEvent<int> WineLevelChangedEvent = new UnityEvent<int>();

    public Toggle Wine1Button;
    public Toggle Wine2Button;
    public Toggle Wine3Button;
    public Toggle Wine4Button;

    void Start()
    {
        if(isLocalPlayer)
        {
            OnWineLevelChanged(0, WineLevel);
        }
    }

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

    [Client]
    private void OnWineLevelChanged(int oldNumber, int newNumber)
    {
        Wine1Button.isOn = newNumber == 1;
        Wine2Button.isOn = newNumber == 2;
        Wine3Button.isOn = newNumber == 3;
        Wine4Button.isOn = newNumber == 4;
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
