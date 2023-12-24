using UnityEditor;
namespace MasterLoaderConfig
{
    public interface IWindowConfig
    {
        string DriveUrl { get; }
        string SheetUrl { get; }
        bool IsFetched { get; }
        int TabIndex { get; }
        DefaultAsset MasterPathFolder { get; }
        IndentType IndentType { get; }
        LineType LineType { get; }

        void SetDriveUrl(string driveUrl);

        void SetSheetUrl(string sheetUrl);

        void SetIsFetched(bool isFetched);

        void SetTabIndex(int index);

        void SetMasterPathFolder(DefaultAsset masterPathFolder);
        void SetIndentType(IndentType indentType);
        void SetLineType(LineType lineType);
    }
}
