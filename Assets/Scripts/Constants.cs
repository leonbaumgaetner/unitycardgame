using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants 
{
    public static Vector3 delayIconScale = new Vector3(0.0044f, 0.0016f, 0.28f);
    public static Vector3 abilityIconPosition = new Vector3(0, 0.0378f, -0.71f);
    public static Vector3 diplomaticIconScale = new Vector3(0.0054f, 0.0016f, 0.28f);
}

public enum DeckType { PLAYER_HAND, CENTER_DECK, RIGHT_SIDE, LEFT_DECK, MAIN_DECK, DISCARD_DECK};

public enum CardType { RED, GREEN, BLUE, WHITE};

public enum GAME_MODE { PLACING, ATTACKING, DEFENDING, NONE}; //game mode is for local player

public enum PLAYER_TYPE { LOCAL, ENEMY};

public enum Card_Key { MINISTER, RESOURSE, ABILITY};

public enum Resource_Type { DONKEY, ELEPHANT, EAGLE};

public enum Turn { LOCAL, ENEMY};

public enum PHASE { NORMAL, ATTACK };

public enum AbilityType
{
    NONE,
    AFFLICT,
    RALLY,
    ENHANCE,
    DELAY,
    DIPLOMATIC_IMMUNITY,    
    GERRYMANDER,
    RECOUNT,
    RECALL,
    MANIPULATE,
    DONATE,
    PATSY,
    CHARGE,
    DEBATE,
    SMEAR
    
}


[System.Serializable]
public class Card
{
    public Card_Key _Key;
    public string title;
    public string imageName;
    public int id = 0;
    public AbilityType thisCardAbility = AbilityType.NONE;
    public Card(Card_Key key, string title, string imageName)
    {
        _Key = key;
        this.title = title;
        this.imageName = imageName;
    }
}
[System.Serializable]
public class AttackCard : Card
{
    public string name;
   
    public int attack;
    public int defense;
    public int hireCost;
    public int voteValue;
    
    public Resource_Type resource_Type;

    public AttackCard(string name, int attack, int defense, int hireCost, int voteValue, Resource_Type resource_Type, Card_Key key, string title, string imageName) : base (key, title, imageName)
    {
        this.name = name;
        this.attack = attack;
        this.defense = defense;
        this.hireCost = hireCost;
        this.voteValue = voteValue;
        this.resource_Type = resource_Type;
    }
}
[System.Serializable]
public class ResourceCard : Card
{
   
    public Resource_Type resource_Type;
    public int resourceCount;
    public string profilePicture;

    public ResourceCard(Resource_Type resource_Type, int resourceCount, string profilePic, Card_Key key, string title, string imageName) : base(key, title, imageName)
    {
        this.resource_Type = resource_Type;
        this.resourceCount = resourceCount;
        this.profilePicture = profilePic;
    }
}

[System.Serializable]
public class AttackCardsGroup
{
    
    public List<AttackCard> attackCards = new List<AttackCard>();

    public AttackCardsGroup(List<AttackCard> attackCards)
    {
        this.attackCards = attackCards;
    }
}
[System.Serializable]
public class ResourceCardsGroup
{
    public List<ResourceCard> resourceCards = new List<ResourceCard>();

    public ResourceCardsGroup(List<ResourceCard> resourceCards)
    {
        this.resourceCards = resourceCards;
    }
}


[System.Serializable]
public class DefendInformation
{
    public List<DefendPair> defendLines;

    public DefendInformation()
    {
        this.defendLines = new List<DefendPair>();
    }
}

[System.Serializable]
public class DefendPair
{
    public int defendingCardID;
    public int attackingCardID;

    public DefendPair(int defendingCardID, int attackingCardID)
    {
        this.defendingCardID = defendingCardID;
        this.attackingCardID = attackingCardID;
    }
}