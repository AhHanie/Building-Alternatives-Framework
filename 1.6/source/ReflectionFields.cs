using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    public static class ReflectionFields
    {
        public static FieldInfo writeStuffField;

        public static void SetWriteStuff(Designator_Build designator, bool value)
        {
            if (writeStuffField != null)
            {
                writeStuffField.SetValue(designator, value);
            }
        }
        public static bool GetWriteStuff(Designator_Build designator)
        {
            if (writeStuffField != null)
            {
                return (bool)writeStuffField.GetValue(designator);
            }
            return false;
        }

        public static void Init()
        {
            writeStuffField = AccessTools.Field(typeof(Designator_Build), "writeStuff");

            if (writeStuffField == null)
            {
                Log.Error("[Building Alternatives Framework] Failed to find writeStuff field in Designator_Build");
            }
        }
    }
}