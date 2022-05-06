using UnityEngine;
using UnityEditor;
using MasterLoaderConfig;
using System.Linq;

namespace MasterLoader
{
    public class MasterLoadWindow : EditorWindow
    {
        private static class Styles
        {
            private static GUIContent[] _tabToggles = null;
            public static GUIContent[] TabToggles
            {
                get
                {
                    if (_tabToggles == null)
                    {
                        _tabToggles = System.Enum.GetNames(typeof(TabStatus)).Select(x => new GUIContent(x)).ToArray();
                    }
                    return _tabToggles;
                }
            }

            public static readonly GUIStyle TabButtonStyle = "LargeButton";

            public static readonly GUI.ToolbarButtonSize TabButtonSize = GUI.ToolbarButtonSize.Fixed;
        }

        private enum TabStatus
        {
            SheetCreator,
            Loader,
        }

        private TabStatus _tabStatus;
        private bool _isDirty = false;

        private bool _acceptedDriveUrl = false;
        private bool _isAuto = false;
        private string _currentMasterName = "";
        private string _driveUrl = string.Empty;
        private string _sheetUrl = string.Empty;
        private string _masterName = string.Empty;
        private string _nameSpace = string.Empty;
        private string _masterPath = string.Empty;
        private int sheetIndex = 0;

        private const string _DRIVE_URL = "https://drive.google.com/drive/";

        [MenuItem("Window/MasterLoader")]
        static void Open()
        {
            GetWindow<MasterLoadWindow>();
        }

        private void OnEnable()
        {
            MasterLoader.UpdateConfig();
        }

        private void DrawTabButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // タブを描画する
                _tabStatus = (TabStatus)GUILayout.Toolbar
                (
                    (int)_tabStatus,
                    Styles.TabToggles,
                    Styles.TabButtonStyle,
                    Styles.TabButtonSize
                );
            }
            EditorGUILayout.Space();
        }

        private void DrawCreatorWindow(Config configData)
        {
            var text = !_acceptedDriveUrl ?
                "Get start to enter your Google Drive folder URL." :
                "Accepted Drive URL!";
            EditorGUILayout.LabelField(text, GUILayout.Width(300));

            EditorGUILayout.Space();

            // drive url field
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Drive URL", GUILayout.Width(80));

                configData.DriveUrl = EditorGUILayout.TextField(configData.DriveUrl, GUILayout.MinWidth(150), GUILayout.MaxWidth(300));

                if(_driveUrl != configData.DriveUrl)
                {
                    _driveUrl = configData.DriveUrl;
                    _isDirty = true;
                }
            }

            var isDriveUrl = _driveUrl.StartsWith(_DRIVE_URL);

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
                // mastername field
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Enter your master name", GUILayout.Width(200));

                    _masterName = EditorGUILayout.TextField(_masterName, GUILayout.MinWidth(50), GUILayout.MaxWidth(100));
                }

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
        }

        private void DrawLoaderWindow(Config configData)
        {
            var isValid = DrawFetchWindow(configData);

            if ((MasterLoader.ConfigData == null || !isValid) || !configData.IsFetched)
            {
                return;
            }

            EditorGUILayout.Space();

            if (EditorUtility.scriptCompilationFailed)
            {
                EditorGUILayout.LabelField("Please fix all compile error.", GUILayout.Width(200));
                return;
            }
            DrawSheetCreateWindow(configData);

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }

        private bool DrawFetchWindow(Config configData)
        {
            var isValid = _sheetUrl.StartsWith("https://docs.google.com/spreadsheets/");
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Sheet URL", GUILayout.Width(80));

                configData.SheetUrl = EditorGUILayout.TextField(configData.SheetUrl, GUILayout.MinWidth(150), GUILayout.MaxWidth(300));

                if(_sheetUrl != configData.SheetUrl)
                {
                    _sheetUrl = configData.SheetUrl;
                }

                if (isValid)
                {
                    var fetchButton = GUILayout.Button("Fetch", GUILayout.Width(80));
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
                        _isDirty = true;
                    }
                }
                else
                {
                    configData.IsFetched = false;
                }
            }
            if (string.IsNullOrEmpty(_sheetUrl))
            {
                return false;
            }
            if (!isValid)
            {
                EditorGUILayout.LabelField("this URL is not SpreadSheet's one.", GUILayout.Width(200));
                return false;
            }

            return true;
        }

        private void DrawSheetCreateWindow(Config configData)
        {
            if (!configData.IsFetched)
            {
                return;
            }
            configData.SheetIndex = EditorGUILayout.Popup(configData.SheetIndex, configData.Masters);
            if (sheetIndex != configData.SheetIndex)
            {
                sheetIndex = configData.SheetIndex;
                _isDirty = true;
            }

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

            DrawUtilityWindow(configData);
        }

        private void DrawUtilityWindow(Config configData)
        {
            // 仕切り線
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

            EditorGUILayout.Space();

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

            if (_isAuto != MasterLoader.IsAutoUpdateEnabled)
            {
            }
            MasterLoader.IsAutoUpdateEnabled = GUILayout.Toggle(MasterLoader.IsAutoUpdateEnabled, "Enable updating masters");
            _isAuto = MasterLoader.IsAutoUpdateEnabled;
        }

        private void OnGUI()
        {
            var configData = MasterLoader.ConfigData;

            EditorGUILayout.Space();

            MasterLoader.ConfigData.Language = (MasterLoaderLanguage)EditorGUILayout.EnumPopup("Langage", MasterLoader.ConfigData.Language);

            EditorGUILayout.Space();

            DrawTabButtons();

            if(_tabStatus == TabStatus.SheetCreator)
            {
                DrawCreatorWindow(configData);
            }
            else
            {
                DrawLoaderWindow(configData);
            }

            if (!_isDirty)
            {
                return;
            }
            _isDirty = false;
            MasterLoader.SaveConfig();
            Debug.Log("Saved Config");
        }
    }
}