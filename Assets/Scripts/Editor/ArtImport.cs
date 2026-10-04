using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FGJ.Editor
{
    public static class ArtImport
    {
        public const string BackgroundFolder = "Assets/Art/Sprites/Backgrounds/Cave/";
        public const string CharacterFolder = "Assets/Art/Sprites/Characters/Player/";
        public const string BuildingFolder = "Assets/Art/Sprites/Buildings/";

        private const float BackgroundPixelsPerUnit = 100f;
        private const float CharacterPixelsPerUnit = 600f;
        private const int BackgroundMaxTextureSize = 8192;
        private const int CharacterMaxTextureSize = 2048;
        private const float BuildingPixelsPerUnit = 190f;
        private const int BuildingMaxTextureSize = 4096;
        private static readonly Vector2 BuildingPivot = new Vector2(0.5f, 0.03f);
        private static readonly Vector2 CharacterPivot = new Vector2(0.5f, 0.075f);

        public static readonly string[] BackgroundSprites = { "Ocean_01", "Ocean_02", "Ocean_03", "Ocean_04" };
        public static readonly string[] CharacterSprites =
            { "Right_01", "Right_02", "Right_03", "Forward_01", "Forward_02", "Forward_03" };
        public static readonly string[] BuildingSprites =
            { "Home_Exterior", "Home_Outline", "Tavern_Exterior", "Tavern_Outline", "Mayor_Exterior", "Mayor_Outline" };

        public static void ConfigureAll()
        {
            foreach (var name in BackgroundSprites)
                ConfigureSprite(name, BackgroundPixelsPerUnit, SpriteAlignment.BottomLeft, Vector2.zero,
                    BackgroundMaxTextureSize);
            foreach (var name in CharacterSprites)
                ConfigureSprite(name, CharacterPixelsPerUnit, SpriteAlignment.Custom, CharacterPivot,
                    CharacterMaxTextureSize);
            foreach (var name in BuildingSprites)
                ConfigureSprite(name, BuildingPixelsPerUnit, SpriteAlignment.Custom, BuildingPivot,
                    BuildingMaxTextureSize);
        }

        public static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath(name));
        }

        private static string SpritePath(string name)
        {
            var folder = BackgroundSprites.Contains(name) ? BackgroundFolder
                : BuildingSprites.Contains(name) ? BuildingFolder
                : CharacterFolder;
            return folder + name + ".png";
        }

        private static void ConfigureSprite(string name, float pixelsPerUnit, SpriteAlignment alignment, Vector2 pivot,
            int maxTextureSize)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath(name));
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = maxTextureSize;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spritePixelsPerUnit = pixelsPerUnit;
            settings.spriteAlignment = (int)alignment;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
