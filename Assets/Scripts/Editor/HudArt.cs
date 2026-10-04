using System;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using UnityEditor;
using UnityEngine;

namespace FGJ.Editor
{
    public static class HudArt
    {
        public const string Folder = "Assets/Art/Sprites/UI/HUD/";
        public const string ItemIconsPath = "Assets/Data/LiarDice/ItemIcons.asset";
        public const string OxygenTank = "Oxygen_Tank";
        public const string OxygenBubble = "Oxygen_Bubble";
        public const string ItemBarFrame = "ItemBar_Frame";
        public const string ControlPanelFrame = "ControlPanel_Frame";

        public static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath(name));

        public static string SpritePath(string name) => $"{Folder}{name}.png";

        public static string ItemIconName(ItemType item) => $"Item_{item}";

        public static ItemIconSet ItemIcons
        {
            get
            {
                var icons = AssetDatabase.LoadAssetAtPath<ItemIconSet>(ItemIconsPath);
                if (icons == null)
                {
                    icons = ScriptableObject.CreateInstance<ItemIconSet>();
                    AssetDatabase.CreateAsset(icons, ItemIconsPath);
                }
                foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
                    icons.SetIcon(item, Load(ItemIconName(item)));
                EditorUtility.SetDirty(icons);
                AssetDatabase.SaveAssets();
                return icons;
            }
        }
    }
}
