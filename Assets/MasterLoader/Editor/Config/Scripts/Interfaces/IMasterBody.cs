namespace MasterLoaderConfig
{
    public interface IMasterBody
    {
        string[] Masters { get; }
        void SetMasters(string[] masters);
    }
}
