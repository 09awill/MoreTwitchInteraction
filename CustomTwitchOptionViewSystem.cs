using Kitchen;
using Unity.Collections;
using Unity.Entities;
using MoreTwitchInteraction;
using static KitchenMoreTwitchInteraction.CustomTwitchSystem;
using UnityEngine;

namespace KitchenMoreTwitchInteraction
{
    internal class CustomTwitchOptionViewSystem : ViewSystemBase
    {
        EntityQuery m_ViewQuery;
        private int m_PrevAnchorVal;
        protected override void Initialise()
        {
            m_PrevAnchorVal = Mod.PrefManager.Get<int>(Mod.ICON_ANCHOR_ID);
            base.Initialise();
            m_ViewQuery = GetEntityQuery(new QueryHelper().All(typeof(COption), typeof(CLinkedView)));

        }
        protected override void OnUpdate()
        {
            using var ents = m_ViewQuery.ToEntityArray(Allocator.Temp);
            using var views = m_ViewQuery.ToComponentDataArray<CLinkedView>(Allocator.Temp);
            using var orders = m_ViewQuery.ToComponentDataArray<COption>(Allocator.Temp);
            using var positions = m_ViewQuery.ToComponentDataArray<CPosition>(Allocator.Temp);
            bool updateAnchor = m_PrevAnchorVal != Mod.PrefManager.Get<int>(Mod.ICON_ANCHOR_ID);

            for (int i = 0; i < views.Length;i++)
            {
                if (updateAnchor)
                {
                    CPosition cPos = positions[i];
                    switch (Mod.PrefManager.Get<int>(Mod.ICON_ANCHOR_ID))
                    {
                        case 0:
                            cPos.Position = new Vector3(1, 1, 0);
                            break;
                        case 1:
                            cPos.Position = new Vector3(0, 1, 0);
                            break;
                        case 2:
                            cPos.Position = new Vector3(0, 0, 0);
                            break;
                        case 3:
                            cPos.Position = new Vector3(1, 0, 0);
                            break;
                    }
                    EntityManager.SetComponentData(ents[i], cPos);
                    Mod.Instance.ResetIconPositions();
                }
                SendUpdate(views[i], new CustomTwitchOptionView.ViewData()
                {
                    HeightIndex = orders[i].HeightIndex,
                    Name = orders[i].EffectName.ToString(),
                    OrderIndex = orders[i].OrderIndex
                });

            }
            m_PrevAnchorVal = Mod.PrefManager.Get<int>(Mod.ICON_ANCHOR_ID);
        }
    }
}
