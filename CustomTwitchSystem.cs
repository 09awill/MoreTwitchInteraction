using Kitchen;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Kitchen.ChefConnector.Commands;
using KitchenMoreTwitchInteraction;
using KitchenLib.Preferences;
using System.Linq;

using MoreTwitchInteraction;
using static MoreTwitchInteraction.CustomEffects;

namespace KitchenModName
{
    internal class CustomTwitchSystem : RestaurantSystem
    {
        private EntityQuery m_OptionsQuery;
        CustomEffect[] m_Effects = new CustomEffect[]
        {
            new Order66(),
            new SpeedBoost(),
            new Slow(),
            new Fire(),
            new CleanMess(),
            new SOS(),
            new CallNextCustomer()
        };

        private Dictionary<string, CCustomOrder> m_Orders;
        private Dictionary<int, COption> m_Options;
        protected override void Initialise()
        {
            m_Options = new Dictionary<int, COption>();
            for (int i = 0; i < m_Effects.Length; i++)
            {
                Mod.LogWarning(m_Effects[i].Name);
                if (m_Effects[i].HasChance && Mod.PManager.GetPreference<PreferenceInt>(m_Effects[i].Name + "Chance").Get() == 0) continue;

                QueryHelper[] helpers = m_Effects[i].GetQueryHelpers();

                EntityQuery[] queries = new EntityQuery[helpers.Length];
                for (int j = 0; j < helpers.Length; j++)
                {
                    queries[j] = GetEntityQuery(helpers[j]);
                }

                m_Effects[i].Initialise(EntityManager, queries);


                m_Options.Add(m_Effects[i].OrderIndex, new COption() { HeightIndex = i, EffectName = m_Effects[i].Name });

            }

            m_Orders = new Dictionary<string, CCustomOrder>();
            m_OptionsQuery = GetEntityQuery(new QueryHelper().All(typeof(COption)));
        }
        public void NewOrder(ChefVisitDetails pOrder)
        {
            if (pOrder.Name != "madvion")
            {
                if (!Has<SIsDayTime>()) return;
                if (!Mod.PManager.GetPreference<PreferenceBool>("ExtraOptionsEnabled").Get()) return;
                if (Mod.PManager.GetPreference<PreferenceInt>("InteractionsPerDay").Get() < 1) return;
            }
            if (!m_Options.ContainsKey(pOrder.Order)) return;
            CCustomOrder ce;
            int DayOfThisOrder = 0;
            if (Require<SDay>(out SDay day))
            {
                DayOfThisOrder = day.Day;
            }
            bool orderedToday = false;
            if (m_Orders.ContainsKey(pOrder.Name))
            {
                if (m_Orders[pOrder.Name].DayOfLastOrder != 0 && m_Orders[pOrder.Name].OrderID == m_Options[pOrder.Order].EffectName) return;
                orderedToday = m_Orders[pOrder.Name].DayOfLastOrder == DayOfThisOrder;
                if (orderedToday && m_Orders[pOrder.Name].OrdersThisDay >= Mod.PManager.GetPreference<PreferenceInt>("InteractionsPerDay").Get() && pOrder.Name != "madvion") return;
                ce = m_Orders[pOrder.Name];
            }
            ce.DayOfLastOrder = DayOfThisOrder;
            ce.OrdersThisDay = orderedToday ? m_Orders[pOrder.Name].OrdersThisDay + 1 : 1;
            ce.OrderID = m_Options[pOrder.Order].EffectName;
            m_Orders[pOrder.Name] = ce;
            CustomEffects.CustomEffect effect = m_Effects.Where(e => e.Name == m_Options[pOrder.Order].EffectName).First();
            if (effect.MadvionOnly && pOrder.Name != "madvion") return;
            if (effect.HasChance && (Random.Range(0f, 1f) > (float)Mod.PManager.GetPreference<PreferenceInt>(effect.Name + "Chance").Get() / 100f)) return;
            effect.Order();
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

            if (!Has<SIsDayTime>() || !Has<STwitchOrderingActive>() || !Mod.PManager.GetPreference<PreferenceBool>("ExtraOptionsEnabled").Get())
            {
                for (int i = 0; i < m_Effects.Length; i++)
                {
                    m_Effects[i].NightUpdate();
                }
                EntityManager.DestroyEntity(m_OptionsQuery);
                return;
            }

            if (Has<SIsDayFirstUpdate>()) return;

            using NativeArray<Entity> options = m_OptionsQuery.ToEntityArray(Allocator.Temp);


            for (int i = 0; i < m_Effects.Length; i++)
            {
                if (m_Options.Values.Where(e => e.EffectName == m_Effects[i].Name).Any())
                {

                    bool shouldShowUI = (m_Effects[i].HasChance && Mod.PManager.GetPreference<PreferenceInt>(m_Effects[i].Name + "Chance").Get() != 0) && m_Effects[i].ShowUI && Mod.PManager.GetPreference<PreferenceBool>("ShowUI").Get();
                    List<Entity> entities = options.Where(e =>
                    {
                        if (Require(e, out COption option))
                        {
                            return option.EffectName == m_Effects[i].Name;
                        }
                        else
                        {
                            return false;
                        }
                    }).ToList();
                    if (shouldShowUI)
                    {
                        if(!entities.Any()) CreateOption(99, m_Effects[i]);
                    }
                    else
                    {
                        if (entities.Any())
                        {
                            for (int j = entities.Count - 1; j >= 0; j--)
                            {
                                EntityManager.DestroyEntity(entities[j]);
                            }
                            entities = null;
                        }

                    }
                    m_Effects[i].Update();
                } else
                {
                    //Initialise, Create option and add to Options
                    QueryHelper[] helpers = m_Effects[i].GetQueryHelpers();
                    EntityQuery[] queries = new EntityQuery[helpers.Length];
                    for (int j = 0; j < helpers.Length; j++)
                    {
                        queries[j] = GetEntityQuery(helpers[j]);
                    }
                    m_Effects[i].Initialise(EntityManager, queries);
                    m_Options.Add(m_Effects[i].OrderIndex, new COption() { HeightIndex = i, EffectName = m_Effects[i].Name});
                }

            }
            using NativeArray<Entity> optionsEntities = m_OptionsQuery.ToEntityArray(Allocator.Temp);
            using NativeArray<COption> optionsComponents = m_OptionsQuery.ToComponentDataArray<COption>(Allocator.Temp);

            for (int i = 0; i < optionsEntities.Length; i++)
            {
                COption opt = optionsComponents[i];
                opt.HeightIndex = i;
                EntityManager.SetComponentData(optionsEntities[i], opt);
            }

        }
        private Entity CreateOption(int pIndex, CustomEffects.CustomEffect pEffect )
        {
            Entity entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new CRequiresView
            {
                Type = (ViewType)666,
                ViewMode = ViewMode.Screen
            });
            EntityManager.AddComponentData(entity, new CPosition(new Vector3(1f, 1f, 0f)));
            EntityManager.AddComponentData(entity, new COption(pIndex, pEffect.Name, pEffect.OrderIndex));

            return entity;
        }

        public struct COption : IComponentData
        {
            public FixedString128 EffectName;
            public int HeightIndex;
            public int OrderIndex;
            public COption(int pHeightIndex, FixedString128 pEffectName, int pOrderIndex)
            {
                HeightIndex = pHeightIndex;
                EffectName = pEffectName;
                OrderIndex = pOrderIndex;
            }
        }

        public struct CCustomOrder : IComponentData
        {
            public FixedString128 OrderID;
            public int DayOfLastOrder;
            public int OrdersThisDay;
            public CCustomOrder(string pOrderID)
            {
                OrderID = pOrderID;
                DayOfLastOrder = 0;
                OrdersThisDay = 0;
            }
        }

    }
}
