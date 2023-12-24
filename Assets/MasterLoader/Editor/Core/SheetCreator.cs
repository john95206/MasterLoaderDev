using UnityEditor;
using UnityEngine;
using MasterLoaderConfig;
using System.Linq;

namespace MasterLoader.Core
{
    public class SheetCreator
    {
        static SheetCreator() { }

        public static void CreateSpreadSheet(IConfig config, string masterName, string id)
        {
            var valueJson = CreateJson(config, masterName);

            var url = StringStore.GetUrl
            (
                StringStore.FUNCTION_CREATE_SHEET,
                new string[]
                {
                    $"id={id}",
                    $"&values={valueJson}"
                }
            );

            var json = WebRequest.SendWebRequest
            (
                url,
                "Creating MasterSheet..."
            );

            var result = JsonUtility.FromJson<Result>(json);
            OnSucceeded(result, config);
        }

        private static string CreateJson(IConfig config, string masterName)
        {
            var list = config.CreateConfig.CreatingMasterValueList.Where(m => !string.IsNullOrEmpty(m.VariableName));
            var comment = list.Select(m => m.Comment).ToArray();
            var parameter = list.Select(m => m.VariableName).ToArray();
            var type = list.Select(m => m.Type).ToArray();
            var value = new MasterDataRaw
            {
                Comment = comment,
                Parameter = parameter,
                Type = type,
                Name = masterName,
            };
            return JsonUtility.ToJson(value);
        }

        private static void OnSucceeded(Result result, IConfig config)
        {
            config.WindowConfig.SetSheetUrl(result.SheetUrl);
            config.MasterBody.SetMasters(result.Masters);
            Debug.Log($"MasterLoader Info: Creating Sheet has completed.\n<color=cyan>{result.SheetUrl}</color>");
            foreach (var m in config.MasterBody.Masters)
            {
                Debug.Log($"MasterLoader Info: sheet '{m}' has created.");
            }
            MasterLoader.SaveConfig();
        }
    }
}
