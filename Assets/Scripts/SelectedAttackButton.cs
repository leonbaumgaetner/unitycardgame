using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SelectedAttackButton : MonoBehaviour
{
    public TextMeshProUGUI attackNumberText;   

    
    public void SetAttackText(int total)
    {
        

        if (total < 2)
            attackNumberText.text = "" + total + " ATTACK";
        else
            attackNumberText.text = "" + total + " ATTACKS";
    }
}
