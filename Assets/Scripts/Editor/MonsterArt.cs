using UnityEditor;
using UnityEngine;

namespace FGJ.Editor
{
    public static class MonsterArt
    {
        public const string DeadSuffix = "_Dead";

        private const float PixelsPerUnit = 1475f;
        private const int MaxTextureSize = 2048;

        [MenuItem("FGJ/Data/Update Monster Art")]
        public static void UpdateAll()
        {
            for (var level = 1; level <= ExplorationSceneMenu.LevelCount; level++)
            {
                var monster = LiarDiceSceneMenu.LevelMonster(level);
                if (monster == null)
                    continue;

                var name = LiarDiceSceneMenu.LevelMonsterName(level);
                monster.SetSprite(ConfigureSprite(SpritePath(name)));
                monster.SetDeadSprite(ConfigureSprite(SpritePath(name + DeadSuffix)));
                EditorUtility.SetDirty(monster);
            }
            AssetDatabase.SaveAssets();
        }

        public static string SpritePath(string name) => $"{MonsterPlaceholderBuilder.SpriteFolder}/{name}.png";

        private static Sprite ConfigureSprite(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                return null;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = MaxTextureSize;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spritePixelsPerUnit = PixelsPerUnit;
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
