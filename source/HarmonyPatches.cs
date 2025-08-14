using System.Collections.Generic;
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
        public static bool Prefix(Designator_Build __instance, Vector2 topLeft, float maxWidth, GizmoRenderParms parms, ref GizmoResult __result)
        {
            if (__instance.PlacingDef.HasAlternatives())
            {
                float buttonSize = 18f;
                var buttonRect = new Rect(topLeft.x + __instance.GetWidth(maxWidth) - buttonSize - 2f, topLeft.y + 2f, buttonSize, buttonSize);

                if (Mouse.IsOver(buttonRect) && Event.current.type == EventType.MouseDown)
                {
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

            float buttonSize = 18f;
            var buttonRect = new Rect(topLeft.x + __instance.GetWidth(maxWidth) - buttonSize - 2f, topLeft.y + 2f, buttonSize, buttonSize);

            Widgets.DrawBoxSolid(buttonRect, new Color(0.2f, 0.2f, 0.2f, 0.8f));

            GUI.color = Color.yellow;
            Widgets.DrawBox(buttonRect, 1);
            GUI.color = Color.white;

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.yellow;
            Widgets.Label(buttonRect, "A");
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            if (Mouse.IsOver(buttonRect))
            {
                TooltipHandler.TipRegion(buttonRect, "Click to see building alternatives");
            }
        }
    }
}