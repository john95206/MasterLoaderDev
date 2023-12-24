namespace MasterLoaderConfig
{
    public interface IResult
    {
        string SheetUrl { get; }
        string[] Masters { get; }
        string[] Alerts { get; }
        void SetMasters(string[] masters);
        void SetSheetUrl(string url);
        void SetAlerts(string[] alerts);
    }
}
