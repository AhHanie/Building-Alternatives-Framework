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
                // Create single designator per alternative - let ProcessInput handle stuff selection
                var designator = new Designator_Build(alt);

                // If respecting original stuff and both original and alternative are made from stuff,
                // try to set the same stuff type if it's compatible
                if (respectsOriginalStuff && alt.MadeFromStuff && originalStuffDef != null)
                {
                    // Check if the original stuff can be used for this alternative
                    if (originalStuffDef.stuffProps.CanMake(alt))
                    {
                        designator.SetStuffDef(originalStuffDef);
                        // Set writeStuff to true to show the stuff name instead of "..."
                        ReflectionFields.SetWriteStuff(designator, true);
                    }
                    // If original stuff isn't compatible, leave it as default (will show "..." and open float menu when clicked)
                }

                designators.Add(designator);
            }

            return designators;
        }

        public static void ValidateAlternatives()
        {
            var allThingDefs = DefDatabase<ThingDef>.AllDefs.ToList();

            foreach (var thingDef in allThingDefs)
            {
                var extension = thingDef.GetModExtension<AlternativesModExtension>();
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