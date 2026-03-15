using UnityEngine;

public class BotsManager : MonoBehaviour
{

/////////////////////////////////////////////////////////////////////////////////////////////////////////
//  B  O  T      L  O  G  I  C   //
///////////////////////////////////
//
//  Behavior States:
//      Offense     - Find and go to a good spot to attack the player from.
//      Defense     - Find and go to the nearest available cannon and fire it. 
//      Retreat     - Find and go to a safe place from the current threat.
//      ---------------------------------------------------------------------------
//      Hibernation - Is inactive inside a crate, barrel, or "Door"(Bot Spawner). 
//      Disabled    - Standing still if player is not onboard that ship.
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
//      Mobility  - High: Uses mobility moves like somersault and jump flip.
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
//      Wake Up Time Min & Max  - Min/Max for how quickly bots leave hibernation when "summoned".
//      Min Count to Rush       - Min amount of bots before all bots are set to offense.
//
//
//
//
//
//
//
//
//  General Notes:
//      Behavior variables control 1 bot.
//      Manager variables control all bots.
//      Manager has it's own set of behavior variables and Min/Max ranges for each variable.
//      Manager variables with identical names to behavior variables...
//          set the "general value" for the bots. EX: Manager Intellect = 0.5, Manager Intellect Range = 0.1,
//          bots' "Intellect" can range from 0.4 to 0.6.
//      ---------------------------------------------------------------------------
//      Bots cannot go into hibernation, they can only start in it.
//      Bots can go fron offense to defense if enough 
//
//
//
//
//
//  Update Cycle:
//      1) Check if the bots should be disabled.
//      2) Get a count of all active bots and their roles.
//      2) Check if any bots should retreat.
//      3) Check if bots should go all offense.
//      4) Check if any bots should switch to offense.
//      5) Check if any bots should switch to defense.
//      6) Check if any bots should come out hibernation.
//
//
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////



    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
