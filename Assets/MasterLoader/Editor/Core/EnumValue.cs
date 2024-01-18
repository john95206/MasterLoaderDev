using System.Collections.Generic;

namespace MasterLoader.Core
{
    [System.Serializable]
    public class EnumValueOld
    {
        public string Name;
        public List<string> ValueList = new List<string>();
        public bool RemarkedAsUsed = false;
    }
}
