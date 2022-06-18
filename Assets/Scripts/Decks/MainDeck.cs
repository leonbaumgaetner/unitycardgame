using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

//distributes cards to the player
public class MainDeck : Deck
{
    public GameObject cardToGive;

    public UnityAction<Turn> DrawCardPlayer;

    AttackCardsGroup attackCardsGroup;
    ResourceCardsGroup resourceCardsGroup;
    [SerializeField]
    private PlayerHandDeck playerHandDeck;

    public GameObject blueEffect;
    [SerializeField]
    private Turn thisTurn;

    CardPlayer localCardPlayer;

    public override void Start()
    {

        base.Start();
        GameManager.OnLocalCardSet += SetLocalPlayer;
        DrawCardPlayer += GiveACardToPlayer;
        TurnManager.OnTurnChangedTo += TurnChanged;
        //Invoke("DrawSomeCards", 3);


    }

    private void OnDestroy()
    {
        GameManager.OnLocalCardSet -= SetLocalPlayer;
        DrawCardPlayer -= GiveACardToPlayer;
        TurnManager.OnTurnChangedTo -= TurnChanged;
    }
    void TurnChanged(Turn whoseTurn)
    {
        if(GameManager.isOnline)
        {
            if (thisPlayerType == PLAYER_TYPE.LOCAL && whoseTurn == Turn.LOCAL)
            {
                GiveACardToPlayer(whoseTurn);
            }
            else if (thisPlayerType == PLAYER_TYPE.ENEMY && whoseTurn == Turn.ENEMY)
            {
                GiveAFakeCardToEnemy();
            }

        }
        else
        {
            if (thisPlayerType == PLAYER_TYPE.LOCAL && whoseTurn == Turn.LOCAL)
            {
                GiveACardToPlayer(whoseTurn);
            }
            else if (thisPlayerType == PLAYER_TYPE.ENEMY && whoseTurn == Turn.ENEMY)
            {
                GiveACardToPlayer(whoseTurn);
            }
        }        
    }

    public void LoadMainAttackDeck(AttackCardsGroup attackCardsGroup)
    {
        this.attackCardsGroup = attackCardsGroup;
    }

    public void LoadMainResourceDeck(ResourceCardsGroup resourceCardsGroup)
    {
        this.resourceCardsGroup = resourceCardsGroup;
    }

    public void LoadMainDeck(AttackCardsGroup ag, ResourceCardsGroup rg)
    {
        this.attackCardsGroup = ag;
        this.resourceCardsGroup = rg;
        GameManager.FetchPlayers?.Invoke();
        GiveTheseCardsToPlayer();
    }

    private void DrawSomeCards()
    {
        for (int i = 0; i < 6; i++)
        {
            GiveACardToPlayer(thisTurn);
            
        }

    }

    private void GiveTheseCardsToPlayer()
    {
       
       
            for (int i = 0; i < attackCardsGroup.attackCards.Count; i++) //4
            {
                GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
                newCard.GetComponent<PlayerHandCard>().LoadCardData(attackCardsGroup.attackCards[i]);
                newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;
                // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);
                PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, thisPlayerType);
                newCard.GetComponent<PlayerHandCard>().thisPlayerType = thisPlayerType;
                newCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);
                newCard.GetComponent<PlayingCard>().ShowCard(true);

                newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
                GameManager.cardID++;
             }

            for (int i = 0; i < resourceCardsGroup.resourceCards.Count; i++) //3
            {
                GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
                newCard.GetComponent<PlayerHandCard>().LoadCardData(resourceCardsGroup.resourceCards[i]);
                newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;

                PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, thisPlayerType);
                newCard.GetComponent<PlayerHandCard>().thisPlayerType = thisPlayerType;
                newCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);
                newCard.GetComponent<PlayingCard>().ShowCard(true);

                newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
                GameManager.cardID++;
        }


            //fake cards to the enemy
        for (int i = 0; i < 7; i++)
        {
            GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
            newCard.GetComponent<PlayerHandCard>().thisPlayerType = PLAYER_TYPE.ENEMY;
            newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;
            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.ENEMY);           
            newCard.GetComponent<PlayingCard>().ShowCard(false);
        }



    }



    public void GiveACardToPlayer(Turn turn = Turn.LOCAL)
    {
       // Debug.LogError("Give card to player: "+ turn.ToString());
        if(turn == Turn.LOCAL)
        {
            int r = Random.Range(0, 2);

            if (r == 0) //load resource card
            {
                int index = Random.Range(0, resourceCardsGroup.resourceCards.Count);
                GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
                newCard.GetComponent<PlayerHandCard>().LoadCardData(resourceCardsGroup.resourceCards[index]);

                newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;
               // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);
                PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
                newCard.GetComponent<PlayerHandCard>().thisPlayerType = thisPlayerType;
                newCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);
                
                newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
                GameManager.cardID++;
            }
            else if (r == 1)//load attack card
            {
                int index = Random.Range(0, attackCardsGroup.attackCards.Count);


                //Duplice a card
                GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
                newCard.GetComponent<PlayerHandCard>().LoadCardData(attackCardsGroup.attackCards[index]);

                newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;
                // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);
                PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
                newCard.GetComponent<PlayerHandCard>().thisPlayerType = thisPlayerType;
                newCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);
                
                newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
                GameManager.cardID++;
            }
            else if (r == 2)
            {
                GiveAfflictCard();
            }
            else if (r == 3)
            {
                GiveRallyCard();
            }
            else if (r == 4)
            {
                GiveEnhanceCard();
            }
            else if(r == 5)
            {
                GiveDelayCardToPlayer();
            }
        }
        else
        {
            int r = Random.Range(0, 3);

            if (r == 0) //load resource card
            {
                int index = Random.Range(0, resourceCardsGroup.resourceCards.Count);
                GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
                newCard.GetComponent<PlayerHandCard>().LoadCardData(resourceCardsGroup.resourceCards[index]);

                newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;
                 //playerHandDeck.SendCardToPlayerHand.Invoke(newCard);
                PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.ENEMY);
                newCard.GetComponent<PlayerHandCard>().thisPlayerType = thisPlayerType;
            }
            else //load attack card
            {
                int index = Random.Range(0, attackCardsGroup.attackCards.Count);


                //Duplice a card
                GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
                newCard.GetComponent<PlayerHandCard>().LoadCardData(attackCardsGroup.attackCards[index]);

                newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;
                 //playerHandDeck.SendCardToPlayerHand.Invoke(newCard);
                PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.ENEMY);
                newCard.GetComponent<PlayerHandCard>().thisPlayerType = thisPlayerType;
            }
        }

       
       
    }

    public void GiveAFakeCardToEnemy()
    {
        GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
        newCard.GetComponent<PlayerHandCard>().thisPlayerType = PLAYER_TYPE.ENEMY;
        newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;
        PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.ENEMY);
        newCard.GetComponent<PlayingCard>().ShowCard(false);
    }

    //afflict
    public void GiveAfflictCard()
    {
        if (GameManager.isOnline)
        {
            int index = Random.Range(0, attackCardsGroup.attackCards.Count);
            GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);

            PlayerHandCard playerHandCard = newCard.GetComponent<PlayerHandCard>();
            playerHandCard.LoadCardData(attackCardsGroup.attackCards[index]);

            playerHandCard.thisCardsDeck = DeckType.MAIN_DECK;
            //  playerHandDeck.SendCardToPlayerHand.Invoke(newCard);

            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
            playerHandCard.thisPlayerType = thisPlayerType;

            playerHandCard.thisCardData.thisCardAbility = AbilityType.AFFLICT;
            playerHandCard.thisCardKey = Card_Key.ABILITY;
            playerHandCard.cardTitle.text = "AFFLICT";

            newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
            GameManager.cardID++;

            localCardPlayer.CmdGiveEnemyFakeCard();

        }
        else
        {

            int index = Random.Range(0, attackCardsGroup.attackCards.Count);
            GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);

            PlayerHandCard playerHandCard = newCard.GetComponent<PlayerHandCard>();
            playerHandCard.LoadCardData(attackCardsGroup.attackCards[index]);

            playerHandCard.thisCardsDeck = DeckType.MAIN_DECK;
            //  playerHandDeck.SendCardToPlayerHand.Invoke(newCard);

            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
            playerHandCard.thisPlayerType = thisPlayerType;

            playerHandCard.thisCardData.thisCardAbility = AbilityType.AFFLICT;
            playerHandCard.thisCardKey = Card_Key.ABILITY;
            playerHandCard.cardTitle.text = "AFFLICT";
        }

    }
    //rally
    public void GiveRallyCard()
    {
        if(GameManager.isOnline)
        {
            int index = Random.Range(0, attackCardsGroup.attackCards.Count);
            GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);

            PlayerHandCard playerHandCard = newCard.GetComponent<PlayerHandCard>();
            playerHandCard.LoadCardData(attackCardsGroup.attackCards[index]);

            playerHandCard.thisCardsDeck = DeckType.MAIN_DECK;
            // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);

            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
            playerHandCard.thisPlayerType = thisPlayerType;

            playerHandCard.thisCardData.thisCardAbility = AbilityType.RALLY;
            playerHandCard.thisCardKey = Card_Key.ABILITY;
            playerHandCard.cardTitle.text = "RALLY";

            newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
            GameManager.cardID++;

            localCardPlayer.CmdGiveEnemyFakeCard();
        }
      
    }
    //enhance
    public void GiveEnhanceCard()
    {
        if(GameManager.isOnline)
        {
            int index = Random.Range(0, attackCardsGroup.attackCards.Count);
            GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);

            PlayerHandCard playerHandCard = newCard.GetComponent<PlayerHandCard>();
            playerHandCard.LoadCardData(attackCardsGroup.attackCards[index]);

            playerHandCard.thisCardsDeck = DeckType.MAIN_DECK;
            // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);

            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
            playerHandCard.thisPlayerType = thisPlayerType;

            playerHandCard.thisCardData.thisCardAbility = AbilityType.ENHANCE;
            playerHandCard.thisCardKey = Card_Key.ABILITY;
            playerHandCard.cardTitle.text = "ENHANCE";

            newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
            GameManager.cardID++;

            localCardPlayer.CmdGiveEnemyFakeCard();
        }
       
    }
    //delay
    public void GiveDelayCardToPlayer()
    {
        if(GameManager.isOnline)
        {
            int index = Random.Range(0, attackCardsGroup.attackCards.Count);
            GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);

            PlayerHandCard playerHandCard = newCard.GetComponent<PlayerHandCard>();
            playerHandCard.LoadCardData(attackCardsGroup.attackCards[index]);

            playerHandCard.thisCardsDeck = DeckType.MAIN_DECK;
            // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);

            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
            playerHandCard.thisPlayerType = thisPlayerType;

            playerHandCard.thisCardData.thisCardAbility = AbilityType.DELAY;
            playerHandCard.thisCardKey = Card_Key.ABILITY;
            playerHandCard.cardTitle.text = "DELAY";

            newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
            GameManager.cardID++;

            localCardPlayer.CmdGiveEnemyFakeCard();
        }
        
    }
    //diplomatic card
    public void GiveDiplomaticCardToPlayer()
    {
        if(GameManager.isOnline)
        {
            int index = Random.Range(0, attackCardsGroup.attackCards.Count);
            GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);

            PlayerHandCard playerHandCard = newCard.GetComponent<PlayerHandCard>();
            playerHandCard.LoadCardData(attackCardsGroup.attackCards[index]);

            playerHandCard.thisCardsDeck = DeckType.MAIN_DECK;
            // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);

            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
            playerHandCard.thisPlayerType = thisPlayerType;

            playerHandCard.thisCardData.thisCardAbility = AbilityType.DIPLOMATIC_IMMUNITY;
            playerHandCard.thisCardKey = Card_Key.ABILITY;
            playerHandCard.cardTitle.text = "DIPLOMATIC IMMUNITY";

            localCardPlayer.CmdGiveEnemyFakeCard();
        }
       

        
    }

    public void GiveSmearCard()
    {
        if (GameManager.isOnline)
        {
            int index = Random.Range(0, attackCardsGroup.attackCards.Count);
            GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);

            PlayerHandCard playerHandCard = newCard.GetComponent<PlayerHandCard>();
            playerHandCard.LoadCardData(attackCardsGroup.attackCards[index]);

            playerHandCard.thisCardsDeck = DeckType.MAIN_DECK;
            // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);

            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
            playerHandCard.thisPlayerType = thisPlayerType;

            playerHandCard.thisCardData.thisCardAbility = AbilityType.SMEAR;
            playerHandCard.thisCardKey = Card_Key.ABILITY;
            playerHandCard.cardTitle.text = "SMEAR";

            newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
            GameManager.cardID++;

            localCardPlayer.CmdGiveEnemyFakeCard();
        }
    }

    public void GiveGarrymander()
    {

    }

    public void GiveThisCardToPlayer(AbilityType abilityType)
    {
        int index = Random.Range(0, attackCardsGroup.attackCards.Count);
        GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);

        PlayerHandCard playerHandCard = newCard.GetComponent<PlayerHandCard>();
        playerHandCard.LoadCardData(attackCardsGroup.attackCards[index]);

        playerHandCard.thisCardsDeck = DeckType.MAIN_DECK;
        // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);

        PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
        playerHandCard.thisPlayerType = thisPlayerType;

        playerHandCard.thisCardData.thisCardAbility = abilityType;
        playerHandCard.thisCardKey = Card_Key.ABILITY;
        playerHandCard.cardTitle.text = abilityType.ToString();

        newCard.GetComponent<PlayerHandCard>().thisCardData.id = GameManager.cardID;
        GameManager.cardID++;

        localCardPlayer.CmdGiveEnemyFakeCard();
    }

    public void GiveResourceCardToPlayer()
    {
        int index = Random.Range(0, resourceCardsGroup.resourceCards.Count);
        GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
        newCard.GetComponent<PlayerHandCard>().LoadCardData(resourceCardsGroup.resourceCards[index]);

        newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;
        // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);
        PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
        newCard.GetComponent<PlayerHandCard>().thisPlayerType = PLAYER_TYPE.LOCAL;
        newCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);
    }

    public void GiveCabinetMinisterCardToPlayer()
    {
        int index = Random.Range(0, attackCardsGroup.attackCards.Count);


        //Duplice a card
        GameObject newCard = Instantiate(cardToGive, cardToGive.transform.position, cardToGive.transform.rotation);
        newCard.GetComponent<PlayerHandCard>().LoadCardData(attackCardsGroup.attackCards[index]);

        newCard.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.MAIN_DECK;
        // playerHandDeck.SendCardToPlayerHand.Invoke(newCard);
        PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, newCard, PLAYER_TYPE.LOCAL);
        newCard.GetComponent<PlayerHandCard>().thisPlayerType = PLAYER_TYPE.LOCAL;
        newCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);
    }   

    #region OnlineCardFunction
    private void SetLocalPlayer(CardPlayer cardPlayer)
    {
        localCardPlayer = cardPlayer;
    }

    #endregion
}
