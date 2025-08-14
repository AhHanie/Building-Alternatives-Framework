using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    public class Dialog_BuildAlternatives : Window
    {
        private readonly Designator_Build originalDesignator;
        private readonly List<Designator_Build> alternativeDesignators;
        private Vector2 scrollPosition = Vector2.zero;

        private const float ItemWidth = 120f;
        private const float ItemHeight = 140f;
        private const float ItemSpacing = 10f;
        private const float IconSize = 80f;
        private const float WindowMargin = 20f;

        public override Vector2 InitialSize => new Vector2(640f, 480f);

        public Dialog_BuildAlternatives(Designator_Build originalDesignator)
        {
            this.originalDesignator = originalDesignator;

            this.alternativeDesignators = AlternativesManager.CreateAlternativeDesignatorsWithStuff(originalDesignator.PlacingDef, originalDesignator.StuffDef);

            // Insert original at the beginning
            this.alternativeDesignators.Insert(0, originalDesignator);

            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            var titleRect = new Rect(0f, 0f, inRect.width, 35f);
            Widgets.Label(titleRect, "WindowAlternative.Title.Label".Translate(originalDesignator.PlacingDef.LabelCap));

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
                DrawAlternativeItem(itemRect, designator, i == 0);
            }

            Widgets.EndScrollView();
        }

        private void DrawAlternativeItem(Rect rect, Designator_Build designator, bool isOriginal)
        {
            bool isHovered = Mouse.IsOver(rect);

            Color backgroundColor;
            Color borderColor;
            int borderThickness = 1;

            if (isHovered)
            {
                backgroundColor = isOriginal
                    ? new Color(0.3f, 0.5f, 0.3f, 0.8f)  // Brighter green for original
                    : new Color(0.25f, 0.25f, 0.35f, 0.4f); // Slight blue tint for alternatives
                borderColor = Color.white;
                borderThickness = 2;
            }
            else
            {
                // Normal colors
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

            try
            {
                Color iconColor = designator.IconDrawColor;

                // Slightly brighten icon on hover
                if (isHovered)
                {
                    iconColor = Color.Lerp(iconColor, Color.white, 0.2f);
                }

                Widgets.DefIcon(iconRect, designator.PlacingDef, designator.StuffDef, 1f, null, false, iconColor);
            }
            catch
            {
                if (designator.icon != null && designator.icon is Texture2D texture)
                {
                    Color iconColor = designator.IconDrawColor;

                    // Slightly brighten icon on hover
                    if (isHovered)
                    {
                        iconColor = Color.Lerp(iconColor, Color.white, 0.2f);
                    }

                    GUI.color = iconColor;
                    GUI.DrawTexture(iconRect, texture);
                    GUI.color = Color.white;
                }
                else
                {
                    Widgets.DrawTextureFitted(iconRect, BaseContent.BadTex, 1f);
                }
            }

            // Label area
            var labelRect = new Rect(rect.x + 5f, iconRect.yMax + 5f, rect.width - 10f, 40f);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;

            string label = designator.LabelCap;
            if (isOriginal)
                label = "WindowAlternative.Item.Original.Label".Translate(label);

            // Brighten text slightly on hover
            if (isHovered)
            {
                GUI.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            }

            Widgets.Label(labelRect, label);

            // Reset text formatting
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // Handle click
            if (Widgets.ButtonInvisible(rect))
            {
                HandleDesignatorClick(designator);
            }

            // Tooltip
            if (isHovered)
            {
                string tooltip = designator.Desc;
                if (isOriginal)
                    tooltip = "WindowAlternative.Item.Original.Tooltip".Translate(tooltip);
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

        private void SelectAlternative(Designator_Build selectedDesignator)
        {
            Find.DesignatorManager.Deselect();
            Find.DesignatorManager.Select(selectedDesignator);
            // Menu is closed from DesignatorManager_Select_Patch patch
        }
    }
}