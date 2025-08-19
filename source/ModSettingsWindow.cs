using RimWorld;
using UnityEngine;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    public static class ModSettingsWindow
    {
        private static Vector2 scrollPosition = Vector2.zero;
        private static bool isCapturingKey = false;
        private static string capturingKeyFor = "";

        // UI Constants
        private const float ROW_HEIGHT = 30f;
        private const float LABEL_WIDTH = 200f;
        private const float BUTTON_WIDTH = 120f;
        private const float SECTION_SPACING = 20f;
        private const float INDENT = 20f;

        public static void Draw(Rect parent)
        {
            var contentRect = new Rect(0f, 0f, parent.width - 20f, GetContentHeight());

            Widgets.BeginScrollView(parent, ref scrollPosition, contentRect);

            float curY = 0f;

            DrawSectionHeader(ref curY, contentRect.width, "SettingsMenu.KeyBindingsSection.Title".Translate());

            DrawKeybindingSetting(ref curY, contentRect.width, "SettingsMenu.KeyBindingsSection.Button.CycleNext.Label".Translate(),
                                ref Settings.cycleNextKey, "cycleNext");

            DrawKeybindingSetting(ref curY, contentRect.width, "SettingsMenu.KeyBindingsSection.Button.CyclePrevious.Label".Translate(),
                                ref Settings.cyclePreviousKey, "cyclePrevious");

            DrawKeybindingSetting(ref curY, contentRect.width, "SettingsMenu.KeyBindingsSection.Button.NavigationKey.Label".Translate(),
                                ref Settings.navigationModifierKey, "navigationModifier");

            curY += SECTION_SPACING;

            DrawActionButtons(ref curY, contentRect.width);

            if (isCapturingKey)
            {
                DrawKeyCaptureOverlay(parent);
            }

            Widgets.EndScrollView();

            HandleKeyCaptureInput();
        }

        private static void DrawSectionHeader(ref float curY, float width, string title)
        {
            Text.Font = GameFont.Medium;
            var headerRect = new Rect(0f, curY, width, 30f);
            Widgets.Label(headerRect, title);

            Widgets.DrawLineHorizontal(0f, curY + 35f, width);

            curY += 55f;
            Text.Font = GameFont.Small;
        }

        private static void DrawKeybindingSetting(ref float curY, float width, string label, ref KeyCode keyCode, string keyId)
        {
            var labelRect = new Rect(INDENT, curY, LABEL_WIDTH, ROW_HEIGHT);
            var buttonRect = new Rect(INDENT + LABEL_WIDTH, curY, BUTTON_WIDTH, ROW_HEIGHT);

            Widgets.Label(labelRect, label);

            string buttonText = isCapturingKey && capturingKeyFor == keyId ? "SettingsMenu.KeyBindingsSection.Button.CapturingKey.Label".Translate().ToString() : keyCode.ToString();
            bool isCapturing = isCapturingKey && capturingKeyFor == keyId;

            if (isCapturing)
            {
                GUI.color = Color.yellow;
            }

            if (Widgets.ButtonText(buttonRect, buttonText))
            {
                if (!isCapturingKey)
                {
                    StartKeyCapture(keyId);
                }
                else if (capturingKeyFor == keyId)
                {
                    StopKeyCapture();
                }
            }

            GUI.color = Color.white;

            var clearButtonRect = new Rect(buttonRect.xMax + 5f, curY, 50f, ROW_HEIGHT);
            if (Widgets.ButtonText(clearButtonRect, "SettingsMenu.KeyBindingsSection.Button.ClearKey.Label".Translate()))
            {
                keyCode = KeyCode.None;
                if (isCapturingKey && capturingKeyFor == keyId)
                {
                    StopKeyCapture();
                }
            }

            curY += ROW_HEIGHT + 5f;
        }

        private static void DrawActionButtons(ref float curY, float width)
        {
            var resetRect = new Rect(INDENT, curY, 120f, 35f);

            if (Widgets.ButtonText(resetRect, "SettingsMenu.Button.ResetDefaults".Translate()))
            {
                Settings.ResetToDefaults();
            }

            curY += 40f;
        }

        private static void DrawKeyCaptureOverlay(Rect parent)
        {
            var overlayRect = new Rect(0f, 0f, parent.width, parent.height);
            Widgets.DrawBoxSolid(overlayRect, new Color(0f, 0f, 0f, 0.5f));

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            var textRect = new Rect(0f, parent.height / 2 - 50f, parent.width, 100f);

            string instructionText = "SettingsMenu.Overlay.Instructions.PressKey".Translate(GetFriendlyKeyName(capturingKeyFor)) + "\n\n" + "SettingsMenu.Overlay.Instructions.EscapeKey".Translate();
            Widgets.Label(textRect, instructionText);

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        private static string GetFriendlyKeyName(string keyId)
        {
            switch (keyId)
            {
                case "cycleNext": return "Cycle to Next Alternative";
                case "cyclePrevious": return "Cycle to Previous Alternative";
                case "navigationModifier": return "Navigation Modifier Key";
                default: return "Unknown Action";
            }
        }

        private static void StartKeyCapture(string keyId)
        {
            isCapturingKey = true;
            capturingKeyFor = keyId;
        }

        private static void StopKeyCapture()
        {
            isCapturingKey = false;
            capturingKeyFor = "";
        }

        private static void HandleKeyCaptureInput()
        {
            if (!isCapturingKey || Event.current.type != EventType.KeyDown)
                return;

            KeyCode pressedKey = Event.current.keyCode;

            if (pressedKey == KeyCode.Escape)
            {
                StopKeyCapture();
                Event.current.Use();
                return;
            }

            if (capturingKeyFor != "navigationModifier" && IsModifierOnlyKey(pressedKey))
            {
                return;
            }

            switch (capturingKeyFor)
            {
                case "cycleNext":
                    Settings.cycleNextKey = pressedKey;
                    break;
                case "cyclePrevious":
                    Settings.cyclePreviousKey = pressedKey;
                    break;
                case "navigationModifier":
                    Settings.navigationModifierKey = pressedKey;
                    break;
            }

            StopKeyCapture();
            Event.current.Use();
        }

        private static bool IsModifierOnlyKey(KeyCode key)
        {
            return key == KeyCode.LeftControl || key == KeyCode.RightControl ||
                   key == KeyCode.LeftShift || key == KeyCode.RightShift ||
                   key == KeyCode.LeftAlt || key == KeyCode.RightAlt ||
                   key == KeyCode.LeftCommand || key == KeyCode.RightCommand ||
                   key == KeyCode.LeftWindows || key == KeyCode.RightWindows;
        }

        private static float GetContentHeight()
        {
            // Calculate total height needed for all content
            float height = 0f; // No title
            height += 55f + (ROW_HEIGHT + 5f) * 3 + SECTION_SPACING; // Keybindings section with increased spacing
            height += 40f; // Action buttons
            height += 20f; // Extra padding
            return height;
        }

        public static bool IsKeyPressed(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        public static bool IsNavigationModifierPressed()
        {
            return Input.GetKey(Settings.navigationModifierKey) ||
                   (Settings.navigationModifierKey == KeyCode.LeftControl && Input.GetKey(KeyCode.RightControl)) ||
                   (Settings.navigationModifierKey == KeyCode.RightControl && Input.GetKey(KeyCode.LeftControl));
        }
    }
}