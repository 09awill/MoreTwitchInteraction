using HarmonyLib;
using Kitchen;
using Kitchen.ChefConnector;
using Kitchen.ChefConnector.Commands;
using KitchenMyMod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace KitchenModName
{
    [HarmonyPatch]
    public class Visit_Patch
    {
        [HarmonyPatch(typeof(Visit), "Handle")]
        [HarmonyPrefix]
        public static bool Handle(ChefCommandUpdate update)
        {
            //ChefVisitUpdate chefVisitUpdate = JsonUtility.FromJson<ChefVisitUpdate>(update.Data);
            if (update.Type == "VISIT_DETAILS")
            {
                ChefVisitDetails chefVisitDetails = JsonUtility.FromJson<ChefVisitDetails>(update.Data);
                Mod.instance.EntityManager.World.GetExistingSystem<CustomTwitchSystem>().NewOrder("NEW ORDER IS " + chefVisitDetails.Order.ToString());
            }
            Mod.LogWarning(update.Type);
            return true;
        }
    }
}
