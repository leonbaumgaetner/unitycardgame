using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
public class AbilitiesManager : MonoBehaviour
{
    public static UnityAction<AbilityType, GameObject> ExecuteAbilty;

    public GameObject targetLineObject;
    public GameObject blueBall;

    public Transform rightDeck;
    bool startUpdate = false;


    PlayerInsteraction playerInsteraction;

    public DiscardDeck discardDeckLocal;

    public GameObject delayIcon, diplomaticIcon;

    private CardPlayer localCardPlayer;

    public GameObject textEffect;

    private void Start()
    {
        ExecuteAbilty += CheckAbility;
        playerInsteraction = GetComponent<PlayerInsteraction>();

        GameManager.OnLocalCardSet += OnLocalPlayerSet;
    }

    private void OnDestroy()
    {
        ExecuteAbilty -= CheckAbility;
    }

    private void Update()
    {
        

        if(startUpdate)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray,out hit,1000))
                {
                    if(hit.collider.GetComponent<PlayerHandCard>())
                    {
                        Debug.Log("Clicked on a card");
                        TargetLine tl = FindObjectOfType<TargetLine>();
                        if(tl)
                        tl.OnTargetCardClicked?.Invoke(hit.collider.gameObject);
                    }
                }
            }
            else if(Input.GetMouseButtonDown(1))
            {
                TargetLine tl = FindObjectOfType<TargetLine>();

                Destroy(tl.gameObject);

                //return card to deck
                Transform card = playerInsteraction.GetPlayerDeck(DeckType.RIGHT_SIDE).GetComponentInChildren<PlayerHandCard>().transform;

                playerInsteraction.MoveCardToDeck(DeckType.PLAYER_HAND, card.gameObject, PLAYER_TYPE.LOCAL);

                startUpdate = false;

                if(GameManager.isOnline)
                {
                    localCardPlayer.CmdCardCancelledFromRightDeck();
                }
            }
        }
       
    }

    private void CheckAbility(AbilityType abilityType, GameObject targetObject)
    {
       var type = targetObject.GetComponent<PlayerHandCard>().thisPlayerType;

        if(type == PLAYER_TYPE.ENEMY)
        {
            return;
        }

        switch (abilityType)
        {
            case AbilityType.NONE:
                break;
            case AbilityType.AFFLICT:
                Afflict(targetObject);
                break;
            case AbilityType.RALLY:
                Rally(targetObject);
                break;
            case AbilityType.ENHANCE:
                Enhance(targetObject);
                break;
            case AbilityType.DELAY:
                Delay(targetObject);
                break;
            case AbilityType.DIPLOMATIC_IMMUNITY:
                DiplomaticImmunity(targetObject);
                break;
            case AbilityType.GERRYMANDER:
                Gerrymander(targetObject);
                break;
            case AbilityType.RECALL:
                RecallAbility(targetObject);
                break;
            default:
                break;
        }
    }
    private void Afflict(GameObject go)
    {
        //check if enemy card available
        //int cc = playerInsteraction.GetEnemyDeck(DeckType.CENTER_DECK).transform.childCount;
        //if(cc > 0)
        //{
            //start a target line from this card
            GameObject to = Instantiate(targetLineObject);
            Debug.Log("creating target line" + to.gameObject);
            TargetLine tl = to.GetComponent<TargetLine>();
            tl.OnTargetCardClicked += OnTargetAfflictClicked;
            startUpdate = true;
            tl.SetStart(go.transform);
        //}
        //else
        //{
        //    //send the card back
        //    playerInsteraction.MoveCardToDeck(DeckType.PLAYER_HAND, go, PLAYER_TYPE.LOCAL);
           
        //}        
    }

    private void OnTargetAfflictClicked(GameObject targetCard)
    {
        PlayerHandCard playerHandCard = targetCard.GetComponent<PlayerHandCard>();

        if (playerHandCard.thisCardsDeck == DeckType.CENTER_DECK) // && playerHandCard.thisPlayerType == PLAYER_TYPE.ENEMY)
        {
            //if the card has diplomatic immunity then you can't target.
            if (targetCard.GetComponentInChildren<DiplomaticAbility>())
            {
                return;
            }


            TargetLine tl = FindObjectOfType<TargetLine>();

            if (tl)
            {
                

                tl.SetEndPoint(targetCard.transform);

                
            }

            DOVirtual.DelayedCall(1.5f, () =>
            {
                Destroy(tl.gameObject);


                GameObject newBlueBall = Instantiate(blueBall, rightDeck.position, Quaternion.identity);

                newBlueBall.transform.DOMove(targetCard.transform.position, 1).OnComplete(() =>
                {
                    Destroy(newBlueBall);

                    var cd = playerHandCard.thisCardData;
                    AttackCard attackCard = cd as AttackCard;
                    attackCard.attack -= 1;
                    ShowTextEffect(playerHandCard.transform.position, " Attack-1", Color.red);
                    playerHandCard.LoadCardData(attackCard);

                    //discard card from right deck
                    GameObject newCard = rightDeck.GetComponentInChildren<PlayerHandCard>().gameObject;

                    discardDeckLocal.DiscardACard(newCard);
                });


            });
        }

        if(GameManager.isOnline)
        {
            int id = targetCard.GetComponent<PlayerHandCard>().thisCardData.id;
            string playerType = PLAYER_TYPE.LOCAL.ToString(); // when opponing gets this, it will attack his card
            string typeOfAbility = AbilityType.AFFLICT.ToString();
            localCardPlayer.CmdTargetCardSelected(id, playerType, typeOfAbility);
        }
    }

    private void Rally(GameObject go)
    {
        //check if there are player cards in center deck
        int cc = playerInsteraction.GetPlayerDeck(DeckType.CENTER_DECK).transform.childCount;
        if(cc > 0)
        {
            int totalDelay = 0;
            //for each card in center deck increase the attack and defence by 1
            foreach (Transform item in playerInsteraction.GetPlayerDeck(DeckType.CENTER_DECK).transform)
            {
                totalDelay++;
                //create the blue glow
                GameObject newBlueBall = Instantiate(blueBall, rightDeck.position, Quaternion.identity);

                newBlueBall.transform.DOMove(item.transform.position, 1).OnComplete(()=> {

                    var cd = item.GetComponent<PlayerHandCard>().thisCardData;
                    AttackCard attackCard = cd as AttackCard;
                    attackCard.attack += 1;
                    attackCard.defense += 1;
                    ShowTextEffect(item.position, "Attack +1", Color.green);
                    Vector3 pos = new Vector3(item.position.x+1.5f, item.position.y-1, item.position.z + 1);
                    ShowTextEffect(pos, "Def +1", Color.green);
                    item.GetComponent<PlayerHandCard>().LoadCardData(attackCard);

                    Destroy(newBlueBall);
                });               
            }

            DOVirtual.DelayedCall(totalDelay, () => 
            {
                //send card to discard deck
                discardDeckLocal.DiscardACard(go);
            });
            
        }
        else
        {
            playerInsteraction.MoveCardToDeck(DeckType.PLAYER_HAND, go, PLAYER_TYPE.LOCAL);
            
        }
    }

    private void Enhance(GameObject go)
    {
        //CREATE target line
        GameObject to = Instantiate(targetLineObject);
        TargetLine tl = to.GetComponent<TargetLine>();
        tl.OnTargetCardClicked += OnEnhanceTargetClicked;
        tl.SetStart(go.transform);
        startUpdate = true;
    }

    private void OnEnhanceTargetClicked(GameObject targetCard)
    {
        startUpdate = false;


        PlayerHandCard playerHandCard = targetCard.GetComponent<PlayerHandCard>();

        if (playerHandCard.thisCardsDeck == DeckType.CENTER_DECK ) //&& playerHandCard.thisPlayerType == PLAYER_TYPE.ENEMY)
        {
            TargetLine tl = FindObjectOfType<TargetLine>();

            if (tl)
            {
                tl.SetEndPoint(targetCard.transform);
            }

            DOVirtual.DelayedCall(1.5f, () =>
            {
                Destroy(tl.gameObject);


                GameObject newBlueBall = Instantiate(blueBall, rightDeck.position, Quaternion.identity);

                newBlueBall.transform.DOMove(targetCard.transform.position, 1).OnComplete(() =>
                {
                    Destroy(newBlueBall);

                    var cd = playerHandCard.thisCardData;
                    AttackCard attackCard = cd as AttackCard;
                    attackCard.voteValue += 1;

                    ShowTextEffect(targetCard.transform.position, "Vote +1", Color.green);                   


                    playerHandCard.LoadCardData(attackCard);

                    //discard card from right deck
                    GameObject newCard = rightDeck.GetComponentInChildren<PlayerHandCard>().gameObject;

                    discardDeckLocal.DiscardACard(newCard);
                });


            });
        }

        if(GameManager.isOnline)
        {
            int id = targetCard.GetComponent<PlayerHandCard>().thisCardData.id;
            string playerType = targetCard.GetComponent<PlayerHandCard>().thisPlayerType.ToString();
            // when opponing gets this, it will attack this card type
            string abilityType = AbilityType.ENHANCE.ToString();
            localCardPlayer.CmdTargetCardSelected(id, playerType, abilityType);
        }


    }

    private void Delay(GameObject go)
    {
        //check if enemy card available
        //int cc = playerInsteraction.GetEnemyDeck(DeckType.CENTER_DECK).transform.childCount;
        //if (cc > 0)
        //{
        //    ////start a target line from this card
        //    //GameObject to = Instantiate(targetLineObject);
        //    //Debug.Log("creating target line" + to.gameObject);
        //    //TargetLine tl = to.GetComponent<TargetLine>();
        //    //tl.OnTargetCardClicked += OnDelayTargetSelected;
        //    //startUpdate = true;
        //    //tl.SetStart(go.transform);


        //}
        //else
        //{
        //    //send the card back
        //    playerInsteraction.MoveCardToDeck(DeckType.PLAYER_HAND, go, PLAYER_TYPE.LOCAL);

        //}

        GameObject d = Instantiate(delayIcon);

        Transform targetCard = playerInsteraction.GetPlayerDeck(DeckType.RIGHT_SIDE).GetComponentInChildren<PlayerHandCard>().transform;

        Transform ab_parent = targetCard.GetComponent<CardObject>().abilityIconParent;

        d.transform.SetParent(ab_parent);

        d.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        d.transform.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
        d.transform.GetComponent<RectTransform>().localScale = Constants.delayIconScale;

        d.GetComponent<DelayAbility>().SetDelay(2);

        DOVirtual.DelayedCall(3.5f, () => {
            targetCard.GetComponent<CardObject>().thisCardsDeck = DeckType.CENTER_DECK;
            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.CENTER_DECK, targetCard.gameObject, PLAYER_TYPE.LOCAL);
            // MoveCardToDeck(DeckType.CENTER_DECK, card);
        });
    }

    private void OnDelayTargetSelected(GameObject targetCard)
    {
       

        Debug.LogError("Delay card added");
        PlayerHandCard playerHandCard = targetCard.GetComponent<PlayerHandCard>();

        if (playerHandCard.thisCardsDeck == DeckType.CENTER_DECK) //&& playerHandCard.thisPlayerType == PLAYER_TYPE.ENEMY)
        {
            if (targetCard.GetComponentInChildren<DiplomaticAbility>())
            {
                return;
            }


            TargetLine tl = FindObjectOfType<TargetLine>();

            if (tl)
            {
                tl.SetEndPoint(targetCard.transform);
                startUpdate = false;
            }

            DOVirtual.DelayedCall(1.5f, () =>
            {
                Destroy(tl.gameObject);


                GameObject newBlueBall = Instantiate(blueBall, rightDeck.position, Quaternion.identity);

                newBlueBall.transform.DOMove(targetCard.transform.position, 1).OnComplete(() =>
                {
                    Destroy(newBlueBall);

                    GameObject d = Instantiate(delayIcon);

                    Transform ab_parent = targetCard.GetComponent<CardObject>().abilityIconParent;

                    d.transform.SetParent(ab_parent);                  

                    d.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    d.transform.GetComponent<RectTransform>().localRotation = new Quaternion(0,0,0,0);
                    d.transform.GetComponent<RectTransform>().localScale = Constants.delayIconScale;

                    d.GetComponent<DelayAbility>().SetDelay(2);
                    //discard card from right deck
                    GameObject newCard = rightDeck.GetComponentInChildren<PlayerHandCard>().gameObject;

                    discardDeckLocal.DiscardACard(newCard);
                });

                if (GameManager.isOnline)
                {
                    int id = targetCard.GetComponent<PlayerHandCard>().thisCardData.id;
                    string playerType = targetCard.GetComponent<PlayerHandCard>().thisPlayerType.ToString();
                    // when opponing gets this, it will attack this card type
                    string abilityType = AbilityType.DELAY.ToString();
                    localCardPlayer.CmdTargetCardSelected(id, playerType, abilityType);
                }
            });
        }
    }

    private void DiplomaticImmunity(GameObject targetCard)
    {
        //int cc = playerInsteraction.GetPlayerDeck(DeckType.CENTER_DECK).transform.childCount;
        //int cc_enemy = playerInsteraction.GetEnemyDeck(DeckType.CENTER_DECK).transform.childCount;

        //if (cc > 0 || cc_enemy > 0)
        //{
        //    //start a target line from this card
        //    GameObject to = Instantiate(targetLineObject);
        //    Debug.Log("creating target line" + to.gameObject);
        //    TargetLine tl = to.GetComponent<TargetLine>();
        //    tl.OnTargetCardClicked += OnDiplomaticImmunityTargetSelected;
        //    startUpdate = true;
        //    //get right deck
        //    Transform rightDeck = playerInsteraction.GetPlayerDeck(DeckType.RIGHT_SIDE).transform;

        //    tl.SetStart(rightDeck.transform);
        //}
        //else
        //{
        //    //send the card back
        //    playerInsteraction.MoveCardToDeck(DeckType.PLAYER_HAND, go, PLAYER_TYPE.LOCAL);
        //}


            //GameObject newBlueBall = Instantiate(blueBall, rightDeck.position, Quaternion.identity);

        

            GameObject d = Instantiate(diplomaticIcon);

            Transform ab_parent = targetCard.GetComponent<CardObject>().abilityIconParent;

            d.transform.SetParent(ab_parent);

            d.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            d.transform.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
            d.transform.GetComponent<RectTransform>().localScale = Constants.diplomaticIconScale;


        DOVirtual.DelayedCall(3.5f, () => {
            targetCard.GetComponent<CardObject>().thisCardsDeck = DeckType.CENTER_DECK;
            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.CENTER_DECK, targetCard.gameObject, PLAYER_TYPE.LOCAL);            
        });


    }

    private void OnDiplomaticImmunityTargetSelected(GameObject targetCard)
    {
        PlayerHandCard playerHandCard = targetCard.GetComponent<PlayerHandCard>();

        if (playerHandCard.thisCardsDeck == DeckType.CENTER_DECK) //any card on center deck
        {
            TargetLine tl = FindObjectOfType<TargetLine>();

            if (tl)
            {
                tl.SetEndPoint(targetCard.transform);
            }

            DOVirtual.DelayedCall(1.5f, () =>
            {
                Destroy(tl.gameObject);


                GameObject newBlueBall = Instantiate(blueBall, rightDeck.position, Quaternion.identity);

                newBlueBall.transform.DOMove(targetCard.transform.position, 1).OnComplete(() =>
                {
                    Destroy(newBlueBall);

                    GameObject d = Instantiate(diplomaticIcon);

                    Transform ab_parent = targetCard.GetComponent<CardObject>().abilityIconParent;

                    d.transform.SetParent(ab_parent);

                    d.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    d.transform.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
                    d.transform.GetComponent<RectTransform>().localScale = Constants.diplomaticIconScale;                    

                    //discard card from right deck
                    GameObject newCard = rightDeck.GetComponentInChildren<PlayerHandCard>().gameObject;

                    discardDeckLocal.DiscardACard(newCard);
                });


            });
        }
    }

    private void SmearPlayed(GameObject targetCard)
    {

    }

    private void Gerrymander(GameObject targetCard)
    {
        PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.CENTER_DECK, targetCard, PLAYER_TYPE.LOCAL);

        Transform parentDeck = playerInsteraction.GetEnemyDeck(DeckType.CENTER_DECK).transform;

        PlayerHandCard[] playerHandCards = parentDeck.GetComponentsInChildren<PlayerHandCard>();

        foreach (var item in playerHandCards)
        {
            var cd = item.thisCardData;
            AttackCard attackCard = cd as AttackCard;
            attackCard.voteValue += 1;
            ShowTextEffect(item.transform.position, "VOTE +1", Color.green);
            item.GetComponent<PlayerHandCard>().LoadCardData(attackCard);
        }
    }

    private void RecallAbility(GameObject targetCard)
    {
        //select a target card
        //CREATE target line
        GameObject to = Instantiate(targetLineObject);
        TargetLine tl = to.GetComponent<TargetLine>();
        tl.OnTargetCardClicked += OnRecallTargetSelected;
        tl.SetStart(targetCard.transform);
        startUpdate = true;

    }

    private void OnRecallTargetSelected(GameObject selectedCard)
    {
        startUpdate = false;

        PlayerHandCard playerHandCard = selectedCard.GetComponent<PlayerHandCard>();

        if (playerHandCard.thisCardsDeck == DeckType.CENTER_DECK) //&& playerHandCard.thisPlayerType == PLAYER_TYPE.ENEMY)
        {
            TargetLine tl = FindObjectOfType<TargetLine>();

            if (tl)
            {
                tl.SetEndPoint(selectedCard.transform);
            }

            DOVirtual.DelayedCall(1.5f, () =>
            {
                Destroy(tl.gameObject);


                GameObject newBlueBall = Instantiate(blueBall, rightDeck.position, Quaternion.identity);

                newBlueBall.transform.DOMove(selectedCard.transform.position, 1).OnComplete(() =>
                {
                    Destroy(newBlueBall);
                    ShowTextEffect(selectedCard.transform.position,"RECALLED", Color.yellow);
                    PLAYER_TYPE pLAYER_TYPE = playerHandCard.thisPlayerType;

                    PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, selectedCard, pLAYER_TYPE);
                   
                    //Add card from right deck
                    GameObject newCard = rightDeck.GetComponentInChildren<PlayerHandCard>().gameObject;
                  
                    PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.CENTER_DECK, newCard, PLAYER_TYPE.LOCAL);
                    
                });


            });

            if(GameManager.isOnline)
            {
                int id = selectedCard.GetComponent<PlayerHandCard>().thisCardData.id;
                string playerType = selectedCard.GetComponent<PlayerHandCard>().thisPlayerType.ToString(); // when opponing gets this, it will attack his card
                string typeOfAbility = AbilityType.RECALL.ToString();
                localCardPlayer.CmdTargetCardSelected(id, playerType, typeOfAbility);
            }
        }
    }

    private void OnLocalPlayerSet(CardPlayer  cardPlayer)
    {
        localCardPlayer = cardPlayer;
    }

   


    #region AbilitiesOnline
    public void TargetAfflictSelected(Transform targetCard, GameObject blueEffect)
    {
        //targetCard

        //blueeffect

       

        if (targetCard)
        {
            

            Transform rightDeck = playerInsteraction.GetEnemyDeck(DeckType.RIGHT_SIDE).transform;

            GameObject newBlueBall = Instantiate(blueEffect, rightDeck.position, Quaternion.identity);

            newBlueBall.transform.DOMove(targetCard.transform.position, 1).OnComplete(() =>
            {
                Destroy(newBlueBall);

                PlayerHandCard playerHandCard = targetCard.GetComponent<PlayerHandCard>();
                var cd = playerHandCard.thisCardData;
                AttackCard attackCard = cd as AttackCard;

                attackCard.attack -= 1;
                ShowTextEffect(targetCard.position,"Attack -1", Color.red);
                playerHandCard.LoadCardData(attackCard);

                //discard card from right deck
                GameObject newCard = rightDeck.GetComponentInChildren<PlayerHandCard>().gameObject;

                playerInsteraction.GetEnemyDeck(DeckType.DISCARD_DECK).transform.GetComponent<DiscardDeck>().DiscardACard(newCard.gameObject);
                // discardDeckLocal.DiscardACard(newCard);
            });

        }
        else
        {
            Debug.LogError("Target card not found");
        }
    }

    public void PlayedRallyCard()
    {

        DOVirtual.DelayedCall(1.5f, () => {

            int cc = playerInsteraction.GetEnemyDeck(DeckType.CENTER_DECK).transform.childCount;
            if (cc > 0)
            {
                int totalDelay = 0;
                //for each card in center deck increase the attack and defence by 1
                foreach (Transform item in playerInsteraction.GetEnemyDeck(DeckType.CENTER_DECK).transform)
                {
                    totalDelay++;
                    //create the blue glow
                    GameObject newBlueBall = Instantiate(blueBall, rightDeck.position, Quaternion.identity);

                    newBlueBall.transform.DOMove(item.transform.position, 1).OnComplete(() =>
                    {

                        var cd = item.GetComponent<PlayerHandCard>().thisCardData;
                        AttackCard attackCard = cd as AttackCard;
                        attackCard.attack += 1;
                        attackCard.defense += 1;
                        
                        ShowTextEffect(item.position, "Attack +1", Color.green);
                        Vector3 pos = new Vector3(item.position.x+1, item.position.y-2, item.position.z + 1);
                        ShowTextEffect(pos, "Def +1", Color.green);
                       
                        item.GetComponent<PlayerHandCard>().LoadCardData(attackCard);

                        Destroy(newBlueBall);
                    });
                }

                DOVirtual.DelayedCall(totalDelay, () =>
                {
                    //send card to discard deck
                    Transform dd = playerInsteraction.GetEnemyDeck(DeckType.DISCARD_DECK).transform;
                    Transform rightCard = playerInsteraction.GetEnemyDeck(DeckType.RIGHT_SIDE).GetComponentInChildren<PlayerHandCard>().transform; 
                    dd.GetComponent<DiscardDeck>().DiscardACard(rightCard.gameObject);

                });
            }

        });
       
    }

    public void PlayedDelayCard()
    {
        GameObject d = Instantiate(delayIcon);

        Transform targetCard = playerInsteraction.GetEnemyDeck(DeckType.RIGHT_SIDE).GetComponentInChildren<PlayerHandCard>().transform;

        Transform ab_parent = targetCard.GetComponent<CardObject>().abilityIconParent;

        d.transform.SetParent(ab_parent);

        d.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        d.transform.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
        d.transform.GetComponent<RectTransform>().localScale = Constants.delayIconScale;

        d.GetComponent<DelayAbility>().SetDelay(2);

        DOVirtual.DelayedCall(3.5f, () => {
            targetCard.GetComponent<CardObject>().thisCardsDeck = DeckType.CENTER_DECK;
            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.CENTER_DECK, targetCard.gameObject, PLAYER_TYPE.ENEMY);
           // MoveCardToDeck(DeckType.CENTER_DECK, card);
        });

    }

    public void PlayedDiplomaticCard()
    {
        GameObject d = Instantiate(diplomaticIcon);

        Transform targetCard = playerInsteraction.GetEnemyDeck(DeckType.RIGHT_SIDE).GetComponentInChildren<PlayerHandCard>().transform;

        Transform ab_parent = targetCard.GetComponent<CardObject>().abilityIconParent;

        d.transform.SetParent(ab_parent);

        d.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        d.transform.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
        d.transform.GetComponent<RectTransform>().localScale = Constants.diplomaticIconScale;

        DOVirtual.DelayedCall(3.5f, () => {
            targetCard.GetComponent<CardObject>().thisCardsDeck = DeckType.CENTER_DECK;
            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.CENTER_DECK, targetCard.gameObject, PLAYER_TYPE.ENEMY);
        });
    }

    public void PlayedEnhanceTarget(Transform targetCard, GameObject blueEffect)
    {
        PlayerHandCard playerHandCard = targetCard.GetComponent<PlayerHandCard>();

        if (playerHandCard.thisCardsDeck == DeckType.CENTER_DECK) //&& playerHandCard.thisPlayerType == PLAYER_TYPE.ENEMY)
        {
           

            DOVirtual.DelayedCall(1.5f, () =>
            {
                GameObject newBlueBall = Instantiate(blueEffect, rightDeck.position, Quaternion.identity);

                newBlueBall.transform.DOMove(targetCard.transform.position, 1).OnComplete(() =>
                {
                    Destroy(newBlueBall);

                    var cd = playerHandCard.thisCardData;
                    AttackCard attackCard = cd as AttackCard;
                    attackCard.voteValue += 1;
                    ShowTextEffect(targetCard.position, "Vote +1", Color.green);
                    playerHandCard.LoadCardData(attackCard);

                    //discard card from right deck                    

                    Transform dd = playerInsteraction.GetEnemyDeck(DeckType.DISCARD_DECK).transform;
                    Transform rightCard = playerInsteraction.GetEnemyDeck(DeckType.RIGHT_SIDE).GetComponentInChildren<PlayerHandCard>().transform;
                    dd.GetComponent<DiscardDeck>().DiscardACard(rightCard.gameObject);

                  
                });


            });
        }
    }

    public void PlayedDelayTarget(Transform targetCard, GameObject blueEffect)
    {
        GameObject newBlueBall = Instantiate(blueEffect, rightDeck.position, Quaternion.identity);

        newBlueBall.transform.DOMove(targetCard.transform.position, 1).OnComplete(() =>
        {
            Destroy(newBlueBall);

            GameObject d = Instantiate(delayIcon);

            Transform ab_parent = targetCard.GetComponent<CardObject>().abilityIconParent;

            d.transform.SetParent(ab_parent);

            d.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            d.transform.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
            d.transform.GetComponent<RectTransform>().localScale = Constants.delayIconScale;

            d.GetComponent<DelayAbility>().SetDelay(2);
          
            //discard card from right deck        

            Transform dd = playerInsteraction.GetEnemyDeck(DeckType.DISCARD_DECK).transform;
            Transform rightCard = playerInsteraction.GetEnemyDeck(DeckType.RIGHT_SIDE).GetComponentInChildren<PlayerHandCard>().transform;
            dd.GetComponent<DiscardDeck>().DiscardACard(rightCard.gameObject);
        });
    }

    public void PlayedDiplomaticTarget(Transform targetCard, GameObject blueEffect)
    {
        GameObject newBlueBall = Instantiate(blueEffect, rightDeck.position, Quaternion.identity);

        newBlueBall.transform.DOMove(targetCard.transform.position, 1).OnComplete(() =>
        {
            Destroy(newBlueBall);

            GameObject d = Instantiate(diplomaticIcon);

            Transform ab_parent = targetCard.GetComponent<CardObject>().abilityIconParent;

            d.transform.SetParent(ab_parent);

            d.transform.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            d.transform.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);
            d.transform.GetComponent<RectTransform>().localScale = Constants.diplomaticIconScale;

            //discard card from right deck
            if (GameManager.isOnline)
            {
                int id = targetCard.GetComponent<PlayerHandCard>().thisCardData.id;
                string playerType = targetCard.GetComponent<PlayerHandCard>().thisPlayerType.ToString();
                // when opponing gets this, it will attack this card type
                string abilityType = AbilityType.DIPLOMATIC_IMMUNITY.ToString();
                localCardPlayer.CmdTargetCardSelected(id, playerType, abilityType);
            }
        });
    }

    public void PlayedGerrymender()
    {
        DOVirtual.DelayedCall(1.5f, () =>
        {

            Transform targetCard = playerInsteraction.GetEnemyDeck(DeckType.RIGHT_SIDE).GetComponentInChildren<PlayerHandCard>().transform;


            PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.CENTER_DECK, targetCard.gameObject, PLAYER_TYPE.ENEMY);

            Transform parentDeck = playerInsteraction.GetPlayerDeck(DeckType.CENTER_DECK).transform;

            PlayerHandCard[] playerHandCards = parentDeck.GetComponentsInChildren<PlayerHandCard>();

            foreach (var item in playerHandCards)
            {
                var cd = item.thisCardData;
                AttackCard attackCard = cd as AttackCard;
                attackCard.voteValue += 1;
                ShowTextEffect(item.transform.position, "VOTE +1", Color.green);
                item.GetComponent<PlayerHandCard>().LoadCardData(attackCard);
            }
        });
    }

    //target card is the selected card
    public void PlayedRecall(Transform targetCard, GameObject blueEffect)
    {
        DOVirtual.DelayedCall(1.5f, () =>
        {      


            GameObject newBlueBall = Instantiate(blueEffect, rightDeck.position, Quaternion.identity);

            newBlueBall.transform.DOMove(targetCard.transform.position, 1).OnComplete(() =>
            {
                Destroy(newBlueBall);

               

                PLAYER_TYPE pLAYER_TYPE = targetCard.GetComponent<PlayerHandCard>().thisPlayerType;

                PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.PLAYER_HAND, targetCard.gameObject, pLAYER_TYPE);

                //Add card from right deck
                GameObject newCard = rightDeck.GetComponentInChildren<PlayerHandCard>().gameObject;

                PlayerInsteraction.ChangeCardDeck?.Invoke(DeckType.CENTER_DECK, newCard, PLAYER_TYPE.ENEMY);

            });


        });
    }
    #endregion


    private void ShowTextEffect(Vector3 pos, string value, Color color)
    {
        GameObject newTextEffect = Instantiate(textEffect, pos, Quaternion.identity);
        newTextEffect.GetComponent<CardUpdate>().SetText(value, color);
    }
}
