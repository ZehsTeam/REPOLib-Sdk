using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
#if REPOLIB_SDK_LOCALIZATION
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
#endif

namespace REPOLibSdk.Editor
{
    public static class LocalizationExporter
    {
        private const string GameFolder = "Assets/REPO/Game/";

        private static readonly Regex TableReferenceRegex = new Regex(@"m_TableCollectionName: (.+?)\r?$", RegexOptions.Multiline);
        private static readonly HashSet<string> YamlExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".asset", ".prefab", ".unity"
        };

        public static List<string> FindModStringTables(IEnumerable<string> assetPaths)
        {
            var result = new List<string>();
#if REPOLIB_SDK_LOCALIZATION
            var tableReferences = new HashSet<string>();

            foreach (string dependency in AssetDatabase.GetDependencies(assetPaths.ToArray(), true))
            {
                if (!YamlExtensions.Contains(Path.GetExtension(dependency)) || !File.Exists(dependency)) continue;

                foreach (Match match in TableReferenceRegex.Matches(File.ReadAllText(dependency)))
                {
                    string value = match.Groups[1].Value.Trim().Trim('\'', '"');
                    if (!string.IsNullOrEmpty(value)) tableReferences.Add(value);
                }
            }

            var collections = new HashSet<StringTableCollection>();

            foreach (string value in tableReferences)
            {
                TableReference tableReference = value.StartsWith("GUID:") && Guid.TryParse(value.Substring(5), out var guid)
                    ? guid
                    : value;

                var collection = LocalizationEditorSettings.GetStringTableCollection(tableReference);
                if (collection == null)
                {
                    Debug.LogWarning($"Could not find the string table collection \"{value}\" referenced by the mod's content.");
                    continue;
                }

                if (AssetDatabase.GetAssetPath(collection.SharedData).StartsWith(GameFolder)) continue;

                collections.Add(collection);
            }

            foreach (var collection in collections)
            {
                result.Add(AssetDatabase.GetAssetPath(collection.SharedData));
                result.AddRange(collection.StringTables
                    .Where(table => table != null)
                    .Select(AssetDatabase.GetAssetPath));

                Debug.Log($"Including string table collection \"{collection.TableCollectionName}\" ({collection.StringTables.Count} locales)");
            }
#endif
            return result;
        }
    }
}
