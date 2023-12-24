using System;
using UnityEngine;

namespace MasterLoaderConfig
{
    [Serializable]
    public class Config : IConfig, IConfigurable
    {
        [SerializeField]
        private WindowConfig _windowConfig = new WindowConfig();
        [SerializeField]
        private int _sheetIndex = 0;
        [SerializeField]
        private string _currentMasterName = string.Empty;
        [SerializeField]
        private MasterLoaderLanguage _currentLanguage;
        [SerializeField]
        private MasterBody _masterBody;
        [SerializeField]
        private CreateConfig _createConfig = new CreateConfig();
        [SerializeField]
        private LoadingConfig _loadingConfig = new LoadingConfig();

        public WindowConfig WindowConfig => _windowConfig;
        public int SheetIndex => _sheetIndex;
        public string CurrentMasterName => _currentMasterName;
        public MasterLoaderLanguage CurrentLanguage => _currentLanguage;
        public IMasterBody MasterBody => _masterBody;
        public ICreateConfig CreateConfig => _createConfig;
        public ILoadingConfig LoadingConfig => _loadingConfig;

        public void SetSheetIndex(int index)
        {
            _sheetIndex = index;
        }

        public void SetCurrentMasterName(string masterName)
        {
            _currentMasterName = masterName;
        }

        public void SetCurrentLanguage(MasterLoaderLanguage language)
        {
            _currentLanguage = language;
        }
    }
}