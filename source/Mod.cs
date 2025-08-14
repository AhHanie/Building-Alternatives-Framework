using HarmonyLib;
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
            instance.PatchAll();
            AlternativesManager.ValidateAlternatives();
            ReflectionFields.Init();
        }
    }
}