//using System;
using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
using Unity.VisualScripting;
//using UnityEditor.Animations;
using UnityEngine;

public class BotsManager : MonoBehaviour
{

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  O  V  E  R  V  I  E  W
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// This script is to be applied to each ship.
// This script carries out the decision making of the team of bots on said ship.
// This script DOES NOT carry out behavior bot. IE make retreating bots run away or offense bots shoot.
// This script manages the decision making of bots, the ratio of defending / offending bots, and which bots
//      if any should be hibernating or retreating.
// This script manages the spawning and parenting bots to the right GameObject.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////
//  S  H  I  P      A  R  C  H  I  T  E  C  T  U  R  E
/////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Zones:
//      Top Deck   - Top most deck.
//      Below Deck - All Decks that are not the
//      Masts      - Up in the masts. Bots need to jump platform to platform
//      Ocean      - Outside the ship, bots and player forced into swim mode if below water line..
//
//  POIs:
//      Top Deck:
//          Ladder off sides of the ship.
//      Below Deck:
//          Staircases.
//      Masts:
//          Platforms. Grapple points.
//      Ocean:
//          Nothing.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////
//  B  O  T      L  O  G  I  C   //
///////////////////////////////////
//
//  Behavior States:
//      Offense     - Find and go to a good spot to attack the player from.
//      Defense     - Find and go to the nearest available cannon and fire it. 
//      Hibernation - Is inactive inside a crate, barrel, or "Door"(Bot Spawner). 
//
//  Behavior Overrides:
//      Retreat     - Find and go to a safe place from the current threat     ****SCRAPPED****--> or just run to a random spot so 20 bots aren't all making the players experience a living hell.
//      Idle        - Just stands still for a bit so 20 bots aren't all making the players experience a living hell.
//
//
//  Behavior Causes:
//      Offense - PRIORITIZE CLOSER BOTS: If the amount of bots currently in "Offense" is too low.
//      Defense - PRIORITIZE FURTHER BOTS: Enough bots in "Offense" mode.
//      Retreat - Depends of how "Brave" they are: Low on health, near grenade.
//
//
//  Behavior Variables:
//      Aggression     - Low: After completing an action, has a low chance of idling for a bit.
//      Bravery        - Low: Retreats last longer. Retreats at higher heath and further away from grenades.
//      Intellect      - Low: Retreats to less optimal positions, gets closer to player, idles longer.
//      Agility        - High: Uses mobility moves more frequently.
//      Allowed Combos - Flags of allowed combo / mobility options.
//      ----------------------------------------------------
//      Damage Multiplier       - Increase/Decrease damage.
//      Speed Multiplier        - Increase/Decrease speed.
//      Weapon Speed Multiplier - Increase/Decrease weapon speed.
//
//
//
//
//
//  Calculating if a bot should go offense:
//      If "Total Offense Bots" out of "Current Active" is less than "Combative".
//      EX: ( TOB(10) / CA(20) is 0.5 ) < C(0.6).
//      ----------------------------------------------------------
//      NOTE: If Current Active is less than Max Active, IE the player is killing the
//          final few bots, the manager will use a modified equation above using max active
//          instead of current active.
//
//
//
//
//
//
//
//  Manager Variables:
//      Intellect Range         - Range for intellect among the bots.
//      Aggression Range        - Range for aggression among the bots.
//      Damage Mult. Min & Max  - Min/Max for damage multiplier among the bots.
//      Speed Mult. Min & Max   - Min/Max for speed multiplier among the bots.
//      Wpn Spd Mult. Min & Max - Min/Max for weapon speed multiplier among the bots.
//      Combative               - Percentage of bots of offense.
//      Max Active              - How many bots can be not in hibernation.
//      Stay Offensive          - Chance a bot will stay on offense if there is to many.
//      Review Defensive Delay  - How often to run a defense minimum check.
//      Wake Up Time Min & Max  - Min/Max for how quickly bots leave hibernation when "summoned".
//      Min Count to Rush       - Min amount of bots before all bots are set to offense.
//
//
//
//
//
//
//  General Notes:
//      Behavior variables control 1 bot.
//      1 Bot Manager per ship.
//      Manager variables control all bots.
//      Manager has it's own set of behavior variables and Min/Max ranges for each variable.
//      Manager variables with identical names to behavior variables...
//          set the "general value" for the bots. EX: Manager Intellect = 0.5, Manager Intellect Range = 0.1,
//          bots' "Intellect" can range from 0.4 to 0.6.
//      ---------------------------------------------------------------------------
//      Bots cannot go into hibernation, they can only start in it.
//      Bots can go fron offense to defense if enough.
//
//
//
//
//
//  Update Cycle:
//      1) Get a count of all active bots and their roles.
//      2) Check if any bots should retreat.
//      3) Check if bots should go all offense.
//      4) Check if any bots should switch to offense.
//      5) Check if any bots should switch to defense.
//      6) Check if any bots should come out hibernation.
//      7) Every "Review Defensive Delay" seconds run a "Stay Offense" update.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////
//  B  O  T      C  O  M  B  O  S   //
//////////////////////////////////////
//
//      1:   Somersault (1001)
//      2:   Jump flip  (1002)
//      4:   Big Slash  (2001)
//      8:   Uppercut   (2003)
//      16:  Block      (2004)
//      32:  Throw      (2005)
//      64:  Pistol     (3001)
//      128: Grenade    (4001)
//      256: Grapple    (5001-500X)
//
//
//
//
//
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////

    private int BotFlag_Hibernation = 0;
    private int BotFlag_Offense = 1;
    private int BotFlag_Defense = 2;


    [Header("Bot Starting Stats")]
    public float BotGlobal_BehaviorAggressionPercent;
    public float BotGlobal_BehaviorBraveryPercent;
    public float BotGlobal_BehaviorIntellectPercent;
    public float BotGlobal_BehaviorMobilityPercent;
    public float BotGlobal_OffensePercent;
    public int BotGlobal_MinCountToRush;
    public int BotGlobal_MaxActive;
    public float BotGlobal_ReviewDefenseDelay;

    private int BotGlobal_CurrentActive;
    private int BotGlobal_CurrentOffense;
    private int BotGlobal_CurrentDefense;
    private float BotGlobal_MaxBots;


    [Header("Bot Stat Ranges")]
    public float BotBehaviorRange_AggressionPercent;
    public float BotBehaviorRange_BraveryPercent;
    public float BotBehaviorRange_IntellectPercent;
    public float BotBehaviorRange_MobilityPercent;
    public float BotBehaviorRange_IdleTimerMax;
    public float BotBehaviorRange_IdleTimerMin;
    public float BotBehaviorMin_WakeUpTime;
    public float BotBehaviorMax_WakeUpTime;

    [Header("Multipliers")]
    public float BotMultiplierMin_Damage;
    public float BotMultiplierMax_Damage;
    public float BotMultiplierMin_Speed;
    public float BotMultiplierMax_Speed;
    public float BotMultiplierMin_WeaponSpeed;
    public float BotMultiplierMax_WeaponSpeed;



////////////////////////////////////////////////////////////////////////
//  P  L  A  Y  E  R      I  N  F  O
////////////////////////////////////////////////////////////////////////
    private GameObject ThePlayer;
    private int PlayersCurrentZoneLevel;

////////////////////////////////////////////////////////////////////////

    private GameObject Navigation;
    private List<GameObject> Zones = new List<GameObject>();
    private List<Transform> GrapplePoints = new List<Transform>();
    private GameObject Crew;


    void Start()
    {
        Navigation = transform.Find("Navigation").gameObject;
        Crew = transform.Find("Crew").gameObject;
        ThePlayer = GameObject.FindWithTag("Player");

        for (int i = 0; i < Navigation.transform.childCount;i++) { Zones.Add(Navigation.transform.GetChild(i).gameObject); }

        for (int i = 0; i < Crew.transform.childCount;i++) 
        { 
            Crew.transform.GetChild(i).gameObject.GetComponent<BotLogic>().SetManager(this); 
            Crew.transform.GetChild(i).gameObject.GetComponent<BotLogic>().SetPlayer(ThePlayer);
            GeneratePersonality(Crew.transform.GetChild(i).gameObject.GetComponent<BotLogic>());
        }


        // Behold the nest.
        if (Navigation.transform.childCount > 0)
        {
            Transform cur_mast;
            for (int i = 0; i < Navigation.transform.GetChild(0).childCount; i++)
            {
                cur_mast = Navigation.transform.GetChild(0).GetChild(i);
                if (cur_mast.tag == "ShipTrigger_Mast") {
                    for (int q = 0; q < cur_mast.childCount; q++)
                    {
                        for (int z = 0; z < cur_mast.GetChild(q).childCount; z++)
                        {
                            GrapplePoints.Add(cur_mast.GetChild(q).GetChild(z));
                        }
                    }
                }
            }
            {
                
            }
        }




    }






    void Update()
    {
        UpdatePlayersCurrentZone();
        GetAllBotStats();
        UpdateAllBotBehavior();
    }


////////////////////////////////////////////////////////////////////////
// C R E W   I N I T I A L I Z E R   F U N C T I O N S
////////////////////////////////////////////////////////////////////////

    private void GeneratePersonality(BotLogic botLogic)
    {
        float aggro = BotGlobal_BehaviorAggressionPercent + UnityEngine.Random.Range(-BotBehaviorRange_AggressionPercent,BotBehaviorRange_AggressionPercent);
        float bravery = BotGlobal_BehaviorBraveryPercent + UnityEngine.Random.Range(-BotBehaviorRange_BraveryPercent,BotBehaviorRange_BraveryPercent);
        float smarts = BotGlobal_BehaviorIntellectPercent + UnityEngine.Random.Range(-BotBehaviorRange_IntellectPercent,BotBehaviorRange_IntellectPercent);
        float jumpy = BotGlobal_BehaviorMobilityPercent + UnityEngine.Random.Range(-BotBehaviorRange_MobilityPercent,BotBehaviorRange_MobilityPercent);
        float damage_mult = UnityEngine.Random.Range(BotMultiplierMin_Damage,BotMultiplierMax_Damage);
        float move_speed_mult = UnityEngine.Random.Range(BotMultiplierMin_Speed,BotMultiplierMax_Speed);
        float wepn_speed_mult = UnityEngine.Random.Range(BotMultiplierMin_WeaponSpeed,BotMultiplierMax_WeaponSpeed);
        botLogic.SetPersonality(aggro, bravery, smarts, jumpy, damage_mult, move_speed_mult, wepn_speed_mult);

    }

////////////////////////////////////////////////////////////////////////
// G A M E   M A N A G E R   G E T T E R   F U N C T I O N S
////////////////////////////////////////////////////////////////////////
    public int GetPlayersCurrentZone() { return PlayersCurrentZoneLevel; }

    public List<GameObject> GetCrew()
    {
        List<GameObject> the_crew = new List<GameObject>();

        for (int i = 0; i < Crew.transform.childCount; i++)
        {
            the_crew.Add(Crew.transform.GetChild(i).gameObject);
        }

        return the_crew;
    }


////////////////////////////////////////////////////////////////////////
// B O T   L O G I C   G E T T E R   F U N C T I O N S
////////////////////////////////////////////////////////////////////////

    public int GetZoneCount() { return Zones.Count; }

    public List<Transform> GetGrapplePoints() { return GrapplePoints; }

    public int GetCrewCount() { 
        List<GameObject> ocean_ladders = GetOceanLadders();
        Transform cur_bot;
        int count = 0;
        for (int i = 0; i < ocean_ladders.Count; i++)
        {
            for (int q = 0; q < ocean_ladders[i].transform.childCount; q++)
            {
                cur_bot = ocean_ladders[i].transform.GetChild(q);
                if (cur_bot.tag == "Bot" && !cur_bot.GetComponent<HealthManager>().IsPlayerDead()) { count++; }
            }
        }

        for (int i = 0; i < Crew.transform.childCount; i++)
        {
            cur_bot = Crew.transform.GetChild(i);
            if (cur_bot.tag == "Bot" && !cur_bot.GetComponent<HealthManager>().IsPlayerDead()) { count++; }
        }


        return count;
    }


    public BoxCollider2D GetZoneCollider(int zone_id) { return Zones[zone_id].GetComponent<BoxCollider2D>(); }

    public List<GameObject> GetOceanLadders()
    {
        List<GameObject> ladders = new List<GameObject>();

        for (int i = 0; i < Zones[0].transform.childCount; i++)
        {
            if (Zones[0].transform.GetChild(i).tag == "ShipTrigger_LadderOcean")
            {
                ladders.Add(Zones[0].transform.GetChild(i).gameObject);
            }
        }
        return ladders;
    }

    public Vector2 GetNeededOceanLadder(float posX)
    {
        List<GameObject> ladders = new List<GameObject>();
        int closest_ladder_id = 0;
        float closest_distance = 999999999;
        float distance;
        for (int i = 0; i < Zones[0].transform.childCount; i++)
        {
            if (Zones[0].transform.GetChild(i).tag == "ShipTrigger_LadderOcean")
            {
                ladders.Add(Zones[0].transform.GetChild(i).gameObject);
                distance = Math.Abs(ladders[ladders.Count-1].transform.position.y - posX);
                if (distance < closest_distance)
                {
                    closest_ladder_id = ladders.Count-1;
                    closest_distance = distance;
                }
            }
        }




        return ladders[closest_ladder_id].transform.position;
    }





    public List<Transform> GetZonesAvailableLadderPositions(int zone_id)
    {
        List<Transform> ladders = new List<Transform>();
        ladders = get_ladders(ladders, zone_id);
        if (zone_id + 1 < Zones.Count) { ladders = get_ladders(ladders, zone_id+1); }
        return ladders;
    }
    private List<Transform> get_ladders(List<Transform> ladders, int zone_id)
    {
        for (int i = 0; i < Zones[zone_id].transform.childCount;i++)
        {
            if (Zones[zone_id].transform.GetChild(i).tag == "ShipTrigger_Ladder")
            {
                ladders.Add(Zones[zone_id].transform.GetChild(i));
            }
        }
        return ladders;
    }




    public int GetObjectLevel(Transform ladder, string cur_tag)
    {
        for (int y = 0; y < Zones.Count;y++)
        {
            for (int x = 0; x < Zones[y].transform.childCount;x++)
            {
                if (Zones[y].transform.GetChild(x).tag == cur_tag && Zones[y].transform.GetChild(x).transform.position == ladder.position)
                {
                    return y;
                }
            }
        }
        return -1;
    }




    public float GetLadderHeight(Transform ladder)
    {
        float final_scale;
        for (int y = 0; y < Zones.Count;y++)
        {
            for (int x = 0; x < Zones[y].transform.childCount;x++)
            {
                if (Zones[y].transform.GetChild(x).tag == "ShipTrigger_Ladder" && Zones[y].transform.GetChild(x).transform.position == ladder.position)
                {
                    final_scale = Zones[y].transform.GetChild(x).transform.localScale.y;
                    return final_scale * Zones[y].transform.GetChild(x).GetComponent<BoxCollider2D>().size.y;
                }
            }
        }
        return -1;
    }

    public GameObject GetLadderFromPos(Vector3 pos)
    {

        for (int y = 0; y < Zones.Count;y++)
        {
            for (int x = 0; x < Zones[y].transform.childCount;x++)
            {
                if (Zones[y].transform.GetChild(x).tag == "ShipTrigger_Ladder" && Zones[y].transform.GetChild(x).transform.position == pos)
                {
                    return Zones[y].transform.GetChild(x).gameObject;
                }
            }
        }



        return null;
    }

    public int GetTopDeck()
    {
        for (int i = 0; i < Zones.Count;i++)
        {
            if (Zones[i].transform.tag == "ShipTrigger_UpperDeck") { return i; }
        }
        return -1;
    }


    public GameObject GetAnOpenDefensivePart()
    {
        List<GameObject> defensibleObjects = new List<GameObject>();

        for (int y = 0; y < Zones.Count;y++)
        {
            for (int x = 0; x < Zones[y].transform.childCount;x++)
            {
                if (IsTagDefensible(Zones[y].transform.GetChild(x).tag) && IsDefensiblePartFree(Zones[y].transform.GetChild(x)))
                {
                    defensibleObjects.Add(Zones[y].transform.GetChild(x).gameObject);
                }
            }
        }

        if (defensibleObjects.Count > 0) { return defensibleObjects[UnityEngine.Random.Range(0,defensibleObjects.Count)]; }
        return null;
    }



    private bool IsTagDefensible(string cur_tag)
    {
        if (cur_tag == "ShipTrigger_Cannon") { return true; }
        else if (cur_tag == "ShipTrigger_Chest") { return true; }

        return false;
    }

    private bool IsDefensiblePartFree(Transform cur_object)
    {
        DefensiveShipPartInfo info = cur_object.gameObject.GetComponent<DefensiveShipPartInfo>();
        if (!info.IsUnityNull())
        {
            return info.GetIsNotBeingUsed();
        }
        
        return false;
    }





////////////////////////////////////////////////////////////////////////
// M A N A G E R   U P D A T E   F U N C T I O N S
////////////////////////////////////////////////////////////////////////

    private void GetAllBotStats()
    {
        BotGlobal_CurrentActive = 0;
        BotGlobal_CurrentOffense = 0;
        BotGlobal_CurrentDefense = 0;

        foreach (Transform bot in Crew.transform)
        {
            if (bot.gameObject.activeSelf && !bot.gameObject.GetComponent<HealthManager>().IsPlayerDead()) { 
                BotGlobal_CurrentActive++; 
                if (bot.gameObject.GetComponent<BotLogic>().GetBehavior() == BotFlag_Offense)      { BotGlobal_CurrentOffense++; }
                else if (bot.gameObject.GetComponent<BotLogic>().GetBehavior() == BotFlag_Defense) { BotGlobal_CurrentDefense++; }
            }

        }
    }



    private void UpdateAllBotBehavior()
    {
        List<Transform> awake_bots = new List<Transform>();
        foreach (Transform bot in Crew.transform)
        {
            if (bot.gameObject.activeSelf) { 
                awake_bots.Add(bot);
            }
        }
        UpdateBotsOffense(awake_bots);
        UpdateBotsDefense(awake_bots);
        UpdateBotsRetreat(awake_bots);
        UpdateBotsWakeUp(awake_bots);
    }

    private void UpdateBotsOffense(List<Transform> awake_bots)
    {
        float offense_ratio;
        for (int i = 0; i < awake_bots.Count;i++)
        {
            offense_ratio = (float)BotGlobal_CurrentOffense / (float)BotGlobal_CurrentActive;
            Debug.Log("offense_ratio = "+offense_ratio + " = "+BotGlobal_CurrentOffense+" / "+BotGlobal_CurrentActive);

            if (offense_ratio < BotGlobal_OffensePercent && awake_bots[i].gameObject.GetComponent<BotLogic>().GetBehavior() != 1)
            {
                awake_bots[i].gameObject.GetComponent<BotLogic>().UpdateBehavior(1);
                BotGlobal_CurrentOffense++;
            }
        }
    }
    private void UpdateBotsDefense(List<Transform> awake_bots)
    {
        for (int i = 0; i < awake_bots.Count;i++)
        {
            if (awake_bots[i].gameObject.GetComponent<BotLogic>().GetBehavior() == 0)
            { 
                awake_bots[i].gameObject.GetComponent<BotLogic>().UpdateBehavior(2);
            }
        }
    }
    private void UpdateBotsRetreat(List<Transform> awake_bots) {}
    private void UpdateBotsWakeUp(List<Transform> awake_bots) {}




    ////////////////////////////////////////////////////////////////////////
    // P L A Y E R   U P D A T E   F U N C T I O N S
    ////////////////////////////////////////////////////////////////////////


    public void UpdatePlayersCurrentZone()
    {

        
        int zone_count = GetZoneCount();
        int players_old_zone = PlayersCurrentZoneLevel;
        BoxCollider2D cur_zone;

        PlayersCurrentZoneLevel = -1;

        for (int i = 0; i < zone_count;i++)
        {
            cur_zone = GetZoneCollider(i);
            if (ThePlayer.GetComponent<Rigidbody2D>().IsTouching(cur_zone))
            {
                PlayersCurrentZoneLevel = i;
                break;
            }
        }

        if (players_old_zone != PlayersCurrentZoneLevel) 
        {
            Debug.Log("Player is now on level "+PlayersCurrentZoneLevel+"!");
            foreach (Transform bot in Crew.transform)
            {
                Debug.Log("Updating for "+bot.gameObject.name);
                bot.gameObject.GetComponent<BotLogic>().UpdatePlayerLevel(PlayersCurrentZoneLevel);
            }
        }
    }


}
