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
using Kitchen;
using Unity.Entities;
using static Kitchen.TwitchNameList;

namespace KitchenMoreTwitchInteraction
{
    [HarmonyPatch]
    public class GetNewName_Patch
    {
        private static bool gettingName = false;
        [HarmonyPatch(typeof(TwitchNameList), "GetNewName")]
        [HarmonyPostfix]
        public static void GetNewName(Entity e)
        {
            TwitchNameList names = Mod.Instance.World.GetExistingSystem<TwitchNameList>();
            if (names.GetName(e) == "madvion")
            {
                CCustomer customer = new CCustomer()
                {
                    Scale = 1.2f,
                    Speed = 2f,
                };
                Mod.Instance.EntityManager.SetComponentData(e, customer);
            }
            else if (names.GetName(e) == "Elsee")
            {
                CCustomer customer = new CCustomer()
                {
                    Scale = 0.8f,
                    Speed = 0.5f,
                };
                Mod.Instance.EntityManager.SetComponentData(e, customer);
            }


        }
        
        //COULD MAYBE ALTER NAMES LIKE THIS
        [HarmonyPatch(typeof(TwitchNameList), "GetName")]
        [HarmonyPostfix]
        public static void GetName(ref string __result, Entity e)
        {
            if (gettingName) return;
            gettingName = true;
            CustomTwitchSystem customsystem = Mod.Instance.World.GetExistingSystem<CustomTwitchSystem>();
            TwitchNameList names = Mod.Instance.World.GetExistingSystem<TwitchNameList>();
            string icon = customsystem.GetOrderIcon(names.GetName(e));
            gettingName = false;
            __result += icon;
            return;
        }
        
    }
}
