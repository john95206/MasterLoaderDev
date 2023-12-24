using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MasterLoaderConfig
{
    public interface ICreateConfig
    {
        ReadOnlyCollection<MasterValue> CreatingMasterValueList { get; }
        void SetMasterValueList(List<MasterValue> masterValueList);
        void AddMasterValue(MasterValue masterValue);
        void InsertMasterValue(int index, MasterValue masterValue);
        void UpdateMasterValue(int index, MasterValue masterValue);
        bool RemoveMasterValue(MasterValue masterValue);
    }
}
