using Newtonsoft.Json;
using REPOLib.Objects.Sdk;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.SharpZipLib.Utils;
using UnityEditor;
using UnityEngine;

namespace REPOLibSdk.Editor
{
    public static class PackageExporter
    {
        public static IEnumerable<(string Path, bool IsDependency)> FindContents(Mod mod)
        {
            string modPath = AssetDatabase.GetAssetPath(mod);
            string directory = Path.GetDirectoryName(modPath);
            string[] content = AssetDatabase.FindAssets("t:Content", new[] { directory })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
            
            foreach (string asset in content)
            {
                yield return (asset, false);
            }

            string[] dependencies = AssetDatabase.GetDependencies(content);
            foreach (string dependency in dependencies)
            {
                yield return (dependency, true);
            }
        }
        
        public static void ExportPackage(Mod mod, string outputPath)
        {
            CreateDirectoryIfNotExists(outputPath);
            
            string[] assetPaths = FindContents(mod)
                .Where(tuple => !tuple.IsDependency) // BuildAssetBundles collects dependencies by itself
                .Select(tuple => tuple.Path)
                .Append(AssetDatabase.GetAssetPath(mod)) // include the Mod
                .ToArray();
            string bundlePath = Path.GetFullPath(Path.Combine(outputPath, "bundle"));
            bundlePath = BuildAssetBundle(mod, bundlePath, assetPaths);

            string packagePath = Path.Combine(outputPath, "package");
            DeleteAndRecreateDirectory(packagePath);
            
            string finalBundlePath = Path.Combine(packagePath, mod.Name + ".repobundle");
            File.Copy(bundlePath, finalBundlePath);

            WriteExtraFiles(mod, packagePath);
            WriteReadme(mod, packagePath);
            WriteIcon(mod, packagePath);
            WriteManifest(mod, packagePath);
            
            string zipPath = Path.Combine(outputPath, $"{mod.Identifier}.zip");
            ZipUtility.CompressFolderToZip(zipPath, null, packagePath);
            
            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(zipPath);
        }

        private static void WriteExtraFiles(Mod mod, string packagePath)
        {
            var settings = ModExportSettingsSource.GetSettings(mod);

            foreach (var file in settings.ExtraFiles)
            {
                if (file == null) continue;
                
                string fromPath = AssetDatabase.GetAssetPath(file);
                string toPath = Path.Combine(packagePath, Path.GetFileName(fromPath));
                
                File.Copy(fromPath, toPath);
            }
        }
        
        private static void WriteReadme(Mod mod, string packagePath)
        {
            if (mod.Readme == null)
            {
                Debug.LogWarning($"No Readme set for mod \"{mod.Name}\"");
                return;
            }
            string fromPath = AssetDatabase.GetAssetPath(mod.Readme);
            if (string.IsNullOrWhiteSpace(fromPath) || !File.Exists(fromPath))
            {
                Debug.LogError($"Invalid readme path \"{fromPath}\".");
                return;
            }
            File.Copy(fromPath, Path.Combine(packagePath, "README.md"));
        }

        private static void WriteIcon(Mod mod, string packagePath)
        {
            if (mod.Icon == null)
            {
                Debug.LogWarning($"No Icon set for mod \"{mod.Name}\"");
                return;
            }
            string fromPath = AssetDatabase.GetAssetPath(mod.Icon);
            if (string.IsNullOrWhiteSpace(fromPath) || !File.Exists(fromPath))
            {
                Debug.LogError($"Invalid icon path \"{fromPath}\".");
                return;
            }
            File.Copy(fromPath, Path.Combine(packagePath, "icon.png"));
        }
        
        private static void WriteManifest(Mod mod, string packagePath)
        {
            var manifest = new ThunderstoreManifest
            {
                Name = mod.Name,
                Description = mod.Description,
                Version = mod.Version,
                Dependencies = mod.Dependencies.ToArray(),
                WebsiteUrl = mod.WebsiteUrl
            };
            
            string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
            File.WriteAllText(Path.Combine(packagePath, "manifest.json"), json);
        }

        private static string BuildAssetBundle(Mod mod, string path, string[] assetNames)
        {
            Debug.Log($"Building bundle to {path}");
            
            DeleteAndRecreateDirectory(path);
            
            BuildPipeline.BuildAssetBundles(new BuildAssetBundlesParameters
            {
                outputPath = path,
                bundleDefinitions = new[]
                {
                    new AssetBundleBuild
                    {
                        assetBundleName = mod.Name,
                        assetNames = assetNames
                    }
                },
                options = BuildAssetBundleOptions.UseContentHash,
                targetPlatform = BuildTarget.StandaloneWindows64
            });
            
            return Path.Combine(path, mod.Name.ToLower());
        }

        private static void CreateDirectoryIfNotExists(string path)
        {
            if (Directory.Exists(path)) return;
            
            Directory.CreateDirectory(path);
        }

        private static void DeleteAndRecreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            
            Directory.CreateDirectory(path);
        }
        
        private class ThunderstoreManifest
        {
            [JsonProperty("name")]
            public string Name;
            
            [JsonProperty("description")]
            public string Description;
            
            [JsonProperty("version_number")]
            public string Version;
            
            [JsonProperty("dependencies")]
            public string[] Dependencies;
            
            [JsonProperty("website_url")]
            public string WebsiteUrl;
        }
    }
}
