using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
public class PlayerHandDeck : Deck
{
    [SerializeField]
    private float xOffset = 50;
    [SerializeField]
    private Vector3 cardPosition = new Vector3(0, -13, 10);

    public  UnityAction<GameObject> SendCardToPlayerHand;
    public  static UnityAction ArrangeHand;
    public override void Start()
    {
        base.Start();

        SendCardToPlayerHand += AddCardToPlayerHand;
        ArrangeHand += RearrangeHandCards;
    }

    public void InitializeCards()
    {
        
        foreach (Transform item in transform)
        {
            item.GetComponent<PlayerHandCard>().InitializeCard.Invoke();
        }
    }

    public void AddCardToPlayerHand(GameObject go)
    {
        Vector3 cardPosition = GetNewCardPosition();
        go.transform.SetParent(transform);
        go.transform.SetAsLastSibling();
        go.transform.DOLocalMove(cardPosition, 1.2f).OnComplete(()=> 
        {

            go.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.PLAYER_HAND;
            ArrangeCardsInDeck();
            go.GetComponent<Collider>().enabled = true;
            
            
            });
        go.transform.DOScale(new Vector3(61, 86, 8), 0.2f).OnComplete(() => {


          //  go.GetComponent<PlayingCard>().SetRandomType();
            //  DOVirtual.DelayedCall(1.2f, ()=> ArrangeCardsInDeck());
            InitializeCards();
        }); ;
        //go.transform.DORotate(new Vector3(0, 0, 0), 0.3f)
       // ArrangeCardsInDeck();
    }

    public void ArrangeCardsInDeck()
    {
        int totalChilds = transform.childCount;

        if (totalChilds == 1)
        {
            transform.GetChild(0).transform.localPosition = new Vector3(0, cardPosition.y, cardPosition.z);
        }
        else if (totalChilds == 2)
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
                    transform.GetChild(i).transform.DOLocalMoveX(-xOffset * (centerIndex - i), 0.0f);
                }
                else if (i == centerIndex)
                {
                    transform.GetChild(i).transform.DOLocalMoveX(0, 0.0f);
                }
                else
                {
                    transform.GetChild(i).transform.DOLocalMoveX(xOffset * (i - centerIndex), 0.0f);
                }
            }
        }
        foreach (Transform item in transform)
        {
            item.GetComponent<PlayerHandCard>().SetStartingPosition();
            
        }
        //DOVirtual.DelayedCall(0.5f, () => 
        //{
        //    //InitializeCards();
           
        //});
    }

    private Vector3 GetNewCardPosition()
    {
        int totalChilds = transform.childCount;
       // Debug.Log("Total Childs :" + totalChilds);
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


    private void RearrangeHandCards()
    {
        ArrangeCardsInDeck();
    }
}
