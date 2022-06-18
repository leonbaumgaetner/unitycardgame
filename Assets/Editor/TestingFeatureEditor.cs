using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(TestingFeatures))]
public class TestingFeatureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TestingFeatures mp = (TestingFeatures)target;

        //if(GUILayout.Button("RightDeckToCenter"))
        //{
        //    mp.HandToCenter();
        //}

        //if(GUILayout.Button("RightDeckToLeft"))
        //{
        //    mp.HandToLeftDeck();
        //}

        //if (GUILayout.Button("DrawCard"))
        //{
        //    mp.DrawACardFromDeck();
        // }

        //if(GUILayout.Button("Discard Random Card"))
        //{
        //    mp.DiscardCard();
        //}

        //if (GUILayout.Button("Activate Random Attack"))
        //{
        //    mp.SetAttackables();
        //}

        //if(GUILayout.Button("Manual Attack"))
        //{
        //    mp.SetManualAttackables();
        //}


        //if (GUILayout.Button("Manual Play Resource Card"))
        //{
        //    mp.Enemy_PlayResourceCard();
        //}

        //if (GUILayout.Button("Manual Play Minister Card"))
        //{
        //    mp.Enemy_PlayMinisterCard();
        //}

        //if (GUILayout.Button("Manual End Opp Turn"))
        //{
        //    mp.Manual_EndEnemyTurn();
        //}

        //if (GUILayout.Button("Activate Attack cards for enemy"))
        //{
        //    mp.SelectEnemyCardsToAttack();
        //}

        //if (GUILayout.Button("Give Afflict Card to Player"))
        //{
        //    mp.GiveLocalPlayerAfflict();
        //}

        //if(GUILayout.Button("Give Rally Card to Player"))
        //{
        //    mp.GiveLocalPlayerRally();
        //}

        //if (GUILayout.Button("Give Enhance Card to Player"))
        //{
        //    mp.GiveLocalPlayerRally();
        //}

        //if (GUILayout.Button("Give Delay Card to Player"))
        //{
        //    mp.GivePlayerDelayCard();
        //}

        //if (GUILayout.Button("Give Diplomatic Card to Player"))
        //{
        //    mp.GivePlayerDimplomaticCard();
        //}

        //if (GUILayout.Button("Give Smear Card to Player"))
        //{
        //    mp.GivePlayerDimplomaticCard();
        //}

        if (GUILayout.Button("Give ABILITY CARD to Player"))
        {
            mp.GiveAAbilityTypeCard();
        }

        //if (GUILayout.Button("Give Resource Card to Player"))
        //{
        //    mp.GiveResourceCardToPlayer();
        //}

        //if (GUILayout.Button("Give CabinetMinister Card to Player"))
        //{
        //    mp.GiveCabinetCardToPlayer();
        //}
        base.DrawDefaultInspector();
    }
}
