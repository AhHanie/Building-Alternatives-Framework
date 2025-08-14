using UnityEngine;
using Verse;


namespace SK_Building_Alternatives_Framework
{
    [StaticConstructorOnStartup]
    public static class Resources
    {
        public static readonly Texture2D DefaultAltButtonIcon = ContentFinder<Texture2D>.Get("UI/List");
        public static readonly Texture2D DefaultAltButtonIconSelected = ContentFinder<Texture2D>.Get("UI/ListSelected");
    }
}
