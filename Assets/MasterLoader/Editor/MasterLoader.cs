using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace MasterLoader
{
    /// <summary>
    /// スプレッドシートからマスタを取得して自動生成したScriptableObjectに流し込むクラス
    /// </summary>
    [InitializeOnLoad]
    public class MasterLoader : Editor
    {
        [System.Serializable]
        public class Config
        {
            public string DriveUrl = string.Empty;
            public string SheetUrl = string.Empty;
            public bool IsFetched = false;
            public int SheetIndex = 0;
            public bool IsAuto = false;
            public bool IsAll = false;
            public int AllCurrentIndex = 0;
            public int _AllCurrentIndex = 0;
            public string CodeJson = string.Empty;
            public string[] Masters;
            public string CurrentMasterName = string.Empty;
            public string[] Alerts;
        }

        public const string ConfigKey = "MasterLoaderConfig";
        public static Config ConfigData;

        public const string Master = "Master";
        private const string api =
            "https://script.google.com/macros/s/AKfycbwjx-pDNW89Hzi0SV_hGHzVhOMt2_v6K6r4S9Txd_JTuiilzxjHOqjwo3IYcm7PnVWGZQ/exec?";
        private const string urlName = "url=";
        /// <summary>
        /// doGet時の独自変数
        /// 読み込むシートの判断用
        /// </summary>
        private const string sheetName = "sheetName=";
        /// <summary>
        /// マスタを配置するパス。ResoucesディレクトリとMasterディレクトリをあらかじめ作成しておく
        /// </summary>
        public const string path = "Assets/MasterLoader/Resources/Master/";

        /// <summary>
        /// マスタ自動更新するかどうか
        /// </summary>
        public static bool IsAutoUpdateEnabled = false;

        /// <summary>
        /// ローカルでマスタ編集できるかどうか
        /// </summary>
        public static bool IsEditable = false;

        /// <summary>
        /// アプリ起動時に自動でScriptableObjectを更新する
        /// </summary>
        static MasterLoader()
        {
            if (EditorApplication.timeSinceStartup > 100)
            {
                return;
            }

            var json = EditorPrefs.GetString(ConfigKey);
            if (json == string.Empty)
            {
                ConfigData = new Config
                {
                    SheetUrl = string.Empty,
                    IsAuto = false,
                    IsFetched = false,
                    SheetIndex = 0,
                    CodeJson = string.Empty,
                    CurrentMasterName = string.Empty
                };
                Debug.Log("MasterLoader Info: Initialized");
            }
            else
            {
                ConfigData = JsonUtility.FromJson<Config>(json);
            }
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
        /// <param name="done">マスタ取得時に起こしたいイベント</param>
        /// <returns>エラー時の警告またはロードしたマスタ名</returns>
        public static bool LoadMaster(string masterName, Action done = null)
        {
            var url = $"{api}function=LoadMaster&{sheetName}{masterName}&{urlName}{ConfigData.SheetUrl}";

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
                        else
                        {
                            ConfigData.CodeJson = json;
                            ConfigData.CurrentMasterName = masterName;
                            SaveConfig();
                            var data = CodeGenerator.Generate(masterName, path, Master, result);
                            Debug.Log($"MasterLoader Info: {masterName} has Loaded");
                            return true;
                        }
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

        public static void CreateSpreadSheet(string masterName, string id)
        {
            var url = $"{api}function=CreateSheet&masterName={masterName}&id={id}&sheetName={masterName}";

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
            var url = $"{api}function=GetSheets&{urlName}={sheetUrl}";

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
                        }
                        if(data.Masters.Length < 1)
                        {
                            Debug.LogError("MasterLoader Info: Loadable master is nothing. Please fix sheet problems.");
                            return false;
                        }
                        else
                        {
                            Debug.Log("MasterLoader Info: Getting Sheet has completed.");
                            ConfigData.Masters = data.Masters;
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
            EditorPrefs.SetString(ConfigKey, JsonUtility.ToJson(ConfigData));
            ConfigData = JsonUtility.FromJson<Config>(EditorPrefs.GetString(ConfigKey));
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