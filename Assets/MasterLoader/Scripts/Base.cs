using System;

namespace MasterLoader
{
    [Serializable]
    public class Base
    {
        public string Name;
        public string[] Parameter;
        public string[] Type;
        public string[] Comment;
        public string[] ValueList;
        public string[] Alerts;
    }
}