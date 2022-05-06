using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using MasterLoaderConfig;

namespace MasterLoader
{
    /// <summary>
    /// スプレッドシートからマスタを取得して自動生成したScriptableObjectに流し込むクラス
    /// </summary>
    [InitializeOnLoad]
    public class MasterLoader : Editor
    {
        public static Config ConfigData;
        private static Base _loadedResult;

        private const string _CONFIG_PATH = "Confing";
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
        /// マスタ自動更新するかどうか
        /// </summary>
        public static bool IsAutoUpdateEnabled = false;

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
            //// ゲームプレビュー終了時に自動でScriptableObjectを更新する
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (ConfigData.IsAuto)
                {
                    if (state == PlayModeStateChange.EnteredEditMode)
                    {
                        //for(var i = 0; i < ConfigData.Masters.Length; i++)
                        //{
                        //    LoadMaster(ConfigData.Masters[i]);
                        //}
                    }
                }
            };
        }

        /// <summary>
        /// スプレッドシートからマスタを取得する
        /// </summary>
        /// <param name="masterName">取得するマスタ名</param>
        /// <returns>エラー時の警告またはロードしたマスタ名</returns>
        private static bool LoadMaster(string masterName)
        {
            var url = $"{_API_URL}function=LoadMaster&{_SHEET_NAME}{masterName}&{_URL}{ConfigData.SheetUrl}";

            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    var req = request.SendWebRequest();

                    while (req.progress < 1)
                    {
                        EditorUtility.DisplayProgressBar("Getting Master Data...", $"{request.downloadProgress * 100}%", request.downloadProgress);
                    }

                    if (request.isHttpError || request.isNetworkError)
                    {
                        Debug.LogError("MasterLoader Info: NetWork Error.");
                        throw new Exception(request.error);
                    }
                    else
                    {
                        var json = request.downloadHandler.text;
                        if (json.Contains("<!DOCTYPE html>"))
                        {
                            Debug.Log(json);
                            return false;
                        }

                        var result = JsonUtility.FromJson<Base>(json);

                        if(result.Alerts?.Length > 0)
                        {
                            for(var i = 0; i < result.Alerts.Length; i++)
                            {
                                Debug.LogError($"MasterLoader Info: {result.Alerts[i]}");
                            }
                            return false;
                        }
                        _loadedResult = result;
                        ConfigData.CodeJson = json;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("MasterLoader Info: Request has failed.");
                Debug.LogException(e);
                return false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool GenerateCode(string masterName, string masterPath, string nameSpace, Base code)
        {
            return CodeGenerator.Generate(masterName, masterPath, nameSpace, code);
        }

        public static bool CreateMaster(string masterName, string masterPath, string nameSpace)
        {
            if (!LoadMaster(masterName))
            {
                return false;
            }

            ConfigData.CurrentMasterName = masterName;
            ConfigData.WaitCreateMaster = true;
            SaveConfig();

            if (!GenerateCode(masterName, masterPath, nameSpace, _loadedResult))
            {
                _loadedResult = default;
                return false;
            }
            _loadedResult = default;

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
            var assetPath = $"{ConfigData.MasterPath}{ConfigData.CurrentMasterName}.asset";
            var soMaster = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (soMaster == null)
            {
                Debug.Log("MasterLoader Info: Creating MasterData...");
                soMaster = CreateInstance($"{ConfigData.CurrentMasterName}{MASTER}");
                AssetDatabase.CreateAsset(soMaster, assetPath);
            }
            var method = soMaster.GetType().GetMethod("SetData");

            try
            {
                var valueList = JsonUtility.FromJson<Base>(ConfigData.CodeJson).ValueList;
                method.Invoke(soMaster, new object[] { valueList });
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return;
            }
            EditorUtility.SetDirty(soMaster);

            Debug.Log($"MasterLoader Info: {MASTER} Completely Created!");
            ConfigData._AllCurrentIndex++;
            SaveConfig();
        }

        public static void CreateSpreadSheet(string masterName, string id)
        {
            var url = $"{_API_URL}function=CreateSheet&masterName={masterName}&id={id}&sheetName={masterName}";

            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    var req = request.SendWebRequest();

                    while (req.progress < 1)
                    {
                        EditorUtility.DisplayProgressBar("Creating MasterSheet...", $"{request.downloadProgress * 100}%", request.downloadProgress);
                    }

                    if (request.isHttpError || request.isNetworkError)
                    {
                        EditorUtility.ClearProgressBar();
                        Debug.Log("MasterLoader Info: Request has failed.");
                        throw new Exception(request.error);
                    }
                    else
                    {
                        EditorUtility.ClearProgressBar();
                        var json = request.downloadHandler.text;
                        var returnConfig = JsonUtility.FromJson<Config>(json);
                        ConfigData.SheetUrl = returnConfig.SheetUrl;
                        ConfigData.Masters = returnConfig.Masters;
                        ConfigData.CurrentMasterName = masterName;
                        Debug.Log("MasterLoader Info: Creating Sheet has completed.");
                        foreach(var m in ConfigData.Masters)
                        {
                            Debug.Log($"MasterLoader Info: sheet '{m}' has created.");
                        }
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

                    if (request.isHttpError || request.isNetworkError)
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
            GetConfigObject().SetData(ConfigData);
        }

        private static ConfigObject GetConfigObject()
        {
            var obj = Resources.Load<ConfigObject>(_CONFIG_PATH);
            if (obj == null)
            {
                var asset = CreateInstance<ConfigObject>();
                AssetDatabase.CreateAsset(asset, _CONFIG_PATH);
                asset.hideFlags = HideFlags.NotEditable;
                obj = asset;
            }
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

        public static void ResetCreateAllConfig()
        {
            ConfigData.IsAll = false;
            ConfigData.AllCurrentIndex = 0;
            ConfigData._AllCurrentIndex = 0;
            SaveConfig();
        }
    }
}