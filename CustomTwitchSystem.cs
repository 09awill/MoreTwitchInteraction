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
using Kitchen.ChefConnector.Commands;

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
        private Dictionary<string, CustomOrder> m_Orders;
        protected override void Initialise()
        {
            m_PlayerQuery = GetEntityQuery(new QueryHelper().All(typeof(CPlayer)));
            m_ApplianceQuery = GetEntityQuery(new QueryHelper().All(typeof(CAppliance)).None(typeof(CImmovable)));
            m_Orders = new Dictionary<string, CustomOrder>();
        }
        public void NewOrder(ChefVisitDetails pOrder)
        {
            if (m_Orders.ContainsKey(pOrder.Name) && m_Orders[pOrder.Name].OrderIndex == pOrder.Order) return;
            CustomOrder ce;
            if (m_Orders.ContainsKey(pOrder.Name))
            {
                ce = m_Orders[pOrder.Name];
            }
            ce.OrderIndex = pOrder.Order;
            ce.RunThisTurn = true;
            m_Orders[pOrder.Name] = ce;

            switch (pOrder.Order)
            {
                case 101:
                    if (Random.Range(0f, 1f) > 0.01)
                    {
                        using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
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
                    }
                    break;
                case 102:
                    if (Random.Range(0f, 1f) > 0.01)
                    {
                        using var apps = m_ApplianceQuery.ToEntityArray(Allocator.Temp);
                        Entity eA = apps[Random.Range(0, apps.Length)];
                        EntityManager.AddComponent<CIsOnFire>(eA);
                    }
                    break;
                default:
                    break;
            }
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

        }
        private struct CustomOrder
        {
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
