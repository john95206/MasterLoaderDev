namespace MasterLoaderConfig
{

    public interface IConfig
    {
        WindowConfig WindowConfig { get; }
        int SheetIndex { get; }
        void SetSheetIndex(int index);
        string CurrentMasterName { get; }
        void SetCurrentMasterName(string masterName);
        MasterLoaderLanguage CurrentLanguage { get; }
        void SetCurrentLanguage(MasterLoaderLanguage language);
        IMasterBody MasterBody { get; }
        ICreateConfig CreateConfig { get; }
        ILoadingConfig LoadingConfig { get; }
    }
}
