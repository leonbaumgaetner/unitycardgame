using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class MyNetworkManager : NetworkManager
{

    public GameManager networkGame;


    CardPlayer[] cardPlayers;
    int currentTurn = 0;
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Connected to the server as a client");
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log($"A client has connected to the server {numPlayers}");

        if(numPlayers == 1)
        {
            Debug.Log("2 players joined");
            //2 players have connected. Call ClientRPC to begin game.
           
        }
    }

    
    public void ReadyToStartGame()
    {
        Debug.Log("Player is ready to start game");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("server has started");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

      

        Debug.Log("Player created" + numPlayers);
        if(numPlayers == 2)
        {
            //string attack1 = networkGame.CreateAttackDeck();
            //string resource1 = networkGame.CreateResourceDeck();

            //string attack2 = networkGame.CreateAttackDeck();
            //string resource2 = networkGame.CreateResourceDeck();

            cardPlayers = FindObjectsOfType<CardPlayer>();

            cardPlayers[0].CmdLoadCardDeck();
            cardPlayers[0].playerID = 1;
            cardPlayers[1].CmdLoadCardDeck();
            cardPlayers[1].playerID = 2;

            Invoke("StartGameWithTurn",3);
        }       
    }

    private void StartGameWithTurn()
    {
        int i = Random.Range(0, 2);

        if (i == 0)
        {
            cardPlayers[0].RpcPlayerToPlay(1);
            currentTurn = 1;

        }
        else
        {
            cardPlayers[1].RpcPlayerToPlay(2);
            currentTurn = 2;
        }

        Debug.Log($"Player with id {currentTurn} plays first");
    }

   
}
