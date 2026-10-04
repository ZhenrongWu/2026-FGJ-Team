using UnityEngine;

namespace FGJ.LiarDice.Table
{
    public sealed class MonsterView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite placeholderSprite;

        public Sprite CurrentSprite => spriteRenderer.sprite;
        public bool IsShowingPlaceholder => spriteRenderer.sprite == placeholderSprite;
        public bool IsShowingDead { get; private set; }

        public void Configure(SpriteRenderer renderer, Sprite placeholder)
        {
            spriteRenderer = renderer;
            placeholderSprite = placeholder;
        }

        public void Show(MonsterProfile monster)
        {
            IsShowingDead = false;
            spriteRenderer.sprite = monster != null && monster.Sprite != null ? monster.Sprite : placeholderSprite;
            if (monster != null)
                MoveToHeight(monster.PositionY);
        }

        private void MoveToHeight(float y)
        {
            var position = transform.localPosition;
            transform.localPosition = new Vector3(position.x, y, position.z);
        }

        public void ShowDead(MonsterProfile monster)
        {
            if (monster == null || monster.DeadSprite == null)
                return;
            spriteRenderer.sprite = monster.DeadSprite;
            IsShowingDead = true;
        }
    }
}
