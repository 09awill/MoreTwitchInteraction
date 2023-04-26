using DeconstructorMod.Components;
using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeconstructorMod.Customs.Deconstructor;
using Unity.Collections;
using Unity.Entities;
using static KitchenModName.CustomTwitchSystem;
using MoreTwitchInteraction;
using KitchenMoreTwitchInteraction;

namespace KitchenMoreTwitchInteraction
{
    internal class CustomTwitchOptionViewSystem : ViewSystemBase
    {
        EntityQuery m_ViewQuery;
        protected override void Initialise()
        {
            base.Initialise();
            m_ViewQuery = GetEntityQuery(new QueryHelper().All(typeof(COption), typeof(CLinkedView)));
        }
        protected override void OnUpdate()
        {
            using var views = m_ViewQuery.ToComponentDataArray<CLinkedView>(Allocator.Temp);
            using var orders = m_ViewQuery.ToComponentDataArray<COption>(Allocator.Temp);

            for (int i = 0; i < views.Length;i++)
            {
                SendUpdate(views[i], new CustomTwitchOptionView.ViewData()
                {
                    HeightIndex = orders[i].HeightIndex,
                    Name = orders[i].EffectName,
                    OrderIndex = orders[i].OrderIndex
                });

            }
        }
    }
}
