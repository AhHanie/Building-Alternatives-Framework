using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace SK_Building_Alternatives_Framework
{
    public class AlternativesModExtension : DefModExtension
    {
        public List<BuildableDef> alternatives;
        public string uiIconPath;
        public string hoverUiIconPath;
        private Texture2D _uiIcon;
        private Texture2D _hoverUiIcon;
        private bool _iconsInitialized = false;

        public AlternativesModExtension()
        {
            alternatives = null;
            uiIconPath = null;
            hoverUiIconPath = null;
        }

        public Texture2D UiIcon
        {
            get
            {
                if (!_iconsInitialized)
                    InitializeIcons();
                return _uiIcon;
            }
        }

        public Texture2D HoverUiIcon
        {
            get
            {
                if (!_iconsInitialized)
                    InitializeIcons();
                return _hoverUiIcon;
            }
        }

        private void InitializeIcons()
        {
            if (_iconsInitialized) return;

            if (uiIconPath != null)
            {
                _uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
                _hoverUiIcon = _uiIcon;
            }
            if (hoverUiIconPath != null)
            {
                _hoverUiIcon = ContentFinder<Texture2D>.Get(hoverUiIconPath);
            }

            _iconsInitialized = true;
        }
    }
}