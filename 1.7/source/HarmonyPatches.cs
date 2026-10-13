using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.GizmoOnGUI))]
    public static class Designator_Build_GizmoOnGUI_Patch
    {
        private static readonly float MIN_BUTTON_SIZE = 16f;
        private static readonly float GIZMO_HEIGHT = 75f;

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
                    BuildAlternativesHelper.OpenAlternativesWindow(__instance);
                    Event.current.Use();
                    // Mouseover instead of Interacted so the grid never calls ProcessInput for this click
                    __result = new GizmoResult(GizmoState.Mouseover, null);
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
    }

    // Handle float menu clicked from Dialog_BuildAlternatives
    [HarmonyPatch(typeof(DesignatorManager), nameof(DesignatorManager.Select))]
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
            Find.WindowStack.WindowOfType<Dialog_BuildAlternatives>()?.Close();
        }
    }

    // Infix on the single ResolvedAllowedDesignators read inside the architect tab GUI: hides alternatives from
    // the grid without touching other readers (e.g. CacheSearchState) and without a static "drawing" flag.
    [HarmonyPatch(typeof(ArchitectCategoryTab), nameof(ArchitectCategoryTab.DesignationTabOnGUI))]
    public static class ArchitectCategoryTab_DesignationTabOnGUI_Patch
    {
        [HarmonyInfix(typeof(DesignationCategoryDef), nameof(DesignationCategoryDef.ResolvedAllowedDesignators), InnerTargetKind.Getter)]
        public static IEnumerable<Designator> Postfix(IEnumerable<Designator> __result)
        {
            return __result.Where(designator => !(designator is Designator_Build buildDesignator) || !buildDesignator.PlacingDef.IsHiddenFromGUI());
        }
    }

    [HarmonyPatch(typeof(DesignatorManager), nameof(DesignatorManager.ProcessInputEvents))]
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

    [HarmonyPatch(typeof(Designator_Build), "DrawPlaceMouseAttachments")]
    public static class Designator_Build_DrawPlaceMouseAttachments_Patch
    {
        public static void Postfix(Designator_Build __instance, float curX, ref float curY)
        {
            if (!__instance.PlacingDef.HasAlternatives())
                return;

            // Don't show if we're in the architect menu info rect (would overlap with other UI)
            if (ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
                return;

            string cyclingText = KeycodeHelper.GetCyclingInstructionText();

            if (string.IsNullOrEmpty(cyclingText))
                return;

            Rect rect = new Rect(curX, curY, 9999f, Text.LineHeight);

            // Set text color to a subtle color to differentiate from cost info
            GUI.color = new Color(0.8f, 0.9f, 1f, 0.8f);

            Widgets.Label(rect, cyclingText);

            GUI.color = Color.white;

            curY += rect.height;
        }
    }

    // Infix on the filter call inside ArchitectCategoryTab.Matches: the filter is the inner receiver and the
    // designator is the outer argument, so the private quickSearchFilter field no longer needs injecting.
    [HarmonyPatch(typeof(ArchitectCategoryTab), "Matches")]
    public static class ArchitectCategoryTab_Matches_Alternatives_Patch
    {
        [HarmonyInfix(typeof(QuickSearchFilter), nameof(QuickSearchFilter.Matches), typeof(string))]
        public static bool Postfix(bool __result, QuickSearchFilter __instance, [HarmonyOuter] Command c)
        {
            if (__result || !__instance.Active)
                return __result;

            if (!(c is Designator_Build buildDesignator) || buildDesignator.PlacingDef == null)
                return __result;

            if (!buildDesignator.PlacingDef.HasAlternatives())
                return __result;

            foreach (var alternative in buildDesignator.PlacingDef.GetAlternatives())
            {
                if (__instance.Matches(alternative.label))
                    return true;
            }

            return __result;
        }
    }
}
