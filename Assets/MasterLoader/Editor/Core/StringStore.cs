namespace MasterLoader.Core
{
    public static class StringStore
    {
        public const string FACADE_PATH = "Assets/MasterLoader/Scripts/Facade/";
        public const string CONFIG_PATH = "Config";
        public const string PREFAB_PATH = "Assets/MasterLoader/Prefab";
        public const string GENERATED_PATH = "Assets/MasterLoader/Scripts/Generated/";
        public const string MASTER = "Master";
        private const string _API_URL = "https://script.google.com/macros/s/";
        private const string _DEPLOY_ID = "AKfycbxxODp1M7c3zFr-C4gLe8cYZWephd1wsRnVP6uOhU9nFK2LFqwjLIR4yA2oHn_sDVKG";
        private const string _EXEC = "/exec?";
        public const string URL = "url=";
        public const string GoogleSpreadsheetURL = "https://sheets.googleapis.com/v4/spreadsheets";
        /// <summary>
        /// doGet時の独自変数
        /// 読み込むシートの判断用
        /// </summary>
        public const string SHEET_NAME = "sheetName=";
        public const string FUNCTION_MASTER = "function=LoadMaster";
        public const string FUNCTION_LOAD_ALL = "function=LoadAll";
        public const string FUNCTION_GET_SHEETS = "function=GetSheets";
        public const string FUNCTION_CREATE_SHEET = "function=CreateSheet";
        public const string METHOD_SET_DATA = "SetData";
        public const string MASTER_FACADE_NAME = "MasterFacade";

        public static string GetUrl(string function, string[] arguments)
        {
            var argument = string.Empty;

            for (var i = 0; i < arguments.Length; i++)
            {
                argument += $"&{arguments[i]}";
            }

            return $"{_API_URL}{_DEPLOY_ID}{_EXEC}{function}{argument}";
        }
    }
}
