using System;
using UnityEditor;
using UnityEngine;

namespace MasterLoaderConfig
{
    [Serializable]
    public class WindowConfig : IWindowConfig, IConfigurable
    {
        [SerializeField]
        private string _driveUrl = string.Empty;
        [SerializeField]
        private string _sheetUrl = string.Empty;
        [SerializeField]
        private bool _isFetched = false;
        [SerializeField]
        private int _windowTabIndex = 0;
        [SerializeField]
        private DefaultAsset _masterPathFolder = null;
        [SerializeField]
        private LineType _lineType = LineType.CRLF;
        [SerializeField]
        private IndentType _indentType = IndentType.SPACE;
        [SerializeField]
        private string _generatedRootPath = string.Empty;

        public string DriveUrl => _driveUrl;
        public string SheetUrl => _sheetUrl;
        public bool IsFetched => _isFetched;
        public int TabIndex => _windowTabIndex;
        public DefaultAsset MasterPathFolder => _masterPathFolder;
        public LineType LineType => _lineType;
        public IndentType IndentType => _indentType;
        public string GeneratedRootPath => _generatedRootPath;

        public void SetDriveUrl(string driveUrl)
        {
            _driveUrl = driveUrl;
        }

        public void SetSheetUrl(string sheetUrl)
        {
            _sheetUrl = sheetUrl;
        }

        public void SetIsFetched(bool isFetched)
        {
            _isFetched = isFetched;
        }

        public void SetTabIndex(int index)
        {
            _windowTabIndex = index;
        }

        public void SetLineType(LineType lineType)
        {
            _lineType = lineType;
        }

        public void SetIndentType(IndentType indentType)
        {
            _indentType = indentType;
        }

        public void SetMasterPathFolder(DefaultAsset masterPathFolder)
        {
            _masterPathFolder = masterPathFolder;
        }

        public void SetGeneratedRootPath(string path)
        {
            _generatedRootPath = path;
        }
    }
}
