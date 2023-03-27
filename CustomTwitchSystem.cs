using DeconstructorMod.Components;
using Kitchen;
using Kitchen.Transports;
using KitchenLib.Utils;
using KitchenMods;
using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using KitchenMyMod;
using static Kitchen.TwitchNameList;

namespace KitchenModName
{
    internal class CustomTwitchSystem : RestaurantSystem, IModSystem
    {
        private TwitchNameList nameList;
        private EntityQuery m_PlayerQuery;
        private EntityQuery m_ApplianceQuery;
        private float m_CoolDown = 60f;
        private float m_SpeedBoostDuration = 5f;

        //private Dictionary<Entity, TwitchCustomerData> data;
        private Dictionary<Entity, CustomOrder> m_Orders;

        private Dictionary<Entity, CustomOrder> m_PrevOrder;
        protected override void Initialise()
        {
            nameList = base.World.GetExistingSystem<TwitchNameList>();
            System.Object ret = ReflectionUtils.GetField<TwitchNameList>("AssignedData").GetValue(nameList);
            data = (Dictionary<Entity, TwitchCustomerData>)ret;
            m_PlayerQuery = GetEntityQuery(new QueryHelper().All(typeof(CPlayer)));
            m_ApplianceQuery = GetEntityQuery(new QueryHelper().All(typeof(CAppliance)).None(typeof(CImmovable)));
            m_PrevOrder = new Dictionary<Entity, CustomOrder>();
        }
        public void NewOrder(string pOrder)
        {
            KitchenMyMod.Mod.LogWarning(pOrder);
        }
        protected override void OnUpdate()
        {
            using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
            foreach (var player in players)
            {
                if (Require(player, out CTakesDuration duration))
                {
                    //Debug.Log("Duration remaining : " + duration.Remaining);
                    if (duration.Remaining < 0.1f)
                    {
                        EntityManager.RemoveComponent<CTakesDuration>(player);
                        EntityManager.RemoveComponent<CSlowPlayer>(player);
                    }
                }
            }
            List<Entity> keys = new List<Entity>(data.Keys);
            foreach(Entity key in keys)
            {
                //Debug.Log($"Order index is : {data[key].OrderIndex}");
                if (data.Count != m_PrevOrder.Count)
                {
                    //Debug.Log("First Day Update");
                    List<Entity> PrevOrderkeys = new List<Entity>(m_PrevOrder.Keys);
                    foreach (var CustomOrderKeys in PrevOrderkeys)
                    {
                        if (m_PrevOrder[CustomOrderKeys].Name == data[key].Name)
                        {
                            //Debug.Log("Name was the same copying across");

                            CustomOrder order = new CustomOrder();
                            order.Name = data[key].Name;
                            order.RunThisTurn = false;
                            order.OrderIndex = m_PrevOrder[CustomOrderKeys].OrderIndex;
                            m_PrevOrder[key] = order;
                            m_PrevOrder.Remove(CustomOrderKeys);
                        }
                    }
                }
                if (m_PrevOrder.ContainsKey(key)){
                    if (m_PrevOrder[key].OrderIndex == data[key].OrderIndex || data[key].OrderIndex == 0)
                    {
                        //Debug.Log("Order was the same");

                        return;
                    }
                    if (m_PrevOrder[key].RunThisTurn)
                    {
                        //Debug.Log("Already Run");
                        return;
                    }
                }
                switch (data[key].OrderIndex)
                {
                    case 101:
                        if (Random.Range(0f, 1f) > 0.01)
                        {
                            Entity e = players[Random.Range(0, players.Length)];
                            EntityManager.AddComponent<CSlowPlayer>(e);
                            CTakesDuration cTakesDuration = new CTakesDuration()
                            {
                                Total = m_SpeedBoostDuration,
                                Active = true,
                                Remaining = m_SpeedBoostDuration,

                            };
                            EntityManager.AddComponent<CTakesDuration>(e);
                            EntityManager.SetComponentData(e, cTakesDuration);
                            EntityManager.SetComponentData(e, new CSlowPlayer() { Factor = 3, Radius = 1000f });
                            FinishCustomOrder(key);
                        }
                        break;
                    case 102:
                        if (Random.Range(0f, 1f) > 0.01)
                        {
                            using var apps = m_ApplianceQuery.ToEntityArray(Allocator.Temp);
                            Entity eA = apps[Random.Range(0, apps.Length)];
                            EntityManager.AddComponent<CIsOnFire>(eA);
                            FinishCustomOrder(key);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        private void FinishCustomOrder(Entity pE)
        {
            CustomOrder co;
            if (m_PrevOrder.ContainsKey(pE))
            {
                co = m_PrevOrder[pE];
            }
            co.OrderIndex = data[pE].OrderIndex;
            co.RunThisTurn = true;
            co.Name = data[pE].Name;
            m_PrevOrder[pE] = co;
        }
        private struct CustomOrder
        {
            public string Name;
            public int OrderIndex;
            public bool RunThisTurn;
            public CustomOrder(int pOrderIndex)
            {
                OrderIndex = pOrderIndex;
                RunThisTurn = false;
            }
        }

    }
}
