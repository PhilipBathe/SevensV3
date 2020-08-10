using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayFabMessageRelay : NetworkBehaviour
{
    private MessageManager messageManager;

    void Start()
    {
        messageManager = GameObject.Find("MessageManager").GetComponent<MessageManager>();
    }
    
    [ClientRpc]
    public void RpcSendClientMessage(string message)
    {
        messageManager.SendMessage(message);
    }
}
