using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Events;
public class LeftDeck : Deck
{
    [SerializeField]
    private float xOffset = 1.13f;
    private Vector3 cardPosition = new Vector3(0, 0, 0);

    int totalResources = 0;

    bool resourceCardPlayedThisTurn = false;
    public void Start()
    {
        cardPosition.x = xOffset;
        TurnManager.OnTurnChangedTo += TurnChanged;
    }

    private void TurnChanged(Turn arg0)
    {
        if (arg0 == Turn.LOCAL)
            resourceCardPlayedThisTurn = false;
    }

    private void OnDestroy()
    {
        TurnManager.OnTurnChangedTo -= TurnChanged;
    }

    public void AddToDeck(GameObject go, UnityAction callback)
    {
        //add resources 
        totalResources += go.GetComponent<CardObject>().resourcePoint;

        resourceCardPlayedThisTurn = true;

        if (CheckForSimilarCards(go) == false)
        {
            Vector3 cardPosition = GetNewCardPosition();
            go.transform.SetParent(transform);
            go.transform.SetAsLastSibling();
            go.transform.DOLocalMove(cardPosition, 0.2f).OnComplete(()=> callback?.Invoke()) ;
            go.transform.DOScale(Vector3.one, 0.2f);
            go.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f).OnComplete(() =>
            {

                ArrangeCardsInDeck();
                //CheckForSimilarCards(go);
            });
        }
        


       
    }

    public bool IsResourceAdded()
    {
        return resourceCardPlayedThisTurn;
    }

    private void ArrangeCardsInDeck()
    {
        int totalChilds = transform.childCount;

        if(totalChilds == 1)
        {
            transform.GetChild(0).transform.localPosition = new Vector3(0, cardPosition.y, cardPosition.z);
        }
        else if(totalChilds == 2)
        {
            transform.GetChild(0).transform.localPosition = new Vector3(0, cardPosition.y, cardPosition.z);
            transform.GetChild(1).transform.localPosition = new Vector3(xOffset, cardPosition.y, cardPosition.z);
        }
        else
        {
            int centerIndex = (int)totalChilds / 2;

            for (int i = 0; i < totalChilds; i++)
            {
                if (i < centerIndex)
                {
                    transform.GetChild(i).transform.DOLocalMoveX(-xOffset * (centerIndex - i), 0.2f);
                }
                else if (i == centerIndex)
                {
                    transform.GetChild(i).transform.DOLocalMoveX(0, 0.2f);
                }
                else
                {
                    transform.GetChild(i).transform.DOLocalMoveX(xOffset * (i - centerIndex), 0.2f);
                }
            }
        }

    }


    private Vector3 GetNewCardPosition()
    {
        int totalChilds = transform.childCount;
        Debug.Log("Total Childs :" + totalChilds);
        if (totalChilds == 0)
        {
            return new Vector3(0, cardPosition.y, cardPosition.z);
        }

        else if (totalChilds == 1)
        {
            return new Vector3(xOffset, cardPosition.y, cardPosition.z);
        }
        else
        {
            int t = (int)totalChilds / 2;

            return new Vector3(xOffset * (totalChilds - t), cardPosition.y, cardPosition.z);
        }

        return Vector3.zero;
    }

    private bool CheckForSimilarCards(GameObject go)
    {
        CardType ct = go.GetComponent<PlayingCard>().cardType;

        PlayingCard[] playingCards = GetComponentsInChildren<PlayingCard>();

        foreach (var item in playingCards)
        {
            if (item.cardType == ct)
            {
                Vector3 newCardPosition = Vector3.zero;

                newCardPosition.y += 0.12f * item.transform.childCount;
                newCardPosition.z += 0.11f * item.transform.childCount;

                go.transform.SetParent(item.transform);
                go.transform.SetAsLastSibling();
                go.transform.DOLocalMove(newCardPosition, 0.2f);
                go.transform.DOScale(Vector3.one, 0.2f);
                go.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.2f).OnComplete(() => { ArrangeCardsInDeck(); });

                return true;
            }
        }        

        return false;
    }

    public int GetTotalResources()
    {
        //get all cards in the deck
        CardObject[] cardObjects = GetComponentsInChildren<CardObject>();

        int totalR = 0;
        foreach (CardObject item in cardObjects)
        {
            if(item.isActive == true)
            {
                totalR += item.resourcePoint;
            }
        }

        return totalR;
    }

    public void DepleteCards(int count)
    {
        PlayingCard[] playingCards = GetComponentsInChildren<PlayingCard>();
        int t = 0;
        foreach (PlayingCard item in playingCards)
        {
            if (item.GetComponent<PlayerHandCard>().isActive == true)
            { 
                item.SetCardState?.Invoke(false);
                t++;
                if (t == count)
                {
                    break;
                }
            }
            
        }          
        
    }
}
