
using UnityEngine;
using DG.Tweening;
public class CenterDeck : Deck
{
    private float xOffset = 178;
    private Vector3 cardPosition = new Vector3(0, -21, 10);
    private Vector3 cardRotation = new Vector3(20, 0, 0);

    private void Start()
    {
        cardPosition.x = xOffset;
    }
    public void AddACardToDeck(GameObject go)
   {
        if(transform.childCount > 3)
        {
          //  if (CheckForSimilarCards(go) == false) //grouping
         //   {
                Vector3 cardPosition = GetNewCardPosition();

                go.transform.SetParent(transform);
                go.transform.SetAsLastSibling();
                go.transform.DOLocalMove(cardPosition, 0.2f);
                go.transform.DOScale(new Vector3(143, 143, 8), 0.2f);
                go.transform.DORotate(new Vector3(20, 0, 0), 0.2f).OnComplete(() => {

                    ArrangeCardsInDeck();
                    go.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.CENTER_DECK;
                });
           // }
        }
        else
        {
            Vector3 cardPosition = GetNewCardPosition();

            go.transform.SetParent(transform);
            go.transform.SetAsLastSibling();
            go.transform.DOLocalMove(cardPosition, 0.2f);
            go.transform.DOScale(new Vector3(143, 143, 8), 0.2f);
            go.transform.DORotate(new Vector3(20, 0, 0), 0.2f).OnComplete(() => {

                ArrangeCardsInDeck();
                go.GetComponent<PlayerHandCard>().thisCardsDeck = DeckType.CENTER_DECK;
            });
        }
        


       

        
   }


    public void ArrangeCardsInDeck()
    {
        int totalChilds = transform.childCount;



      // int middleChild = (int)(totalChilds / 2);

      //  transform

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
                if(i < centerIndex)
                {
                    transform.GetChild(i).transform.DOLocalMoveX(-xOffset * (centerIndex - i), 0.2f);
                }
                else if(i == centerIndex)
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
     //   Debug.Log("Total Childs :" + totalChilds);
        if(totalChilds == 0)
        {
            return new Vector3(0, cardPosition.y, cardPosition.z);
        }

        else if(totalChilds == 1)
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

    public bool CheckForSimilarCards(GameObject go)
    {
        CardType ct = go.GetComponent<PlayingCard>().cardType;

        PlayingCard[] playingCards = transform.GetComponentsInChildren<PlayingCard>();

        foreach (var item in playingCards)
        {
            if(item.cardType == ct)
            {
                Vector3 newCardPosition = Vector3.zero;

                newCardPosition.y += 0.05f * item.transform.childCount;
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
}
