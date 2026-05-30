using MasterLoader.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace MasterLoaderConfig
{
    [Serializable]
    internal class MasterSignatureEntry
    {
        public string Name;
        public string Signature;
    }
}

namespace MasterLoaderConfig
{
    /// <summary>
    /// マスタのロードで使う処理のクラス
    /// </summary>
    [Serializable]
    public class LoadingConfig : ILoadingConfig, IConfigurable
    {
        [SerializeField]
        private string _nameSpace = "MasterLoader";
        [SerializeField]
        private bool _isWaitingCreateMaster = false;
        [SerializeField]
        private List<MasterDataRaw> _loadedResultList = new List<MasterDataRaw>();
        [SerializeField]
        private List<EnumValue> _enumValueList = new List<EnumValue>();
        [SerializeField]
        private List<MasterSignatureEntry> _masterSignatures = new List<MasterSignatureEntry>();

        public string NameSpace => _nameSpace;
        public bool IsWaitingCreateMaster => _isWaitingCreateMaster;
        public ReadOnlyCollection<MasterDataRaw> LoadedResultList => _loadedResultList.AsReadOnly();
        public ReadOnlyCollection<EnumValue> EnumValueList => _enumValueList.AsReadOnly();

        public void SetNameSpace(string nameSpace)
        {
            _nameSpace = nameSpace;
        }

        public void SetLoadedResultList(List<MasterDataRaw> loadedResultList)
        {
            _loadedResultList = loadedResultList;
        }

        public void AddLoadedResult(MasterDataRaw loadedResult)
        {
            if (_loadedResultList.Any(x => x.Name == loadedResult.Name))
            {
                return;
            }
            _loadedResultList.Add(loadedResult);
        }
        public void SetIsWaitingCreateMaster(bool isWaitingCreateMaster)
        {
            _isWaitingCreateMaster = isWaitingCreateMaster;
        }

        public void SetEnumValueList(EnumValue[] enumValues)
        {
            _enumValueList = enumValues.ToList();
        }

        public bool RemoveLoadedResultList(MasterDataRaw loadedResult)
        {
            return _loadedResultList.Remove(loadedResult);
        }

        public void ClearLoadedResultList()
        {
            _loadedResultList.Clear();
        }

        public void ClearEnumValueList()
        {
            _enumValueList.Clear();
        }

        public IReadOnlyList<string> StoredMasterNames
            => _masterSignatures.Select(e => e.Name).ToList().AsReadOnly();

        public string GetMasterSignature(string masterName)
        {
            return _masterSignatures.FirstOrDefault(e => e.Name == masterName)?.Signature ?? string.Empty;
        }

        public void SetMasterSignature(string masterName, string signature)
        {
            var entry = _masterSignatures.FirstOrDefault(e => e.Name == masterName);
            if (entry != null)
                entry.Signature = signature;
            else
                _masterSignatures.Add(new MasterSignatureEntry { Name = masterName, Signature = signature });
        }

        public void RemoveMasterSignature(string masterName)
        {
            _masterSignatures.RemoveAll(e => e.Name == masterName);
        }
    }
}
