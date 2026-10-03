using System.IO;
using FGJ.LiarDice.Table;
using UnityEditor;
using UnityEngine;

namespace FGJ.Editor
{
    public sealed class MonsterPlaceholderBuilder
    {
        public const string SpriteFolder = "Assets/Art/Sprites/Characters/Monsters";
        public const string PlaceholderSpritePath = SpriteFolder + "/MonsterPlaceholder.png";

        private const int Width = 512;
        private const int Height = 768;
        private const float PixelsPerUnit = 480f;

        private readonly Color _body = new Color32(26, 30, 28, 255);
        private readonly Color _eye = new Color32(170, 255, 210, 255);
        private readonly Color _pupil = new Color32(12, 20, 16, 255);

        public GameObject CreateMonster()
        {
            var root = new GameObject("Monster");
            var renderer = root.AddComponent<SpriteRenderer>();
            var placeholder = PlaceholderSprite();
            renderer.sprite = placeholder;
            root.AddComponent<MonsterView>().Configure(renderer, placeholder);
            return root;
        }

        private Sprite PlaceholderSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderSpritePath);
            if (existing != null)
                return existing;

            if (!AssetDatabase.IsValidFolder(SpriteFolder))
                AssetDatabase.CreateFolder("Assets/Art/Sprites/Characters", "Monsters");

            var pixels = new Color[Width * Height];
            PaintSilhouette(pixels);
            foreach (var eyeX in new[] { Width * 0.36f, Width * 0.64f })
            {
                var center = new Vector2(eyeX, Height * 0.66f);
                PaintEllipse(pixels, center, new Vector2(38f, 24f), _eye);
                PaintEllipse(pixels, center + new Vector2(0f, -2f), new Vector2(11f, 15f), _pupil);
            }

            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(PlaceholderSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(PlaceholderSpritePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(PlaceholderSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spritePixelsPerUnit = PixelsPerUnit;
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderSpritePath);
        }

        private void PaintSilhouette(Color[] pixels)
        {
            var headCenter = new Vector2(Width * 0.5f, Height * 0.66f);
            var headRadius = new Vector2(Width * 0.36f, Height * 0.27f);
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var shoulderHalfWidth = Mathf.Lerp(Width * 0.48f, Width * 0.36f, Mathf.Clamp01((float)y / (Height * 0.66f)));
                    var inBody = y <= Height * 0.66f && Mathf.Abs(x - Width * 0.5f) <= shoulderHalfWidth;
                    var offset = new Vector2((x - headCenter.x) / headRadius.x, (y - headCenter.y) / headRadius.y);
                    var inHead = offset.sqrMagnitude <= 1f;
                    if (inBody || inHead)
                        pixels[y * Width + x] = _body;
                }
            }
        }

        private void PaintEllipse(Color[] pixels, Vector2 center, Vector2 radius, Color color)
        {
            for (var y = Mathf.FloorToInt(center.y - radius.y); y <= Mathf.CeilToInt(center.y + radius.y); y++)
            {
                for (var x = Mathf.FloorToInt(center.x - radius.x); x <= Mathf.CeilToInt(center.x + radius.x); x++)
                {
                    if (x < 0 || y < 0 || x >= Width || y >= Height)
                        continue;
                    var offset = new Vector2((x - center.x) / radius.x, (y - center.y) / radius.y);
                    if (offset.sqrMagnitude <= 1f)
                        pixels[y * Width + x] = color;
                }
            }
        }
    }
}
