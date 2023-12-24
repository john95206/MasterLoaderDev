using System.Collections.Generic;

namespace MasterLoader.Core
{
    [System.Serializable]
    public class EnumValue
    {
        public string Parameter;
        public List<string> ValueList = new List<string>();
        public bool RemarkedAsUsed = false;
    }
}
