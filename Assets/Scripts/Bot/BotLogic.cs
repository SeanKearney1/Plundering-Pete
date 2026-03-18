using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BotLogic : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  O  V  E  R  V  I  E  W
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// This script is to be applied to each bot.
// This script carries out the brain of a bot. IE make retreating bots run away or offense bots shoot.
// This script DOES NOT carry out behavior decision making.
// Behavior decision making is updated and set by the BotsManager script.
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// G E N E R I C   L O G I C   V A R I A B L E S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private GameObject ThePlayer;
    private BotsManager TheBotsManager;

    private Rigidbody2D rb;
    private MovementLogic movementLogic;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// M A I N   B E H A V I O R   V A R I A B L E S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private int BehaviorState = 0;
    private int BehaviorOverride = 0;
    private float Behavior_Bravery;
    private float Behavior_Intellect;
    private int Behavior_Mobility; // Bit Flag: Crouch, Jump, Somersault, Jump Flip, Grapple.

    private float Multiplier_Damage;
    private float Multiplier_Speed;
    private float Multiplier_Weapon_Speed;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// A D D I T I O N A L   B E H A V I O R   L O G I C   V A R I A B L E S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private Vector2 DesiredPosition = new Vector2(0,0);
    private List<Vector2> DesiredPath = new List<Vector2>();
    private int CurrentZoneLevel;
    private int PlayersCurrentZoneLevel;
    private bool RouteRecalculationNeeded = false;
    private bool FoundObjective = false;

    private float MinDistanceToCheckPoint = 0.25f;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// B O T   M O V E M E N T   I N P U T   V A R I A B L E S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private bool IsUsingMovmentCombo;
    private bool IsUsingCutlass;
    private bool IsUsingPistol;
    private bool IsUsingGrenade;
    private bool IsUsingGrapple;
    private bool IsDucking;
    private bool IsTryingToJump;
    private bool IsWalking;
    private Vector2 Look = new Vector2(0,0);
    private int CurrentLadderDirection = -1;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////





    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movementLogic = GetComponent<MovementLogic>();
    }


    void Update()
    {
        ResetInputs();
        UpdateCurrentZone();
        Behavior();
        SetMovementInputs();
    }



//////////////////////////////////////////////////////////////////////////////////////
// I  N  I  T  I  A  L  I  Z  E  R  S
//////////////////////////////////////////////////////////////////////////////////////

    public void SetManager(BotsManager bot_manager)
    {
        TheBotsManager = bot_manager;
    }
    public void SetPlayer(GameObject player)
    {
        ThePlayer = player;
    }

    public void SetBehaviors(float bb, float bi, int bm)
    {
        Behavior_Bravery = bb;
        Behavior_Intellect = bi;
        Behavior_Mobility = bm;
    }

    public void SetMultipliers(float md, float ms, float mws)
    {
        Multiplier_Damage = md;
        Multiplier_Speed = ms;
        Multiplier_Weapon_Speed = mws;
    }

//////////////////////////////////////////////////////////////////////////////////////
// U  P  D  A  T  E  R  S
//////////////////////////////////////////////////////////////////////////////////////

    private void UpdateCurrentZone()
    {
        int zone_count = TheBotsManager.GetZoneCount();
        BoxCollider2D cur_zone;

        for (int i = 0; i < zone_count;i++)
        {
            cur_zone = TheBotsManager.GetZoneCollider(i);
            if (rb.IsTouching(cur_zone))
            {
                CurrentZoneLevel = i;
                return;
            }
        }
        CurrentZoneLevel = -1;
    }



//////////////////////////////////////////////////////////////////////////////////////
// B  E  H  A  V  I  O  R         S  T  A  T  E  S
//////////////////////////////////////////////////////////////////////////////////////

    public int GetBehavior() { return BehaviorState; }
    public void UpdateBehavior(int b)
    {
        Debug.Log("Behavior Updated to "+b);
        BehaviorState = b;
        RouteRecalculationNeeded = true;
    }



    private void Behavior()
    {
        if (BehaviorOverride == 0)
        {
            if (BehaviorState == 0)      { BehaveHibernation(); }
            else if (BehaviorState == 1) { BehaveOffensive(); }
            else if (BehaviorState == 2) { BehaveDefensive(); }
        }
        else if (BehaviorOverride == 1) { BehaveRetreat(); }
    }

//////////////////////////////////////////////////////////////////////////////////////
// C  O  R  E      B  E  H  A  V  I  O  R      L  O  G  I  C
//////////////////////////////////////////////////////////////////////////////////////


    public void UpdatePlayerLevel(int new_zone)
    {
        if (PlayersCurrentZoneLevel != new_zone)
        {

            RouteRecalculationNeeded = true;
            PlayersCurrentZoneLevel = new_zone;
        }
    }





////////////////////////////////////////////////
// C A L C U L A T I N G   P A T H
////////////////////////////////////////////////



    private void RecalculateRoute(int ObjectivesCurrentZoneLevel)
    {
        DesiredPosition = CalcDesiredPosition();
        
        List<Vector2> checkpoints = new List<Vector2>();
        List<EntityId> VisitedLadders = new List<EntityId>();
        Vector2 cur_pos = transform.position;
        int level_destination;
        int cur_level = CurrentZoneLevel;

        if (ObjectivesCurrentZoneLevel == -1) { level_destination = TheBotsManager.GetTopDeck(); }
        else { level_destination = ObjectivesCurrentZoneLevel; }

        RouteRecalculationNeeded = false;

        FoundObjective = false;
        DesiredPath = CalculatePath(DesiredPosition, cur_pos, checkpoints, VisitedLadders, level_destination, cur_level);

        // Show Path.
        debug__ShowPath(cur_pos);
    
    }





    private Vector2 CalcDesiredPosition()
    {
        Vector2 new_desired_pos = new Vector2(0,0);

        if (BehaviorOverride == 0)
        {
            if (BehaviorState == 1) // Offense
            {
                return ThePlayer.transform.position;
            }
            else if (BehaviorState == 2) // Defense
            {
                
            }
        }
        else if (BehaviorOverride == 1) // Retreat
        {
            
        }

        return new_desired_pos;
    }




        ////////////////////////////////////////////
        // F U N C T I O N   O V E R V I E W
        ////////////////////////////////////////////
        //  CalculatePath recursivley checks the
        //  entire ship using the recursive
        //  A* algorithm.
        // 
        //
        //
        //
        //
        //
        //
        ////////////////////////////////////////////
    private List<Vector2> CalculatePath(Vector2 final_pos, Vector2 cur_pos, List<Vector2> checkpoints, List<EntityId> VisitedLadders, int level_destination, int cur_level)
    {
        List<Transform> ladders;
        LayerMask layerMask = LayerMask.GetMask("Hull");
        Vector2 new_pos;
        Vector2 adjusted_ladder_pos;
        int new_level;

        //cur_pos.y += UnityEngine.Random.Range(-0.5f,0.5f);

        //Debug.Log("Checking Level "+cur_level+"!  Player is on level "+level_destination);

        ladders = TheBotsManager.GetZonesAvailableLadderPositions(cur_level);



        if (cur_level == level_destination)
        {
            
            if (CanReachPath(cur_pos, new Vector2(final_pos.x, cur_pos.y)))
            {
                //Debug.Log("Located Objective!");
                //Debug.DrawLine(cur_pos, new Vector2(final_pos.x, cur_pos.y), Color.white, .100f);
                FoundObjective = true;
                return checkpoints;
            }
        }


        foreach (Transform ladder in ladders)
        {

            if (!LadderAlreadyChecked(VisitedLadders, ladder.GetEntityId())) {

                adjusted_ladder_pos = new Vector2(ladder.position.x,cur_pos.y);

                if (!Physics2D.Linecast(adjusted_ladder_pos, cur_pos, layerMask, 0, 0))
                {
                    // Add this ladder to visited ladders.
                    VisitedLadders.Add(ladder.GetEntityId());

                    // Get the new level and position after the pathfinder climbs the ladder.
                    new_level = TheBotsManager.GetLadderLevel(ladder);
                    new_pos = GetEndLadder(ladder.position, cur_level, new_level, TheBotsManager.GetLadderHeight(ladder));

                    // If the ladder is on the current level, the ladder goes up to cur_level-1;
                    if (new_level == cur_level) { new_level--; }
                    if (new_level < 0) { new_level = 0; }

                    //Debug.DrawLine(adjusted_ladder_pos, cur_pos, new Color(0, UnityEngine.Random.Range(128,255), 0), .100f);
                    //Debug.DrawLine(adjusted_ladder_pos, new_pos, new Color(0, 0, UnityEngine.Random.Range(128,255)), .100f);

                    checkpoints = CalculatePath(final_pos, new_pos, checkpoints, VisitedLadders, level_destination, new_level);

                    if (FoundObjective)
                    {
                        checkpoints.Add(ladder.position);
                        //Debug.Log("Building Path!!!");
                        return checkpoints;
                    }

                }





                else
                {
                    //Debug.DrawLine(adjusted_ladder_pos, cur_pos, new Color(UnityEngine.Random.Range(128,255), 0, 0), .100f);
                }
            }
        }

        return new List<Vector2>();
    }




    private bool CanReachPath(Vector2 final_pos, Vector2 cur_pos)
    {
        LayerMask layerMask = LayerMask.GetMask("Hull");
        if (Physics2D.Linecast(final_pos, cur_pos, layerMask, 0, 0)) { return false; }
        return true;
    }

    private bool LadderAlreadyChecked(List<EntityId> Ladders, EntityId Ladder)
    {
        foreach (EntityId cur_ladder in Ladders)
        {
            if (cur_ladder == Ladder) { return true; }
        }
        return false;
    }

    private Vector2 GetEndLadder(Vector2 ladder_pos, int cur_level, int ladder_level, float ladder_height)
    {
        // Going Down
        if (cur_level == ladder_level) { ladder_pos.y += ladder_height; }
        return ladder_pos;
    }
////////////////////////////////////////////////























//////////////////////////////////////////////////////////////////////////////////////////////////
//  O  F  F  E  N  S  E
//////////////////////////////////////////////////////////////////////////////////////////////////
    private void BehaveOffensive()
    {

        // Needs to calculate path to player.
        if (RouteRecalculationNeeded) { RecalculateRoute(PlayersCurrentZoneLevel); }

        // Needs to move to player.
        else if (DesiredPath.Count > 0 && (CurrentZoneLevel != PlayersCurrentZoneLevel || !CanReachPath(DesiredPosition, transform.position)))
        {
            ProgressAlongPath();
        }
        // Finish climbing ladder.
        else if (CurrentLadderDirection != -1)
        {
            ProgressLadder(DesiredPath[DesiredPath.Count-1]);
        }
        // Is within line of sight of the player.
        else
        {
            AttackPlayer();
        }
    }



    private void AttackPlayer()
    {
        if (transform.position.x < ThePlayer.transform.position.x)
        {
            Look.x = 1;
        }
        else
        {
            Look.x = -1;
        }
    }


//////////////////////////////////////////////////////////////////////////////////////////////////
//  D  E  F  E  N  S  E
//////////////////////////////////////////////////////////////////////////////////////////////////

    private void BehaveDefensive()
    {
        Debug.Log("Bot is defensive!!!");
    }



//////////////////////////////////////////////////////////////////////////////////////////////////
//  H  I  B  E  R  N  A  T  I  O  N
//////////////////////////////////////////////////////////////////////////////////////////////////
    private void BehaveHibernation()
    {
        //Debug.Log("Bot is Hibernating!!!");
    }

//////////////////////////////////////////////////////////////////////////////////////////////////
//  R  E  T  R  E  A  T
//////////////////////////////////////////////////////////////////////////////////////////////////

    private void BehaveRetreat()
    {
        Debug.Log("Bot is Retreating!!!");
    }













//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  B  O  T      M  O  V  E  M  E  N  T      F  U  N  C  T  I  O  N  S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




    private void SetMovementInputs()
    {
        if (Look.x != 0) { IsWalking = true; }
        else { IsWalking = false; }
        movementLogic.Set_IsUsingMovmentCombo(IsUsingMovmentCombo);
        movementLogic.Set_IsUsingCutlass(IsUsingCutlass);
        movementLogic.Set_IsUsingPistol(IsUsingPistol);
        movementLogic.Set_IsUsingGrenade(IsUsingGrenade);
        movementLogic.Set_IsUsingGrapple(IsUsingGrapple);
        movementLogic.Set_IsDucking(IsDucking);
        movementLogic.Set_IsWalking(IsWalking);
        movementLogic.Set_IsTryingToJump(IsTryingToJump);
        movementLogic.Set_Look(Look);
    }

    private void ResetInputs()
    {
        Look = new Vector2(0,0);
    }


////////////////////////////////////////////////
// M O V I N G   A L O N G   P A T H
////////////////////////////////////////////////


    private void ProgressAlongPath()
    {
        Vector2 next_checkpoint = DesiredPath[DesiredPath.Count-1];
        bool close_to_checkpoint = false;

        Debug.DrawLine(transform.position, next_checkpoint, Color.black, 0.1f);

        if (Math.Abs(transform.position.x - next_checkpoint.x) <= MinDistanceToCheckPoint) { close_to_checkpoint = true;}


        // If not on a ladder and if not close to a ladder, walk towards it.
        if (CurrentLadderDirection == -1 && !close_to_checkpoint)
        {
            ProgressToNextCheckPoint(next_checkpoint.x);
        }
        // If on or just getting on a ladder, do ladder logic.
        else if (CurrentLadderDirection != -1 || close_to_checkpoint)
        { 
            ProgressLadder(next_checkpoint); 
        }
    }


    private void ProgressToNextCheckPoint(float cp_x)
    {
        // Checkpoint to the left.
        if (transform.position.x > cp_x) { Look.x = -1; }
        else { Look.x = 1; }
    }








    private void ProgressLadder(Vector2 checkpoint)
    {
        bool player_above_ladder = false;

        if (transform.position.y > TheBotsManager.GetLadderFromPos(checkpoint).transform.position.y) { player_above_ladder = true; }


        // Sets initial ladder direction so code knows what direction the bot is going.
        if (CurrentLadderDirection == -1) {
            if (player_above_ladder) { CurrentLadderDirection = 0; }
            else { CurrentLadderDirection = 1; }
        }
        else if (LadderFinishedValidator(player_above_ladder)) // Checks to see if bot is finished with the ladder.
        {
            LadderFinished();
        }

        // Down, Up
        if (CurrentLadderDirection == 0) { IsDucking = true; }
        else if (CurrentLadderDirection == 1)
        { 
            Look.y = 1;
            IsTryingToJump = true;
        }
    }


    private bool LadderFinishedValidator(bool player_above_ladder)
    {
        if (CurrentLadderDirection == 0 && !player_above_ladder) { return true; }

        if (CurrentLadderDirection == 1)
        {
            if (player_above_ladder && movementLogic.GetCurrentCollidedLadder().IsUnityNull()) { return true; }
        }

        return false;
    }



    private void LadderFinished()
    {
        if (CurrentLadderDirection == 0) { IsDucking = false; }
        else
        {
            Look.y = 0;
            IsTryingToJump = false;
        }

        CurrentLadderDirection = -1;
        DesiredPath.RemoveAt(DesiredPath.Count-1);
    }



























////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  D  E  B  U  G
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void debug__ShowPath(Vector2 cur_pos)
    {
        if (DesiredPath.Count > 0) {
            Debug.DrawLine(cur_pos, DesiredPath[DesiredPath.Count-1], Color.deepPink, 10f);
            for (int i = DesiredPath.Count-2; i >= 0;i--)
            {
                Debug.DrawLine(DesiredPath[i+1], DesiredPath[i], Color.gold, 10f);
            }
            Debug.DrawLine(DesiredPath[0], DesiredPosition, Color.darkGoldenRod, 10f);
        }
    }



}
