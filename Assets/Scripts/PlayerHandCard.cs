using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
public class PlayerHandCard : CardObject
{
   
    public Vector3 startingPosition = new Vector3();
    
    public Vector3 startingRotation = new Vector3();
    
    public Vector3 startingScale = new Vector3();
    bool followMouse = false;

    public UnityAction InitializeCard;


    private bool canToggle = true;

    private void Awake()
    {
        InitializeCard += SetInitialData;
        GetComponent<Collider>().enabled = false;
    }
    public void Start()
    {
        
    }

    public void SetInitialData()
    {
        startingPosition = transform.localPosition;
        startingRotation = transform.localEulerAngles;

        if (thisPlayerType == PLAYER_TYPE.ENEMY)
            return;
        GetComponent<PlayingCard>().SetCardState(true);
      
        Invoke("ActivateCard", 1);
        //GetComponent<Collider>().enabled = true;
       // thisCardsDeck = DeckType.PLAYER_HAND;
    }
   
    public void SetStartingPosition()
    {
        startingPosition = transform.localPosition;
        startingRotation = transform.localEulerAngles;
    }
    void ActivateCard()
    {
        GetComponent<Collider>().enabled = true;
    }
    public void ReInitialize()
    {
        InitializeCard = SetInitialData;
        SetInitialData();
    }
    public override void OnMouseEnter()
    {
        if(thisPlayerType == PLAYER_TYPE.LOCAL)
        {
            if (thisCardsDeck == DeckType.PLAYER_HAND)
            {

                if (followMouse)
                    return;


                InitializeCard?.Invoke();


                Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                //PlayerInsteraction.OnCardSelected?.Invoke(this.gameObject);
                GetComponent<MeshRenderer>().enabled = false;
                backImage.gameObject.SetActive(false);
                if (thisCardKey == Card_Key.RESOURSE)
                {
                    resourceParent.transform.DOLocalMove(new Vector3(resourceParent.transform.localPosition.x, 1.5f, -2f), 0.12f);
                    resourceParent.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.12f);
                    resourceParent.transform.DOScale(new Vector3(1.7477f, 1.7477f, 1.7477f), 0.2f);
                }
                else if(thisCardKey == Card_Key.MINISTER || thisCardKey == Card_Key.ABILITY)
                {
                    attackParent.transform.DOLocalMove(new Vector3(resourceParent.transform.localPosition.x, 1.5f, -2f), 0.12f);
                    attackParent.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.12f);
                    attackParent.transform.DOScale(new Vector3(1.7477f, 1.7477f, 1.7477f), 0.2f);
                }
                //foreach (Transform item in transform)
                //{
                //    if(item.gameObject.activeSelf)
                //    {
                //        item.transform.DOLocalMove(new Vector3(item.transform.localPosition.x, 120, -140), 0.12f);
                //        item.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.12f);
                //        item.transform.DOScale(new Vector3(93, 131, 8), 0.2f);
                //    }
                //}
             

                InitializeCard = null;
            }
            else if (thisCardsDeck == DeckType.CENTER_DECK || thisCardsDeck == DeckType.LEFT_DECK)
            {
                PlayerInsteraction.SetHighlightCard?.Invoke(this.gameObject, true);
            }
        }
        else
        {
            PlayerInsteraction.SetHighlightCard?.Invoke(this.gameObject, true);
        }

       
       
       
    }
    bool isExiting = false;
    public override void OnMouseExit()
    {
        if (thisPlayerType == PLAYER_TYPE.LOCAL)
        {
            if (thisCardsDeck == DeckType.PLAYER_HAND)
            {

                if (followMouse)
                    return;

                if (thisCardKey == Card_Key.RESOURSE)
                {
                    resourceParent.transform.DOLocalMove(new Vector3(0, 0, 0), 0.12f).OnComplete(() => { backImage.gameObject.SetActive(true); GetComponent<MeshRenderer>().enabled = true; });
                    resourceParent.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.12f);
                    resourceParent.transform.DOScale(new Vector3(1, 1, 1), 0.2f);
                }
                else if (thisCardKey == Card_Key.MINISTER || thisCardKey == Card_Key.ABILITY)
                {
                    attackParent.transform.DOLocalMove(new Vector3(0, 0, 0), 0.12f).OnComplete(() => { backImage.gameObject.SetActive(true); GetComponent<MeshRenderer>().enabled = true; }); ;
                    attackParent.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.12f);
                    attackParent.transform.DOScale(new Vector3(1, 1, 1), 0.2f);
                }


                //foreach (Transform item in transform)
                //{
                //    if(item.gameObject.activeSelf)
                //    {
                //        item.transform.DOLocalMove(startingPosition, 0.0f);
                //        item.transform.DOLocalRotate(startingRotation, 0.12f);
                //        item.transform.DOScale(new Vector3(61, 86, 8), 0.2f);
                //    }
                //}




            }
            else if (thisCardsDeck == DeckType.CENTER_DECK || thisCardsDeck == DeckType.LEFT_DECK)
            {
                PlayerInsteraction.SetHighlightCard?.Invoke(this.gameObject, false);
            }
        }
        else
        {
            PlayerInsteraction.SetHighlightCard?.Invoke(this.gameObject, false);
        }
    }

    public void OnMouseDown()
    {
        if(thisPlayerType == PLAYER_TYPE.LOCAL)
        {
            if (thisCardsDeck == DeckType.PLAYER_HAND)
            {
                followMouse = true;
                if (thisCardKey == Card_Key.RESOURSE)
                {
                    resourceParent.transform.DOLocalMove(new Vector3(0, 0, 0), 0.12f).OnComplete(() => { backImage.gameObject.SetActive(true); GetComponent<MeshRenderer>().enabled = true; });
                    resourceParent.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.12f);
                    resourceParent.transform.DOScale(new Vector3(1, 1, 1), 0.2f);
                }
                else if (thisCardKey == Card_Key.MINISTER || thisCardKey == Card_Key.ABILITY)
                {
                    attackParent.transform.DOLocalMove(new Vector3(0, 0, 0), 0.12f).OnComplete(() => { backImage.gameObject.SetActive(true); GetComponent<MeshRenderer>().enabled = true; }); ;
                    attackParent.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.12f);
                    attackParent.transform.DOScale(new Vector3(1, 1, 1), 0.2f);
                }

                PlayerInsteraction.OnCardFollow?.Invoke(gameObject);
            }
        }

        
    }

    public void OnMouseUp()
    {
        if(thisPlayerType == PLAYER_TYPE.LOCAL)
        {
            if (thisCardsDeck == DeckType.PLAYER_HAND)
            {
                followMouse = false;                
                PlayerInsteraction.StopFollow?.Invoke(startingPosition, startingRotation, gameObject);
            }
        }     
      
    }



    public void Attack(Transform t)
    {
         SetInitialData();
        // Vector3 power = transform.position - t.position;
        //transform.DOPunchPosition(power.normalized * 10, 2.4f, 1);

        transform.DOMove(t.position, 0.3f).SetEase(Ease.InFlash).OnComplete(() => transform.DOLocalMove(startingPosition, 0.3f)

        .OnComplete(() => {

            transform.GetComponent<PlayingCard>().SetCardGlow?.Invoke(false);
        })

        ); ;
    }

    
}
