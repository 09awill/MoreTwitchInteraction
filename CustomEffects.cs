﻿using Kitchen.Layouts;
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

        }
        public class Order66 : CustomEffect
        {
            private EntityQuery m_PlayerQuery;
            private EntityManager m_EManager;
            public static QueryHelper QueryHelper = new QueryHelper().All(typeof(CPlayer));

            public string Name => "Order66";
            public int OrderIndex => 66;
            public bool ShowUI => false;
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
                if (Random.Range(0f, 1f) > (float)Mod.PManager.GetPreference<PreferenceInt>(Name + "Chance").Get() / 100f) return;

                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                foreach (var p in players)
                {
                    CookItemInHand(p);
                    CreateMessAroundPlayer(p);
                }
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

            public void Update()
            {
                return;
            }
        }
        public class SpeedBoost : CustomEffect
        {
            private EntityQuery m_PlayerQuery;
            private EntityManager m_EManager;
            private float m_SpeedBoostDuration = 5f;
            public static QueryHelper QueryHelper = new QueryHelper().All(typeof(CPlayer));

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
                if (Random.Range(0f, 1f) < (float)Mod.PManager.GetPreference<PreferenceInt>(Name + "Chance").Get() / 100f)
                {
                    using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                    AdjustPlayerSpeed(m_EManager, (float)Mod.PManager.GetPreference<PreferenceInt>(Name + "Effect").Get() / 100, m_SpeedBoostDuration, players);
                }
            }
            public void Update()
            {
                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                foreach (var player in players)
                {
                    if (m_EManager.HasComponent<CTakesDuration>(player))
                    {
                        CTakesDuration duration = m_EManager.GetComponentData<CTakesDuration>(player);
                        //Debug.Log("Duration remaining : " + duration.Remaining);
                        if (duration.Remaining < 0.1f)
                        {
                            m_EManager.RemoveComponent<CTakesDuration>(player);
                            m_EManager.RemoveComponent<CSlowPlayer>(player);
                        }
                    }
                }
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
                if (Random.Range(0f, 1f) < (float)Mod.PManager.GetPreference<PreferenceInt>(Name + "Chance").Get() / 100f)
                {
                    using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                    AdjustPlayerSpeed(m_EManager, (float)Mod.PManager.GetPreference<PreferenceInt>(Name + "Effect").Get() / 100, m_SlowDuration, players);
                }
            }
            public void Update()
            {
                using var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);
                foreach (var player in players)
                {
                    if (m_EManager.HasComponent<CTakesDuration>(player))
                    {
                        CTakesDuration duration = m_EManager.GetComponentData<CTakesDuration>(player);
                        //Debug.Log("Duration remaining : " + duration.Remaining);
                        if (duration.Remaining < 0.1f)
                        {
                            m_EManager.RemoveComponent<CTakesDuration>(player);
                            m_EManager.RemoveComponent<CSlowPlayer>(player);
                        }
                    }
                }
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
                if (Random.Range(0f, 1f) < (float)Mod.PManager.GetPreference<PreferenceInt>(Name + "Chance").Get() / 100f)
                {
                    using var apps = m_ApplianceQuery.ToEntityArray(Allocator.Temp);
                    Entity eA = apps[Random.Range(0, apps.Length)];
                    m_EManager.AddComponent<CIsOnFire>(eA);
                }
            }
            public void Update()
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


        }





    }
}