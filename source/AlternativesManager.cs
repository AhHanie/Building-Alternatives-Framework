using System.Collections.Generic;
using System.Linq;
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

        public static List<Designator_Build> CreateAlternativeDesignatorsWithStuff(BuildableDef originalDef, ThingDef originalStuffDef)
        {
            var alternatives = GetCachedAlternatives(originalDef);
            var designators = new List<Designator_Build>();
            bool respectsOriginalStuff = originalDef.RespectsOriginalStuff();

            foreach (var alt in alternatives)
            {
                if (respectsOriginalStuff && alt.MadeFromStuff)
                {
                    // Create designators for each available stuff type
                    foreach (ThingDef stuff in GetAvailableStuffFor(alt))
                    {
                        if (originalStuffDef != stuff)
                        {
                            continue;
                        }
                        var designator = new Designator_Build(alt);
                        designator.SetStuffDef(stuff);

                        // Set writeStuff to true to prevent "..." from appearing in labels
                        ReflectionFields.SetWriteStuff(designator, true);

                        designators.Add(designator);
                    }
                }
                else
                {
                    if (alt.MadeFromStuff)
                    {
                        // Create designators for each available stuff type
                        foreach (var stuff in GetAvailableStuffFor(alt))
                        {
                            var designator = new Designator_Build(alt);
                            designator.SetStuffDef(stuff);

                            // Set writeStuff to true to prevent "..." from appearing in labels
                            ReflectionFields.SetWriteStuff(designator, true);

                            designators.Add(designator);
                        }
                    }
                    else
                    {
                        // Create single designator for non-stuff items
                        designators.Add(new Designator_Build(alt));
                    }
                }
            }

            return designators;
        }

        private static List<ThingDef> GetAvailableStuffFor(ThingDef thingDef)
        {
            if (!thingDef.MadeFromStuff)
                return new List<ThingDef>();

            var map = Find.CurrentMap;
            if (map == null)
                return new List<ThingDef>();

            return map.resourceCounter.AllCountedAmounts.Keys
                .Where(stuff => stuff.IsStuff &&
                               stuff.stuffProps.CanMake(thingDef) &&
                               (DebugSettings.godMode || map.listerThings.ThingsOfDef(stuff).Count > 0))
                .OrderByDescending(stuff => stuff.stuffProps?.commonality ?? 0f)
                .ThenBy(stuff => stuff.BaseMarketValue)
                .ToList();
        }

        public static void ValidateAlternatives()
        {
            var allThingDefs = DefDatabase<ThingDef>.AllDefs.ToList();

            foreach (var thingDef in allThingDefs)
            {
                var extension = thingDef.GetModExtension<AlternativesModExtensions>();
                if (extension?.alternatives == null) continue;

                for (int i = extension.alternatives.Count - 1; i >= 0; i--)
                {
                    if (extension.alternatives[i] == null)
                    {
                        Log.Warning($"[Building Alternatives Framework] ThingDef {thingDef.defName} has null alternative at index {i}");
                        extension.alternatives.RemoveAt(i);
                    }
                    else if (!extension.alternatives[i].BuildableByPlayer)
                    {
                        Log.Warning($"[Building Alternatives Framework] ThingDef {thingDef.defName} has alternative {extension.alternatives[i].defName} that is not buildable by player");
                    }
                }
            }
        }
    }
}