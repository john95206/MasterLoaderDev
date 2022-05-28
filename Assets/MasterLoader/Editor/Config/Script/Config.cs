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
        [NonSerialized]
        public bool IsFetched = false;
        [NonSerialized]
        public int SheetIndex = 0;
        [NonSerialized]
        public string[] Masters;
        public string CurrentMasterName = string.Empty;
        [NonSerialized]
        public string[] Alerts;
        public string NameSpace = "MasterLoader";
        [NonSerialized]
        public List<MasterNamespace> MasterNamespaceList = new List<MasterNamespace>();
        public DefaultAsset MasterPathFolder = null;
        [NonSerialized]
        public bool WaitCreateMaster = false;
        [NonSerialized]
        public MasterLoaderLanguage Language;
        [NonSerialized]
        public List<Base> LoadedResultList = new List<Base>();
        [NonSerialized]
        public List<MasterValue> CreatingMasterValueList = new List<MasterValue>();
    }
}