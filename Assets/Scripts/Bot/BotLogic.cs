using System;
using System.Collections.Generic;
using Unity.VisualScripting;

//using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
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
    private CapsuleCollider2D hitbox;
    private MovementLogic movementLogic;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// M A I N   B E H A V I O R   V A R I A B L E S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private int BehaviorState = 0;
    private int BehaviorOverride = 0;
    private float Behavior_Aggression;
    private float Behavior_Bravery;
    private float Behavior_Intellect;
    private float Behavior_Mobility;

    

    private float Multiplier_Damage;
    private float Multiplier_Speed;
    private float Multiplier_Weapon_Speed;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// B O T   T Y P E   B E H A V I O R   V A R I A B L E S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public bool Combo_AllowSomerSault;
    public bool Combo_AllowJumpFlip;
    public bool Combo_AllowBigSlash;
    public bool Combo_AllowUppercut;
    public bool Combo_AllowBlock;
    public bool Combo_AllowThrow;
    public bool Combo_AllowPistol;
    public bool Combo_AllowGrenade;
    public bool Combo_AllowGrapple;


    public float MeleeAttackDistance;
    public float RangedAttackDistance;
    public float GrenadeAttackDistance;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// A D D I T I O N A L   B E H A V I O R   L O G I C   V A R I A B L E S
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private Vector2 DesiredPosition = new Vector2(0,0);
    private List<Vector2> DesiredPath = new List<Vector2>();
    private int CurrentZoneLevel;
    private int PlayersCurrentZoneLevel;
    private int ObjectivesCurrentZoneLevel;
    private bool RouteRecalculationNeeded = false;
    private bool FoundObjective = false;
    private bool HasSpottedObjective = false;
    private float MinDistanceToCheckPoint = 0.25f;
    private int NextObjectiveMove = -1;
    private int NextObjectiveMoveType = -1;
    private float IdleTimer = 0;
    private float IdleCooldownMax = 3;
    private float IdleCooldown = 0;

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
    private bool IsSwimming = false;
    private Vector2 Look = new Vector2(0,0);
    private int CurrentLadderDirection = -1;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////





    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movementLogic = GetComponent<MovementLogic>();
        hitbox = transform.Find("HitBox").GetComponent<CapsuleCollider2D>();

        movementLogic.SetComboDelay(0);
    }


    void Update()
    {
        ResetInputs();
        UpdateTimers();
        UpdateCurrentZone();
        OverBoardStatus();
        if (movementLogic.Get_ComboCooldown() == 0) { Behavior(); }
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

    public void SetPersonality(float aggro, float bravery, float smarts, float jumpy, float damage_mult, float move_speed_mult, float wepn_speed_mult)
    {
        Behavior_Aggression = aggro;
        Behavior_Bravery = bravery;
        Behavior_Intellect = smarts;
        Behavior_Mobility = jumpy;

        Multiplier_Damage = damage_mult;
        Multiplier_Speed = move_speed_mult;
        Multiplier_Weapon_Speed = wepn_speed_mult;
    }




//////////////////////////////////////////////////////////////////////////////////////
// G E T T E R   F U N C T I O N S
//////////////////////////////////////////////////////////////////////////////////////
    public float GetDamageMultiplier() { return Multiplier_Damage; }


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


    private void UpdateTimers()
    {
        if (IdleTimer > 0) { IdleTimer -= Time.deltaTime; }
        else if (IdleTimer < 0) { IdleTimer = 0; }

        if (IdleTimer == 0) {
            if (IdleCooldown > 0) { IdleCooldown -= Time.deltaTime; }
            else if (IdleCooldown < 0) { IdleCooldown = 0; }
        }
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

    private void OverBoardStatus()
    {
        if (!IsSwimming && CurrentZoneLevel == -1)
        {
            IsSwimming = true;
            RouteRecalculationNeeded = true;
        }
        else if (IsSwimming && CurrentZoneLevel != -1)
        {
            IsSwimming = false;
            RouteRecalculationNeeded = true;
        }

    }



    private void Behavior()
    {
        if (IsSwimming) { BehaveSwim(); }
        else if (IdleTimer != 0) { BehaveIdle(); }
        else if (BehaviorOverride == 0)
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

    // This is called whenever an action is completed like: 
    // Climbing a ladder, attacking, reaching an objective, shooting a cannon.
    private void CompletedAnAction()
    {
        if (IdleCooldown == 0 && Behavior_Aggression < UnityEngine.Random.Range(0,1f)) { SetIdleTimer(); }
    }


    private void SetIdleTimer()
    {
        float max = TheBotsManager.GetComponent<BotsManager>().BotBehaviorRange_IdleTimerMax;
        float min = TheBotsManager.GetComponent<BotsManager>().BotBehaviorRange_IdleTimerMin;

        IdleTimer = min + ((max - 1) * UnityEngine.Random.Range(0, 1 - Behavior_Intellect));

        IdleCooldown = IdleCooldownMax;
        
    }



////////////////////////////////////////////////
// C A L C U L A T I N G   P A T H
////////////////////////////////////////////////



    private void RecalculateRoute()
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
                GameObject defenseObject = TheBotsManager.GetAnOpenDefensivePart();
                ObjectivesCurrentZoneLevel = TheBotsManager.GetObjectLevel(defenseObject.transform, defenseObject.tag);

                defenseObject.GetComponent<DefensiveShipPartInfo>().SetOwner(gameObject);

                return defenseObject.transform.position;
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
        List<Transform> ladders = TheBotsManager.GetZonesAvailableLadderPositions(cur_level);
        LayerMask layerMask = LayerMask.GetMask("Hull");
        Vector2 new_pos;
        Vector2 adjusted_ladder_pos;
        int new_level;

        if (cur_level == level_destination)
        {
            
            if (CanReachPath(cur_pos, new Vector2(final_pos.x, cur_pos.y)))
            {
                //Debug.DrawLine(cur_pos, new Vector2(final_pos.x, cur_pos.y), Color.white, 100f);
                FoundObjective = true;
                return checkpoints;
            }
        }




        //      Culls vistited ladders, adds these ladders to visited ladders, then sorts by distance. 
        //  The reason while they are all added to the list all at once and not after each individual 
        //  one is checked is because of the following reason:
        //
        //      If you have two ladders on the same level and they have LOS of eachother on both their level and the
        //  level their going to you create a loop, which can cause the bot to take longer routes or
        //  sometimes loop in areas they don't even have to go.  Doing this prevents the A* algorithm from
        //  small loops like this.
        //
        //      Another issue is if you have two ladders in LOS that go to the same place, the bot will pick the first
        //  in the list not the nearest one, so they are then sorted by distance. This same flaw causes the looping.

        ladders = CullVisitedLadders(ladders, VisitedLadders);
        for (int i = 0; i < ladders.Count;i++)
        {
            adjusted_ladder_pos = new Vector2(ladders[i].position.x,cur_pos.y);

            if (!Physics2D.Linecast(adjusted_ladder_pos, cur_pos, layerMask, 0, 0))
            {
                VisitedLadders.Add(ladders[i].GetEntityId());
                //Debug.DrawLine(new Vector2(ladders[i].position.x,cur_pos.y), cur_pos, new Color(0, UnityEngine.Random.Range(128,255), 0), 100f);
                //Debug.DrawLine(new Vector2(ladders[i].position.x,cur_pos.y), GetEndLadder(ladders[i].position, cur_level, TheBotsManager.GetLadderLevel(ladders[i]), TheBotsManager.GetLadderHeight(ladders[i])), new Color(0, 0, UnityEngine.Random.Range(128,255)), 100f);
            }
            else
            {
                ladders.RemoveAt(i);
                i--;
                //Debug.DrawLine(adjusted_ladder_pos, cur_pos, new Color(UnityEngine.Random.Range(128,255), 0, 0), 100f); 
            }
        }
        ladders = SortLaddersByDistance(ladders, cur_pos.x);




        for (int i = 0; i < ladders.Count;i++)
        {
            // Get the new level and position after the pathfinder climbs the ladder.
            new_level = TheBotsManager.GetObjectLevel(ladders[i],"ShipTrigger_Ladder");
            new_pos = GetEndLadder(ladders[i].position, cur_level, new_level, TheBotsManager.GetLadderHeight(ladders[i]));

            // If the ladder is on the current level, the ladder goes up to cur_level-1;
            if (new_level == cur_level) { new_level--; }
            if (new_level < 0) { new_level = 0; }

            // Recursion Point
            checkpoints = CalculatePath(final_pos, new_pos, checkpoints, VisitedLadders, level_destination, new_level);

            if (FoundObjective)
            {
                checkpoints.Add(ladders[i].position);
                return checkpoints;
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


    private List<Transform> CullVisitedLadders(List<Transform> ladders, List<EntityId> visited_ladders)
    {
        for (int i = 0; i < ladders.Count;i++)
        {
            if (LadderAlreadyChecked(visited_ladders, ladders[i].GetEntityId()))
            {
                ladders.RemoveAt(i);
                i--;
            }
        }
        return ladders;
    }




    private List<Transform> SortLaddersByDistance(List<Transform> ladders, float cur_posX)
    {
        bool made_switch = true;
        Transform switcher;

        while (made_switch)
        {
            made_switch = false;
            for (int i = 1; i < ladders.Count;i++)
            {
                if (Math.Abs(ladders[i-1].position.x - cur_posX) > Math.Abs(ladders[i].position.x - cur_posX))
                {
                    switcher = ladders[i-1];
                    ladders[i-1] = ladders[i];
                    ladders[i] = switcher;
                    made_switch = true;
                }
            }
        }

        return ladders;
    }


////////////////////////////////////////////////



////////////////////////////////////////////////
//  O B J E C T I V E   L O G I C
////////////////////////////////////////////////



    private bool InRangeToDoObjective(Vector2 objective_pos)
    {   
        float cur_distance = (new Vector2(transform.position.x,transform.position.y) - objective_pos).magnitude;
        float min_distance = -1;

        if (NextObjectiveMoveType == 1) { min_distance = MeleeAttackDistance; } // Melee
        else if (NextObjectiveMoveType == 2) { min_distance = RangedAttackDistance; } // Ranged
        else if (NextObjectiveMoveType == 3) { min_distance = GrenadeAttackDistance; } // Grenade

        if (min_distance != -1 && cur_distance > min_distance) { return false; }
        return true;
    }
















//////////////////////////////////////////////////////////////////////////////////////////////////
//  O  F  F  E  N  S  E
//////////////////////////////////////////////////////////////////////////////////////////////////
    private void BehaveOffensive()
    {
        ObjectivesCurrentZoneLevel = PlayersCurrentZoneLevel;
        // Needs to calculate path to player.
        if (RouteRecalculationNeeded) 
        {
            HasSpottedObjective = false;
            RecalculateRoute(); 
        }
        // If progressing to target returns a false, run behavior specific "in LOS of objective" logic.
        else if (!ProgressingToTarget())
        {
            if (!HasSpottedObjective)
            {
                HasSpottedObjective = true;
                CompletedAnAction();
            }
            AttackLogic();
        }

    }



    private void AttackLogic()
    {
        if (NextObjectiveMove == -1) { ChooseNextAttack(); }

        if (!InRangeToDoObjective(ThePlayer.transform.position)) { WalkTowardsObjective(ThePlayer.transform.position); }
        else
        {
            movementLogic.flipSprite(ThePlayer.transform.position.x - transform.position.x);
            SetCombo();
        }
    }


    private void ChooseNextAttack()
    {
        int fail_counter = 0;
        int rangeMin;
        int rangeMax;

        if (Behavior_Mobility > UnityEngine.Random.Range(0,1f))
        {
            rangeMin = 0;
            rangeMax = 2;
        }
        else
        {
            rangeMin = 2;
            rangeMax = 8; 
        }
        while (fail_counter < 20)
        {
            NextObjectiveMove = UnityEngine.Random.Range(rangeMin,rangeMax);
            if (MoveIsValid(NextObjectiveMove)) { break; }
            NextObjectiveMove = 2;
            fail_counter++;
        }
        NextObjectiveMoveType = GetMoveType();

        Debug.Log("Chose Attack "+NextObjectiveMove+" of type "+NextObjectiveMoveType);

    }






//////////////////////////////////////////////////////////////////////////////////////////////////
//  D  E  F  E  N  S  E
//////////////////////////////////////////////////////////////////////////////////////////////////

    private void BehaveDefensive()
    {

        if (RouteRecalculationNeeded)
        {
            HasSpottedObjective = false;
            RecalculateRoute();
        }
        // If progressing to target returns a false, run behavior specific "in LOS of objective" logic.
        else if (!ProgressingToTarget())
        {
            if (!HasSpottedObjective)
            {
                HasSpottedObjective = true;
                CompletedAnAction();
            }
            DefendObjective();
        }

    }

    private void DefendObjective()
    {
        if (transform.position.x < DesiredPosition.x) { Look.x = 1; }
        else { Look.x = -1; }
    }





//////////////////////////////////////////////////////////////////////////////////////////////////
//  R  E  T  R  E  A  T
//////////////////////////////////////////////////////////////////////////////////////////////////

    private void BehaveRetreat()
    {
        if (RouteRecalculationNeeded)
        {
            HasSpottedObjective = false;
            RecalculateRoute();
        }
        // If progressing to target returns a false, run behavior specific "in LOS of objective" logic.
        else if (!ProgressingToTarget())
        {
            if (!HasSpottedObjective)
            {
                HasSpottedObjective = true;
                CompletedAnAction();
            }
            RetreatToPosition();
        }
    }

    private void RetreatToPosition()
    {
        
    }


//////////////////////////////////////////////////////////////////////////////////////////////////
//  H  I  B  E  R  N  A  T  I  O  N
//////////////////////////////////////////////////////////////////////////////////////////////////
    private void BehaveHibernation()
    {
        //Debug.Log("Bot is Hibernating!!!");
    }




//////////////////////////////////////////////////////////////////////////////////////////////////
//  I  D  L  E
//////////////////////////////////////////////////////////////////////////////////////////////////
    private void BehaveIdle() {

        
    }


//////////////////////////////////////////////////////////////////////////////////////////////////
//  S W I M   T O   N E A R E S T   L A D D E R
//////////////////////////////////////////////////////////////////////////////////////////////////
    private void BehaveSwim()
    {
        // Needs to get closest available ladder.
        if (RouteRecalculationNeeded)
        {
            RouteRecalculationNeeded = false;
            DesiredPosition = TheBotsManager.GetNeededOceanLadder(transform.position.x);
        }
        else
        {
            if (DesiredPosition.x > transform.position.x) { Look.x = 1; }
            else { Look.x = -1; }
        }
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
        IsUsingMovmentCombo = false;
        IsUsingCutlass = false;
        IsUsingPistol = false;
        IsUsingGrenade = false;
        IsUsingGrapple = false;
        IsDucking = false;
        IsWalking = false;
        IsTryingToJump = false;
        Look = new Vector2(0,0);
    }


////////////////////////////////////////////////
// M O V I N G   A L O N G   P A T H
////////////////////////////////////////////////

    private bool ProgressingToTarget()
    {
        // Needs to move to objective.
        if (DesiredPath.Count > 0 && (CurrentZoneLevel != ObjectivesCurrentZoneLevel || !CanReachPath(DesiredPosition, transform.position)))
        {
            ProgressAlongPath();
        }
        // Finish climbing ladder.
        else if (CurrentLadderDirection != -1 && DesiredPath.Count > 0)
        {
            ProgressLadder(DesiredPath[DesiredPath.Count-1]);
        }
        // Is within line of sight of the objective, sends a false to hand logic over to the individual behaviors.
        else
        {
            return false;
        }

        return true;
    }



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

        GameObject CurLadder = TheBotsManager.GetLadderFromPos(checkpoint);

        // Sets initial ladder direction so code knows what direction the bot is going.
        if (CurrentLadderDirection == -1) {
            if (transform.position.y - hitbox.transform.localScale.y > CurLadder.transform.position.y) { CurrentLadderDirection = 0; }
            else { CurrentLadderDirection = 1; }
        }

        // Checks to see if bot is finished with the ladder.
        else if (LadderFinishedValidator( CurLadder))
        {
            LadderFinished();
        }

        // Ladder Movement Logic: Down, Up
        if (CurrentLadderDirection == 0) { IsDucking = true; }
        else if (CurrentLadderDirection == 1)
        { 
            Look.y = 1;
            IsTryingToJump = true;
        }
    }



    private bool LadderFinishedValidator(GameObject CurLadder)
    {
        // Going Down
        if (CurrentLadderDirection == 0 && (transform.position.y < CurLadder.transform.position.y)) { return true; }
        // Going Up
        if (!rb.IsTouching(CurLadder.GetComponent<BoxCollider2D>())) { return true; }

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

        CompletedAnAction();
    }




    private void WalkTowardsObjective(Vector3 objective_pos)
    {
        Debug.Log("ObjectivesCurrentZoneLevel = "+ObjectivesCurrentZoneLevel);
        if (ObjectivesCurrentZoneLevel != -1) {
            if (transform.position.x < objective_pos.x) { Look.x = 1; }
            else { Look.x = -1; }
        }
    }




////////////////////////////////////////////////////////////////////////////////////////
// E X T R A   M O V E M E N T   F U N C T I O N S
////////////////////////////////////////////////////////////////////////////////////////


    private bool MoveIsValid(int move)
    {
        // Movement
        if (move == 0 && Combo_AllowSomerSault) { return true; }
        if (move == 1 && Combo_AllowJumpFlip)   { return true; }

        // Cutlass
        if (move == 2 && Combo_AllowBigSlash) { return true; }
        if (move == 3 && Combo_AllowUppercut) { return true; }
        if (move == 4 && Combo_AllowBlock)    { return true; }
        if (move == 5 && Combo_AllowThrow)    { return true; }

        // Others
        if (move == 6 && Combo_AllowPistol)  { return true; }
        if (move == 7 && Combo_AllowGrenade) { return true; }
        if (move == 8 && Combo_AllowGrapple) { return true; }


        return false;
    }

    private int GetMoveType()
    {
        if (NextObjectiveMove < 2) { return 0; } // Movement
        else if (NextObjectiveMove > 1 && NextObjectiveMove < 6) { return 1; } // Melee
        else if (NextObjectiveMove == 6) { return 2; } // Ranged
        else if (NextObjectiveMove == 7) { return 3; } // Grenade
        else if (NextObjectiveMove == 8) { return 4; } // Grapple

        return -1;
    }



    private void SetCombo()
    {

        if (NextObjectiveMove == 0) // Somersault
        {
            SetComboMovement(true);
            IsTryingToJump = false;
            IsDucking = true;
        }
        else if (NextObjectiveMove == 1) // Jump Flip
        {
            SetComboMovement(true);
            IsTryingToJump = true;
        }
        else if (NextObjectiveMove == 2) // Big Slash
        {
            SetComboMovement(false);
            IsTryingToJump = false;
            IsUsingCutlass = true;
            IsDucking = false;
        }
        else if (NextObjectiveMove == 3) // Uppercut
        {
            SetComboMovement(false);
            IsTryingToJump = true;
            IsUsingCutlass = true;
            IsDucking = false;
        }
        else if (NextObjectiveMove == 4) // Block
        {
            SetComboMovement(false);
            IsTryingToJump = false;
            IsUsingCutlass = true;
            IsDucking = true;
        }
        else if (NextObjectiveMove == 5) // Throw
        {
            SetComboMovement(true);
            IsTryingToJump = true;
            IsUsingCutlass = true;
        }
        else if (NextObjectiveMove == 6) // Pistol
        {
            IsUsingPistol = true;
        }
        else if (NextObjectiveMove == 7) // Grenade
        {
            IsUsingGrenade = true;
        }
        else if (NextObjectiveMove == 8) // Grapple
        {
            IsUsingGrapple = true;
        }

        NextObjectiveMove = -1;
        NextObjectiveMoveType = -1;

    }


    private void SetComboMovement(bool makeWalk)
    {
        if (makeWalk)
        {
            if (transform.eulerAngles.y == 0) { Look.x = -1; }
            else { Look.x = 1; }
        }
        else { Look.x = 0; }

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
