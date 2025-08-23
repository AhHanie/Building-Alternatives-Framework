using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    public static class AlternativesCycler
    {
        private static BuildableDef cachedMainItem = null;
        private static List<BuildableDef> cachedAlternatives = null;
        private static bool disableCaching = false;

        public static void CycleToNextAlternative()
        {
            if (!CanCycleCurrentDesignator())
            {
                return;
            }
            CycleAlternative(1);
        }

        public static void CycleToPreviousAlternative()
        {
            if (!CanCycleCurrentDesignator())
            {
                return;
            }
            CycleAlternative(-1);
        }

        public static void OnDesignatorSelected(Designator_Build designator)
        {
            if (disableCaching)
                return;

            if (designator?.PlacingDef == null)
            {
                ClearCache();
                return;
            }

            if (designator.PlacingDef.HasAlternatives())
            {
                // This is a main item with alternatives
                cachedMainItem = designator.PlacingDef;
                cachedAlternatives = GetAllAlternativesIncludingOriginal(cachedMainItem);
            }
            else
            {
                // Check if this item is an alternative of a cached main item
                if (cachedMainItem != null && cachedAlternatives != null)
                {
                    if (!cachedAlternatives.Contains(designator.PlacingDef))
                    {
                        // This item is not part of the cached alternatives, clear cache
                        ClearCache();
                    }
                    // If it is part of cached alternatives, keep the cache
                }
                else
                {
                    // No cache and this item doesn't have alternatives
                    ClearCache();
                }
            }
        }

        public static void ClearCache()
        {
            cachedMainItem = null;
            cachedAlternatives = null;
        }

        private static void CycleAlternative(int direction)
        {
            var currentDesignator = Find.DesignatorManager.SelectedDesignator as Designator_Build;
            if (currentDesignator?.PlacingDef == null)
                return;

            if (cachedAlternatives == null || cachedAlternatives.Count <= 1)
                return;

            int currentIndex = FindCurrentDesignatorIndex(cachedAlternatives, currentDesignator);
            if (currentIndex == -1)
                return;

            int nextIndex = GetNextIndex(currentIndex, cachedAlternatives.Count, direction);
            var nextAlternative = cachedAlternatives[nextIndex];

            var newDesignator = new Designator_Build(nextAlternative);

            PreserveStuffIfPossible(currentDesignator, newDesignator);

            // Disable caching during selection to prevent clearing the cache
            disableCaching = true;
            Find.DesignatorManager.Select(newDesignator);
            disableCaching = false;
        }

        private static List<BuildableDef> GetAllAlternativesIncludingOriginal(BuildableDef originalDef)
        {
            var alternatives = AlternativesManager.GetCachedAlternatives(originalDef.GetAlternativeListTag());
            var allAlternatives = new List<BuildableDef>();

            if (alternatives != null && alternatives.Contains(originalDef))
            {
                allAlternatives.AddRange(alternatives);
            }
            else
            {
                allAlternatives.Add(originalDef);
                if (alternatives != null)
                {
                    allAlternatives.AddRange(alternatives);
                }
            }

            return allAlternatives;
        }


        private static int FindCurrentDesignatorIndex(List<BuildableDef> alternatives, Designator_Build currentDesignator)
        {
            for (int i = 0; i < alternatives.Count; i++)
            {
                if (alternatives[i] == currentDesignator.PlacingDef)
                {
                    return i;
                }
            }
            return -1;
        }

        private static int GetNextIndex(int currentIndex, int totalCount, int direction)
        {
            int nextIndex = currentIndex + direction;

            // Wrap around
            if (nextIndex >= totalCount)
                nextIndex = 0;
            else if (nextIndex < 0)
                nextIndex = totalCount - 1;

            return nextIndex;
        }

        private static void PreserveStuffIfPossible(Designator_Build oldDesignator, Designator_Build newDesignator)
        {
            // Check if both designators use stuff
            if (oldDesignator.PlacingDef is ThingDef oldThingDef && oldThingDef.MadeFromStuff &&
                newDesignator.PlacingDef is ThingDef newThingDef && newThingDef.MadeFromStuff &&
                oldDesignator.StuffDef != null)
            {
                // Check if the old stuff can be used for the new item
                if (oldDesignator.StuffDef.stuffProps.CanMake(newThingDef))
                {
                    newDesignator.SetStuffDef(oldDesignator.StuffDef);
                    ReflectionFields.SetWriteStuff(newDesignator, true);
                }
                else
                {
                    // If the old stuff can't be used, try to find a suitable alternative
                    SetBestAvailableStuff(newDesignator, newThingDef);
                }
            }
            else if (newDesignator.PlacingDef is ThingDef newThingDefOnly && newThingDefOnly.MadeFromStuff)
            {
                // If only the new designator uses stuff, set the best available stuff
                SetBestAvailableStuff(newDesignator, newThingDefOnly);
            }
        }

        private static void SetBestAvailableStuff(Designator_Build designator, ThingDef thingDef)
        {
            var availableStuff = GetAvailableStuffFor(thingDef);
            if (availableStuff.Count > 0)
            {
                designator.SetStuffDef(availableStuff[0]);
                ReflectionFields.SetWriteStuff(designator, true);
            }
        }

        private static List<ThingDef> GetAvailableStuffFor(ThingDef thingDef)
        {
            var availableStuff = new List<ThingDef>();

            foreach (var item in from d in Find.CurrentMap.resourceCounter.AllCountedAmounts.Keys
                                 orderby d.stuffProps?.commonality ?? float.PositiveInfinity descending, d.BaseMarketValue
                                 select d)
            {
                if (item.IsStuff && item.stuffProps.CanMake(thingDef) &&
                    (DebugSettings.godMode || Find.CurrentMap.listerThings.ThingsOfDef(item).Count > 0))
                {
                    availableStuff.Add(item);
                }
            }

            return availableStuff;
        }

        public static bool CanCycleCurrentDesignator()
        {
            var currentDesignator = Find.DesignatorManager.SelectedDesignator as Designator_Build;
            if (currentDesignator?.PlacingDef == null)
                return false;

            return cachedAlternatives != null &&
                   cachedAlternatives.Count > 1 &&
                   cachedAlternatives.Contains(currentDesignator.PlacingDef);
        }
    }
}