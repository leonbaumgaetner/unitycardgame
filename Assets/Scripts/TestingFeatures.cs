using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingFeatures : MonoBehaviour
{
    public PlayerInsteraction playerInsteraction;
    public MainDeck mainDeckPlayer;

    public AbilityType abilityType;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            GiveLocalPlayerAfflict();
        }
    }
    public void HandToCenter()
    {
        playerInsteraction.testingHandToCenter = true;
        playerInsteraction.testingHandToLeftDeck = false;
    }

    public void HandToLeftDeck()
    {
        playerInsteraction.testingHandToCenter = false;
        playerInsteraction.testingHandToLeftDeck = true;
    }

    public void DrawACardFromDeck()
    {
        //mainDeckPlayer.DrawCardPlayer?.Invoke();
    }

    public void DiscardCard()
    {
        playerInsteraction.DiscardCard();
    }

    public void SetAttackables()
    {
      //  playerInsteraction.ActiveAttackableCards();
    }

    public void SelectEnemyCardsToAttack()
    {
        playerInsteraction.ActivateAttackablesEnemyCards();
    }

    public void SetManualAttackables()
    {
        playerInsteraction.ManualActivateAttack();
    }

    //enemy function
    public void Enemy_PlayResourceCard()
    {
        playerInsteraction.EnemyPlayResourceCard();
    }

    public void Enemy_PlayMinisterCard()
    {
        playerInsteraction.EnemyPlayMinisterCard();
    }

    public void Manual_EndEnemyTurn()
    {
        TurnManager.FinishedTurn?.Invoke(Turn.ENEMY);
        playerInsteraction.TurnOffGlowOpponent();
    }

    //ability

    public void GiveLocalPlayerAfflict()
    {
        playerInsteraction.GetPlayerDeck(DeckType.MAIN_DECK).GetComponent<MainDeck>().GiveAfflictCard();
    }

    public void GiveLocalPlayerRally()
    {
        playerInsteraction.GetPlayerDeck(DeckType.MAIN_DECK).GetComponent<MainDeck>().GiveRallyCard();
    }

    public void GiveEnhanceCardToLocalPlayer()
    {
        playerInsteraction.GetPlayerDeck(DeckType.MAIN_DECK).GetComponent<MainDeck>().GiveEnhanceCard();
    }

    public void GivePlayerDelayCard()
    {
        playerInsteraction.GetPlayerDeck(DeckType.MAIN_DECK).GetComponent<MainDeck>().GiveDelayCardToPlayer();
    }

    public void GivePlayerDimplomaticCard()
    {
        playerInsteraction.GetPlayerDeck(DeckType.MAIN_DECK).GetComponent<MainDeck>().GiveDiplomaticCardToPlayer();
    }

    public void GiveAAbilityTypeCard()
    {
        playerInsteraction.GetPlayerDeck(DeckType.MAIN_DECK).GetComponent<MainDeck>().GiveThisCardToPlayer(abilityType);
    }

    //public void GiveResourceCardToPlayer()
    //{
    //    playerInsteraction.GetPlayerDeck(DeckType.MAIN_DECK).GetComponent<MainDeck>().GiveResourceCardToPlayer();
    //}

    //public void GiveCabinetCardToPlayer()
    //{
    //    playerInsteraction.GetPlayerDeck(DeckType.MAIN_DECK).GetComponent<MainDeck>().GiveCabinetMinisterCardToPlayer();
    //}
}
