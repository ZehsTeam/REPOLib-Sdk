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
        // the editor-only "prefab" field of a PrefabRef, which is always followed by "prefabName"
        private static readonly Regex PrefabRefTargetRegex = new Regex(@"^[ \t]*(?:- )?prefab: \{fileID: -?\d+, guid: ([0-9a-f]{32}), type: \d+\}\r?\n[ \t]*prefabName:", RegexOptions.Multiline);
        private static readonly Regex GuidRegex = new Regex(@"guid: ([0-9a-f]{32})");
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

        /// <summary>
        /// PrefabRefs to base-game assets load the game's own copy in-game, but their editor-only prefab field still
        /// makes Unity copy the asset into the bundle. Returns the base-game assets the given assets only reach through
        /// PrefabRefs, so they can be built into a separate bundle that isn't shipped.
        /// Base-game assets that are also referenced directly are left out, since the bundle needs its own copy of those.
        /// </summary>
        public static List<string> FindPrefabRefOnlyGameAssets(IEnumerable<string> assetPaths)
        {
            var reachable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var prefabRefTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>(assetPaths);

            while (queue.Count > 0)
            {
                string path = queue.Dequeue();
                if (!reachable.Add(path)) continue;

                foreach (var (dependency, viaPrefabRef) in GetDirectDependencies(path))
                {
                    if (viaPrefabRef && dependency.StartsWith(GameFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        prefabRefTargets.Add(dependency);
                        continue;
                    }

                    if (!reachable.Contains(dependency)) queue.Enqueue(dependency);
                }
            }

            var result = prefabRefTargets.Where(x => !reachable.Contains(x)).ToList();
            if (result.Count > 0)
            {
                Debug.Log($"Leaving out {result.Count} base-game asset(s) only referenced by PrefabRefs, the game loads its own copy:\n{string.Join("\n", result)}");
            }

            return result;
        }

        private static IEnumerable<(string Path, bool ViaPrefabRef)> GetDirectDependencies(string path)
        {
            if (!YamlExtensions.Contains(Path.GetExtension(path)) || !File.Exists(path))
            {
                foreach (string dependency in AssetDatabase.GetDependencies(path, false))
                {
                    if (dependency != path) yield return (dependency, false);
                }
                yield break;
            }

            string text = File.ReadAllText(path);
            var prefabRefGuidIndices = new HashSet<int>(PrefabRefTargetRegex.Matches(text)
                .Cast<Match>()
                .Select(x => x.Groups[1].Index));

            foreach (Match match in GuidRegex.Matches(text))
            {
                string dependency = AssetDatabase.GUIDToAssetPath(match.Groups[1].Value);
                if (string.IsNullOrEmpty(dependency) || dependency == path) continue;
                if (!dependency.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) && !dependency.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase)) continue;

                yield return (dependency, prefabRefGuidIndices.Contains(match.Groups[1].Index));
            }
        }
    }
}
