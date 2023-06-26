using Kitchen;
using MessagePack;
using TMPro;
using UnityEngine;
using KitchenMoreTwitchInteraction;

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
            bool horizontal = Mod.PrefManager.Get<bool>(Mod.HORIZONTAL_ID);
            int size = Mod.PrefManager.Get<int>(Mod.ICON_SIZE_ID);
            float yPos = Mod.PrefManager.Get<int>(Mod.ICON_Y_POS_ID)/ 100f;
            float xPos = Mod.PrefManager.Get<int>(Mod.ICON_X_POS_ID)/ 100f;


            float sizefloat = size / 100f;
            transform.localScale = new Vector3(sizefloat, sizefloat, sizefloat);

            Vector3 localPosition = Container.transform.localPosition;
            if (!horizontal)
            {
                localPosition.y = ((Height * ((float)data.HeightIndex + 1)) - 0.5f) - yPos;
                localPosition.x = -0.8f - xPos;
            } else
            {
                localPosition.y = (Height - 0.5f) - yPos;
                localPosition.x = (1.2f * ((float)data.HeightIndex + 1) - xPos) + 1.2f;
            }
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
