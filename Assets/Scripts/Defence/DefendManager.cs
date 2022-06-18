using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class DefendManager : MonoBehaviour
{
    public static UnityAction OnDefendLineCreated;
    public static UnityAction OnDefendLineDeleted;

    public static UnityAction<DefendableCard, UnityAction<DefendLine>> LineForDefendableCard;
    public static UnityAction<DefendableCard> FinishedSettingLine;

    public static UnityAction<DefendableCard> OnCardUp;

    public DefendLine defendLine;
    public GameManager gameManager;

    private DefendLine currentFloatingDefendLine;

    private void Start()
    {
        LineForDefendableCard += CreateLineForCard;
        FinishedSettingLine += OnLineSetFinished;
        OnCardUp += CardUp;
    }

    private void CardUp(DefendableCard defendableCard)
    {
        

        if(currentFloatingDefendLine)
        {
            if(currentFloatingDefendLine.isLineSet == false)
            {
                DefendableCard dc = currentFloatingDefendLine.GetDefendableOwner(); //from
                defendableCard.GetComponent<AttackableCard>().SetDefendableCard(dc); //to 
                //assigned. Next if the line is deleted
                currentFloatingDefendLine.SetAttackingOwner(defendableCard);
                OnDefendLineCreated?.Invoke();
                // delete this reference
                FinishedSettingLine?.Invoke(defendableCard);
            }
        }




    }

    public void CreateLineForCard(DefendableCard defendableCard, UnityAction<DefendLine> callback)
    {
        DefendLine newDefendLine = Instantiate(defendLine, Vector3.zero, Quaternion.identity);
        newDefendLine.SetStart(defendableCard.transform);

        newDefendLine.SetLineOwner(defendableCard);

        callback?.Invoke(newDefendLine);
       
        currentFloatingDefendLine = newDefendLine;
    }

    private void OnLineSetFinished( DefendableCard defendableCard)
    {
        currentFloatingDefendLine.isLineSet = true;
        currentFloatingDefendLine = null;
    }
}
