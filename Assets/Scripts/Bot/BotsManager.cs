using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Animations;
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
//      Retreat     - Find and go to a safe place from the current threat.
//
//
//
//  Behavior Causes:
//      Offense - PRIORITIZE CLOSER BOTS: If the amount of bots currently in "Offense" is too low.
//      Defense - PRIORITIZE FURTHER BOTS: Enough bots in "Offense" mode.
//      Retreat - Depends of how "Brave" they are: Low on health, near grenade.
//
//
//  Behavior Variables:
//      Bravery   - Low: Retreats last longer. Retreats at higher heath and further away from grenades.
//      Intellect - Low: Retreats to less optimal positions, gets closer to player, idles longer.
//      Agility   - High: Uses mobility moves more frequently.
//      Mobility  - Flags: Crouch, Jump, Somersault, Jump Flip, Grapple.
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
//      ----Aggression Range        - Range for aggression among the bots.
//      Damage Mult. Min & Max  - Min/Max for damage multiplier among the bots.
//      Speed Mult. Min & Max   - Min/Max for speed multiplier among the bots.
//      Wpn Spd Mult. Min & Max - Min/Max for weapon speed multiplier among the bots.
//      Combative               - Percentage of bots of offense.
//      Max Active              - How many bots can be not in hibernation.
//      Stay Offensive          - Chance a bot will stay on offense if there is to many.
//      Review Defensive Delay  - How often to run a defense minimum check.
//      Wake Up Time Min & Max  - Min/Max for how quickly bots leave hibernation when "summoned".
//      Min Count to Rush       - Min amount of bots before all bots are set to offense.
//      Allowed Mobility        - Flag of allowed mobility options.
//      Mobility Bias           - Chance a bot will have any given allowed mobility flag.
//
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
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////

    private int BotFlag_Hibernation = 0;
    private int BotFlag_Offense = 1;
    private int BotFlag_Defense = 2;


    [Header("Bot Starting Stats")]
    public float BotGlobal_BehaviorBraveryPercent;
    public float BotGlobal_BehaviorIntellectPercent;
    public int BotGlobal_MobilityPercent;
    public float BotGlobal_MobilityBiasPercent;
    public float BotGlobal_OffensePercent;
    public int BotGlobal_MinCountToRush;
    public int BotGlobal_MaxActive;
    public float BotGlobal_ReviewDefenseDelay;
    
    private int BotGlobal_CurrentActive;
    private int BotGlobal_CurrentOffense;
    private int BotGlobal_CurrentDefense;
    private float BotGlobal_MaxBots;


    [Header("Bot Stat Ranges")]
    public float BotBehaviorRange_IntellectPercent;
    public float BotBehaviorRange_AggressionPercent;
    public float BotBehaviorMin_WakeUpTime;
    public float BotBehaviorMax_WakeUpTime;

    [Header("Multipliers")]

    public float BotMultiplier_MultiplierDamage;
    public float BotMultiplier_MultiplierSpeed;
    public float BotMultiplier_MultiplierWeaponSpeed;
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
        }
    }






    void Update()
    {
        GetPlayersCurrentZone();
        GetAllBotStats();
        UpdateAllBotBehavior();
    }






////////////////////////////////////////////////////////////////////////
// B O T   L O G I C   G E T T E R   F U N C T I O N S
////////////////////////////////////////////////////////////////////////

    public int GetZoneCount() { return Zones.Count; }
    public BoxCollider2D GetZoneCollider(int zone_id) { return Zones[zone_id].GetComponent<BoxCollider2D>(); }








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









    public int GetLadderLevel(Transform ladder)
    {
        for (int y = 0; y < Zones.Count;y++)
        {
            for (int x = 0; x < Zones[y].transform.childCount;x++)
            {
                if (Zones[y].transform.GetChild(x).tag == "ShipTrigger_Ladder" && Zones[y].transform.GetChild(x).transform.position == ladder.position)
                {
                    return y;
                }
            }
        }
        return -1;
    }

    public float GetLadderHeight(Transform ladder)
    {
        for (int y = 0; y < Zones.Count;y++)
        {
            for (int x = 0; x < Zones[y].transform.childCount;x++)
            {
                if (Zones[y].transform.GetChild(x).tag == "ShipTrigger_Ladder" && Zones[y].transform.GetChild(x).transform.position == ladder.position)
                {
                    return Zones[y].transform.GetChild(x).transform.localScale.y*2;
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







////////////////////////////////////////////////////////////////////////
// M A N A G E R   U P D A T E   F U N C T I O N S
////////////////////////////////////////////////////////////////////////

    private void GetAllBotStats()
    {
        BotGlobal_CurrentActive  = 0;
        BotGlobal_CurrentOffense = 0;
        BotGlobal_CurrentDefense = 0;

        foreach (Transform bot in Crew.transform)
        {
            if (bot.gameObject.activeSelf) { 

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
            if (bot.gameObject.activeSelf) { awake_bots.Add(bot); }
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
            offense_ratio = BotGlobal_CurrentOffense / BotGlobal_CurrentActive;
            //Debug.Log("offense_ratio = "+offense_ratio + " = "+BotGlobal_CurrentOffense+" / "+BotGlobal_CurrentActive);

            if (offense_ratio < BotGlobal_OffensePercent && awake_bots[i].gameObject.GetComponent<BotLogic>().GetBehavior() != 1)
            {
                awake_bots[i].gameObject.GetComponent<BotLogic>().UpdateBehavior(1);
                BotGlobal_CurrentOffense++;
            }
        }
    }
    private void UpdateBotsDefense(List<Transform> awake_bots) {}
    private void UpdateBotsRetreat(List<Transform> awake_bots) {}
    private void UpdateBotsWakeUp(List<Transform> awake_bots) {}




    ////////////////////////////////////////////////////////////////////////
    // P L A Y E R   U P D A T E   F U N C T I O N S
    ////////////////////////////////////////////////////////////////////////


    public void GetPlayersCurrentZone()
    {

        
        int zone_count = GetZoneCount();
        int players_old_zone = PlayersCurrentZoneLevel;
        BoxCollider2D cur_zone;

        for (int i = 0; i < zone_count;i++)
        {
            cur_zone = GetZoneCollider(i);
            if (ThePlayer.GetComponent<Rigidbody2D>().IsTouching(cur_zone))
            {
                PlayersCurrentZoneLevel = i;
            }
        }

        if (players_old_zone != PlayersCurrentZoneLevel) 
        { 
            foreach (Transform bot in Crew.transform)
            {
                bot.gameObject.GetComponent<BotLogic>().UpdatePlayerLevel(PlayersCurrentZoneLevel);
            }
        }
    }


}
