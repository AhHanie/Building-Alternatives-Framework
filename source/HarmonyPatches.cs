using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    [HarmonyPatch(typeof(Designator_Build), "GizmoOnGUI")]
    public static class Designator_Build_GizmoOnGUI_Patch
    {
        private static readonly float BUTTON_SIZE = 16f;
        public static bool alternativesButtonClicked = false;

        public static bool Prefix(Designator_Build __instance, Vector2 topLeft, float maxWidth, GizmoRenderParms parms, ref GizmoResult __result)
        {
            if (__instance.PlacingDef.HasAlternatives())
            {
                float buttonSize = 18f;
                var buttonRect = new Rect(topLeft.x + __instance.GetWidth(maxWidth) - buttonSize - 2f, topLeft.y + 2f, buttonSize, buttonSize);

                if (Mouse.IsOver(buttonRect) && Event.current.type == EventType.MouseDown)
                {
                    alternativesButtonClicked = true;
                    BuildAlternativesHelper.OpenAlternativesWindow(__instance);
                    Event.current.Use();
                    __result = new GizmoResult(GizmoState.Interacted, Event.current);
                    return false;
                }
            }

            return true;
        }

        public static void Postfix(Designator_Build __instance, Vector2 topLeft, float maxWidth, GizmoRenderParms parms, ref GizmoResult __result)
        {
            if (!__instance.PlacingDef.HasAlternatives())
                return;

            (Texture2D defaultIcon, Texture2D hoverIcon) = __instance.PlacingDef.GetUIIcons();

            var buttonRect = new Rect(topLeft.x + __instance.GetWidth(maxWidth) - BUTTON_SIZE - 2f, topLeft.y + 2f, BUTTON_SIZE, BUTTON_SIZE);
            bool mouseOverRect = Mouse.IsOver(buttonRect);

            Texture2D iconToUse = mouseOverRect ? hoverIcon : defaultIcon;

            Widgets.DrawTextureFitted(buttonRect, iconToUse, 1f);

            if (mouseOverRect)
            {
                TooltipHandler.TipRegion(buttonRect, "Gizmo.Button.Label".Translate());
            }
        }

        public static bool WasAlternativesButtonClicked()
        {
            bool result = alternativesButtonClicked;
            return result;
        }
    }

    // Handle float menu clicked from Dialog_BuildAlternatives
    [HarmonyPatch(typeof(DesignatorManager), "Select")]
    public static class DesignatorManager_Select_Patch
    {
        public static bool disablePostfix = false;
        public static void Postfix()
        {
            if (disablePostfix)
            {
                return;
            }
            var buildAlternativesDialog = Find.WindowStack.Windows.FirstOrDefault(w => w is Dialog_BuildAlternatives);
            if (buildAlternativesDialog != null)
            {
                buildAlternativesDialog.Close();
            }
        }
    }

    [HarmonyPatch(typeof(Designator_Build), "ProcessInput")]
    public static class Designator_Build_ProcessInput_Patch
    {
        public static Designator_Build designator;
        public static bool Prefix(Designator_Build __instance, Event ev)
        {
            // If this designator has alternatives and our button was just clicked, suppress ProcessInput
            if (__instance.PlacingDef.HasAlternatives() && Designator_Build_GizmoOnGUI_Patch.WasAlternativesButtonClicked())
            {
                Designator_Build_GizmoOnGUI_Patch.alternativesButtonClicked = false;
                return false; // Suppress the default ProcessInput behavior
            }

            Designator_Build_GizmoOnGUI_Patch.alternativesButtonClicked = false;

            return true;
        }
    }
}