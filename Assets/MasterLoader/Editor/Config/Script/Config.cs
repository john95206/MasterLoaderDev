using MasterLoader.Core;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace MasterLoaderConfig
{
    [Serializable]
    public class Config
    {
        public string DriveUrl = string.Empty;
        public string SheetUrl = string.Empty;
        public bool IsFetched = false;
        public int SheetIndex = 0;
        public string[] Masters;
        public string CurrentMasterName = string.Empty;
        public string[] Alerts;
        public string NameSpace = "MasterLoader";
        public List<MasterNamespace> MasterNamespaceList = new List<MasterNamespace>();
        public DefaultAsset MasterPathFolder = null;
        public bool WaitCreateMaster = false;
        public bool NeedInstaller = true;
        public MasterLoaderLanguage Language;
        public List<Base> LoadedResultList = new List<Base>();
        public List<MasterValue> CreatingMasterValueList = new List<MasterValue>();
    }
}