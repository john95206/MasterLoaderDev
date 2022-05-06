namespace MasterLoaderConfig
{
    public static class Localization
    {
        public const string TAB_GET_STARTED = "GET STARTED";
        public const string TAB_LOADER = "LOADER";

        public static string TutorialMessage(MasterLoaderLanguage language)
        {
            switch (language)
            {
                case MasterLoaderLanguage.English:
                default:
                    return
                        $"Here you can create MasterLoader-readable Spread Sheet\n" +
                        $"to assigned Google Drive folder.\n" +
                        $"Off course this step is skippable,\n" +
                        $"but better way to keep readable master.";
                case MasterLoaderLanguage.Japanese:
                    return
                        $"ここでは指定した Google ドライブフォルダに\n" +
                        $"MasterLoader で読み込み可能なフォーマットの\n" +
                        $"スプレッドシートを作ることができます。\n" +
                        $"この手順を省略して作成したスプレッドシートでも\n" +
                        $"MasterLoader は利用できますが\n" +
                        $"フォーマットを守るために一度は作成することをお勧めします。";
            }
        }
    }
}