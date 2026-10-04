using System.IO;
using FGJ.Exploration;
using UnityEditor;
using UnityEngine;

namespace FGJ.Editor
{
    public sealed class BuildingSpanMeasurer
    {
        private const byte SolidAlpha = 128;

        public Vector2 Measure(BuildingDefinition building)
        {
            var sprite = building.Exterior;
            var (firstColumn, lastColumn, sourceWidth) = SolidColumns(sprite);
            var pixelsToSprite = sprite.rect.width / sourceWidth;
            var unitsPerPixel = building.ExteriorScale.x / sprite.pixelsPerUnit;
            var spriteCenter = building.ScaledExteriorOffset.x;

            var left = spriteCenter + (firstColumn * pixelsToSprite - sprite.pivot.x) * unitsPerPixel;
            var right = spriteCenter + ((lastColumn + 1) * pixelsToSprite - sprite.pivot.x) * unitsPerPixel;
            return new Vector2(-left, right);
        }

        private (int first, int last, int width) SolidColumns(Sprite sprite)
        {
            var texture = new Texture2D(2, 2);
            try
            {
                texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)));
                var pixels = texture.GetPixels32();
                var width = texture.width;
                var first = width;
                var last = -1;
                for (var i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i].a < SolidAlpha)
                        continue;
                    var column = i % width;
                    first = Mathf.Min(first, column);
                    last = Mathf.Max(last, column);
                }
                return last < 0 ? (0, width - 1, width) : (first, last, width);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
