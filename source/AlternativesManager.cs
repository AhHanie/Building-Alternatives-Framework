using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    public static class AlternativesManager
    {
        private static Dictionary<BuildableDef, List<ThingDef>> cachedAlternatives = new Dictionary<BuildableDef, List<ThingDef>>();

        public static void ClearCache()
        {
            cachedAlternatives.Clear();
        }

        public static List<ThingDef> GetCachedAlternatives(BuildableDef def)
        {
            if (cachedAlternatives.TryGetValue(def, out var cached))
            {
                return cached;
            }

            var alternatives = def.GetAlternatives();
            cachedAlternatives[def] = alternatives;
            return alternatives;
        }

        public static bool ShouldShowAlternativesDropdown(BuildableDef def)
        {
            if (def == null) return false;

            var alternatives = GetCachedAlternatives(def);
            return alternatives.Count > 0;
        }

        public static List<Designator_Build> CreateAlternativeDesignators(BuildableDef originalDef)
        {
            var alternatives = GetCachedAlternatives(originalDef);
            var designators = new List<Designator_Build>();

            foreach (var alt in alternatives)
            {
                designators.Add(new Designator_Build(alt));
            }

            return designators;
        }
    }
}