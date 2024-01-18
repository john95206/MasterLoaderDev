using System;
using UnityEditor;

namespace MasterLoader.Core
{
    [Serializable]
    public class MasterDataRawAll
    {
        public MasterDataRaw[] Values;
        public EnumValue[] Enums;
    }
}