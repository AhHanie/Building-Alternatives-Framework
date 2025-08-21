using RimWorld;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Verse;

namespace SK_Building_Alternatives_Framework
{
    public static class PatchGenerator
    {
        public static void GeneratePatchFile(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Messages.Message("SettingsMenu.Messages.EmptyInput".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            try
            {
                var matchingDefs = FindMatchingBuildableDefs(searchTerm);

                if (!matchingDefs.Any())
                {
                    Messages.Message($"SettingsMenu.Messages.NoResults".Translate(searchTerm), MessageTypeDefOf.RejectInput);
                    return;
                }

                var xmlContent = GenerateXmlPatch(matchingDefs, searchTerm);

                string filePath = SavePatchFile(xmlContent, searchTerm);

                Messages.Message($"SettingsMenu.Messages.PatchSuccess".Translate(Path.GetFileName(filePath), matchingDefs.Count), MessageTypeDefOf.PositiveEvent);
                Log.Message($"Building Alternatives Framework: Generated patch file at {filePath}");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Building Alternatives Framework: Failed to generate patch file: {ex.Message}");
                Messages.Message($"SettingsMenu.Messages.PatchFail".Translate(ex.Message), MessageTypeDefOf.RejectInput);
            }
        }

        private static List<BuildableDef> FindMatchingBuildableDefs(string searchTerm)
        {
            return DefDatabase<BuildableDef>.AllDefsListForReading
                .Where(def => def.defName.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()))
                .ToList();
        }

        private static string GenerateXmlPatch(List<BuildableDef> defs, string tag)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.AppendLine("<Patch>");
            sb.AppendLine($"    <!-- Auto-generated patch for buildings containing '{tag}' -->");
            sb.AppendLine("    <Operation Class=\"PatchOperationSequence\">");
            sb.AppendLine("        <operations>");

            foreach (var def in defs)
            {
                AddModExtensionOperation(sb, def, tag);

                if (def.designatorDropdown != null)
                {
                    AddRemoveDesignatorDropdownOperation(sb, def);
                }
            }

            sb.AppendLine("        </operations>");
            sb.AppendLine("    </Operation>");
            sb.AppendLine("</Patch>");

            return sb.ToString();
        }

        private static void AddModExtensionOperation(StringBuilder sb, BuildableDef def, string tag)
        {
            sb.AppendLine("            <li Class=\"PatchOperationAdd\">");
            sb.AppendLine($"                <xpath>/Defs/ThingDef[defName=\"{def.defName}\"]</xpath>");
            sb.AppendLine("                <value>");
            sb.AppendLine("                    <modExtensions>");
            sb.AppendLine("                        <li Class=\"SK_Building_Alternatives_Framework.AlternativesModExtension\">");
            sb.AppendLine($"                            <tag>{tag}</tag>");
            sb.AppendLine("                            <isMaster>false</isMaster>");
            sb.AppendLine("                            <hideFromGUI>true</hideFromGUI>");
            sb.AppendLine("                        </li>");
            sb.AppendLine("                    </modExtensions>");
            sb.AppendLine("                </value>");
            sb.AppendLine("            </li>");
        }

        private static void AddRemoveDesignatorDropdownOperation(StringBuilder sb, BuildableDef def)
        {
            sb.AppendLine("            <li Class=\"PatchOperationRemove\">");
            sb.AppendLine($"                <xpath>/Defs/ThingDef[defName=\"{def.defName}\"]/designatorDropdown</xpath>");
            sb.AppendLine("            </li>");
        }

        private static string SavePatchFile(string xmlContent, string searchTerm)
        {
            string desktopPath = GetDesktopPath();
            string fileName = GenerateFileName(searchTerm);
            string filePath = Path.Combine(desktopPath, fileName);

            File.WriteAllText(filePath, xmlContent, Encoding.UTF8);
            return filePath;
        }

        private static string GenerateFileName(string searchTerm)
        {
            string sanitizedTerm = SanitizeFileName(searchTerm);
            return $"BuildingAlternatives_{sanitizedTerm}_{System.DateTime.Now:yyyyMMdd_HHmmss}.xml";
        }

        private static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        private static string GetDesktopPath()
        {
            try
            {
                string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                if (!string.IsNullOrEmpty(desktopPath) && Directory.Exists(desktopPath))
                {
                    return desktopPath;
                }
            }
            catch
            {
            }

            return GetCrossPlatformDesktopPath();
        }

        private static string GetCrossPlatformDesktopPath()
        {
            string userProfile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            return !string.IsNullOrEmpty(userProfile) ? userProfile : Directory.GetCurrentDirectory();
        }

        public static PatchGenerationInfo GetPatchInfo(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new PatchGenerationInfo { IsValid = false, ErrorMessage = "Search term is empty" };
            }

            var matchingDefs = FindMatchingBuildableDefs(searchTerm);

            return new PatchGenerationInfo
            {
                IsValid = matchingDefs.Any(),
                MatchCount = matchingDefs.Count,
                MatchingDefNames = matchingDefs.Select(d => d.defName).ToList(),
                ErrorMessage = matchingDefs.Any() ? null : $"No buildable definitions found containing '{searchTerm}'"
            };
        }
    }

    public class PatchGenerationInfo
    {
        public bool IsValid { get; set; }
        public int MatchCount { get; set; }
        public List<string> MatchingDefNames { get; set; } = new List<string>();
        public string ErrorMessage { get; set; }
    }
}