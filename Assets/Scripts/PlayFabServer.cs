using System.Collections;
using UnityEngine;
using PlayFab;
using System;

public class PlayFabServer : MonoBehaviour
{
	private PlayFabMessageRelay playFabMessageRelay;

    void Start()
    {
        #if UNITY_SERVER
			Debug.Log("We are running as UNITY_SERVER");
            StartRemoteServer();
        #endif
    }

    private void StartRemoteServer()
	{
		PlayFabMultiplayerAgentAPI.Start();
		PlayFabMultiplayerAgentAPI.IsDebugging = false;
		PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
		PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
		//PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive; //TODO: use this to start polling for backfill?
		PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;

		StartCoroutine(ReadyForPlayers());
		//StartCoroutine(ShutdownServerInXTime());
	}

	// IEnumerator ShutdownServerInXTime()
	// {
	// 	yield return new WaitForSeconds(300f);
	// 	OnShutdown();
	// }

	IEnumerator ReadyForPlayers()
	{
		yield return new WaitForSeconds(.5f);
		PlayFabMultiplayerAgentAPI.ReadyForPlayers();
		
	}

	private void getRelay()
	{
		if(playFabMessageRelay == null)
		{
			playFabMessageRelay = GameObject.Find("PlayFabMessageRelay").GetComponent<PlayFabMessageRelay>();
		}
	}

	private void OnAgentError(string error)
	{
		getRelay();
		playFabMessageRelay.RpcSendClientMessage(error);
	}

	private void OnShutdown()
	{
		getRelay();
		playFabMessageRelay.RpcSendClientMessage("The server is shutting down - sorry!");
		StartCoroutine(ShutdownServer());
	}

	IEnumerator ShutdownServer()
	{
		yield return new WaitForSeconds(5f);
		Application.Quit();
	}

	private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc)
	{
		getRelay();
		playFabMessageRelay.RpcSendClientMessage($"Maintenance scheduled for: {NextScheduledMaintenanceUtc.Value.ToLongDateString()}");
	}
}
