using System;
using UnityEngine;

namespace MasterLoaderConfig
{
    /// <summary>
    /// 通信で受け取ったマスタの名前を格納するクラス
    /// </summary>
    [Serializable]
    public class MasterBody : IMasterBody, IConfigurable
    {
        [SerializeField]
        private string[] _masters = new string[] { };
        public string[] Masters => _masters;

        public void SetMasters(string[] masters)
        {
            _masters = masters;
        }
    }
}
