using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
public class DeckManager : MonoBehaviour
{

    public static UnityAction<string> PlayedResourceCard;
    public static UnityAction<string> PlayedMinisterCard;
    public static UnityAction<string> PlayedAbilityCard;



    public Dictionary<DeckType, Deck> DecksAndKeysPlayer = new Dictionary<DeckType, Deck>();
    public Dictionary<DeckType, Deck> DecksAndKeysEnemy = new Dictionary<DeckType, Deck>();

    public Deck[] AllDecks;

    public static UnityAction<string,string> LoadDeckCardsLocal;
    public static UnityAction<string, string> LoadDeckCardsEnemy;

    public GameObject blueEffect;


    public static UnityAction<List<int>> AttackCardsSelectedOnline;
    public static UnityAction<string> DefenseCardsSelectedOnline;

    public static UnityAction HandAFakeCard;
    public static UnityAction EnemyCancelledCard;

    public static UnityAction<int,string,string> AbilityTargetSelected;

    public PlayerInsteraction playerInsteraction;
    private AbilitiesManager abilitiesManager;
    public void Start()
    {
        AllDecks = FindObjectsOfType<Deck>();

        foreach (var item in AllDecks)
        {
            if (item.thisPlayerType == PLAYER_TYPE.LOCAL)
                DecksAndKeysPlayer.Add(item.deckType, item);
            else
                DecksAndKeysEnemy.Add(item.deckType, item);
        }

        abilitiesManager = FindObjectOfType<AbilitiesManager>();

        LoadDeckCardsLocal += LoadForLocal;
        PlayedResourceCard += PlayAResourceCard;
        PlayedMinisterCard += PlayAMinisterCard;
        


        AttackCardsSelectedOnline += OnAttackCardsSelectedOnline;
        DefenseCardsSelectedOnline += OnDefenseCardsSelectedOnline;

        HandAFakeCard += HandFakeCardToEnemy;
        EnemyCancelledCard += OnEnemyCancelledCard;

        PlayedAbilityCard += OnPlayedAbilityCard;
        AbilityTargetSelected += OnAbilityTargetSelected;
    }

    private void OnDestroy()
    {       
        LoadDeckCardsLocal -= LoadForLocal;
        PlayedResourceCard -= PlayAResourceCard;
        PlayedMinisterCard -= PlayAMinisterCard;
        PlayedAbilityCard  -= OnPlayedAbilityCard;

        AttackCardsSelectedOnline -= OnAttackCardsSelectedOnline;
        DefenseCardsSelectedOnline -= OnDefenseCardsSelectedOnline;

        HandAFakeCard -= HandFakeCardToEnemy;
        EnemyCancelledCard -= OnEnemyCancelledCard;
        AbilityTargetSelected -= OnAbilityTargetSelected;
    }

    public void LoadAttackCardDeck(AttackCardsGroup attackCardsGroup)
    {
       

        //foreach (var item in AllDecks)
        //{
        //    if (item.thisPlayerType == PLAYER_TYPE.LOCAL)
        //        DecksAndKeysPlayer.Add(item.deckType, item);
        //    else
        //        DecksAndKeysEnemy.Add(item.deckType, item);
        //}


        DecksAndKeysPlayer[DeckType.MAIN_DECK].GetComponent<MainDeck>().LoadMainAttackDeck(attackCardsGroup);
        DecksAndKeysEnemy[DeckType.MAIN_DECK].GetComponent<MainDeck>().LoadMainAttackDeck(attackCardsGroup);
    }

    public void LoadResourceCardDeck(ResourceCardsGroup resourceCardsGroup)
    {
        DecksAndKeysPlayer[DeckType.MAIN_DECK].GetComponent<MainDeck>().LoadMainResourceDeck(resourceCardsGroup);
        DecksAndKeysEnemy[DeckType.MAIN_DECK].GetComponent<MainDeck>().LoadMainResourceDeck(resourceCardsGroup);
    }

    private void LoadForLocal(string a, string r)
    {
        AttackCardsGroup attackCardsGroup = JsonUtility.FromJson<AttackCardsGroup>(a);
        ResourceCardsGroup resourceCardsGroup = JsonUtility.FromJson<ResourceCardsGroup>(r);
        DecksAndKeysPlayer[DeckType.MAIN_DECK].GetComponent<MainDeck>().LoadMainDeck(attackCardsGroup, resourceCardsGroup);
    }

    #region PlayedACardOnline
    private void PlayAResourceCard(string cardData)
    {
        Transform t = DecksAndKeysEnemy[DeckType.PLAYER_HAND].transform;        

        int cardIndex = Random.Range(0, t.childCount);

        Transform selectedCard = t.GetChild(cardIndex);

        //load the card with data
        ResourceCard resourceCard = JsonUtility.FromJson<ResourceCard>(cardData);

        selectedCard.GetComponent<PlayerHandCard>().LoadCardData(resourceCard);

        selectedCard.GetComponent<PlayingCard>().ShowCard(true);
        selectedCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);

        PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.LEFT_DECK, selectedCard.gameObject, PLAYER_TYPE.ENEMY);


    }

    //when opponent plays a minister card 
    private void PlayAMinisterCard(string cardData)
    {
        Transform t = DecksAndKeysEnemy[DeckType.PLAYER_HAND].transform;

        int cardIndex = Random.Range(0, t.childCount);

        Transform selectedCard = t.GetChild(cardIndex);

        //load the card with data
        AttackCard attackCard = JsonUtility.FromJson<AttackCard>(cardData);

        selectedCard.GetComponent<PlayerHandCard>().LoadCardData(attackCard);

        selectedCard.GetComponent<PlayingCard>().ShowCard(true);
        selectedCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);


        PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.RIGHT_SIDE, selectedCard.gameObject, PLAYER_TYPE.ENEMY);

        //find resource requirements for current card
        int hc = selectedCard.GetComponent<CardObject>().hireCost;

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
            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.CENTER_DECK, selectedCard.gameObject, PLAYER_TYPE.ENEMY);            

        });
    }
    
    //when oppoent plays a function card

    //names the card, places it on right side and shows resource consumption
    private void OnPlayedAbilityCard(string cardData)
    {
        Transform t = DecksAndKeysEnemy[DeckType.PLAYER_HAND].transform;

        int cardIndex = Random.Range(0, t.childCount);

        Transform selectedCard = t.GetChild(cardIndex);

        //load the card with data
        AttackCard attackCard = JsonUtility.FromJson<AttackCard>(cardData);

        selectedCard.GetComponent<PlayerHandCard>().LoadCardData(attackCard);

        selectedCard.GetComponent<PlayerHandCard>().cardTitle.text = attackCard.thisCardAbility.ToString();

        selectedCard.GetComponent<PlayingCard>().ShowCard(true);
        selectedCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);


        PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.RIGHT_SIDE, selectedCard.gameObject, PLAYER_TYPE.ENEMY);

        //find resource requirements for current card
        int hc = selectedCard.GetComponent<CardObject>().hireCost;

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

        ProcessAbility(attackCard.thisCardAbility);
    }

    private void ProcessAbility(AbilityType abilityType)
    {
        switch (abilityType)
        {
            case AbilityType.NONE:
                break;
            case AbilityType.AFFLICT:
                break;
            case AbilityType.RALLY:
                abilitiesManager.PlayedRallyCard();
                break;
            case AbilityType.ENHANCE:
                break;
            case AbilityType.DELAY:
                abilitiesManager.PlayedDelayCard();
                break;
            case AbilityType.DIPLOMATIC_IMMUNITY:
                abilitiesManager.PlayedDiplomaticCard();
                break;
            case AbilityType.GERRYMANDER:
                abilitiesManager.PlayedGerrymender();
                break;
            case AbilityType.RECOUNT:
                break;
            case AbilityType.RECALL:                
                break;
            case AbilityType.MANIPULATE:
                break;
            case AbilityType.DONATE:
                break;
            case AbilityType.PATSY:
                break;
            case AbilityType.CHARGE:
                break;
            case AbilityType.DEBATE:
                break;
            case AbilityType.SMEAR:
                break;
            default:
                break;
        }
    }

    #endregion

    #region Online
    private void OnAttackCardsSelectedOnline(List<int> cardIndex)
    {
        Transform t = DecksAndKeysEnemy[DeckType.CENTER_DECK].transform;
        AttackManager.AttackCardsSelectedOnline?.Invoke(t, cardIndex);
    }

    private void OnDefenseCardsSelectedOnline(string s)
    {
        Transform enemyParent = DecksAndKeysEnemy[DeckType.CENTER_DECK].transform;
        Transform playerParent = DecksAndKeysPlayer[DeckType.CENTER_DECK].transform;

        AttackManager.DefenseCardsSelectedOnline?.Invoke(enemyParent, playerParent,s);
    }

    private void HandFakeCardToEnemy()
    {
        DecksAndKeysEnemy[DeckType.MAIN_DECK].GetComponent<MainDeck>().GiveAFakeCardToEnemy();
    }

    private void OnEnemyCancelledCard()
    {
        Transform cardInDeck = DecksAndKeysEnemy[DeckType.RIGHT_SIDE].GetComponentInChildren<PlayerHandCard>().transform;
        if (cardInDeck == null)
        {
            Debug.LogError("No Child in enemy deck");
        }
        else
        {
            playerInsteraction.MoveCardToDeck(DeckType.PLAYER_HAND, cardInDeck.gameObject, PLAYER_TYPE.ENEMY);
            cardInDeck.GetComponent<PlayingCard>().ShowCard(false);
        }
    }

    private void OnAbilityTargetSelected(int id, string targetCardPlayerType, string abilityType)
    {
        PLAYER_TYPE targetPlayerType = (PLAYER_TYPE)System.Enum.Parse(typeof(PLAYER_TYPE), targetCardPlayerType);

        AbilityType typeOfAbility = (AbilityType)System.Enum.Parse(typeof(AbilityType), abilityType);

        Transform card = GetCardWithID(targetPlayerType, id);

        if (typeOfAbility == AbilityType.AFFLICT)
        {
            abilitiesManager.TargetAfflictSelected(card, blueEffect);
        }
        else if(typeOfAbility == AbilityType.ENHANCE)
        {
            abilitiesManager.PlayedEnhanceTarget(card, blueEffect);
        }
        else if(typeOfAbility == AbilityType.DELAY)
        {
            abilitiesManager.PlayedDelayTarget(card, blueEffect);
        }
        else if(typeOfAbility == AbilityType.DIPLOMATIC_IMMUNITY)
        {
            abilitiesManager.PlayedDiplomaticTarget(card, blueEffect);
        }
        else if(typeOfAbility == AbilityType.RECALL)
        {
            abilitiesManager.PlayedRecall(card, blueEffect);
        }

        
       
        //if(card)
        //{
        //    Transform rightDeck = DecksAndKeysEnemy[DeckType.RIGHT_SIDE].transform;

        //    GameObject newBlueBall = Instantiate(blueEffect, rightDeck.position, Quaternion.identity);

        //    newBlueBall.transform.DOMove(card.transform.position, 1).OnComplete(() =>
        //    {
        //        Destroy(newBlueBall);
             
        //        PlayerHandCard playerHandCard = card.GetComponent<PlayerHandCard>();
        //        var cd = playerHandCard.thisCardData;
        //        AttackCard attackCard = cd as AttackCard;
               
        //        attackCard.attack -= 1;

        //        playerHandCard.LoadCardData(attackCard);

        //        //discard card from right deck
        //        GameObject newCard = rightDeck.GetComponentInChildren<PlayerHandCard>().gameObject;

        //        DecksAndKeysEnemy[DeckType.DISCARD_DECK].GetComponent<DiscardDeck>().DiscardACard(newCard.gameObject);
        //       // discardDeckLocal.DiscardACard(newCard);
        //    });

        //}
        //else
        //{
        //    Debug.LogError("Target card not found");
        //}
    }

    private Transform GetCardWithID(PLAYER_TYPE pLAYER_TYPE, int id)
    {
        switch (pLAYER_TYPE)
        {
            case PLAYER_TYPE.LOCAL:
                Transform playerDeckParent = DecksAndKeysPlayer[DeckType.CENTER_DECK].transform;

                foreach (Transform item in playerDeckParent)
                {
                    if (item.GetComponent<PlayerHandCard>().thisCardData.id == id)
                    {
                        return item;
                    }
                }
                return transform;
                
            case PLAYER_TYPE.ENEMY:
                Transform enemyDeckParent = DecksAndKeysEnemy[DeckType.CENTER_DECK].transform;

                foreach (Transform item in enemyDeckParent)
                {
                    if (item.GetComponent<PlayerHandCard>().thisCardData.id == id)
                    {
                        return item;
                    }
                }
                return transform;
               
            default:
                return transform;

        }
    }

    #endregion

    #region Abilities
    private void ProcessRallyCard()
    {

    }

    #endregion
}
