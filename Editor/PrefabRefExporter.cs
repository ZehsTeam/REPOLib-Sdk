using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace REPOLibSdk.Editor
{
    public static class PrefabRefExporter
    {
        private const string GameFolder = "Assets/REPO/Game/";

        private static readonly Regex ResourcePathRegex = new Regex(@"^\s*resourcePath: (.+?)\r?$", RegexOptions.Multiline);
        private static readonly HashSet<string> YamlExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".asset", ".prefab", ".unity"
        };

        /// <summary>
        /// PrefabRefs to mod assets store the asset's full path, which the game loads with AssetBundle.LoadAsset.
        /// That only works for assets explicitly added to the bundle, so this returns every such PrefabRef target
        /// referenced by the given assets.
        /// </summary>
        public static List<string> FindPrefabRefTargets(IEnumerable<string> assetPaths)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string dependency in AssetDatabase.GetDependencies(assetPaths.ToArray(), true))
            {
                if (!YamlExtensions.Contains(Path.GetExtension(dependency)) || !File.Exists(dependency)) continue;

                foreach (Match match in ResourcePathRegex.Matches(File.ReadAllText(dependency)))
                {
                    string path = match.Groups[1].Value.Trim().Trim('\'', '"');
                    if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) continue;
                    if (path.StartsWith(GameFolder, StringComparison.OrdinalIgnoreCase)) continue;

                    if (!File.Exists(path))
                    {
                        Debug.LogWarning($"PrefabRef in \"{dependency}\" points to \"{path}\", which doesn't exist. Use Prefabs > Update All PrefabRef to fix moved assets.");
                        continue;
                    }

                    result.Add(path);
                }
            }

            return result.ToList();
        }
    }
}
