using System;

namespace MasterLoader
{
    [Serializable]
    public class Config
    {
        public string DriveUrl = string.Empty;
        public string SheetUrl = string.Empty;
        public bool IsFetched = false;
        public int SheetIndex = 0;
        public bool IsAuto = false;
        public bool IsAll = false;
        public int AllCurrentIndex = 0;
        public int _AllCurrentIndex = 0;
        public string CodeJson = string.Empty;
        public string[] Masters;
        public string CurrentMasterName = string.Empty;
        public string[] Alerts;
        public string NameSpace = string.Empty;
        public string MasterPath = string.Empty;
        public bool WaitCreateMaster = false;
    }
}