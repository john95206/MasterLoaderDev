using UnityEngine;
using UnityEditor;

namespace MasterLoader
{
    public class MasterLoadWindow : EditorWindow
    {
        private bool _acceptedDriveUrl = false;
        private bool _isAuto = false;
        private string _currentMasterName = "";
        private string _driveUrl = string.Empty;
        private string _sheetUrl = string.Empty;
        private string _masterName = string.Empty;
        private string _nameSpace = string.Empty;
        private string _masterPath = string.Empty;
        private int sheetIndex = 0;

        [MenuItem("Window/MasterLoader")]
        static void Open()
        {
            GetWindow<MasterLoadWindow>();
        }

        private void OnEnable()
        {
            MasterLoader.UpdateConfig();
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

            _driveUrl = configData.DriveUrl;

            EditorGUILayout.EndHorizontal();

            var isDriveUrl = _driveUrl.StartsWith("https://drive.google.com/drive/");

            if (string.IsNullOrEmpty(_driveUrl))
            {
                EditorGUILayout.LabelField("Enter your drive URL", GUILayout.Width(200));
            }
            else if (!isDriveUrl)
            {
                EditorGUILayout.LabelField("this URL is not drive ones", GUILayout.Width(200));
                _acceptedDriveUrl = false;
            }
            else if (isDriveUrl &&
                _driveUrl.IndexOf("folders") < 0)
            {
                EditorGUILayout.LabelField("this URL is drive ones, but not drive floder.", GUILayout.Width(250));
                _acceptedDriveUrl = false;
            }
            else if (isDriveUrl && _driveUrl.IndexOf("folders") > -1)
            {
                _acceptedDriveUrl = true;
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Enter your master name", GUILayout.Width(200));

                _masterName = EditorGUILayout.TextField(_masterName, GUILayout.MinWidth(50), GUILayout.MaxWidth(100));

                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(_masterName))
                {
                    EditorGUILayout.Space();

                    var createButton = GUILayout.Button("Create", GUILayout.Width(50));
                    if (createButton)
                    {
                        var idIndex = _driveUrl.LastIndexOf('/');
                        var id = _driveUrl.Substring(idIndex + 1);
                        MasterLoader.CreateSpreadSheet(_masterName, id);
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

            configData.SheetUrl = EditorGUILayout.TextField(configData.SheetUrl, GUILayout.MinWidth(150), GUILayout.MaxWidth(300));

            _sheetUrl = configData.SheetUrl;

            if (_sheetUrl != configData.SheetUrl)
            {
                configData.IsFetched = false;
            }
            var isValid = false;

            if(_sheetUrl != string.Empty)
            {
                if (!_sheetUrl.StartsWith("https://docs.google.com/spreadsheets/"))
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
                        if (MasterLoader.GetSheets(_sheetUrl))
                        {
                            configData.SheetUrl = _sheetUrl;
                            configData.IsFetched = true;
                        }
                        else
                        {
                            configData.IsFetched = false;
                        }
                        MasterLoader.SaveConfig();
                    }
                }
            }

            if((MasterLoader.ConfigData == null || !isValid) || !configData.IsFetched)
            {
                return;
            }

            EditorGUILayout.Space();

            if (EditorUtility.scriptCompilationFailed)
            {
                EditorGUILayout.LabelField("Please fix all compile error.", GUILayout.Width(200));
                return;
            }

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

                if (!configData.IsAll)
                {
                    _currentMasterName = configData.Masters[configData.SheetIndex];
                }

                var createMasterButton = GUILayout.Button($"Create {_currentMasterName} Master");
                if (createMasterButton)
                {
                    MasterLoader.CreateMaster(_currentMasterName, _masterPath, _nameSpace);
                }

                var resetAllButton = GUILayout.Button($"一括DLがうまくいかないときに押すボタン");
                if (resetAllButton)
                {
                    MasterLoader.ResetCreateAllConfig();
                }

                if (!configData.IsAll)
                {
                    var createAllButton = GUILayout.Button($"Create All Master");

                    if (createAllButton)
                    {
                        // configData.IsAll = true;
                        // MasterLoader.SaveConfig();
                    }
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
        }
    }
}