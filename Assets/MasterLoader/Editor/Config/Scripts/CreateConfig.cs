using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace MasterLoaderConfig
{
    /// <summary>
    /// スプレッドシートの新規作成に使うパラメータを格納するクラス
    /// </summary>
    [Serializable]
    public class CreateConfig : ICreateConfig, IConfigurable
    {
        [SerializeField]
        private List<MasterValue> _creatingMasterValueList = new List<MasterValue>();
        public ReadOnlyCollection<MasterValue> CreatingMasterValueList => _creatingMasterValueList.AsReadOnly();

        public void AddMasterValue(MasterValue creatingMasterValue)
        {
            if (_creatingMasterValueList.Any(x => x.VariableName == creatingMasterValue.VariableName))
            {
                return;
            }
            _creatingMasterValueList.Add(creatingMasterValue);
        }

        public void SetMasterValueList(List<MasterValue> creatingMasterValueList)
        {
            _creatingMasterValueList = creatingMasterValueList;
        }

        public bool RemoveMasterValue(MasterValue masterValue)
        {
            return _creatingMasterValueList.Remove(masterValue);
        }

        public void InsertMasterValue(int index, MasterValue masterValue)
        {
            _creatingMasterValueList.Insert(index, masterValue);
        }

        public void UpdateMasterValue(int index, MasterValue masterValue)
        {
            _creatingMasterValueList[index] = masterValue;
        }
    }
}
