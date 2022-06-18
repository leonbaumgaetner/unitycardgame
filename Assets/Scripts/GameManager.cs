using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
public class GameManager : MonoBehaviour
{
    public static UnityAction FetchPlayers;
    public static UnityAction<string> ResourceCardPlayed;
    public static UnityAction<string> MinisterCardPlayed;
    public static UnityAction<string> AbilityCardPlayed;

    public static UnityAction CardCanceledRightDeck;



    public static UnityAction<CardPlayer> OnLocalCardSet;
    public static UnityAction<CardPlayer> OnOpponentCardSet;

    public static bool isOnline = true;



    public static UnityAction<GAME_MODE> CurrentGameMode;

    public static UnityAction<GAME_MODE> OnGameModeChanged;

    private GAME_MODE thisGameMode;

    public GAME_MODE ThisGameMode { get => thisGameMode; private set => thisGameMode = value; }

    List<AttackCard> allAttackCards = new List<AttackCard>();
    [SerializeField]
    public AttackCardsGroup attackCardsGroup;
    [SerializeField]
    public ResourceCardsGroup resourceCardsGroup;
    
    public string attackCardsPath = "Assets/Resources/CardsData/AttackCards/AttackCardsData";
   
    public TextAsset attackTextFile = null; //local Data
    public TextAsset resourceTextFile = null; //local Data

    public DeckManager deckManager;
    private TurnManager turnManager;

    private CardPlayer localCardPlayer;
    private CardPlayer opponentCardPlayer;

    public static int cardID = 0;

    private void Awake()
    {
        CurrentGameMode += SetGameState;
        turnManager = FindObjectOfType<TurnManager>();
       // attackTextFile = (TextAsset)Resources.Load("CardsData/AttackCards/AttackCardsData");
       // resourceTextFile = (TextAsset)Resources.Load("CardsData/ResourceCards/ResourceCardsData");

        //// StreamReader reader = new StreamReader(Application.persistentDataPath + "/"+attackCardsPath);
        // Debug.Log(attackTextFile.text);
        // attackCardsGroup = JsonUtility.FromJson<AttackCardsGroup>(attackTextFile.text.ToString());
        // resourceCardsGroup = JsonUtility.FromJson<ResourceCardsGroup>(resourceTextFile.text.ToString());
        // deckManager.LoadAttackCardDeck(attackCardsGroup);
        // deckManager.LoadResourceCardDeck(resourceCardsGroup);


        //List<ResourceCard> rcards = new List<ResourceCard>();
        //for (int i = 0; i < 10; i++)
        //{
        //    ResourceCard resourceCard = new ResourceCard(Resource_Type.DONKEY, 2, "donkey", Card_Key.RESOURSE, "RESOURCE - DEMOCRATIC", "whitehouse");
        //    rcards.Add(resourceCard);
        //}
        //resourceCardsGroup = new ResourceCardsGroup(rcards);

        //Debug.Log(JsonUtility.ToJson(resourceCardsGroup));
    }

    public string CreateAttackDeck()
    {
        attackTextFile = (TextAsset)Resources.Load("CardsData/AttackCards/AttackCardsData");
       // resourceTextFile = (TextAsset)Resources.Load("CardsData/ResourceCards/ResourceCardsData");

        attackCardsGroup = JsonUtility.FromJson<AttackCardsGroup>(attackTextFile.text.ToString());
       // resourceCardsGroup = JsonUtility.FromJson<ResourceCardsGroup>(resourceTextFile.text.ToString());
        List<AttackCard> attackCards = new List<AttackCard>();
        for (int i = 0; i < 4; i++) //load 4 attack cards
        {
            int index = Random.Range(0, attackCardsGroup.attackCards.Count);

            attackCards.Add(attackCardsGroup.attackCards[index]);
        }

        AttackCardsGroup selectedAttackGroup = new AttackCardsGroup(attackCards);
        string attck = JsonUtility.ToJson(selectedAttackGroup);

        return attck;
    }

    public string CreateResourceDeck()
    {
        resourceTextFile = (TextAsset)Resources.Load("CardsData/ResourceCards/ResourceCardsData");
        resourceCardsGroup = JsonUtility.FromJson<ResourceCardsGroup>(resourceTextFile.text.ToString());

        List<ResourceCard> resourceCards = new List<ResourceCard>();

        for (int i = 0; i < 3; i++) //load 4 attack cards
        {
            int index = Random.Range(0, resourceCardsGroup.resourceCards.Count);

            resourceCards.Add(resourceCardsGroup.resourceCards[index]);
        }

        ResourceCardsGroup selectedResourceCards = new ResourceCardsGroup(resourceCards);
        string res = JsonUtility.ToJson(selectedResourceCards);

        return res;
    }

    public void CreateDeckForPlayer2()
    {

    }

    private void Start()
    {
        FetchPlayers += GetPlayers;
        ResourceCardPlayed += OnResourceCardPlayed;
        MinisterCardPlayed += OnMinisterCardPlayed;
        AbilityCardPlayed += OnAbilityCardPlayed;
        CardCanceledRightDeck += OnCardCancelled;
    }

    private void OnDestroy()
    {
        CurrentGameMode -= SetGameState;
        FetchPlayers -= GetPlayers;
        ResourceCardPlayed -= OnResourceCardPlayed;
        MinisterCardPlayed -= OnMinisterCardPlayed;
    }
    private void SetGameState(GAME_MODE gAME_MODE)
    {
        thisGameMode = gAME_MODE;
        OnGameModeChanged?.Invoke(thisGameMode);
    }


    #region online
    private void GetPlayers()
    {
        CardPlayer[] cardPlayers = FindObjectsOfType<CardPlayer>();
        if (cardPlayers[0].hasAuthority)
        {
            localCardPlayer = cardPlayers[0];
            OnOpponentCardSet?.Invoke(cardPlayers[1]);
        }
        else
        {
            localCardPlayer = cardPlayers[1];
            OnOpponentCardSet?.Invoke(cardPlayers[0]);
        }

        OnLocalCardSet?.Invoke(localCardPlayer);
       
    }

    private void OnResourceCardPlayed(string c)
    {
        localCardPlayer.CmdResourceCardPlayed(c);
    }

    private void OnMinisterCardPlayed(string a)
    {
        localCardPlayer.CmdMinisterCardPlayed(a);
    }

    private void OnAbilityCardPlayed(string a)
    {
        localCardPlayer.CmdAbilityCardPlayed(a);
    }

    private void OnCardCancelled()
    {
        localCardPlayer.CmdCardCancelledFromRightDeck();
    }

    #endregion
}
