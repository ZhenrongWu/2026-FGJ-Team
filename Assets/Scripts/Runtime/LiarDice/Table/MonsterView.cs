using UnityEngine;

namespace FGJ.LiarDice.Table
{
    public sealed class MonsterView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite placeholderSprite;

        public Sprite CurrentSprite => spriteRenderer.sprite;
        public bool IsShowingPlaceholder => spriteRenderer.sprite == placeholderSprite;

        public void Configure(SpriteRenderer renderer, Sprite placeholder)
        {
            spriteRenderer = renderer;
            placeholderSprite = placeholder;
        }

        public void Show(MonsterProfile monster)
        {
            spriteRenderer.sprite = monster != null && monster.Sprite != null ? monster.Sprite : placeholderSprite;
        }
    }
}
