using System.IO;
using FGJ.LiarDice.Table;
using UnityEditor;
using UnityEngine;

namespace FGJ.Editor
{
    public sealed class DicePlaceholderBuilder
    {
        public const string TextureFolder = "Assets/Art/Textures/Dice";
        public const string MaterialFolder = "Assets/Art/Materials/Gameplay";

        public const float DieSize = 0.08f;
        public const float CupTrayRadius = 0.25f;
        public const float TableTopY = 0.75f;

        private const int FaceTextureSize = 256;
        private const float FaceOffset = 0.501f;
        private const float FaceScale = 0.94f;
        private const string LitShader = "Universal Render Pipeline/Lit";

        private static readonly Vector3[] FaceNormals =
            { Vector3.up, Vector3.back, Vector3.right, Vector3.left, Vector3.forward, Vector3.down };

        private static readonly Vector2[][] PipLayouts =
        {
            new[] { new Vector2(0, 0) },
            new[] { new Vector2(-1, 1), new Vector2(1, -1) },
            new[] { new Vector2(-1, 1), new Vector2(0, 0), new Vector2(1, -1) },
            new[] { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1) },
            new[] { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(-1, -1), new Vector2(1, -1) },
            new[]
            {
                new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(-1, -1),
                new Vector2(1, -1)
            }
        };

        private readonly Color _dieWhite = new Color32(244, 244, 246, 255);
        private readonly Color _pipBlack = new Color32(20, 20, 22, 255);
        private readonly Color _pipRed = new Color32(205, 30, 40, 255);

        public GameObject CreateDie()
        {
            EnsureFolders();
            var root = new GameObject("Die");
            root.transform.localScale = Vector3.one * DieSize;

            var renderers = new Renderer[FaceNormals.Length + 1];
            var body = CreatePrimitive(PrimitiveType.Cube, "Body", root.transform, Material("DieBody", _dieWhite));
            body.transform.localScale = Vector3.one * 0.99f;
            renderers[0] = body.GetComponent<Renderer>();

            for (var value = 1; value <= FaceNormals.Length; value++)
            {
                var normal = FaceNormals[value - 1];
                var up = Mathf.Abs(normal.y) > 0.5f ? Vector3.forward : Vector3.up;
                var face = CreatePrimitive(PrimitiveType.Quad, $"Face_{value}", root.transform,
                    FaceMaterial(value));
                face.transform.localPosition = normal * FaceOffset;
                face.transform.localRotation = Quaternion.LookRotation(-normal, up);
                face.transform.localScale = Vector3.one * FaceScale;
                renderers[value] = face.GetComponent<Renderer>();
            }

            root.AddComponent<DieView>().Configure(renderers);
            return root;
        }

        public GameObject CreateCup(Vector3 liftedOffset, Vector3 liftedEuler)
        {
            EnsureFolders();
            var black = Material("CupBlack", new Color32(18, 18, 20, 255), 0.85f);
            var root = new GameObject("DiceCup");

            var tray = CreatePrimitive(PrimitiveType.Cylinder, "Tray", root.transform, black);
            tray.transform.localScale = new Vector3(0.58f, 0.008f, 0.58f);
            tray.transform.localPosition = new Vector3(0f, 0.008f, 0f);

            var diceRoot = new GameObject("DiceRoot").transform;
            diceRoot.SetParent(root.transform, false);
            diceRoot.localPosition = new Vector3(0f, 0.016f + DieSize * 0.5f, 0f);

            var dome = new GameObject("Dome").transform;
            dome.SetParent(root.transform, false);
            dome.localPosition = new Vector3(0f, 0.016f, 0f);
            var shell = CreatePrimitive(PrimitiveType.Cylinder, "Shell", dome, black);
            shell.transform.localScale = new Vector3(0.44f, 0.07f, 0.44f);
            shell.transform.localPosition = new Vector3(0f, 0.07f, 0f);
            var cap = CreatePrimitive(PrimitiveType.Sphere, "Cap", dome, black);
            cap.transform.localScale = Vector3.one * 0.44f;
            cap.transform.localPosition = new Vector3(0f, 0.14f, 0f);

            var cup = root.AddComponent<DiceCupView>();
            cup.Configure(dome, diceRoot, CupTrayRadius);
            var serialized = new SerializedObject(cup);
            serialized.FindProperty("liftedOffset").vector3Value = liftedOffset;
            serialized.FindProperty("liftedEuler").vector3Value = liftedEuler;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        public GameObject CreateTable(GameObject cupPrefab, DieView diePrefab)
        {
            EnsureFolders();
            var root = new GameObject("DiceTable");

            var top = CreatePrimitive(PrimitiveType.Cube, "TableTop", root.transform,
                Material("TableWood", new Color32(64, 44, 30, 255), 0.25f));
            top.transform.localScale = new Vector3(2.2f, 0.06f, 1.5f);
            top.transform.localPosition = new Vector3(0f, TableTopY - 0.03f, 0.2f);

            var playerCup = InstantiateCup(cupPrefab, root.transform, "PlayerCup", new Vector3(0f, TableTopY, -0.22f), 0f);
            var monsterCup = InstantiateCup(cupPrefab, root.transform, "MonsterCup", new Vector3(0f, TableTopY, 0.5f), 180f);

            root.AddComponent<DiceTableView>().Configure(playerCup, monsterCup, diePrefab);
            return root;
        }

        private DiceCupView InstantiateCup(GameObject cupPrefab, Transform parent, string name, Vector3 position,
            float yaw)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(cupPrefab, parent);
            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return instance.GetComponent<DiceCupView>();
        }

        private GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Material material)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            Object.DestroyImmediate(primitive.GetComponent<Collider>());
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            return primitive;
        }

        private Material FaceMaterial(int value)
        {
            var path = $"{MaterialFolder}/DieFace_{value}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            var material = new Material(Shader.Find(LitShader));
            material.SetTexture("_BaseMap", FaceTexture(value));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", 0.45f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private Texture2D FaceTexture(int value)
        {
            var path = $"{TextureFolder}/DieFace_{value}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
                return existing;

            var texture = new Texture2D(FaceTextureSize, FaceTextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color[FaceTextureSize * FaceTextureSize];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = _dieWhite;

            var pipColor = value == 1 || value == 4 ? _pipRed : _pipBlack;
            var pipRadius = FaceTextureSize * (value == 1 ? 0.15f : 0.085f);
            foreach (var pip in PipLayouts[value - 1])
            {
                var center = new Vector2(0.5f, 0.5f) * FaceTextureSize + pip * FaceTextureSize * 0.26f;
                PaintCircle(pixels, center, pipRadius, pipColor);
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private void PaintCircle(Color[] pixels, Vector2 center, float radius, Color color)
        {
            var minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius - 1));
            var maxX = Mathf.Min(FaceTextureSize - 1, Mathf.CeilToInt(center.x + radius + 1));
            var minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius - 1));
            var maxY = Mathf.Min(FaceTextureSize - 1, Mathf.CeilToInt(center.y + radius + 1));
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var coverage = Mathf.Clamp01(radius + 0.5f - Vector2.Distance(new Vector2(x, y), center));
                    var index = y * FaceTextureSize + x;
                    pixels[index] = Color.Lerp(pixels[index], color, coverage);
                }
            }
        }

        private Material Material(string name, Color color, float smoothness = 0.4f)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            var material = new Material(Shader.Find(LitShader));
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private void EnsureFolders()
        {
            EnsureFolder("Assets/Art", "Textures");
            EnsureFolder("Assets/Art/Textures", "Dice");
            EnsureFolder("Assets/Art", "Materials");
            EnsureFolder("Assets/Art/Materials", "Gameplay");
        }

        private void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
