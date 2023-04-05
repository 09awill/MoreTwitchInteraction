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
            m_ApplianceQuery = GetEntityQuery(new QueryHelper().All(typeof(CAppliance)).None(typeof(CImmovable), typeof(CPlayer), typeof(CCommandView), typeof(CDoesNotOccupy), typeof(CPartOfTableSet)));
            m_OrderQuery = GetEntityQuery(new QueryHelper().All(typeof(CCustomOrder)));
            m_Orders = new Dictionary<string, CCustomOrder>();
        }
        public void NewOrder(ChefVisitDetails pOrder)
        {
            if (!Mod.PManager.GetPreference<PreferenceBool>("ExtraOptionsEnabled").Get()) return;
            if (Mod.PManager.GetPreference<PreferenceInt>("InteractionsPerDay").Get() < 1) return;
            CCustomOrder ce;
            int DayOfThisOrder= 0;
            if(Require<SDay>(out SDay day))
            {
                DayOfThisOrder = day.Day;
            }
            bool orderedToday = false;
            if (m_Orders.ContainsKey(pOrder.Name))
            {
                if (m_Orders[pOrder.Name].DayOfLastOrder != 0 && m_Orders[pOrder.Name].OrderIndex == pOrder.Order) return;
                orderedToday = m_Orders[pOrder.Name].DayOfLastOrder == DayOfThisOrder;
                if (orderedToday && m_Orders[pOrder.Name].OrdersThisDay >= Mod.PManager.GetPreference<PreferenceInt>("InteractionsPerDay").Get()) return;
                ce = m_Orders[pOrder.Name];
            }
            ce.OrderIndex = pOrder.Order;
            ce.DayOfLastOrder = DayOfThisOrder;
            ce.OrdersThisDay = orderedToday ? m_Orders[pOrder.Name].OrdersThisDay + 1 : 1;

            m_Orders[pOrder.Name] = ce;

            switch (pOrder.Order)
            {
                case 100:
                    OrderSlow();
                    break;
                case 101:
                    OrderSpeed();
                    break;
                case 102:
                    OrderFire();
                    break;
                default:
                    break;
            }
        }

        private void OrderSlow()
        {
            if (Random.Range(0f, 1f) < (float)Mod.PManager.GetPreference<PreferenceInt>("SlowChance").Get() / 100f)
            {
                AdjustPlayerSpeed((float)Mod.PManager.GetPreference<PreferenceInt>("SlowEffect").Get()/100);
            }
        }
        private void OrderSpeed()
        {
            if (Random.Range(0f, 1f) < (float)Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Get() / 100f)
            {
                AdjustPlayerSpeed((float)Mod.PManager.GetPreference<PreferenceInt>("SpeedEffect").Get()/100);
            }
        }

        private void OrderFire()
        {
            if (Random.Range(0f, 1f) < (float)Mod.PManager.GetPreference<PreferenceInt>("FireChance").Get() / 100f)
            {
                using var apps = m_ApplianceQuery.ToEntityArray(Allocator.Temp);
                Entity eA = apps[Random.Range(0, apps.Length)];
                EntityManager.AddComponent<CIsOnFire>(eA);
            }
        }
        private void AdjustPlayerSpeed(float pFactor)
        {
            using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
            foreach(var p in players) {
                EntityManager.AddComponent<CSlowPlayer>(p);
                CTakesDuration cTakesDuration = new CTakesDuration()
                {
                    Total = m_SpeedBoostDuration,
                    Active = true,
                    Remaining = m_SpeedBoostDuration,

                };
                EntityManager.AddComponent<CTakesDuration>(p);
                EntityManager.SetComponentData(p, cTakesDuration);
                EntityManager.SetComponentData(p, new CSlowPlayer() { Factor = pFactor, Radius = 0.01f });
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
            if (!Has<SIsDayTime>() || !Has<STwitchOrderingActive>() || !Mod.PManager.GetPreference<PreferenceBool>("ExtraOptionsEnabled").Get())
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
            public int OrdersThisDay;
            public CCustomOrder(int pOrderIndex)
            {
                OrderIndex = pOrderIndex;
                DayOfLastOrder = 0;
                OrdersThisDay = 0;
            }
        }

    }
}
