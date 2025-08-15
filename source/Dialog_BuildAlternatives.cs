using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SK_Building_Alternatives_Framework
{
    public class Dialog_BuildAlternatives : Window
    {
        private readonly Designator_Build originalDesignator;
        private readonly List<Designator_Build> alternativeDesignators;
        private Vector2 scrollPosition = Vector2.zero;
        private List<bool> lastHoverStates = new List<bool>();
        private bool respectOriginalStuff = false;

        private const float ItemWidth = 120f;
        private const float ItemHeight = 140f;
        private const float ItemSpacing = 10f;
        private const float IconSize = 80f;
        private const float WindowMargin = 20f;
        private const float CycleButtonSize = 24f;
        private const float CycleButtonSpacing = 10f;

        public override Vector2 InitialSize => new Vector2(640f, 480f);

        public Dialog_BuildAlternatives(Designator_Build originalDesignator)
        {
            this.originalDesignator = originalDesignator;

            this.alternativeDesignators = AlternativesManager.CreateAlternativeDesignators(originalDesignator.PlacingDef);

            this.alternativeDesignators.Insert(0, originalDesignator);

            lastHoverStates = new List<bool>(new bool[alternativeDesignators.Count]);

            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            ApplyStuffSettings();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;

            // Get the width of the title text
            string titleText = "WindowAlternative.Title.Label".Translate(originalDesignator.PlacingDef.LabelCap);
            float titleTextWidth = Text.CalcSize(titleText).x;

            // Draw title
            var titleRect = new Rect(0f, 0f, titleTextWidth, 35f);
            Widgets.Label(titleRect, titleText);

            // Draw cycle all button right after the title with spacing
            float buttonsStartX = titleTextWidth + CycleButtonSpacing;
            DrawCycleAllButton(buttonsStartX);

            // Draw toggle similar stuff button next to cycle all button
            DrawToggleSimilarStuffButton(buttonsStartX + CycleButtonSize + CycleButtonSpacing);

            Text.Font = GameFont.Small;
            var contentRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f - CloseButSize.y - 10f);

            int itemsPerRow = Mathf.FloorToInt((contentRect.width - WindowMargin) / (ItemWidth + ItemSpacing));
            if (itemsPerRow < 1) itemsPerRow = 1;

            int totalRows = Mathf.CeilToInt((float)alternativeDesignators.Count / itemsPerRow);
            float totalHeight = totalRows * (ItemHeight + ItemSpacing) + WindowMargin;

            var viewRect = new Rect(0f, 0f, contentRect.width - 16f, totalHeight);

            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);

            for (int i = 0; i < alternativeDesignators.Count; i++)
            {
                var designator = alternativeDesignators[i];
                int row = i / itemsPerRow;
                int col = i % itemsPerRow;

                float x = col * (ItemWidth + ItemSpacing) + WindowMargin / 2;
                float y = row * (ItemHeight + ItemSpacing) + WindowMargin / 2;

                var itemRect = new Rect(x, y, ItemWidth, ItemHeight);
                DrawAlternativeItem(itemRect, designator, i == 0, i);
            }

            Widgets.EndScrollView();
        }

        private void DrawCycleAllButton(float x)
        {
            var cycleButtonRect = new Rect(x, 5f, CycleButtonSize, CycleButtonSize);

            bool hasStuffDesignators = alternativeDesignators.Any(d => d.PlacingDef is ThingDef td && td.MadeFromStuff);

            bool isHovered = Mouse.IsOver(cycleButtonRect);

            // Set icon color based on hover state and whether button is enabled
            Color iconColor;
            if (!hasStuffDesignators)
            {
                iconColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Grayed out when disabled
            }
            else if (isHovered)
            {
                iconColor = new Color(0.3f, 0.6f, 1f, 1f); // Blue when hovered
            }
            else
            {
                iconColor = Color.white; // Normal white
            }

            // Draw icon with color
            GUI.color = iconColor;
            Widgets.DrawTextureFitted(cycleButtonRect, Resources.CycleAllButtonIcon, 1f);
            GUI.color = Color.white;

            // Handle click
            if (Widgets.ButtonInvisible(cycleButtonRect) && hasStuffDesignators)
            {
                CycleAllStuff();
            }

            // Tooltip
            if (isHovered)
            {
                string tooltip = "WindowAlternative.CycleButton.Tooltip".Translate();
                TooltipHandler.TipRegion(cycleButtonRect, tooltip);
            }
        }

        private void DrawToggleSimilarStuffButton(float x)
        {
            var toggleButtonRect = new Rect(x, 5f, CycleButtonSize, CycleButtonSize);

            bool hasStuffDesignators = alternativeDesignators.Any(d => d.PlacingDef is ThingDef td && td.MadeFromStuff);
            bool isHovered = Mouse.IsOver(toggleButtonRect);

            Texture2D iconToUse = respectOriginalStuff
                ? Resources.ToggleSimilarStuffButtonOnStateIcon
                : Resources.ToggleSimilarStuffButtonOffStateIcon;

            Color iconColor;
            if (!hasStuffDesignators)
            {
                iconColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            }
            else if (isHovered)
            {
                iconColor = new Color(0.3f, 0.6f, 1f, 1f);
            }
            else
            {
                iconColor = respectOriginalStuff ? new Color(0.3f, 1f, 0.3f, 1f) : Color.white;
            }

            GUI.color = iconColor;
            Widgets.DrawTextureFitted(toggleButtonRect, iconToUse, 1f);
            GUI.color = Color.white;

            if (Widgets.ButtonInvisible(toggleButtonRect) && hasStuffDesignators)
            {
                ToggleRespectOriginalStuff();
            }

            // Tooltip
            if (isHovered)
            {
                string tooltipKey = respectOriginalStuff
                    ? "WindowAlternative.ToggleButton.Tooltip.On"
                    : "WindowAlternative.ToggleButton.Tooltip.Off";
                string tooltip = tooltipKey.Translate();
                TooltipHandler.TipRegion(toggleButtonRect, tooltip);
            }
        }

        private void ToggleRespectOriginalStuff()
        {
            respectOriginalStuff = !respectOriginalStuff;
            ApplyStuffSettings();
        }

        private void ApplyStuffSettings()
        {
            if (!respectOriginalStuff) return;

            // Apply original stuff to all alternatives that can use it
            for (int i = 1; i < alternativeDesignators.Count; i++) // Skip index 0 (original)
            {
                var designator = alternativeDesignators[i];
                if (designator.PlacingDef is ThingDef thingDef && thingDef.MadeFromStuff && originalDesignator.StuffDef != null)
                {
                    // Check if the original stuff can be used for this alternative
                    if (originalDesignator.StuffDef.stuffProps.CanMake(thingDef))
                    {
                        designator.SetStuffDef(originalDesignator.StuffDef);
                        ReflectionFields.SetWriteStuff(designator, true);
                    }
                }
            }
        }

        private void CycleAllStuff()
        {
            for (int i = 0; i < alternativeDesignators.Count; i++)
            {
                var designator = alternativeDesignators[i];
                if (designator.PlacingDef is ThingDef thingDef && thingDef.MadeFromStuff)
                {
                    var availableStuff = GetAvailableStuffFor(thingDef);
                    if (availableStuff.Count > 1)
                    {
                        int currentIndex = availableStuff.IndexOf(designator.StuffDef);
                        int nextIndex = (currentIndex + 1) % availableStuff.Count;
                        var nextStuff = availableStuff[nextIndex];

                        designator.SetStuffDef(nextStuff);
                        ReflectionFields.SetWriteStuff(designator, true);
                    }
                }
            }
        }

        private List<ThingDef> GetAvailableStuffFor(ThingDef thingDef)
        {
            var availableStuff = new List<ThingDef>();

            // Get only materials that are available on the map, matching the original game logic
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

        private void DrawAlternativeItem(Rect rect, Designator_Build designator, bool isOriginal, int index)
        {
            bool isHovered = Mouse.IsOver(rect);

            // Play hover sound when transitioning from not-hovered to hovered
            if (isHovered && !lastHoverStates[index])
            {
                SoundDefOf.Mouseover_Command.PlayOneShotOnCamera();
            }
            lastHoverStates[index] = isHovered;

            Color backgroundColor;
            Color borderColor;
            int borderThickness = 1;

            if (isHovered)
            {
                backgroundColor = isOriginal
                    ? new Color(0.3f, 0.5f, 0.3f, 0.8f)
                    : new Color(0.25f, 0.25f, 0.35f, 0.4f);
                borderColor = Color.white;
                borderThickness = 2;
            }
            else
            {
                backgroundColor = isOriginal
                    ? new Color(0.2f, 0.4f, 0.2f, 0.5f)
                    : new Color(0.1f, 0.1f, 0.1f, 0.5f);
                borderColor = isOriginal ? Color.green : Color.gray;
            }

            Widgets.DrawBoxSolid(rect, backgroundColor);

            GUI.color = borderColor;
            Widgets.DrawBox(rect, borderThickness);
            GUI.color = Color.white;

            var iconRect = new Rect(rect.x + (rect.width - IconSize) / 2, rect.y + 10f, IconSize, IconSize);

            Color iconColor = designator.IconDrawColor;

            if (isHovered)
            {
                iconColor = Color.Lerp(iconColor, Color.white, 0.2f);
            }

            Widgets.DefIcon(iconRect, designator.PlacingDef, designator.StuffDef, 1f, null, false, iconColor);

            var labelRect = new Rect(rect.x + 5f, iconRect.yMax + 5f, rect.width - 10f, 40f);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;

            string label = designator.LabelCap;
            if (isOriginal)
                label = "WindowAlternative.Item.Original.Label".Translate(label);

            if (isHovered)
            {
                GUI.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            }

            Widgets.Label(labelRect, label);

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // Handle clicks - use different behavior for stuff-based vs non-stuff buildings
            if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown)
            {
                if (designator.PlacingDef is ThingDef thingDef && thingDef.MadeFromStuff)
                {
                    if (Event.current.button == 0)
                    {
                        SelectAlternative(designator);
                        Event.current.Use();
                    }
                    else if (Event.current.button == 1)
                    {
                        HandleRightClick(designator);
                        Event.current.Use();
                    }
                }
                else
                {
                    HandleDesignatorClick(designator);
                    Event.current.Use();
                }
            }

            if (isHovered)
            {
                string tooltip = designator.Desc;
                if (isOriginal)
                    tooltip = "WindowAlternative.Item.Original.Tooltip".Translate(tooltip);

                if (designator.PlacingDef is ThingDef thingDef && thingDef.MadeFromStuff)
                {
                    tooltip += "\n\n" + "WindowAlternative.Item.Original.Tooltip.LeftClick".Translate() + "\n" +
                        "WindowAlternative.Item.Original.Tooltip.RightClick".Translate();
                }

                TooltipHandler.TipRegion(rect, tooltip);
            }
        }

        private void HandleDesignatorClick(Designator_Build designator)
        {
            // Check if this is a stuff-based building that needs material selection
            if (designator.PlacingDef is ThingDef thingDef && thingDef.MadeFromStuff)
            {
                DesignatorManager_Select_Patch.disablePostfix = true;
                // For stuff-based buildings without pre-selected stuff, call ProcessInput to show the float menu
                designator.ProcessInput(Event.current);
                DesignatorManager_Select_Patch.disablePostfix = false;
            }
            else
            {
                // For non-stuff buildings or buildings with pre-selected stuff, select directly
                SelectAlternative(designator);
            }
        }

        private void HandleRightClick(Designator_Build designator)
        {
            DesignatorManager_Select_Patch.disablePostfix = true;
            designator.ProcessInput(Event.current);
            DesignatorManager_Select_Patch.disablePostfix = false;
        }

        private void SelectAlternative(Designator_Build selectedDesignator)
        {
            Find.DesignatorManager.Deselect();
            Find.DesignatorManager.Select(selectedDesignator);
            // Menu is closed from DesignatorManager_Select_Patch patch
        }
    }
}