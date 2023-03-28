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
            m_ViewQuery = GetEntityQuery(new QueryHelper().All(typeof(CCustomOrder), typeof(CLinkedView)));
        }
        protected override void OnUpdate()
        {
            using var views = m_ViewQuery.ToComponentDataArray<CLinkedView>(Allocator.Temp);
            using var orders = m_ViewQuery.ToComponentDataArray<CCustomOrder>(Allocator.Temp);

            for (int i = 0; i < views.Length;i++)
            {
                SendUpdate(views[i], new CustomTwitchOptionView.ViewData()
                {
                    Index = orders[i].OrderIndex
                });

            }
            /*
            using var views = m_DeconstructViewQuery.ToComponentDataArray<CLinkedView>(Allocator.Temp);
            bool isDay = HasSingleton<SIsDayTime>();

            for (int i = 0; i < views.Length; i++)
            {
                var deconstruct = deconstructs[i];
                var dur = duration[i];
                SendUpdate(views[i], new DeconstructorView.ViewData
                {
                    InUse = deconstruct.InUse,
                    IsDeconstructed = deconstruct.IsDeconstructed,
                    HasDeconstructEvent = deconstruct.IsDeconstructed,
                    IsDay = isDay,
                    Appliance = deconstruct.ApplianceID,
                    Deconstructing = dur.Active,
                    DeconstructionProgress = dur.CurrentChange
                });
            }
            */
        }
    }
}
