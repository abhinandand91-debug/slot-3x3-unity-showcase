using UnityEngine;
using UnityEngine.UI;
using static Slot3x3.EditorTools.BuilderHelpers;

namespace Slot3x3.EditorTools
{
    public static partial class SlotSceneBuilder
    {
        private const string ArtHud = "Assets/Art/Hud/";
        private const string ArtPopups = "Assets/Art/Popups/";
        private static readonly Vector2 ReferenceResolution = new Vector2(2736f, 2052f);

        /// <summary>A uGUI text in the built-in font. The style name is kept so callers read the same as before.</summary>
        private static Text BitmapText(RectTransform rect, string text, string style, int size, Color color)
        {
            return UIText(rect, text, BuiltInFont, size, color);
        }

        #region header
        /// <summary>The bar across the top of the screen: level progress and the pay table button.</summary>
        private static GameObject BuildHeader(Camera cam, PlayerWallet wallet, PopupWindow payTableWindow, SettingsPopup settingsWindow, DailyRewardPopup dailyWindow)
        {
            // a world space canvas that covers exactly the 4:3 view (100 canvas pixels per world unit)
            var canvasGo = new GameObject("GameCanvas", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cam;
            canvas.sortingOrder = 200;
            canvasGo.AddComponent<GraphicRaycaster>();
            var canvasRect = (RectTransform)canvasGo.transform;
            canvasRect.sizeDelta = ReferenceResolution;
            canvasRect.localScale = Vector3.one * 0.01f;
            canvasRect.position = new Vector3(0f, 0f, -10f);

            // the plate hangs from the top edge
            RectTransform plate = UIRect("HeaderMenuSlot", canvasRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                                         new Vector2(0.5f, 1f), Vector2.zero, new Vector2(2732f, 224f));
            UIImage(plate, LoadSprite(ArtHud + "Bottom GUI Plate.png"));
            canvasGo.AddComponent<HeaderFitter>().Setup(plate, cam);

            BuildLevelBar(plate);
            GameObject badge = BuildDailyButton(plate, dailyWindow);

            RectTransform payTable = UICentered("ButtonPayTable", plate, new Vector2(446.5f, 30f), new Vector2(389f, 152f));
            Image payImage = UIImage(payTable, LoadSprite(ArtHud + "Button Big Violet Normal.png"), true);
            var payButton = payTable.gameObject.AddComponent<Button>();
            payButton.targetGraphic = payImage;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(payButton.onClick, payTableWindow.Open);
            RectTransform payLabel = UIStretched("TextPayTable", payTable);
            BitmapText(payLabel, "Pay Table", "TradesmithRound_SEFont_0(60)", 60, Color.white);

            // menu button opens the settings window
            RectTransform menu = UICentered("ButtonMenu", plate, new Vector2(1200f, 10.7f), new Vector2(331f, 200f));
            Image menuImage = UIImage(menu, LoadSprite(ArtHud + "Button Menu Normal.png"), true);
            var menuButton = menu.gameObject.AddComponent<Button>();
            menuButton.targetGraphic = menuImage;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(menuButton.onClick, settingsWindow.Open);
            return badge;
        }

        /// <summary>The gift button next to the level bar; a glowing badge shows when a reward is waiting.</summary>
        private static GameObject BuildDailyButton(RectTransform header, DailyRewardPopup window)
        {
            RectTransform rect = UICentered("ButtonDailyReward", header, new Vector2(-342.6f, 28.9f), new Vector2(175f, 173f));
            Image image = UIImage(rect, LoadSprite(ArtHud + "Button Square Violet Normal.png"), true);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, window.Open);
            UIImage(UICentered("Chip", rect, new Vector2(1.9f, -4f), new Vector2(106f, 106f)), LoadSprite(ArtHud + "Icon Chip Normal.png"));

            RectTransform badge = UICentered("Badge", rect, new Vector2(62f, 62f), new Vector2(90f, 90f));
            Image glow = UIImage(badge, LoadSprite(ArtPopups + "Lamp Light Pop Ups.png"));
            glow.color = new Color(1f, 0.25f, 0.2f, 1f);
            return badge.gameObject;
        }

        private static void BuildLevelBar(RectTransform header)
        {
            RectTransform bar = UICentered("LevelGuiController", header, new Vector2(-761f, 23f), new Vector2(598f, 167f));
            UIImage(bar, LoadSprite(ArtHud + "Progress Bar Empty.png"));

            // fill: starts at the left end of the bar's inner slot and grows to the right
            RectTransform fillRect = UIRect("ProgressSlider", bar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                                            new Vector2(0f, 0.5f), new Vector2(-220.4f, 5f), new Vector2(480f, 53f));
            Image fill = UIImage(fillRect, LoadSprite(ArtHud + "Progress Bar FullTrim.png"));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;

            RectTransform pointer = UICentered("Pointer", bar, new Vector2(-220.4f - 11f, 5f), new Vector2(100f, 100f));
            UIImage(pointer, LoadSprite(ArtHud + "Progress Bar Spot.png"));

            RectTransform star = UICentered("ImageStar", bar, new Vector2(-213f, 9f), new Vector2(78f, 70f));
            UIImage(star, LoadSprite(ArtHud + "Star GUI.png"));

            RectTransform levelRect = UICentered("TextLevelNumber", bar, new Vector2(191.5f, 6.2f), new Vector2(200f, 40f));
            Text level = BitmapText(levelRect, "LV 0", "TradesmithRound_SEFont_1 (36)", 36, Color.white);

            var levelBar = bar.gameObject.AddComponent<LevelBar>();
            levelBar.Setup(fill, pointer, level);
        }
        #endregion header

        #region big win popup
        private static BigWinPopup BuildBigWinPopup(Camera cam)
        {
            var canvasGo = new GameObject("CanvasOverlay(for popups)", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 5f;
            canvas.sortingOrder = 500;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            RectTransform root = UIStretched("BigWinPU", canvasGo.transform);
            RectTransform dim = UIStretched("Bkg", root);
            UIImage(dim, LoadSprite(ArtPopups + "grayBkg_80.png"), true);

            RectTransform panel = UICentered("WM", root, new Vector2(0f, 60f), new Vector2(894f, 689f));
            UIImage(panel, LoadSprite(ArtPopups + "Round Pop Up Plate.png"));

            BitmapText(UICentered("TextWin", panel, new Vector2(0f, 235f), new Vector2(800f, 150f)), "BIG WIN", "TradesmithRound_SEFont_0(60)", 110, Color.white);
            UIImage(UICentered("ImageCoinsFly", panel, new Vector2(0f, 40f), new Vector2(441f, 332f)), LoadSprite(ArtPopups + "Win Pop Up Coins Fly.png"));
            UIImage(UICentered("ImageCoinsStack", panel, new Vector2(0f, -250f), new Vector2(337f, 123f)), LoadSprite(ArtPopups + "Win Pop Up Coins Stack.png"));
            UIImage(UICentered("ImageCoin", panel, new Vector2(-190f, -150f), new Vector2(98f, 98f)), LoadSprite(ArtPopups + "Coin-BigWin.png"));
            Text amount = BitmapText(UICentered("TextBigWinValueCaption", panel, new Vector2(30f, -150f), new Vector2(500f, 120f)), "0", "TradesmithRound_SEFont_5 (90)", 90, Color.white);

            // collect button below the plate
            RectTransform collect = UICentered("ButtonOk", root, new Vector2(0f, -400f), new Vector2(389f, 152f));
            Image collectImage = UIImage(collect, LoadSprite(ArtPopups + "Button Big Violet Normal.png"), true);
            var button = collect.gameObject.AddComponent<Button>();
            button.targetGraphic = collectImage;
            button.transition = Selectable.Transition.SpriteSwap;
            var spriteState = button.spriteState;
            spriteState.highlightedSprite = LoadSprite(ArtPopups + "Button Big Violet Hover.png");
            spriteState.pressedSprite = LoadSprite(ArtPopups + "Button Big Violet Hover.png");
            button.spriteState = spriteState;
            BitmapText(UIStretched("TextCollect", collect), "COLLECT", "TradesmithRound_SEFont_0(60)", 60, Color.white);

            var popup = canvasGo.AddComponent<BigWinPopup>();
            popup.Setup(root.gameObject, amount, button, panel);
            return popup;
        }
        #endregion big win popup
    }
}
