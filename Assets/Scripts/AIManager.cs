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

    [SyncVar(hook = nameof(OnNumberOfCardPacksChanged))]
    public int NumberOfCardPacks = 1;

    public UnityEvent<int> AINumberChangedEvent = new UnityEvent<int>();
    public UnityEvent<int> CardPackNumberChangedEvent = new UnityEvent<int>();
    public UnityEvent<int> WineLevelChangedEvent = new UnityEvent<int>();
    public UnityEvent LeaveTableEvent = new UnityEvent();
    public UnityEvent ToggleSitOutEvent = new UnityEvent();

    public Toggle Wine1Button;
    public Toggle Wine2Button;
    public Toggle Wine3Button;
    public Toggle Wine4Button;
    public Toggle Wine5Button;

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

        AINumberChangedEvent.Invoke(newNumber);
    }

    public void SetNumberOfCardPacks(float numberOfPacks)
    {
        int newNumber = (int)numberOfPacks;

        CardPackNumberChangedEvent.Invoke(newNumber);
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
    private void OnNumberOfCardPacksChanged(int oldNumber, int newNumber)
    {
        var cardPackNumberSlider = GameObject.Find("PackNumberSlider").GetComponent<UnityEngine.UI.Slider>();
        if(cardPackNumberSlider.value != newNumber)
        {
            cardPackNumberSlider.value = newNumber;
        }
    }

    [Client]
    private void OnWineLevelChanged(int oldNumber, int newNumber)
    {
        switch(newNumber)
        {
            case 1:
                Wine1Button.isOn = true;
                break;
            case 2:
                Wine2Button.isOn = true;
                break;
            case 3:
                Wine3Button.isOn = true;
                break;
            case 4:
                Wine4Button.isOn = true;
                break;
            case 5:
                Wine5Button.isOn = true;
                break;
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

    public void SetWineLevel_5()
    {
        WineLevelChangedEvent.Invoke(5);
    }

    public void LeaveTable()
    {
        LeaveTableEvent.Invoke();
    }

    public void ToggleSitOut()
    {
        ToggleSitOutEvent.Invoke();
    }
}
