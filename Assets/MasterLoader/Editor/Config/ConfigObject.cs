using UnityEngine;

namespace MasterLoader
{
    [CreateAssetMenu]
    public class ConfigObject : ScriptableObject
    {
        //[HideInInspector]
        public Config Config;

        public void SetData(Config config)
        {
            Config = config;
        }
    }
}