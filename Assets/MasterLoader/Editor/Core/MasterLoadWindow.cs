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

        private DefaultAsset _pathFolder;
        private TabStatus _tabStatus;
        private bool _isDirty = false;

        private bool _acceptedDriveUrl = false;
        private string _sheetUrl = string.Empty;
        private string _nameSpace = string.Empty;
        private string _masterPath = string.Empty;
        private int sheetIndex = 0;
        private string _masterName = string.Empty;

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
            _acceptedDriveUrl = DrawDriveUrlField(configData);

            if (!_acceptedDriveUrl)
            {
                return;
            }

            // mastername field
            _masterName = EditorGUILayout.TextField("Enter your master name", _masterName, GUILayout.MinWidth(50), GUILayout.MaxWidth(100));

            if (!string.IsNullOrEmpty(_masterName))
            {
                DrawMasterValueField(configData);

                EditorGUILayout.Space();

                var createButton = GUILayout.Button("Create", GUILayout.Width(50));
                if (createButton)
                {
                    var idIndex = configData.DriveUrl.LastIndexOf('/');
                    var id = configData.DriveUrl.Substring(idIndex + 1);
                    MasterLoader.CreateSpreadSheet(_masterName, id);
                }
            }
        }

        {
            var text = !_acceptedDriveUrl ?
                "Get start to enter your Google Drive folder URL." :
                "Accepted Drive URL!";
            EditorGUILayout.LabelField(text, GUILayout.Width(300));

            EditorGUILayout.Space();

            var driveUrl = configData.DriveUrl;

            configData.DriveUrl = EditorGUILayout.TextField("Drive URL", configData.DriveUrl, GUILayout.MinWidth(150), GUILayout.MaxWidth(300));

            if (driveUrl != configData.DriveUrl)
            {
                driveUrl = configData.DriveUrl;
                _isDirty = true;
            }

            var isDriveUrl = driveUrl.StartsWith(_DRIVE_URL);

            if (string.IsNullOrEmpty(driveUrl))
            {
                EditorGUILayout.LabelField("Enter your drive URL", GUILayout.Width(200));
                return false;
            }
            else if (!isDriveUrl)
            {
                EditorGUILayout.LabelField("this URL is not drive ones", GUILayout.Width(200));
                return false;
            }
            else if (isDriveUrl &&
                driveUrl.IndexOf("folders") < 0)
            {
                EditorGUILayout.LabelField("this URL is drive ones, but not drive floder.", GUILayout.Width(250));
                return false;
            }
            else if (isDriveUrl && driveUrl.IndexOf("folders") > -1)
            {
                return true;
            }
            else
            {
                EditorGUILayout.LabelField("unexpected URL.", GUILayout.Width(120));
                return false;
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
                configData.SheetUrl = EditorGUILayout.TextField("Sheet URL", configData.SheetUrl, GUILayout.MinWidth(150), GUILayout.MaxWidth(300));

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

            var currentMasterName = configData.CurrentMasterName;
            configData.CurrentMasterName = configData.Masters[configData.SheetIndex];
            if (currentMasterName != configData.CurrentMasterName)
            {
                _isDirty = true;
            }

            var createMasterButton = GUILayout.Button($"Create {configData.CurrentMasterName} Master");
            if (createMasterButton)
            {
                MasterLoader.CreateMaster(configData.CurrentMasterName);
            }
            var createAllButton = GUILayout.Button($"Create All Master");
            if (createAllButton)
            {
                MasterLoader.CreateAll();
            }

            DrawUtilityWindow(configData);
        }

        private void DrawUtilityWindow(Config configData)
        {
            // 仕切り線
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

            EditorGUILayout.Space();

            _nameSpace = EditorGUILayout.TextField("namespace", configData.NameSpace);
            if(_nameSpace != configData.NameSpace)
            {
                configData.NameSpace = _nameSpace;
                _isDirty = true;
            }

            EditorGUILayout.Space();

            _pathFolder = (DefaultAsset)EditorGUILayout.ObjectField("Master path", _pathFolder, typeof(DefaultAsset), true);
            _masterPath = AssetDatabase.GetAssetPath(_pathFolder);
            if(_masterPath != configData.MasterPath && !string.IsNullOrEmpty(_masterPath))
            {
                configData.MasterPath = _masterPath;
                _isDirty = true;
            }
            EditorGUILayout.LabelField($"Master will be generated to: {configData.MasterPath}");

            EditorGUILayout.Space();
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