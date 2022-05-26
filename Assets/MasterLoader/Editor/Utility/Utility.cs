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

        public static bool OnValidateInputedValue(string text)
        {
            for(var i = 0; i < text.Length; i++)
            {
                var t = text[i];
                // 先頭文字が数字や記号だったら弾く
                if(i == 0)
                {
                    if (char.IsNumber(t))
                    {
                        return false;
                    }
                    if (char.IsSymbol(t))
                    {
                        if (!t.IsValidSymbol())
                        {
                            return false;
                        }
                    }
                }
                // 二文字目以降が数字やアルファベットでも _ でもなかったら弾く
                if (!char.IsLetterOrDigit(t) && !t.IsValidSymbol())
                {
                    return false;
                }
            }
            text.Replace(" ", string.Empty);
            text.Replace("　", string.Empty);
            return true;
        }

        public static bool IsValidSymbol(this char c)
        {
            return c != '_';
        }
    }
}