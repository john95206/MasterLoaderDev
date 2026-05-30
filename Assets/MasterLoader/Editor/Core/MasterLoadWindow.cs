using UnityEngine;
using UnityEditor;
using MasterLoaderConfig;
using System.Linq;
using System.Collections.Generic;
using System;

namespace MasterLoader.Core
{
    public class MasterLoadWindow : EditorWindow
    {
        private static class TabStyles
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
        private int _invalidCount = 0;

        private bool _acceptedDriveUrl = false;
        private string _sheetUrl = string.Empty;
        private string _nameSpace = string.Empty;
        private int _sheetIndex = 0;
        private string _masterName = string.Empty;
        private int _indentTypeIndex = 0;
        private int _lineTypeIndex = 0;

        private const string _DRIVE_URL = "https://drive.google.com/drive/";

        [MenuItem("Window/MasterLoader")]
        static void Open()
        {
#if UNITY_2018_1_OR_NEWER
            GetWindow<MasterLoadWindow>(title: "MasterLoader");
#else
            GetWindow<MasterLoadWindow>();
#endif
        }

        private void OnEnable()
        {
            MasterLoader.UpdateConfig();
        }

        private string TextField(string label, string text, float labelWidth = 100, float fieldWidth = 150)
        {
            var value = string.Empty;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.MaxWidth(labelWidth));

                value = GUILayout.TextField(text, GUILayout.MaxWidth(fieldWidth));
            }
            return value;
        }

        private void DrawRefreshConfigButton()
        {
            if (GUILayout.Button("Refresh Config", GUILayout.Width(150)))
            {
                MasterLoader.RefreshConfig();
            }
        }

        private void DrawTabButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // タブを描画する
                _tabStatus = (TabStatus)GUILayout.Toolbar
                (
                    (int)_tabStatus,
                    TabStyles.TabToggles,
                    TabStyles.TabButtonStyle,
                    TabStyles.TabButtonSize
                );
            }
            EditorGUILayout.Space();
        }

        private void DrawCreatorWindow(IConfig configData)
        {
            _acceptedDriveUrl = DrawDriveUrlField(configData);

            if (!_acceptedDriveUrl)
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var input = TextField("Master name", _masterName);
                if (!Utility.Utility.OnValidateInputedValue(input, out var text))
                {
                    _invalidCount++;
                    {
                        GUILayout.Box(EditorGUIUtility.IconContent("Error"));
                    }
                }
                _masterName = text;
            }

            if (!string.IsNullOrEmpty(_masterName))
            {
                DrawMasterValueField(configData);

                EditorGUILayout.Space();

                using (new EditorGUI.DisabledGroupScope(_invalidCount > 0))
                {
                    var createButton = false;
                    if (_invalidCount > 0)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            createButton = GUILayout.Button("Create", GUILayout.Width(50));
                            GUILayout.Box(EditorGUIUtility.IconContent("Error"));
                            GUILayout.Label($"{_invalidCount} invalid content exist.");
                        }
                    }
                    else
                    {
                        createButton = GUILayout.Button("Create", GUILayout.Width(50));
                    }
                    if (createButton)
                    {
                        var idIndex = configData.WindowConfig.DriveUrl.LastIndexOf('/');
                        var id = configData.WindowConfig.DriveUrl.Substring(idIndex + 1);
                        SheetCreator.CreateSpreadSheet(configData, _masterName, id);
                    }
                }
            }
            _invalidCount = 0;
        }

        private void DrawMasterValueField(IConfig configData)
        {
            if(configData.CreateConfig.CreatingMasterValueList.Count < 1)
            {
                if (!GUILayout.Button("Start creating variables"))
                {
                    return;
                }
                configData.CreateConfig.AddMasterValue(new MasterValue { });
            }

            GUILayout.Label("Decrlare master's variables if need.");

            var style = GUI.skin.box;
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos, style, GUILayout.ExpandHeight(false)))
            {
                var actions = new List<ValueAction>();
                for (var i = 0; i < configData.CreateConfig.CreatingMasterValueList.Count; i++)
                {
                    var action = DrawValue(configData.CreateConfig.CreatingMasterValueList[i], configData.CreateConfig.CreatingMasterValueList.Count);
                    if (action.Status == ValueStatus.None)
                    {
                        continue;
                    }
                    actions.Add(action);
                }

                for (var i = 0; i < actions.Count; i++)
                {
                    EditMasterValueList(actions[i], configData);
                }

                _scrollPos = scrollView.scrollPosition;
            }
        }

        private void EditMasterValueList(ValueAction action, IConfig configData)
        {
            var list = configData.CreateConfig.CreatingMasterValueList;
            var currentIndex = list.IndexOf(action.Value);
            switch (action.Status)
            {
                case ValueStatus.None:
                    return;
                case ValueStatus.Up:
                    var upIndex = list.IndexOf(action.Value) - 1;
                    if(upIndex < 0)
                    {
                        break;
                    }
                    var upCache = list[upIndex];
                    configData.CreateConfig.UpdateMasterValue(upIndex, action.Value);
                    configData.CreateConfig.UpdateMasterValue(currentIndex, upCache);
                    break;
                case ValueStatus.Down:
                    var downIndex = list.IndexOf(action.Value) + 1;
                    if(downIndex >= list.Count)
                    {
                        break;
                    }
                    var downCache = list[downIndex];
                    configData.CreateConfig.UpdateMasterValue(downIndex, action.Value);
                    configData.CreateConfig.UpdateMasterValue(currentIndex, downCache);
                    break;
                case ValueStatus.Delete:
                    configData.CreateConfig.RemoveMasterValue(action.Value);
                    break;
                case ValueStatus.Add:
                    if(list.Count < 1)
                    {
                        configData.CreateConfig.AddMasterValue(action.Value);
                        configData.CreateConfig.AddMasterValue(new MasterValue { });
                    }
                    else
                    {
                        var addIndex = list.IndexOf(action.Value) + 1;
                        if(addIndex < list.Count)
                        {
                            configData.CreateConfig.InsertMasterValue(addIndex, new MasterValue { });
                        }
                        else
                        {
                            configData.CreateConfig.AddMasterValue(new MasterValue { });
                        }
                    }
                    break;
                default:
                    return;
            }
        }

        private ValueAction DrawValue(MasterValue _value, int count)
        {
            var status = ValueStatus.None;
            var typeIndex = ArrayUtility.IndexOf(_TYPE_LABELS, _value.Type);
            if(typeIndex < 0)
            {
                typeIndex = 0;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Name", GUILayout.MaxWidth(40));

                var inputtedName = GUILayout.TextField(_value?.VariableName ?? string.Empty, GUILayout.MaxWidth(100));
                if (!Utility.Utility.OnValidateInputedValue(inputtedName, out var name))
                {
                    _invalidCount++;
                    GUILayout.Box(EditorGUIUtility.IconContent("Error"));
                }
                else
                {
                    GUILayout.Box("", GUILayout.Width(20));
                }
                _value.VariableName = inputtedName;

                typeIndex = EditorGUILayout.Popup(typeIndex, _TYPE_LABELS, GUILayout.Width(50));

                _value.Type = _TYPE_LABELS[typeIndex];
                GUILayout.Label("Comment", GUILayout.MaxWidth(70));

                _value.Comment = GUILayout.TextField(_value.Comment);
                if (count > 0)
                {
                    if (GUILayout.Button("▲", GUILayout.Width(30)))
                    {
                        status = ValueStatus.Up;
                    }
                    else if (GUILayout.Button("▼", GUILayout.Width(30)))
                    {
                        status = ValueStatus.Down;
                    }
                    else if (GUILayout.Button("-", GUILayout.Width(30)))
                    {
                        status = ValueStatus.Delete;
                    }
                }
                if (GUILayout.Button("+", GUILayout.Width(30)))
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

        private bool DrawDriveUrlField(IConfig configData)
        {
            var text = "Get start to enter your Google Drive folder URL.";
            EditorGUILayout.LabelField(text, GUILayout.ExpandWidth(true));

            EditorGUILayout.Space();

            var driveUrl = configData.WindowConfig.DriveUrl;

            driveUrl = TextField("Drive URL", configData.WindowConfig.DriveUrl, fieldWidth: 400);

            if (driveUrl != configData.WindowConfig.DriveUrl)
            {
                configData.WindowConfig.SetDriveUrl(driveUrl);
                _isDirty = true;
            }

            var isDriveUrl = driveUrl.StartsWith(_DRIVE_URL);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (string.IsNullOrEmpty(driveUrl))
                {
                    EditorGUILayout.LabelField("Enter your drive URL", GUILayout.ExpandWidth(true));
                    return false;
                }
                else if (!isDriveUrl)
                {
                    GUILayout.Box(EditorGUIUtility.IconContent("Error"));
                    EditorGUILayout.LabelField("this URL is not drive ones", GUILayout.ExpandWidth(true));
                    return false;
                }
                else if (isDriveUrl &&
                    driveUrl.IndexOf("folders") < 0)
                {
                    GUILayout.Box(EditorGUIUtility.IconContent("Error"));
                    EditorGUILayout.LabelField("this URL is drive ones, but not drive floder.", GUILayout.ExpandWidth(true));
                    return false;
                }
                else if (isDriveUrl && driveUrl.IndexOf("folders") > -1)
                {
                    return true;
                }
                else
                {
                    GUILayout.Box(EditorGUIUtility.IconContent("Error"));
                    EditorGUILayout.LabelField("unexpected URL.", GUILayout.ExpandWidth(true));
                    return false;
                }
            }
        }

        private void DrawLoaderWindow(IConfig configData)
        {
            var isValid = DrawFetchWindow(configData);

            if ((MasterLoader.ConfigData == null || !isValid) || !configData.WindowConfig.IsFetched)
            {
                return;
            }

            EditorGUILayout.Space();

            if (EditorUtility.scriptCompilationFailed)
            {
                EditorGUILayout.LabelField("Please fix all compile error.", GUILayout.Width(200));
                return;
            }
            DrawMasterCreateWindow(configData);

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }

        private bool DrawFetchWindow(IConfig configData)
        {
            var isValid = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Sheet URL", GUILayout.Width(100));

                var sheetUrl = configData.WindowConfig.SheetUrl;
                sheetUrl = GUILayout.TextField(configData.WindowConfig.SheetUrl, GUILayout.MaxWidth(300));

                if(sheetUrl != _sheetUrl)
                {
                    _sheetUrl = configData.WindowConfig.SheetUrl;
                    configData.WindowConfig.SetSheetUrl(sheetUrl);
                }

                isValid = _sheetUrl.StartsWith("https://docs.google.com/spreadsheets/");
                if (isValid)
                {
                    var fetchButton = GUILayout.Button("Fetch", GUILayout.Width(80));
                    if (fetchButton)
                    {
                        Undo.RecordObject(this, "url");
                        EditorUtility.SetDirty(this);
                        if (MasterLoader.GetSheets(_sheetUrl))
                        {
                            configData.WindowConfig.SetSheetUrl(_sheetUrl);
                            configData.WindowConfig.SetIsFetched(true);
                        }
                        else
                        {
                            configData.WindowConfig.SetIsFetched(false);
                        }
                        _isDirty = true;
                    }
                }
                else
                {
                    configData.WindowConfig.SetIsFetched(false);
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

        private void DrawMasterCreateWindow(IConfig configData)
        {
            if (!configData.WindowConfig.IsFetched)
            {
                return;
            }
            if(configData.SheetIndex >= configData.MasterBody.Masters.Length)
            {
                configData.SetSheetIndex(0);
            }
            _sheetIndex = EditorGUILayout.Popup("Target sheet", configData.SheetIndex, configData.MasterBody.Masters);
            if (_sheetIndex != configData.SheetIndex)
            {
                configData.SetSheetIndex(_sheetIndex);
                _isDirty = true;
            }

            // 仕切り線
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

            EditorGUILayout.Space();

            var currentMasterName = configData.CurrentMasterName;
            configData.SetCurrentMasterName(configData.MasterBody.Masters[configData.SheetIndex]);
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

            DrawUtilityWindow(configData.WindowConfig, configData.LoadingConfig);
        }

        private void DrawUtilityWindow(IWindowConfig windowConfig, ILoadingConfig loadingConfig)
        {
            DrawLine();

            EditorGUILayout.Space();

            var generatedRootPath = EditorGUILayout.TextField("Generated Root Path", windowConfig.GeneratedRootPath);
            if (generatedRootPath != windowConfig.GeneratedRootPath)
            {
                windowConfig.SetGeneratedRootPath(generatedRootPath);
                _isDirty = true;
            }
            var generatedRootDisplay = string.IsNullOrEmpty(windowConfig.GeneratedRootPath)
                ? "Assets/PlugIns/MasterLoader/Generated (default)"
                : windowConfig.GeneratedRootPath;
            EditorGUILayout.LabelField($"Scripts will be generated to: {generatedRootDisplay}/Scripts/");

            EditorGUILayout.Space();

            _nameSpace = EditorGUILayout.TextField("namespace", loadingConfig.NameSpace);
            if(_nameSpace != loadingConfig.NameSpace)
            {
                loadingConfig.SetNameSpace(_nameSpace);
                _isDirty = true;
            }

            EditorGUILayout.Space();

            _folderPath = AssetDatabase.GetAssetPath(windowConfig.MasterPathFolder);
            windowConfig.SetMasterPathFolder((DefaultAsset)EditorGUILayout.ObjectField("Master path", windowConfig.MasterPathFolder, typeof(DefaultAsset), true));
            var masterPath = AssetDatabase.GetAssetPath(windowConfig.MasterPathFolder);
            if (_folderPath != masterPath && !string.IsNullOrEmpty(masterPath))
            {
                _isDirty = true;
            }
            EditorGUILayout.LabelField($"Master will be generated to: {AssetDatabase.GetAssetPath(windowConfig.MasterPathFolder)}");

            EditorGUILayout.Space();

            DrawIndentSetting(windowConfig);
            DrawLineSetting(windowConfig);

            EditorGUILayout.Space();
        }

        private void DrawIndentSetting(IWindowConfig windowConfig)
        {
            _indentTypeIndex = EditorGUILayout.Popup
            (
                "Indent Type",
                (int)windowConfig.IndentType,
                Enum.GetNames(typeof(IndentType))
            );
            if (_indentTypeIndex != (int)windowConfig.IndentType)
            {
                windowConfig.SetIndentType((IndentType)_indentTypeIndex);
                _isDirty = true;
            }
        }

        private void DrawLineSetting(IWindowConfig windowConfig)
        {
            _lineTypeIndex = EditorGUILayout.Popup
            (
                "Line Type",
                (int)windowConfig.LineType,
                Enum.GetNames(typeof(LineType))
            );
            if (_lineTypeIndex != (int)windowConfig.LineType)
            {
                windowConfig.SetLineType((LineType)_lineTypeIndex);
                _isDirty = true;
            }
        }

        private void DrawConfig(IConfig config)
        {
            DrawLine();

            DrawOpenUrlButton(config);

            //MasterLoader.ConfigData.Language = (MasterLoaderLanguage)EditorGUILayout.EnumPopup("Langage", MasterLoader.ConfigData.Language);
        }

        private void DrawOpenUrlButton(IConfig config)
        {
            if (string.IsNullOrEmpty(config.WindowConfig.SheetUrl))
            {
                return;
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Current Master Sheet", GUILayout.ExpandWidth(false)))
            {
                MasterLoader.JumpCreatedSheet(config.WindowConfig.SheetUrl);
            }
        }

        private void OnGUI()
        {
            DrawRefreshConfigButton();

            var configData = MasterLoader.ConfigData;

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

            if((int)_tabStatus != configData.WindowConfig.TabIndex)
            {
                configData.WindowConfig.SetTabIndex((int)_tabStatus);
                _isDirty = true;
            }

            DrawConfig(configData);

            if (!_isDirty)
            {
                return;
            }
            _isDirty = false;
            MasterLoader.SaveConfig();
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