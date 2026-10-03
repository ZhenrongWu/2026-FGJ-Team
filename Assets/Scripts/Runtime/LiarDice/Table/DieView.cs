using UnityEngine;

namespace FGJ.LiarDice.Table
{
    public sealed class DieView : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer[] tintedRenderers = new Renderer[0];
        [SerializeField] private Color normalTint = Color.white;
        [SerializeField] private Color highlightTint = new Color32(255, 214, 110, 255);

        private MaterialPropertyBlock _propertyBlock;

        public int Value { get; private set; }
        public bool IsHighlighted { get; private set; }

        public void Configure(Renderer[] renderers)
        {
            tintedRenderers = renderers;
        }

        public void ShowValue(int value, float yawDegrees, DieFaceLayout layout)
        {
            Value = value;
            transform.localRotation = layout.RotationShowing(value, yawDegrees);
        }

        public void SetHighlighted(bool highlighted)
        {
            IsHighlighted = highlighted;
            _propertyBlock ??= new MaterialPropertyBlock();
            foreach (var tintedRenderer in tintedRenderers)
            {
                if (tintedRenderer == null)
                    continue;
                tintedRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(BaseColorId, highlighted ? highlightTint : normalTint);
                tintedRenderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
