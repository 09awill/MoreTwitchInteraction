using DeconstructorMod.Components;
using Kitchen;
using KitchenData;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeconstructorMod.Customs.Deconstructor;
using TMPro;
using UnityEngine.VFX;
using UnityEngine;
using KitchenLib.Utils;
using KitchenMoreTwitchInteraction;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;

namespace MoreTwitchInteraction
{
    internal class CustomTwitchOptionView : UpdatableObjectView<CustomTwitchOptionView.ViewData>
    {
        [MessagePackObject(false)]
        public struct ViewData : IViewData
        {
            [Key(0)]
            public int HeightIndex;
            [Key(1)]
            public string Name;
            [Key(2)]
            public int OrderIndex;
            public bool IsChangedFrom(ViewData check)
            {
                return HeightIndex != check.HeightIndex || Name != check.Name || OrderIndex != check.OrderIndex;
            }
        }
        public float Height = -1.2f;

        public GameObject Container;

        public Renderer Renderer;

        public TextMeshPro Text;


        public ViewData Data;


        private static readonly int Image = Shader.PropertyToID("_Image");
        protected override void UpdateData(ViewData data)
        {
            gameObject.name = "CUSTOM TWITCH OPTION";
            Vector3 localPosition = Container.transform.localPosition;
            localPosition.y = (Height * ((float)data.HeightIndex + 1)) - 0.5f;
            localPosition.x = - 0.8f;
            Transform t = Renderer.transform;
            t.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            t.localPosition = new Vector3(t.localPosition.x, -0.2f, t.localPosition.z);
            Renderer.transform.localPosition = t.localPosition;
            Renderer.transform.localScale = t.localScale;

            Container.transform.localPosition = localPosition;
            if (!(Renderer == null))
            {
                Text.text = $"!order {data.OrderIndex}";
                Renderer.gameObject.SetActive(true);
                Renderer.material.SetTexture(Image, Mod.Bundle.LoadAsset<Texture>(data.Name.ToString()));
            }
            Data = data;
        }
    }
}
