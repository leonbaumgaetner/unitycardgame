using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PlayerProfile : MonoBehaviour
{
    public PLAYER_TYPE thisPlayerType;

    public UnityAction ProfileClickedUp;
   

    public void OnMouseEnter()
    {
        ProfileClickedUp?.Invoke();
    }
}
