using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
public class AttackManager : MonoBehaviour
{
    public Transform avatar_opponent, avatar_local;

    public static UnityAction<List<Transform>,UnityAction> AttackCards;
    public static UnityAction<List<Transform>, UnityAction> AttackCardsSelected;


    public static UnityAction<AttackableCard, UnityAction<AttackLine>> LineForAttackableCard;
    public static UnityAction<AttackLine> FinishedSettingLine;

    public static UnityAction<Transform, List<int>> AttackCardsSelectedOnline;
    public static UnityAction<Transform,Transform,string> DefenseCardsSelectedOnline;

    public static UnityAction ExecuteAllAttackWithoutDefenceAction;

    public static UnityAction<string> ExecuteCombat;

    public static UnityAction localPlayerDead;
    public static UnityAction enemyPlayerDead;


    public AttackLine attackLine;

    public GameManager gameManager;

    public PlayerProfile enemyPlayer;

    private DeckManager deckManager;

    public HealthBox enemyHealthDeck;
    public HealthBox playerHealthDeck;

    public GameObject cardUpdate;

    private void Start()
    {
        AttackCards += AttackWithRandomCards;
        AttackCardsSelected += AttackWithSelectedCards;
        LineForAttackableCard += CreateLineForCard;
        FinishedSettingLine += OnLineSetFinished;

        AttackCardsSelectedOnline += OnAttackCardsSelectedOnline;
        DefenseCardsSelectedOnline += OnDefenseCardsSelectedOnline;


       deckManager = FindObjectOfType<DeckManager>();
       GameManager.OnLocalCardSet += SetLocalPlayer;
       GameManager.OnOpponentCardSet += SetEnemyPlayer;

        ExecuteAllAttackWithoutDefenceAction += ExecuteCombatForAllAttackLocalWithoutDefence;
    }

    private void OnDestroy()
    {
        DefenseCardsSelectedOnline -= OnDefenseCardsSelectedOnline;
        GameManager.OnLocalCardSet -= SetLocalPlayer;
        GameManager.OnOpponentCardSet -= SetEnemyPlayer;
        ExecuteAllAttackWithoutDefenceAction -= ExecuteCombatForAllAttackLocalWithoutDefence;
    }
    public void AttackWithRandomCards(List<Transform> t, UnityAction callback)
    {       
        StartCoroutine(AttackWithDelay(t,callback));
    }

    public void AttackWithSelectedCards(List<Transform> t, UnityAction callback)
    {
        StartCoroutine(AttackSelectedWithDelay(t, callback));
    }

    public void ExecuteCombatForDefendingPlayer(DefendInformation defendInformation)
    {
        StartCoroutine(ExecuteCombatForDefendingPlayerDelayed(defendInformation));

    }

    private void ExecuteCombatForAttackingPlayer(DefendInformation defendInformation)
    {
        StartCoroutine(ExecuteCombatForAttackingPlayerDelayed(defendInformation));
    }
    //attacking enemy
    private IEnumerator ExecuteCombatForAttackingPlayerDelayed(DefendInformation defendInformation)
    {
        AttackLine[] attackLines = FindObjectsOfType<AttackLine>();

        List<Transform> attackCards = new List<Transform>();

        if(attackLines.Length == 0)
        {
            Transform playerDeckParent = deckManager.DecksAndKeysPlayer[DeckType.CENTER_DECK].transform;

            foreach (Transform item in playerDeckParent)
            {
                if(item.GetComponent<PlayingCard>().IsCardDelpleted == false)
                {
                    attackCards.Add(item);
                }
            }
        }

        foreach (AttackLine item in attackLines)
        {
            attackCards.Add(item.GetOwner());
        }
        foreach (var item in attackLines)
        {
            Destroy(item.gameObject);
        }

        foreach (Transform item in attackCards)
        {
           
                int otherCardId = item.GetComponent<PlayerHandCard>().thisCardData.id;


                //check if this card is being defended by opponent
                if (IsBeingDefended(otherCardId, defendInformation))
                {
                    Transform targetCard = GetDefendingCard(PLAYER_TYPE.ENEMY, otherCardId, defendInformation);
                    targetCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);
                    yield return new WaitForSeconds(1.5f);
                    //attack the defending card
                    item.GetComponent<PlayerHandCard>().Attack(targetCard);

                    item.GetComponent<PlayingCard>().SetCardState?.Invoke(false);
                    yield return new WaitForSeconds(1.5f);
                    targetCard.GetComponent<PlayingCard>().SetCardState?.Invoke(false);
                }
                else
                {
                    //attack the player profile
                    item.GetComponent<PlayerHandCard>().Attack(avatar_opponent);
                    item.GetComponent<PlayingCard>().SetCardState?.Invoke(false);

                //deduct health
                int currentHealth = System.Convert.ToInt32(enemyHealthDeck.healthText.text);
                int healthToDeduct = item.GetComponent<PlayerHandCard>().attackPoint;
                currentHealth = currentHealth - healthToDeduct;
                enemyHealthDeck.healthText.text = ""+currentHealth;
                enemyHealthDeck.ShowTextDeduction(healthToDeduct);
                yield return new WaitForSeconds(1.5f);

            }


           
        }


        yield return new WaitForSeconds(3);
        localCardPlayer.CmdSwitchTurn();
        
        TurnManager.FinishedTurn?.Invoke(Turn.LOCAL);
    }

    //getting attacked
    private IEnumerator ExecuteCombatForDefendingPlayerDelayed(DefendInformation defendInformation)
    {
        //Get all the transform of cards that are attacking i.e. cards with glow
        Transform enemyDeckParent = deckManager.DecksAndKeysEnemy[DeckType.CENTER_DECK].transform;
        Transform playerDeckParent = deckManager.DecksAndKeysPlayer[DeckType.CENTER_DECK].transform;

        

        foreach (Transform item in enemyDeckParent)
        {
            DefendLine[] defendLines = FindObjectsOfType<DefendLine>();
            foreach (DefendLine d in defendLines)
            {
                Destroy(d.gameObject);
            }
            if (item.GetComponent<PlayingCard>().glow.activeSelf)
            {
                int otherCardId = item.GetComponent<PlayerHandCard>().thisCardData.id;


                //check if this card is being defended by opponent
                if (IsBeingDefended(otherCardId, defendInformation))
                {
                    Transform targetCard = GetDefendingCard(PLAYER_TYPE.LOCAL, otherCardId, defendInformation);
                    targetCard.GetComponent<PlayingCard>().SetCardState?.Invoke(true);
                    yield return new WaitForSeconds(1.5f);
                    //attack the defending card
                    item.GetComponent<PlayerHandCard>().Attack(targetCard);
                   
                    item.GetComponent<PlayingCard>().SetCardState?.Invoke(false);
                    yield return new WaitForSeconds(1.5f);
                    targetCard.GetComponent<PlayingCard>().SetCardState?.Invoke(false);

                    ShowTextEffect(targetCard.transform.position, "Def -1", Color.red);
                }
                else
                {
                    //attack the player profile
                    item.GetComponent<PlayerHandCard>().Attack(avatar_local);
                    item.GetComponent<PlayingCard>().SetCardState?.Invoke(false);

                    //deduct health
                    int currentHealth = System.Convert.ToInt32(playerHealthDeck.healthText.text);
                    int healthToDeduct = item.GetComponent<PlayerHandCard>().attackPoint;
                    currentHealth = currentHealth - healthToDeduct;
                    playerHealthDeck.healthText.text = "" + currentHealth;
                    playerHealthDeck.ShowTextDeduction(healthToDeduct);
                    ShowTextEffect(avatar_local.transform.position, "-1", Color.red);

                    yield return new WaitForSeconds(1.5f);

                    if(currentHealth < 0)
                    {
                        localPlayerDead?.Invoke();
                        localCardPlayer.CmdPlayerDied();
                    }

                }


            }
        }


    }
    public void ExecuteCombatForAllAttackLocalWithoutDefence()
    {
        StartCoroutine(ExecuteCombatForAllAttackLocalWithoutDefenceDelayed());
    }

    //attacking enemy
    private IEnumerator ExecuteCombatForAllAttackLocalWithoutDefenceDelayed()
    {
        Transform playerDeckParent = deckManager.DecksAndKeysPlayer[DeckType.CENTER_DECK].transform;

        foreach (Transform item in playerDeckParent)
        {
            if (item.GetComponent<PlayingCard>().glow.activeSelf)
            {
                item.GetComponent<PlayerHandCard>().Attack(avatar_opponent);
                item.GetComponent<PlayingCard>().SetCardState?.Invoke(false);

                //deduct health
                int currentHealth = System.Convert.ToInt32(enemyHealthDeck.healthText.text);
                int healthToDeduct = item.GetComponent<PlayerHandCard>().attackPoint;
                currentHealth = currentHealth - healthToDeduct;
                enemyHealthDeck.healthText.text = "" + currentHealth;
                enemyHealthDeck.ShowTextDeduction(healthToDeduct);
                ShowTextEffect(avatar_opponent.transform.position, "-1", Color.red);
                yield return new WaitForSeconds(1.5f);
            }
        }
    }

    private Transform GetDefendingCard(PLAYER_TYPE pLAYER_TYPE, int attackingCardID, DefendInformation defendInformation)
    {
        if(pLAYER_TYPE == PLAYER_TYPE.LOCAL)
        {
            for (int i = 0; i < defendInformation.defendLines.Count; i++)
            {
                if (defendInformation.defendLines[i].attackingCardID == attackingCardID)
                {
                    return GetCardWithID(PLAYER_TYPE.LOCAL, defendInformation.defendLines[i].defendingCardID);
                }
            }
        }
        else
        {
            for (int i = 0; i < defendInformation.defendLines.Count; i++)
            {
                if (defendInformation.defendLines[i].attackingCardID == attackingCardID)
                {
                    return GetCardWithID(PLAYER_TYPE.ENEMY, defendInformation.defendLines[i].defendingCardID);
                }
            }
        }
       

        return transform;
    }
    private Transform GetCardWithID(PLAYER_TYPE pLAYER_TYPE, int id)
    {
        switch (pLAYER_TYPE)
        {
            case PLAYER_TYPE.LOCAL:
                Transform playerDeckParent = deckManager.DecksAndKeysPlayer[DeckType.CENTER_DECK].transform;

                foreach (Transform item in playerDeckParent)
                {
                    if (item.GetComponent<PlayerHandCard>().thisCardData.id == id)
                    {
                        return item;
                    }
                }
                return transform;
                break;
            case PLAYER_TYPE.ENEMY:
                Transform enemyDeckParent = deckManager.DecksAndKeysEnemy[DeckType.CENTER_DECK].transform;

                foreach (Transform item in enemyDeckParent)
                {
                    if(item.GetComponent<PlayerHandCard>().thisCardData.id == id)
                    {
                        return item;
                    }
                }
                return transform;
                break;
            default:
                return transform;
                
        }
    }

    public bool IsBeingDefended(int attackingCard, DefendInformation defendInformation)
    {
        for (int i = 0; i < defendInformation.defendLines.Count; i++)
        {
            if(defendInformation.defendLines[i].attackingCardID == attackingCard)
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator AttackWithDelay(List<Transform> t,UnityAction callback)
    {
        foreach (var item in t)
        {
            item.GetComponent<PlayerHandCard>().Attack(avatar_opponent);
            yield return new WaitForSeconds(0.8f);
            item.GetComponent<PlayingCard>().SetCardState?.Invoke(false);
        }
        callback?.Invoke();
        GameManager.CurrentGameMode?.Invoke(GAME_MODE.NONE);
    }

    IEnumerator AttackSelectedWithDelay(List<Transform> t, UnityAction callback) 
    {
        AttackLine[] attackLines = FindObjectsOfType<AttackLine>();

        foreach (var item in attackLines)
        {
            Destroy(item.gameObject);
        }

        if(t[0].GetComponent<PlayerHandCard>().thisPlayerType == PLAYER_TYPE.LOCAL)
        {
            foreach (var item in t)
            {
                item.GetComponent<PlayerHandCard>().Attack(avatar_opponent);
                yield return new WaitForSeconds(0.8f);
                item.GetComponent<PlayingCard>().SetCardState?.Invoke(false);
            }
            
        }
        else
        {    
            /*
               --- Defense Mode ----
                1.We have defense lines connecting player and enemy cards
                2.

            */

            DefendLine[] defendLines = FindObjectsOfType<DefendLine>();
            
            foreach (var item in defendLines)
            {
                item.lineRenderer.enabled = false;
            }

            foreach (var item in t)
            {
                var dc = item.GetComponent<AttackableCard>().GetDefendableCard();
                if (dc) //check if defending card there
                {
                    item.GetComponent<PlayerHandCard>().Attack(dc.transform);
                }
                else
                {
                    item.GetComponent<PlayerHandCard>().Attack(avatar_local);
                }
               
                yield return new WaitForSeconds(0.8f);
                item.GetComponent<PlayingCard>().SetCardState?.Invoke(false);
            }

            foreach (var item in defendLines)
            {
                item.GetAttackingOwnder().GetComponent<AttackableCard>().SetDefendableCard(null);
                Destroy(item.gameObject);
            }
           
        }

        callback?.Invoke();
        GameManager.CurrentGameMode?.Invoke(GAME_MODE.NONE);
    }


    public void CreateLineForCard(AttackableCard attackableCard, UnityAction<AttackLine> callback)
    {
        AttackLine newAttackLine = Instantiate(attackLine, Vector3.zero, Quaternion.identity);

        newAttackLine.SetStart(attackableCard.transform);

        enemyPlayer.ProfileClickedUp += newAttackLine.ClickedUp;
        
      //  attackableCard.SetAttackable(false);

        newAttackLine.SetLineOwner(attackableCard);

        callback?.Invoke(newAttackLine);
    }

    public void OnEnemyPlayerProfileClickedUp()
    {
        if(gameManager.ThisGameMode == GAME_MODE.ATTACKING)
        {
            //if there is a line waiting to be set
        }
    }

    private void OnLineSetFinished(AttackLine attackLine)
    {
        enemyPlayer.ProfileClickedUp -= attackLine.ClickedUp;


    }

    #region Online
    private void OnAttackCardsSelectedOnline(Transform parent, List<int> cardIds)
    {
        //changes game mode to defense mode and doesn't attack until the defense response comes

        GameManager.OnGameModeChanged?.Invoke(GAME_MODE.DEFENDING);

        //for (int i = 0; i < cardIds.Count; i++)
        //{
        //    parent.GetChild(cardIds[i]).GetComponent<PlayingCard>().SetCardGlow(true);
        //}
        //get the attack cards to start glowing
        foreach (Transform item in parent)
        {
            for (int i = 0; i < cardIds.Count; i++)
            {
                if(item.GetComponent<PlayerHandCard>().thisCardData.id == cardIds[i])
                {
                    Debug.Log("got attack card data " + cardIds[i] + "&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
                    item.GetComponent<PlayingCard>().SetCardGlow(true);
                }
            }           
        }

        if(CheckDefensePossibilityLocal() == false)
        {
            Debug.Log("Defence not possible!!!!!!!!!!!!!!!!!!!");
            //make enemy cards attack
            DefendInformation defendInformation = new DefendInformation();
            ExecuteCombatForDefendingPlayer(defendInformation);
        }
    }

    private bool CheckDefensePossibilityLocal()
    {
        Transform deckParent = deckManager.DecksAndKeysPlayer[DeckType.CENTER_DECK].transform;

        foreach (Transform item in deckParent)
        {
            if(item.GetComponent<PlayerHandCard>().isActive)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDefenseCardsSelectedOnline(Transform enemyDeck, Transform playerDeck, string data)
    {
        DefendInformation defendInformation = JsonUtility.FromJson<DefendInformation>(data);
        Debug.Log("Setting card glow");
        //first glow the cards
        //foreach (Transform item in enemyDeck)
        //{
        //    for (int i = 0; i < defendInformation.defendLines.Count; i++)
        //    {
        //        if(item.GetComponent<PlayerHandCard>().thisCardData.id == defendInformation.defendLines[i].defendingCardID)
        //        {
        //            item.GetComponent<PlayingCard>().SetCardGlow(true);
        //        }
        //    }
        //}
        ExecuteCombatForAttackingPlayer(defendInformation);
    }


    #endregion
    CardPlayer localCardPlayer;
    CardPlayer opponentCardPlayer;
    private void SetLocalPlayer(CardPlayer cardPlayer)
    {
        localCardPlayer = cardPlayer;
    }

    private void SetEnemyPlayer(CardPlayer cardPlayer)
    {
        opponentCardPlayer = cardPlayer;
    }

    private void ShowTextEffect(Vector3 pos, string value, Color color)
    {
        GameObject newTextEffect = Instantiate(cardUpdate, pos, Quaternion.identity);
        newTextEffect.GetComponent<CardUpdate>().SetText(value, color);
    }
}
