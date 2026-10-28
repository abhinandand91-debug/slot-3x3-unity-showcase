using UnityEngine;
using static Slot3x3.EditorTools.BuilderHelpers;

namespace Slot3x3.EditorTools
{
    public static partial class SlotSceneBuilder
    {
        private const string MachineButtons = "Assets/Art/Buttons/Machine/";

        /// <summary>The control panel below the reels: spin / auto / max bet, bet and lines buttons and all the readouts.</summary>
        /// <returns>The text used for short messages such as "You have no money."</returns>
        private static TextMesh BuildPanel(Transform controls, SlotControls slotControls, LinesController lines)
        {
            // buttons: name, sprite stem, position, collider size, toggle
            SceneButton betMinus = PanelButton(controls, "BetMinusButton", "Machine Button Bet   Violet", new Vector3(-8.45f, -6.74f, 0f), new Vector2(2f, 1.5f));
            SceneButton betPlus = PanelButton(controls, "BetPlusButton", "Machine Button Bet + Violet", new Vector3(-6.25f, -6.74f, 0f), new Vector2(2.33f, 1.5f));
            SceneButton linesMinus = PanelButton(controls, "LinesMinusButton", "Machine Button Lines   Violet", new Vector3(8.39f, -6.74f, 0f), new Vector2(2.33f, 1.5f), " copy");
            SceneButton linesPlus = PanelButton(controls, "LinesPlusButton", "Machine Button Lines + Violet", new Vector3(6.23f, -6.74f, 0f), new Vector2(2.33f, 1.5f), " copy");
            SceneButton spin = PanelButton(controls, "SpinButton", "Machine Button Spin Violet", new Vector3(0f, -6.71f, 0f), new Vector2(3.35f, 2.13f));
            SceneButton maxBet = PanelButton(controls, "MaxBetButton", "Machine Button Max Bet Violet", new Vector3(-3.14f, -6.71f, 0f), new Vector2(2.94f, 1.93f));
            SceneButton auto = PanelButton(controls, "AutoSpinButton", "Machine Button Auto Violet", new Vector3(3.14f, -6.71f, 0f), new Vector2(2.94f, 1.93f), "", true);

            // readouts
            TextMesh credit = HudText("TextCredit", controls, new Vector3(-8.17f, 5.29f, -0.5f), "Credit", TextAnchor.MiddleCenter, TextAlignment.Center);
            TextMesh balance = HudText("TextBalanceAmount", controls, new Vector3(-4.58f, 5.29f, -0.5f), "0", TextAnchor.MiddleRight, TextAlignment.Right);
            TextMesh win = HudText("TextWin", controls, new Vector3(5.26f, 5.29f, -0.5f), "win ", TextAnchor.MiddleCenter, TextAlignment.Center);
            TextMesh winAmount = HudText("TextWinAmount", controls, new Vector3(8.84f, 5.29f, -0.5f), "0", TextAnchor.MiddleRight, TextAlignment.Right);
            TextMesh totalBet = HudText("TextTotalBet", controls, new Vector3(0f, -5.13f, -0.5f), "TOTAL BET", TextAnchor.MiddleCenter, TextAlignment.Center);
            TextMesh freeSpinsLabel = HudText("FreeSpinsText", controls, new Vector3(-1.66f, -8.19f, -0.5f), "", TextAnchor.MiddleCenter, TextAlignment.Center);
            TextMesh freeSpinsCount = HudText("FreeSpinsCountText", controls, new Vector3(-0.04f, -8.19f, -0.5f), "", TextAnchor.MiddleCenter, TextAlignment.Center);

            BuildDecor(controls);

            // message shown over the reels
            TextMesh message = TextObject("MessageText", controls, new Vector3(0f, -0.5f, -1f), "", hudFont, hudMaterial, 0.18f,
                TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, 150);

            // dark strip behind the message so it stays readable over the symbols
            SpriteRenderer backdrop = SpriteObject("MessageBackdrop", message.transform, new Vector3(0f, 0f, 0.1f),
                                                   LoadSprite("Assets/Art/Popups/grayBkg_80.png"), 149);
            backdrop.transform.localScale = new Vector3(150f, 17f, 1f);

            Set(slotControls, "balanceText", balance);
            Set(slotControls, "winText", winAmount);
            Set(slotControls, "totalBetText", totalBet);
            Set(slotControls, "freeSpinsLabel", freeSpinsLabel);
            Set(slotControls, "freeSpinsCountText", freeSpinsCount);
            Set(slotControls, "spinButton", spin);
            Set(slotControls, "autoSpinButton", auto);
            Set(slotControls, "maxBetButton", maxBet);
            Set(slotControls, "betPlusButton", betPlus);
            Set(slotControls, "betMinusButton", betMinus);
            Set(slotControls, "linesPlusButton", linesPlus);
            Set(slotControls, "linesMinusButton", linesMinus);
            Set(slotControls, "lines", lines);
            Set(slotControls, "hold", BuildHold(controls));
            return message;
        }

        /// <summary>The hold buttons under the reels and the bet multiplier shown on both sides of the reels.</summary>
        private static HoldFeature BuildHold(Transform controls)
        {
            const string art = "Assets/Art/Buttons/Hold/Hold Button ";
            Sprite normal = LoadSprite(art + "Violet Normal.png");
            Sprite pressed = LoadSprite(art + "Red Normal.png");

            GameObject holdGo = Child("Hold", controls, new Vector3(0f, -4.22f, 0f));
            var buttons = new SceneButton[3];
            float[] xs = { -4f, 0f, 4f };
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = ButtonObject("HoldButton (" + i + ")", holdGo.transform, new Vector3(xs[i], 0f, 0f), normal, pressed,
                                          new Vector2(3.35f, 2.13f), Vector2.zero, true, OrderButton + 1, true, false);
                TextObject("Text", buttons[i].transform, new Vector3(0f, 0.061f, 0f), "HOLD", hudFont, hudMaterial, 0.1f,
                           TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderLabel + 1);
            }

            // "X 2" on both sides of the reels
            var multipliers = new TextMesh[2];
            Vector3[] positions = { new Vector3(-8.34f, 3.81f, 0f), new Vector3(7.36f, 3.81f, 0f) };
            for (int i = 0; i < multipliers.Length; i++)
            {
                TextMesh x = TextObject("TextMultiplierX", holdGo.transform, positions[i], "X", hudFont, hudMaterial, 0.1f,
                                        TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderLabel);
                multipliers[i] = TextObject("TextMultiplier", x.transform, new Vector3(0.87f, 0f, 0f), "1", hudFont, hudMaterial, 0.2f,
                                            TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderLabel);
            }

            var hold = holdGo.AddComponent<HoldFeature>();
            hold.Setup(buttons, multipliers);
            return hold;
        }

        private static SceneButton PanelButton(Transform parent, string name, string stem, Vector3 position, Vector2 colliderSize,
                                               string suffix = "", bool toggle = false)
        {
            Sprite normal = LoadSprite(MachineButtons + stem + " Normal" + suffix + ".png");
            Sprite pressed = LoadSprite(MachineButtons + stem + " Hover" + suffix + ".png");
            SceneButton button = ButtonObject(name, parent, position, normal, pressed, colliderSize, Vector2.zero, true, OrderPanel, toggle, false);

            // the generated buttons carry no lettering of their own
            TextObject("Label", button.transform, new Vector3(0f, 0.03f, -0.01f), ButtonLabel(name), hudFont, hudMaterial,
                       LabelSize(name), TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderLabel);
            return button;
        }

        /// <summary>The generated frame has no baked-in display plates or lettering, so draw them.</summary>
        private static void BuildDecor(Transform controls)
        {
            void Plate(string name, Vector2 position, float width, float height)
            {
                SpriteObject(name, controls, new Vector3(position.x, position.y, -0.4f), PlaceholderArt.Plate(width, height), OrderPanel + 1);
            }
            Plate("PlateCredit", new Vector2(-6.75f, 5.29f), 5.0f, 1.15f);
            Plate("PlateWin", new Vector2(6.85f, 5.29f), 4.9f, 1.15f);
            Plate("PlateJackpot", new Vector2(-0.26f, 5.29f), 3.2f, 1.15f);
            Plate("PlateTotalBet", new Vector2(0f, -5.13f), 3.6f, 1.0f);
            Plate("PlateTitle", new Vector2(-0.07f, 6.58f), 4.6f, 0.9f);

            TextObject("TextJackpotLabel", controls, new Vector3(0f, 7.75f, -0.5f), "JACKPOT", titleFont, titleMaterial, 0.16f,
                       TextAnchor.MiddleCenter, TextAlignment.Center, new Color(0.98f, 0.70f, 0.15f), OrderText);
            TextObject("TextBetLabel", controls, new Vector3(-7.35f, -5.3f, -0.5f), "BET", titleFont, titleMaterial, 0.1f,
                       TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderText);
            TextObject("TextLinesLabel", controls, new Vector3(7.3f, -5.3f, -0.5f), "LINES", titleFont, titleMaterial, 0.1f,
                       TextAnchor.MiddleCenter, TextAlignment.Center, Color.white, OrderText);
        }

        private static float LabelSize(string buttonName)
        {
            if (buttonName == "SpinButton") return 0.16f;
            return buttonName.Contains("Plus") || buttonName.Contains("Minus") ? 0.26f : 0.1f;
        }

        private static string ButtonLabel(string buttonName)
        {
            switch (buttonName)
            {
                case "BetMinusButton":
                case "LinesMinusButton": return "-";
                case "BetPlusButton":
                case "LinesPlusButton": return "+";
                case "SpinButton": return "SPIN";
                case "MaxBetButton": return "MAX\nBET";
                case "AutoSpinButton": return "AUTO";
                default: return "";
            }
        }

        private static TextMesh HudText(string name, Transform parent, Vector3 position, string text, TextAnchor anchor, TextAlignment alignment)
        {
            return TextObject(name, parent, position, text, hudFont, hudMaterial, 0.09f, anchor, alignment, HudPurple, OrderText);
        }
    }
}
