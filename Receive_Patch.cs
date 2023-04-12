using HarmonyLib;
using Kitchen.ChefConnector.Commands;
using Kitchen.ChefConnector;
using KitchenModName;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.InputSystem.InputRemoting;

namespace KitchenMoreTwitchInteraction
{
    /*
    [HarmonyPatch]
    public class Receive_Patch
    {
        [HarmonyPatch(typeof(ChefClient), "Receive")]
        [HarmonyPrefix]
        public static bool Handle(string message)
        {
            Mod.LogWarning(message);
            return true;
        }

    }
    */
}
