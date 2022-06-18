using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

using UnityEngine.Events;
public class CardPlayer : NetworkBehaviour
{
    public UnityAction OnlineSwitchTurnEvent;


    [SyncVar]
    public int playerID = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Player client has started");
    }

   [ClientRpc]
   public void RpcReadyToStartGame(string attackData, string resourceData)
    {        
       Debug.Log("Displaying data from server"+ attackData + " Resource Data " + resourceData + " has authority" + hasAuthority);

        if(hasAuthority)
        {
            //load as local player
            DeckManager.LoadDeckCardsLocal?.Invoke(attackData, resourceData);
        }
        else
        {
            //load as enemy player
           // DeckManager.LoadDeckCardsEnemy?.Invoke(attackData, resourceData);
        }
    }

    [Server]
    public void CmdLoadCardDeck()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        string a = gameManager.CreateAttackDeck();
        string r = gameManager.CreateResourceDeck();
        RpcReadyToStartGame(a, r);
    }

    [ClientRpc]
    public void RpcPlayerToPlay(int i)
    {
        if(hasAuthority)
        {
            TurnManager.OnTurnChangedTo?.Invoke(Turn.LOCAL);
        }
        else
        {
            TurnManager.OnTurnChangedTo?.Invoke(Turn.ENEMY);
        }
    }


    [Command]
    public void CmdResourceCardPlayed(string s)
    {
        RpcPlayedResourceCard(s);
    }


    [Command]
    public void CmdMinisterCardPlayed(string s)
    {
        RpcPlayedMinisterCard(s);
    }

    [Command]
    public void CmdAbilityCardPlayed(string s)
    {
        RpcAbility(s);
    }

    [Command]
    public void CmdCardCancelledFromRightDeck()
    {
        RpcCardCancelled();
    }

    [ClientRpc]
    private void RpcCardCancelled()
    {
        if(!hasAuthority)
        {
            DeckManager.EnemyCancelledCard?.Invoke();
        }
        
    }

    #region CardPlayed

    [ClientRpc]
    private void RpcAbility(string cardPlayed)
    {
        if(!hasAuthority)
        {
            DeckManager.PlayedAbilityCard?.Invoke(cardPlayed);
        }
    }

    [ClientRpc]
    private void RpcPlayedResourceCard(string c)
    {
        if(!hasAuthority)
        {
            Debug.Log("My player played a resource card" + c);
            // play any card from the enemies deck
            DeckManager.PlayedResourceCard?.Invoke(c);
        }
    }

    [ClientRpc]
    private void RpcPlayedMinisterCard(string c)
    {
        if (!hasAuthority)
        {
            Debug.Log("My player played a minister card" + c);
            DeckManager.PlayedMinisterCard?.Invoke(c);
        }
    }




    #endregion

    #region TargetAbility
    [Command]
    public void CmdTargetCardSelected(int id, string targetCardPlayerType, string abilityType)
    {
        RpcEnemySelectedTarget(id,targetCardPlayerType, abilityType);
    }

    [ClientRpc]
    public void RpcEnemySelectedTarget(int id,string targetCardPlayerType, string abilityType)
    {
        if(!hasAuthority)
        {
            DeckManager.AbilityTargetSelected?.Invoke(id, targetCardPlayerType, abilityType);
        }
    }

    #endregion

    [Command]
    public void CmdSwitchTurn()
    {
        Debug.Log("Server is switching turns");
        RpcSwitchTurnForThisClient();
    }

    [ClientRpc]
    public void RpcSwitchTurnForThisClient()
    {
        Debug.Log("Rpc switching turns" + hasAuthority);
       if(!hasAuthority)
        OnlineSwitchTurnEvent?.Invoke();
        
    }


    #region Combat   


    [Command]
    public void CmdOnCardsSelectedForAttack(List<int> cardsIndexes)
    {      
        //server got selected cards
        Debug.Log("got cards selected for attacks on server"+ cardsIndexes[0]);
        RpcOnCardsSelectedForAttack(cardsIndexes);
    }

    [ClientRpc]
    public void RpcOnCardsSelectedForAttack(List<int> cardsIndex)
    {
        if(!hasAuthority)
        {
            //got cards selected for attack
            Debug.Log("attack cards selected " + cardsIndex[0]);
            DeckManager.AttackCardsSelectedOnline?.Invoke(cardsIndex);
        }
    }

    [Command]
    public void CmdDefendLinesConfirmed(string defendData)
    {        
        RpcDefendLinesConfirmed(defendData);        
    }

    [ClientRpc]
    public void RpcDefendLinesConfirmed(string defendData)
    {
        if(!hasAuthority)
        {
            DeckManager.DefenseCardsSelectedOnline?.Invoke(defendData);
        }       
    }



    #endregion


    #region GameOver
    [Command]
    public void CmdPlayerDied()
    {
        RpcPlayerDied();
    }

    [ClientRpc]
    public void RpcPlayerDied()
    {
        if(!hasAuthority)
        {
            AttackManager.enemyPlayerDead?.Invoke();
        }
    }

    #endregion

    #region Functions
    [Command]
    public void CmdGiveEnemyFakeCard()
    {
        RpcFakeCard();
    }

    [ClientRpc]
    public void RpcFakeCard()
    {
        if(!hasAuthority)
        {
            DeckManager.HandAFakeCard?.Invoke();
        }
    }
    #endregion
    private void OnDestroy()
    {
        OnlineSwitchTurnEvent = null;
    }
}
