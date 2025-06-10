using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmbcApp.LearnGame.UIWidgets.Buttons
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class UIButtonAnimation :
        UIBehaviour, IMaterialModifier,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        private static readonly int HoverStatePropId = Shader.PropertyToID("_HoverState");
        private static readonly int ClickStatePropId = Shader.PropertyToID("_ClickState");
        private static readonly int BasePropId = Shader.PropertyToID("_Base");
        private static readonly int HoverPropId = Shader.PropertyToID("_Hover");
        private static readonly int ClickPropId = Shader.PropertyToID("_Click");

        [SerializeField] private Texture2D baseTexture;
        [SerializeField] private Texture2D hoverTexture;
        [SerializeField] private Texture2D clickTexture;

        private UnityEngine.UI.Button _button;
        private Material _material;
        private UnityEngine.UI.Button Button => _button ? _button : _button = GetComponent<UnityEngine.UI.Button>();

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!Button) return;
            Button.image.SetMaterialDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_material) DestroyImmediate(_material);
            _material = null;
            if (Button) Button.image.SetMaterialDirty();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            if (!IsActive() || !Button) return;
            Button.image.SetMaterialDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!IsActive() || !Button) return;
            Button.image.SetMaterialDirty();
        }
#endif

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!IsActive() || !baseMaterial.HasProperty(HoverStatePropId))
                return baseMaterial;

            if (!_material)
                _material = new Material(baseMaterial)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            _material.CopyPropertiesFromMaterial(baseMaterial);
            _material.SetTexture(BasePropId, baseTexture);
            _material.SetTexture(HoverPropId, hoverTexture);
            _material.SetTexture(ClickPropId, clickTexture);
            return _material;
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Button.interactable) return;
            LMotion.Create(0f, 1f, 0.1f)
                .BindToMaterialFloat(_material, ClickStatePropId)
                .AddTo(gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!Button.interactable) return;
            LMotion.Create(0f, 1f, 0.1f)
                .BindToMaterialFloat(_material, HoverStatePropId)
                .AddTo(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!Button.interactable) return;
            LMotion.Create(1f, 0f, 0.1f)
                .BindToMaterialFloat(_material, HoverStatePropId)
                .AddTo(gameObject);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Button.interactable) return;
            LMotion.Create(1f, 0f, 0.1f)
                .BindToMaterialFloat(_material, ClickStatePropId)
                .AddTo(gameObject);
        }
    }
}