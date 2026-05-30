using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using System.Reflection;
using MasterLoaderConfig;
using System.Linq;
using System.IO;


namespace MasterLoader.Core
{
    /// <summary>
    /// スプレッドシートからマスタを取得して自動生成したScriptableObjectに流し込むクラス
    /// </summary>
    [InitializeOnLoad]
    public class MasterLoader : Editor
    {
        private static IConfig _config;
        public static IConfig ConfigData => _config ?? UpdateConfig();
        private static MasterLoaderScheduler _scheduler = new MasterLoaderScheduler();

        /// <summary>
        /// アプリ起動時に自動でScriptableObjectを更新する
        /// </summary>
        static MasterLoader()
        {
            if (EditorApplication.timeSinceStartup > 100)
            {
                return;
            }

            UpdateConfig();
        }

        /// <summary>
        /// スプレッドシートからマスタを取得する
        /// </summary>
        /// <param name="masterName">取得するマスタ名</param>
        /// <returns>エラー時の警告またはロードしたマスタ名</returns>
        private static bool LoadMaster(string masterName)
        {
            var url = StringStore.GetUrl
            (
                StringStore.FUNCTION_MASTER,
                new string[]
                {
                    $"{StringStore.SHEET_NAME}{masterName}",
                    $"{StringStore.URL}{ConfigData.WindowConfig.SheetUrl}"
                }
            );
            //Debug.Log(url);
            var json = LoadMasterCore(url);

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            return OnLoadedMaster(json);
        }

        private static bool LoadAllMaster()
        {
            var url = StringStore.GetUrl
            (
                StringStore.FUNCTION_LOAD_ALL,
                new string[]
                {
                    $"{StringStore.URL}{ConfigData.WindowConfig.SheetUrl}"
                }
            );

            var json = LoadMasterCore(url);

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                var result = JsonUtility.FromJson<MasterDataRawAll>(json);
                var resultValues = result.Values;
                var _isValid = true;

                foreach (var obj in resultValues)
                {
                    if (obj.Alerts?.Length > 0)
                    {
                        for (var i = 0; i < obj.Alerts.Length; i++)
                        {
                            Debug.LogError($"MasterLoader Info: {obj.Alerts[i]}");
                        }
                        _isValid = false;
                    }
                }
                if (!_isValid)
                {
                    throw new Exception(message: "");
                }
                ConfigData.LoadingConfig.SetLoadedResultList(resultValues.ToList());
                ConfigData.LoadingConfig.SetEnumValueList(result.Enums);
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(e?.Message))
                {
                    Debug.LogException(e);
                }
                Debug.Log(json);

                return false;
            }
            return true;
        }

        private static string LoadMasterCore(string url)
        {
            return WebRequest.SendWebRequest
            (
                url,
                "Getting Master Data..."
            );
        }

        private static bool OnLoadedMaster(string json)
        {
            var result = JsonUtility.FromJson<MasterDataRaw>(json);

            if (HasAlerts(result))
            {
                LogAlerts(result.Alerts);
                return false;
            }
            ConfigData.LoadingConfig.AddLoadedResult(result);
            ConfigData.LoadingConfig.SetEnumValueList(result.Enums);
            return true;
        }

        private static bool HasAlerts(MasterDataRaw result)
        {
            return result.Alerts?.Length > 0;
        }

        private static void LogAlerts(string[] alerts)
        {
            for (var i = 0; i < alerts.Length; i++)
            {
                Debug.LogError($"MasterLoader Info: {alerts[i]}");
            }
        }

        private static bool GenerateCode(IConfig config, MasterDataRaw code)
        {
            return CodeGenerator.Generate(config, code);
        }

        public static bool CreateMaster(string masterName)
        {
            if (!LoadMaster(masterName)) return false;
            return CreateMaster_(deleteObsolete: false);
        }

        public static bool CreateAll()
        {
            if (!LoadAllMaster()) return false;
            return CreateMaster_(deleteObsolete: true);
        }

        private static bool CreateMaster_(bool deleteObsolete)
        {
            _scheduler.SetIsWaitingCompiled(true);
            var list = ConfigData.LoadingConfig.LoadedResultList;
            var currentNames = new HashSet<string>(list.Select(r => r.Name));

            var hadDeletions = deleteObsolete && (DeleteObsoleteFiles(currentNames) | DeleteObsoleteEnumFiles());

            var needsRecompile = hadDeletions;
            foreach (var loadedResult in list)
            {
                if (!IsStructureChanged(loadedResult)) continue;
                needsRecompile = true;
                if (!GenerateCode(ConfigData, loadedResult)) return false;
                ConfigData.LoadingConfig.SetMasterSignature(loadedResult.Name, ComputeSignature(loadedResult));
            }

            if (needsRecompile)
            {
                ConfigData.MasterBody.SetMasters(list.Select(r => r.Name).ToArray());
                if (!CodeGenerator.GenerateInjector(ConfigData)) return false;
            }

            if (deleteObsolete)
            {
                ConfigData.LoadingConfig.SetStoredEnumNames(
                    ConfigData.LoadingConfig.EnumValueList.Select(e => CodeGenerator.ToPascalCase(e.Name)));
            }

            SaveConfig();

            if (!needsRecompile)
            {
                _scheduler.SetIsWaitingCompiled(false);
                CreateMasterScriptableObjects(ConfigData);
                RefreshLoadedResult();
                RefreshEnumValueList();
                _scheduler.OnStartWaitingTicked(CreateFacade);
                return true;
            }

            if (!EditorApplication.isCompiling)
            {
                OnCreateMasterScriptableObjects();
                return true;
            }
            Debug.Log("MasterLoader Info: Now Compiling...");
            return true;
        }

        private static bool IsStructureChanged(MasterDataRaw result)
        {
            var csPath = CodeGenerator.GetGeneratedScriptsPath(ConfigData);
            if (!File.Exists($"{csPath}{CodeGenerator.ToPascalCase(result.Name)}{CodeGenerator.CS}")) return true;
            return ComputeSignature(result) != ConfigData.LoadingConfig.GetMasterSignature(result.Name);
        }

        private static string ComputeSignature(MasterDataRaw result)
        {
            if (result.Parameter == null || result.Type == null) return string.Empty;
            return string.Join(",", result.Parameter.Zip(result.Type, (p, t) => $"{t}:{p}"));
        }

        private static bool DeleteObsoleteFiles(HashSet<string> currentNames)
        {
            var obsolete = ConfigData.LoadingConfig.StoredMasterNames
                .Where(n => !currentNames.Contains(n))
                .ToList();
            if (obsolete.Count == 0) return false;

            var csPath = CodeGenerator.GetGeneratedScriptsPath(ConfigData);
            var masterFolder = ConfigData.WindowConfig.MasterPathFolder != null
                ? AssetDatabase.GetAssetPath(ConfigData.WindowConfig.MasterPathFolder)
                : string.Empty;

            foreach (var name in obsolete)
            {
                var pascalName = CodeGenerator.ToPascalCase(name);
                DeleteFileIfExists($"{csPath}{pascalName}{CodeGenerator.CS}");
                DeleteFileIfExists($"{csPath}{pascalName}{StringStore.MASTER}{CodeGenerator.CS}");
                if (!string.IsNullOrEmpty(masterFolder))
                    DeleteFileIfExists($"{masterFolder}/{pascalName}.asset");
                ConfigData.LoadingConfig.RemoveMasterSignature(name);
                Debug.Log($"MasterLoader Info: Deleted obsolete master: {name}");
            }

            return true;
        }

        private static bool DeleteObsoleteEnumFiles()
        {
            var currentEnumNames = new HashSet<string>(
                ConfigData.LoadingConfig.EnumValueList.Select(e => CodeGenerator.ToPascalCase(e.Name)));

            var obsolete = ConfigData.LoadingConfig.StoredEnumNames
                .Where(n => !currentEnumNames.Contains(n))
                .ToList();
            if (obsolete.Count == 0) return false;

            var csPath = CodeGenerator.GetGeneratedScriptsPath(ConfigData);
            foreach (var name in obsolete)
            {
                DeleteFileIfExists($"{csPath}{name}{CodeGenerator.CS}");
                Debug.Log($"MasterLoader Info: Deleted obsolete enum: {name}");
            }
            return true;
        }

        private static void DeleteFileIfExists(string path)
        {
            if (!File.Exists(path)) return;
            File.Delete(path);
            var meta = path + ".meta";
            if (File.Exists(meta)) File.Delete(meta);
        }

        [DidReloadScripts]
        private static void OnCreateMasterScriptableObjects()
        {

            UpdateConfig();
            if (ConfigData.LoadingConfig.LoadedResultList.Count < 1)
            {
                return;
            }
            _scheduler.SetIsWaitingCompiled(false);
            SaveConfig();

            CreateMasterScriptableObjects(ConfigData);

            RefreshLoadedResult();
            RefreshEnumValueList();

            _scheduler.OnStartWaitingTicked(CreateFacade);
        }

        private static void CreateMasterScriptableObjects(IConfig ConfigData)
        {
            foreach (var target in ConfigData.LoadingConfig.LoadedResultList)
            {
                var assetPath = $"{AssetDatabase.GetAssetPath(ConfigData.WindowConfig.MasterPathFolder)}/{target.Name}.asset";
                var soMaster = Utility.Utility.GetAssetPathObject(assetPath, target.Name);
                if (soMaster == null)
                {
                    Debug.Log("MasterLoader Info: Creating MasterData...");
                    soMaster = CreateInstance($"{target.Name}{StringStore.MASTER}");
                    AssetDatabase.CreateAsset(soMaster, assetPath);
                }
                var method = soMaster.GetType().GetMethod(StringStore.METHOD_SET_DATA);

                method.Invoke(soMaster, new object[] { target.ValueList });
                EditorUtility.SetDirty(soMaster);
                AssetDatabase.SaveAssetIfDirty(soMaster);
                Debug.Log($"MasterLoader Info: {target.Name} Completely Created!");
            }
        }

        private static void RefreshLoadedResult()
        {
            ConfigData.LoadingConfig.ClearLoadedResultList();
        }

        private static void RefreshEnumValueList()
        {
            ConfigData.LoadingConfig.ClearEnumValueList();
        }

        private static void CreateFacade()
        {
            var masterPath = ConfigData.WindowConfig.MasterPathFolder != null
                ? AssetDatabase.GetAssetPath(ConfigData.WindowConfig.MasterPathFolder)
                : "Assets/PlugIns/MasterLoader/Generated/Scripts";
            var prefabPath = $"{masterPath}/Facade";
            var facade = AssetDatabase.LoadMainAssetAtPath($"{prefabPath}/{StringStore.MASTER_FACADE_NAME}.prefab") as GameObject;
            if (facade == null)
            {
                if (!Directory.Exists($"{prefabPath}/"))
                {
                    Directory.CreateDirectory($"{prefabPath}/");
                }
                var facadeType = GetMasterFacadeType();
                if (facadeType == null)
                {
                    Debug.LogError("MasterLoaderInfo: MasterFacade type not found.");
                    MasterLoaderScheduler.OnEndTickAction();
                    return;
                }
                var go = new GameObject();
                go.AddComponent(facadeType);
                facade = PrefabUtility.SaveAsPrefabAsset(go, $"{prefabPath}/{StringStore.MASTER_FACADE_NAME}.prefab");
            }
            var component = facade.GetComponent(GetMasterFacadeType()) as MonoBehaviour;
            try
            {
                if (component == null) throw new NullReferenceException("MasterFacade component is null.");
                component.GetType().GetMethod("SetMaster")?.Invoke(component, null);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"MasterLoaderInfo: MasterFacade was null. {e.Message}");
                MasterLoaderScheduler.OnEndTickAction();
                return;
            }
            EditorUtility.SetDirty(component);
            AssetDatabase.SaveAssetIfDirty(component);
        }

        public static void JumpCreatedSheet(string sheetUrl)
        {
            Application.OpenURL(sheetUrl);
        }

        public static bool GetSheets(string sheetUrl)
        {
            var url = StringStore.GetUrl
            (
                StringStore.FUNCTION_GET_SHEETS,
                new string[]
                {
                    $"{StringStore.URL}{sheetUrl}"
                }
            );

            var json = WebRequest.SendWebRequest
            (
                url,
                "Fetching Master Name..."
            );

            var result = JsonUtility.FromJson<Result>(json);
            if (result.Alerts.Length > 0)
            {
                Debug.LogError($"MasterLoader Info: {result.Alerts.Length} sheet problems detected.");
                for (var i = 0; i < result.Alerts.Length; i++)
                {
                    Debug.LogAssertion(result.Alerts[i]);
                }
                return false;
            }
            else if (result.Masters.Length < 1)
            {
                Debug.LogError("MasterLoader Info: Loadable master is nothing. Please fix sheet problems.");
                return false;
            }
            else
            {
                Debug.Log("MasterLoader Info: Getting Sheet has completed.");
                ConfigData.MasterBody.SetMasters(result.Masters);
                SaveConfig();
                return true;
            }
        }

        private static Type GetMasterFacadeType()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType($"MasterLoader.{StringStore.MASTER_FACADE_NAME}");
                if (type != null) return type;
            }
            return null;
        }

        public static void SaveConfig()
        {
            var config = GetConfigObject();
            EditorUtility.SetDirty(config);
        }

        private static ConfigObject GetConfigObject()
        {
            var obj = Resources.Load<ConfigObject>(StringStore.CONFIG_PATH);
            if (obj == null)
            {
                var asset = CreateInstance<ConfigObject>();
                AssetDatabase.CreateAsset(asset, StringStore.CONFIG_PATH);
                obj = asset;
            }
            obj.hideFlags = HideFlags.NotEditable;
            return obj;
        }

        public static IConfig LoadConfig()
        {
            return GetConfigObject().Config;
        }

        public static IConfig UpdateConfig()
        {
            _config = LoadConfig();
            return ConfigData;
        }

        public static void RefreshConfig()
        {
            GetConfigObject().RefreshConfig();
            _config = GetConfigObject().Config;
        }
    }
}