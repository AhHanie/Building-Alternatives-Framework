using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    public static class BuildAlternativesHelper
    {
        public static bool HasAlternatives(this BuildableDef def)
        {
            List<BuildableDef> alternatives = AlternativesManager.GetCachedAlternatives(def.GetAlternativeListTag());
            if (alternatives == null || alternatives.Count == 0)
                return false;

            // Check if any alternatives are actually available (visible)
            return alternatives.Any(alt => IsVisible(alt));
        }

        public static List<BuildableDef> GetAlternatives(this BuildableDef def)
        {
            List<BuildableDef> alternatives = AlternativesManager.GetCachedAlternatives(def.GetAlternativeListTag());
            if (alternatives == null)
                return new List<BuildableDef>();

            // Filter alternatives to only include visible ones
            return alternatives.Where(alt => IsVisible(alt)).ToList();
        }

        public static bool IsHiddenFromGUI(this BuildableDef def)
        {
            AlternativesModExtension extension = AlternativesManager.GetModExtension(def);
            if (extension == null)
            {
                return false;
            }
            return extension.hideFromGUI && !extension.isMaster;
        }

        public static string GetAlternativeListTag(this BuildableDef def)
        {
            AlternativesModExtension extension = AlternativesManager.GetModExtension(def);
            if (extension == null)
            {
                return null;
            }
            return extension.tag;
        }

        private static bool IsVisible(BuildableDef thingDef)
        {
            var tempDesignator = new Designator_Build(thingDef);
            return tempDesignator.Visible;
        }

        public static (Texture2D, Texture2D) GetUIIcons(this BuildableDef def)
        {
            var extension = AlternativesManager.GetModExtension(def);
            if (extension == null)
            {
                return (Resources.DefaultAltButtonIcon, Resources.DefaultAltButtonIconSelected);
            }
            else if (extension.UiIcon != null)
            {
                return (extension.UiIcon, extension.HoverUiIcon);
            }
            return (Resources.DefaultAltButtonIcon, Resources.DefaultAltButtonIconSelected);
        }

        public static List<Designator_Build> GetAlternativeDesignators(this BuildableDef def)
        {
            var alternatives = def.GetAlternatives();
            var designators = new List<Designator_Build>();

            foreach (var alt in alternatives)
            {
                designators.Add(new Designator_Build(alt));
            }

            return designators;
        }

        public static void OpenAlternativesWindow(Designator_Build originalDesignator)
        {
            if (originalDesignator.PlacingDef.HasAlternatives())
            {
                var window = new Dialog_BuildAlternatives(originalDesignator);
                Find.WindowStack.Add(window);
            }
        }
    }
}