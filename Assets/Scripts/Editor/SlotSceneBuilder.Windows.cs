using UnityEngine;
using UnityEngine.UI;
using static Slot3x3.EditorTools.BuilderHelpers;

namespace Slot3x3.EditorTools
{
    public static partial class SlotSceneBuilder
    {
        private const string LettersFont = "TradesmithRound_SEFont_0(60)";

        private static Canvas windowCanvas;

        private static Canvas WindowCanvas(Camera cam)
        {
            if (windowCanvas != null) return windowCanvas;
            var go = new GameObject("CanvasWindows", typeof(RectTransform));
            windowCanvas = go.AddComponent<Canvas>();
            windowCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            windowCanvas.worldCamera = cam;
            windowCanvas.planeDistance = 6f;
            windowCanvas.sortingOrder = 400;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return windowCanvas;
        }

        /// <summary>Creates dim backdrop, plate and exit button for a window and returns the plate rect.</summary>
        private static RectTransform WindowFrame(Canvas canvas, string name, string plateSprite, Vector2 plateSize, Vector2 exitPosition,
                                                 out RectTransform root, out Button exit)
        {
            root = UIStretched(name, canvas.transform);
            UIImage(UIStretched("Bkg", root), LoadSprite(ArtPopups + "grayBkg_80.png"), true);

            RectTransform plate = UICentered("Plate", root, Vector2.zero, plateSize);
            UIImage(plate, LoadSprite(ArtPopups + plateSprite));

            RectTransform exitRect = UICentered("ButtonExit", plate, exitPosition, new Vector2(125f, 124f));
            Image exitImage = UIImage(exitRect, LoadSprite(ArtPopups + "Button Exit Normal.png"), true);
            exit = exitRect.gameObject.AddComponent<Button>();
            exit.targetGraphic = exitImage;
            exit.transition = Selectable.Transition.SpriteSwap;
            var state = exit.spriteState;
            state.highlightedSprite = LoadSprite(ArtPopups + "Button Exit Hover.png");
            state.pressedSprite = LoadSprite(ArtPopups + "Button Exit Hover.png");
            exit.spriteState = state;
            return plate;
        }

        #region pay table
        private static PopupWindow BuildPayTableWindow(Camera cam, SlotConfig config)
        {
            Canvas canvas = WindowCanvas(cam);
            RectTransform plate = WindowFrame(canvas, "PayTablePU", "Pop Up Paytable Plate.png", new Vector2(2272f, 1078f),
                                              new Vector2(985f, 375f), out RectTransform root, out Button exit);

            BitmapText(UICentered("Title", plate, new Vector2(-100f, 385f), new Vector2(900f, 120f)), "PAY TABLE", LettersFont, 90, Color.white);

            // winning combinations, two columns of six rows
            const float rowHeight = 135f;
            for (int i = 0; i < config.payTable.Count; i++)
            {
                PayLine line = config.payTable[i];
                float x = i < 6 ? -820f : -150f;
                float y = 255f - (i % 6) * rowHeight;

                for (int s = 0; s < 3; s++)
                {
                    Sprite sprite = config.symbols[line.symbols[s]].sprite;
                    UIImage(UICentered("Symbol", plate, new Vector2(x + s * 125f, y), new Vector2(115f, 100f)), sprite);
                }

                string reward = line.freeSpins > 0 ? line.freeSpins + " FREE SPINS" : "x " + line.pay;
                var label = BitmapText(UICentered("Pay", plate, new Vector2(x + 480f, y), new Vector2(300f, 90f)), reward, LettersFont, 52, Color.white);
                label.alignment = TextAnchor.MiddleLeft;
            }

            // rules
            string rules =
                "WILD replaces any symbol\nexcept the first of a line\n\n" +
                "5 SCATTERS anywhere\nfill the level bar\n\n" +
                "5 JACKPOT symbols anywhere\nwin the progressive jackpot\n\n" +
                "Wins are multiplied\nby the line bet";
            var rulesText = BitmapText(UICentered("Rules", plate, new Vector2(690f, 20f), new Vector2(700f, 700f)), rules, LettersFont, 46, Color.white);
            rulesText.alignment = TextAnchor.MiddleCenter;
            rulesText.lineSpacing = 1.1f;

            var window = root.gameObject.AddComponent<PopupWindow>();
            window.Setup(root.gameObject, exit);
            return window;
        }
        #endregion pay table

        #region settings
        private static SettingsPopup BuildSettingsWindow(Camera cam)
        {
            Canvas canvas = WindowCanvas(cam);
            RectTransform plate = WindowFrame(canvas, "SettingsPU", "Pop Up Settings Plate.png", new Vector2(923f, 972f),
                                              new Vector2(392f, 320f), out RectTransform root, out Button exit);

            BitmapText(UICentered("Title", plate, new Vector2(-30f, 300f), new Vector2(600f, 120f)), "SETTINGS", LettersFont, 80, Color.white);

            Sprite on = LoadSprite(ArtPopups + "Toggle Button On.png");
            Sprite off = LoadSprite(ArtPopups + "Toggle Button Off.png");
            Button sound = ToggleRow(plate, "SOUND", 130f, on);
            Button music = ToggleRow(plate, "MUSIC", -50f, on);

            // developer helpers, only shown in the editor and development builds
            RectTransform dev = UICentered("DevPanel", plate, new Vector2(0f, -310f), new Vector2(800f, 260f));
            BitmapText(UICentered("DevTitle", dev, new Vector2(0f, 105f), new Vector2(600f, 60f)), "DEVELOPER", LettersFont, 40, new Color(1f, 0.8f, 0.3f));
            Button addCoins = SmallButton(dev, "AddCoins", new Vector2(-195f, -15f), "ADD", out Text addLabel);
            Button resetDaily = SmallButton(dev, "ResetDaily", new Vector2(195f, -15f), "RESET DAILY", out Text _);

            var window = root.gameObject.AddComponent<SettingsPopup>();
            window.Setup(root.gameObject, exit);
            window.SetupToggles(sound, music, on, off);
            window.SetupDev(dev.gameObject, addCoins, addLabel, resetDaily);
            return window;
        }

        /// <summary>A violet button with a label, sized for tight spaces.</summary>
        private static Button SmallButton(RectTransform parent, string name, Vector2 position, string label, out Text text)
        {
            RectTransform rect = UICentered(name, parent, position, new Vector2(350f, 136f));
            Image image = UIImage(rect, LoadSprite(ArtPopups + "Button Big Violet Normal.png"), true);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            var state = button.spriteState;
            state.highlightedSprite = LoadSprite(ArtPopups + "Button Big Violet Hover.png");
            state.pressedSprite = LoadSprite(ArtPopups + "Button Big Violet Hover.png");
            button.spriteState = state;
            text = BitmapText(UIStretched("Label", rect), label, LettersFont, 44, Color.white);
            return button;
        }

        private static Button ToggleRow(RectTransform plate, string label, float y, Sprite sprite)
        {
            var text = BitmapText(UICentered(label + "Label", plate, new Vector2(-190f, y), new Vector2(300f, 100f)), label, LettersFont, 64, Color.white);
            text.alignment = TextAnchor.MiddleLeft;
            RectTransform rect = UICentered(label + "Toggle", plate, new Vector2(210f, y), new Vector2(228f, 134f));
            Image image = UIImage(rect, sprite, true);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            return button;
        }
        #endregion settings
    
        #region daily reward
        private static DailyRewardPopup BuildDailyRewardWindow(Camera cam, SlotConfig config)
        {
            Canvas canvas = WindowCanvas(cam);
            RectTransform plate = WindowFrame(canvas, "DailyRewardPU", "Pop Up Paytable Plate.png", new Vector2(2272f, 1078f),
                                              new Vector2(985f, 375f), out RectTransform root, out Button exit);

            BitmapText(UICentered("Title", plate, new Vector2(-100f, 385f), new Vector2(1000f, 120f)), "DAILY REWARD", LettersFont, 90, Color.white);

            int days = config.dailyRewards.Length;
            var plates = new Image[days];
            var dayTexts = new Text[days];
            var amountTexts = new Text[days];
            Sprite coin = LoadSprite(ArtPopups + "Coin-BigWin.png");
            const float step = 290f;

            for (int i = 0; i < days; i++)
            {
                float x = (i - (days - 1) * 0.5f) * step - 40f;
                RectTransform cell = UICentered("Day " + (i + 1), plate, new Vector2(x, 30f), new Vector2(260f, 380f));
                plates[i] = UIImage(cell, null); // no sprite = a solid colour block that the popup tints
                dayTexts[i] = BitmapText(UICentered("DayText", cell, new Vector2(0f, 145f), new Vector2(240f, 70f)), "DAY " + (i + 1), LettersFont, 44, Color.white);
                UIImage(UICentered("Coin", cell, new Vector2(0f, 25f), new Vector2(130f, 130f)), coin);
                amountTexts[i] = BitmapText(UICentered("Amount", cell, new Vector2(0f, -115f), new Vector2(250f, 80f)), "0", "TradesmithRound_SEFont_5 (90)", 56, Color.white);
            }

            RectTransform collectRect = UICentered("ButtonCollect", plate, new Vector2(-40f, -395f), new Vector2(520f, 170f));
            Image collectImage = UIImage(collectRect, LoadSprite(ArtPopups + "Button Big Violet Normal.png"), true);
            var collect = collectRect.gameObject.AddComponent<Button>();
            collect.targetGraphic = collectImage;
            collect.transition = Selectable.Transition.ColorTint;
            Text label = BitmapText(UIStretched("Label", collectRect), "COLLECT", LettersFont, 60, Color.white);

            var window = root.gameObject.AddComponent<DailyRewardPopup>();
            window.Setup(root.gameObject, exit);
            window.SetupDays(plates, dayTexts, amountTexts, collect, label);
            return window;
        }
        #endregion daily reward

        #region out of credits
        private static OutOfCreditsPopup BuildOutOfCreditsWindow(Camera cam)
        {
            Canvas canvas = WindowCanvas(cam);
            RectTransform plate = WindowFrame(canvas, "OutOfCreditsPU", "Pop Up Settings Plate.png", new Vector2(923f, 972f),
                                              new Vector2(392f, 320f), out RectTransform root, out Button exit);

            BitmapText(UICentered("Title", plate, new Vector2(-30f, 300f), new Vector2(700f, 120f)), "NO CREDITS", LettersFont, 80, Color.white);
            var message = BitmapText(UICentered("Message", plate, new Vector2(0f, 60f), new Vector2(760f, 300f)),
                "YOU CANNOT AFFORD\nTHIS BET.\n\nLOWER THE BET OR LINES,\nOR TAKE SOME FREE COINS.", LettersFont, 46, Color.white);
            message.lineSpacing = 1.1f;

            Button refill = SmallButton(plate, "Refill", new Vector2(0f, -250f), "GET", out Text refillLabel);

            var window = root.gameObject.AddComponent<OutOfCreditsPopup>();
            window.Setup(root.gameObject, exit);
            window.SetupRefill(refill, refillLabel);
            return window;
        }
        #endregion out of credits
    }
}
