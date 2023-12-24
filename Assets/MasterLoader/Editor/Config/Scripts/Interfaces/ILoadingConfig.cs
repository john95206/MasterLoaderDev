using MasterLoader.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MasterLoaderConfig
{
    public interface ILoadingConfig
    {
        string NameSpace { get; }
        void SetNameSpace(string masterNamespace);
        bool IsWaitingCreateMaster { get; }
        void SetIsWaitingCreateMaster(bool waitingCreateMaster);
        ReadOnlyCollection<MasterDataRaw> LoadedResultList { get; }
        void SetLoadedResultList(List<MasterDataRaw> loadedResultList);
        void AddLoadedResult(MasterDataRaw loadedResult);
        bool RemoveLoadedResultList(MasterDataRaw loadedResult);
        ReadOnlyCollection<EnumValue> EnumValueList { get; }
        void SetEnumValueList(List<EnumValue> enumValueList);
        void AddEnumValue(EnumValue enumValue);
        bool RemoveEnumValue(EnumValue enumValue);
        void ClearLoadedResultList();
        void ClearEnumValueList();
    }
}
