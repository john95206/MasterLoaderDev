using UnityEngine;

namespace MasterLoaderConfig
{
    public class ConfigObject : ScriptableObject
    {
        [SerializeField]
        private Config _config;
        public IConfig Config => _config;

        public void RefreshConfig()
        {
            _config = new Config();
        }
    }
}