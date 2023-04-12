using HarmonyLib;
using Kitchen;
using Kitchen.ChefConnector;
using Kitchen.ChefConnector.Commands;
using KitchenMoreTwitchInteraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

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
                    Mod.instance.EntityManager.World.GetExistingSystem<CustomTwitchSystem>().NewOrder(chefVisitDetails);
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
