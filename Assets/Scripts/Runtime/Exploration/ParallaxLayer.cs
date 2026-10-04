using UnityEngine;

namespace FGJ.Exploration
{
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [Range(0f, 1f)] [SerializeField] private float followFactor;
        [Min(0.01f)] [SerializeField] private float tileWidth = 1f;
        [SerializeField] private Transform[] tiles = new Transform[0];
        [SerializeField] private bool mirrorAlternateTiles = true;
        [SerializeField] private Sprite tileSprite;
        [Tooltip("開頭幾塊改用這張圖，例如走出洞穴的地面")]
        [SerializeField] private Sprite openingSprite;
        [Min(0)] [SerializeField] private int openingTileCount;

        private SpriteRenderer[] _tileRenderers;

        public float FollowFactor => followFactor;

        public static bool IsMirrored(int tileIndex) => tileIndex % 2 != 0;

        public static bool IsOpeningTile(int tileIndex, int openingTileCount) => tileIndex < openingTileCount;

        public static float TilePositionX(float offset, int tileIndex, float tileWidth, bool mirrored)
        {
            var left = offset + tileIndex * tileWidth;
            return mirrored ? left + tileWidth : left;
        }

        public void Configure(Camera followCamera, float factor, float width, Transform[] layerTiles)
        {
            targetCamera = followCamera;
            followFactor = factor;
            tileWidth = width;
            tiles = layerTiles;
        }

        public void SetOpening(Sprite tile, Sprite opening, int openingTiles)
        {
            tileSprite = tile;
            openingSprite = opening;
            openingTileCount = openingTiles;
        }

        public Sprite SpriteForTile(int tileIndex)
        {
            return openingSprite != null && IsOpeningTile(tileIndex, openingTileCount) ? openingSprite : tileSprite;
        }

        public static int TilesNeeded(float viewWidth, float tileWidth)
        {
            return Mathf.CeilToInt(viewWidth / tileWidth) + 1;
        }

        public static float Offset(float cameraX, float factor) => cameraX * factor;

        public static int FirstTileIndex(float viewLeft, float offset, float tileWidth)
        {
            return Mathf.FloorToInt((viewLeft - offset) / tileWidth);
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
            if (targetCamera == null || tiles.Length == 0)
                return;

            var cameraX = targetCamera.transform.position.x;
            var halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
            var offset = Offset(cameraX, followFactor);
            var first = FirstTileIndex(cameraX - halfWidth, offset, tileWidth);

            _tileRenderers ??= System.Array.ConvertAll(tiles, tile => tile.GetComponent<SpriteRenderer>());
            for (var i = 0; i < tiles.Length; i++)
            {
                var tileIndex = first + i;
                var mirrored = mirrorAlternateTiles && IsMirrored(tileIndex);
                var position = tiles[i].position;
                position.x = TilePositionX(offset, tileIndex, tileWidth, mirrored);
                tiles[i].position = position;
                if (_tileRenderers[i] == null)
                    continue;
                _tileRenderers[i].flipX = mirrored;
                if (tileSprite != null)
                    _tileRenderers[i].sprite = SpriteForTile(tileIndex);
            }
        }
    }
}
