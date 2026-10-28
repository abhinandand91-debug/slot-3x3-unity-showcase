using UnityEditor;
using UnityEngine;
using static Slot3x3.EditorTools.BuilderHelpers;

namespace Slot3x3.EditorTools
{
    /// <summary>Creates the small reusable prefabs: the reel tile, the win sparkles and the flying coins.</summary>
    public static class PrefabBuilder
    {
        private const string PrefabFolder = "Assets/Prefabs";

        public static SlotTile CreateSlotTile()
        {
            var go = new GameObject("SlotTile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask; // clipped by the reel window mask
            go.AddComponent<SlotTile>();
            return SaveAsPrefab(go, "SlotTile").GetComponent<SlotTile>();
        }

        public static GameObject CreateStarParticles()
        {
            EnsureFolder();
            string materialPath = PrefabFolder + "/StarParticle.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                material.mainTexture = LoadSprite("Assets/Art/Particles/part_star.png").texture;
                AssetDatabase.CreateAsset(material, materialPath);
            }

            var go = new GameObject("StarParticles");
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop();

            var main = ps.main;
            main.duration = 2f;
            main.loop = true;
            main.startLifetime = 1.5f;
            main.startSpeed = 2f;
            main.startSize = 0.5f;
            main.maxParticles = 1000;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 30f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 80f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = 20; // above the symbols (10)

            return SaveAsPrefab(go, "StarParticles");
        }

        /// <summary>Three spinning coins that start on different frames so a burst does not spin in lockstep.</summary>
        public static GameObject[] CreateCoins()
        {
            var frames = new Sprite[7];
            for (int i = 0; i < frames.Length; i++)
                frames[i] = LoadSprite("Assets/Art/Coins/Coin Spin " + (i + 1) + ".png");

            var coins = new GameObject[3];
            for (int c = 0; c < coins.Length; c++)
            {
                var go = new GameObject("Coin" + (c + 1));
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = frames[0];
                sr.sortingOrder = 140;
                go.AddComponent<SpriteFrameAnimator>().Setup(frames, 14f, c * 2);
                coins[c] = SaveAsPrefab(go, "Coin" + (c + 1));
            }
            return coins;
        }

        private static GameObject SaveAsPrefab(GameObject go, string name)
        {
            EnsureFolder();
            string path = PrefabFolder + "/" + name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
    }
}
