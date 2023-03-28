using Kitchen;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Kitchen.ChefConnector.Commands;
using KitchenMoreTwitchInteraction;
using KitchenLib.Preferences;
using static UnityEngine.UI.GridLayoutGroup;
using System.Linq;
using static Kitchen.TwitchNameList;
using KitchenLib.Systems;

namespace KitchenModName
{
    internal class CustomTwitchSystem : RestaurantSystem
    {
        private TwitchNameList nameList;
        private EntityQuery m_PlayerQuery;
        private EntityQuery m_ApplianceQuery;
        private EntityQuery m_OrderQuery;
        private float m_CoolDown = 60f;
        private float m_SpeedBoostDuration = 5f;

        private bool createdView = false;
        //private Dictionary<Entity, TwitchCustomerData> data;
        private Dictionary<string, CCustomOrder> m_Orders;
        protected override void Initialise()
        {
            m_PlayerQuery = GetEntityQuery(new QueryHelper().All(typeof(CPlayer)));
            m_ApplianceQuery = GetEntityQuery(new QueryHelper().All(typeof(CAppliance)).None(typeof(CImmovable), typeof(CPlayer), typeof(CCommandView)));
            m_OrderQuery = GetEntityQuery(new QueryHelper().All(typeof(CCustomOrder)));
            m_Orders = new Dictionary<string, CCustomOrder>();
            EntityManager.CreateEntity();
            //CreateOption(1);
        }
        public void NewOrder(ChefVisitDetails pOrder)
        {
            CCustomOrder ce;
            int DayOfThisOrder= 0;
            if(Require<SDay>(out SDay day))
            {
                DayOfThisOrder = day.Day;
                Mod.LogWarning($"Day of this order : {DayOfThisOrder}");
            }
            if (m_Orders.ContainsKey(pOrder.Name))
            {
                if (m_Orders[pOrder.Name].DayOfLastOrder == DayOfThisOrder && Mod.PManager.GetPreference<PreferenceBool>("OneInteractionPerDay").Get()) return;
                if (m_Orders[pOrder.Name].DayOfLastOrder != 0 && m_Orders[pOrder.Name].OrderIndex == pOrder.Order) return;
                ce = m_Orders[pOrder.Name];
            }
            ce.OrderIndex = pOrder.Order;
            ce.DayOfLastOrder = DayOfThisOrder;
            m_Orders[pOrder.Name] = ce;
            Mod.LogWarning(Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Get());
            Mod.LogWarning($"Speed Chance :{(float)Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Get() / 100f}");
            Mod.LogWarning($"Slow Chance :{(float)Mod.PManager.GetPreference<PreferenceInt>("SlowChance").Get() / 100f}");
            Mod.LogWarning($"Fire Chance :{(float)Mod.PManager.GetPreference<PreferenceInt>("FireChance").Get() / 100f}");

            switch (pOrder.Order)
            {
                case 100:
                    if (Random.Range(0f, 1f) < (float)Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Get()/100f)
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
                        EntityManager.SetComponentData(e, new CSlowPlayer() { Factor = 0.3f, Radius = 1000f });
                    }
                    break;
                case 101:
                    if (Random.Range(0f, 1f) < (float)Mod.PManager.GetPreference<PreferenceInt>("SlowChance").Get()/ 100f)
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
                    if (Random.Range(0f, 1f) < (float)Mod.PManager.GetPreference<PreferenceInt>("FireChance").Get()/100f)
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
            if (Has<SKitchenMarker>() && !Has<SResetCustomOrders>())
            {
                Entity entity = Set<SResetCustomOrders>();
                base.EntityManager.AddComponent<CDoNotPersist>(entity);
                base.EntityManager.AddComponent<CPersistThroughSceneChanges>(entity);
                m_Orders.Clear();
                Mod.LogWarning("Resetting Orders");
            }

            if (!Has<SPerformSceneTransition>() && !Has<SKitchenMarker>())
            {
                Clear<SResetCustomOrders>();
            }
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
            if (!Has<SIsDayTime>() || !Has<STwitchOrderingActive>())
            {
                base.EntityManager.DestroyEntity(m_OrderQuery);
            }
            else
            {
                if (!m_OrderQuery.IsEmpty || Has<SIsDayFirstUpdate>())
                {
                    return;
                }
                CreateOption(100);
                CreateOption(101);
                CreateOption(102);

            }
        }

        private bool CreateOption(int index)
        {
            Entity entity = base.EntityManager.CreateEntity();
            base.EntityManager.AddComponentData(entity, new CRequiresView
            {
                Type = (ViewType)666,
                ViewMode = ViewMode.Screen
            });
            base.EntityManager.AddComponentData(entity, new CPosition(new Vector3(0f, 1f, 0f)));
            base.EntityManager.AddComponentData(entity, new CCustomOrder(index));

            return true;
        }


        public struct CCustomOrder : IComponentData
        {
            public int OrderIndex;
            public int DayOfLastOrder;
            public CCustomOrder(int pOrderIndex)
            {
                OrderIndex = pOrderIndex;
                DayOfLastOrder = 0;
            }
        }

    }
}
