using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;
using DG.Tweening;
public class UIManager : MonoBehaviour
{
    public Button attackButtonAll, attackButtonSelected, noAttackButton;

    public Button noBlocksButton, cancelBlocksButton, selectedBlocksButton;

    SelectedAttackButton selectedAttack;

    PlayerInsteraction playerInsteraction;

    public static UnityAction LineDestroyed;

    public Button nextPhaseButton, finishTurnButton;

    public TextMeshProUGUI whoseTurnText;

    public Transform[] actionUIButtons;

    private CardPlayer opponentCardPlayer;
    private CardPlayer localCardPlayer;

    public GameObject gameOverParent;
    public TextMeshProUGUI gameOverText;

    public void Start()
    {
        whoseTurnText.gameObject.SetActive(false);

        attackButtonAll.gameObject.SetActive(false);
        attackButtonSelected.gameObject.SetActive(false);

        attackButtonAll.onClick.AddListener(OnAttackButtonAllClicked);
        attackButtonSelected.onClick.AddListener(OnAttackButtonSelectedClicked);
        nextPhaseButton.onClick.AddListener(NextPhaseForLocalPlayer);
        finishTurnButton.onClick.AddListener(FinishTurnForLocalPlayer);
        noAttackButton.onClick.AddListener(ShowFinishTurnUI);

        //defense buttons
        noBlocksButton.onClick.AddListener(NoDefence);
        cancelBlocksButton.onClick.AddListener(CancelDefenses);
        selectedBlocksButton.onClick.AddListener(DefendLinesConfirmed);

        AttackManager.FinishedSettingLine += UpdateAttacks;
        LineDestroyed += RecountAttacks;
        selectedAttack = attackButtonSelected.GetComponent<SelectedAttackButton>();

        GameManager.OnGameModeChanged += GameModeChanged;
        GameManager.OnOpponentCardSet += SetOpponentPlayer;
        GameManager.OnLocalCardSet += SetLocalPlayer;

        playerInsteraction = FindObjectOfType<PlayerInsteraction>();

        TurnManager.OnTurnChangedTo += WhenTurnChanged;
        TurnManager.FinishedTurn += OnFinishedTurn;
        playerInsteraction.OnCardPlacedOnCenterDeck += CardPlacedOnCenterDeck;

        //defense
        DefendManager.OnDefendLineCreated += DefendLineCompleted;
        DefendManager.OnDefendLineDeleted += DefendLineDeleted;

        //game over
        AttackManager.localPlayerDead += LocalPlayerDied;
        AttackManager.enemyPlayerDead += EnemyPlayerDied;
       


    }

    private void OnFinishedTurn(Turn arg0)
    {
        //whoseTurnText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        AttackManager.FinishedSettingLine -= UpdateAttacks;
        LineDestroyed -= RecountAttacks;
        TurnManager.FinishedTurn -= OnFinishedTurn;

        //defense
        DefendManager.OnDefendLineCreated -= DefendLineCompleted;
        DefendManager.OnDefendLineDeleted -= DefendLineDeleted;

        //defense buttons
        noBlocksButton.onClick.RemoveListener(NoDefence);
        cancelBlocksButton.onClick.RemoveListener(CancelDefenses);
        selectedBlocksButton.onClick.RemoveListener(DefendLinesConfirmed);
    }

    private void GameModeChanged(GAME_MODE gAME_MODE)
    {
        if(gAME_MODE == GAME_MODE.ATTACKING)
        {
            //cancel attack
            noAttackButton.gameObject.SetActive(true);


            //attack all
            attackButtonAll.gameObject.SetActive(true);

            //attackButtonSelected.gameObject.SetActive(true);

        }

        else if(gAME_MODE == GAME_MODE.DEFENDING  && !TurnManager.isPlayerTurn)
        {
            //if no cards to defend with, call no blocks directly
            if(playerInsteraction.IsAttackPossible()) //change to is defence possible later
                ShowDefenceButton();
            else
            {
                NoDefence();
            }
        }
        else
        {
            attackButtonSelected.gameObject.SetActive(false);
        }
    }
    private void OnAttackButtonAllClicked()
    {
      if(GameManager.isOnline)
        {
            noAttackButton.gameObject.SetActive(false);
            playerInsteraction.ActivateAllAttackableOnline();
            attackButtonAll.gameObject.SetActive(false);
        }
      else
        {
            noAttackButton.gameObject.SetActive(false);
            playerInsteraction.ActiveAttackableCards(OnFinishAttackingWithSelectedCards);
            attackButtonAll.gameObject.SetActive(false);
        }    
       
    }

    #region Attack
    private void OnAttackButtonSelectedClicked()
    {
        if(GameManager.isOnline)
        {
            //send the selected cards to attack to the other player
            attackButtonSelected.gameObject.SetActive(false);
            noAttackButton.gameObject.SetActive(false);
            whoseTurnText.gameObject.SetActive(false);

            AttackLine[] allAttackLines = FindObjectsOfType<AttackLine>();
           
            List<Transform> selectedAttackCards = new List<Transform>();
            foreach (var item in allAttackLines)
            {
                selectedAttackCards.Add(item.GetOwner());
            }

            //find the index of the attacking cards
            Transform cardParent = selectedAttackCards[0].parent;
            //holds the index of attack cards selected
            
            List<int> selectedCardIds = new List<int>();
           
            for (int i = 0; i < selectedAttackCards.Count; i++)
            {

                selectedCardIds.Add(selectedAttackCards[i].GetComponent<PlayerHandCard>().thisCardData.id);
                Debug.Log("sending attack card id " + selectedCardIds[i]);
            }

            
            localCardPlayer.CmdOnCardsSelectedForAttack(selectedCardIds);

        }
        else
        {
            attackButtonSelected.gameObject.SetActive(false);

            AttackLine[] allAttackLines = FindObjectsOfType<AttackLine>();
            List<Transform> selectedAttackCards = new List<Transform>();
            foreach (var item in allAttackLines)
            {
                selectedAttackCards.Add(item.GetOwner());
            }

            AttackManager.AttackCardsSelected?.Invoke(selectedAttackCards, OnFinishAttackingWithSelectedCards);
        }

       

        
    }

    private void OnFinishAttackingWithSelectedCards()
    {
        ShowFinishTurnUI();
    }

    private void UpdateAttacks(AttackLine line = null)
    {
        AttackLine[] allAttackLines = FindObjectsOfType<AttackLine>();

        

        attackButtonAll.gameObject.SetActive(false);
        attackButtonSelected.gameObject.SetActive(true);
        selectedAttack.SetAttackText(allAttackLines.Length);
    }

    private void RecountAttacks()
    {
        AttackLine[] allAttackLines = FindObjectsOfType<AttackLine>();    
       
        selectedAttack.SetAttackText(allAttackLines.Length);
    }

    #endregion


    #region Turn Related
    private void WhenTurnChanged(Turn t)
    {
        whoseTurnText.gameObject.SetActive(true);
        if (t == Turn.LOCAL)
        {
            nextPhaseButton.gameObject.SetActive(true);
            whoseTurnText.text = "Your Turn";
        }
        else
        {
            nextPhaseButton.gameObject.SetActive(false);
            whoseTurnText.text = "Opponent's Turn";
        }
    }

    private void NextPhaseForLocalPlayer()
    {        
        nextPhaseButton.gameObject.SetActive(false);

        if(playerInsteraction.IsAttackPossible())
        {
            GameManager.CurrentGameMode?.Invoke(GAME_MODE.ATTACKING);
            //playerInsteraction.ManualActivateAttack();
        }
        else
        {
            FinishTurnForLocalPlayer();
        }

       
    }

    private void ShowFinishTurnUI()
    {
        TurnOffAllActionButtons();
        playerInsteraction.TurnOffGlowLocal();
        finishTurnButton.gameObject.SetActive(true);
    }

    private void FinishTurnForLocalPlayer()
    {
       if(GameManager.isOnline)
       {
            localCardPlayer.CmdSwitchTurn();

            finishTurnButton.gameObject.SetActive(false);
            TurnManager.FinishedTurn?.Invoke(Turn.LOCAL);
        }
       else
        {
            finishTurnButton.gameObject.SetActive(false);
            TurnManager.FinishedTurn?.Invoke(Turn.LOCAL);
        }       
    }

    

    #endregion

    


    #region Defending
    private void ShowDefenceButton()
    {

        TurnOffAllActionButtons();
        noBlocksButton.gameObject.SetActive(true);
    }

    private void ShowDefenseCountAndCancelButtons()
    {
        noBlocksButton.gameObject.SetActive(false);
        cancelBlocksButton.gameObject.SetActive(true);
        selectedBlocksButton.gameObject.SetActive(true); 
    }
    //player doesnt have any defense
    private void NoDefence()
    {
        if(GameManager.isOnline)
        {
            TurnOffAllActionButtons();
            DefendInformation defendInformation = new DefendInformation();
            string defendString = JsonUtility.ToJson(defendInformation);

            localCardPlayer.CmdDefendLinesConfirmed(defendString);
           

            AttackManager attackManager = FindObjectOfType<AttackManager>();
            attackManager.ExecuteCombatForDefendingPlayer(defendInformation);
        }
        else
        {
            //tell enemy to attack
            TurnOffAllActionButtons();
            playerInsteraction.EnemyAttacks();
        }
       

    }

    private void CancelDefenses()
    {
        DefendLine[] defendLines = FindObjectsOfType<DefendLine>();
        int count = defendLines.Length;

        for (int i = 0; i < count; i++)
        {
            Destroy(defendLines[i].gameObject);
        }
        DefendLineDeleted();
    }

    private void DefendLineCompleted()
    {
        //count total defense Lines
        //set blocks count
        DefendLine[] defendLines = FindObjectsOfType<DefendLine>();
        int count = defendLines.Length;
        if (count > 1)
            selectedBlocksButton.GetComponentInChildren<TextMeshProUGUI>().text = "" + count + " BLOCKS";
        else
            selectedBlocksButton.GetComponentInChildren<TextMeshProUGUI>().text = "" + count + " BLOCK";
        
        ShowDefenseCountAndCancelButtons();
    }

    private void DefendLineDeleted()
    {
        //add a little delay before counting
        DOVirtual.DelayedCall(0.5f, () => {

            DefendLine[] defendLines = FindObjectsOfType<DefendLine>();
            int count = defendLines.Length;

            int correctedCount = 0;
            for (int i = 0; i < count; i++)
            {
                if(defendLines[i].isLineSet == true)
                {
                    correctedCount++;
                }
            }
            if (correctedCount > 0)
            {
                if(correctedCount > 1)
                    selectedBlocksButton.GetComponentInChildren<TextMeshProUGUI>().text = "" + correctedCount + " BLOCKS";
                else
                    selectedBlocksButton.GetComponentInChildren<TextMeshProUGUI>().text = "" + correctedCount + " BLOCK";
            }
            else
            {
                ShowDefenceButton();
            }

        });
        
    }

    private void DefendLinesConfirmed()
    {
        if(GameManager.isOnline)
        {
            TurnOffAllActionButtons();

            #region Send Data Online
            //1. collect the info from defend lines
            DefendLine[] defendLines = FindObjectsOfType<DefendLine>();


            DefendInformation defendInformation = new DefendInformation();

           
            //we collect cards IDs from Defend Lines
            foreach (DefendLine item in defendLines)
            {
               
                int id_1 = item.GetDefendableOwner().GetComponent<PlayerHandCard>().thisCardData.id;
                int id_2 = item.GetAttackingOwnder().GetComponent<PlayerHandCard>().thisCardData.id;

                DefendPair defendPair = new DefendPair(id_1, id_2);

                defendInformation.defendLines.Add(defendPair);
            }

            //convert the Defend Info to string and send it to the other player
            string defendString = JsonUtility.ToJson(defendInformation);

            localCardPlayer.CmdDefendLinesConfirmed(defendString);
            #endregion

            AttackManager attackManager = FindObjectOfType<AttackManager>();
            attackManager.ExecuteCombatForDefendingPlayer(defendInformation);
        }
        else
        {
            TurnOffAllActionButtons();
            playerInsteraction.EnemyAttacks();
        }

       
    }

    #endregion

    #region general
    private void TurnOffAllActionButtons()
    {
        for (int i = 0; i < actionUIButtons.Length; i++)
        {
            actionUIButtons[i].gameObject.SetActive(false);
        }
    }

    private void CardPlacedOnCenterDeck()
    {
        if (playerInsteraction.IsAttackPossible() == false)
        {
            ShowFinishTurnUI();
        }
    }
    #endregion

    #region Online
    private void SetOpponentPlayer(CardPlayer cardPlayer)
    {
        opponentCardPlayer = cardPlayer;

        opponentCardPlayer.OnlineSwitchTurnEvent += OnEndTurnPressed;
    }

    private void SetLocalPlayer(CardPlayer cardPlayer)
    {
        localCardPlayer = cardPlayer;
    }

    private void OnEndTurnPressed()
    {
        Debug.Log("Opponent pressed Finish turn ");
            //finishTurnButton.gameObject.SetActive(false);
        TurnManager.FinishedTurn?.Invoke(Turn.ENEMY);
    }

    private void OnCardsSelectedForAttack()
    {

    }

    #endregion

    #region GameOver
    private void LocalPlayerDied()
    {
        gameOverText.text = "DEFEATED!";
        gameOverParent.SetActive(true);
    }

    private void EnemyPlayerDied()
    {
        gameOverText.text = "YOU WIN!";
        gameOverParent.SetActive(true);
    }
    #endregion
}
