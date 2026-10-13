using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace SK_Building_Alternatives_Framework
{
    public class Settings : ModSettings
    {
        // Default keybindings
        public static KeyCode cycleNextKey = KeyCode.F;
        public static KeyCode cyclePreviousKey = KeyCode.H;
        public static KeyCode navigationModifierKey = KeyCode.LeftControl;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref cycleNextKey, "cycleNextKey", KeyCode.F);
            Scribe_Values.Look(ref cyclePreviousKey, "cyclePreviousKey", KeyCode.H);
            Scribe_Values.Look(ref navigationModifierKey, "navigationModifierKey", KeyCode.LeftControl);
            base.ExposeData();
        }

        public static void ResetToDefaults()
        {
            cycleNextKey = KeyCode.F;
            cyclePreviousKey = KeyCode.H;
            navigationModifierKey = KeyCode.LeftControl;
        }
    }
}
