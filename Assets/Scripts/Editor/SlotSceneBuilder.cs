using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Slot3x3.EditorTools.BuilderHelpers;

namespace Slot3x3.EditorTools
{
    /// <summary>
    /// Builds the whole classic 3x3 scene from code: camera, background, machine frame, three reels, twenty
    /// paylines with their selector buttons, control panel, jackpot, lamps and the HUD. Run it from the
    /// "Slot3x3/Build Classic Scene" menu; it can be re-run at any time to regenerate the scene.
    ///
    /// Positions are given in the machine's local space (the "Slot" object), measured at the 4:3
    /// design aspect ratio where the machine scale is exactly 1.
    /// </summary>
    public static partial class SlotSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Classic3x3.unity";

        // sorting orders
        private const int OrderBackground = 3, OrderFrame = 4, OrderPlate = 5, OrderPanel = 6, OrderLamp = 7,
                          OrderButton = 10, OrderText = 11, OrderLabel = 12;

        // fonts shared by the whole scene
        private static Font hudFont, titleFont;
        private static Material hudMaterial, titleMaterial;

        [MenuItem("Slot3x3/Build Classic Scene")]
        public static void Build()
        {
            BuildScene();
        }

        private static void BuildScene()
        {
            // open the empty scene first: switching scenes unloads unused assets, which would invalidate anything loaded earlier
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SlotConfig config = ClassicConfigFactory.CreateOrUpdate();
            SlotTile tilePrefab = PrefabBuilder.CreateSlotTile();
            GameObject starParticles = PrefabBuilder.CreateStarParticles();
            GameObject[] coinPrefabs = PrefabBuilder.CreateCoins();
            LoadFonts();

            Camera cam = BuildCamera();
            BuildEventSystem();

            // services
            var player = new GameObject("Player").AddComponent<PlayerWallet>();
            player.Configure(config.startCoins);
            Set(player, "defaultCoins", config.startCoins);
            SoundController sound = BuildSound();
            new GameObject("Input").AddComponent<PointerInput>();

            // machine
            var root = new GameObject("SlotController");
            BuildBackground(root.transform);
            Transform slot = BuildMachineFrame(root.transform);
            Reel[] reels = BuildReels(slot);

            var controlsGo = Child("Controls", slot, Vector3.zero);
            var controls = controlsGo.AddComponent<SlotControls>();
            LinesController lines = BuildLines(controlsGo.transform);

            BuildJackpotHeader(controlsGo.transform, out TextMesh jackpotTitle, out TextMesh jackpotAmount,
                               out Lamps[] lamps, out CoinFountain[] fountains, coinPrefabs);
            TextMesh toastText = BuildPanel(controlsGo.transform, controls, lines);

            windowCanvas = null; // one canvas shared by the windows of this build

            // game logic components
            var presenter = root.AddComponent<WinPresenter>();
            Set(presenter, "lines", lines);
            Set(presenter, "starParticles", starParticles);

            var jackpotPresenter = controlsGo.AddComponent<JackpotPresenter>();
            jackpotPresenter.Setup(jackpotTitle, jackpotAmount, lamps, fountains);

            var toast = controlsGo.AddComponent<MessageToast>();
            toast.Setup(toastText);

            BigWinPopup bigWin = BuildBigWinPopup(cam);
            PopupWindow payTable = BuildPayTableWindow(cam, config);
            SettingsPopup settings = BuildSettingsWindow(cam);
            DailyRewardPopup dailyPopup = BuildDailyRewardWindow(cam, config);
            OutOfCreditsPopup outOfCreditsPopup = BuildOutOfCreditsWindow(cam);
            GameObject dailyBadge = BuildHeader(cam, player, payTable, settings, dailyPopup);
            var daily = new GameObject("DailyReward").AddComponent<DailyRewardController>();
            daily.Setup(dailyBadge, dailyPopup);

            var machine = root.AddComponent<SlotMachine>();
            Set(machine, "config", config);
            Set(machine, "reels", reels);
            Set(machine, "tilePrefab", tilePrefab);
            Set(machine, "lines", lines);
            Set(machine, "controls", controls);
            Set(machine, "winPresenter", presenter);
            Set(machine, "jackpotPresenter", jackpotPresenter);
            Set(machine, "bigWinPopup", bigWin);
            Set(machine, "toast", toast);
            Set(machine, "wallet", player);
            Set(machine, "dailyReward", daily);
            Set(machine, "outOfCredits", outOfCreditsPopup);
            Set(machine, "settings", settings);

            AssetDatabase.SaveAssets();
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("Classic 3x3 scene built: " + ScenePath);
        }

        /// <summary>Entry point for batch mode: Unity -batchmode -executeMethod Slot3x3.EditorTools.SlotSceneBuilder.BuildBatch</summary>
        public static void BuildBatch()
        {
            Build();
        }

        #region setup
        private static void LoadFonts()
        {
            // Unity's built-in font: free to ship and needs no import
            hudFont = titleFont = BuiltInFont;
            hudMaterial = titleMaterial = BuiltInFont.material;
        }

        private static Camera BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -60f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 10.2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.0515f, 0.0515f, 0.0515f, 1f);
            cam.nearClipPlane = 0.3f;
            // the far plane also hides the back half of every reel cylinder (it lies beyond z = 5)
            cam.farClipPlane = 65f;
            go.AddComponent<AudioListener>();
            return cam;
        }

        private static void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static SoundController BuildSound()
        {
            var go = new GameObject("SoundMaster");
            var sound = go.AddComponent<SoundController>();
            // every clip is synthesised by code (see SynthSounds)
            Set(sound, "music", SynthSounds.Get("music"));
            Set(sound, "buttonClick", SynthSounds.Get("button"));
            Set(sound, "windowOpen", SynthSounds.Get("window"));
            Set(sound, "winCoins", SynthSounds.Get("win"));
            Set(sound, "winFreeSpin", SynthSounds.Get("win"));
            Set(sound, "slotRotation", SynthSounds.Get("spin"));
            Set(sound, "slotLose", SynthSounds.Get("lose"));
            Set(sound, "bigWin", SynthSounds.Get("bigwin"));
            Set(sound, "bonus", SynthSounds.Get("bonus"));
            return sound;
        }
        #endregion setup

        #region machine
        private static void BuildBackground(Transform root)
        {
            SpriteRenderer bg = SpriteObject("SlotBkg", root, Vector3.zero, LoadSprite("Assets/Art/Backgrounds/Background Violet.png"), OrderBackground);
            bg.gameObject.AddComponent<SceneScaler>().Setup(1.334f, 1f);
        }

        private static Transform BuildMachineFrame(Transform root)
        {
            SpriteRenderer frame = SpriteObject("Slot", root, new Vector3(0f, -1.01f, 0f), LoadSprite("Assets/Art/Machine/Slot Machine Violet.png"), OrderFrame);
            frame.gameObject.AddComponent<SceneScaler>().Setup(1.334f, 0.04f);

            SpriteObject("background", frame.transform, new Vector3(0f, -0.62f, 0f),
                         LoadSprite("Assets/Art/Machine/Machine Background Violet.png"), OrderPlate);
            return frame.transform;
        }

        private static Reel[] BuildReels(Transform slot)
        {
            Sprite plate = LoadSprite("Assets/Art/Machine/Reel Violet.png");
            Sprite maskSprite = LoadSprite("Assets/Art/Machine/ReelMask.png");
            float[] xs = { -4f, 0f, 4f };
            var reels = new Reel[xs.Length];

            for (int i = 0; i < xs.Length; i++)
            {
                GameObject group = Child("SlotGroup (" + i + ")", slot, new Vector3(xs[i], -0.52f, 0.31f));
                var reel = group.AddComponent<Reel>();

                // window rows, top to bottom
                GameObject rays = Child("RaycasterGroup(" + i + ")", group.transform, Vector3.zero);
                var rows = new[]
                {
                    Child("rc_group_" + i + "(top)", rays.transform, new Vector3(0f, 2.25f, 0f)).transform,
                    Child("rc_group_" + i + "(middle)", rays.transform, Vector3.zero).transform,
                    Child("rc_group_" + i + "(bot)", rays.transform, new Vector3(0f, -2.25f, 0f)).transform,
                };
                Set(reel, "rows", rows);

                // reel plate behind the tiles, and the mask that clips the tiles to the window
                SpriteRenderer foreground = SpriteObject("Foreground (" + i + ")", group.transform, new Vector3(0f, 0f, -0.51f), plate, OrderPlate);
                var mask = Child("Mask (" + i + ")", foreground.transform, new Vector3(0f, 0f, 0.3f)).AddComponent<SpriteMask>();
                mask.sprite = maskSprite;

                reels[i] = reel;
            }
            return reels;
        }
        #endregion machine

        #region paylines
        // which window row (0 = top, 1 = middle, 2 = bottom) each line crosses on reels 1, 2 and 3
        private static readonly int[][] LineRows =
        {
            new[] {1,1,1}, new[] {0,0,0}, new[] {2,2,2}, new[] {0,1,2}, new[] {2,1,0},
            new[] {1,0,1}, new[] {1,2,1}, new[] {0,0,1}, new[] {2,2,1}, new[] {1,1,2},
            new[] {0,1,1}, new[] {0,1,0}, new[] {0,0,2}, new[] {1,0,0}, new[] {1,2,2},
            new[] {2,1,1}, new[] {2,0,1}, new[] {0,2,1}, new[] {2,0,2}, new[] {2,2,0},
        };

        // height of each line's selector button (machine local space); lines 1-10 sit left, 11-20 right
        private static readonly float[] ButtonY =
        {
            -0.87f, 1.94f, -2.97f, 2.63f, -3.67f, -0.17f, -1.59f, 1.23f, -2.28f, 0.53f,
            -0.87f, 1.23f, -2.28f, 2.63f, -2.97f, -0.17f, -1.59f, 0.53f, -3.67f, 1.94f,
        };

        private static LinesController BuildLines(Transform controls)
        {
            Sprite leftNormal = LoadSprite("Assets/Art/Buttons/Line/Line Botton Left Violet Normal copy.png");
            Sprite leftPressed = LoadSprite("Assets/Art/Buttons/Line/Line Botton Left Violet Hover copy.png");
            Sprite rightNormal = LoadSprite("Assets/Art/Buttons/Line/Line Botton Right Violet Normal.png");
            Sprite rightPressed = LoadSprite("Assets/Art/Buttons/Line/Line Botton Right Violet Hover.png");

            GameObject linesGo = Child("SlotLines", controls, new Vector3(0f, -0.52f, 0f));
            var lines = linesGo.AddComponent<LinesController>();
            var views = new PaylineView[LineRows.Length];

            for (int n = 0; n < LineRows.Length; n++)
            {
                bool left = n < 10;
                GameObject lineGo = Child("Line_" + (n + 1).ToString("00"), linesGo.transform, Vector3.zero);
                var view = lineGo.AddComponent<PaylineView>();

                Vector3 buttonPosition = new Vector3(left ? -9.3f : 9.3f, ButtonY[n] + 0.52f, -0.5f);
                SceneButton button = ButtonObject("Button" + (n + 1), lineGo.transform, buttonPosition,
                    left ? leftNormal : rightNormal, left ? leftPressed : rightPressed,
                    left ? new Vector2(1.5f, 0.7f) : new Vector2(1.5f, 0.8f),
                    left ? new Vector2(-0.1f, 0.04f) : new Vector2(0.1f, 0f),
                    false, OrderButton, false, true);

                TextMesh number = TextObject("Text (" + (n + 1) + ")", button.transform, new Vector3(0f, 0.02f, 0f), (n + 1).ToString(),
                    hudFont, hudMaterial, 0.09f, TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderLabel);

                // the amount that appears on a winning line
                TextMesh winText = TextObject("WinText", lineGo.transform, new Vector3(0f, 0f, -0.5f), "",
                    hudFont, hudMaterial, 0.2f, TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderLabel);

                view.Setup(n + 1, LineRows[n], button, winText);
                views[n] = view;
            }

            lines.Setup(views, LoadSprite("Assets/Art/Lines/Line.png"), LoadSprite("Assets/Art/Lines/Line-Circle.png"));
            return lines;
        }
        #endregion paylines

        #region jackpot and lamps
        // measured world positions of the marquee bulbs at the 16:9 reference, converted to machine-local below
        private static readonly Vector2[] LeftBulbs =
        {
            new Vector2(-8.40f, 6.27f), new Vector2(-7.82f, 6.30f), new Vector2(-7.23f, 6.36f),
            new Vector2(-6.61f, 6.35f), new Vector2(-6.02f, 6.38f), new Vector2(-5.41f, 6.36f),
        };
        private static readonly Vector2[] RightBulbs =
        {
            new Vector2(8.39f, 6.25f), new Vector2(7.85f, 6.33f), new Vector2(7.21f, 6.31f),
            new Vector2(6.60f, 6.38f), new Vector2(5.98f, 6.38f), new Vector2(5.37f, 6.34f),
        };

        private static Vector3 ToMachineLocal(Vector2 world)
        {
            const float referenceScale = 1.0133f;
            return new Vector3(world.x / referenceScale, (world.y + 1.01f) / referenceScale, 0f);
        }

        private static void BuildJackpotHeader(Transform controls, out TextMesh title, out TextMesh amount,
                                               out Lamps[] lamps, out CoinFountain[] fountains, GameObject[] coinPrefabs)
        {
            GameObject jackpot = Child("JackPot", controls, Vector3.zero);

            amount = TextObject("TextJackPotAmount", jackpot.transform, new Vector3(-0.26f, 5.29f, -0.5f), "1 000",
                hudFont, hudMaterial, 0.09f, TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderText);
            title = TextObject("TextJackPotTitle", jackpot.transform, new Vector3(-0.071f, 6.579f, -0.5f), "YOUR LUCK HERE",
                titleFont, titleMaterial, 0.068f, TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderText);

            Set(controls.GetComponent<SlotControls>(), "jackpotText", amount);

            // marquee lamps on both sides of the title
            Sprite bulb = LoadSprite("Assets/Art/Lamps/Lamp Light.png");
            GameObject lampsRoot = Child("Lamps", jackpot.transform, Vector3.zero);
            Lamps left = BuildLampSide("LeftLamps", lampsRoot.transform, new Vector3(-6.80f, 7.25f, 0f),
                "Assets/Art/Lamps/Slot Machine Violet Lamps Left.png", LeftBulbs, bulb);
            Lamps right = BuildLampSide("RightLamps", lampsRoot.transform, new Vector3(6.83f, 7.25f, 0f),
                "Assets/Art/Lamps/Slot Machine Violet Lamps Right.png", RightBulbs, bulb);
            lamps = new[] { left, right };

            // coin fountains used by the jackpot celebration
            GameObject coins = Child("Coins", jackpot.transform, Vector3.zero);
            Vector3[] fountainPositions = { new Vector3(-7.36f, -3.48f, 0f), new Vector3(8.01f, -3.48f, 0f), new Vector3(0.03f, -3.48f, 0f) };
            fountains = new CoinFountain[fountainPositions.Length];
            for (int i = 0; i < fountains.Length; i++)
            {
                GameObject f = Child("CoinsFountainScene" + (i == 0 ? "" : " (" + i + ")"), coins.transform, fountainPositions[i]);
                fountains[i] = f.AddComponent<CoinFountain>();
                fountains[i].Setup(coinPrefabs);
            }
        }

        private static Lamps BuildLampSide(string name, Transform parent, Vector3 sideLocalPosition, string spritePath, Vector2[] bulbWorld, Sprite bulbSprite)
        {
            // the lamps root sits at the machine origin, so machine-local positions can be used directly
            SpriteRenderer side = SpriteObject(name, parent, sideLocalPosition, LoadSprite(spritePath), OrderPanel);

            var bulbs = new List<SpriteRenderer>();
            for (int i = 0; i < bulbWorld.Length; i++)
            {
                Vector3 local = ToMachineLocal(bulbWorld[i]) - sideLocalPosition;
                SpriteRenderer b = SpriteObject("Lamp (" + i + ")", side.transform, local, bulbSprite, OrderLamp);
                b.gameObject.SetActive(false);
                bulbs.Add(b);
            }

            var lamps = side.gameObject.AddComponent<Lamps>();
            lamps.Setup(bulbs);
            return lamps;
        }
        #endregion jackpot and lamps
    }
}
