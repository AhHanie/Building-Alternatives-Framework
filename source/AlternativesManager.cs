using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    public static class AlternativesManager
    {
        private static Dictionary<string, List<BuildableDef>> cachedAlternatives = new Dictionary<string, List<BuildableDef>>();
        private static Dictionary<BuildableDef, AlternativesModExtension> cachedDefsExtensions = new Dictionary<BuildableDef, AlternativesModExtension>();

        public static void ClearCache()
        {
            cachedAlternatives.Clear();
        }

        public static List<BuildableDef> GetCachedAlternatives(string tag)
        {
            if (tag == null)
            {
                return null;
            }

            if (cachedAlternatives.TryGetValue(tag, out var cached))
            {
                return cached;
            }

            return null;
        }

        public static bool ShouldShowAlternativesDropdown(BuildableDef def)
        {
            if (def == null) return false;

            var alternatives = GetCachedAlternatives(def.GetAlternativeListTag());
            return alternatives.Count > 0;
        }

        public static List<Designator_Build> CreateAlternativeDesignators(BuildableDef originalDef)
        {
            var alternatives = GetCachedAlternatives(originalDef.GetAlternativeListTag());
            var designators = new List<Designator_Build>();

            for (int i=1;i< alternatives.Count;i++)
            {
                designators.Add(new Designator_Build(alternatives[i]));
            }

            return designators;
        }

        public static AlternativesModExtension GetModExtension(BuildableDef def)
        {
            if (cachedDefsExtensions.TryGetValue(def, out AlternativesModExtension value))
            {
                return value;
            }
            return null;
        }
        
        public static void Init()
        {
            List<BuildableDef> allBuildableDefs = DefDatabase<BuildableDef>.AllDefsListForReading.ToList();
            bool found;
            foreach (BuildableDef def in allBuildableDefs)
            {
                found = false;
                if (def.modExtensions == null)
                {
                    continue;
                }
                foreach (DefModExtension ext in def.modExtensions)
                {
                    if (ext is AlternativesModExtension altExt)
                    {
                        cachedDefsExtensions[def] = altExt;
                        found = true;
                    }
                }

                if (!found)
                {
                    continue;
                }

                if (!cachedAlternatives.ContainsKey(cachedDefsExtensions[def].tag))
                {
                    cachedAlternatives[cachedDefsExtensions[def].tag] = new List<BuildableDef>();
                }
                if (cachedDefsExtensions[def].isMaster)
                {
                    cachedAlternatives[cachedDefsExtensions[def].tag].Insert(0, def);
                }
                else
                {
                    cachedAlternatives[cachedDefsExtensions[def].tag].Add(def);
                }
            }
        }
    }
}