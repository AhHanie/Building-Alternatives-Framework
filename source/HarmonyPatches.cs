using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    [HarmonyPatch(typeof(Designator_Build), "GizmoOnGUI")]
    public static class Designator_Build_GizmoOnGUI_Patch
    {
        private static readonly float MIN_BUTTON_SIZE = 16f;
        private static readonly float GIZMO_HEIGHT = 75f;
        public static bool alternativesButtonClicked = false;

        private static float CalculateButtonSize(Designator_Build instance, float maxWidth)
        {
            (Texture2D defaultIcon, Texture2D hoverIcon) = instance.PlacingDef.GetUIIcons();
            Texture2D iconToMeasure = defaultIcon ?? hoverIcon;

            if (iconToMeasure == null)
                return MIN_BUTTON_SIZE;

            // Use the smaller dimension of the texture as the actual size
            float textureSize = Mathf.Min(iconToMeasure.width, iconToMeasure.height);

            // Calculate max allowed button size based on gizmo dimensions
            float gizmoWidth = instance.GetWidth(maxWidth);
            float maxButtonSize = Mathf.Min(gizmoWidth, GIZMO_HEIGHT);

            // Use texture size but cap it at gizmo dimensions, ensure minimum size
            return Mathf.Clamp(textureSize, MIN_BUTTON_SIZE, maxButtonSize);
        }

        private static Rect CalculateButtonRect(Designator_Build instance, Vector2 topLeft, float maxWidth)
        {
            float buttonSize = CalculateButtonSize(instance, maxWidth);

            // Position button so top-right corner is anchored (scales toward bottom-left)
            float rightEdge = topLeft.x + instance.GetWidth(maxWidth);
            float topEdge = topLeft.y;

            return new Rect(rightEdge - buttonSize, topEdge, buttonSize, buttonSize);
        }

        public static bool Prefix(Designator_Build __instance, Vector2 topLeft, float maxWidth, GizmoRenderParms parms, ref GizmoResult __result)
        {
            if (__instance.PlacingDef.HasAlternatives())
            {
                var buttonRect = CalculateButtonRect(__instance, topLeft, maxWidth);

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

            var buttonRect = CalculateButtonRect(__instance, topLeft, maxWidth);
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
        public static void Postfix(Designator des)
        {
            if (disablePostfix)
            {
                return;
            }
            if (des is Designator_Build buildDesignator)
            {
                AlternativesCycler.OnDesignatorSelected(buildDesignator);
            }
            var buildAlternativesDialog = Find.WindowStack.Windows.FirstOrDefault(w => w is Dialog_BuildAlternatives);
            buildAlternativesDialog?.Close();
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

    [HarmonyPatch(typeof(ArchitectCategoryTab), "DesignationTabOnGUI")]
    public static class ArchitectCategoryTab_DesignationTabOnGUI_Patch
    {
        public static bool IsDrawingGUI { get; set; }
        public static void Prefix()
        {
            IsDrawingGUI = true;
        }

        public static void Postfix()
        {
            IsDrawingGUI = false;
        }
    }

    [HarmonyPatch(typeof(DesignationCategoryDef), "get_ResolvedAllowedDesignators")]
    public static class DesignationCategoryDef_ResolvedAllowedDesignators_Patch
    {
        public static void Postfix(ref IEnumerable<Designator> __result)
        {
            // Only filter when drawing the GUI
            if (!ArchitectCategoryTab_DesignationTabOnGUI_Patch.IsDrawingGUI)
                return;

            __result = __result.Where(designator =>
            {
                // Check if this is a build designator and should be hidden
                if (designator is Designator_Build buildDesignator)
                {
                    return !buildDesignator.PlacingDef.IsHiddenFromGUI();
                }

                return true;
            });
        }
    }

    [HarmonyPatch(typeof(DesignatorManager), "ProcessInputEvents")]
    public static class DesignatorManager_ProcessInputEvents_Patch
    {
        public static void Postfix()
        {
            HandleAlternativesCycling();
        }

        private static void HandleAlternativesCycling()
        {
            if (Event.current.type != EventType.KeyDown)
                return;

            bool keyHandled = false;

            if (Event.current.keyCode == Settings.cycleNextKey && !Event.current.control && !Event.current.alt && !Event.current.shift)
            {
                AlternativesCycler.CycleToNextAlternative();
                keyHandled = true;
            }
            else if (Event.current.keyCode == Settings.cyclePreviousKey && !Event.current.control && !Event.current.alt && !Event.current.shift)
            {
                AlternativesCycler.CycleToPreviousAlternative();
                keyHandled = true;
            }

            if (keyHandled)
            {
                Event.current.Use();
            }
        }
    }
}