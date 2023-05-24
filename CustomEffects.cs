using Kitchen.Layouts;
using Kitchen;
using KitchenData;
using KitchenLib.Preferences;
using KitchenLib.References;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using KitchenMoreTwitchInteraction;
using UnityEngine.UIElements;
using KitchenLib.Systems;
using DeconstructorMod.Components;
using static UnityEngine.EventSystems.EventTrigger;

namespace MoreTwitchInteraction
{
    public class CustomEffects
    {
        private static void AdjustPlayerSpeed(EntityManager pEntityManager, float pFactor, float pDuration, NativeArray<Entity> pPlayers)
        {
            foreach (var p in pPlayers)
            {
                pEntityManager.AddComponent<CSlowPlayer>(p);
                CTakesDuration cTakesDuration = new CTakesDuration()
                {
                    Total = pDuration,
                    Active = true,
                    Remaining = pDuration,

                };
                pEntityManager.AddComponent<CTakesDuration>(p);
                pEntityManager.SetComponentData(p, cTakesDuration);
                pEntityManager.SetComponentData(p, new CSlowPlayer() { Factor = pFactor, Radius = 0.01f });
            }
        }
        private static void CheckRemovePlayerSpeedModifiers(EntityManager pEntityManager, NativeArray<Entity> pPlayers)
        {
            foreach (var player in pPlayers)
            {
                if (pEntityManager.HasComponent<CTakesDuration>(player))
                {
                    CTakesDuration duration = pEntityManager.GetComponentData<CTakesDuration>(player);
                    //Debug.Log("Duration remaining : " + duration.Remaining);
                    if (duration.Remaining < 0.1f)
                    {
                        pEntityManager.RemoveComponent<CTakesDuration>(player);
                        pEntityManager.RemoveComponent<CSlowPlayer>(player);
                    }
                }
            }
        }
        public interface CustomEffect
        {
            public string Name { get; }
            public int OrderIndex { get; }
            public bool ShowUI { get; }
            public bool MadvionOnly { get; }
            public bool HasChance { get; }
            public QueryHelper[] GetQueryHelpers();
            public void Initialise(EntityManager pEntityManager, EntityQuery[] pQueries);
            public void Update();
            public void Order();
            /// <summary>
            /// Should be used to clean up effect at night
            /// </summary>
            public void NightUpdate();

        }
        public class Order66 : CustomEffect
        {
            private EntityQuery m_PlayerQuery;
            private EntityQuery m_ApplianceQuery;

            private EntityManager m_EManager;
            public static QueryHelper QueryHelper = new QueryHelper().All(typeof(CPlayer));

            public string Name => "Order66";
            public int OrderIndex => 66;
            public bool ShowUI => false;
            public bool HasChance => true;

            public bool MadvionOnly => false;

            public QueryHelper[] GetQueryHelpers()
            {
                return new QueryHelper[] { new QueryHelper().All(typeof(CPlayer)), new QueryHelper().All(typeof(CAppliance),typeof(CApplyingProcess)).None(typeof(CNoBadProcesses)) };
            }

            public void Initialise(EntityManager pEntityManager, EntityQuery[] pQueries)
            {
                m_EManager = pEntityManager;
                m_PlayerQuery = pQueries[0];
                m_ApplianceQuery = pQueries[1];
            }
            public void Order()
            {
                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                foreach (var p in players)
                {
                    CookItemInHand(p);
                    CreateMessAroundPlayer(p);
                }
                CookItemsOnHobs();
            }

            private void CreateMessAroundPlayer(Entity p)
            {
                if (!m_EManager.RequireComponent(p, out CPosition pos)) return;
                // Define the radius of the area you want to search around the position
                float searchRadius = 2;

                // Get the integer coordinates of the position
                int x = Mathf.RoundToInt(pos.Position.x);
                int z = Mathf.RoundToInt(pos.Position.z);
                foreach (var nearby in LayoutHelpers.AllNearbyRange2)
                {
                    var relPos = new Vector2(pos.Position.x + nearby.x, pos.Position.z + nearby.y);
                    float distance = Vector2.Distance(new Vector2(x, z), relPos);
                    if (distance <= searchRadius)
                    {
                        Entity ent = m_EManager.CreateEntity();
                        m_EManager.AddComponentData(ent, new CPosition(new Vector3(relPos.x, 0, relPos.y)));
                        m_EManager.AddComponentData(ent, new CMessRequest
                        {
                            ID = AssetReference.CustomerMess
                        });
                        // Do something with the tile at (i, j)
                    }
                }
                CSoundEvent.Create(m_EManager, SoundEvent.MessCreated);
            }

            private void CookItemInHand(Entity p)
            {
                //Check there is an item in the players hand
                if (!m_EManager.RequireComponent(p, out CItemHolder holder)) return;
                if (holder.HeldItem == default) return;
                if (!m_EManager.RequireComponent(holder.HeldItem, out CItem item)) return;
                GameDataObject gdo = GameData.Main.Get(item.ID);
                if (gdo == null) return;
                Item itemObj = gdo as Item;
                if (itemObj == null) return;

                foreach (var process in itemObj.DerivedProcesses)
                {
                    if (process.Process.ID == ProcessReferences.Cook)
                    {
                        m_EManager.DestroyEntity(holder.HeldItem);
                        Entity ent = m_EManager.CreateEntity();
                        m_EManager.AddComponentData(ent, new CCreateItem());
                        m_EManager.SetComponentData(ent, new CCreateItem() { ID = process.Result.ID, Holder = p });
                    }
                }
            }
            private void CookItemsOnHobs()
            {
                using var cookers = m_ApplianceQuery.ToEntityArray(Allocator.Temp);
                foreach (var cooker in cookers)
                {
                    if (!m_EManager.RequireComponent(cooker, out CItemHolder holder)) continue;
                    if (holder.HeldItem == default) continue;
                    if (!m_EManager.RequireComponent(holder.HeldItem, out CItem item)) continue;
                    GameDataObject gdo = GameData.Main.Get(item.ID);
                    if (gdo == null) continue;
                    Item itemObj = gdo as Item;
                    if (itemObj == null) continue;

                    foreach (var process in itemObj.DerivedProcesses)
                    {
                        if (process.Process.ID == ProcessReferences.Cook)
                        {
                            m_EManager.DestroyEntity(holder.HeldItem);
                            Entity ent = m_EManager.CreateEntity();
                            m_EManager.AddComponentData(ent, new CCreateItem());
                            m_EManager.SetComponentData(ent, new CCreateItem() { ID = process.Result.ID, Holder = cooker });
                        }
                    }
                }
            }

            public void Update()
            {
                return;
            }

            public void NightUpdate()
            {
                return;
            }
        }
        public class SpeedBoost : CustomEffect
        {
            private EntityQuery m_PlayerQuery;
            private EntityManager m_EManager;
            private float m_SpeedBoostDuration = 5f;
            public string Name => "SpeedBoost";
            public int OrderIndex => 101;
            public bool ShowUI => true;
            public bool HasChance => true;


            public bool MadvionOnly => false;

            public QueryHelper[] GetQueryHelpers()
            {
                return new QueryHelper[] { new QueryHelper().All(typeof(CPlayer)) };
            }

            public void Initialise(EntityManager pEntityManager, EntityQuery[] pQueries)
            {
                m_EManager = pEntityManager;
                m_PlayerQuery = pQueries[0];
            }
            public void Order()
            {
                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                AdjustPlayerSpeed(m_EManager, (float)Mod.PManager.GetPreference<PreferenceInt>(Name + "Effect").Get() / 100, m_SpeedBoostDuration, players);
            }
            public void Update()
            {
                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                CheckRemovePlayerSpeedModifiers(m_EManager, players);
            }
            public void NightUpdate()
            {
                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                CheckRemovePlayerSpeedModifiers(m_EManager, players);
            }
        }
        public class Slow : CustomEffect
        {
            private EntityQuery m_PlayerQuery;
            private EntityManager m_EManager;
            private float m_SlowDuration = 5f;
            public string Name => "Slow";
            public int OrderIndex => 100;
            public bool ShowUI => true;

            public bool MadvionOnly => false;
            public bool HasChance => true;


            public QueryHelper[] GetQueryHelpers()
            {
                return new QueryHelper[] { new QueryHelper().All(typeof(CPlayer)) };
            }

            public void Initialise(EntityManager pEntityManager, EntityQuery[] pQueries)
            {
                m_EManager = pEntityManager;
                m_PlayerQuery = pQueries[0];
            }
            public void Order()
            {
                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                AdjustPlayerSpeed(m_EManager, (float)Mod.PManager.GetPreference<PreferenceInt>(Name + "Effect").Get() / 100, m_SlowDuration, players);
            }
            public void Update()
            {
                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                CheckRemovePlayerSpeedModifiers(m_EManager, players);
            }
            public void NightUpdate()
            {
                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                CheckRemovePlayerSpeedModifiers(m_EManager, players);
            }

        }
        public class SOS : CustomEffect
        {
            private EntityQuery m_FiresQuery;
            private EntityManager m_EManager;
            public static QueryHelper QueryHelper = new QueryHelper().All(typeof(CIsOnFire));

            public string Name => "SOS";
            public int OrderIndex => 911;
            public bool ShowUI => true;

            public bool MadvionOnly => false;
            public bool HasChance => false;


            public QueryHelper[] GetQueryHelpers()
            {
                return new QueryHelper[] { new QueryHelper().All(typeof(CIsOnFire)) };
            }

            public void Initialise(EntityManager pEntityManager, EntityQuery[] pQueries)
            {
                m_EManager = pEntityManager;
                m_FiresQuery = pQueries[0];
            }
            public void Order()
            {
                m_EManager.RemoveComponent<CIsOnFire>(m_FiresQuery);
            }
            public void Update()
            {
                return;
            }
            public void NightUpdate()
            {
                return;
            }
        }
        public class Fire : CustomEffect
        {
            private EntityQuery m_ApplianceQuery;
            private EntityManager m_EManager;
            public string Name => "Fire";
            public int OrderIndex => 102;
            public bool ShowUI => true;

            public bool MadvionOnly => false;

            public bool HasChance => true;

            public QueryHelper[] GetQueryHelpers()
            {
                return new QueryHelper[] { new QueryHelper().All(typeof(CAppliance)).None(typeof(CImmovable), typeof(CPlayer), typeof(CCommandView), typeof(CDoesNotOccupy), typeof(CPartOfTableSet), typeof(CMess), typeof(CMessRequest)) };
            }

            public void Initialise(EntityManager pEntityManager, EntityQuery[] pQueries)
            {
                m_EManager = pEntityManager;
                m_ApplianceQuery = pQueries[0];
            }
            public void Order()
            {
                using var apps = m_ApplianceQuery.ToEntityArray(Allocator.Temp);
                Entity eA = apps[Random.Range(0, apps.Length)];
                m_EManager.AddComponent<CIsOnFire>(eA);
            }
            public void Update()
            {
                return;
            }
            public void NightUpdate()
            {
                return;
            }


        }
        public class CleanMess : CustomEffect
        {
            private EntityQuery m_MessQuery;
            private EntityQuery m_StackableMessQuery;

            private EntityManager m_EManager;
            public string Name => "CleanMess";
            public int OrderIndex => 50;
            public bool ShowUI => true;

            public bool MadvionOnly => true;

            public bool HasChance => false;

            public QueryHelper[] GetQueryHelpers()
            {
                return new QueryHelper[] { new QueryHelper().All(typeof(CMess)), new QueryHelper().All(typeof(CStackableMess)) };
            }

            public void Initialise(EntityManager pEntityManager, EntityQuery[] pQueries)
            {
                m_EManager = pEntityManager;
                m_MessQuery = pQueries[0];
                m_StackableMessQuery = pQueries[1];
            }
            public void Order()
            {
                m_EManager.DestroyEntity(m_MessQuery);
                m_EManager.DestroyEntity(m_StackableMessQuery);
                CSoundEvent.Create(m_EManager, SoundEvent.MopWater);
            }
            public void Update()
            {
                return;
            }
            public void NightUpdate()
            {
                return;
            }


        }

        public class CallNextCustomer : CustomEffect
        {
            private EntityQuery m_AutomatedOrderQuery;
            private EntityQuery m_IsDayEntityQuery;

            private EntityManager m_EManager;
            public string Name => "CallNextCustomer";
            public int OrderIndex => 103;
            public bool ShowUI => true;

            public bool MadvionOnly => false;

            public bool HasChance => true;
            public QueryHelper[] GetQueryHelpers()
            {
                return new QueryHelper[] { new QueryHelper().All(typeof(CBedroomPart), typeof(CAccelerateTimeAfterDuration)), new QueryHelper().All(typeof(SIsDayTime))};
            }

            public void Initialise(EntityManager pEntityManager, EntityQuery[] pQueries)
            {
                m_EManager = pEntityManager;
                m_AutomatedOrderQuery = pQueries[0];
                m_IsDayEntityQuery = pQueries[1];
            }
            public void Order()
            {
                using var isDay = m_IsDayEntityQuery.ToEntityArray(Allocator.Temp);
                if (isDay.Length != 1)
                {
                    return;
                }
                Entity e = m_EManager.CreateEntity();
                //m_EManager.AddComponent<CAppliance>(e);
                CAppliance appliance = new CAppliance()
                {
                    ID = ApplianceReferences.BookingDesk
                };
                m_EManager.AddComponentData(e, appliance);
                m_EManager.AddComponent<CDoesNotOccupy>(e);
                m_EManager.AddComponent<CDoNotPersist>(e);
                m_EManager.AddComponent<CPosition>(e);
                m_EManager.AddComponent<CAccelerateTimeAfterDuration>(e);
                m_EManager.AddComponent<CBedroomPart>(e);
                CLifetime liftime = new CLifetime()
                {
                    RemainingLife = 0.15f
                };
                m_EManager.AddComponentData(e, liftime);

                CDurationRequirement durationRequirement = new CDurationRequirement()
                {
                    NeedsBeforeClosing = true,
                    NeedsScheduledCustomers = false
                };
                m_EManager.AddComponentData(e, durationRequirement);
                CTakesDuration cTakesDuration = new CTakesDuration()
                {
                    Active = true,
                    Remaining = 0,
                    Total = 0.1f
                };
                m_EManager.AddComponentData(e, cTakesDuration);


            }
            public void Update()
            {
                using var apps = m_AutomatedOrderQuery.ToEntityArray(Allocator.Temp);
                /*
                foreach(var app in apps)
                {
                    CTakesDuration cTakesDuration = m_EManager.GetComponentData<CTakesDuration>(app);
                    if (cTakesDuration.Remaining > 50)
                    {
                        m_EManager.DestroyEntity(app);
                    }
                    if (cTakesDuration.Remaining == 0)
                    {
                        cTakesDuration.Remaining = 100;
                        cTakesDuration.Total = 100;
                        m_EManager.SetComponentData(app, cTakesDuration);
                    }
                }
                */
                return;
            }
            public void NightUpdate()
            {
                return;
            }


        }



    }
}
