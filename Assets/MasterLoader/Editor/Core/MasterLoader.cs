using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
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
        public static Config ConfigData;

        public const string UTILITY_PATH = "Assets/MasterLoader/Scripts/Utility/";
        private const string _CONFIG_PATH = "Confing";
        private const string _INSTALLER_PATH = "Assets/MasterLoader/Prefab";
        public const string MASTER = "Master";
        private const string _API_URL =
            "https://script.google.com/macros/s/AKfycbwjx-pDNW89Hzi0SV_hGHzVhOMt2_v6K6r4S9Txd_JTuiilzxjHOqjwo3IYcm7PnVWGZQ/exec?";
        private const string _URL = "url=";
        /// <summary>
        /// doGet時の独自変数
        /// 読み込むシートの判断用
        /// </summary>
        private const string _SHEET_NAME = "sheetName=";

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
            var url = $"{_API_URL}function=LoadMaster&{_SHEET_NAME}{masterName}&{_URL}{ConfigData.SheetUrl}";
            //Debug.Log(url);
            var json = LoadMasterCore(url);

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            var result = JsonUtility.FromJson<Base>(json);

            if (result.Alerts?.Length > 0)
            {
                for (var i = 0; i < result.Alerts.Length; i++)
                {
                    Debug.LogError($"MasterLoader Info: {result.Alerts[i]}");
                }
                return false;
            }
            var list = ConfigData.LoadedResultList;
            if (list.Count > 0)
            {
                ConfigData.LoadedResultList.Clear();
                //Debug.LogWarning($"{nameof(list)} has old list, cleared.");
            }
            list.Add(result);
            return true;
        }

        private static bool LoadAllMaster()
        {
            var url = $"{_API_URL}function=LoadAll&{_URL}{ConfigData.SheetUrl}";

            var json = LoadMasterCore(url);

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            var result = JsonUtility.FromJson<BaseAll>(json).Values;
            var _isValid = true;

            foreach(var obj in result)
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
                return false;
            }
            ConfigData.LoadedResultList = result.ToList();
            return true;
        }

        private static string LoadMasterCore(string url)
        {
            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    var req = request.SendWebRequest();

                    while (req.progress < 1)
                    {
                        EditorUtility.DisplayProgressBar("Getting Master Data...", $"{request.downloadProgress * 100}%", request.downloadProgress);
                    }
#if UNITY_2020_1_OR_NEWER
                    if(request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
#else
                    if (request.isHttpError || request.isNetworkError)
#endif
                    {
                        Debug.LogError("MasterLoader Info: NetWork Error.");
                        throw new Exception(request.error);
                    }

                    var json = request.downloadHandler.text;
                    if (json.Contains("<!DOCTYPE html>"))
                    {
                        throw new Exception(json);
                    }
                    return json;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("MasterLoader Info: Request has failed.");
                Debug.LogException(e);
                return string.Empty;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool GenerateCode(Config config, Base code)
        {
            return CodeGenerator.Generate(config, code);
        }

        public static bool CreateMaster(string masterName)
        {
            if (!LoadMaster(masterName))
            {
                return false;
            }
            return CreateMaster_();
        }

        public static bool CreateAll()
        {
            if (!LoadAllMaster())
            {
                return false;
            }
            return CreateMaster_();
        }

        private static bool CreateMaster_()
        {
            ConfigData.WaitCreateMaster = true;
            var list = ConfigData.LoadedResultList;
            foreach (var _loadedResult in list)
            {
                var target = ConfigData.MasterNamespaceList.FirstOrDefault(m => m.MasterName == _loadedResult.Name);
                if (target != null)
                {
                    target.Namespace = ConfigData.NameSpace;
                }
                else
                {
                    ConfigData.MasterNamespaceList.Add(new MasterNamespace
                    {
                        MasterName = _loadedResult.Name,
                        Namespace = ConfigData.NameSpace
                    });
                }
                if (!GenerateCode(ConfigData, _loadedResult))
                {
                    //list.Clear();
                    return false;
                }
            }
            SaveConfig();

            if (!EditorApplication.isCompiling)
            {
                OnCreateMaster();
                return true;
            }
            Debug.Log("MasterLoader Info: Now Compiling...");
            return true;
        }

        [DidReloadScripts]
        private static void OnCreateMaster()
        {
            UpdateConfig();
            if (!ConfigData.WaitCreateMaster)
            {
                return;
            }
            ConfigData.WaitCreateMaster = false;
            SaveConfig();
            foreach (var target in ConfigData.LoadedResultList)
            {
                var assetPath = $"{AssetDatabase.GetAssetPath(ConfigData.MasterPathFolder)}/{target.Name}.asset";
                var soMaster = Utility.Utility.GetAssetPathObject(assetPath, target.Name);
                if (soMaster == null)
                {
                    Debug.Log("MasterLoader Info: Creating MasterData...");
                    soMaster = CreateInstance($"{target.Name}{MASTER}");
                    AssetDatabase.CreateAsset(soMaster, assetPath);
                }
                var method = soMaster.GetType().GetMethod("SetData");

                method.Invoke(soMaster, new object[] { target.ValueList });
                EditorUtility.SetDirty(soMaster);
                Debug.Log($"MasterLoader Info: {target.Name} Completely Created!");
            }

            var installer = AssetDatabase.LoadMainAssetAtPath($"{_INSTALLER_PATH}/MasterInstaller.prefab") as GameObject;
            if (installer == null)
            {
                if (!Directory.Exists($"{_INSTALLER_PATH}/"))
                {
                    Directory.CreateDirectory($"{_INSTALLER_PATH}/");
                }
                var go = new GameObject();
                go.AddComponent<MasterInstaller>();
                installer = PrefabUtility.SaveAsPrefabAsset(go, $"{_INSTALLER_PATH}/MasterInstaller.prefab");
                DestroyImmediate(go);
            }
            var component = installer.GetComponent<MasterInstaller>();
            component.SetMaster();
        }

        public static void CreateSpreadSheet(string masterName, string id)
        {
            var list = ConfigData.CreatingMasterValueList.Where(m => !string.IsNullOrEmpty(m.VariableName));
            var comment = list.Select(m => m.Comment).ToArray();
            var parameter = list.Select(m => m.VariableName).ToArray();
            var type = list.Select(m => m.Type).ToArray();
            var value = new Base
            {
                Comment = comment,
                Parameter = parameter,
                Type = type,
                Name = masterName,
            };
            var valueJson = JsonUtility.ToJson(value);
            var url = $"{_API_URL}function=CreateSheet&id={id}&values={valueJson}";

            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    var req = request.SendWebRequest();

                    while (req.progress < 1)
                    {
                        EditorUtility.DisplayProgressBar("Creating MasterSheet...", $"{request.downloadProgress * 100}%", request.downloadProgress);
                    }
#if UNITY_2020_1_OR_NEWER
                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
#else
                    if (request.isHttpError || request.isNetworkError)
#endif
                    {
                        EditorUtility.ClearProgressBar();
                        Debug.Log("MasterLoader Info: Request has failed.");
                        throw new Exception(request.error);
                    }
                    else
                    {
                        EditorUtility.ClearProgressBar();
                        var json = request.downloadHandler.text;
                        if (json.Contains("<!DOCTYPE html>"))
                        {
                            throw new Exception(json);
                        }
                        var returnConfig = JsonUtility.FromJson<Config>(json);
                        ConfigData.SheetUrl = returnConfig.SheetUrl;
                        ConfigData.Masters = returnConfig.Masters;
                        Debug.Log($"MasterLoader Info: Creating Sheet has completed.\n<color=cyan>{returnConfig.SheetUrl}</color>");
                        foreach(var m in ConfigData.Masters)
                        {
                            Debug.Log($"MasterLoader Info: sheet '{m}' has created.");
                        }
                        RemoveOldMasterDictionary();
                    }
                    SaveConfig();
                }
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("MasterLoader Info: Request has failed.");
                Debug.LogException(e);
            }
        }

        public static void JumpCreatedSheet(string sheetUrl)
        {
            Application.OpenURL(sheetUrl);
        }

        public static bool GetSheets(string sheetUrl)
        {
            var url = $"{_API_URL}function=GetSheets&{_URL}={sheetUrl}";

            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    var req = request.SendWebRequest();

                    while (req.progress < 1)
                    {
                        EditorUtility.DisplayProgressBar("Fetching Master Name...", $"{request.downloadProgress * 100}%", request.downloadProgress);
                    }
#if UNITY_2020_1_OR_NEWER
                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
#else
                    if (request.isHttpError || request.isNetworkError)
#endif
                    {
                        EditorUtility.ClearProgressBar();
                        Debug.Log("MasterLoader Info: Request has failed.");
                        throw new Exception(request.error);
                    }
                    else
                    {
                        EditorUtility.ClearProgressBar();
                        var data = JsonUtility.FromJson<Config>(request.downloadHandler.text);
                        if (data.Alerts.Length > 0)
                        {
                            Debug.LogError($"MasterLoader Info: {data.Alerts.Length} sheet problems detected.");
                            for (var i = 0; i < data.Alerts.Length; i++)
                            {
                                Debug.LogAssertion(data.Alerts[i]);
                            }
                            return false;
                        }
                        else if(data.Masters.Length < 1)
                        {
                            Debug.LogError("MasterLoader Info: Loadable master is nothing. Please fix sheet problems.");
                            return false;
                        }
                        else
                        {
                            Debug.Log("MasterLoader Info: Getting Sheet has completed.");
                            ConfigData.Masters = data.Masters;
                            RemoveOldMasterDictionary();
                            SaveConfig();
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("MasterLoader Info: Request has failed.");
                Debug.LogException(e);
                return false;
            }
        }

        public static void SaveConfig()
        {
            var config = GetConfigObject();
            EditorUtility.SetDirty(config);
        }

        private static void RemoveOldMasterDictionary()
        {
            var oldMasters = ConfigData.MasterNamespaceList.Where(m => !ConfigData.Masters.Contains(m.MasterName)).ToList();
            for (var i = 0; i < oldMasters.Count; i++)
            {
                if (ConfigData.Masters.Contains(oldMasters[i].MasterName))
                {
                    ConfigData.MasterNamespaceList.Remove(oldMasters[i]);
                    Debug.Log($"MasterLoader Info: {oldMasters[i]} is removed because it no longer used");
                }
            }
        }

        private static ConfigObject GetConfigObject()
        {
            var obj = Resources.Load<ConfigObject>(_CONFIG_PATH);
            if (obj == null)
            {
                var asset = CreateInstance<ConfigObject>();
                AssetDatabase.CreateAsset(asset, _CONFIG_PATH);
                obj = asset;
            }
            obj.hideFlags = HideFlags.NotEditable;
            return obj;
        }

        public static Config LoadConfig()
        {
            return GetConfigObject().Config;
        }

        public static void UpdateConfig()
        {
            ConfigData = LoadConfig();
        }
    }
}