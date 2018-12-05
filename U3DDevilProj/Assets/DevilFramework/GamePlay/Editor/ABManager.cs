using Devil.GamePlay.Assistant;
using Devil.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class ABBuilder
    {
        /*
        * version file

        {
          "version": 1,
          "platform": "Android",
          "ab_list":[
                "manifest": {
                "ver": 1,
                "crc": 3758259188
                },
                "atlas.ab": {
                "ver": 1,
                "crc": 3422187829
                },
                "materials.ab": {
                "ver": 1,
                "crc": 1092797414
                },
                "npc.ab": {
                "ver": 1,
                "crc": 3357090792
                },
                "prefabs.ab": {
                "ver": 1,
                "crc": 3583504635
                },
                "tables.ab": {
                "ver": 1,
                "crc": 4102250506
                }]
        }

        */
        static readonly string abmapFile = "Assets/Scripts/Editor/abmap.json";
        // 导出路径
        static string dataFolder { get { return Path.Combine(Application.dataPath, abrepository); } }

        public struct ABVersion
        {
            public uint crc;
            public string abName;
            public string assetHash;

            public override string ToString()
            {
                return string.Format(@"AssetBundle: {0}
CRC: {1}
Hash: {2}", abName, crc, assetHash);
            }
        }

        public static bool CheckManifest(string folder, string abname, out ABVersion ver)
        {
            ver = new ABVersion();
            ver.abName = abname;
            string file = Path.Combine(folder, abname + ".manifest");
            if (!File.Exists(file))
                return false;
            try
            {
                var stream = File.OpenRead(file);
                var reader = new StreamReader(stream);
                string key;
                bool success = true;
                while ((key = reader.ReadLine()) != null)
                {
                    key = key.TrimStart();
                    if (key.StartsWith("CRC:"))
                    {
                        ver.crc = uint.Parse(key.Substring(4).Trim());
                    }
                    else if (key.StartsWith("Hashes:"))
                    {
                        string assethash;
                        while ((assethash = reader.ReadLine()) != null)
                        {
                            assethash = assethash.TrimStart();
                            if (assethash.StartsWith("AssetFileHash:"))
                                break;
                        }
                        if (assethash == null)
                        {
                            success = false;
                            break;
                        } while ((assethash = reader.ReadLine()) != null)
                        {
                            assethash = assethash.TrimStart();
                            if (assethash.StartsWith("Hash:"))
                                break;
                        }
                        if (assethash == null)
                        {
                            success = false;
                            break;
                        }
                        ver.assetHash = assethash.Substring(5).Trim();
                        break;
                    }
                }
                reader.Close();
                stream.Close();
                return success;
            }
            catch
            {
                return false;
            }
        }

        static JArray abmap;
        static string abrepository;
        public static void UpdateABMap()
        {
            var map = AssetDatabase.LoadAssetAtPath<TextAsset>(abmapFile);
            if (map == null)
            {
                abmap = null;
                abrepository = "../AssetBundle";
            }
            else
            {
                var abcfg = JsonConvert.DeserializeObject<JObject>(map.text);
                abrepository = abcfg.Value<string>("ab_repository");
                abmap = abcfg.Value<JArray>("ab_list");
            }
        }

        public static JObject GetAbMap(string filePath)
        {
            if (abmap == null || abmap.Count == 0)
            {
                UpdateABMap();
            }
            if (abmap == null)
                return null;
            var folder = Path.GetDirectoryName(filePath);
            for (int i = 0; i < abmap.Count; i++)
            {
                var obj = abmap[i] as JObject;
                if (obj.Value<string>("folder") == folder)
                    return obj;
            }
            return null;
        }

        public static void SetAssetBundleNames()
        {
            UpdateABMap();

            if (abmap == null)
            {
                EditorUtility.DisplayDialog("Error", abmap + " 文件不存在", "OK");
                return;
            }
            var title = "Set AssetBundle";
            EditorUtility.DisplayProgressBar(title, "Collectting information.", 0);
            JArray arr = abmap;

            var assets = new List<string>[arr.Count];
            int size = 0;
            for (int i = 0; i < assets.Length; i++)
            {
                var folder = arr[i].Value<string>("folder");
                var recursive = arr[i].Value<bool>("recursive");
                var ab = arr[i].Value<string>("ab");
                var lst = AssetDatabase.FindAssets("", new string[] { folder });
                var list = new List<string>((lst.Length >> 1) + 1);
                assets[i] = list;
                if (lst == null)
                    continue;
                for (int j = 0; j < lst.Length; j++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(lst[j]);
                    if (!recursive && Path.GetDirectoryName(path) != folder)
                        continue;
                    list.Add(path);
                }
                size += list.Count;
            }
            if (size > 0)
            {
                float p = 1f / size;
                float n = 0;
                for (int i = 0; i < assets.Length; i++)
                {
                    var ab = arr[i].Value<string>("ab");
                    title = string.Format("Set AssetBundle [{0}]", ab);
                    foreach (var path in assets[i])
                    {
                        n += 1;
                        EditorUtility.DisplayProgressBar(title, path, n * p);
                        if (!File.Exists(path))
                            continue;
                        var import = AssetImporter.GetAtPath(path);
                        var abname = ab;
                        var folder = Path.GetDirectoryName(path);
                        folder = Path.GetFileName(folder);
                        abname = StringUtil.Replace(abname, "#folder#", folder).ToLower();
                        if (import.assetBundleName != abname)
                        {
                            import.assetBundleName = abname;
                            import.SaveAndReimport();
                        }
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        public static void SetAssetBundleName(string folder, string abName, bool recursive = true)
        {
            string title = string.Format("Set AB name: " + abName);
            string[] assets = AssetDatabase.FindAssets("", new string[] { folder });
            for (int i = 0; i < assets.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[i]);
                if (!recursive && Path.GetDirectoryName(path) != folder)
                    continue;
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (importer.assetBundleName != abName)
                {
                    EditorUtility.DisplayProgressBar(title, path, (float)i / assets.Length);
                    importer.assetBundleName = abName;
                    importer.SaveAndReimport();
                }
            }
            EditorUtility.ClearProgressBar();
        }

        public static void BuildAB(BuildAssetBundleOptions option, BuildTarget platform, string folder)
        {
            UpdateABMap();
            EditorUtility.DisplayProgressBar("Build AB for " + platform, "Preparing...", 0);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var manifest = BuildPipeline.BuildAssetBundles(folder, option, platform);
            var ver = GenerateVersionFile(manifest, folder);
            CopyABFiles(ver, folder);
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Build AB for " + platform,
                StringUtil.Concat("AssetBundle was build to ", folder, "\nVersion: ", ver.Value<int>("version")),
                "OK");
        }

        // 生成 ab 版本文件
        private static JObject GenerateVersionFile(AssetBundleManifest manifest, string folder)
        {
            var file = Path.Combine(folder, "version.json");
            JObject oldversion;
            var txt = ReadText(file);
            oldversion = string.IsNullOrEmpty(txt) ? null : JsonConvert.DeserializeObject<JObject>(txt);
            var abs = oldversion == null ? null : oldversion.Value<JObject>("assetBundle");
            JObject version = new JObject();
            ABVersion ver;

            int abver = 0;
            uint abcrc = 0;
            var dicname = Path.GetFileName(folder);
            if (CheckManifest(folder, dicname, out ver))
            {
                abver = oldversion == null ? 0 : oldversion.Value<int>("version");
                abcrc = oldversion == null ? 0 : oldversion.Value<uint>("crc");
            }
            else
            {
                Debug.LogError("Faild to generate version file.\ncan't parse Manifest.manifest");
                return null;
            }
            var newabs = new JObject();
            bool newversion = false;
            foreach (var ab in manifest.GetAllAssetBundles())
            {
                if (CheckManifest(folder, ab, out ver))
                {
                    var man = abs == null ? null : abs.Value<JObject>(ab);
                    int v = man == null ? 0 : man.Value<int>("ver");
                    uint crc = man == null ? 0 : man.Value<uint>("crc");
                    if (ver.crc != crc)
                    {
                        crc = ver.crc;
                        v++;
                        newversion = true;
                    }
                    man = new JObject();
                    man["ver"] = v;
                    man["crc"] = crc;
                    newabs[ab] = man;
                }
                else
                {
                    Debug.LogErrorFormat("Faild to generate version file.\ncan't parse {0}.manifest", ab);
                    return null;
                }
            }
            if (newversion)
                abver++;
            version["version"] = abver;
            version["crc"] = abcrc;
            version["category"] = dicname;
            version["assetBundle"] = newabs;
            WriteText(file, version.ToString());
            return version;
        }

        // 迁移 ab 文件
        private static void CopyABFiles(JObject version, string folder)
        {
            var dicname = Path.GetFileName(folder);
            var destination = Path.Combine(dataFolder, dicname);
            destination = Path.Combine(destination, "ab_ver." + version.Value<int>("version"));
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            else
                Cleanup(destination);
            string src;
            string dest;
            List<string> files = new List<string>();
            var cat = version.Value<string>("category");
            files.Add(cat);
            foreach (var ab in version.Value<JObject>("assetBundle").Properties())
            {
                files.Add(ab.Name);
            }
            float len = files.Count;
            for (int i = 0; i < files.Count; i++)
            {
                var ab = files[i];
                EditorUtility.DisplayProgressBar("Copy AB Files", "Moving " + ab, i / len);
                src = Path.Combine(folder, ab);
                dest = Path.Combine(destination, ab);
                File.Copy(src, dest);
                src = Path.Combine(folder, ab + ".manifest");
                dest = Path.Combine(destination, ab + ".manifest");
                File.Copy(src, dest);
            }
            src = Path.Combine(folder, "version.json");
            dest = Path.Combine(destination, "version.json");
            File.Copy(src, dest);
        }

        public static void Cleanup(string folder, params string[] exclude)
        {
            if (!Directory.Exists(folder))
                return;
            int exc = exclude == null ? 0 : exclude.Length;
            DirectoryInfo dir = new DirectoryInfo(folder);
            foreach (var file in dir.GetFiles())
            {
                bool skip = false;
                for (int i = 0; i < exc; i++)
                {
                    if (Regex.IsMatch(file.Name, exclude[i]))
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip)
                    continue;
                File.Delete(file.FullName);
                Debug.Log("Delete file: " + file.FullName);
            }
        }

        public static void Cleanup(RuntimePlatform platform)
        {
            var folder = LocalFileUtil.GetABPath(platform);
            Cleanup(folder, "^version.json$");
            AssetDatabase.Refresh();
        }

        public static void Cleanup(BuildTarget platform)
        {
            switch (platform)
            {
                case BuildTarget.Android:
                    Cleanup(RuntimePlatform.Android);
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    Cleanup(RuntimePlatform.WindowsPlayer);
                    break;
                default:
                    break;
            }
        }

        public static void BuildAB(BuildTarget platform)
        {
            switch (platform)
            {
                case BuildTarget.Android:
                    BuildAB(BuildAssetBundleOptions.None, BuildTarget.Android, LocalFileUtil.GetABPath(RuntimePlatform.Android));
                    break;
                case BuildTarget.StandaloneWindows:
                    BuildAB(BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows, LocalFileUtil.GetABPath(RuntimePlatform.WindowsPlayer));
                    break;
                case BuildTarget.StandaloneWindows64:
                    BuildAB(BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64, LocalFileUtil.GetABPath(RuntimePlatform.WindowsPlayer));
                    break;
                default:
                    EditorUtility.DisplayDialog("Error", "暂不支持该平台的资源打包", "OK");
                    break;
            }
        }

        static string ReadText(string file)
        {
            if (!File.Exists(file))
                return "";
            var bytes = File.ReadAllBytes(file);
            return Encoding.UTF8.GetString(bytes);
        }

        static void WriteText(string file, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            File.WriteAllBytes(file, bytes);
        }
    }
}