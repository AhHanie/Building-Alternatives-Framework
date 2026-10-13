using UnityEngine;

namespace SK_Building_Alternatives_Framework
{
    public static class KeycodeHelper
    {
        public static string GetFriendlyKeyName(KeyCode keyCode)
        {
            switch (keyCode)
            {
                // Special cases for common keys
                case KeyCode.None:
                    return "None";
                case KeyCode.Space:
                    return "Space";
                case KeyCode.Escape:
                    return "Esc";
                case KeyCode.Return:
                    return "Enter";
                case KeyCode.KeypadEnter:
                    return "Num Enter";
                case KeyCode.Backspace:
                    return "Backspace";
                case KeyCode.Delete:
                    return "Delete";
                case KeyCode.Tab:
                    return "Tab";
                case KeyCode.CapsLock:
                    return "Caps Lock";

                // Arrow keys
                case KeyCode.UpArrow:
                    return "Up";
                case KeyCode.DownArrow:
                    return "Down";
                case KeyCode.LeftArrow:
                    return "Left";
                case KeyCode.RightArrow:
                    return "Right";

                // Modifier keys
                case KeyCode.LeftShift:
                    return "L Shift";
                case KeyCode.RightShift:
                    return "R Shift";
                case KeyCode.LeftControl:
                    return "L Ctrl";
                case KeyCode.RightControl:
                    return "R Ctrl";
                case KeyCode.LeftAlt:
                    return "L Alt";
                case KeyCode.RightAlt:
                    return "R Alt";
                case KeyCode.LeftWindows:
                case KeyCode.LeftCommand:
                    return "L Win";
                case KeyCode.RightWindows:
                case KeyCode.RightCommand:
                    return "R Win";

                // Function keys
                case KeyCode.F1: return "F1";
                case KeyCode.F2: return "F2";
                case KeyCode.F3: return "F3";
                case KeyCode.F4: return "F4";
                case KeyCode.F5: return "F5";
                case KeyCode.F6: return "F6";
                case KeyCode.F7: return "F7";
                case KeyCode.F8: return "F8";
                case KeyCode.F9: return "F9";
                case KeyCode.F10: return "F10";
                case KeyCode.F11: return "F11";
                case KeyCode.F12: return "F12";

                // Keypad
                case KeyCode.Keypad0: return "Num 0";
                case KeyCode.Keypad1: return "Num 1";
                case KeyCode.Keypad2: return "Num 2";
                case KeyCode.Keypad3: return "Num 3";
                case KeyCode.Keypad4: return "Num 4";
                case KeyCode.Keypad5: return "Num 5";
                case KeyCode.Keypad6: return "Num 6";
                case KeyCode.Keypad7: return "Num 7";
                case KeyCode.Keypad8: return "Num 8";
                case KeyCode.Keypad9: return "Num 9";
                case KeyCode.KeypadPeriod: return "Num .";
                case KeyCode.KeypadDivide: return "Num /";
                case KeyCode.KeypadMultiply: return "Num *";
                case KeyCode.KeypadMinus: return "Num -";
                case KeyCode.KeypadPlus: return "Num +";
                case KeyCode.KeypadEquals: return "Num =";

                // Number keys (remove the prefix for cleaner display)
                case KeyCode.Alpha0: return "0";
                case KeyCode.Alpha1: return "1";
                case KeyCode.Alpha2: return "2";
                case KeyCode.Alpha3: return "3";
                case KeyCode.Alpha4: return "4";
                case KeyCode.Alpha5: return "5";
                case KeyCode.Alpha6: return "6";
                case KeyCode.Alpha7: return "7";
                case KeyCode.Alpha8: return "8";
                case KeyCode.Alpha9: return "9";

                // Special characters
                case KeyCode.Minus: return "-";
                case KeyCode.Equals: return "=";
                case KeyCode.LeftBracket: return "[";
                case KeyCode.RightBracket: return "]";
                case KeyCode.Backslash: return "\\";
                case KeyCode.Semicolon: return ";";
                case KeyCode.Quote: return "'";
                case KeyCode.Comma: return ",";
                case KeyCode.Period: return ".";
                case KeyCode.Slash: return "/";
                case KeyCode.BackQuote: return "`";

                // Mouse buttons
                case KeyCode.Mouse0: return "LMB";
                case KeyCode.Mouse1: return "RMB";
                case KeyCode.Mouse2: return "MMB";
                case KeyCode.Mouse3: return "Mouse3";
                case KeyCode.Mouse4: return "Mouse4";
                case KeyCode.Mouse5: return "Mouse5";
                case KeyCode.Mouse6: return "Mouse6";

                // For letter keys and others, just return the enum name converted to uppercase
                default:
                    string keyName = keyCode.ToString();

                    // If it's a single letter, just return it uppercased
                    if (keyName.Length == 1 && char.IsLetter(keyName[0]))
                    {
                        return keyName.ToUpper();
                    }

                    // For multi-character names, add spaces before capitals for readability
                    return AddSpacesToPascalCase(keyName);
            }
        }

        public static string GetCyclingInstructionText()
        {
            string nextKey = GetFriendlyKeyName(Settings.cycleNextKey);
            string prevKey = GetFriendlyKeyName(Settings.cyclePreviousKey);

            if (Settings.cycleNextKey == KeyCode.None && Settings.cyclePreviousKey == KeyCode.None)
            {
                return "";
            }
            else if (Settings.cycleNextKey == KeyCode.None)
            {
                return $"{prevKey}: Previous alternative";
            }
            else if (Settings.cyclePreviousKey == KeyCode.None)
            {
                return $"{nextKey}: Next alternative";
            }
            else
            {
                return $"{prevKey}/{nextKey}: Cycle alternatives";
            }
        }

        private static string AddSpacesToPascalCase(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (i > 0 && char.IsUpper(c))
                {
                    sb.Append(' ');
                }
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}