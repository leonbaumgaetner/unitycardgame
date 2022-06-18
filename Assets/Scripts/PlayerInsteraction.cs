using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
public class PlayerInsteraction : MonoBehaviour
{

    public static UnityAction<GameObject> OnCardSelected;
    public static UnityAction<GameObject> OnCardDeSelected;

    public static UnityAction<GameObject> OnCardFollow; 
    public static UnityAction<Vector3,Vector3,GameObject> StopFollow;

    public static UnityAction<DeckType, GameObject, PLAYER_TYPE> ChangeCardDeck;
    public static UnityAction<GameObject, bool> SetHighlightCard;
    public UnityAction OnCardPlacedOnCenterDeck; 

    public GameObject cardInteracted_selected;
    public GameObject cardInteracted_selected_previous;

    private Vector3 cardInitialPosition_current;
    private Vector3 cardInitialRotation;

    private bool followMouse = false;
    private GameObject cardFollower;

    public LayerMask backWall;
    public Camera otherCamera;

    public Deck[] AllDecks;
   // private Dictionary<DeckType, Deck> DecksAndKeys = new Dictionary<DeckType, Deck>();
    private Dictionary<DeckType, Deck> DecksAndKeysPlayer = new Dictionary<DeckType, Deck>();
    private Dictionary<DeckType, Deck> DecksAndKeysEnemy = new Dictionary<DeckType, Deck>();

    public GameObject highlightCard; //this card shows the selected card in larger view

    public GameObject blueEffect;

    private CardPlayer localPlayer;
    private void Start()
    {
        OnCardSelected += CardSelected;
        OnCardDeSelected += CardDeSelected;
        OnCardFollow += StartFollow;
        StopFollow += StopFollowing;

        AllDecks = FindObjectsOfType<Deck>();

        foreach (var item in AllDecks)
        {
            if (item.thisPlayerType == PLAYER_TYPE.ENEMY)
                DecksAndKeysEnemy.Add(item.deckType, item);
           else
                DecksAndKeysPlayer.Add(item.deckType, item);
        }

        ChangeCardDeck += MoveCardToDeck;
        SetHighlightCard += HighlightCardSet;
        GameManager.OnLocalCardSet += SetLocalPlayer;
    }

    private void StopFollowing(Vector3 s, Vector3 e,GameObject go)
    {
        followMouse = false;
       
        if(go.GetComponent<CardObject>().thisPlayerType == PLAYER_TYPE.LOCAL && TurnManager.isPlayerTurn)
        {
            if (Vector3.Distance(go.transform.position, DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.position) > 50)
            {


                DecideCardDeck(go);

            }
            else
            {
                go.transform.DOLocalMove(s, 0.12f).OnComplete(()=> { PlayerHandDeck.ArrangeHand?.Invoke(); });
                go.transform.DOLocalRotate(e, 0.12f);
            }
        }
        else
        {
            go.transform.DOLocalMove(s, 0.12f).OnComplete(() => { PlayerHandDeck.ArrangeHand?.Invoke(); });
            go.transform.DOLocalRotate(e, 0.12f);
        }

        //check card
       // Debug.Log(Vector3.Distance(go.transform.position, DecksAndKeys[DeckType.CENTER_DECK].transform.position));
      
       



    }
    public bool testingHandToCenter = false;
    public bool testingHandToLeftDeck = false;
    //when a card is dropped on the board
    private void DecideCardDeck(GameObject card)
    {        

        CardObject cardObject = card.GetComponent<CardObject>();

        if (cardObject)
        {

            if (cardObject.thisCardsDeck == DeckType.PLAYER_HAND)
            {
                if (cardObject.thisCardKey == Card_Key.MINISTER)
                {

                    //check for resource availibity

                    int rc = DecksAndKeysPlayer[DeckType.LEFT_DECK].GetComponent<LeftDeck>().GetTotalResources();

                    //check this cards resource count
                    int hc = cardObject.hireCost;

                    if(hc <= rc) //resource available
                    {
                        //sending data online
                        Card c = card.GetComponent<PlayerHandCard>().thisCardData;
                        string cardData = JsonUtility.ToJson(c);
                        GameManager.MinisterCardPlayed?.Invoke(cardData);


                        cardObject.thisCardsDeck = DeckType.RIGHT_SIDE;
                        MoveCardToDeck(DeckType.RIGHT_SIDE, card);
                      
                        //show particle effects

                        for (int i = 0; i < hc; i++)
                        {

                            Transform spawnPoint = DecksAndKeysPlayer[DeckType.LEFT_DECK].transform;
                            GameObject newEffectBall = Instantiate(blueEffect, spawnPoint.position, Quaternion.identity);
                            Transform targetPoint = DecksAndKeysPlayer[DeckType.RIGHT_SIDE].transform;
                            newEffectBall.transform.DOMove(targetPoint.position, i + 1).OnComplete(() => {

                                Destroy(newEffectBall);
                                DecksAndKeysPlayer[DeckType.LEFT_DECK].GetComponent<LeftDeck>().DepleteCards(1);
                            });
                        }
                       
                        





                        DOVirtual.DelayedCall(3.5f, () => {
                            card.GetComponent<CardObject>().thisCardsDeck = DeckType.CENTER_DECK;
                            MoveCardToDeck(DeckType.CENTER_DECK, card);
                        });

                        DOVirtual.DelayedCall(4, () => {

                            OnCardPlacedOnCenterDeck?.Invoke();
                        });
                    }
                    else //resource not available
                    {
                        DOVirtual.DelayedCall(2.5f, () => {
                            
                            MoveCardToDeck(DeckType.PLAYER_HAND, card);
                          //  PlayerHandDeck.ArrangeHand.Invoke();
                        });
                    }



                    
                }
                else if (cardObject.thisCardKey == Card_Key.RESOURSE)
                {
                    //only one resource per turn
                   if( DecksAndKeysPlayer[DeckType.LEFT_DECK].GetComponent<LeftDeck>().IsResourceAdded())
                    {
                        MoveCardToDeck(DeckType.PLAYER_HAND, card);
                        cardObject.thisCardsDeck = DeckType.PLAYER_HAND;
                    }
                   else
                    {
                        cardObject.thisCardsDeck = DeckType.LEFT_DECK;
                        MoveCardToDeck(DeckType.LEFT_DECK, card);
                        
                        
                        //sending data online
                        if(GameManager.isOnline)
                        {
                            Card c = card.GetComponent<PlayerHandCard>().thisCardData;
                            string cardData = JsonUtility.ToJson(c);
                            GameManager.ResourceCardPlayed?.Invoke(cardData);
                        }
                       
                    }
                   
                  //  PlayerHandDeck.ArrangeHand.Invoke();
                }
                else if(cardObject.thisCardKey == Card_Key.ABILITY)
                {
                    cardObject.thisCardsDeck = DeckType.RIGHT_SIDE;
                    MoveCardToDeck(DeckType.RIGHT_SIDE, card);

                    //DOVirtual.DelayedCall(3.5f, () => {
                    //    card.GetComponent<CardObject>().thisCardsDeck = DeckType.CENTER_DECK;
                    //    MoveCardToDeck(DeckType.CENTER_DECK, card);
                    //});
                    //sending data online
                    if (GameManager.isOnline)
                    {
                        Card c = card.GetComponent<PlayerHandCard>().thisCardData;
                        string cardData = JsonUtility.ToJson(c);

                        GameManager.AbilityCardPlayed?.Invoke(cardData);
                    }
                    


                    //check if enemy card is there in center deck
                    //int cc = DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.childCount;
                    //if(cc > 0)
                    //{
                        
                    //}
                    //else
                    //{
                    //    //return the card to the players hand
                    //    MoveCardToDeck(DeckType.PLAYER_HAND, card);
                    //    cardObject.thisCardsDeck = DeckType.PLAYER_HAND;
                    //}

                    
                }
            }
            
        }


    }

    public Deck GetPlayerDeck(DeckType deckType)
    {
        return DecksAndKeysPlayer[deckType];
    }

    public Deck GetEnemyDeck(DeckType deckType)
    {
        return DecksAndKeysEnemy[deckType];
    }

    public void MoveCardToDeck(DeckType deckType, GameObject card, PLAYER_TYPE pt = PLAYER_TYPE.LOCAL)
    {
        if(pt == PLAYER_TYPE.LOCAL)
        {
            switch (deckType)
            {
                case DeckType.PLAYER_HAND:
                    DecksAndKeysPlayer[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().AddCardToPlayerHand(card);
                  
                    break;
                case DeckType.CENTER_DECK:

                    DecksAndKeysPlayer[DeckType.CENTER_DECK].GetComponent<CenterDeck>().AddACardToDeck(card);
                  //  DecksAndKeysPlayer[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck();
                    card.GetComponent<PlayingCard>().SetCardState?.Invoke(false);

                    break;
                case DeckType.RIGHT_SIDE:
                    UnityAction newCallback = DecksAndKeysPlayer[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck;
                    DecksAndKeysPlayer[DeckType.RIGHT_SIDE].GetComponent<RightDeck>().AddACard(card, newCallback);
                    
                    //DOVirtual.DelayedCall(0.1f, () =>
                    //{
                    //    DecksAndKeysPlayer[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck();
                    //});
                    break;
                case DeckType.LEFT_DECK:
                    UnityAction newCallback2 = DecksAndKeysPlayer[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck;
                    DecksAndKeysPlayer[DeckType.LEFT_DECK].GetComponent<LeftDeck>().AddToDeck(card, newCallback2);
                   
                    
                    break;
                case DeckType.MAIN_DECK:
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (deckType)
            {
                case DeckType.PLAYER_HAND:
                    DecksAndKeysEnemy[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().AddCardToPlayerHand(card);
                   // DecksAndKeysEnemy[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck();
                    DOVirtual.DelayedCall(2, () =>
                    {
                        DecksAndKeysEnemy[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck();
                    });
                    break;
                case DeckType.CENTER_DECK:
                    DecksAndKeysEnemy[DeckType.CENTER_DECK].GetComponent<CenterDeck>().AddACardToDeck(card);
                    card.GetComponent<PlayingCard>().SetCardState?.Invoke(false);
                  
                    break;
                case DeckType.RIGHT_SIDE:
                    UnityAction newCallback = DecksAndKeysEnemy[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck;
                    DecksAndKeysEnemy[DeckType.RIGHT_SIDE].GetComponent<RightDeck>().AddACard(card, newCallback);
                    //DOVirtual.DelayedCall(2, () =>
                    //{
                    //    DecksAndKeysEnemy[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck();
                    //});
                    break;
                case DeckType.LEFT_DECK:
                    UnityAction newCallback3 = DecksAndKeysEnemy[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck;
                    DecksAndKeysEnemy[DeckType.LEFT_DECK].GetComponent<LeftDeck>().AddToDeck(card, newCallback3);
                   
                    break;
                case DeckType.MAIN_DECK:
                    break;
                default:
                    break;
            }
        }
       
    }

   

    private void StartFollow(GameObject go)
    {
        cardFollower = go;
        followMouse = true;
    }
    
    private void CardSelected(GameObject cardSelected)
    {
        cardInteracted_selected = cardSelected;
        EnlargeHandCards();
    }

    private void CardDeSelected(GameObject cardDeselected)
    {
        cardInteracted_selected_previous = cardInteracted_selected;
        ReplaceHandCard();
    }

    private void EnlargeHandCards()
    {
        cardInitialPosition_current = cardInteracted_selected.transform.position;
        cardInteracted_selected.transform.DOLocalMove(new Vector3(cardInteracted_selected.transform.position.x, -60, -83), 0.2f);       
    }

    private void ReplaceHandCard()
    {
        cardInteracted_selected_previous.transform.DOLocalMove(cardInitialPosition_current,0.2f);
    }


    public void Update()
    {
        if(followMouse)
        {
            //Vector3 mouseWorldPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            //mouseWorldPos.z = -90;
            //cardFollower.transform.localPosition = mouseWorldPos;
            
            
            Ray ray = otherCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, backWall))
            {
               // Debug.DrawRay(ray.origin, hit.point, Color.red);
              //  Debug.Log(hit.collider.name);

                Vector3 followPoint = hit.point;


                followPoint.z -= 20;

                cardFollower.transform.position = followPoint;
            }
        }
    }


    public float highlightCardOffset = 20;

    //highlight card
    public void HighlightCardSet(GameObject go, bool state)
    {
        if(go.GetComponent<PlayingCard>().isFlipped == false)
        {
            return;
        }

        if(state)
        {
            highlightCard.GetComponent<CardObject>().LoadCardData(go.GetComponent<CardObject>().thisCardData);

            highlightCard.SetActive(state);
            highlightCard.transform.position = go.transform.position + new Vector3(highlightCardOffset, 0, -5);
            
        }
        else
        {
            highlightCard.SetActive(false);
        }
      
    }




    //discard
    public void DiscardCard()
    {
        // DecksAndKeys[DeckType.DISCARD_DECK].GetComponent<>

        int t = DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.childCount;

        int random = Random.Range(0, t);

        GameObject go = DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.GetChild(random).gameObject;

       // DiscardDeck.DiscardPileCard?.Invoke(go);


    }

    

    //Attack
    public void ActiveAttackableCards(UnityAction callback)
    {
       
        int t = DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.childCount;

        List<Transform> attackableCards = new List<Transform>();


        for (int i = 0; i < t; i++)
        {

            if (DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayerHandCard>().isActive == true)
            {
                DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayingCard>().SetCardGlow?.Invoke(true);

                attackableCards.Add(DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.GetChild(i));
            }

        }

        AttackManager.AttackCards?.Invoke(attackableCards, callback);
        
    }

    

    public void ActivateAttackablesEnemyCards()
    {
        int t = DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.childCount;

        List<Transform> attackableCards = new List<Transform>();

        for (int i = 0; i < t; i++)
        {

            if (DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayerHandCard>().isActive == true)
            {
                Debug.Log("enemy card setting glow");
                DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayingCard>().SetCardGlow?.Invoke(true);

                attackableCards.Add(DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.GetChild(i));
            }

        }

        GameManager.CurrentGameMode?.Invoke(GAME_MODE.DEFENDING);
    }

    public void EnemyAttacks()
    {
        int t = DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.childCount;

        List<Transform> attackableCards = new List<Transform>();

        for (int i = 0; i < t; i++)
        {

            if (DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayerHandCard>().isActive == true)
            {
                //Debug.Log("enemy card setting glow");
              //  DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayingCard>().SetCardGlow?.Invoke(true);

                attackableCards.Add(DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.GetChild(i));
            }

        }

        AttackManager.AttackCardsSelected?.Invoke(attackableCards, OnEnemyFinishedAttacking);
    }

    private void OnEnemyFinishedAttacking()
    {
        TurnManager.FinishedTurn?.Invoke(Turn.ENEMY);
        TurnOffGlowOpponent();
    }

    public void ManualActivateAttack()
    {
        GameManager.CurrentGameMode?.Invoke(GAME_MODE.ATTACKING);


        int t = DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.childCount;

        


        for (int i = 0; i < t; i++)
        {
            if(DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayerHandCard>().isActive == false)
                DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayingCard>().SetCardGlow?.Invoke(true);
       
        }

       // AttackManager.AttackCardsSelected?.Invoke(attackableCards);
    }

    //Enemy Plays a Resource Card if available in the hand
    public void EnemyPlayResourceCard()
    {
        //check player's hand 
        Transform t = DecksAndKeysEnemy[DeckType.PLAYER_HAND].transform;
        List<Transform> resourceCards = new List<Transform>();
        foreach (Transform item in t)
        {
            if(item.gameObject.GetComponent<CardObject>().thisCardKey == Card_Key.RESOURSE)
            {
                resourceCards.Add(item);
            }
        }
        int r = Random.Range(0, resourceCards.Count);
       
        if (resourceCards.Count > 0)
        {          
            MoveCardToDeck(DeckType.LEFT_DECK, resourceCards[r].gameObject, PLAYER_TYPE.ENEMY);
            DecksAndKeysEnemy[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck();
        }

       

    }


    //Enemy Plays a random minister card
    public void EnemyPlayMinisterCard()
    {
        Transform t = DecksAndKeysEnemy[DeckType.PLAYER_HAND].transform;
        List<Transform> attackCards = new List<Transform>();

        foreach (Transform item in t)
        {
            if (item.gameObject.GetComponent<CardObject>().thisCardKey == Card_Key.MINISTER)
            {
                attackCards.Add(item);
            }
        }

        int r = Random.Range(0, attackCards.Count);

        if (attackCards.Count > 0)
        {
            MoveCardToDeck(DeckType.RIGHT_SIDE, attackCards[r].gameObject, PLAYER_TYPE.ENEMY);
            
        }
       
        //find resource requirements for current card
        GameObject selectedCard = attackCards[r].gameObject;
        int hc = attackCards[r].GetComponent<CardObject>().hireCost;

        //effect
        for (int i = 0; i < hc; i++)
        {
            Transform spawnPoint = DecksAndKeysEnemy[DeckType.LEFT_DECK].transform;
            GameObject newEffectBall = Instantiate(blueEffect, spawnPoint.position, Quaternion.identity);
            Transform targetPoint = DecksAndKeysEnemy[DeckType.RIGHT_SIDE].transform;
            newEffectBall.transform.DOMove(targetPoint.position, i + 1).OnComplete(() => {

                Destroy(newEffectBall);
                DecksAndKeysEnemy[DeckType.LEFT_DECK].GetComponent<LeftDeck>().DepleteCards(1);
            });
        }
        
        //move card to center deck
        DOVirtual.DelayedCall(3.5f, () =>
        {
            Debug.Log("R value: " + r);
            MoveCardToDeck(DeckType.CENTER_DECK, attackCards[r].gameObject, PLAYER_TYPE.ENEMY);
          //  DecksAndKeysEnemy[DeckType.PLAYER_HAND].GetComponent<PlayerHandDeck>().ArrangeCardsInDeck();

        });
    }

    public bool IsAttackPossible()
    {
        int t = DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.childCount;

        if (t == 0)
            return false;
        for (int i = 0; i < t; i++)
        {
            if (DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayerHandCard>().isActive == true)
                return true;           
        }

        return false;
    }

    public void TurnOffGlowLocal()
    {
        int t = DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.childCount;
        if (t == 0)
            return;
        for (int i = 0; i < t; i++)
        {
            DecksAndKeysPlayer[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayingCard>().SetCardGlow?.Invoke(false);               
        }
    }

    public void TurnOffGlowOpponent()
    {
        int t = DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.childCount;
        if (t == 0)
            return;
        for (int i = 0; i < t; i++)
        {
            DecksAndKeysEnemy[DeckType.CENTER_DECK].transform.GetChild(i).GetComponent<PlayingCard>().SetCardGlow?.Invoke(false);
        }
    }

    #region Online
    private void SetLocalPlayer(CardPlayer cardPlayer)
    {
        localPlayer = cardPlayer;
    }

    public void ActivateAllAttackableOnline()
    {
        List<Transform> selectedAttackCards = new List<Transform>();

        Transform deckParent = DecksAndKeysPlayer[DeckType.CENTER_DECK].transform;
        foreach (Transform item in deckParent)
        {
            if (item.GetComponent<PlayerHandCard>().isActive)
            {
                item.GetComponent<PlayingCard>().SetCardGlow?.Invoke(true);
                selectedAttackCards.Add(item);
            }

        }

        //find the index of the attacking cards

        //holds the index of attack cards selected

        List<int> selectCardIndexes = new List<int>();

        for (int i = 0; i < selectedAttackCards.Count; i++)
        {
            //for (int j = 0; j < deckParent.childCount; j++)
            //{
            //    if (deckParent.GetChild(j) == selectedAttackCards[i])
            //    {
            //        selectCardIndexes.Add(j);
            //    }
            //}

            selectCardIndexes.Add(selectedAttackCards[i].GetComponent<PlayerHandCard>().thisCardData.id);
            Debug.Log("sending card indexes " + selectedAttackCards[i].GetComponent<PlayerHandCard>().thisCardData.id);
        }

        localPlayer.CmdOnCardsSelectedForAttack(selectCardIndexes);


        if(IsDefendPossible() == false)
        {
            AttackManager.ExecuteAllAttackWithoutDefenceAction?.Invoke();
        }       

    }

    private bool IsDefendPossible()
    {
        Transform p = DecksAndKeysEnemy[DeckType.CENTER_DECK].transform;

        foreach (Transform item in p)
        {
            if (item.GetComponent<PlayerHandCard>().isActive)
                return true;
        }

        return false;
    }
    #endregion
}
