using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PlayingCard : MonoBehaviour
{
    public CardType cardType;
    public GameObject glow;

    public UnityAction<bool> SetCardGlow;
    public UnityAction<bool> SetCardState;

    bool isCardDelpleted = false;
    public bool isFlipped = false;
    public bool IsCardDelpleted { get => isCardDelpleted; private set => isCardDelpleted = value; }

    public void Start()
    {
        SetCardGlow += SetGlow;
        //GameManager.OnGameModeChanged += CheckGameMode;
        SetCardState += SetDepletedState;
        TurnManager.OnTurnChangedTo += TurnChanged;
    }

    public void OnDestroy()
    {
        SetCardGlow -= SetGlow;
    }

    void CheckGameMode(GAME_MODE gAME_MODE)
    {
        if(gAME_MODE != GAME_MODE.ATTACKING)
        {
            SetGlow(false);
        }
    }


    public void SetRandomType()
    {
        int t = Random.Range(0,3);

        cardType = (CardType)t;
    }

    private void SetGlow(bool state)
    {
        if(glow == null)
        {
            Debug.LogError("Objet name " + gameObject.name);
        }
        else
        {
            glow.SetActive(state);
        }
      
    }

    private void TurnChanged(Turn t)
    {
        //check if there is Delay
        var da = GetComponentInChildren<DelayAbility>();

        if (da)
        {
            return;
        }


        if (t == Turn.LOCAL && GetComponent<PlayerHandCard>().thisPlayerType == PLAYER_TYPE.LOCAL)
        {
            SetDepletedState(true);
        }
        else if(t == Turn.ENEMY && GetComponent<PlayerHandCard>().thisPlayerType == PLAYER_TYPE.ENEMY)
        {
            SetDepletedState(true);
        }
    }

    private void SetDepletedState(bool state)
    {      
        

       // Debug.LogError("Setting depleted state" + state);
        CardObject cardObject = GetComponent<CardObject>();
        PlayerHandCard currentPlayerHandCard = GetComponent<PlayerHandCard>();
        if (state == false)
        {
            isCardDelpleted = true;
            //fecth the images
            if(cardObject.thisCardKey == Card_Key.RESOURSE)
            {
                currentPlayerHandCard.resourceProfilePicture.color = Color.grey;
                currentPlayerHandCard.resourceCardPicture.color = Color.grey;
                currentPlayerHandCard.isActive = false;
            }
            else if(cardObject.thisCardKey == Card_Key.MINISTER)
            {
               currentPlayerHandCard.profilePicture.color = Color.grey;
               currentPlayerHandCard.attackCardPicture.color = Color.grey;
                currentPlayerHandCard.isActive = false;
            }

            SetGlow(false);
        }
        else
        {
            isCardDelpleted = false;
            //fecth the images
            if (cardObject.thisCardKey == Card_Key.RESOURSE)
            {
                currentPlayerHandCard.resourceProfilePicture.color = Color.white;
                currentPlayerHandCard.resourceCardPicture.color = Color.white;
                currentPlayerHandCard.isActive = true;
                GetComponent<Collider>().enabled = true;
            }
            else if (cardObject.thisCardKey == Card_Key.MINISTER)
            {
                currentPlayerHandCard.profilePicture.color = Color.white;
                currentPlayerHandCard.attackCardPicture.color = Color.white;
                currentPlayerHandCard.isActive = true;
                GetComponent<Collider>().enabled = true;
            }
        }
    }

    public void ShowCard(bool flip)
    {
        if (flip)
        { 
            transform.rotation = Quaternion.Euler(Vector3.zero);
            isFlipped = true;
        }
        else
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            isFlipped = false;
        }
    }
}
