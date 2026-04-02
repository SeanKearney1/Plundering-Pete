using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    
    private float CutsceneTimer;

    private int PlayersCurrentShip;
    private int PlayersCurrentZone;

    private GameObject TheLevelUI;
    private GameObject ThePlayer;
    private GameObject[] TheFleet;
    private SessionManager sessionManager;
    private List<GameObject> PeopleInCutscene;
    private Level_Info levelInfo;
    private GameObject CutsceneBoat;
    private Animator animator;
    private Vector2 playerParentPos;
    private bool DidPlayerWin;
    private bool InCutscene = true;
    private bool HasDoneCutscene = false;
    private bool DetachedFromLadder = false;
    private bool GameIsOver = false;
    private bool StartedGameOver = false;
    private float GameStartTimeStamp;
    private float GameEndTimeStamp;
    

    void Start()
    {
        TheLevelUI = GameObject.Find("Level Canvas");
        ThePlayer = GameObject.FindWithTag("Player");
        TheFleet = GameObject.FindGameObjectsWithTag("ShipTrigger");
        sessionManager = GameObject.FindWithTag("DavyJones").GetComponent<SessionManager>();
        levelInfo = GetComponent<Level_Info>();

        ThePlayer.GetComponent<PlayerLogic>().SetPoseidon(gameObject);
        ThePlayer.GetComponent<MovementLogic>().SetPoseidon(gameObject);

        GameStartTimeStamp = Time.time;

        GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");

        for (int i = 0; i < bots.Length; i++) { bots[i].GetComponent<MovementLogic>().SetPoseidon(gameObject); }
    }


    void Update()
    {
        if (!GameIsOver) { CheckForGameFinished(); }

        // If it is still not a game over continue as usual.
        if (!GameIsOver) {
            if (!HasDoneCutscene) { BeginCutscene(); }
            if (InCutscene) {
                Cutscene();
            }
            else
            {
                GetPlayersCurrentShipAndZone();
            }

            if (Input.GetKeyDown(KeyCode.Escape)) { TheLevelUI.GetComponent<LevelUI>().PauseLogic(); }
        }
        else { GameOver(); }
    }


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  G  E  T  T  E  R      F  U  N  C  T  I  O  N  S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    public GameObject RetrievePlayerShip()
    {
        if (PlayersCurrentShip == -1) { return null; }
        return TheFleet[PlayersCurrentShip]; 
    }
    public int RetrievePlayerZone() { return PlayersCurrentZone; }


    private void GetPlayersCurrentShipAndZone()
    {
        int new_zone;
        if (PlayersCurrentShip != -1 && TheFleet[PlayersCurrentShip].GetComponent<BotsManager>().GetPlayersCurrentZone() != -1)
        {
            PlayersCurrentZone = TheFleet[PlayersCurrentShip].GetComponent<BotsManager>().GetPlayersCurrentZone();
            return;
        }

        for (int i = 0; i < TheFleet.Length;i++)
        {
            new_zone = TheFleet[i].GetComponent<BotsManager>().GetPlayersCurrentZone();
            if (new_zone != -1)
            {
                PlayersCurrentShip = i;
                PlayersCurrentZone = new_zone;
                return;
            }
        }
        PlayersCurrentShip = -1;
        PlayersCurrentZone = -1;
    }


    private GameObject GetFirstLadder()
    {
        if (TheFleet.Length > 0)
        {
            return TheFleet[0].GetComponent<BotsManager>().GetOceanLadders()[0].GetComponent<OceanLadder>().gameObject;
        }
        return null;
    }



    public List<GameObject> GetEveryone()
    {
        List<GameObject> everyone = new List<GameObject> {ThePlayer};

        for (int i = 0; i < TheFleet.Length; i++)
        {
            List<GameObject> cur_crew = TheFleet[i].GetComponent<BotsManager>().GetCrew();
            for (int q = 0; q < cur_crew.Count; q++)
            {
                everyone.Add(cur_crew[q]);
            }
        }

        return everyone;
    }


    public List<Transform> GetGrapplePoints(Vector3 player_pos)
    {
        List<Transform> grapple_points = new List<Transform>();
        List<Transform> new_grapple_points;

        for (int i = 0; i < TheFleet.Length; i++)
        {
            new_grapple_points = TheFleet[i].GetComponent<BotsManager>().GetGrapplePoints(); 

            for (int q = 0; q < new_grapple_points.Count; q++)
            {
                if ((transform.position - new_grapple_points[q].position).magnitude <= GeneralGameInfo.Const_MaxGrappleDistance)
                {
                    grapple_points.Add(new_grapple_points[q]);
                }
            }
        }
        Debug.Log("Total Amount of Grapple Points in level is "+grapple_points.Count);
        return grapple_points;
    }


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  G  A  M  E      S  T  A  T  E      F  U  N  C  T  I  O  N  S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




////////////////////////////////////////////////
//  C  U  T  S  C  E  N  E
////////////////////////////////////////////////

    private void BeginCutscene()
    {
        CutsceneTimer = levelInfo.CutsceneTime;
        PeopleInCutscene = GetEveryone();
        HasDoneCutscene = true;


        for (int i = 0; i < PeopleInCutscene.Count; i++)
        {
            if (!PeopleInCutscene[i].GetComponent<BotLogic>().IsUnityNull()) // Is a bot.
            {
                PeopleInCutscene[i].GetComponent<BotLogic>().enabled = false;
                PeopleInCutscene[i].GetComponent<MovementLogic>().enabled = false;
            }
            else
            {
                InitializeCutsceneForPlayer(PeopleInCutscene[i]);
            }
        }
    }


    private void InitializeCutsceneForPlayer(GameObject player)
    {
            // Set starting animation
            animator = player.GetComponent<Animator>();
            animator.SetInteger("Cutscene",sessionManager.GetShipType()+1);
            animator.SetInteger("Level",levelInfo.Level);

            // Set direction and freeze speed.
            player.GetComponent<MovementLogic>().flipSprite(1);
            player.GetComponent<Rigidbody2D>().linearVelocityY = 0;

            // Disable logic
            player.GetComponent<Rigidbody2D>().gravityScale = 0;
            player.GetComponent<PlayerLogic>().enabled = false;
            player.GetComponent<PlayerInput>().enabled = false;
            player.GetComponent<MovementLogic>().Enable(false);


            // Create Ship
            CutsceneBoat = Instantiate(GeneralGameInfo.Ships[sessionManager.GetShipType()], new Vector3(0,0,0), Quaternion.identity);

            // Move player to position.
            ThePlayer.transform.position = levelInfo.PlayerStartCutscenePositions[sessionManager.GetShipType()];

            if (sessionManager.GetShipType() < 2)
            { 
                CutsceneBoat.GetComponent<SmallBoatWaveLogic>().SetWaveStats(levelInfo.WaveSize, levelInfo.WaveFrequency);
                ThePlayer.transform.parent = CutsceneBoat.transform;
                playerParentPos = ThePlayer.transform.localPosition;
            }
    }




    private void Cutscene()
    {
        CutsceneTimer -= Time.deltaTime;

        if (!DetachedFromLadder && (levelInfo.CutsceneTime-CutsceneTimer) >= levelInfo.DetachFromBoatTime)
        {
            DetachedFromLadder = true;
            ThePlayer.transform.parent = null;

            animator.SetInteger("Cutscene",0);
            animator.SetInteger("Level",0);
            ThePlayer.GetComponent<Rigidbody2D>().linearVelocityX = 0;
            CutsceneBoat.GetComponent<Rigidbody2D>().linearVelocityX = 0;
            ThePlayer.GetComponent<MovementLogic>().Enable(true);
            ThePlayer.GetComponent<MovementLogic>().SetOceanLadder(GetFirstLadder());
        }


        else if (!DetachedFromLadder && sessionManager.GetShipType() < 2)
        {
            float vel = levelInfo.SmallBoatDistance / levelInfo.DetachFromBoatTime;
            CutsceneBoat.GetComponent<Rigidbody2D>().linearVelocityX = vel;
            ThePlayer.transform.localPosition = playerParentPos;
        }

        if (CutsceneTimer <= 0 && (sessionManager.GetShipType() >= 2 || !ThePlayer.GetComponent<MovementLogic>().Get_OnOceanLadder()))
        {
            InCutscene = false;
            EndCutscene();
        }
    }




    private void EndCutscene()
    {
        for (int i = 0; i < PeopleInCutscene.Count; i++)
        {
            if (!PeopleInCutscene[i].GetComponent<BotLogic>().IsUnityNull()) // Is a bot.
            {
                PeopleInCutscene[i].GetComponent<BotLogic>().enabled = true;
                PeopleInCutscene[i].GetComponent<MovementLogic>().enabled = true;
            }
            else
            {
                EndCutsceneForPlayer(PeopleInCutscene[i]);
            }
        }
    }


    private void EndCutsceneForPlayer(GameObject player)
    {
        //Animator animator = player.GetComponent<Animator>();
        player.GetComponent<Rigidbody2D>().gravityScale = 1;
        player.GetComponent<PlayerLogic>().enabled = true;
        player.GetComponent<PlayerInput>().enabled = true;
        player.GetComponent<MovementLogic>().Enable(true);
    }










    






    private bool PlayerHasWon()
    {
        bool is_all_bots_dead = true;
        bool is_all_ships_sunk = true;
        for (int i = 0; i < TheFleet.Length; i++)
        {
            if (TheFleet[i].GetComponent<BotsManager>().GetCrewCount() > 0) { is_all_bots_dead = false; }
            if (!TheFleet[i].GetComponent<ShipLogic>().IsSunk()) { is_all_ships_sunk = false; }
            if (!(is_all_bots_dead || is_all_ships_sunk)) { return false; }
        }
        return true;
    }



    private void CheckForGameFinished()
    {
        if (ThePlayer.GetComponent<HealthManager>().IsPlayerDead()) 
        {
            GameIsOver = true;
            DidPlayerWin = false;
        }
        else if (PlayerHasWon())
        {
            GameIsOver = true;
            DidPlayerWin = true;
        }
    }


    private void GameOver()
    {
        if (!StartedGameOver) { GameOverStart(); }
    }

    private void GameOverStart()
    {
        Debug.Log("GAME OVER START!!!!");
        StartedGameOver = true;
        TheLevelUI.GetComponent<LevelUI>().EnableGameOverUI(DidPlayerWin);

        
    }


}
