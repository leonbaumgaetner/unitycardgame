using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
public class CardObject : MonoBehaviour
{
    public DeckType thisCardsDeck;
    public PLAYER_TYPE thisPlayerType;

    public TextMeshProUGUI cardItemName;
    public TextMeshProUGUI cardTitle;
    public TextMeshProUGUI attackText, defenseText, hireCostText,  voteText;

    public int attackPoint, defensePoint, hireCost, voteValue;

    public Image profilePicture,attackCardPicture;

    public Transform attackParent, resourceParent;

    public Transform backImage, abilityIconParent;

    public TextMeshProUGUI resourceCountText,resourceTitleText;
    public int resourcePoint;
    public Image resourceProfilePicture, resourceCardPicture;

    public Card_Key thisCardKey;            // { MINISTER, RESOURSE}; 
    public Resource_Type thisResourceType; //{ DONKEY, ELEPHANT, EAGLE };

    public Card thisCardData;

    public bool isActive = false;

    //public AbilityType thiscardability;
    private void Start()
    {
        
    }

    private void TurnChanged(Turn turn)
    {

    }

    public void LoadCardData(Card card)
    {
        if (card._Key == Card_Key.MINISTER)
        {
            attackParent.gameObject.SetActive(true);
            resourceParent.gameObject.SetActive(false);

            AttackCard attackCard = card as AttackCard;

            cardItemName.text = "" + attackCard.name;
            cardTitle.text = "" + attackCard.title;
            attackText.text = "" + attackCard.attack;
            defenseText.text = "" + attackCard.defense;
            hireCostText.text = "" + attackCard.hireCost;
            voteText.text = "" + attackCard.voteValue;

            attackPoint = attackCard.attack;
            defensePoint = attackCard.defense;
            hireCost = attackCard.hireCost;
            voteValue = attackCard.voteValue;

            thisCardKey = attackCard._Key;
            thisResourceType = attackCard.resource_Type;

            thisCardData = attackCard;

            profilePicture.sprite = Resources.Load<Sprite>("Ministers/" + attackCard.imageName);
        }
        else if (card._Key == Card_Key.RESOURSE)
        {
            attackParent.gameObject.SetActive(false);
            resourceParent.gameObject.SetActive(true);

            ResourceCard resourceCard = card as ResourceCard;
            resourceCountText.text = ""+resourceCard.resourceCount;
            resourceTitleText.text = "" + resourceCard.title;
            resourcePoint = resourceCard.resourceCount;

            thisCardKey = resourceCard._Key;
            thisResourceType = resourceCard.resource_Type;

            resourceProfilePicture.sprite = Resources.Load<Sprite>("ResourcesCards/" + resourceCard.profilePicture);
            resourceCardPicture.sprite = Resources.Load<Sprite>("ResourcesCards/" + resourceCard.imageName);

            thisCardData = resourceCard;
        }
    }

    public virtual void OnMouseEnter()
    {
       // Debug.Log("Card Selected");
    }

    public virtual void OnMouseExit()
    {
        //Debug.Log("Card DeSelected");
    }
}
