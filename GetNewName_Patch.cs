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
        [HarmonyPatch(typeof(TwitchNameList), "GetNewName")]
        [HarmonyPostfix]
        public static void GetNewName(Entity e)
        {
            TwitchNameList names = Mod.Instance.World.GetExistingSystem<TwitchNameList>();
            if(names.GetName(e) == "madvion")
            {
                CCustomer customer = new CCustomer()
                {
                    Scale = 1.2f,
                    Speed = 2f,

                };
                Mod.Instance.EntityManager.SetComponentData(e, customer);

            }
        }
    }
}
