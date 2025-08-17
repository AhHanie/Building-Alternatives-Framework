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
            instance = new Harmony("rimworld.sk.buildingalternativesframework");
            LongEventHandler.QueueLongEvent(Init, "Building Alternatives Framework Init", doAsynchronously: true, null);
        }

        public void Init()
        {
            GetSettings<Settings>();
            instance.PatchAll();
            ReflectionFields.Init();
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