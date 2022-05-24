using UnityEditor;
using UnityEngine;

namespace MasterLoader.Utility
{
    public static class Utility
    {
        public static Object GetAssetPathObject(string path, string name)
        {
            var assetPath = $"{path}/{name}.asset";
            return AssetDatabase.LoadMainAssetAtPath(assetPath);
        }
    }
}