using UnityEngine;
using UnityEditor;
using MasterLoaderConfig;
using System.Linq;
using System.Collections.Generic;

namespace MasterLoader.Core
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
        private enum ValueStatus
        {
            None,
            Up,
            Down,
            Delete,
            Add,
        }
        private class ValueAction
        {
            public ValueStatus Status;
            public MasterValue Value;
        }

        private string[] _TYPE_LABELS = new string[]
        {
            "int",
            "float",
            "double",
            "bool",
            "string",
            "enum"
        };
        private string _folderPath;
        private TabStatus _tabStatus;
        private Vector2 _scrollPos;
        private bool _isDirty = false;

        private bool _acceptedDriveUrl = false;
        private string _sheetUrl = string.Empty;
        private string _nameSpace = string.Empty;
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
            _masterName = EditorGUILayout.TextField("Master name", _masterName);

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

        private void DrawMasterValueField(Config configData)
        {
            if(configData.CreatingMasterValueList.Count < 1)
            {
                if (!GUILayout.Button("Start Editing Values"))
                {
                    return;
                }
                configData.CreatingMasterValueList.Add(new MasterValue { });
            }
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos))
            {
                var actions = new List<ValueAction>();
                for(var i = 0; i < configData.CreatingMasterValueList.Count; i++)
                {
                    var action = DrawValue(configData.CreatingMasterValueList[i], configData.CreatingMasterValueList.Count);
                    if(action.Status == ValueStatus.None)
                    {
                        continue;
                    }
                    actions.Add(action);
                }

                for(var i = 0; i < actions.Count; i++)
                {
                    EditMasterValueList(actions[i], configData);
                }

                _scrollPos = scrollView.scrollPosition;
            }
        }

        private void EditMasterValueList(ValueAction action, Config configData)
        {
            var list = configData.CreatingMasterValueList;
            switch (action.Status)
            {
                case ValueStatus.None:
                    return;
                case ValueStatus.Up:
                    break;
                case ValueStatus.Down:
                    break;
                case ValueStatus.Delete:
                    list.Remove(action.Value);
                    break;
                case ValueStatus.Add:
                    if(list.Count < 1)
                    {
                        list.Add(action.Value);
                        list.Add(new MasterValue { });
                    }
                    else
                    {
                        var index = list.IndexOf(action.Value) + 1;
                        if(index < list.Count)
                        {
                            list.Insert(index, new MasterValue { });
                        }
                        else
                        {
                            list.Add(new MasterValue { });
                        }
                    }
                    break;
                default:
                    return;
            }
        }

        private ValueAction DrawValue(MasterValue _value, int count)
        {
            var typeIndex = 0;
            var status = ValueStatus.None;

            using(new EditorGUILayout.HorizontalScope())
            {
                _value.VariableName = EditorGUILayout.TextField("Name", _value.VariableName);
                typeIndex = EditorGUILayout.Popup(typeIndex, _TYPE_LABELS);
                _value.Type = _TYPE_LABELS[typeIndex];
                _value.Value = EditorGUILayout.TextField("Value", _value.Value);
                if(count > 0)
                {
                    if (GUILayout.Button("▲"))
                    {
                        status = ValueStatus.Up;
                    }
                    else if (GUILayout.Button("▼"))
                    {
                        status = ValueStatus.Down;
                    }
                    else if (GUILayout.Button(" - "))
                    {
                        status = ValueStatus.Delete;
                    }
                }
                if(GUILayout.Button(" + "))
                {
                    status = ValueStatus.Add;
                }
            }
            return new ValueAction
            {
                Status = status,
                Value = _value
            };
        }

        private bool DrawDriveUrlField(Config configData)
        {
            var text = !_acceptedDriveUrl ?
                "Get start to enter your Google Drive folder URL." :
                "Accepted Drive URL!";
            EditorGUILayout.LabelField(text, GUILayout.Width(300));

            EditorGUILayout.Space();

            var driveUrl = configData.DriveUrl;

            configData.DriveUrl = EditorGUILayout.TextField("Drive URL", configData.DriveUrl);

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
            DrawLine();

            EditorGUILayout.Space();

            _nameSpace = EditorGUILayout.TextField("namespace", configData.NameSpace);
            if(_nameSpace != configData.NameSpace)
            {
                configData.NameSpace = _nameSpace;
                _isDirty = true;
            }

            EditorGUILayout.Space();

            _folderPath = AssetDatabase.GetAssetPath(configData.MasterPathFolder);
            configData.MasterPathFolder = (DefaultAsset)EditorGUILayout.ObjectField("Master path", configData.MasterPathFolder, typeof(DefaultAsset), true);
            var masterPath = AssetDatabase.GetAssetPath(configData.MasterPathFolder);
            if (_folderPath != masterPath && !string.IsNullOrEmpty(masterPath))
            {
                _isDirty = true;
            }
            EditorGUILayout.LabelField($"Master will be generated to: {configData.MasterPathFolder}");

            EditorGUILayout.Space();
        }

        private void DrawConfig(Config config)
        {
            DrawLine();

            EditorGUILayout.Space();

            var needInstaller = config.NeedInstaller;
            needInstaller = EditorGUILayout.Toggle("Create MasterInstaller?", needInstaller);
            if(needInstaller != config.NeedInstaller)
            {
                config.NeedInstaller = needInstaller;
                _isDirty = true;
            }
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

            DrawConfig(configData);

            if (!_isDirty)
            {
                return;
            }
            _isDirty = false;
            MasterLoader.SaveConfig();
            Debug.Log("Saved Config");
        }

        private void DrawLine(bool isHorizontal = true)
        {
            if (isHorizontal)
            {
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            }
            else
            {
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Width(1));
            }
        }
    }
}