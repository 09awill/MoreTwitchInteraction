using HarmonyLib;
using Kitchen.ChefConnector;
using Kitchen.ChefConnector.Commands;
using KitchenMoreTwitchInteraction;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenModName
{
    [HarmonyPatch]
    public class Visit_Patch
    {
        private static List<string> m_Names = new List<string>();
        [HarmonyPatch(typeof(Visit), "Handle")]
        [HarmonyPrefix]
        public static bool Handle(ChefCommandUpdate update)
        {
            try
            {
                Mod.LogWarning($"Patched Handle Update : {update.Data}");
                if (update.Type == "VISIT_ADD_NAME")
                {
                    ChefVisitUpdate chefVisitUpdate = JsonUtility.FromJson<ChefVisitUpdate>(update.Data);
                    if(!m_Names.Contains(chefVisitUpdate.Name))
                    {
                        m_Names.Add(chefVisitUpdate.Name);
                    }
                }
                if (update.Type == "VISIT_DETAILS")
                {
                    ChefVisitDetails chefVisitDetails = JsonUtility.FromJson<ChefVisitDetails>(update.Data);
                    Mod.Instance.EntityManager.World.GetExistingSystem<CustomTwitchSystem>().NewOrder(chefVisitDetails);
                }
            }
            catch (Exception message)
            {
                Mod.LogWarning("[Visit Patch Chef Connector] Malformed data");
                Mod.LogWarning(message);
                return true;
            }
            return true;
        }

        [HarmonyPatch(typeof(Visit), "SendMessages")]
        [HarmonyPrefix]
        public static bool SendMessages(Action<string> send)
        {
            foreach(var name in m_Names)
            {
                ChefNamedRequest chefNamedRequest = new ChefNamedRequest
                {
                    Type = "VISIT",
                    Instruction = "details",
                    Name = name
                };
                send(JsonUtility.ToJson(chefNamedRequest));
            }

            return true;
        }
    }
}
