using Verse;
using System.Collections.Generic;

namespace SK_Building_Alternatives_Framework
{
    public class AlternativesModExtensions: DefModExtension
    {
        public List<ThingDef> alternatives;
        public bool respectOriginalStuff;

        public AlternativesModExtensions()
        {
            alternatives = null;
            respectOriginalStuff = false;
        }
    }
}
