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
            Widgets.Label(titleRect, $"Choose Alternative for {originalDesignator.PlacingDef.LabelCap}");

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
            Color backgroundColor = isOriginal ? new Color(0.2f, 0.4f, 0.2f, 0.5f) : new Color(0.1f, 0.1f, 0.1f, 0.5f);
            Widgets.DrawBoxSolid(rect, backgroundColor);

            Color borderColor = isOriginal ? Color.green : Color.gray;
            GUI.color = borderColor;
            Widgets.DrawBox(rect, 1);
            GUI.color = Color.white;

            var iconRect = new Rect(rect.x + (rect.width - IconSize) / 2, rect.y + 10f, IconSize, IconSize);

            try
            {
                Color iconColor = designator.IconDrawColor;
                Widgets.DefIcon(iconRect, designator.PlacingDef, designator.StuffDef, 1f, null, false, iconColor);
            }
            catch
            {
                if (designator.icon != null && designator.icon is Texture2D texture)
                {
                    Color iconColor = designator.IconDrawColor;
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
                label += " (Original)";

            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // Handle click
            if (Widgets.ButtonInvisible(rect))
            {
                SelectAlternative(designator);
            }

            // Tooltip
            if (Mouse.IsOver(rect))
            {
                string tooltip = designator.Desc;
                if (isOriginal)
                    tooltip = "Original: " + tooltip;
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }

        private void SelectAlternative(Designator_Build selectedDesignator)
        {
            Find.DesignatorManager.Deselect();
            Find.DesignatorManager.Select(selectedDesignator);
            Close();
        }
    }
}