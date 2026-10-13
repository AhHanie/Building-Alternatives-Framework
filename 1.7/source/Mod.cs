using HarmonyLib;
using UnityEngine;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    public class Mod : Verse.Mod
    {
        public static Harmony instance;

        public Mod(ModContentPack content)
            : base(content)
        {
            GetSettings<Settings>();
            instance = new Harmony("rimworld.sk.buildingalternativesframework");
            instance.PatchAll();
            ReflectionFields.Init();
            LongEventHandler.QueueLongEvent(Init, "Building Alternatives Framework Init", doAsynchronously: false, null);
        }

        public void Init()
        {
            AlternativesManager.Init();
        }

        public override string SettingsCategory()
        {
            return "Building Alternatives Framework";
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            ModSettingsWindow.Draw(rect);
        }
    }
}
