using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotAbilities", menuName = "Scriptable Objects/BotAbilities")]
public class BotAbilities : ScriptableObject
{
    private int[] Combos = {
        1001, // Somersault
        1002, // Jump flip

        2001, // Big Slash
        2003, // Uppercut
        2004, // Block
        2005, // Throw

        3001, // Pistol

        4001, // Grenade

        5001, // Grapple
    };

    private int[] ComboTypes = {
        0, // Movement
        0, // Movement

        1, // Melee
        1, // Melee
        1, // Melee
        1, // Melee

        2, // Ranged

        3, // Grenade

        4, // Grapple

    };


    public List<int> GetCombosByType(bool GrabMovement, bool GrabMelee, bool GrabRanged, bool GrabGrenade, bool GrabGrapple)
    {
        List<int> combos = new List<int>();
        for (int i = 0; i < Combos.Length; i++)
        {
            if (ComboTypes[i] == 0 && GrabMovement) { combos.Add(Combos[i]); }
            else if (ComboTypes[i] == 1 && GrabMelee) { combos.Add(Combos[i]); }
            else if (ComboTypes[i] == 2 && GrabRanged) { combos.Add(Combos[i]); }
            else if (ComboTypes[i] == 3 && GrabGrenade) { combos.Add(Combos[i]); }
            else if (ComboTypes[i] == 4 && GrabGrapple) { combos.Add(Combos[i]); }
        }
        return combos;
    }


}



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
