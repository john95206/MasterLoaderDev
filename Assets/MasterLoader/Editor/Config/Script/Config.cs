using MasterLoader.Core;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace MasterLoaderConfig
{
    [Serializable]
    public class Config : IConfig
    {
        public string DriveUrl = string.Empty;
        public string SheetUrl = string.Empty;
        public bool IsFetched = false;
        public int TabIndex = 0;
        public int SheetIndex = 0;
        public string[] Masters = new string[] { };
        public string CurrentMasterName = string.Empty;
        public string[] Alerts = new string[] { };
        public string NameSpace = "MasterLoader";
        public List<MasterNamespace> MasterNamespaceList = new List<MasterNamespace>();
        public DefaultAsset MasterPathFolder = null;
        public bool WaitCreateMaster = false;
        public MasterLoaderLanguage Language;
        public List<Base> LoadedResultList = new List<Base>();
        public List<MasterValue> CreatingMasterValueList = new List<MasterValue>();
        public string DriveUrl_ { get { return DriveUrl; } set { DriveUrl = value; } }
        public string SheetUrl_ { get { return SheetUrl; } set { SheetUrl = value; } }
        public bool IsFetched_ { get { return IsFetched; } set { IsFetched = value; } }
        public int TabIndex_ { get { return TabIndex; } set { TabIndex = value; } }
        public int SheetIndex_ { get { return SheetIndex; } set { SheetIndex = value; } }
        public string[] Masters_ { get { return Masters; } set { Masters = value; } }
        public string CurrentMasterName_ { get { return CurrentMasterName; } set { CurrentMasterName = value; } }
        public string[] Alerts_ { get { return Alerts; } set { Alerts = value; } }
        public string NameSpace_ { get { return NameSpace; } set { NameSpace = value; } }
        public List<MasterNamespace> MasterNamespaceList_ { get { return MasterNamespaceList; } set { MasterNamespaceList = value; } }
        public DefaultAsset MasterPathFolder_ { get { return MasterPathFolder; } set { MasterPathFolder = value; } }
        public bool WaitCreateMaster_ { get { return WaitCreateMaster; } set { WaitCreateMaster = value; } }
        public MasterLoaderLanguage Language_ { get { return Language; } set { Language = value; } }
        public List<Base> LoadedResultList_ { get { return LoadedResultList; } set { LoadedResultList = value; } }
        public List<MasterValue> CreatingMasterValueList_ { get { return CreatingMasterValueList; } set { CreatingMasterValueList = value; } }
    }

    public interface IConfig
    {
        public string DriveUrl_ { get; set; }
        public string SheetUrl_ { get; set; }
        public bool IsFetched_ { get; set; }
        public int TabIndex_ { get; set; }
        public int SheetIndex_ { get; set; }
        public string[] Masters_ { get; set; }
        public string CurrentMasterName_ { get; set; }
        public string[] Alerts_ { get; set; }
        public string NameSpace_ { get; set; }
        public List<MasterNamespace> MasterNamespaceList_ { get; set; }
        public DefaultAsset MasterPathFolder_ { get; set; }
        public bool WaitCreateMaster_ { get; set; }
        public MasterLoaderLanguage Language_ { get; set; }
        public List<Base> LoadedResultList_ { get; set; }
        public List<MasterValue> CreatingMasterValueList_ { get; set; }
    }
}