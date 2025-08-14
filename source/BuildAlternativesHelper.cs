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
            var extension = def.GetModExtension<AlternativesModExtension>();
            return extension?.alternatives != null && extension.alternatives.Count > 0;
        }

        public static List<ThingDef> GetAlternatives(this BuildableDef def)
        {
            var extension = def.GetModExtension<AlternativesModExtension>();
            if (extension?.alternatives == null)
                return new List<ThingDef>();

            return extension.alternatives.Where(alt => alt != null).ToList();
        }

        public static (Texture2D, Texture2D) GetUIIcons(this BuildableDef def)
        {
            var extension = def.GetModExtension<AlternativesModExtension>();
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

        public static bool RespectsOriginalStuff(this BuildableDef def)
        {
            var extension = def.GetModExtension<AlternativesModExtension>();
            if (extension == null)
            {
                return false;
            }
            return extension.respectOriginalStuff;
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