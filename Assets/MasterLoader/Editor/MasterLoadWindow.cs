using UnityEngine;
using UnityEditor;

namespace MasterLoader
{
    public class MasterLoadWindow : EditorWindow
    {
        private bool _acceptedDriveUrl = false;
        private bool _isAuto = false;
        private bool _IsCompiling = false;
        private bool _IsWaitingGet = false;
        private string _currentMasterName = "";
        private string sheetUrl = string.Empty;
        private string masterName = string.Empty;
        private int sheetIndex = 0;

        [MenuItem("Window/MasterLoader")]
        static void Open()
        {
            GetWindow<MasterLoadWindow>();
        }

        private void OnEnable()
        {
            var json = EditorPrefs.GetString(MasterLoader.ConfigKey);
            if(json == string.Empty)
            {
                MasterLoader.ConfigData = new MasterLoader.Config
                {
                    SheetUrl = string.Empty,
                    IsAuto = false,
                    IsFetched = false,
                    SheetIndex = 0,
                    CodeJson = string.Empty
                };
                Debug.Log("MasterLoader Info: Initialized");
            }
            else
            {
                MasterLoader.ConfigData = JsonUtility.FromJson<MasterLoader.Config>(json);
            }
        }

        private void OnGUI()
        {
            var configData = MasterLoader.ConfigData;

            EditorGUILayout.Space();

            var text = !_acceptedDriveUrl ?
                "Get start to enter your Google Drive folder URL." :
                "Accepted Drive URL!";
            EditorGUILayout.LabelField(text, GUILayout.Width(300));

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Drive URL", GUILayout.Width(80));

            configData.DriveUrl = EditorGUILayout.TextField(configData.DriveUrl, GUILayout.MinWidth(150), GUILayout.MaxWidth(300));

            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(configData.DriveUrl))
            {
                EditorGUILayout.LabelField("Enter your drive URL", GUILayout.Width(200));
            }
            else if (!configData.DriveUrl.StartsWith("https://drive.google.com/drive/"))
            {
                EditorGUILayout.LabelField("this URL is not drive ones", GUILayout.Width(200));
                _acceptedDriveUrl = false;
            }
            else if (configData.DriveUrl.StartsWith("https://drive.google.com/drive/") &&
                configData.DriveUrl.IndexOf("folders") < 0)
            {
                EditorGUILayout.LabelField("this URL is drive ones, but not drive floder.", GUILayout.Width(250));
                _acceptedDriveUrl = false;
            }
            else if (configData.DriveUrl.StartsWith("https://drive.google.com/drive/") && configData.DriveUrl.IndexOf("folders") > -1)
            {
                _acceptedDriveUrl = true;
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Enter your master name", GUILayout.Width(200));

                masterName = EditorGUILayout.TextField(masterName, GUILayout.MinWidth(50), GUILayout.MaxWidth(100));

                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(masterName))
                {
                    EditorGUILayout.Space();

                    var createButton = GUILayout.Button("Create", GUILayout.Width(50));
                    if (createButton)
                    {
                        var idIndex = configData.DriveUrl.LastIndexOf('/');
                        var id = configData.DriveUrl.Substring(idIndex + 1);
                        MasterLoader.CreateSpreadSheet(masterName, id);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("unexpected URL.", GUILayout.Width(120));
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Sheet URL", GUILayout.Width(80));

            sheetUrl = EditorGUILayout.TextField(sheetUrl, GUILayout.MinWidth(150), GUILayout.MaxWidth(300));
            if (sheetUrl != configData.SheetUrl)
            {
                configData.IsFetched = false;
            }
            var isValid = false;

            if(sheetUrl != string.Empty)
            {
                if (!sheetUrl.StartsWith("https://docs.google.com/spreadsheets/"))
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("this URL is not SpreadSheet's one.", GUILayout.Width(200));
                }
                else
                {
                    isValid = true;
                    var fetchButton = GUILayout.Button("Fetch", GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    if (fetchButton)
                    {
                        Undo.RecordObject(this, "url");
                        EditorUtility.SetDirty(this);
                        if (MasterLoader.GetSheets(sheetUrl))
                        {
                            configData.SheetUrl = sheetUrl;
                            configData.IsFetched = true;
                            MasterLoader.SaveConfig();
                        }
                        else
                        {
                            configData.IsFetched = false;
                            MasterLoader.SaveConfig();
                        }
                    }
                }
            }

            if((MasterLoader.ConfigData == null || !isValid) || !configData.IsFetched)
            {
                return;
            }

            EditorGUILayout.Space();

            if (configData.IsFetched)
            {
                configData.SheetIndex = EditorGUILayout.Popup(configData.SheetIndex, configData.Masters);
                if(sheetIndex != configData.SheetIndex)
                {
                    MasterLoader.SaveConfig();
                }
                sheetIndex = configData.SheetIndex;

                // 仕切り線
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                EditorGUILayout.Space();

                _currentMasterName = configData.Masters[configData.SheetIndex];
                var createMasterButton = GUILayout.Button($"Create {_currentMasterName} Master");

                if (createMasterButton)
                {
                    MasterLoader.LoadMaster(_currentMasterName);
                    _IsWaitingGet = true;
                }

                EditorGUILayout.Space();
            }

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

            if(_isAuto != MasterLoader.IsAutoUpdateEnabled)
            {
                MasterLoader.SaveConfig();
            }
            MasterLoader.IsAutoUpdateEnabled = GUILayout.Toggle(MasterLoader.IsAutoUpdateEnabled, "Enable updating masters");
            _isAuto = MasterLoader.IsAutoUpdateEnabled;

            if (EditorApplication.isCompiling)
            {
                if (!_IsCompiling && _IsWaitingGet)
                {
                    Debug.Log("MasterLoader Info: Now Compiling...");
                    _IsCompiling = true;
                }
            }
            if (_IsCompiling)
            {
                if (!EditorApplication.isCompiling)
                {
                    var assetPath = $"{MasterLoader.path}{_currentMasterName}.asset";
                    var soMaster = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (soMaster == null)
                    {
                        Debug.Log("MasterLoader Info: Creating MasterData...");
                        soMaster = CreateInstance($"{_currentMasterName}{MasterLoader.Master}");
                        AssetDatabase.CreateAsset(soMaster, assetPath);
                    }
                    var method = soMaster.GetType().GetMethod("SetData");

                    var valueList = JsonUtility.FromJson<Base>(configData.CodeJson).ValueList;
                    method.Invoke(soMaster, new object[] { valueList });
                    EditorUtility.SetDirty(soMaster);
                    _IsCompiling = false;
                    _IsWaitingGet = false;

                    Debug.Log($"MasterLoader Info: {MasterLoader.Master} Completely Created!");
                }
            }
        }
    }
}